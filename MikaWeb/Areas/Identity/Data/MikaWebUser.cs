using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace MikaWeb.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the MikaWebUser class
    public class MikaWebUser : IdentityUser
    {

        [PersonalData]
        public string Nombre { get; set; }
        [PersonalData]
        public string Apellidos { get; set; }
        [PersonalData]
        public bool Activo { get; set; }
        public bool IsAdmin { get; set; }
        [PersonalData]
        public string Codigo { get; set; }
        [PersonalData]
        public int Salon { get; set; }

    }
}
