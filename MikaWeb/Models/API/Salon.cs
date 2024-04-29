using System.Collections.Generic;

namespace MikaWeb.Models.API
{
    public class Salon
    {
        public int Id { get; set; }
        public int IdEmpresa { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public List<Empleado> Empleados { get; set;}
        public List<Empleado_Comisiones> EmpleadosComisiones { get; set; }
    }
}
