using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using MikaWeb.Areas.Identity.Data;
using MikaWeb.Extensions.DB;

namespace MikaWeb.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<MikaWebUser> _userManager;
        private readonly SignInManager<MikaWebUser> _signInManager;
        private readonly DBConfig _dbConfig;

        public IndexModel(
            UserManager<MikaWebUser> userManager,
            SignInManager<MikaWebUser> signInManager, IOptions<DBConfig> dbConf)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbConfig = dbConf.Value;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public List<SelectListItem> Salones { get; set; }

        public class InputModel
        {

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Nombre")]
            public string Nombre { get; set; }

            [Required]
            [Display(Name = "Apellidos")]
            [DataType(DataType.Text)]
            public string Apellidos { get; set; }

            [Phone]
            [Display(Name = "Teléfono")]
            public string PhoneNumber { get; set; }

            [Required]
            [Display(Name = "Codigo empleado")]
            [DataType(DataType.Text)]
            public string Codigo { get; set; }

            [Required]
            [Display(Name = "Salón")]
            public int Salon { get; set; }

            [Required]
            [Display(Name = "Activo")]
            //[DataType(DataType.Custom)]
            public bool Activo { get; set; }

            [Required]
            [Display(Name = "Administrador")]
            //[DataType(DataType.Custom)]
            public bool Administrador { get; set; }
        }

        private async Task LoadAsync(MikaWebUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                Nombre = user.Nombre,
                Apellidos = user.Apellidos,
                PhoneNumber = phoneNumber,
                Codigo = user.Codigo,
                Salon = user.Salon,
                Activo = user.Activo, 
                Administrador = user.IsAdmin
            };
        }

        private async Task LoadSalones(string userId)
        {
            try
            {
                DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
                string sql = "select IdSalon, Nombre from Salones where IdEmpresa in (" +
                    "select sal.IdEmpresa from AspNetUsers us " +
                    "inner join Salones sal on us.Salon = sal.IdSalon " +
                    "where us.Id=@id) order by Nombre";
                System.Data.SqlClient.SqlCommand sc = new System.Data.SqlClient.SqlCommand(sql);
                sc.Parameters.AddWithValue("@id", userId);
                DataSet ds = await db.GetDataSet(sc, "salones");
                List<SelectListItem> lst = new List<SelectListItem>();
                for (int i=0; i <= ds.Tables["salones"].Rows.Count - 1; i++)
                {
                    DataRow fila = ds.Tables["salones"].Rows[i];
                    SelectListItem it = new SelectListItem(fila["Nombre"].ToString(), fila["IdSalon"].ToString());
                    lst.Add(it);
                }
                this.Salones = lst;
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            MikaWebUser user = null;
            if (Request.QueryString.HasValue)
            {
                 user = await _userManager.FindByIdAsync(Request.Query["id"]);
            }
            else
            {
                user = await _userManager.GetUserAsync(User);
            }
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            await LoadAsync(user);
            await LoadSalones(user.Id);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                MikaWebUser user = null;
                if (Request.QueryString.HasValue)
                {
                    user = await _userManager.FindByIdAsync(Request.Query["id"]);
                }
                else
                {
                    user = await _userManager.GetUserAsync(User);
                }
                if (user == null)
                {
                    return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                }

                if (!ModelState.IsValid)
                {
                    await LoadAsync(user);
                    return Page();
                }

                var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
                if (Input.PhoneNumber != phoneNumber)
                {
                    var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                    if (!setPhoneResult.Succeeded)
                    {
                        StatusMessage = "Unexpected error when trying to set phone number.";
                        return RedirectToPage();
                    }
                }

                if (Input.Nombre != user.Nombre)
                {
                    user.Nombre = Input.Nombre;
                }
                if (Input.Apellidos != user.Apellidos)
                {
                    user.Apellidos = Input.Apellidos;
                }
                if (Input.Codigo != user.Codigo)
                {
                    user.Codigo = Input.Codigo;
                }
                if (Input.Salon != user.Salon)
                {
                    user.Salon = Input.Salon;
                }
                if (Input.Activo != user.Activo)
                {
                    user.Activo = Input.Activo;
                }
                if (Input.Administrador != user.IsAdmin)
                {
                    user.IsAdmin = Input.Administrador;
                }

                await _userManager.UpdateAsync(user);

                if (!Request.QueryString.HasValue)
                {
                    await _signInManager.RefreshSignInAsync(user);
                }

                StatusMessage = "Se han actualizado los datos del usuario";
            }
            catch (Exception ex)
            {
                StatusMessage = "Error: " + ex.Message;
                if (ex.InnerException != null)
                {
                    StatusMessage += " - Inner Exception: " + ex.InnerException.Message;
                }
            }
            if (Request.QueryString.HasValue)
            {
                return RedirectToPage(new { id = Request.Query["id"] });
            }
            else
            {
                return RedirectToPage();
            }
                
        }
    }
}
