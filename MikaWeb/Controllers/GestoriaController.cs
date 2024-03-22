using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MikaWeb.Models.ViewModels;
using MikaWeb.Extensions;
using MikaWeb.Extensions.DB;
using Microsoft.AspNetCore.Identity;
using MikaWeb.Areas.Identity.Data;
using Microsoft.Extensions.Options;
using MikaWeb.Models.AuxiliaryModels;
using MikaWeb.Models;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace MikaWeb.Controllers
{
    [Authorize(Policy = "AdminArea")]
    public class GestoriaController : Controller
    {

        private readonly IWebHostEnvironment _env;
        private readonly DBConfig _dbConfig;
        private readonly UserManager<MikaWebUser> _userManager;

        private IQueryable<GestoriaProduccion> lstProducciones = null;
        private IQueryable<Ficha> fichasCaja = null;

        public GestoriaController(IWebHostEnvironment env, IOptions<DBConfig> dbConf, UserManager<MikaWebUser> userManager)
        {
            _env = env;
            _dbConfig = dbConf.Value;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int anio, int nmes, int salon)
        {
            ViewModelGestoria v = new ViewModelGestoria();
            v.Anio = anio;
            v.NMes = nmes;
            v.IdSalon = salon;
            v.Mes = anio.ToString() + "/" + Utils.FormatoFecha(nmes.ToString());
            try
            {
                DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
                string sql = "select IvaRepercutido, IvaSoportado, IvaTarjeta, IvaEfectivo " +
                    "from Gestoria where IdSalon=@sal and Anio=@anio and Mes=@mes";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@sal", salon);
                sc.Parameters.AddWithValue("@anio", anio);
                sc.Parameters.AddWithValue("@mes", nmes);
                DataSet ds = await db.GetDataSet(sc, "datos");
                v.IvaRepercutido = double.Parse(ds.Tables["datos"].Rows[0]["IvaRepercutido"].ToString());
                v.IvaSoportado = double.Parse(ds.Tables["datos"].Rows[0]["IvaSoportado"].ToString());
                v.IvaT = double.Parse(ds.Tables["datos"].Rows[0]["IvaTarjeta"].ToString());
                v.IvaE = double.Parse(ds.Tables["datos"].Rows[0]["IvaEfectivo"].ToString());
            }
            catch (Exception ex)
            {
                ViewBag.StatusMessage = "Error: " + ex.Message;
            }
            return View(v);
        }

        [HttpPost]
        public async Task<IActionResult> Detalle(ViewModelGestoria v, string accion)
        {
            try
            {
                if (string.IsNullOrEmpty(accion))
                {
                    DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
                    try
                    {
                        db.OpenConnection();
                        if (fichasCaja == null)
                        {
                            List<Ficha> lst = await this.getFichas(db, v.IdSalon, v.Anio, v.NMes);
                            fichasCaja = lst.AsQueryable();
                        }
                        Gestoria ges = new Gestoria(db);
                        double ajuste = ges.RecalculoFichas(v, fichasCaja);
                        v.Efectivo = 0;
                        v.IvaE -= ajuste;
                        v.IvaRepercutido -= ajuste;
                        db.Close();
                        ModelState.Clear();
                    }catch 
                    {
                        try
                        {
                            db.Close();
                        }
                        catch { }
                    }
                }
                else
                {
                    if (accion.Equals("P"))
                    {
                        DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
                        try
                        {
                            db.OpenConnection();
                            List<DataTable> lstTablas = new List<DataTable>();
                            List<string> lstHojas = new List<string>();
                            if (fichasCaja == null)
                            {
                                List<Ficha> lst = await this.getFichas(db, v.IdSalon, v.Anio, v.NMes);
                                fichasCaja = lst.AsQueryable();
                            }
                            var Incluidas = fichasCaja.Where(r => r.Cerrada == true);
                            var excluidas = fichasCaja.Where(r => !r.Cerrada);
                            lstHojas.Add("Listado1-" + v.Anio + Utils.FormatoFecha(v.NMes.ToString()));
                            lstTablas.Add(this.QuitaColumnas(LinqExtensions.CopyToDataTable<Ficha>(Incluidas)));
                            lstHojas.Add("Lisatdo2-" + v.Anio + Utils.FormatoFecha(v.NMes.ToString()));
                            lstTablas.Add(this.QuitaColumnas(LinqExtensions.CopyToDataTable<Ficha>(excluidas)));
                            db.Close();
                            Export conf = new Export();
                            conf.NombreArchivo = "ListadosGestoria_" + v.Anio + Utils.FormatoFecha(v.NMes.ToString()) + ".xlsx";
                            conf.NombreHoja = "Listado1-" + v.Anio + Utils.FormatoFecha(v.NMes.ToString());
                            ExportXLS exp = new ExportXLS(conf);
                            return exp.Export(lstTablas, lstHojas);
                        }
                        catch 
                        {
                            try
                            {
                                db.Close();
                            }
                            catch { }
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.StatusMessage = "Error: " + ex.Message;
            }
            return View(v);
        }

        private async Task<DataTable> FichasGestoria(DBExtension db, int IdSalon, int Anio, int Mes)
        {
            DataTable ret = null;
            try
            {
                string sql = "select gf.FichaCierre as NFicha, f.Fecha, c.Nombre as Cliente, f.FormaPago, f.Base, f.Descuentos, f.Iva, f.Total " + 
                    "from Gestoria_Fichas gf " +
                    "inner join Fichas f on gf.IdSalon = f.IdSalon and gf.FichaCaja = f.NFicha " +
                    "inner join Clientes c on f.IdCliente = c.IdCliente and f.IdSalon = c.IdSalon " +
                    "where gf.IdSalon=@sal and gf.Anio=@anio and gf.Mes=@mes " +
                    "order by gf.FichaCierre";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@sal", IdSalon);
                sc.Parameters.AddWithValue("@anio", Anio);
                sc.Parameters.AddWithValue("@mes", Mes);
                DataSet ds = await db.GetDataSet(sc, "fichas");
                ret = ds.Tables["fichas"];
                if (ret.Rows.Count > 0)
                {
                    double dblBase = 0;
                    double dblDesc = 0;
                    double dblIva = 0;
                    double dblTotal = 0;
                    DateTime fecRef = DateTime.Parse(ret.Rows[0]["Fecha"].ToString());
                    for(int i=0; i <= ret.Rows.Count - 1; i++)
                    {
                        if(fecRef!= DateTime.Parse(ret.Rows[i]["Fecha"].ToString()))
                        {
                            DataRow fTot = ret.NewRow();
                            fTot["FormaPago"] = "Total " + fecRef.ToShortDateString() + ": ";
                            fTot["Base"] = Math.Round(dblBase, 2).ToString();
                            fTot["Descuentos"] = Math.Round(dblDesc, 2).ToString();
                            fTot["Iva"] = Math.Round(dblIva, 2).ToString();
                            fTot["Total"] = Math.Round(dblTotal, 2).ToString();
                            ret.Rows.InsertAt(fTot, i);
                            i += 1;
                            fecRef = DateTime.Parse(ret.Rows[i]["Fecha"].ToString());
                            dblBase = 0;
                            dblDesc = 0;
                            dblIva = 0;
                            dblTotal = 0;
                        }
                        dblBase += double.Parse(ret.Rows[i]["Base"].ToString());
                        dblDesc += double.Parse(ret.Rows[i]["Descuentos"].ToString());
                        dblIva += double.Parse(ret.Rows[i]["Iva"].ToString());
                        dblTotal += double.Parse(ret.Rows[i]["Total"].ToString());
                    }
                    DataRow fTotF = ret.NewRow();
                    fTotF["FormaPago"] = "Total " + fecRef.ToShortDateString() + ": ";
                    fTotF["Base"] = Math.Round(dblBase, 2).ToString();
                    fTotF["Descuentos"] = Math.Round(dblDesc, 2).ToString();
                    fTotF["Iva"] = Math.Round(dblIva, 2).ToString();
                    fTotF["Total"] = Math.Round(dblTotal, 2).ToString();
                    ret.Rows.Add(fTotF);
                    ret.AcceptChanges();
                }
            }
            catch
            {
                throw;
            }
            return ret;
        }

        private async Task<DataTable> GastosGestoria(DBExtension db, int IdSalon, int Anio, int Mes)
        {
            DataTable ret = null;
            try
            {
                string sql = "select c.Fecha, cg.Concepto, cg.Importe, cg.Iva " +
                    "from Cajas c " +
                    "inner join Cajas_Gastos cg on c.NCaja = cg.NCaja and c.IdSalon = cg.IdSalon " +
                    "where c.IdSalon=@sal and c.Anio=@anio and c.Mes=@mes " +
                    "order by c.Fecha, cg.Concepto";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@sal", IdSalon);
                sc.Parameters.AddWithValue("@anio", Anio);
                sc.Parameters.AddWithValue("@mes", Mes);
                DataSet ds = await db.GetDataSet(sc, "gastos");
                ret = ds.Tables["gastos"];
            }
            catch { throw; }
            return ret;
        }

        private async Task<List<Ficha>> getFichas(DBExtension db, int idSalon, int anio, int mes)
        {
            List<Ficha> lst = new List<Ficha>();
            try
            { 
                string sql = "select f.NFicha, f.FormaPago, f.Base, f.DescuentoImp, f.Iva, f.Total, f.Fecha " + 
                    "from Gestoria_Fichas gf " +
                    "inner join Fichas f on gf.FichaCaja = f.NFicha and gf.IdSalon = f.IdSalon " +
                    "where gf.IdSalon=@sal and gf.Anio=@anio and gf.Mes=@mes";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@sal", idSalon);
                sc.Parameters.AddWithValue("@anio", anio);
                sc.Parameters.AddWithValue("@mes", mes);
                DataSet ds = await db.GetDataSet(sc, "fichas");
                for (int i = 0; i <= ds.Tables["fichas"].Rows.Count - 1; i++)
                {
                    DataRow fila = ds.Tables["fichas"].Rows[i];
                    Ficha f = new Ficha();
                    f.Fecha = DateTime.Parse(fila["Fecha"].ToString());
                    f.NFicha = fila["NFicha"].ToString();
                    f.FormaPago = fila["FormaPago"].ToString();
                    f.Base = double.Parse(fila["Base"].ToString());
                    f.DescuentoImp = double.Parse(fila["DescuentoImp"].ToString());
                    f.Iva = double.Parse(fila["Iva"].ToString());
                    f.Total = double.Parse(fila["Total"].ToString());
                    lst.Add(f);
                }
            }
            catch { throw; }
            return lst;
        }

        public async Task<List<GestoriaProduccion>> GetProduccion_Datos(DBExtension db, string salon, string anio, string mes)
        {
            List<GestoriaProduccion> lst = new List<GestoriaProduccion>();
            try
            {
                string sql = "select gp.Codigo, gp.Codigo + '-' + us.Nombre + ' ' + us.Apellidos as Empleado, " +
                    "gp.NServiciosL, gp.ServiciosL, gp.ServComisionL, gp.NServiciosR, gp.ServiciosR, gp.ServComisionR, " +
                    "gp.NServiciosT, gp.ServiciosT, gp.ServComisionT, " +
                    "gp.NProductos, gp.Productos, gp.ProdComision, gp.TotalProduccion, gp.TotalComision " +
                    "from Gestoria_Produccion gp " +
                    "inner join AspNetUsers us on gp.IdSalon = us.Salon and gp.Codigo = us.Codigo " +
                    "where gp.IdSalon=@sal and gp.Anio=@anio and gp.Mes=@mes order by Empleado";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@sal", salon);
                sc.Parameters.AddWithValue("@anio", anio);
                sc.Parameters.AddWithValue("@mes", mes);
                DataSet ds = await db.GetDataSet(sc, "producciones");
                for (int i = 0; i <= ds.Tables["producciones"].Rows.Count - 1; i++)
                {
                    DataRow fila = ds.Tables["producciones"].Rows[i];
                    GestoriaProduccion p = new GestoriaProduccion();
                    p.Codigo = fila["Codigo"].ToString();
                    p.Empleado = fila["Empleado"].ToString();
                    p.NServiciosL = int.Parse(fila["NServiciosL"].ToString());
                    p.ProdServiciosL = double.Parse(fila["ServiciosL"].ToString());
                    p.ComisionServiciosL = double.Parse(fila["ServComisionL"].ToString());
                    p.NServiciosR = int.Parse(fila["NServiciosR"].ToString());
                    p.ProdServiciosR = double.Parse(fila["ServiciosR"].ToString());
                    p.ComisionServiciosR = double.Parse(fila["ServComisionR"].ToString());
                    p.NServiciosT = int.Parse(fila["NServiciosT"].ToString());
                    p.ProdServiciosT = double.Parse(fila["ServiciosT"].ToString());
                    p.ComisionServiciosT = double.Parse(fila["ServComisionT"].ToString());
                    p.NProductos = int.Parse(fila["NProductos"].ToString());
                    p.ProdProductos = double.Parse(fila["Productos"].ToString());
                    p.ComisionProductos = double.Parse(fila["ProdComision"].ToString());
                    p.TotalProduccion = double.Parse(fila["TotalProduccion"].ToString());
                    p.TotalComisiones = double.Parse(fila["TotalComision"].ToString());
                    lst.Add(p);
                }
            }
            catch { throw; }
            return lst;
        }

        public async Task<ActionResult> GetProduccion([FromBody]DtParameters dtParameters)
        {
            var orderCriteria = "Empleado";
            var orderAscendingDirection = true;
            if (lstProducciones == null)
            {
                string salon = dtParameters.AdditionalValues.ToList<string>()[0];
                string anio = dtParameters.AdditionalValues.ToList<string>()[1];
                string mes = dtParameters.AdditionalValues.ToList<string>()[2];
                DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
                List<GestoriaProduccion> lst = await this.GetProduccion_Datos(db, salon, anio, mes);
                lstProducciones = lst.AsQueryable();
            }
            lstProducciones = orderAscendingDirection ? lstProducciones.OrderByDynamic(orderCriteria, DtOrderDir.Asc) : lstProducciones.OrderByDynamic(orderCriteria, DtOrderDir.Desc);

            return Json(new DtResult<GestoriaProduccion>
            {
                Draw = dtParameters.Draw,
                RecordsTotal = lstProducciones.Count(),
                RecordsFiltered = lstProducciones.Count(),
                Data = lstProducciones
            });
        }

        public async Task<IActionResult> Index(ViewModelGestoria data, string accion)
        {
            try
            {
                if (string.IsNullOrEmpty(data.Mes))
                {
                    if (data.NMes > 0)
                    {
                        data.Mes = Utils.NombreMes(data.NMes);
                    }
                    else
                    {
                        var user = await _userManager.GetUserAsync(User);
                        data = new ViewModelGestoria();
                        data.Anio = System.DateTime.Now.Year;
                        data.NMes = System.DateTime.Now.Month;
                        data.Mes = Utils.NombreMes(data.NMes);
                        data.IdSalon = user.Salon;
                    }
                }
                else
                {
                    data.NMes = Utils.NumeroMes(data.Mes);
                }
                Gestoria ges = new Gestoria(new DBExtension(_dbConfig.MikaWebContextConnection));
                data = await ges.Recuperar(data);
                if (data.Producciones != null)
                {
                    this.lstProducciones = data.Producciones.AsQueryable();
                }
                if (!string.IsNullOrEmpty(accion))
                {
                    if (accion.Equals("C") || accion.Equals("CM") || accion.Equals("ROP"))
                    {
                        if (accion.Equals("CM"))
                        {
                            DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
                            SqlTransaction t = db.GetTransaction();
                            try
                            {
                                string sql = "update Fichas set Cerrada=@cerr where IdSalon=@sal and Anio=@anio and Mes=@mes";
                                SqlCommand sc = new SqlCommand(sql);
                                sc.Transaction = t;
                                sc.Parameters.AddWithValue("@cerr", true);
                                sc.Parameters.AddWithValue("@sal", data.IdSalon);
                                sc.Parameters.AddWithValue("@anio", data.Anio);
                                sc.Parameters.AddWithValue("@mes", data.NMes);
                                db.Command(sc);
                                sql = "update Cajas set Cerrada=@cerr where IdSalon=@sal and Anio=@anio and Mes=@mes";
                                sc = new SqlCommand(sql);
                                sc.Transaction = t;
                                sc.Parameters.AddWithValue("@cerr", true);
                                sc.Parameters.AddWithValue("@sal", data.IdSalon);
                                sc.Parameters.AddWithValue("@anio", data.Anio);
                                sc.Parameters.AddWithValue("@mes", data.NMes);
                                db.Command(sc);
                                sql = "update Gestoria set Cerrado=@cerr where IdSalon=@sal and Anio=@anio and Mes=@mes";
                                sc = new SqlCommand(sql);
                                sc.Transaction = t;
                                sc.Parameters.AddWithValue("@cerr", true);
                                sc.Parameters.AddWithValue("@sal", data.IdSalon);
                                sc.Parameters.AddWithValue("@anio", data.Anio);
                                sc.Parameters.AddWithValue("@mes", data.NMes);
                                db.Command(sc);
                                t.Commit();
                                db.Close();
                                data.Cerrado = true;
                            }catch 
                            {
                                try
                                {
                                    t.Rollback();
                                    db.Close();
                                }
                                catch { }
                                throw;
                            }
                        }
                        if (accion.Equals("ROP"))
                        {
                            DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
                            try
                            {
                                string sql = "update Gestoria set Cerrado=@cerr where IdSalon=@sal and Anio=@anio and Mes=@mes";
                                SqlCommand sc = new SqlCommand(sql);
                                sc.Parameters.AddWithValue("@cerr", false);
                                sc.Parameters.AddWithValue("@sal", data.IdSalon);
                                sc.Parameters.AddWithValue("@anio", data.Anio);
                                sc.Parameters.AddWithValue("@mes", data.NMes);
                                db.Command(sc);
                                data.Cerrado = false;
                            }
                            catch 
                            {
                                try
                                {
                                    db.Close();
                                }
                                catch { }
                                throw;
                            }
                        }
                        ModelState.Clear();
                    }
                    else
                    {
                        if (accion.Equals("PG"))
                        {
                            DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
                            try
                            {
                                db.OpenConnection();
                                List<DataTable> lstTablas = new List<DataTable>();
                                List<string> lstHojas = new List<string>();
                                List<ViewModelGestoria> lst = new List<ViewModelGestoria>();
                                lst.Add(data);
                                DataTable dtResumen = LinqExtensions.CopyToDataTable<ViewModelGestoria>(lst.AsQueryable());
                                dtResumen.Columns.Remove("NMes");
                                dtResumen.Columns.Remove("IdSalon");
                                dtResumen.Columns.Remove("Cerrado");
                                dtResumen.Columns.Remove("Comisiones");
                                dtResumen.Columns.Remove("SaldoNeto");
                                dtResumen.Columns.Remove("SaldoNetoCom");
                                dtResumen.Columns.Remove("Fichas");
                                dtResumen.Columns.Remove("Producciones");
                                lstTablas.Add(dtResumen);
                                lstHojas.Add("Resumen-" + data.Anio + Utils.FormatoFecha(data.NMes.ToString()));
                                lstTablas.Add(await this.FichasGestoria(db, data.IdSalon, data.Anio, data.NMes));
                                lstHojas.Add("Fichas-" + data.Anio + Utils.FormatoFecha(data.NMes.ToString()));
                                lstTablas.Add(await this.GastosGestoria(db, data.IdSalon, data.Anio, data.NMes));
                                lstHojas.Add("Gastos-" + data.Anio + Utils.FormatoFecha(data.NMes.ToString()));
                                db.Close();
                                Export conf = new Export();
                                conf.NombreArchivo = "DatosGestoria_" + data.Anio + Utils.FormatoFecha(data.NMes.ToString()) + ".xlsx";
                                conf.NombreHoja = "Resumen-" + data.Anio + Utils.FormatoFecha(data.NMes.ToString());
                                ExportXLS exp = new ExportXLS(conf);
                                return exp.Export(lstTablas, lstHojas);
                            }
                            catch 
                            {
                                try
                                {
                                    db.Close();
                                }
                                catch { }
                                throw;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.StatusMessage = "Error: " + ex.Message;
            }
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> LoadFichas([FromBody]DtParameters dtParameters)
        {
            int opcion = int.Parse(dtParameters.AdditionalValues.ToList<string>()[0]);
            if (fichasCaja == null)
            {
                DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
                try
                {
                    int salon = int.Parse(dtParameters.AdditionalValues.ToList<string>()[1]);
                    int anio = int.Parse(dtParameters.AdditionalValues.ToList<string>()[2]);
                    int mes = int.Parse(dtParameters.AdditionalValues.ToList<string>()[3]);
                    List<Ficha> lst = await this.getFichas(db, salon, anio, mes);
                    fichasCaja = lst.AsQueryable();
                }
                catch { }
            }
            var ret = fichasCaja;

            return Json(new DtResult<Ficha>
            {
                Draw = dtParameters.Draw,
                RecordsTotal = fichasCaja.Count(),
                RecordsFiltered = ret.Count(),
                Data = ret
            });
        }

        public async Task<FileStreamResult> Print(int anio, int mes, int salon)
        {
            DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
            try
            {
                db.OpenConnection();
                ViewModelGestoria vm = new ViewModelGestoria();
                vm.IdSalon = salon;
                vm.Anio = anio;
                vm.NMes = mes;
                vm.Producciones = await this.GetProduccion_Datos(db, salon.ToString(), anio.ToString(), mes.ToString());
                Gestoria g = new Gestoria(db);
                vm = await g.DatosResumen(vm, true);
                ExportPDF pdf = new ExportPDF(_env.WebRootPath, db);
                FileStreamResult ret = await pdf.ExportGestoria(vm);
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

        public async Task<FileStreamResult> PrintFacts(int anio, int mes, int salon)
        {
            DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
            try
            {
                List<Ficha> lstFichas = new List<Ficha>();
                db.OpenConnection();
                string sql = "select gf.FichaCierre as NFicha, f.Fecha, c.Nombre as Cliente, f.FormaPago, f.Base, f.Descuentos, f.Iva, f.Total, " +
                    "fl.Linea, serv.Codigo, fl.Descripcion, fl.Base as BaseLinea " +
                    "from Gestoria_Fichas gf " +
                    "inner join Fichas f on gf.IdSalon = f.IdSalon and gf.FichaCaja = f.NFicha " +
                    "inner join Fichas_Lineas fl on f.NFicha = fl.NFicha and f.IdSalon = fl.IdSalon " +
                    "inner join Servicios serv on serv.IdServicio=fl.IdServicio " +
                    "inner join Clientes c on f.IdCliente = c.IdCliente and f.IdSalon = c.IdSalon " +
                    "where gf.IdSalon=@sal and gf.Anio=@anio and gf.Mes=@mes " +
                    "order by gf.FichaCierre, fl.Linea ";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@sal", salon);
                sc.Parameters.AddWithValue("@anio", anio);
                sc.Parameters.AddWithValue("@mes", mes);
                DataSet ds = await db.GetDataSet(sc, "producciones");
                string nficha = "";
                Ficha f = null;
                for (int i = 0; i <= ds.Tables["producciones"].Rows.Count - 1; i++)
                {
                    DataRow fila = ds.Tables["producciones"].Rows[i];
                    if (!nficha.Equals(fila["NFicha"].ToString()))
                    {
                        nficha = fila["NFicha"].ToString();
                        f = new Ficha();
                        f.Anio = anio;
                        f.Mes = mes;
                        f.IdSalon = salon;
                        f.NFicha = fila["NFicha"].ToString();
                        f.Fecha = DateTime.Parse(fila["Fecha"].ToString());
                        f.Cliente = fila["Cliente"].ToString();
                        f.FormaPago = fila["FormaPago"].ToString();
                        f.Base = double.Parse(fila["Base"].ToString());
                        f.DescuentoImp = double.Parse(fila["Descuentos"].ToString());
                        f.Iva = double.Parse(fila["Iva"].ToString());
                        f.Total = double.Parse(fila["Total"].ToString());
                        f.Lineas = new List<Ficha_Linea>();
                        lstFichas.Add(f);
                    }
                    Ficha_Linea fl = new Ficha_Linea();
                    fl.Linea = int.Parse(fila["Linea"].ToString());
                    fl.CodigoServicio = fila["Codigo"].ToString();
                    fl.Descripcion = fila["Descripcion"].ToString();
                    fl.Base = double.Parse(fila["BaseLinea"].ToString());
                    f.Lineas.Add(fl);
                }
                if (lstFichas.Count > 0)
                {
                    ExportPDF pdf = new ExportPDF(_env.WebRootPath, db);
                    FileStreamResult ret = await pdf.ExportFacturas(lstFichas);
                    db.Close();
                    return ret;
                }
                else
                {
                    throw new Exception("No se han encontrado fichas");
                }
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

        public async Task<FileStreamResult> PrintDet(int anio, int mes, int salon, string codigo)
        {
            DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
            try
            {
                string tipoP = codigo.Substring(codigo.Length - 1);
                codigo = codigo.Substring(0, codigo.Length - 1);
                db.OpenConnection();
                Gestoria g = new Gestoria(db);
                GestoriaProduccion p = await g.DatosProduccion_Mes_Empleado(anio, mes, salon, codigo);
                List<Ficha> lst = null;
                if (tipoP.Equals("D"))
                {
                    lst = await g.DatosProduccion_Mes_EmpleadoFichas(anio, mes, salon, codigo);
                }
                ExportPDF pdf = new ExportPDF(_env.WebRootPath, db);
                FileStreamResult ret = pdf.ExportProduccion(anio.ToString() + Utils.FormatoFecha(mes.ToString()), p, lst);
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

        public DataTable QuitaColumnas(DataTable dt)
        {
            try
            {
                dt.Columns.Remove("Anio");
                dt.Columns.Remove("Mes");
                dt.Columns.Remove("IdSalon");
                dt.Columns.Remove("IdCliente");
                dt.Columns.Remove("Cliente");
                dt.Columns.Remove("DescuentoPorc");
                dt.Columns.Remove("DescuentoImp");
                dt.Columns.Remove("TotalDescuentos");
                dt.Columns.Remove("Pagado");
                dt.Columns.Remove("Cambio");
                dt.Columns.Remove("Cerrada");
                dt.Columns.Remove("Lineas");
            }
            catch { throw; }
            return dt;
        }


    }
}
