using System.Collections.Generic;

namespace MikaWeb.Models.API
{
    public class ServerData
    {
        public Empresa DatosEmpresa { get; set; }
        public Salon DatosSalon { get; set; }
        public List<Cliente> ListaClientes { get; set; }
        public List<Servicio> ListaServicios { get; set; }
    }
}
