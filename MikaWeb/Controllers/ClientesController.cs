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
using NPOI.SS.Formula.Functions;

namespace MikaWeb.Controllers
{
    [Authorize(AuthenticationSchemes = "Identity.Application")]
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
            try
            {
                if (data.Cliente == null)
                {
                    ClientesExtension cliext = new ClientesExtension(db);
                    data.Cliente = await cliext.GetClientData(Id);
                    data.Historial = data.Cliente.Historial;
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
                    ClientesExtension cliext = new ClientesExtension(db);
                    bool result = await cliext.SaveClient(data.Cliente);
                    data.Historial = await cliext.GetRecordData(Id);
                    ViewBag.StatusMessage = "Datos guardados";
                    return View(data);
                }
            }
            catch (Exception ex)
            {
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
                    Cliente_Historia ch = await cliext.SaveRecordData(data.Cliente.IdCliente, data.NuevaHistoria);
                    data.NuevaHistoria = "";
                    ModelState.Clear();
                }
                else
                {
                    //borrar línea de la historia
                    cliext.DeleteRecordData(data.Cliente.IdCliente, data.IdBorrar);
                }
                data.Historial = await cliext.GetRecordData(data.Cliente.IdCliente);
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
                    data.Cliente.IdCliente = -1;
                    ClientesExtension cliext = new ClientesExtension(new DBExtension(_dbConfig.MikaWebContextConnection));
                    bool resul = await cliext.SaveClient(data.Cliente);
                    return RedirectToAction("Index", "Clientes");
                }
                catch (Exception ex)
                {
                    ViewBag.StatusMessage = "Error: " + ex.Message;
                    return View(data);
                }
            }
        }

    }
}
