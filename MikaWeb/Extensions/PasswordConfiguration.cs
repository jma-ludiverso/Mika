namespace MikaWeb.Extensions
{
    public class PasswordConfiguration
    {
        public bool RequireDigit { get; set; }
        public bool RequireLowercase { get; set; }
        public int RequiredLength { get; set; }
        public int RequiredUniqueChars { get; set; }
        public bool RequireNonAlphanumeric { get; set; }
        public bool RequireUppercase { get; set; }
    }
}
