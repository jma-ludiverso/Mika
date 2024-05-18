using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MikaWeb.Models
{
    public class Cliente
    {

        public List<Cliente_Historia> Historial { get; set; }
        public int IdCliente { get; set; }
        public int IdSalon { get; set; }
        [MaxLength(50)]
        public string Nombre { get; set; }
        [NotMapped]
        public bool Nuevo { get; set; }
        [DataType(DataType.PhoneNumber), MaxLength(50)]
        public string Telefono { get; set; }
        [DataType(DataType.EmailAddress), MaxLength(50)]
        public string Email { get; set; }

    }
}
