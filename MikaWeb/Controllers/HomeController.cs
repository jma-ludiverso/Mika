using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MikaWeb.Areas.Identity.Data;
using MikaWeb.Extensions.DB;
using MikaWeb.Models;

namespace MikaWeb.Controllers
{
    [Authorize(AuthenticationSchemes = "Identity.Application")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DBConfig _dbConfig;
        private readonly UserManager<MikaWebUser> _userManager;

        public HomeController(ILogger<HomeController> logger, IOptions<DBConfig> dbConf, UserManager<MikaWebUser> userManager)
        {
            _logger = logger;
            _dbConfig = dbConf.Value;
            _userManager = userManager;
        }

        private List<CitasEmpleado> Citas(DBExtension db, DateTime fec)
        {
            List<CitasEmpleado> ret = new List<CitasEmpleado>();
            try
            {
                SqlCommand sc = new SqlCommand();
                string sql = "select c.Hora, c.Minutos, c.IdCliente, cli.Nombre as Cliente, cli.Telefono, c.Cliente as NoRegistrado, " +
                    "us.Codigo, us.Nombre, us.Apellidos, c.Notas " +
                    "from Calendario c " +
                    "inner join Clientes cli on c.IdCliente = cli.IdCliente " +
                    "inner join AspNetUsers us on c.Codigo = us.Codigo " +
                    "where c.Fecha=@fec ";
                sc.Parameters.AddWithValue("@fec", fec);
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
                        if (!string.IsNullOrEmpty(citemp.CodigoEmpleado) && citemp.Citas.Count > 0)
                        {
                            ret.Add(citemp);
                        }
                        citemp = new CitasEmpleado();
                        citemp.CodigoEmpleado = fila["Codigo"].ToString();
                        citemp.Empleado = fila["Nombre"].ToString();
                    }
                    Calendario_Cita cc = new Calendario_Cita();
                    cc.Hora = Extensions.Utils.FormatoFecha(fila["Hora"].ToString()) + ":" + Extensions.Utils.FormatoFecha(fila["Minutos"].ToString());
                    string horaActual = Extensions.Utils.FormatoFecha(System.DateTime.Now.Hour.ToString()) + ":" + Extensions.Utils.FormatoFecha(System.DateTime.Now.Minute.ToString());
                    int comparacionHora = String.Compare(horaActual, cc.Hora, comparisonType: StringComparison.OrdinalIgnoreCase);
                    if (comparacionHora < 0)
                    {
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
                }
                if (citemp.Citas.Count > 0)
                {
                    ret.Add(citemp);
                }
            }
            catch
            {
                throw;
            }
            return ret;
        }

        public async Task<IActionResult> Index()
        {
            Inicio data = new Inicio();
            DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
            try
            {
                db.OpenConnection();
                var user = await _userManager.GetUserAsync(User);
                DateTime fecActual = System.DateTime.Now;
                data.Fecha = fecActual.ToShortDateString();
                data.NCaja = fecActual.Year.ToString() + Extensions.Utils.FormatoFecha(fecActual.Month.ToString()) + Extensions.Utils.FormatoFecha(fecActual.Day.ToString());
                switch (System.DateTime.Now.Hour)
                {
                    case int n when n >= 6 && n < 14:
                        data.Mensaje = "Buenos días";
                        break;
                    case int n when n >= 14 && n < 21:
                        data.Mensaje = "Buenas tardes";
                        break;
                    default:
                        data.Mensaje = "Buenas noches";
                        break;
                }
                data.Citas = this.Citas(db, DateTime.Parse(data.Fecha));
                string sql = "select f.NFicha, c.Nombre as Cliente, dbo.MK_EmpleadosFicha(f.NFicha, f.IdSalon) as Empleados, " +
                    "dbo.MK_ServiciosFicha(f.NFicha, f.IdSalon) as Descripciones, Total " +
                    "from Fichas f " +
                    "inner join Clientes c on f.IdCliente = c.IdCliente " +
                    "where f.Fecha=@fec and f.IdSalon=@sal and f.Cerrada='False' " +
                    "order by f.NFicha";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@fec", DateTime.Parse(data.Fecha));
                sc.Parameters.AddWithValue("@sal", user.Salon);
                DataSet ds = await db.GetDataSet(sc, "fichas");
                List<Ficha_Resumen> lst = new List<Ficha_Resumen>();
                for(int i=0; i <= ds.Tables["fichas"].Rows.Count-1; i++)
                {
                    DataRow fila = ds.Tables["fichas"].Rows[i];
                    Ficha_Resumen fr = new Ficha_Resumen();
                    fr.Cliente = fila["Cliente"].ToString();
                    fr.Empleado = fila["Empleados"].ToString().Trim();
                    if (fr.Empleado.EndsWith(";"))
                    {
                        fr.Empleado = fr.Empleado.Substring(0, fr.Empleado.Length - 1);
                    }
                    fr.NFicha = fila["NFicha"].ToString();
                    fr.Servicios = fila["Descripciones"].ToString().Trim();
                    if (fr.Servicios.EndsWith(";"))
                    {
                        fr.Servicios = fr.Servicios.Substring(0, fr.Servicios.Length - 1);
                    }
                    lst.Add(fr);
                }
                data.Fichas = lst;
                sql = "select ISNULL(sum(Total),0) as Total, ISNULL(count(1),0) as Fichas, " +
                    "ISNULL((select Cerrada from Cajas Where NCaja=@ncaja and IdSalon=@sal),'False') as EstadoCaja " +
                    "from Fichas where Fecha=@fec and IdSalon=@sal";
                sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@ncaja", data.NCaja);
                sc.Parameters.AddWithValue("@fec", DateTime.Parse(data.Fecha));
                sc.Parameters.AddWithValue("@sal", user.Salon);
                DataSet dsCaja = await db.GetDataSet(sc, "caja");
                data.Total = Math.Round(double.Parse(dsCaja.Tables["caja"].Rows[0]["Total"].ToString()),2);
                data.Clientes = int.Parse(dsCaja.Tables["caja"].Rows[0]["Fichas"].ToString());
                data.EstadoCaja = bool.Parse(dsCaja.Tables["caja"].Rows[0]["EstadoCaja"].ToString());
                if (lst.Count >= 23)
                {
                    data.ProgresoLista = 100;
                }
                else
                {
                    double progreso = (data.Clientes * 100) / 23;
                    data.ProgresoLista = (int)Math.Round(progreso, 0);
                }
                switch (data.Clientes)
                {
                    case int n when n <= 6:
                        data.CssCaja = "bg-danger";
                        break;
                    case int n when n > 6 && n <= 17:
                        data.CssCaja = "bg-warning";
                        break;
                    case int n when n > 17:
                        data.CssCaja = "bg-success";
                        break;
                }
                db.Close();
            }
            catch (Exception ex)
            {
                try
                {
                    db.Close();
                }
                catch { }
                ViewBag.StatusMessage = "Error: " + ex.Message;
            }
            return View(data);
        }

        [AllowAnonymous]
        public IActionResult AvisoLegal()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
