using System;
using System.Collections.Generic;
using System.Globalization;

namespace MikaWeb.Extensions
{
    public class Utils
    {

        public static string DiaSemana(int ndia)
        {
            List<string> dias = new List<string> { "Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado" };
            return dias[ndia];
        }

        public static string FormatoCadena(string ent, int longitud)
        {
            string ret = ent;
            while (ret.Length < longitud)
            {
                ret = "0" + ret;
            }
            return ret;
        }

        public static string FormatoFecha(string dia)
        {
            if (dia.Length == 1)
            {
                dia = "0" + dia;
            }
            return dia;
        }

        public static string ListToString(List<string> lst)
        {
            string ret = "";
            for (int i = 0; i <= lst.Count - 1; i++)
            {
                ret += lst[i] + ",";
            }
            if (ret.EndsWith(","))
            {
                ret = ret.Substring(0, ret.Length - 1);
            }
            return ret;
        }

        public static string NombreMes(int mes)
        {
            string ret = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mes);
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ret);
        }

        public static int NumeroMes(string mes)
        {
            return DateTime.ParseExact(mes, "MMMM", CultureInfo.CurrentCulture).Month;
        }
    }
}
