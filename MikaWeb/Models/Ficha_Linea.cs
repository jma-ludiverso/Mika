using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MikaWeb.Models
{
    public class Ficha_Linea
    {

        public int Linea { get; set; }
        [DisplayName("Empleado")]
        [RegularExpression("^[0-9]*(\\.[0-9]{0,2})?$", ErrorMessage = "Sólo pueden usarse números")]
        public string Codigo { get; set; }
        public string Empleado { get; set; }
        [DisplayName("Servicio")]
        [RegularExpression("^[0-9]*(\\.[0-9]{0,2})?$", ErrorMessage = "Sólo pueden usarse números")]
        public string CodigoServicio { get; set; }
        [DisplayName("Servicio")]
        public int IdServicio { get; set; }
        public string Tipo { get; set; }
        public string Descripcion { get; set; }
        [DisplayName("Base €")]
        public double Base { get; set; }
        [DisplayName("Desc. %")]
        public double DescuentoPorc { get; set; }
        [DisplayName("Descuento €")]
        public double DescuentoCant { get; set; }
        [DisplayName("Iva %")]
        public double IvaPorc { get; set; }
        [DisplayName("Iva €")]
        public double IvaCant { get; set; }
        [DisplayName("Total €")]
        public double Total { get; set; }
        public double Comision { get; set; }

    }
}
