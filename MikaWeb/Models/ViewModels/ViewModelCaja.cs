using System.ComponentModel;

namespace MikaWeb.Models.ViewModels
{
    public class ViewModelCaja
    {
        [DisplayName("Año: ")]
        public int FiltroAnio { get; set; }
        [DisplayName("Mes: ")]
        public string FiltroMes { get; set; }
        public Cajas Caja { get; set; }
        [DisplayName("Retirada efectivo (€): ")]
        public double RetiradaEfectivo { get; set; }
        [DisplayName("Saldo bruto (€): ")]
        public double SaldoBruto { get; set; }
        public Cajas_Gastos LineaGasto { get; set; }
        public string FichaBorrar { get; set; }
        [DisplayName("Listado (2) (€): ")]
        public double Listado { get; set; }
    }
}
