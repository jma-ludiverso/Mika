using System.ComponentModel.DataAnnotations;

namespace MikaWeb.Models
{
    public class Servicio
    {
        public int IdServicio { get; set; }
        public int IdEmpresa { get; set; }
        [RegularExpression("^[0-9]*(\\.[0-9]{0,2})?$", ErrorMessage = "Sólo pueden usarse números")]
        public string Codigo { get; set; }
        public string Tipo { get; set; }
        public string Grupo { get; set; }
        public string Nombre { get; set; }
        [Display(Name = "Precio base (€)")]
        public double Precio { get; set; }
        [Display(Name = "% Iva")]
        public double IvaPorc { get; set; }
        [Display(Name = "Importe Iva (€)")]
        public double IvaCant { get; set; }
        [Display(Name = "PVP (€)")]
        public double PVP { get; set; }
        public bool Activo { get; set; }

    }
}
