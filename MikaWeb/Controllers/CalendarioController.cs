using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.CompilerServices;
using MikaWeb.Areas.Identity.Data;
using MikaWeb.Extensions;
using MikaWeb.Extensions.DB;
using MikaWeb.Models;
using MikaWeb.Models.ViewModels;

namespace MikaWeb.Controllers
{
    [Authorize]
    public class CalendarioController : Controller
    {
        private readonly ILogger<CalendarioController> _logger;
        private readonly DBConfig _dbConfig;
        private readonly UserManager<MikaWebUser> _userManager;

        public CalendarioController(ILogger<CalendarioController> logger, IOptions<DBConfig> dbConf, UserManager<MikaWebUser> userManager)
        {
            _logger = logger;
            _dbConfig = dbConf.Value;
            _userManager = userManager;
        }

        public async Task<IActionResult> CitasAsync(ViewModelCitas data, string fecha, string emp, string borrar)
        {
            DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
            if (string.IsNullOrEmpty(data.NuevaCitaEmpleado))
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (string.IsNullOrEmpty(emp))
                    {
                        data.NuevaCitaEmpleado = user.Codigo;
                    }
                    else
                    {
                        data.NuevaCitaEmpleado = emp;
                    }
                    data.FiltroEmpleado = emp;
                    data.Salon = user.Salon;
                    List<Empleado> lst = await this.getEmpleados(db, user.Salon, emp);
                    data.Empleados = new SelectList(lst, "Codigo", "Nombre");
                    Calendario cal = new Calendario();
                    cal.Fecha = DateTime.Parse(fecha);
                    cal.DiaSemana = Extensions.Utils.DiaSemana((int)cal.Fecha.DayOfWeek);
                    cal.Citas = this.Citas(cal.Fecha, emp);
                    cal.Horas = this.getHoras();
                    data.Dia = cal;
                    List<Cliente> lstCli = await this.getClientes(db, 10, "");
                    data.Clientes = new SelectList(lstCli, "IdCliente", "Nombre");
                }
                catch (Exception ex)
                {
                    ViewBag.StatusMessage = "Error: " + ex.Message;
                }
            }
            else
            {
                try
                {
                    if (string.IsNullOrEmpty(borrar))
                    {
                        if (int.Parse(data.NuevaCitaHora)>23 || int.Parse(data.NuevaCitaMinutos)>59)
                        {
                            throw new Exception("El valor de hora / minutos introducido no es válido");
                        }
                        else
                        {
                            if(data.NuevaCitaCliente==0 && string.IsNullOrEmpty(data.NuevaCitaNoregistrado))
                            {
                                throw new Exception("Debe indicar el nombre del cliente que reserva la cita");
                            }
                        }
                        string sql = "insert into Calendario values(@fec,@hor,@min,@idcliente,@codigo,@cliente,@notas)";
                        SqlCommand sc = new SqlCommand(sql);
                        sc.Parameters.AddWithValue("@fec", data.Dia.Fecha);
                        sc.Parameters.AddWithValue("@hor", data.NuevaCitaHora);
                        sc.Parameters.AddWithValue("@min", data.NuevaCitaMinutos);
                        sc.Parameters.AddWithValue("@idcliente", data.NuevaCitaCliente);
                        sc.Parameters.AddWithValue("@codigo", data.NuevaCitaEmpleado);
                        sc.Parameters.AddWithValue("@cliente", data.NuevaCitaNoregistrado);
                        sc.Parameters.AddWithValue("@notas", data.NuevaCitaNotas);
                        db.Command(sc);
                        db.Close();
                        ViewBag.StatusMessage = "Nueva cita guardada";
                        data.NuevaCitaCliente = 0;
                        data.NuevaCitaHora = "";
                        data.NuevaCitaMinutos = "";
                    }
                    else
                    {
                        string[] vals = data.DatosBorrar.Split(';');
                        string[] hm = vals[1].Split(':');
                        if (hm[0].StartsWith("0"))
                        {
                            hm[0] = hm[0].Substring(1);
                        }
                        if (hm[1].StartsWith("0"))
                        {
                            hm[1] = hm[1].Substring(1);
                        }
                        string sql = "Delete from Calendario where Fecha=@fec and Hora=@hor and Minutos=@min and IdCliente=@idcliente and Codigo=@codigo";
                        SqlCommand sc = new SqlCommand(sql);
                        sc.Parameters.AddWithValue("@fec", data.Dia.Fecha);
                        sc.Parameters.AddWithValue("@hor", hm[0]);
                        sc.Parameters.AddWithValue("@min", hm[1]);
                        sc.Parameters.AddWithValue("@idcliente", vals[0]);
                        sc.Parameters.AddWithValue("@codigo", vals[2]);
                        db.Command(sc);
                        db.Close();
                        ViewBag.StatusMessage = "Cita borrada";
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        db.Close();
                    }
                    catch { };
                    ViewBag.StatusMessage = "Error: " + ex.Message;
                }
                List<Empleado> lst = await this.getEmpleados(db, data.Salon, data.FiltroEmpleado);
                data.Empleados = new SelectList(lst, "Codigo", "Nombre");
                Calendario cal = data.Dia;
                cal.Citas = this.Citas(cal.Fecha, data.FiltroEmpleado);
                cal.Horas = this.getHoras();
                data.Dia = cal;
                List<Cliente> lstCli = await this.getClientes(db, 10, "");
                data.Clientes = new SelectList(lstCli, "IdCliente", "Nombre");
            }
            return View(data);
        }

        private List<CitasEmpleado> Citas(DateTime fec, string codEmpleado = "")
        {
            List<CitasEmpleado> ret = new List<CitasEmpleado>();
            try
            {
                DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
                SqlCommand sc = new SqlCommand();
                string sql = "select c.Hora, c.Minutos, c.IdCliente, cli.Nombre as Cliente, cli.Telefono, c.Cliente as NoRegistrado, " +
                    "us.Codigo, us.Nombre, us.Apellidos, c.Notas " +
                    "from Calendario c " +
                    "inner join Clientes cli on c.IdCliente = cli.IdCliente " +
                    "inner join AspNetUsers us on c.Codigo = us.Codigo " +
                    "where c.Fecha=@fec ";
                sc.Parameters.AddWithValue("@fec", fec);
                if (!string.IsNullOrEmpty(codEmpleado))
                {
                    sql += "and us.Codigo=@codigo ";
                    sc.Parameters.AddWithValue("@codigo", codEmpleado);
                }
                sql += "order by us.Codigo, c.Hora, c.Minutos";
                sc.CommandText = sql;
                DataSet ds = db.GetDataSet(sc, "citas").Result;
                CitasEmpleado citemp = new CitasEmpleado();
                citemp.CodigoEmpleado = "";
                for (int i = 0; i <= ds.Tables["citas"].Rows.Count - 1; i++)
                {
                    DataRow fila = ds.Tables["citas"].Rows[i];
                    if (!citemp.CodigoEmpleado.Equals(fila["Codigo"].ToString()))
                    {
                        if (!string.IsNullOrEmpty(citemp.CodigoEmpleado))
                        {
                            ret.Add(citemp);
                        }
                        citemp = new CitasEmpleado();
                        citemp.CodigoEmpleado = fila["Codigo"].ToString();
                        citemp.Empleado = fila["Nombre"].ToString();
                    }
                    Calendario_Cita cc = new Calendario_Cita();
                    cc.Hora = Extensions.Utils.FormatoFecha(fila["Hora"].ToString()) + ":" + Extensions.Utils.FormatoFecha(fila["Minutos"].ToString());
                    Cliente c = new Cliente();
                    c.IdCliente = int.Parse(fila["IdCliente"].ToString());
                    if (c.IdCliente == 0)
                    {
                        c.Nombre = fila["NoRegistrado"].ToString();
                        c.Telefono = "";
                    }
                    else
                    {
                        c.Nombre = fila["Cliente"].ToString();
                        c.Telefono = fila["Telefono"].ToString();
                    }
                    cc.Cliente = c;
                    cc.Notas = fila["Notas"].ToString();
                    citemp.Citas.Add(cc);
                }
                if (citemp.Citas.Count > 0)
                {
                    ret.Add(citemp);
                }
            }
            catch { }
            return ret;
        }

        private async Task<List<Cliente>> getClientes(DBExtension db, int top, string search)
        {
            List<Cliente> ret = new List<Cliente>();
            try
            {
                SqlCommand sc = new SqlCommand();
                string sql = "Select ";
                if (top != -1)
                {
                    sql = "Select top " + top.ToString();
                }
                sql += "IdCliente, Nombre From Clientes Where IdCliente<>0 ";
                if (!string.IsNullOrEmpty(search))
                {
                    sql += "and Nombre like @nombre";
                    sc.Parameters.AddWithValue("@nombre", "%" + search + "%");
                }
                sql += " order by nombre";
                sc.CommandText = sql;
                DataSet ds = await db.GetDataSet(sc, "clientes");
                for(int i=0; i<=ds.Tables["clientes"].Rows.Count-1; i++)
                {
                    DataRow fila = ds.Tables["clientes"].Rows[i];
                    Cliente c = new Cliente();
                    c.IdCliente = int.Parse(fila["IdCliente"].ToString());
                    c.Nombre = fila["Nombre"].ToString();
                    ret.Add(c);
                }
            }catch (Exception ex)
            {
                ViewBag.StatusMessage = "Error: " + ex.Message;
            }
            return ret;
        }

        private async Task<List<Empleado>> getEmpleados(DBExtension db, int salon, string codigo)
        {
            List<Empleado> ret = new List<Empleado>();
            try
            {
                string sql = "select Codigo, Nombre + ' ' + Apellidos as Empleado, Activo from vEmpleados " +
                    "Where (Activo='True' or Codigo=@cod) and Salon=@salon order by Codigo";
                SqlCommand sc = new SqlCommand(sql);
                if (string.IsNullOrEmpty(codigo))
                {
                    sc.Parameters.AddWithValue("@cod", "");
                }
                else
                {
                    sc.Parameters.AddWithValue("@cod", codigo);
                }
                sc.Parameters.AddWithValue("@salon", salon);
                DataSet ds = await db.GetDataSet(sc, "empleados");
                for (int i = 0; i <= ds.Tables["empleados"].Rows.Count - 1; i++)
                {
                    DataRow fila = ds.Tables["empleados"].Rows[i];
                    Empleado e = new Empleado();
                    e.Activo = bool.Parse(fila["Activo"].ToString());
                    e.Codigo = fila["Codigo"].ToString();
                    e.Nombre = fila["Empleado"].ToString();
                    ret.Add(e);
                }
            }
            catch (Exception ex)
            {
                ViewBag.StatusMessage = "Error: " + ex.Message;
            }
            return ret;
        }

        private List<CalendarioHoras> getHoras()
        {
            List<CalendarioHoras> horas = new List<CalendarioHoras>();
            for (int i = 9; i <= 19; i++)
            {
                CalendarioHoras h1 = new CalendarioHoras();
                h1.Inicio = i * 100;
                h1.Fin = h1.Inicio + 30;
                h1.Hora = Extensions.Utils.FormatoFecha(i.ToString()) + ":00";
                horas.Add(h1);
                CalendarioHoras h2 = new CalendarioHoras();
                h2.Inicio = h1.Fin;
                h2.Fin = h2.Inicio + 30;
                h2.Hora = Extensions.Utils.FormatoCadena(h2.Inicio.ToString(), 4);
                h2.Hora = h2.Hora.Substring(0, 2) + ":" + h2.Hora.Substring(2);
                horas.Add(h2);
            }
            return horas;
        }

        [HttpGet]
        public async Task<JsonResult> getClientes(string search)
        {
            DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
            List<Cliente> lstCli = await this.getClientes(db, -1, search);
            return Json(lstCli);
        }

        public IActionResult Index(IEnumerable<Calendario> data, string mesref)
        {
            if (data.Count() == 0)
            {
                List<Calendario> datosCal = data.ToList<Calendario>();
                DateTime fecref;
                DateTime diaActual = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, System.DateTime.Now.Day);
                if (string.IsNullOrEmpty(mesref))
                {
                    fecref = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, 1);
                }
                else
                {
                    fecref = new DateTime(int.Parse(mesref.Substring(0,4)), int.Parse(mesref.Substring(4, 2)), 1);
                }
                int nmes = fecref.Month;
                int diasem = (int)fecref.DayOfWeek;
                for(int i=1; i<=diasem-1; i++)
                {
                    DateTime fecPrev = fecref.AddDays(-(diasem - i));
                    Calendario c = new Calendario();
                    c.Fecha = fecPrev;
                    c.MesCargado = fecref;
                    if (fecPrev>= diaActual)
                    {
                        c.AdmiteCitas = true;
                    }
                    else
                    {
                        c.AdmiteCitas = false;
                    }
                    c.DiaSemana = MikaWeb.Extensions.Utils.DiaSemana((int)fecPrev.DayOfWeek);
                    c.Citas = this.Citas(fecref);
                    datosCal.Add(c);
                }
                while (fecref.Month == nmes)
                {
                    Calendario c = new Calendario();
                    c.Fecha = fecref;
                    if (fecref >= diaActual)
                    {
                        c.AdmiteCitas = true;
                    }
                    else
                    {
                        c.AdmiteCitas = false;
                    }
                    c.DiaSemana = MikaWeb.Extensions.Utils.DiaSemana((int)fecref.DayOfWeek);
                    c.Citas = this.Citas(fecref);
                    datosCal.Add(c);
                    fecref = fecref.AddDays(1);
                }
                diasem = (int)fecref.DayOfWeek;
                if (diasem != 1)
                {
                    for(int i=diasem; i<=7; i++)
                    {
                        Calendario c = new Calendario();
                        c.Fecha = fecref;
                        if (fecref >= diaActual)
                        {
                            c.AdmiteCitas = true;
                        }
                        else
                        {
                            c.AdmiteCitas = false;
                        }
                        c.DiaSemana = MikaWeb.Extensions.Utils.DiaSemana((int)fecref.DayOfWeek);
                        c.Citas = this.Citas(fecref);
                        datosCal.Add(c);
                        fecref = fecref.AddDays(1);
                        if (c.DiaSemana.Equals("Domingo"))
                        {
                            break;
                        }
                    }
                }
                data = datosCal.ToArray<Calendario>();
            }
            return View(data);
        }
    }
}
