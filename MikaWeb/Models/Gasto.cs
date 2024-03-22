using System.ComponentModel;

namespace MikaWeb.Models
{
    public class Cajas_Gastos
    {

        public string NCaja { get; set; }
        public int IdSalon { get; set; }
        public int Linea { get; set; }
        public string  Concepto { get; set; }
        [DisplayName("Importe (€): ")]
        public double Importe { get; set; }
        [DisplayName("Iva (€): ")]
        public double Iva { get; set; }

    }
}
