using System;
using System.ComponentModel;

namespace MikaWeb.Models
{
    public class Cajas
    {

        public string NCaja { get; set; }
        public int IdSalon { get; set; }
        public DateTime Fecha { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
        public bool Cerrada { get; set; }
        [DisplayName("Metálico (€): ")]
        public double Metalico { get; set; }
        [DisplayName("Tarjetas (€): ")]
        public double Visas { get; set; }
        [DisplayName("Gastos (€): ")]
        public double Gastos { get; set; }
        [DisplayName("Iva soportado (€): ")]
        public double IvaSoportado { get; set; }
        [DisplayName("Iva repercutido (€): ")]
        public double IvaRepercutido { get; set; }
        [DisplayName("Saldo neto (€): ")]
        public double SaldoNeto { get; set; }

    }
}
