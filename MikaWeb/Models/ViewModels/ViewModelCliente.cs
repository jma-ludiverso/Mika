using System.Collections.Generic;
using System.ComponentModel;

namespace MikaWeb.Models.ViewModels
{
    public class ViewModelCliente
    {
        public Cliente Cliente { get; set; }
        [DisplayName("Datos tintura")]
        public string NuevaHistoria { get; set; }
        public bool MuestraCabecera { get; set; }
        public List<Cliente_Historia> Historial { get; set; }
        public int IdBorrar { get; set; }
        public string FichaRef { get; set; }
    }
}
