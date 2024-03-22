using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MikaWeb.Models
{
    public class Ficha
    {
        [DisplayName("# Ficha: ")]
        public string NFicha { get; set; }
        public DateTime Fecha { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
        public int IdSalon { get; set; }
        [DisplayName("Cliente: ")]
        public int IdCliente { get; set; }
        public string Cliente { get; set; }
        [DisplayName("Forma de pago: ")]
        public string FormaPago { get; set; }
        [DisplayName("Base €: ")]
        public double Base { get; set; }
        [DisplayName("Descuento %: ")]
        public double DescuentoPorc { get; set; }
        [DisplayName("Descuento €: ")]
        public double DescuentoImp { get; set; }
        [DisplayName("Descuentos: ")]
        public double TotalDescuentos { get; set; }
        [DisplayName("Iva €: ")]
        public double Iva { get; set; }
        [DisplayName("Total €: ")]
        public double Total { get; set; }
        [DisplayName("Pagado €: ")]
        public double Pagado { get; set; }
        [DisplayName("Cambio €: ")]
        public double Cambio { get; set; }
        public bool Cerrada { get; set; }
        //public string IncExc { get; set; }
        public string Anulable { get; set; }
        public List<Ficha_Linea> Lineas { get; set; }
    }
}
