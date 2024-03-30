using MikaWeb.Areas.Identity.Data;

namespace MikaWeb.Models.API
{
    public class AuthenticateResponse
    {
        public bool Succeeded { get; set; }
        public string Token { get; set; } = string.Empty;
        public MikaWebUser UserData { get; set; }

    }
}
