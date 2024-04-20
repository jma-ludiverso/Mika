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
using MikaWeb.Extensions.DB;
using MikaWeb.Models;
using MikaWeb.Models.AuxiliaryModels;

namespace MikaWeb.Controllers
{
    [Authorize(AuthenticationSchemes = "Identity.Application", Policy = "AdminArea")]
    public class EmpleadosController : Controller
    {

        private readonly ILogger<EmpleadosController> _logger;
        private readonly MikaWebContext _context;
        private readonly DBConfig _dbConfig;
        private readonly UserManager<MikaWebUser> _userManager;

        public EmpleadosController(ILogger<EmpleadosController> logger, MikaWebContext context, IOptions<DBConfig> dbConf, UserManager<MikaWebUser> userManager)
        {
            _logger = logger;
            _context = context;
            _dbConfig = dbConf.Value;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Comisiones(string id)
        {
            Empleado_Comisiones ret = new Empleado_Comisiones();
            List<Servicio_Comision> lst = new List<Servicio_Comision>();
            DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
            try
            {
                db.OpenConnection();
                string sql = "select us.Codigo, us.Nombre, us.Apellidos, s.IdEmpresa, us.Salon, " +
                    "com.ProductosE1,com.ProductosE2,com.ProductosE3,com.ProductosE4, " +
                    "com.ProductosP1,com.ProductosP2,com.ProductosP3,com.ProductosP4, " +
                    "com.ServiciosLE1,com.ServiciosLE2,com.ServiciosLE3,com.ServiciosLE4, " +
                    "com.ServiciosLP1,com.ServiciosLP2,com.ServiciosLP3,com.ServiciosLP4, " +
                    "com.ServiciosSE1,com.ServiciosSE2,com.ServiciosSE3,com.ServiciosSE4, " +
                    "com.ServiciosSP1,com.ServiciosSP2,com.ServiciosSP3,com.ServiciosSP4, " +
                    "com.ServiciosTE1,com.ServiciosTE2,com.ServiciosTE3,com.ServiciosTE4, " +
                    "com.ServiciosTP1,com.ServiciosTP2,com.ServiciosTP3,com.ServiciosTP4 " +
                    "from AspNetUsers us " +
                    "inner join EmpleadosComisiones com on us.Codigo=com.Codigo and us.Salon=com.Salon " +
                    "inner join Salones s on us.Salon=s.IdSalon " +
                    "where us.Id=@id";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@id", id);
                DataSet dsDef = await db.GetDataSet(sc, "defecto");
                DataRow fila = dsDef.Tables["defecto"].Rows[0];
                ret.Codigo = fila["Codigo"].ToString();
                ret.IdSalon = int.Parse(fila["Salon"].ToString());
                ret.IdEmpresa = int.Parse(fila["IdEmpresa"].ToString());
                ret.ProductoE1 = double.Parse(fila["ProductosE1"].ToString());
                ret.ProductoE2 = double.Parse(fila["ProductosE2"].ToString());
                ret.ProductoE3 = double.Parse(fila["ProductosE3"].ToString());
                ret.ProductoE4 = double.Parse(fila["ProductosE4"].ToString());
                ret.ProductoP1 = double.Parse(fila["ProductosP1"].ToString());
                ret.ProductoP2 = double.Parse(fila["ProductosP2"].ToString());
                ret.ProductoP3 = double.Parse(fila["ProductosP3"].ToString());
                ret.ProductoP4 = double.Parse(fila["ProductosP4"].ToString());
                ret.ServicioLE1 = double.Parse(fila["ServiciosLE1"].ToString());
                ret.ServicioLE2 = double.Parse(fila["ServiciosLE2"].ToString());
                ret.ServicioLE3 = double.Parse(fila["ServiciosLE3"].ToString());
                ret.ServicioLE4 = double.Parse(fila["ServiciosLE4"].ToString());
                ret.ServicioLP1 = double.Parse(fila["ServiciosLP1"].ToString());
                ret.ServicioLP2 = double.Parse(fila["ServiciosLP2"].ToString());
                ret.ServicioLP3 = double.Parse(fila["ServiciosLP3"].ToString());
                ret.ServicioLP4 = double.Parse(fila["ServiciosLP4"].ToString());
                ret.ServicioSE1 = double.Parse(fila["ServiciosSE1"].ToString());
                ret.ServicioSE2 = double.Parse(fila["ServiciosSE2"].ToString());
                ret.ServicioSE3 = double.Parse(fila["ServiciosSE3"].ToString());
                ret.ServicioSE4 = double.Parse(fila["ServiciosSE4"].ToString());
                ret.ServicioSP1 = double.Parse(fila["ServiciosSP1"].ToString());
                ret.ServicioSP2 = double.Parse(fila["ServiciosSP2"].ToString());
                ret.ServicioSP3 = double.Parse(fila["ServiciosSP3"].ToString());
                ret.ServicioSP4 = double.Parse(fila["ServiciosSP4"].ToString());
                ret.ServicioTE1 = double.Parse(fila["ServiciosTE1"].ToString());
                ret.ServicioTE2 = double.Parse(fila["ServiciosTE2"].ToString());
                ret.ServicioTE3 = double.Parse(fila["ServiciosTE3"].ToString());
                ret.ServicioTE4 = double.Parse(fila["ServiciosTE4"].ToString());
                ret.ServicioTP1 = double.Parse(fila["ServiciosTP1"].ToString());
                ret.ServicioTP2 = double.Parse(fila["ServiciosTP2"].ToString());
                ret.ServicioTP3 = double.Parse(fila["ServiciosTP3"].ToString());
                ret.ServicioTP4 = double.Parse(fila["ServiciosTP4"].ToString());
                ret.Empleado = dsDef.Tables["defecto"].Rows[0]["Nombre"].ToString() + " " + dsDef.Tables["defecto"].Rows[0]["Apellidos"].ToString();
                ret.Comisiones = await this.Comision_Servicios(db, ret.Codigo, ret.IdEmpresa);
                db.Close();
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
            return View(ret);
        }

        [HttpPost]
        public async Task<IActionResult> Comisiones(Empleado_Comisiones data, string acc)
        {
            DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
            try
            {
                db.OpenConnection();
                string sql = "update EmpleadosComisiones Set ProductosE1=@p1,ProductosE2=@p2,ProductosE3=@p3,ProductosE4=@p4, " +
                    "ProductosP1=@p5,ProductosP2=@p6,ProductosP3=@p7,ProductosP4=@p8, " + 
                    "ServiciosLE1=@p9,ServiciosLE2=@p10,ServiciosLE3=@p11,ServiciosLE4=@p12, " +
                    "ServiciosLP1=@p13,ServiciosLP2=@p14,ServiciosLP3=@p15,ServiciosLP4=@p16, " + 
                    "ServiciosSE1=@p17,ServiciosSE2=@p18,ServiciosSE3=@p19,ServiciosSE4=@p20, " +
                    "ServiciosSP1=@p21,ServiciosSP2=@p22,ServiciosSP3=@p23,ServiciosSP4=@p24, " +
                    "ServiciosTE1=@p25,ServiciosTE2=@p26,ServiciosTE3=@p27,ServiciosTE4=@p28, " +
                    "ServiciosTP1=@p29,ServiciosTP2=@p30,ServiciosTP3=@p31,ServiciosTP4=@p32 " +
                    "where Codigo=@cod and Salon=@sal";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@p1", data.ProductoE1);
                sc.Parameters.AddWithValue("@p2", data.ProductoE2);
                sc.Parameters.AddWithValue("@p3", data.ProductoE3);
                sc.Parameters.AddWithValue("@p4", data.ProductoE4);
                sc.Parameters.AddWithValue("@p5", data.ProductoP1);
                sc.Parameters.AddWithValue("@p6", data.ProductoP2);
                sc.Parameters.AddWithValue("@p7", data.ProductoP3);
                sc.Parameters.AddWithValue("@p8", data.ProductoP4);
                sc.Parameters.AddWithValue("@p9", data.ServicioLE1);
                sc.Parameters.AddWithValue("@p10", data.ServicioLE2);
                sc.Parameters.AddWithValue("@p11", data.ServicioLE3);
                sc.Parameters.AddWithValue("@p12", data.ServicioLE4);
                sc.Parameters.AddWithValue("@p13", data.ServicioLP1);
                sc.Parameters.AddWithValue("@p14", data.ServicioLP2);
                sc.Parameters.AddWithValue("@p15", data.ServicioLP3);
                sc.Parameters.AddWithValue("@p16", data.ServicioLP4);
                sc.Parameters.AddWithValue("@p17", data.ServicioSE1);
                sc.Parameters.AddWithValue("@p18", data.ServicioSE2);
                sc.Parameters.AddWithValue("@p19", data.ServicioSE3);
                sc.Parameters.AddWithValue("@p20", data.ServicioSE4);
                sc.Parameters.AddWithValue("@p21", data.ServicioSP1);
                sc.Parameters.AddWithValue("@p22", data.ServicioSP2);
                sc.Parameters.AddWithValue("@p23", data.ServicioSP3);
                sc.Parameters.AddWithValue("@p24", data.ServicioSP4);
                sc.Parameters.AddWithValue("@p25", data.ServicioTE1);
                sc.Parameters.AddWithValue("@p26", data.ServicioTE2);
                sc.Parameters.AddWithValue("@p27", data.ServicioTE3);
                sc.Parameters.AddWithValue("@p28", data.ServicioTE4);
                sc.Parameters.AddWithValue("@p29", data.ServicioTP1);
                sc.Parameters.AddWithValue("@p30", data.ServicioTP2);
                sc.Parameters.AddWithValue("@p31", data.ServicioTP3);
                sc.Parameters.AddWithValue("@p32", data.ServicioTP4);
                sc.Parameters.AddWithValue("@cod", data.Codigo);
                sc.Parameters.AddWithValue("@sal", data.IdSalon);
                db.Command(sc);
                if (!string.IsNullOrEmpty(acc))
                {
                    if (acc.Equals("A"))
                    {
                        sql = "update EmpleadosServicios set ComisionP1=@com1,ComisionP2=@com2,ComisionP3=@com3,ComisionP4=@com4 " +
                            "Where Codigo=@cod and IdServicio in (Select IdServicio From Servicios Where IdEmpresa=@emp and Tipo=@tipo and Grupo=@grupo)";
                        sc = new SqlCommand(sql);
                        sc.Parameters.AddWithValue("@com1", data.ServicioLP1);
                        sc.Parameters.AddWithValue("@com2", data.ServicioLP2);
                        sc.Parameters.AddWithValue("@com3", data.ServicioLP3);
                        sc.Parameters.AddWithValue("@com4", data.ServicioLP4);
                        sc.Parameters.AddWithValue("@cod", data.Codigo);
                        sc.Parameters.AddWithValue("@emp", data.IdEmpresa);
                        sc.Parameters.AddWithValue("@tipo", "Servicio");
                        sc.Parameters.AddWithValue("@grupo", "Lavado");
                        db.Command(sc);
                        sql = "update EmpleadosServicios set ComisionP1=@com1,ComisionP2=@com2,ComisionP3=@com3,ComisionP4=@com4 " +
                            "Where Codigo=@cod and IdServicio in (Select IdServicio From Servicios Where IdEmpresa=@emp and Tipo=@tipo and Grupo=@grupo)";
                        sc = new SqlCommand(sql);
                        sc.Parameters.AddWithValue("@com1", data.ServicioSP1);
                        sc.Parameters.AddWithValue("@com2", data.ServicioSP2);
                        sc.Parameters.AddWithValue("@com3", data.ServicioSP3);
                        sc.Parameters.AddWithValue("@com4", data.ServicioSP4);
                        sc.Parameters.AddWithValue("@cod", data.Codigo);
                        sc.Parameters.AddWithValue("@emp", data.IdEmpresa);
                        sc.Parameters.AddWithValue("@tipo", "Servicio");
                        sc.Parameters.AddWithValue("@grupo", "Servicios");
                        db.Command(sc);
                        sql = "update EmpleadosServicios set ComisionP1=@com1,ComisionP2=@com2,ComisionP3=@com3,ComisionP4=@com4 " +
                        "Where Codigo=@cod and IdServicio in (Select IdServicio From Servicios Where IdEmpresa=@emp and Tipo=@tipo and Grupo=@grupo)";
                        sc = new SqlCommand(sql);
                        sc.Parameters.AddWithValue("@com1", data.ServicioTP1);
                        sc.Parameters.AddWithValue("@com2", data.ServicioTP2);
                        sc.Parameters.AddWithValue("@com3", data.ServicioTP3);
                        sc.Parameters.AddWithValue("@com4", data.ServicioTP4);
                        sc.Parameters.AddWithValue("@cod", data.Codigo);
                        sc.Parameters.AddWithValue("@emp", data.IdEmpresa);
                        sc.Parameters.AddWithValue("@tipo", "Servicio");
                        sc.Parameters.AddWithValue("@grupo", "Tecnicos");
                        db.Command(sc);
                        sql = "update EmpleadosServicios set ComisionP1=@com1,ComisionP2=@com2,ComisionP3=@com3,ComisionP4=@com4 " +
                            "Where Codigo=@cod and IdServicio in (Select IdServicio From Servicios Where IdEmpresa=@emp and Tipo=@tipo)";
                        sc = new SqlCommand(sql);
                        sc.Parameters.AddWithValue("@com1", data.ProductoP1);
                        sc.Parameters.AddWithValue("@com2", data.ProductoP2);
                        sc.Parameters.AddWithValue("@com3", data.ProductoP3);
                        sc.Parameters.AddWithValue("@com4", data.ProductoP4);
                        sc.Parameters.AddWithValue("@cod", data.Codigo);
                        sc.Parameters.AddWithValue("@emp", data.IdEmpresa);
                        sc.Parameters.AddWithValue("@tipo", "Producto");
                        db.Command(sc);
                        data.Comisiones = await this.Comision_Servicios(db, data.Codigo, data.IdEmpresa);
                        ModelState.Clear();
                    }
                }
                else
                {
                    foreach (Servicio_Comision com in data.Comisiones)
                    {
                        sql = "update EmpleadosServicios set ComisionP1=@com1,ComisionP2=@com2,ComisionP3=@com3,ComisionP4=@com4 " +
                            "Where Codigo=@cod and IdServicio=@serv";
                        sc = new SqlCommand(sql);
                        sc.Parameters.AddWithValue("@com1", com.ComisionP1);
                        sc.Parameters.AddWithValue("@com2", com.ComisionP2);
                        sc.Parameters.AddWithValue("@com3", com.ComisionP3);
                        sc.Parameters.AddWithValue("@com4", com.ComisionP4);
                        sc.Parameters.AddWithValue("@cod", data.Codigo);
                        sc.Parameters.AddWithValue("@serv", com.IdServicio);
                        db.Command(sc);
                    }
                }
                db.Close();
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
            return View(data);
        }

        private async Task<List<Servicio_Comision>> Comision_Servicios(DBExtension db, string Codigo, int IdEmpresa)
        {
            List<Servicio_Comision> lst = new List<Servicio_Comision>();
            try
            {
                string sql = "select s.IdServicio, s.Tipo, s.Nombre, ISNULL(es.ComisionP1, 0) as ComisionP1," +
                    "ISNULL(es.ComisionP2, 0) as ComisionP2, ISNULL(es.ComisionP3, 0) as ComisionP3, ISNULL(es.ComisionP4, 0) as ComisionP4 " +
                    "from Servicios s " +
                    "left join EmpleadosServicios es on s.IdServicio = es.IdServicio and es.Codigo=@cod " +
                    "where s.IdEmpresa=@emp and s.Activo='True' " +
                    "order by s.tipo desc, s.Nombre";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@cod", Codigo);
                sc.Parameters.AddWithValue("@emp", IdEmpresa);
                DataSet ds = await db.GetDataSet(sc, "comisiones");
                for (int i = 0; i <= ds.Tables["comisiones"].Rows.Count - 1; i++)
                {
                    DataRow fila = ds.Tables["comisiones"].Rows[i];
                    Servicio_Comision com = new Servicio_Comision();
                    com.IdServicio = int.Parse(fila["IdServicio"].ToString());
                    com.Tipo = fila["Tipo"].ToString();
                    com.Descripcion = fila["Nombre"].ToString();
                    com.ComisionP1 = double.Parse(fila["ComisionP1"].ToString());
                    com.ComisionP2 = double.Parse(fila["ComisionP2"].ToString());
                    com.ComisionP3 = double.Parse(fila["ComisionP3"].ToString());
                    com.ComisionP4 = double.Parse(fila["ComisionP4"].ToString());
                    lst.Add(com);
                }
            }
            catch
            {
                throw;
            }
            return lst;
        }

        [HttpPost]
        public async Task<IActionResult> LoadTable([FromBody]DtParameters dtParameters)
        {
            var searchBy = dtParameters.Search?.Value;
            var orderCriteria = "Nombre, Apellidos";
            var orderAscendingDirection = true;

            if (dtParameters.Order != null)
            {
                orderCriteria = dtParameters.Columns[dtParameters.Order[0].Column].Data;
                orderAscendingDirection = dtParameters.Order[0].Dir.ToString().ToLower() == "asc";
            }

            var result = _context.vEmpleados.AsQueryable();
            var user = await _userManager.GetUserAsync(User);

            if (!string.IsNullOrEmpty(searchBy))
            {
                bool flag;
                if (bool.TryParse(searchBy, out flag))
                {
                    result = result.Where(r => (r.Activo == flag || r.Administrador == flag) && r.Salon==user.Salon);
                }
                else
                {
                    result = result.Where(r => (r.Codigo != null && r.Codigo.ToUpper().Contains(searchBy.ToUpper()) ||
                           r.Nombre != null && r.Nombre.ToUpper().Contains(searchBy.ToUpper()) ||
                           r.Apellidos != null && r.Apellidos.ToUpper().Contains(searchBy.ToUpper()) ||
                           r.Email != null && r.Email.ToUpper().Contains(searchBy.ToUpper())) && r.Salon==user.Salon);
                }
            }
            else
            {
                result = result.Where(r => r.Salon == user.Salon);
            }

            result = orderAscendingDirection ? result.OrderByDynamic(orderCriteria, DtOrderDir.Asc) : result.OrderByDynamic(orderCriteria, DtOrderDir.Desc);

            var filteredResultsCount = await result.CountAsync();
            var totalResultsCount = await _context.vEmpleados.CountAsync();

            return Json(new DtResult<Empleado>
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

    }
}
