using System;
using System.Collections.Generic;

namespace MikaWeb.Models
{
    public class Calendario
    {
        private List<CitasEmpleado> _citas = new List<CitasEmpleado>();
        public bool AdmiteCitas { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime MesCargado { get; set; }
        public string DiaSemana { get; set; }        
        public List<CitasEmpleado> Citas
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
        public List<CalendarioHoras> Horas { get; set; }

    }
}
