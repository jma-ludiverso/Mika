using System;

namespace MikaWeb.Models.API
{
    public class JWTToken
    {

        public DateTime Expires { get; set; }
        public string Token { get; set; }

    }
}
