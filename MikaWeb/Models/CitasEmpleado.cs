using System.Collections.Generic;

namespace MikaWeb.Models
{
    public class CitasEmpleado
    {
        private List<Calendario_Cita> _citas = new List<Calendario_Cita>();

        public string CodigoEmpleado { get; set; }
        public string Empleado { get; set; }

        public List<Calendario_Cita> Citas
        {
            get
            {
                return _citas;
            }
            set
            {
                _citas = value;
            }
        }
    }
}
