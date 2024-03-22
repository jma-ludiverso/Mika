using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MikaWeb.Models.ViewModels
{
    public class ViewModelCitas
    {

        public SelectList Empleados { get; set; }
        public SelectList Clientes { get; set; }
        public Calendario Dia { get; set; }
        [DisplayName("Hora")]
        [RegularExpression("^[0-9]*(\\.[0-9]{0,2})?$", ErrorMessage = "Sólo pueden usarse números")]
        public string NuevaCitaHora { get; set; }
        [DisplayName("Minutos")]
        [RegularExpression("^[0-9]*(\\.[0-9]{0,2})?$", ErrorMessage = "Sólo pueden usarse números")]
        public string NuevaCitaMinutos { get; set; }
        [DisplayName("Cliente")]
        public int NuevaCitaCliente { get; set; }
        [DisplayName("Cliente (no registrado):")]
        public string NuevaCitaNoregistrado { get; set; }
        [DisplayName("Empleado")]
        public string NuevaCitaEmpleado { get; set; }
        [DisplayName("Notas:")]
        public string NuevaCitaNotas { get; set; }
        public string FiltroEmpleado { get; set; }
        public int Salon { get; set; }
        public string DatosBorrar { get; set; }

    }
}
