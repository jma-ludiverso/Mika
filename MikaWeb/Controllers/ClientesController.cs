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
    [Authorize]
    public class ClientesController : Controller
    {
        private readonly ILogger<ClientesController> _logger;
        private readonly MikaWebContext _context;
        private readonly UserManager<MikaWebUser> _userManager;
        private readonly DBConfig _dbConfig;

        public ClientesController(ILogger<ClientesController> logger, MikaWebContext context, UserManager<MikaWebUser> userManager, IOptions<DBConfig> dbConf)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _dbConfig = dbConf.Value;
        }

        public async Task<IActionResult> Edit(int Id, ViewModelCliente data)
        {
            DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
            System.Data.SqlClient.SqlTransaction t = null;
            try
            {
                if (data.Cliente == null)
                {
                    data.Cliente = new Cliente();
                    string sql = "select IdSalon, Nombre, Telefono, Email From Clientes where IdCliente=@id";
                    System.Data.SqlClient.SqlCommand sc = new System.Data.SqlClient.SqlCommand(sql);
                    sc.Parameters.AddWithValue("id", Id);
                    DataSet ds = await db.GetDataSet(sc, "datos");
                    DataRow fila = ds.Tables["datos"].Rows[0];
                    data.Cliente.IdCliente = Id;
                    data.Cliente.IdSalon = int.Parse(fila["IdSalon"].ToString());
                    data.Cliente.Nombre = fila["Nombre"].ToString();
                    data.Cliente.Telefono = fila["Telefono"].ToString();
                    data.Cliente.Email = fila["Email"].ToString();
                    ClientesExtension cliext = new ClientesExtension(db);
                    data.Historial = await cliext.getHistorial(Id);
                    return View(data);
                }
                else
                {
                    if (string.IsNullOrEmpty(data.Cliente.Telefono))
                    {
                        data.Cliente.Telefono = "";
                    }
                    if (string.IsNullOrEmpty(data.Cliente.Email))
                    {
                        data.Cliente.Email = "";
                    }
                    t = db.GetTransaction();
                    ClientesExtension cliext = new ClientesExtension(db, t);
                    data.Historial = await cliext.getHistorial(Id);
                    string sql = "Select count(1) from Clientes where IdSalon=@salon and IdCliente<>@id and nombre=@nombre";
                    System.Data.SqlClient.SqlCommand sc = new System.Data.SqlClient.SqlCommand(sql);
                    sc.Transaction = t;
                    sc.Parameters.AddWithValue("@salon", data.Cliente.IdSalon);
                    sc.Parameters.AddWithValue("id", Id);
                    sc.Parameters.AddWithValue("@nombre", data.Cliente.Nombre);
                    DataSet dsCheck = await db.GetDataSet(sc, "check");
                    if (int.Parse(dsCheck.Tables["check"].Rows[0][0].ToString()) != 0)
                    {
                        throw new Exception("Ya existe un cliente con el mismo nombre");
                    }
                    sql = "update Clientes Set Nombre=@nombre,Telefono=@telefono,Email=@email " +
                        "where IdCliente=@id";
                    sc = new System.Data.SqlClient.SqlCommand(sql);
                    sc.Transaction = t;
                    sc.Parameters.AddWithValue("@nombre", data.Cliente.Nombre);
                    sc.Parameters.AddWithValue("@telefono", data.Cliente.Telefono);
                    sc.Parameters.AddWithValue("@email", data.Cliente.Email);
                    sc.Parameters.AddWithValue("@id", Id);
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

        public IActionResult Fichas(int Id)
        {
            return View(Id);
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
                    ModelState.Clear();
                }
                else
                {
                    //borrar línea de la historia
                    cliext.BorraHistoria(data.Cliente.IdCliente, data.IdBorrar);
                }
                data.Historial = await cliext.getHistorial(data.Cliente.IdCliente);
                db.Close();
                return View("Edit", data);
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

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoadTable([FromBody]DtParameters dtParameters)
        {
            var searchBy = dtParameters.Search?.Value;
            var orderCriteria = "Nombre";
            var orderAscendingDirection = true;

            if (dtParameters.Order != null)
            {
                orderCriteria = dtParameters.Columns[dtParameters.Order[0].Column].Data;
                orderAscendingDirection = dtParameters.Order[0].Dir.ToString().ToLower() == "asc";
            }
            var result = _context.Clientes.AsQueryable();
            var user = await _userManager.GetUserAsync(User);

            if (!string.IsNullOrEmpty(searchBy))
            {
                result = result.Where(r => (r.Nombre.ToUpper().Contains(searchBy.ToUpper()) || 
                    r.Telefono.ToUpper().Contains(searchBy.ToUpper()) || 
                    r.Email.ToUpper().Contains(searchBy.ToUpper())) && r.IdSalon == user.Salon);
            }
            else
            {
                result = result.Where(r => r.IdSalon == user.Salon);
            }
            result = orderAscendingDirection ? result.OrderByDynamic(orderCriteria, DtOrderDir.Asc) : result.OrderByDynamic(orderCriteria, DtOrderDir.Desc);

            var filteredResultsCount = await result.CountAsync();
            var totalResultsCount = await _context.Clientes.CountAsync();

            return Json(new DtResult<Cliente>
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

        public async Task<IActionResult> Nuevo(ViewModelCliente data)
        {
            if (data.Cliente == null)
            {
                var user = await _userManager.GetUserAsync(User);
                data.Cliente = new Cliente();
                data.Cliente.IdSalon = user.Salon;
                return View(data);
            }
            else
            {
                DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
                System.Data.SqlClient.SqlTransaction t = null;
                try
                {
                    if (string.IsNullOrEmpty(data.Cliente.Telefono))
                    {
                        data.Cliente.Telefono = "";
                    }
                    if (string.IsNullOrEmpty(data.Cliente.Email))
                    {
                        data.Cliente.Email = "";
                    }
                    t = db.GetTransaction();
                    string sql = "Select count(1) from Clientes where IdSalon=@salon and nombre=@nombre";
                    System.Data.SqlClient.SqlCommand sc = new System.Data.SqlClient.SqlCommand(sql);
                    sc.Transaction = t;
                    sc.Parameters.AddWithValue("@salon", data.Cliente.IdSalon);
                    sc.Parameters.AddWithValue("@nombre", data.Cliente.Nombre);
                    DataSet dsCheck = await db.GetDataSet(sc, "check");
                    if (int.Parse(dsCheck.Tables["check"].Rows[0][0].ToString()) != 0)
                    {
                        throw new Exception("Ya existe un cliente con el mismo nombre");
                    }
                    sql = "select Max(IdCliente) + 1 From Clientes ";
                    sc = new System.Data.SqlClient.SqlCommand(sql);
                    sc.Transaction = t;
                    DataSet ds = await db.GetDataSet(sc, "max");
                    sql = "insert into Clientes Values(@IdCliente,@Idsalon,@nombre,@telefono,@email)";
                    sc = new System.Data.SqlClient.SqlCommand(sql);
                    sc.Transaction = t;
                    data.Cliente.IdCliente = int.Parse(ds.Tables["max"].Rows[0][0].ToString());
                    sc.Parameters.AddWithValue("@IdCliente", data.Cliente.IdCliente);
                    sc.Parameters.AddWithValue("@Idsalon", data.Cliente.IdSalon);
                    sc.Parameters.AddWithValue("@nombre", data.Cliente.Nombre);
                    sc.Parameters.AddWithValue("@telefono", data.Cliente.Telefono);
                    sc.Parameters.AddWithValue("@email", data.Cliente.Email);
                    db.Command(sc);
                    if (!string.IsNullOrEmpty(data.NuevaHistoria))
                    {
                        ClientesExtension cliext = new ClientesExtension(db, t);
                        await cliext.GuardaHistoria(data.Cliente.IdCliente, data.NuevaHistoria);
                    }
                    db.CommitTransaction(t);
                    db.Close();
                    return RedirectToAction("Index", "Clientes");
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
