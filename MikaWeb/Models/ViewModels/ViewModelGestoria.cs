using System.Collections.Generic;
using System.ComponentModel;

namespace MikaWeb.Models.ViewModels
{
    public class ViewModelGestoria
    {
        [DisplayName("Año: ")]
        public int Anio { get; set; }
        public string Mes { get; set; }
        public int NMes { get; set; }
        public int IdSalon { get; set; }
        public bool Cerrado { get; set; }
        [DisplayName("Tarjeta:")]
        public double Tarjeta { get; set; }
        [DisplayName("Iva tarjeta:")]
        public double IvaT { get; set; }
        [DisplayName("Efectivo:")]
        public double Efectivo { get; set; }
        [DisplayName("Iva efectivo:")]
        public double IvaE { get; set; }
        [DisplayName("Total:")]
        public double TotalIngresos { get; set; }
        [DisplayName("Iva repercut.:")]
        public double IvaRepercutido { get; set; }
        [DisplayName("Importe:")]
        public double Gastos { get; set; }
        [DisplayName("Iva soportado:")]
        public double IvaSoportado { get; set; }
        [DisplayName("Comisiones:")]
        public double Comisiones { get; set; }
        [DisplayName("Neto:")]
        public double SaldoNeto { get; set; }
        [DisplayName("Neto-com.:")]
        public double SaldoNetoCom { get; set; }
        public List<GestoriaFicha> Fichas { get; set; }
        public List<GestoriaProduccion> Producciones { get; set; }
    }
}
