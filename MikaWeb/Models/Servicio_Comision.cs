using System.ComponentModel.DataAnnotations;

namespace MikaWeb.Models
{
    public class Servicio_Comision
    {

        public int IdServicio { get; set; }
        [Required]
        public double ComisionP1 { get; set; }
        [Required]
        public double ComisionP2 { get; set; }
        [Required]
        public double ComisionP3 { get; set; }
        [Required]
        public double ComisionP4 { get; set; }
        public string Tipo { get; set; }
        public string Descripcion { get; set; }

    }
}
