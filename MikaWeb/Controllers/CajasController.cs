using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MikaWeb.Areas.Identity.Data;
using MikaWeb.Data;
using MikaWeb.Extensions;
using MikaWeb.Extensions.DB;
using MikaWeb.Models;
using MikaWeb.Models.AuxiliaryModels;
using MikaWeb.Models.ViewModels;

namespace MikaWeb.Controllers
{
    [Authorize]
    public class CajasController : Controller
    {

        private readonly ILogger<CajasController> _logger;
        private readonly MikaWebContext _context;
        private readonly DBConfig _dbConfig;
        private readonly UserManager<MikaWebUser> _userManager;

        private IQueryable<Ficha> fichasCaja = null;
        private IQueryable<Cajas> lstCajas = null;
        private IQueryable<Cajas_Gastos> lstGastos = null;

        public CajasController(ILogger<CajasController> logger, MikaWebContext context, IOptions<DBConfig> dbConf, UserManager<MikaWebUser> userManager)
        {
            _logger = logger;
            _context = context;
            _dbConfig = dbConf.Value;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<PartialViewResult> AddOrEdit([FromBody]ViewModelCaja data, int nlinea)
        {
            if (nlinea == -1)
            {
                data.LineaGasto = new Cajas_Gastos();
                data.LineaGasto.NCaja = data.Caja.NCaja;
                data.LineaGasto.IdSalon = data.Caja.IdSalon;
            }
            else
            {
                try
                {
                    DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
                    string sql = "Select Concepto, Importe, Iva from Cajas_Gastos " +
                        "where NCaja=@caja and IdSalon=@salon and Linea=@linea";
                    SqlCommand sc = new SqlCommand(sql);
                    sc.Parameters.AddWithValue("@caja", data.Caja.NCaja);
                    sc.Parameters.AddWithValue("@salon", data.Caja.IdSalon);
                    sc.Parameters.AddWithValue("@linea", nlinea);
                    DataSet ds = await db.GetDataSet(sc, "linea");
                    DataRow fila = ds.Tables["linea"].Rows[0];
                    data.LineaGasto = new Cajas_Gastos();
                    data.LineaGasto.NCaja = data.Caja.NCaja;
                    data.LineaGasto.IdSalon = data.Caja.IdSalon;
                    data.LineaGasto.Linea = nlinea;
                    data.LineaGasto.Concepto = fila["Concepto"].ToString();
                    data.LineaGasto.Importe = double.Parse(fila["Importe"].ToString());
                    data.LineaGasto.Iva = double.Parse(fila["Iva"].ToString());
                }
                catch (Exception ex)
                {
                    ViewBag.StatusMessage = "Error: " + ex.Message;
                }
            }
            return PartialView("AddOrEdit", data);
        }

        private void BorraFicha(DBExtension db, string nFicha, int salon)
        {
            SqlTransaction t = db.GetTransaction();
            try
            {
                string sql = "delete from Fichas_Lineas where NFicha=@nficha and IdSalon=@salon";
                SqlCommand sc = new SqlCommand(sql);
                sc.Transaction = t;
                sc.Parameters.AddWithValue("@nficha", nFicha);
                sc.Parameters.AddWithValue("@salon", salon);
                db.Command(sc);
                sql = "delete from Gestoria_Fichas where FichaCaja=@nficha and IdSalon=@salon";
                sc = new SqlCommand(sql);
                sc.Transaction = t;
                sc.Parameters.AddWithValue("@nficha", nFicha);
                sc.Parameters.AddWithValue("@salon", salon);
                db.Command(sc);
                sql = "delete from Fichas where NFicha=@nficha and IdSalon=@salon";
                sc = new SqlCommand(sql);
                sc.Transaction = t;
                sc.Parameters.AddWithValue("@nficha", nFicha);
                sc.Parameters.AddWithValue("@salon", salon);
                db.Command(sc);
                db.CommitTransaction(t);
                db.OpenConnection();
            }
            catch
            {
                try
                {
                    t.Rollback();
                }
                catch { }
                throw;
            }
        }

        private void BorraLinea(DBExtension db, string numcaja, int salon, int idLinea)
        {
            try
            {
                string sql = "delete from Cajas_Gastos where NCaja=@ncaja and IdSalon=@idsalon and Linea=@linea";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@ncaja", numcaja);
                sc.Parameters.AddWithValue("@idsalon", salon);
                sc.Parameters.AddWithValue("@linea", idLinea);
                db.Command(sc);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> Caja(string nCaja, string acc)
        {
            ViewModelCaja ret = new ViewModelCaja();
            DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
            try
            {
                Cajas c = new Cajas();
                db.OpenConnection();
                var user = await _userManager.GetUserAsync(User);
                string sql = "select Fecha, Cerrada, " +
                    "(select isnull(sum(Importe),0) from Cajas_Gastos where NCaja=Cajas.NCaja and IdSalon=Cajas.IdSalon) as Gastos, " +
                    "(select isnull(Sum(Iva), 0) from Cajas_Gastos where NCaja = Cajas.NCaja and IdSalon = Cajas.IdSalon) as IvaSoportado " +
                    "from Cajas where NCaja=@ncaja and IdSalon=@idsalon";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@ncaja", nCaja);
                sc.Parameters.AddWithValue("@idsalon", user.Salon);
                DataSet ds = await db.GetDataSet(sc, "caja");
                if (ds.Tables["caja"].Rows.Count > 0)
                {
                    DataRow fila = ds.Tables["caja"].Rows[0];
                    if (!string.IsNullOrEmpty(acc))
                    {
                        if (acc.Equals("ROP"))
                        {
                            if (user.IsAdmin)
                            {
                                c.Cerrada = false;
                            }
                            else
                            {
                                throw new Exception("No tiene permisos para reabrir la caja");
                            }
                        }
                        else
                        {
                            throw new Exception("Se ha encontrado un parámetro incorrecto");
                        }
                    }
                    else
                    {
                        c.Cerrada = bool.Parse(fila["Cerrada"].ToString());
                    }
                    c.Fecha = DateTime.Parse(fila["Fecha"].ToString());
                    c.Anio = c.Fecha.Year;
                    c.Mes = c.Fecha.Month;
                    c.NCaja = nCaja;
                    c.IdSalon = user.Salon;
                    c.Gastos = double.Parse(fila["Gastos"].ToString());
                    c.IvaSoportado = double.Parse(fila["IvaSoportado"].ToString());
                    ret.Caja = c;
                    ret = await this.Caja_Cabecera(db, ret);
                }
                else
                {
                    c.Fecha = new DateTime(int.Parse(nCaja.Substring(0,4)), int.Parse(nCaja.Substring(4,2)), int.Parse(nCaja.Substring(6,2)));
                    ret.Caja = c;
                    throw new Exception("La caja del día no está creada. No existen fichas para el día indicado");
                }
                ret.SaldoBruto = Math.Round(c.Metalico + c.Visas - c.Gastos,2);
                ret.RetiradaEfectivo = Math.Round(c.Metalico - c.Gastos, 2);
                this.guardaCaja(db, ret.Caja);
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
            return View(ret);
        }

        [HttpPost]
        public async Task<IActionResult> Caja(ViewModelCaja data, string acc)
        {
            DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
            try
            {
                if (!string.IsNullOrEmpty(acc))
                {
                    db.OpenConnection();
                    if (acc.Contains("P"))
                    {
                        List<DataTable> lstTablas = new List<DataTable>();
                        List<string> lstHojas = new List<string>();
                        if (fichasCaja == null)
                        {
                            List<Ficha> lst = await this.getFichas(db, data.Caja.NCaja, data.Caja.IdSalon);
                            fichasCaja = lst.AsQueryable();
                        }
                        DataTable dtFichas = LinqExtensions.CopyToDataTable<Ficha>(fichasCaja);
                        dtFichas.Columns.Remove("Anio");
                        dtFichas.Columns.Remove("Mes");
                        dtFichas.Columns.Remove("IdSalon");
                        dtFichas.Columns.Remove("IdCliente");
                        dtFichas.Columns.Remove("DescuentoPorc");
                        dtFichas.Columns.Remove("DescuentoImp");
                        dtFichas.Columns.Remove("Pagado");
                        dtFichas.Columns.Remove("Cambio");
                        dtFichas.Columns.Remove("Lineas");
                        lstTablas.Add(dtFichas);
                        lstHojas.Add("Caja-" + data.Caja.NCaja);
                        if (acc.Equals("PD"))
                        {
                            DateTime fecha = new DateTime(int.Parse(data.Caja.NCaja.Substring(0, 4)), int.Parse(data.Caja.NCaja.Substring(4, 2)), int.Parse(data.Caja.NCaja.Substring(6, 2)));
                            string sql = "select f.NFicha, f.DescuentoPorc, fl.Linea, fl.Codigo as CodigoEmp, us.Nombre + ' ' + us.Apellidos as Empleado, " +
                                "serv.Tipo, serv.Codigo as CodigoServ, fl.Descripcion, fl.Base, fl.DescuentoCant as Descuento, fl.IvaCant as Iva, fl.Total " +
                                "from Fichas f " +
                                "inner join Clientes c on f.IdCliente = c.IdCliente and f.IdSalon = c.IdSalon " +
                                "inner join Fichas_Lineas fl on f.NFicha = fl.NFicha and f.IdSalon = fl.IdSalon " +
                                "inner join AspNetUsers us on fl.Codigo = us.Codigo and fl.IdSalon = us.Salon " +
                                "inner join Salones s on us.Salon = s.IdSalon " +
                                "inner join Servicios serv on fl.IdServicio = serv.IdServicio and s.IdEmpresa = serv.IdEmpresa " +
                                "Where f.Fecha=@fecha and f.IdSalon=@salon " +
                                "order by f.NFicha, fl.Linea";
                            SqlCommand sc = new SqlCommand(sql);
                            sc.Parameters.AddWithValue("@fecha", fecha);
                            sc.Parameters.AddWithValue("@salon", data.Caja.IdSalon);
                            DataSet ds = await db.GetDataSet(sc, "detalle");
                            DataTable dtDetalle = ds.Tables["detalle"];
                            lstTablas.Add(dtDetalle);
                            lstHojas.Add("Detalle-" + data.Caja.NCaja);
                            //-----------------
                            lstGastos = this.getGastos(data.Caja.IdSalon, data.Caja.NCaja);
                            DataTable dtGastos = LinqExtensions.CopyToDataTable<Cajas_Gastos>(lstGastos);
                            dtGastos.Columns.Remove("IdSalon");
                            lstTablas.Add(dtGastos);
                            lstHojas.Add("Gastos-" + data.Caja.NCaja);
                        }
                        Export conf = new Export();
                        conf.NombreArchivo = "Caja" + data.Caja.NCaja + ".xlsx";
                        conf.NombreHoja = "Caja-" + data.Caja.NCaja;
                        ExportXLS exp = new ExportXLS(conf);
                        return exp.Export(lstTablas, lstHojas);
                    }
                    else
                    {
                        if (acc.Equals("L"))
                        {
                            //guardar línea
                            Cajas_Gastos l = await this.guardaLinea(db, data.LineaGasto);
                            data.Caja = await this.Caja_CabeceraGastos(data.Caja);
                            ModelState.Clear();
                        }
                        else
                        {
                            if (acc.Equals("B"))
                            {
                                //borrar línea
                                this.BorraLinea(db, data.Caja.NCaja, data.Caja.IdSalon, data.LineaGasto.Linea);
                                data.Caja = await this.Caja_CabeceraGastos(data.Caja);
                                ModelState.Clear();
                            }
                            else
                            {
                                if (acc.Equals("S"))
                                {
                                    data.Caja.Cerrada = true;
                                    ModelState.Clear();
                                }
                                else
                                {
                                    if (acc.Equals("BF"))
                                    {
                                        this.BorraFicha(db, data.FichaBorrar, data.Caja.IdSalon);
                                        ModelState.Clear();
                                    }
                                }
                            }
                        }
                        //guardar caja
                        data = await this.Caja_Cabecera(db, data);
                        data.SaldoBruto = Math.Round(data.Caja.Metalico + data.Caja.Visas - data.Caja.Gastos, 2);
                        data.RetiradaEfectivo = Math.Round(data.Caja.Metalico - data.Caja.Gastos, 2);
                        this.guardaCaja(db, data.Caja);
                    }
                    db.Close();
                }
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

        private async Task<ViewModelCaja> Caja_Cabecera(DBExtension db, ViewModelCaja ret)
        {
            try
            {
                Cajas c = ret.Caja;
                List<Ficha> fichas = await this.getFichas(db, c.NCaja, c.IdSalon);
                c.Metalico = 0;
                c.Visas = 0;
                c.IvaRepercutido = 0;
                ret.Listado = 0;
                for (int i = 0; i <= fichas.Count - 1; i++)
                {
                    Ficha f = fichas[i];
                    if (f.FormaPago.Equals("Efectivo"))
                    {
                        c.Metalico += f.Total;
                    }
                    else
                    {
                        c.Visas += f.Total;
                    }
                    c.IvaRepercutido += f.Iva;
                    ret.Listado += f.Iva;
                }
                c.Metalico = Math.Round(c.Metalico, 2);
                c.Visas = Math.Round(c.Visas, 2);
                c.IvaRepercutido = Math.Round(c.IvaRepercutido, 2);
                c.SaldoNeto = Math.Round(c.Metalico + c.Visas - c.IvaRepercutido - c.Gastos + c.IvaSoportado,2);
                ret.Caja = c;
            }
            catch
            {
                throw;
            }
            return ret;
        }

        private async Task<Cajas> Caja_CabeceraGastos(Cajas c)
        {
            try
            {
                DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
                string sql = "select (select isnull(sum(Importe),0) from Cajas_Gastos where NCaja=Cajas.NCaja and IdSalon=Cajas.IdSalon) as Gastos, " +
                    "(select isnull(Sum(Iva), 0) from Cajas_Gastos where NCaja = Cajas.NCaja and IdSalon = Cajas.IdSalon) as IvaSoportado " +
                    "from Cajas where NCaja=@ncaja and IdSalon=@idsalon";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@ncaja", c.NCaja);
                sc.Parameters.AddWithValue("@idsalon", c.IdSalon);
                DataSet ds = await db.GetDataSet(sc, "caja");
                DataRow fila = ds.Tables["caja"].Rows[0];
                c.Gastos = double.Parse(fila["Gastos"].ToString());
                c.IvaSoportado = double.Parse(fila["IvaSoportado"].ToString());
            }
            catch
            {
                throw;
            }
            return c;
        }

        private async Task<IQueryable<Cajas>> getCajas(int anio, int mes)
        {
            var ret = _context.Cajas.AsQueryable();
            var user = await _userManager.GetUserAsync(User);
            ret = ret.Where(r => r.IdSalon == user.Salon && r.Anio == anio && r.Mes == mes);
            return ret;
        }

        private async Task<List<Ficha>> getFichas(DBExtension db, string nCaja, int idSalon)
        {
            List<Ficha> lst = new List<Ficha>();
            try
            {
                var user = await _userManager.GetUserAsync(User);
                DateTime fecha = new DateTime(int.Parse(nCaja.Substring(0, 4)), int.Parse(nCaja.Substring(4, 2)), int.Parse(nCaja.Substring(6, 2)));
                string sql = "Select Cerrada from Cajas where NCaja=@ncaja and IdSalon=@salon";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@ncaja", nCaja);
                sc.Parameters.AddWithValue("@salon", idSalon);
                DataSet ds = await db.GetDataSet(sc, "caja");
                bool cajaCerrada = bool.Parse(ds.Tables["caja"].Rows[0]["Cerrada"].ToString());
                sql = "select f.NFicha, f.Fecha, c.Nombre as Cliente, f.FormaPago, f.Base, f.Descuentos, " +
                    "f.Iva, f.Total " +
                    "from Fichas f " +
                    "inner join Clientes c on f.IdCliente = c.IdCliente and f.IdSalon = c.IdSalon " +
                    "Where f.Fecha=@fecha and f.IdSalon=@salon";
                sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@fecha", fecha);
                sc.Parameters.AddWithValue("@salon", idSalon);
                ds = await db.GetDataSet(sc, "fichas");
                for (int i = 0; i <= ds.Tables["fichas"].Rows.Count - 1; i++)
                {
                    DataRow fila = ds.Tables["fichas"].Rows[i];
                    Ficha f = new Ficha();
                    f.NFicha = fila["NFicha"].ToString();
                    f.Fecha = DateTime.Parse(fila["Fecha"].ToString());
                    f.Cliente = fila["Cliente"].ToString();
                    f.FormaPago = fila["FormaPago"].ToString();
                    f.Base = double.Parse(fila["Base"].ToString());
                    f.DescuentoImp = double.Parse(fila["Descuentos"].ToString());
                    f.Iva = double.Parse(fila["Iva"].ToString());
                    f.Total = double.Parse(fila["Total"].ToString());
                    if (!cajaCerrada)
                    {
                        if (user.IsAdmin)
                        {
                            f.Anulable = "SI";
                        }
                        else
                        {
                            f.Anulable = "N/A";
                        }
                    }
                    else
                    {
                        f.Anulable = "N/A";
                    }
                    lst.Add(f);
                }
            }
            catch
            {
                throw;
            }
            return lst;
        }

        private IQueryable<Cajas_Gastos> getGastos(int IdSalon, string nCaja)
        {
            var result = _context.Cajas_Gastos.AsQueryable();
            result = result.Where(r => r.IdSalon == IdSalon && r.NCaja.Equals(nCaja));
            return result;
        }

        private Cajas guardaCaja(DBExtension db, Cajas datos)
        {
            try
            {
                string sql = "update Cajas set Cerrada=@cerrada, Metalico=@metalico, Visas=@visas, Gastos=@gastos, " +
                    "IvaSoportado=@ivasop, IvaRepercutido=@ivarep, SaldoNeto=@saldo " +
                    "where NCaja=@ncaja and IdSalon=@sal";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@cerrada", datos.Cerrada);
                sc.Parameters.AddWithValue("@metalico", datos.Metalico);
                sc.Parameters.AddWithValue("@visas", datos.Visas);
                sc.Parameters.AddWithValue("@gastos", datos.Gastos);
                sc.Parameters.AddWithValue("@ivasop", datos.IvaSoportado);
                sc.Parameters.AddWithValue("@ivarep", datos.IvaRepercutido);
                sc.Parameters.AddWithValue("@saldo", datos.SaldoNeto);
                sc.Parameters.AddWithValue("@ncaja", datos.NCaja);
                sc.Parameters.AddWithValue("@sal", datos.IdSalon);
                db.Command(sc);
                if (datos.Cerrada)
                {
                    DateTime fecha = new DateTime(int.Parse(datos.NCaja.Substring(0, 4)), int.Parse(datos.NCaja.Substring(4, 2)), int.Parse(datos.NCaja.Substring(6, 2)));
                    sql = "update Fichas set Cerrada='True' Where Fecha=@fec and IdSalon=@salon";
                    sc = new SqlCommand(sql);
                    sc.Parameters.AddWithValue("@fec", fecha);
                    sc.Parameters.AddWithValue("@salon", datos.IdSalon);
                    db.Command(sc);
                }
            }
            catch
            {
                throw;
            }
            return datos;
        }

        private async Task<Cajas_Gastos> guardaLinea(DBExtension db, Cajas_Gastos linea)
        {
            try
            {
                if (linea.Linea == 0)
                {
                    SqlTransaction t = db.GetTransaction();
                    try
                    {
                        string sql = "select Isnull(max(Linea),0) + 1 from Cajas_Gastos where NCaja=@ncaja and IdSalon=@sal";
                        SqlCommand sc = new SqlCommand(sql);
                        sc.Transaction = t;
                        sc.Parameters.AddWithValue("@ncaja", linea.NCaja);
                        sc.Parameters.AddWithValue("@sal", linea.IdSalon);
                        DataSet ds = await db.GetDataSet(sc, "max");
                        linea.Linea = int.Parse(ds.Tables["max"].Rows[0][0].ToString());
                        sql = "Insert into Cajas_Gastos Values(@ncaja,@sal,@linea,@concepto,@importe,@iva)";
                        sc = new SqlCommand(sql);
                        sc.Transaction = t;
                        sc.Parameters.AddWithValue("@ncaja", linea.NCaja);
                        sc.Parameters.AddWithValue("@sal", linea.IdSalon);
                        sc.Parameters.AddWithValue("@linea", linea.Linea);
                        sc.Parameters.AddWithValue("@concepto", linea.Concepto);
                        sc.Parameters.AddWithValue("@importe", linea.Importe);
                        sc.Parameters.AddWithValue("@iva", linea.Iva);
                        db.Command(sc);
                        t.Commit();
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
                    string sql = "update Cajas_Gastos set Concepto=@concepto, Importe=@importe, Iva=@iva " +
                        "where NCaja=@ncaja and IdSalon=@sal and Linea=@linea";
                    SqlCommand sc = new SqlCommand(sql);
                    sc.Parameters.AddWithValue("@concepto", linea.Concepto);
                    sc.Parameters.AddWithValue("@importe", linea.Importe);
                    sc.Parameters.AddWithValue("@iva", linea.Iva);
                    sc.Parameters.AddWithValue("@ncaja", linea.NCaja);
                    sc.Parameters.AddWithValue("@sal", linea.IdSalon);
                    sc.Parameters.AddWithValue("@linea", linea.Linea);
                    db.Command(sc);
                }
            }
            catch
            {
                throw;
            }
            return linea;
        }

        public async Task<IActionResult> Index(ViewModelCaja data, string print)
        {
            if (data.FiltroAnio==0)
            {
                data = new ViewModelCaja();
                data.FiltroAnio = System.DateTime.Now.Year;
                data.FiltroMes = Extensions.Utils.NombreMes(System.DateTime.Now.Month);
            }
            else
            {
                if (!string.IsNullOrEmpty(print))
                {
                    lstCajas = await this.getCajas(data.FiltroAnio, Extensions.Utils.NumeroMes(data.FiltroMes));
                    DataTable dtCajas = LinqExtensions.CopyToDataTable<Cajas>(lstCajas);
                    dtCajas.Columns.RemoveAt(1);
                    Export conf = new Export();
                    conf.NombreArchivo = "Cajas" + data.FiltroAnio.ToString() + "-" + data.FiltroMes + ".xlsx";
                    conf.NombreHoja = "Cajas-" + data.FiltroAnio.ToString() + "-" + data.FiltroMes;
                    ExportXLS exp = new ExportXLS(conf);
                    return exp.Export(dtCajas);
                }
            }
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> LoadFichas([FromBody]DtParameters dtParameters)
        {
            var orderCriteria = "NFicha";
            var orderAscendingDirection = false;
            if (dtParameters.Order != null)
            {
                orderCriteria = dtParameters.Columns[dtParameters.Order[0].Column].Data;
                orderAscendingDirection = dtParameters.Order[0].Dir.ToString().ToLower() == "asc";
            }
            string nCaja = dtParameters.AdditionalValues.ToList<string>()[0];
            int opcion = int.Parse(dtParameters.AdditionalValues.ToList<string>()[1]);
            if (fichasCaja == null)
            {
                DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    List<Ficha> lst = await this.getFichas(db, nCaja, user.Salon);
                    fichasCaja = lst.AsQueryable();
                }
                catch { }
            }
            var ret = fichasCaja;
            if (opcion > 0)
            {
                if (opcion == 1)
                {
                    //filtrar sólo efectivo
                    ret = fichasCaja.Where(r => r.FormaPago.Equals("Efectivo"));
                }
                else
                {
                    //filtrar sólo tarjeta
                    ret = fichasCaja.Where(r => r.FormaPago.Equals("Tarjeta"));
                }
            }
            ret = orderAscendingDirection ? ret.OrderByDynamic(orderCriteria, DtOrderDir.Asc) : ret.OrderByDynamic(orderCriteria, DtOrderDir.Desc);

            return Json(new DtResult<Ficha>
            {
                Draw = dtParameters.Draw,
                RecordsTotal = fichasCaja.Count(),
                RecordsFiltered = ret.Count(),
                Data = ret
            });

        }

        [HttpPost]
        public async Task<IActionResult> LoadGastos([FromBody]DtParameters dtParameters)
        {
            var orderCriteria = "Linea";
            var orderAscendingDirection = true;

            if (dtParameters.Order != null)
            {
                orderCriteria = dtParameters.Columns[dtParameters.Order[0].Column].Data;
                orderAscendingDirection = dtParameters.Order[0].Dir.ToString().ToLower() == "asc";
            }
            if (lstGastos == null)
            {
                string ncaja = dtParameters.AdditionalValues.ToList<string>()[0];
                var user = await _userManager.GetUserAsync(User);
                lstGastos = this.getGastos(user.Salon, ncaja);
                lstGastos = orderAscendingDirection ? lstGastos.OrderByDynamic(orderCriteria, DtOrderDir.Asc) : lstGastos.OrderByDynamic(orderCriteria, DtOrderDir.Desc);
            }

            try
            {
                var filteredResultsCount = await lstGastos.CountAsync();
                var totalResultsCount = await _context.Cajas_Gastos.CountAsync();

                return Json(new DtResult<Cajas_Gastos>
                {
                    Draw = dtParameters.Draw,
                    RecordsTotal = totalResultsCount,
                    RecordsFiltered = filteredResultsCount,
                    Data = await lstGastos.ToListAsync()
                });
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return null;
            }

        }

        [HttpPost]
        public async Task<IActionResult> LoadTable([FromBody]DtParameters dtParameters)
        {
            var orderCriteria = "Fecha";
            var orderAscendingDirection = false;

            if (dtParameters.Order != null)
            {
                orderCriteria = dtParameters.Columns[dtParameters.Order[0].Column].Data;
                orderAscendingDirection = dtParameters.Order[0].Dir.ToString().ToLower() == "asc";
            }
            if (lstCajas == null)
            {
                int anio = int.Parse(dtParameters.AdditionalValues.ToList<string>()[0]);
                int mes = Extensions.Utils.NumeroMes(dtParameters.AdditionalValues.ToList<string>()[1]);
                lstCajas = await this.getCajas(anio, mes);
                lstCajas = orderAscendingDirection ? lstCajas.OrderByDynamic(orderCriteria, DtOrderDir.Asc) : lstCajas.OrderByDynamic(orderCriteria, DtOrderDir.Desc);
            }
            var filteredResultsCount = await lstCajas.CountAsync();
            var totalResultsCount = await _context.Cajas.CountAsync();

            return Json(new DtResult<Cajas>
            {
                Draw = dtParameters.Draw,
                RecordsTotal = totalResultsCount,
                RecordsFiltered = filteredResultsCount,
                Data = await lstCajas
                    .Skip(dtParameters.Start)
                    .Take(dtParameters.Length)
                    .ToListAsync()
            });

        }

    }
}
