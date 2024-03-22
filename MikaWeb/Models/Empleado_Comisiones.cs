using System.Collections.Generic;

namespace MikaWeb.Models
{
    public class Empleado_Comisiones
    {

        public string Codigo { get; set; }
        public string Empleado { get; set; }
        public int IdSalon { get; set; }
        public int IdEmpresa { get; set; }
        public double ServicioLE1 { get; set; }
        public double ServicioLE2 { get; set; }
        public double ServicioLE3 { get; set; }
        public double ServicioLE4 { get; set; }
        public double ServicioSE1 { get; set; }
        public double ServicioSE2 { get; set; }
        public double ServicioSE3 { get; set; }
        public double ServicioSE4 { get; set; }
        public double ServicioTE1 { get; set; }
        public double ServicioTE2 { get; set; }
        public double ServicioTE3 { get; set; }
        public double ServicioTE4 { get; set; }
        public double ServicioLP1 { get; set; }
        public double ServicioLP2 { get; set; }
        public double ServicioLP3 { get; set; }
        public double ServicioLP4 { get; set; }
        public double ServicioSP1 { get; set; }
        public double ServicioSP2 { get; set; }
        public double ServicioSP3 { get; set; }
        public double ServicioSP4 { get; set; }
        public double ServicioTP1 { get; set; }
        public double ServicioTP2 { get; set; }
        public double ServicioTP3 { get; set; }
        public double ServicioTP4 { get; set; }
        public double ProductoE1 { get; set; }
        public double ProductoE2 { get; set; }
        public double ProductoE3 { get; set; }
        public double ProductoE4 { get; set; }
        public double ProductoP1 { get; set; }
        public double ProductoP2 { get; set; }
        public double ProductoP3 { get; set; }
        public double ProductoP4 { get; set; }
        public List<Servicio_Comision> Comisiones { get; set; }

    }
}
