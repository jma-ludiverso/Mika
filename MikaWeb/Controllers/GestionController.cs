using Google.DataTable.Net.Wrapper.Extension;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MikaWeb.Areas.Identity.Data;
using MikaWeb.Extensions.DB;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace MikaWeb.Controllers
{
    [Authorize(AuthenticationSchemes = "Identity.Application", Policy = "AdminArea")]
    public class GestionController : Controller
    {

        private readonly ILogger<GestionController> _logger;
        private readonly DBConfig _dbConfig;
        private readonly UserManager<MikaWebUser> _userManager;

        public GestionController(ILogger<GestionController> logger, IOptions<DBConfig> dbConf, UserManager<MikaWebUser> userManager)
        {
            _logger = logger;
            _dbConfig = dbConf.Value;
            _userManager = userManager;
        }

        public async Task<ActionResult> getChartAnual()
        {
            string json = "";
            try
            {
                DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
                var user = await _userManager.GetUserAsync(User);
                int anio = System.DateTime.Now.Year;
                string sql = "with SelBase as ( " +
                    "select am.Mes, Isnull(sum(f.Base - f.Descuentos), 0) as ProdAnt " +
                    "from dbo.MK_AniosMeses(" + user.Salon.ToString() + ", " + (anio-1).ToString() + ") am " +
                    "left join Fichas f on am.Mes = f.Mes and am.anio = f.Anio and am.IdSalon = f.IdSalon " +
                    "group by am.Mes " +
                ") " +
                "select am.Mes, (select ProdAnt from SelBase s where s.Mes = am.Mes) as [Producción " + (anio - 1).ToString() + "], " +
                "Isnull(sum(f.Base - f.Descuentos), 0) as [Producción " + anio.ToString() + "] " +
                "from dbo.MK_AniosMeses(" + user.Salon.ToString() + ", " + anio.ToString() + ") am " +
                "left join Fichas f on am.Mes=f.Mes and am.anio=f.Anio and am.IdSalon=f.IdSalon " +
                "group by am.Mes order by am.Mes";
                DataSet ds = await db.GetDataSet(sql, "anios");
                json = ds.Tables["anios"].ToGoogleDataTable().GetJson();
            }
            catch { }
            return Content(json);
        }

        public async Task<ActionResult> getChartProducciones()
        {
            string json = "";
            try
            {
                DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
                var user = await _userManager.GetUserAsync(User);
                string sql = "select  us.Nombre + ' ' + us.Apellidos as [Name], sum(fl.Base - fl.DescuentoCant) as [Count] " +
                    "from Fichas_Lineas fl " +
                    "inner join Fichas f on fl.NFicha = f.NFicha and fl.IdSalon = f.IdSalon " +
                    "inner join AspNetUsers us on fl.Codigo = us.Codigo and fl.IdSalon = us.Salon " +
                    "where f.IdSalon=@sal and f.Anio=@anio and f.Mes=@mes " +
                    "group by us.Nombre + ' ' + us.Apellidos";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@sal", user.Salon);
                sc.Parameters.AddWithValue("@anio", System.DateTime.Now.Year);
                sc.Parameters.AddWithValue("@mes", System.DateTime.Now.Month);
                DataSet ds = await db.GetDataSet(sc, "prod");
                json = ds.Tables["prod"].ToGoogleDataTable().GetJson();
            }
            catch { }
            return Content(json);
        }

        public async Task<ActionResult> getChartServicios(string tipo)
        {
            string json = "";
            try
            {
                DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
                var user = await _userManager.GetUserAsync(User);
                string sql = "";
                SqlCommand sc = new SqlCommand();
                sc.Parameters.AddWithValue("@sal", user.Salon);
                sc.Parameters.AddWithValue("@anio", System.DateTime.Now.Year);
                sc.Parameters.AddWithValue("@mes", System.DateTime.Now.Month);
                if (string.IsNullOrEmpty(tipo))
                {
                    sql = "select isnull(s.Grupo,s.Tipo) as [Tipo], sum(fl.Base - fl.DescuentoCant) as [Ventas (€)], count(1) as [Numero Servicios] " +
                        "from Fichas_Lineas fl " +
                        "inner join Fichas f on fl.NFicha = f.NFicha and fl.IdSalon = f.IdSalon " +
                        "inner join Salones sal on sal.IdSalon = f.IdSalon " +
                        "inner join Servicios s on s.IdEmpresa = sal.IdEmpresa and s.IdServicio = fl.IdServicio " +
                        "where f.IdSalon=@sal and f.Anio=@anio and f.Mes=@mes " +
                        "group by s.Grupo, s.Tipo";
                }
                else
                {
                    sql = "select s.Nombre, sum(fl.Base - fl.DescuentoCant) as Ventas, count(1) as Numero " +
                        "from Fichas_Lineas fl " +
                        "inner join Fichas f on fl.NFicha = f.NFicha and fl.IdSalon = f.IdSalon " +
                        "inner join Salones sal on sal.IdSalon = f.IdSalon " +
                        "inner join Servicios s on s.IdEmpresa = sal.IdEmpresa and s.IdServicio = fl.IdServicio " +
                        "where f.IdSalon=@sal and f.Anio=@anio and f.Mes=@mes and isnull(s.Grupo,s.Tipo)=@tipo " +
                        "group by s.Nombre ";
                    sc.Parameters.AddWithValue("@tipo", tipo);
                }
                sc.CommandText = sql;
                DataSet ds = await db.GetDataSet(sc, "serv");
                json = ds.Tables["serv"].ToGoogleDataTable().GetJson();
            }
            catch { }
            return Content(json);
        }

        public IActionResult Index()
        {
            return View();
        }

    }
}
