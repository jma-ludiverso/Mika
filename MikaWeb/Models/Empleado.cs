namespace MikaWeb.Models
{
    public class Empleado
    {
        public string Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Email { get; set; }
        public bool Activo { get; set; }
        public bool IsAdmin { get; set; }
        public string PhoneNumber { get; set; }
        public int Salon { get; set; }
        public string UserName { get; set; }
    }
}
