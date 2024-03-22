using System;
using System.Data;
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
    [Authorize(Policy = "AdminArea")]
    public class ServiciosController : Controller
    {

        private readonly ILogger<ServiciosController> _logger;
        private readonly MikaWebContext _context;
        private readonly UserManager<MikaWebUser> _userManager;
        private readonly DBConfig _dbConfig;
        private readonly MikaConf _mkconf;

        public ServiciosController(ILogger<ServiciosController> logger, MikaWebContext context, UserManager<MikaWebUser> userManager, IOptions<DBConfig> dbConf, IOptions<MikaConf> mkConf)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _dbConfig = dbConf.Value;
            _mkconf = mkConf.Value;
        }

        public async Task<IActionResult> Edit(int Id, Servicio data)
        {
            DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
            System.Data.SqlClient.SqlTransaction t = null;
            try
            {
                if (data.IdEmpresa == 0)
                {
                    string sql = "select IdEmpresa, Codigo, Tipo, Grupo, Nombre, Precio, IvaPorc, IvaCant, PVP, Activo From Servicios where IdServicio=@id";
                    System.Data.SqlClient.SqlCommand sc = new System.Data.SqlClient.SqlCommand(sql);
                    sc.Parameters.AddWithValue("id", Id);
                    DataSet ds = await db.GetDataSet(sc, "datos");
                    DataRow fila = ds.Tables["datos"].Rows[0];
                    data.IdEmpresa = int.Parse(fila["IdEmpresa"].ToString());
                    data.Codigo = fila["Codigo"].ToString();
                    data.Tipo = fila["Tipo"].ToString();
                    data.Grupo = fila["Grupo"].ToString();
                    data.Nombre = fila["Nombre"].ToString();
                    data.Precio = double.Parse(fila["Precio"].ToString());
                    data.IvaPorc = double.Parse(fila["IvaPorc"].ToString());
                    data.IvaCant = double.Parse(fila["IvaCant"].ToString());
                    data.PVP = double.Parse(fila["PVP"].ToString());
                    data.Activo = bool.Parse(fila["Activo"].ToString());
                    return View(data);
                }
                else
                {
                    if (data.Codigo.Length == 1)
                    {
                        data.Codigo = "0" + data.Codigo;
                    }
                    t = db.GetTransaction();
                    string sql = "Select count(1) from Servicios where IdEmpresa=@empresa and IdServicio<>@id and (nombre=@nombre or codigo=@codigo)";
                    System.Data.SqlClient.SqlCommand sc = new System.Data.SqlClient.SqlCommand(sql);
                    sc.Transaction = t;
                    sc.Parameters.AddWithValue("@empresa", data.IdEmpresa);
                    sc.Parameters.AddWithValue("id", Id);
                    sc.Parameters.AddWithValue("@nombre", data.Nombre);
                    sc.Parameters.AddWithValue("@codigo", data.Codigo);
                    DataSet dsCheck = await db.GetDataSet(sc, "check");
                    if (int.Parse(dsCheck.Tables["check"].Rows[0][0].ToString()) != 0)
                    {
                        throw new Exception("Ya existe un servicio con el mismo nombre o código");
                    }
                    sql = "update Servicios Set Codigo=@codigo,Tipo='Servicio',Grupo=@grupo,Nombre=@nombre,Precio=@precio," +
                        "IvaPorc=@iva,IvaCant=@ivacant,PVP=@pvp,Activo=@activo " +
                        "where IdServicio=@idserv";
                    sc = new System.Data.SqlClient.SqlCommand(sql);
                    sc.Transaction = t;
                    sc.Parameters.AddWithValue("@codigo", data.Codigo);
                    sc.Parameters.AddWithValue("@grupo", data.Grupo);
                    sc.Parameters.AddWithValue("@nombre", data.Nombre);
                    sc.Parameters.AddWithValue("@precio", Math.Round(data.Precio, 3));
                    sc.Parameters.AddWithValue("@iva", data.IvaPorc);
                    sc.Parameters.AddWithValue("@ivacant", Math.Round(data.IvaCant, 3));
                    sc.Parameters.AddWithValue("@pvp", Math.Round(data.PVP, 2));
                    sc.Parameters.AddWithValue("@activo", data.Activo);
                    sc.Parameters.AddWithValue("@idserv", Id);
                    db.Command(sc);
                    db.CommitTransaction(t);
                    db.Close();
                    ViewBag.StatusMessage = "Datos guardados";
                    return View(data);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (t != null)
                    {
                        t.Rollback();
                    }
                    db.Close();
                }
                catch { };
                ViewBag.StatusMessage = "Error: " + ex.Message;
                return View(data);
            }
        }

        public IActionResult Index(ViewModelServicios data)
        {
            if (!data.IsPostBack)
            {
                data.SoloActivos = true;
                data.IsPostBack = true;
            }
            ViewBag.SoloActivos = data.SoloActivos;
            return View(data);
        }

        private async Task<int> getIdEmpresaAsync(int idSalon)
        {
            int ret = 0;
            try
            { 
                DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
                string sql = "select sal.IdEmpresa from Salones sal where sal.IdSalon=@id";
                System.Data.SqlClient.SqlCommand sc = new System.Data.SqlClient.SqlCommand(sql);
                sc.Parameters.AddWithValue("@id", idSalon);
                DataSet ds = await db.GetDataSet(sc, "emp");
                ret = int.Parse(ds.Tables["emp"].Rows[0]["IdEmpresa"].ToString());
            }
            catch { throw; }
            return ret;
        }

        [HttpPost]
        public async Task<IActionResult> LoadTable([FromBody]DtParameters dtParameters)
        {
            var searchBy = dtParameters.Search?.Value;

            var orderCriteria = "Tipo, Nombre";
            var orderAscendingDirection = true;

            if (dtParameters.Order != null)
            {
                orderCriteria = dtParameters.Columns[dtParameters.Order[0].Column].Data;
                orderAscendingDirection = dtParameters.Order[0].Dir.ToString().ToLower() == "asc";
            }
            bool soloActivos = false;
            if (!string.IsNullOrEmpty(dtParameters.AdditionalValues.ToList<string>()[0]))
            {
                soloActivos = bool.Parse(dtParameters.AdditionalValues.ToList<string>()[0]);
            }
            var result = _context.Servicios.AsQueryable();
            var user = await _userManager.GetUserAsync(User);
            int empresa = await this.getIdEmpresaAsync(user.Salon);

            if (!string.IsNullOrEmpty(searchBy))
            {
                bool flag;
                if (bool.TryParse(searchBy, out flag))
                {
                    result = result.Where(r => (r.Activo == flag) && r.IdEmpresa == empresa);
                }
                else
                {
                    double dblflag;
                    if(double.TryParse(searchBy, out dblflag))
                    {
                        if (soloActivos)
                        {
                            result = result.Where(r => (r.Precio >= dblflag ||
                                r.PVP >= dblflag) && r.IdEmpresa == empresa && r.Activo == soloActivos);
                        }
                        else
                        {
                            result = result.Where(r => (r.Precio >= dblflag ||
                                r.PVP >= dblflag) && r.IdEmpresa == empresa);
                        }
                    }
                    else
                    {
                        if (soloActivos)
                        {
                            result = result.Where(r => (r.Codigo != null && r.Codigo.ToUpper().Contains(searchBy.ToUpper()) ||
                               r.Tipo != null && r.Tipo.ToUpper().Contains(searchBy.ToUpper()) ||
                               r.Nombre != null && r.Nombre.ToUpper().Contains(searchBy.ToUpper())) && r.IdEmpresa == empresa && r.Activo == soloActivos);
                        }
                        else
                        {
                            result = result.Where(r => (r.Codigo != null && r.Codigo.ToUpper().Contains(searchBy.ToUpper()) ||
                               r.Tipo != null && r.Tipo.ToUpper().Contains(searchBy.ToUpper()) ||
                               r.Nombre != null && r.Nombre.ToUpper().Contains(searchBy.ToUpper())) && r.IdEmpresa == empresa);
                        }
                    }
                }
            }
            else
            {
                if (soloActivos)
                {
                    result = result.Where(r => r.IdEmpresa == empresa && r.Activo == soloActivos);
                }
                else
                {
                    result = result.Where(r => r.IdEmpresa == empresa);
                }
            }
            result = orderAscendingDirection ? result.OrderByDynamic(orderCriteria, DtOrderDir.Asc) : result.OrderByDynamic(orderCriteria, DtOrderDir.Desc);

            var filteredResultsCount = await result.CountAsync();
            var totalResultsCount = await _context.Servicios.CountAsync();

            return Json(new DtResult<Servicio>
            {
                Draw = dtParameters.Draw,
                RecordsTotal = totalResultsCount,
                RecordsFiltered = filteredResultsCount,
                Data = await result
                    .Skip(dtParameters.Start)
                    .Take(dtParameters.Length)
                    .ToListAsync()
            });

        }

        public async Task<IActionResult> Nuevo(Servicio data)
        {
            if (data.IdEmpresa == 0)
            {
                var user = await _userManager.GetUserAsync(User);
                data.IdEmpresa = await this.getIdEmpresaAsync(user.Salon);
                data.Precio = 0;
                data.IvaPorc = _mkconf.Iva;
                data.IvaCant = 0;
                return View(data);
            }
            else
            {
                DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
                System.Data.SqlClient.SqlTransaction t = null;
                try
                {
                    if (data.Codigo.Length == 1)
                    {
                        data.Codigo = "0" + data.Codigo;
                    }
                    t = db.GetTransaction();
                    string sql = "Select count(1) from Servicios where IdEmpresa=@empresa and (nombre=@nombre or codigo=@codigo)";
                    System.Data.SqlClient.SqlCommand sc = new System.Data.SqlClient.SqlCommand(sql);
                    sc.Transaction = t;
                    sc.Parameters.AddWithValue("@empresa", data.IdEmpresa);
                    sc.Parameters.AddWithValue("@nombre", data.Nombre);
                    sc.Parameters.AddWithValue("@codigo", data.Codigo);
                    DataSet dsCheck = await db.GetDataSet(sc, "check");
                    if (int.Parse(dsCheck.Tables["check"].Rows[0][0].ToString()) != 0)
                    {
                        throw new Exception("Ya existe un servicio con el mismo nombre o código");
                    }
                    sql = "select Max(IdServicio) + 1 From Servicios ";
                    sc = new System.Data.SqlClient.SqlCommand(sql);
                    sc.Transaction = t;
                    DataSet ds = await db.GetDataSet(sc, "max");
                    sql = "insert into Servicios values(@idserv,@empresa,@codigo,'Servicio',@grupo,@nombre,@precio,@iva,@ivacant,@pvp,'True')";
                    sc = new System.Data.SqlClient.SqlCommand(sql);
                    sc.Transaction = t;
                    sc.Parameters.AddWithValue("@idserv", ds.Tables["max"].Rows[0][0]);
                    sc.Parameters.AddWithValue("@empresa", data.IdEmpresa);
                    sc.Parameters.AddWithValue("@codigo", data.Codigo);
                    sc.Parameters.AddWithValue("@grupo", data.Grupo);
                    sc.Parameters.AddWithValue("@nombre", data.Nombre);
                    sc.Parameters.AddWithValue("@precio", Math.Round(data.Precio,3));
                    sc.Parameters.AddWithValue("@iva", data.IvaPorc);
                    sc.Parameters.AddWithValue("@ivacant", Math.Round(data.IvaCant,3));
                    sc.Parameters.AddWithValue("@pvp", Math.Round(data.PVP,2));
                    db.Command(sc);
                    db.CommitTransaction(t);
                    db.Close();
                    return RedirectToAction("Index", "Servicios");
                }
                catch (Exception ex)
                {
                    try
                    {
                        if (t != null)
                        {
                            t.Rollback();
                        }
                        db.Close();
                    }
                    catch { };
                    ViewBag.StatusMessage = "Error: " + ex.Message;
                    return View(data);
                }
            }
        }
    }
}
