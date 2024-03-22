using System.ComponentModel.DataAnnotations;

namespace MikaWeb.Models
{
    public class Cliente
    {

        public int IdCliente { get; set; }
        public int IdSalon { get; set; }
        [MaxLength(50)]
        public string Nombre { get; set; }
        [DataType(DataType.PhoneNumber), MaxLength(50)]
        public string Telefono { get; set; }
        [DataType(DataType.EmailAddress), MaxLength(50)]
        public string Email { get; set; }

    }
}
