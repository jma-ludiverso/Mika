using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MikaWeb.Areas.Identity.Data;
using MikaWeb.Extensions;
using MikaWeb.Extensions.DB;
using MikaWeb.Models;
using MikaWeb.Models.AuxiliaryModels;
using MikaWeb.Models.ViewModels;
using Newtonsoft.Json;

namespace MikaWeb.Controllers
{
    [Authorize(AuthenticationSchemes = "Identity.Application")]
    public class FichasController : Controller
    {

        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FichasController> _logger;
        private readonly DBConfig _dbConfig;
        private readonly UserManager<MikaWebUser> _userManager;

        public FichasController(IWebHostEnvironment env, ILogger<FichasController> logger, IOptions<DBConfig> dbConf, UserManager<MikaWebUser> userManager)
        {
            _env = env;
            _logger = logger;
            _dbConfig = dbConf.Value;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<PartialViewResult> AddOrEdit([FromBody] ViewModelFicha data, int nlinea)
        {
            if (nlinea == -1)
            {
                data.LineaTrabajo = new Ficha_Linea();
            }
            else
            {
                try
                {
                    //cargar datos línea desde BD
                    DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
                    string sql = "select fl.Linea, fl.Codigo, us.Nombre + ' ' + us.Apellidos as Empleado, fl.IdServicio, " +
                        "s.Codigo as CodServicio, fl.Descripcion, s.Tipo, fl.Base, fl.DescuentoPorc, " +
                        "fl.DescuentoCant, fl.IvaPorc, fl.IvaCant, fl.Total " +
                        "from Fichas_Lineas fl " +
                        "inner join Servicios s on fl.IdServicio = s.IdServicio " +
                        "inner join AspNetUsers us on fl.Codigo = us.Codigo and fl.IdSalon = us.Salon " +
                        "Where fl.IdSalon=@sal and fl.NFicha=@nficha and fl.Linea=@linea";
                    SqlCommand sc = new SqlCommand(sql);
                    sc.Parameters.AddWithValue("@sal", data.Datos.IdSalon);
                    sc.Parameters.AddWithValue("@nficha", data.Datos.NFicha);
                    sc.Parameters.AddWithValue("@linea", nlinea);
                    DataSet ds = await db.GetDataSet(sc, "linea");
                    DataRow fila = ds.Tables["linea"].Rows[0];
                    data.LineaTrabajo = new Ficha_Linea();
                    data.LineaTrabajo.Linea = nlinea;
                    data.LineaTrabajo.Codigo = fila["Codigo"].ToString();
                    data.LineaTrabajo.Empleado = fila["Empleado"].ToString();
                    data.LineaTrabajo.IdServicio = int.Parse(fila["IdServicio"].ToString());
                    data.LineaTrabajo.CodigoServicio = fila["CodServicio"].ToString();
                    data.LineaTrabajo.Descripcion = fila["Descripcion"].ToString();
                    data.LineaTrabajo.Tipo = fila["Tipo"].ToString();
                    data.LineaTrabajo.Base = Math.Round(double.Parse(fila["Base"].ToString()),3);
                    data.LineaTrabajo.DescuentoPorc = double.Parse(fila["DescuentoPorc"].ToString());
                    data.LineaTrabajo.DescuentoCant = double.Parse(fila["DescuentoCant"].ToString());
                    data.LineaTrabajo.IvaPorc = double.Parse(fila["IvaPorc"].ToString());
                    data.LineaTrabajo.IvaCant = double.Parse(fila["IvaCant"].ToString());
                    data.LineaTrabajo.Total = Math.Round(double.Parse(fila["Total"].ToString()),2);
                }
                catch (Exception ex)
                {
                    ViewBag.StatusMessage = "Error: " + ex.Message;
                }
            }
            return PartialView("AddOrEdit", data);
        }

        private void BorraLinea(DBExtension db, string numficha, int salon, int idLinea)
        {
            try
            {
                string sql = "Delete from Fichas_Lineas Where IdSalon=@sal and NFicha=@nficha and Linea=@linea";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@sal", salon);
                sc.Parameters.AddWithValue("@nficha", numficha);
                sc.Parameters.AddWithValue("@linea", idLinea);
                db.Command(sc);
            }
            catch 
            {
                throw;
            }
        }

        private async Task<Ficha> CargaFicha(DBExtension db, string numficha, string salon)
        {
            Ficha ret = new Ficha();
            try
            {
                string sql = "select Fecha, IdCliente, FormaPago, Base, DescuentoPorc, DescuentoImp, Descuentos, Iva, Total, Cerrada " +
                    "from Fichas Where IdSalon=@sal and NFicha=@nficha";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@sal", salon);
                sc.Parameters.AddWithValue("@nficha", numficha);
                DataSet ds = await db.GetDataSet(sc, "ficha");
                DataRow filaF = ds.Tables["ficha"].Rows[0];
                ret.Base = double.Parse(filaF["Base"].ToString());
                ret.DescuentoImp = double.Parse(filaF["DescuentoImp"].ToString());
                ret.DescuentoPorc = double.Parse(filaF["DescuentoPorc"].ToString());
                ret.Fecha = DateTime.Parse(filaF["Fecha"].ToString());
                ret.FormaPago = filaF["FormaPago"].ToString();
                ret.IdCliente = int.Parse(filaF["IdCliente"].ToString());
                ret.IdSalon = int.Parse(salon);
                ret.Iva = double.Parse(filaF["Iva"].ToString());
                ret.NFicha = numficha;
                ret.Total = double.Parse(filaF["Total"].ToString());
                ret.TotalDescuentos = double.Parse(filaF["Descuentos"].ToString());
                ret.Cerrada = bool.Parse(filaF["Cerrada"].ToString());
                ret.Lineas = await this.CargaFicha_Lineas(db, numficha, salon);
            }
            catch { throw; }
            return ret;
        }

        private async Task<List<Ficha_Linea>> CargaFicha_Lineas(DBExtension db, string numficha, string salon)
        {
            List<Ficha_Linea> ret = new List<Ficha_Linea>();
            try
            {
                string sql = "select fl.Linea, fl.Codigo, fl.IdServicio, s.Codigo as CodigoServicio, fl.Descripcion, " +
                    "s.Tipo, fl.Base, fl.DescuentoPorc, fl.DescuentoCant, fl.IvaPorc, fl.IvaCant, fl.Total " +
                    "from Fichas_Lineas fl " +
                    "inner join Servicios s on fl.IdServicio = s.IdServicio " + 
                    "Where fl.IdSalon=@sal and fl.NFicha=@nficha";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@sal", salon);
                sc.Parameters.AddWithValue("@nficha", numficha);
                DataSet ds = await db.GetDataSet(sc, "lineas");
                for(int i=0; i<=ds.Tables["lineas"].Rows.Count-1; i++)
                {
                    DataRow fila = ds.Tables["lineas"].Rows[i];
                    Ficha_Linea linea = new Ficha_Linea();
                    linea.Linea = int.Parse(fila["linea"].ToString());
                    linea.Codigo = fila["Codigo"].ToString();
                    linea.IdServicio = int.Parse(fila["IdServicio"].ToString());
                    linea.CodigoServicio = fila["CodigoServicio"].ToString();
                    linea.Descripcion = fila["Descripcion"].ToString();
                    linea.Tipo = fila["Tipo"].ToString();
                    linea.Base = double.Parse(fila["Base"].ToString());
                    linea.DescuentoPorc = double.Parse(fila["DescuentoPorc"].ToString());
                    linea.DescuentoCant = double.Parse(fila["DescuentoCant"].ToString());
                    linea.IvaPorc = double.Parse(fila["IvaPorc"].ToString());
                    linea.IvaCant = double.Parse(fila["IvaCant"].ToString());
                    linea.Total = double.Parse(fila["Total"].ToString());
                    ret.Add(linea);
                }
            }
            catch { throw; }
            return ret;
        }

        public async Task<IActionResult> Ficha(ViewModelFicha data, int cli, string numficha, string acc)
        {
            DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
            try
            {
                db.OpenConnection();
                var user = await _userManager.GetUserAsync(User);
                if (data.Datos == null)
                {
                    Ficha f = new Ficha();
                    f.IdCliente = cli;
                    if (string.IsNullOrEmpty(numficha))
                    {
                        f.Fecha = System.DateTime.Now;
                        f.NFicha = "(sin guardar)";
                        f.IdSalon = user.Salon; 
                    }
                    else
                    {
                        //cargar ficha desde BD
                        f = await this.CargaFicha(db, numficha, user.Salon.ToString());
                    }
                    f.FormaPago = "Tarjeta";
                    data.Datos = f;
                    if (data.Datos.Lineas == null)
                    {
                        data.Datos.Lineas = new List<Ficha_Linea>();
                    }
                }
                else
                {
                    data.Datos.IdSalon = user.Salon;
                    if (data.Datos.Lineas == null)
                    {
                        data.Datos.Lineas = await this.CargaFicha_Lineas(db, data.Datos.NFicha, data.Datos.IdSalon.ToString());
                    }
                }
                if (!string.IsNullOrEmpty(acc))
                {
                    if (acc.Equals("L"))
                    {
                        //guardar línea
                        if (data.Datos.NFicha.Equals("(sin guardar)"))
                        {
                            data.Datos = await this.guarda_Ficha(db, data.Datos);
                            ModelState.Clear();
                        }
                        int idlinea = data.LineaTrabajo.Linea;
                        Ficha_Linea l = await this.guardaLinea(db, data.LineaTrabajo, data.Datos.NFicha, data.Datos.IdSalon, data.Datos.DescuentoPorc);
                        if (idlinea == 0)
                        {
                            if (data.Datos.Lineas == null)
                            {
                                data.Datos.Lineas = new List<Ficha_Linea>();
                            }
                            data.Datos.Lineas.Add(l);
                            return RedirectToAction("Ficha", new { numficha = data.Datos.NFicha });
                        }
                        else
                        {
                            data.Datos.Lineas = await this.CargaFicha_Lineas(db, data.Datos.NFicha, data.Datos.IdSalon.ToString());
                        }
                    }
                    else
                    {
                        if (acc.Equals("B"))
                        {
                            //borrar línea
                            this.BorraLinea(db, data.Datos.NFicha, data.Datos.IdSalon, data.LineaTrabajo.Linea);
                            acc = "D";
                        }
                        else
                        {
                            //guardar ficha / cerrar ficha
                            if (acc.Equals("S"))
                            {
                                data.Datos.Cerrada = true;
                            }
                            else
                            {
                                if (acc.Equals("ROP"))
                                {
                                    data.Datos.Cerrada = false;
                                }
                            }
                            if (!data.Datos.NFicha.Equals("(sin guardar)"))
                            {
                                string nfichaCompara = data.Datos.Fecha.Year.ToString() + Extensions.Utils.FormatoFecha(data.Datos.Fecha.Month.ToString());
                                if (!data.Datos.NFicha.StartsWith(nfichaCompara))
                                {
                                    throw new Exception("La fecha indicada no coincide con el mes/año de numeración de la ficha");
                                }
                            }
                            else
                            {
                                ModelState.Clear();
                            }
                            data.Datos = await this.guarda_Ficha(db, data.Datos);
                            if (data.Datos.Cerrada)
                            {
                                return RedirectToAction("Index", "Home");
                            }
                        }
                        data.Datos.Lineas = await this.CargaFicha_Lineas(db, data.Datos.NFicha, data.Datos.IdSalon.ToString());
                    }
                }
                List<Cliente> lstCli = await this.getClientes(db, data.Datos.IdSalon.ToString(), "", data.Datos.IdCliente);
                data.Clientes = new SelectList(lstCli, "IdCliente", "Nombre");
                ViewModelCliente hist = new ViewModelCliente();
                if (data.Datos.IdCliente != 0 && !data.Datos.NFicha.Equals("(sin guardar)"))
                {
                    ClientesExtension cliext = new ClientesExtension(db);
                    hist.Historial = await cliext.getHistorial(data.Datos.IdCliente);
                    hist.MuestraCabecera = true;
                    hist.Cliente = lstCli[0];
                    hist.FichaRef = data.Datos.NFicha;
                }
                else
                {
                    hist.Cliente = new Cliente();
                }
                data.Historial = hist;
                if (data.Datos.Lineas.Count > 0 || data.TotalServicios > 0 || data.TotalProductos > 0 || data.Datos.Base > 0)
                {
                    //calcular datos cabecera
                    data = await this.Ficha_Cabecera(db, data, acc);
                    data.Datos = await this.guarda_Ficha(db, data.Datos);
                }
                db.Close();
            }
            catch (Exception ex)
            {
                try
                {
                    try
                    {
                        List<Cliente> lstCli = await this.getClientes(db, data.Datos.IdSalon.ToString(), "", data.Datos.IdCliente);
                        data.Clientes = new SelectList(lstCli, "IdCliente", "Nombre");
                    }
                    catch 
                    {
                        data.Clientes = new SelectList(new List<Cliente>(), "IdCliente", "Nombre");
                    }
                    ViewModelCliente hist = new ViewModelCliente();
                    hist.Cliente = new Cliente();
                    data.Historial = hist;
                    db.Close();
                }
                catch { }
                ViewBag.StatusMessage = "Error: " + ex.Message;
            }
            return View(data);
        }

        private async Task<ViewModelFicha> Ficha_Cabecera(DBExtension db, ViewModelFicha data, string acc)
        {
            try
            {
                data.Datos.Base = 0;
                data.Datos.TotalDescuentos = 0;
                data.Datos.Iva = 0;
                data.Datos.Total = 0;
                data.TotalProductos = 0;
                data.TotalServicios = 0;
                bool recalculo = false;
                if (!string.IsNullOrEmpty(acc))
                {
                    if (acc.Equals("D"))
                    {
                        //recalcular descuentos por líneas y comisiones cuando ha cambiado el descuento en la cabecera
                        for (int i = 0; i <= data.Datos.Lineas.Count - 1; i++)
                        {
                            Ficha_Linea l = data.Datos.Lineas[i];
                            if (l.Tipo.Equals("Servicio"))
                            {
                                l = await this.guardaLinea(db, l, data.Datos.NFicha, data.Datos.IdSalon, data.Datos.DescuentoPorc);
                            }
                            data.Datos.Lineas[i] = l;
                        }
                        recalculo = true;
                    }
                }
                for(int i=0; i <= data.Datos.Lineas.Count - 1; i++)
                {
                    Ficha_Linea l = data.Datos.Lineas[i];
                    data.Datos.Base += l.Base;
                    data.Datos.TotalDescuentos += l.DescuentoCant;
                    data.Datos.Iva += l.IvaCant;
                    data.Datos.Total += l.Total;
                    if (l.Tipo.Equals("Servicio"))
                    {
                        data.TotalServicios += l.Total;
                    }
                    else
                    {
                        data.TotalProductos += l.Total;
                    }
                }
                data.Datos.Base = Math.Round(data.Datos.Base, 2);
                data.Datos.TotalDescuentos = Math.Round(data.Datos.TotalDescuentos, 2);
                data.Datos.Iva = Math.Round(data.Datos.Iva, 2);
                data.Datos.Total = Math.Round(data.Datos.Total, 2);
                data.TotalProductos = Math.Round(data.TotalProductos,2);
                data.TotalServicios = Math.Round(data.TotalServicios,2);
                if (recalculo)
                {
                    data.Datos = await this.guarda_Ficha(db, data.Datos);
                }
                ModelState.Clear();
            }
            catch { throw; }
            return data;
        }

        private async Task<List<Cliente>> getClientes(DBExtension db, string salon, string search, int idcli = 0)
        {
            List<Cliente> ret = new List<Cliente>();
            try
            {
                SqlCommand sc = new SqlCommand();
                string sql = "Select ";
                sql += "IdCliente, Nombre From Clientes Where IdSalon=@sal and ";
                sc.Parameters.AddWithValue("@sal", salon);
                if (!string.IsNullOrEmpty(search))
                {
                    sql += "Nombre like @nombre";
                    sc.Parameters.AddWithValue("@nombre", "%" + search + "%");
                }
                else
                {
                    sql += "IdCliente=@cli";
                    sc.Parameters.AddWithValue("@cli", idcli);
                }
                sql += " order by nombre";
                sc.CommandText = sql;
                DataSet ds = await db.GetDataSet(sc, "clientes");
                for (int i = 0; i <= ds.Tables["clientes"].Rows.Count - 1; i++)
                {
                    DataRow fila = ds.Tables["clientes"].Rows[i];
                    Cliente c = new Cliente();
                    c.IdCliente = int.Parse(fila["IdCliente"].ToString());
                    c.Nombre = fila["Nombre"].ToString();
                    ret.Add(c);
                }
            }
            catch (Exception ex)
            {
                ViewBag.StatusMessage = "Error: " + ex.Message;
            }
            return ret;
        }

        [HttpGet]
        public async Task<JsonResult> getClientes(string search)
        {
            DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
            var user = await _userManager.GetUserAsync(User);
            List<Cliente> lstCli = await this.getClientes(db, user.Salon.ToString(), search);
            return Json(lstCli);
        }

        public async Task<Servicio_Comision> getComision(DBExtension db, Ficha_Linea flinea, SqlTransaction t = null)
        {
            Servicio_Comision ret = new Servicio_Comision();
            try
            {
                string sql = "Select ComisionP1,ComisionP2,ComisionP3,ComisionP4 " +
                    "from EmpleadosServicios where Codigo=@codigo and IdServicio=@idservicio";
                SqlCommand sc = new SqlCommand(sql);
                if (t != null)
                {
                    sc.Transaction = t;
                }
                sc.Parameters.AddWithValue("@codigo", flinea.Codigo);
                sc.Parameters.AddWithValue("@idservicio", flinea.IdServicio);
                DataSet ds = await db.GetDataSet(sc, "comision");
                if (ds.Tables["comision"].Rows.Count > 0)
                {
                    DataRow fila = ds.Tables["comision"].Rows[0];
                    double porc = double.Parse(fila["ComisionP1"].ToString());
                    ret.ComisionP1 = Math.Round((flinea.Base - flinea.DescuentoCant) * (porc / 100), 2);
                    porc = double.Parse(fila["ComisionP2"].ToString());
                    ret.ComisionP2 = Math.Round((flinea.Base - flinea.DescuentoCant) * (porc / 100), 2);
                    porc = double.Parse(fila["ComisionP3"].ToString());
                    ret.ComisionP3 = Math.Round((flinea.Base - flinea.DescuentoCant) * (porc / 100), 2);
                    porc = double.Parse(fila["ComisionP4"].ToString());
                    ret.ComisionP4 = Math.Round((flinea.Base - flinea.DescuentoCant) * (porc / 100), 2);
                }
            }
            catch { throw; }
            return ret;
        }

        [HttpGet]
        public async Task<string> getEmpleados(string search, string salon)
        {
            string ret = "";
            try
            {
                DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
                string sql = "select Nombre, Apellidos from AspNetUsers where Activo='True' and Salon=@sal and Codigo=@cod";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@sal", salon);
                sc.Parameters.AddWithValue("@cod", search);
                DataSet ds = await db.GetDataSet(sc, "emp");
                if (ds.Tables["emp"].Rows.Count > 0)
                {
                    ret = ds.Tables["emp"].Rows[0]["Nombre"].ToString() + " " + ds.Tables["emp"].Rows[0]["Apellidos"].ToString();
                }
            }
            catch { }
            return ret;
        }

        public async Task<ActionResult> GetLineas([FromBody]DtParameters dtParameters)
        {
            List<Ficha_Linea> lst = new List<Ficha_Linea>();
            if (!string.IsNullOrEmpty(dtParameters.AdditionalValues.ToList<string>()[0]))
            {
                var user = await _userManager.GetUserAsync(User);
                string nficha = dtParameters.AdditionalValues.ToList<string>()[0];
                DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
                string sql = "select f.Linea, f.Codigo + '-' + us.Nombre + ' ' + us.Apellidos as Empleado, " +
                    "serv.Tipo, serv.Codigo + '-' + f.Descripcion as Descripcion, f.Base, f.DescuentoPorc, f.DescuentoCant, f.IvaPorc, f.IvaCant, f.Total, " +
                    "f.ComisionP1, f.ComisionP2, f.ComisionP3, f.ComisionP4 " +
                    "from Fichas_Lineas f " +
                    "inner join Fichas fich on f.NFicha = fich.NFicha and f.IdSalon=fich.IdSalon " +
                    "inner join AspNetUsers us on f.Codigo = us.Codigo and fich.IdSalon = us.Salon " +
                    "inner join Salones s on us.Salon = s.IdSalon " +
                    "inner join Servicios serv on serv.IdEmpresa = s.IdEmpresa and serv.IdServicio=f.IdServicio " +
                    "where f.NFicha=@nficha and us.Salon=@sal " +
                    "order by f.Linea";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@nficha", nficha);
                sc.Parameters.AddWithValue("@sal", user.Salon);
                DataSet ds = await db.GetDataSet(sc, "lineas");
                for (int i = 0; i <= ds.Tables["lineas"].Rows.Count - 1; i++)
                {
                    DataRow fila = ds.Tables["lineas"].Rows[i];
                    Ficha_Linea f = new Ficha_Linea();
                    f.Base = double.Parse(fila["Base"].ToString());
                    f.Descripcion = fila["Descripcion"].ToString();
                    f.DescuentoCant = double.Parse(fila["DescuentoCant"].ToString());
                    f.DescuentoPorc = double.Parse(fila["DescuentoPorc"].ToString());
                    f.Empleado = fila["Empleado"].ToString();
                    f.Tipo = fila["Tipo"].ToString();
                    f.IvaCant = double.Parse(fila["IvaCant"].ToString());
                    f.IvaPorc = double.Parse(fila["IvaPorc"].ToString());
                    f.Linea = int.Parse(fila["Linea"].ToString());
                    f.Total = double.Parse(fila["Total"].ToString());
                    lst.Add(f);
                }
            }
            return Json(new { data = lst });
        }

        [HttpGet]
        public async Task<Servicio> getServicios(string search, string salon)
        {
            Servicio ret = new Servicio();
            try
            {
                DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
                string sql = "select s.IdServicio,s.Codigo,s.Tipo,s.Nombre,s.Precio,s.IvaPorc,s.IvaCant,s.PVP " +
                    "from Servicios s " +
                    "inner join Empresas e on s.IdEmpresa = e.IdEmpresa " + 
                    "inner join Salones sal on e.IdEmpresa = sal.IdEmpresa " +
                    "where s.Activo='True' and sal.IdSalon=@sal and s.Codigo=@cod";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@sal", salon);
                sc.Parameters.AddWithValue("@cod", search);
                DataSet ds = await db.GetDataSet(sc, "serv");
                if (ds.Tables["serv"].Rows.Count > 0)
                {
                    ret.IdServicio = int.Parse(ds.Tables["serv"].Rows[0]["IdServicio"].ToString());
                    ret.IvaCant = double.Parse(ds.Tables["serv"].Rows[0]["IvaCant"].ToString());
                    ret.IvaPorc = double.Parse(ds.Tables["serv"].Rows[0]["IvaPorc"].ToString());
                    ret.Nombre = ds.Tables["serv"].Rows[0]["Nombre"].ToString();
                    ret.Precio = double.Parse(ds.Tables["serv"].Rows[0]["Precio"].ToString());
                    ret.PVP = double.Parse(ds.Tables["serv"].Rows[0]["PVP"].ToString());
                    ret.Tipo = ds.Tables["serv"].Rows[0]["Tipo"].ToString();
                }
            }
            catch { }
            return ret;
        }

        private async Task<Ficha> guarda_Ficha(DBExtension db, Ficha datos)
        {
            try
            {
                string sql = "";
                if (datos.NFicha.Equals("(sin guardar)"))
                {
                    SqlTransaction t = db.GetTransaction();
                    try
                    {
                        sql = "select ISNULL(Max(Numero),0) + 1 from Fichas where Anio=@anio and Mes=@mes and IdSalon=@sal";
                        SqlCommand sc = new SqlCommand(sql);
                        sc.Transaction = t;
                        sc.Parameters.AddWithValue("@anio", datos.Fecha.Year);
                        sc.Parameters.AddWithValue("@mes", datos.Fecha.Month);
                        sc.Parameters.AddWithValue("@sal", datos.IdSalon);
                        DataSet ds = await db.GetDataSet(sc, "max");
                        sql = "insert into Fichas values(@nficha,@fecha,@anio,@mes,@numero,@idsalon,@idcliente,@formapago,@base," +
                            "@descuentoporc,@descuentoimp,@descuentos,@iva,@total,@pagado,@cambio,@cerrada)";
                        sc = new SqlCommand(sql);
                        sc.Transaction = t;
                        datos.NFicha = datos.Fecha.Year.ToString() + Extensions.Utils.FormatoFecha(datos.Fecha.Month.ToString()) + Extensions.Utils.FormatoCadena(ds.Tables["max"].Rows[0][0].ToString(), 3);
                        sc.Parameters.AddWithValue("@nficha", datos.NFicha);
                        sc.Parameters.AddWithValue("@fecha", datos.Fecha);
                        sc.Parameters.AddWithValue("@anio", datos.Fecha.Year);
                        sc.Parameters.AddWithValue("@mes", datos.Fecha.Month);
                        sc.Parameters.AddWithValue("@numero", ds.Tables["max"].Rows[0][0]);
                        sc.Parameters.AddWithValue("@idsalon", datos.IdSalon);
                        sc.Parameters.AddWithValue("@idcliente", datos.IdCliente);
                        sc.Parameters.AddWithValue("@formapago", datos.FormaPago);
                        sc.Parameters.AddWithValue("@base", datos.Base);
                        sc.Parameters.AddWithValue("@descuentoporc", datos.DescuentoPorc);
                        sc.Parameters.AddWithValue("@descuentoimp", datos.DescuentoImp);
                        sc.Parameters.AddWithValue("@descuentos", datos.TotalDescuentos);
                        sc.Parameters.AddWithValue("@iva", datos.Iva);
                        sc.Parameters.AddWithValue("@total", datos.Total);
                        sc.Parameters.AddWithValue("@pagado", datos.Pagado);
                        sc.Parameters.AddWithValue("@cambio", datos.Cambio);
                        sc.Parameters.AddWithValue("@cerrada", false);
                        db.Command(sc);
                        t.Commit();
                        db.Close();
                    }
                    catch 
                    {
                        try
                        {
                            datos.NFicha = "(sin guardar)";
                            if (t != null)
                            {
                                t.Rollback();
                            }
                            db.Close();
                        }
                        catch { }
                        throw;
                    }
                }
                else
                {
                    sql = "update Fichas set IdCliente=@idcliente, FormaPago=@formapago, Base=@base, DescuentoPorc=@descuentoporc, " +
                        "DescuentoImp=@descuentoimp, Descuentos=@descuentos, iva=@iva, Total=@total, Pagado=@pagado, Cambio=@cambio, " +
                        "Cerrada=@cerrada " +
                        "where NFicha=@nficha and IdSalon=@idsalon";
                    SqlCommand sc = new SqlCommand(sql);
                    sc.Parameters.AddWithValue("@idcliente", datos.IdCliente);
                    sc.Parameters.AddWithValue("@formapago", datos.FormaPago);
                    sc.Parameters.AddWithValue("@base", datos.Base);
                    sc.Parameters.AddWithValue("@descuentoporc", datos.DescuentoPorc);
                    sc.Parameters.AddWithValue("@descuentoimp", datos.DescuentoImp);
                    sc.Parameters.AddWithValue("@descuentos", datos.TotalDescuentos);
                    sc.Parameters.AddWithValue("@iva", datos.Iva);
                    sc.Parameters.AddWithValue("@total", datos.Total);
                    sc.Parameters.AddWithValue("@pagado", datos.Pagado);
                    sc.Parameters.AddWithValue("@cambio", datos.Cambio);
                    sc.Parameters.AddWithValue("@cerrada", datos.Cerrada);
                    sc.Parameters.AddWithValue("@nficha", datos.NFicha);
                    sc.Parameters.AddWithValue("@idsalon", datos.IdSalon);
                    db.Command(sc);
                }
            }
            catch { throw; }
            return datos;
        }

        private async Task<Ficha_Linea> guardaLinea(DBExtension db, Ficha_Linea linea, string nficha, int salon, double descCabecera)
        {
            try
            {
                double descCant = linea.Base * (linea.DescuentoPorc / 100);
                if (linea.Tipo.Equals("Servicio"))
                {
                    //no se aplican descuentos a productos, sólo a servicios
                    descCant += (linea.Base - descCant) * (descCabecera / 100);
                }
                linea.DescuentoCant = Math.Round(descCant, 2);
                linea.IvaCant = Math.Round((linea.Base - linea.DescuentoCant) * (linea.IvaPorc / 100), 3);
                linea.Total = linea.Base - linea.DescuentoCant + linea.IvaCant;
                string sql = "";
                if (linea.Linea == 0)
                {
                    SqlTransaction t = db.GetTransaction();
                    try
                    {
                        sql = "select ISNULL(Max(Linea),0) + 1 from Fichas_Lineas where NFicha=@nficha and IdSalon=@sal";
                        SqlCommand sc = new SqlCommand(sql);
                        sc.Transaction = t;
                        sc.Parameters.AddWithValue("@nficha", nficha);
                        sc.Parameters.AddWithValue("@sal", salon);
                        DataSet ds = await db.GetDataSet(sc, "max");
                        linea.Linea = int.Parse(ds.Tables["max"].Rows[0][0].ToString());
                        sql = "insert into Fichas_Lineas values(@nficha,@idsalon,@linea,@codigo,@idservicio,@descripcion," +
                            "@base,@descuentoporc,@descuentocant,@ivaporc,@ivacant,@total,@comision1,@comision2,@comision3,@comision4)";
                        sc = new SqlCommand(sql);
                        sc.Transaction = t;
                        sc.Parameters.AddWithValue("@nficha", nficha);
                        sc.Parameters.AddWithValue("@idsalon", salon);
                        sc.Parameters.AddWithValue("@linea", linea.Linea);
                        sc.Parameters.AddWithValue("@codigo", linea.Codigo);
                        sc.Parameters.AddWithValue("@idservicio", linea.IdServicio);
                        sc.Parameters.AddWithValue("@descripcion", linea.Descripcion);
                        sc.Parameters.AddWithValue("@base", linea.Base);
                        sc.Parameters.AddWithValue("@descuentoporc", linea.DescuentoPorc);
                        sc.Parameters.AddWithValue("@descuentocant", linea.DescuentoCant);
                        sc.Parameters.AddWithValue("@ivaporc", linea.IvaPorc);
                        sc.Parameters.AddWithValue("@ivacant", linea.IvaCant);
                        sc.Parameters.AddWithValue("@total", linea.Total);
                        Servicio_Comision comisiones = await this.getComision(db, linea, t);
                        sc.Parameters.AddWithValue("@comision1", comisiones.ComisionP1);
                        sc.Parameters.AddWithValue("@comision2", comisiones.ComisionP2);
                        sc.Parameters.AddWithValue("@comision3", comisiones.ComisionP3);
                        sc.Parameters.AddWithValue("@comision4", comisiones.ComisionP4);
                        db.Command(sc);
                        t.Commit();
                        db.Close();
                    }
                    catch 
                    {
                        try
                        {
                            if (t != null)
                            {
                                t.Rollback();
                            }
                            db.Close();
                        }
                        catch { }
                        throw;
                    }
                }
                else
                {
                    sql = "update Fichas_Lineas set Codigo=@codigo,IdServicio=@idservicio,Descripcion=@descripcion," +
                        "Base=@base,DescuentoPorc=@descuentoporc,DescuentoCant=@descuentocant,IvaPorc=@ivaporc," +
                        "IvaCant=@ivacant,Total=@total,ComisionP1=@comision1,ComisionP2=@comision2,ComisionP3=@comision3,ComisionP4=@comision4 " +
                        "where NFicha=@nficha and IdSalon=@idsalon and Linea=@linea ";
                    SqlCommand sc = new SqlCommand(sql);
                    sc.Parameters.AddWithValue("@codigo", linea.Codigo);
                    sc.Parameters.AddWithValue("@idservicio", linea.IdServicio);
                    sc.Parameters.AddWithValue("@descripcion", linea.Descripcion);
                    sc.Parameters.AddWithValue("@base", linea.Base);
                    sc.Parameters.AddWithValue("@descuentoporc", linea.DescuentoPorc);
                    sc.Parameters.AddWithValue("@descuentocant", linea.DescuentoCant);
                    sc.Parameters.AddWithValue("@ivaporc", linea.IvaPorc);
                    sc.Parameters.AddWithValue("@ivacant", linea.IvaCant);
                    sc.Parameters.AddWithValue("@total", linea.Total);
                    Servicio_Comision comisiones = await this.getComision(db, linea);
                    sc.Parameters.AddWithValue("@comision1", comisiones.ComisionP1);
                    sc.Parameters.AddWithValue("@comision2", comisiones.ComisionP2);
                    sc.Parameters.AddWithValue("@comision3", comisiones.ComisionP3);
                    sc.Parameters.AddWithValue("@comision4", comisiones.ComisionP4);
                    sc.Parameters.AddWithValue("@nficha", nficha);
                    sc.Parameters.AddWithValue("@idsalon", salon);
                    sc.Parameters.AddWithValue("@linea", linea.Linea);
                    db.Command(sc);
                }
            }catch { throw; }
            return linea;
        }

        public async Task<IActionResult> Historia(ViewModelCliente data, string deleteId)
        {
            DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
            try
            {
                db.OpenConnection();
                ClientesExtension cliext = new ClientesExtension(db);
                if (string.IsNullOrEmpty(deleteId))
                {
                    //guardar nueva línea de la historia
                    Cliente_Historia ch = await cliext.GuardaHistoria(data.Cliente.IdCliente, data.NuevaHistoria);
                    data.NuevaHistoria = "";
                }
                else
                {
                    //borrar línea de la historia
                    cliext.BorraHistoria(data.Cliente.IdCliente, data.IdBorrar);
                }
                db.Close();
                return RedirectToAction("Ficha", new { numficha = data.FichaRef });
            }
            catch (Exception ex)
            {
                try
                {
                    db.Close();
                }
                catch { };
                ViewBag.StatusMessage = "Error: " + ex.Message;
                return View(data);
            }
        }

        public async Task<FileStreamResult> Print(string nficha, string salon)
        {
            DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
            try
            {
                db.OpenConnection();
                Ficha f = await this.CargaFicha(db, nficha, salon);
                ViewModelFicha data = new ViewModelFicha();
                data.Datos = f;
                data = await this.Ficha_Cabecera(db, data, "");
                ExportPDF pdf = new ExportPDF(_env.WebRootPath, db);
                FileStreamResult ret = await pdf.ExportFicha(data);
                db.Close();
                return ret;
            }
            catch 
            {
                try
                {
                    db.Close();
                }
                catch { }
                return null;
            }
        }

    }
}
