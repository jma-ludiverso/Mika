using System.Collections.Generic;
using System.ComponentModel;

namespace MikaWeb.Models
{
    public class Inicio
    {

        public string Mensaje { get; set; }
        public string CssCaja { get; set; }
        public int ProgresoLista { get; set; }
        public string Fecha { get; set; }
        public string NCaja { get; set; }
        public bool EstadoCaja { get; set; }
        [DisplayName("Total clientes: ")]
        public int Clientes { get; set; }
        [DisplayName("Total cobrado (€)")]
        public double Total { get; set; }
        public List<CitasEmpleado> Citas { get; set; }
        public List<Ficha_Resumen> Fichas { get; set; }

        public int TotalCitas
        {
            get
            {
                int ret = 0;
                foreach(CitasEmpleado item in Citas)
                {
                    ret += item.Citas.Count;
                }
                return ret;
            }
        }
        

    }
}
