using NPOI.SS.UserModel;
using System.Collections.Generic;

namespace MikaWeb.Extensions
{
    public class Export
    {
        public bool Abrir { get; set; }
        public List<Export_Column> Columnas { get; set; }
        public ICellStyle EstiloBase { get; set; }
        public ICellStyle EstiloCabecera { get; set; }
        public ICellStyle EstiloCentrado { get; set; }
        public string NombreArchivo { get; set; }
        public string NombreHoja { get; set; }

    }
}
