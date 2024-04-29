using MikaWeb.Areas.Identity.Data;
using System;

namespace MikaWeb.Models.API
{
    public class AuthenticateResponse
    {
        public bool Succeeded { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime TokenExpires { get; set; }
        public MikaWebUser UserData { get; set; }

    }
}
