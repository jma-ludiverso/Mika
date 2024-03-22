namespace MikaWeb.Models
{
    public class GestoriaProduccion
    {
        public string Codigo { get; set; }
        public string Empleado { get; set; }
        public int NServiciosL { get; set; }
        public double ProdServiciosL { get; set; }
        public double ComisionServiciosL { get; set; }
        public int NServiciosR { get; set; }
        public double ProdServiciosR { get; set; }
        public double ComisionServiciosR { get; set; }
        public int NServiciosT { get; set; }
        public double ProdServiciosT { get; set; }
        public double ComisionServiciosT { get; set; }
        public int NProductos { get; set; }
        public double ProdProductos { get; set; }
        public double ComisionProductos { get; set; }
        public double TotalProduccion { get; set; }
        public double TotalComisiones { get; set; }
        public Empleado_Comisiones Paquetes { get; set; }
    }
}
