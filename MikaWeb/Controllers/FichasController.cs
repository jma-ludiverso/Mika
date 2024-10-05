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
                    FichasExtension fext = new FichasExtension(new DBExtension(_dbConfig.MikaWebContextConnection));
                    data.LineaTrabajo = await fext.getDatosLinea(data.Datos.IdSalon, data.Datos.NFicha, nlinea);
                }
                catch (Exception ex)
                {
                    ViewBag.StatusMessage = "Error: " + ex.Message;
                }
            }
            return PartialView("AddOrEdit", data);
        }

        public async Task<IActionResult> Ficha(ViewModelFicha data, int cli, string numficha, string acc)
        {
            DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
            try
            {
                db.OpenConnection();
                FichasExtension fext = new FichasExtension(db);
                var user = await _userManager.GetUserAsync(User);
                if (data.Datos == null)
                {
                    Ficha f = new Ficha();
                    f.IdCliente = cli;
                    f.FormaPago = "Tarjeta";
                    if (string.IsNullOrEmpty(numficha))
                    {
                        f.Fecha = System.DateTime.Now;
                        f.NFicha = "(sin guardar)";
                        f.IdSalon = user.Salon; 
                    }
                    else
                    {
                        //cargar ficha desde BD
                        f = await fext.CargaFicha(numficha, user.Salon.ToString());
                    }
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
                        data.Datos.Lineas = await fext.CargaFicha_Lineas(data.Datos.NFicha, data.Datos.IdSalon.ToString());
                    }
                }
                if (!string.IsNullOrEmpty(acc))
                {
                    if (acc.Equals("L"))
                    {
                        //guardar línea
                        if (data.Datos.NFicha.Equals("(sin guardar)"))
                        {
                            data.Datos = await fext.guarda_Ficha(data.Datos);
                            ModelState.Clear();
                        }
                        int idlinea = data.LineaTrabajo.Linea;
                        Ficha_Linea l = await fext.guardaLinea(data.LineaTrabajo, data.Datos.NFicha, data.Datos.IdSalon, data.Datos.DescuentoPorc);
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
                            data.Datos.Lineas = await fext.CargaFicha_Lineas(data.Datos.NFicha, data.Datos.IdSalon.ToString());
                        }
                    }
                    else
                    {
                        if (acc.Equals("B"))
                        {
                            //borrar línea
                            fext.BorraLinea(data.Datos.NFicha, data.Datos.IdSalon, data.LineaTrabajo.Linea);
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
                            data.Datos = await fext.guarda_Ficha(data.Datos);
                            if (data.Datos.Cerrada)
                            {
                                return RedirectToAction("Index", "Home");
                            }
                        }
                        data.Datos.Lineas = await fext.CargaFicha_Lineas(data.Datos.NFicha, data.Datos.IdSalon.ToString());
                    }
                }
                List<Cliente> lstCli = await this.GetClientes(db, data.Datos.IdSalon.ToString(), "", data.Datos.IdCliente);
                data.Clientes = new SelectList(lstCli, "IdCliente", "Nombre");
                ViewModelCliente hist = new ViewModelCliente();
                if (data.Datos.IdCliente != 0 && !data.Datos.NFicha.Equals("(sin guardar)"))
                {
                    ClientesExtension cliext = new ClientesExtension(db);
                    hist.Historial = await cliext.GetRecordData(data.Datos.IdCliente);
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
                    data = await this.Ficha_Cabecera(fext, data, acc);
                    data.Datos = await fext.guarda_Ficha(data.Datos);
                }
                db.Close();
            }
            catch (Exception ex)
            {
                try
                {
                    try
                    {
                        List<Cliente> lstCli = await this.GetClientes(db, data.Datos.IdSalon.ToString(), "", data.Datos.IdCliente);
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

        private async Task<ViewModelFicha> Ficha_Cabecera(FichasExtension fext, ViewModelFicha data, string acc)
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
                                l = await fext.guardaLinea(l, data.Datos.NFicha, data.Datos.IdSalon, data.Datos.DescuentoPorc);
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
                    data.Datos = await fext.guarda_Ficha(data.Datos);
                }
                ModelState.Clear();
            }
            catch { throw; }
            return data;
        }

        private async Task<List<Cliente>> GetClientes(DBExtension db, string salon, string search, int idcli = 0)
        {            
            try
            {
                ClientesExtension cext = new ClientesExtension(db);
                return await cext.getClientsData(salon, search, idcli);
            }
            catch (Exception ex)
            {
                ViewBag.StatusMessage = "Error: " + ex.Message;
                return new List<Cliente>();
            }            
        }

        [HttpGet]
        public async Task<JsonResult> getClientes(string search)
        {
            DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
            var user = await _userManager.GetUserAsync(User);
            List<Cliente> lstCli = await this.GetClientes(db, user.Salon.ToString(), search);
            return Json(lstCli);
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
                FichasExtension fext = new FichasExtension(new DBExtension(_dbConfig.MikaWebContextConnection));
                lst = await fext.CargaFicha_Lineas(nficha, user.Salon);
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
                    Cliente_Historia ch = await cliext.SaveRecordData(data.Cliente.IdCliente, data.NuevaHistoria);
                    data.NuevaHistoria = "";
                }
                else
                {
                    //borrar línea de la historia
                    cliext.DeleteRecordData(data.Cliente.IdCliente, data.IdBorrar);
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
                FichasExtension fext = new FichasExtension(db);
                Ficha f = await fext.CargaFicha(nficha, salon);
                ViewModelFicha data = new ViewModelFicha();
                data.Datos = f;
                data = await this.Ficha_Cabecera(fext, data, "");
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
