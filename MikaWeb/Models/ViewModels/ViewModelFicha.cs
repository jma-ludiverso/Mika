using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MikaWeb.Models.ViewModels
{
    public class ViewModelFicha
    {
        public SelectList Clientes { get; set; }
        public Ficha Datos { get; set; }
        [DisplayName("Total servicios: ")]
        public double TotalServicios { get; set; }
        [DisplayName("Total productos: ")]
        public double TotalProductos { get; set; }
        public Ficha_Linea LineaTrabajo { get; set; }
        public ViewModelCliente Historial { get; set; }

    }
}
