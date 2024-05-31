using MikaWeb.Extensions.DB;
using MikaWeb.Models;
using System.Data;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MikaWeb.Extensions
{
    public class FichasExtension
    {

        DBExtension db;

        public FichasExtension(DBExtension _db)
        {
            db = _db;
        }

        public void BorraLinea(string numficha, int salon, int idLinea)
        {
            try
            {
                string sql = "Delete from Fichas_Lineas Where IdSalon=@sal and NFicha=@nficha and Linea=@linea";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@sal", salon);
                sc.Parameters.AddWithValue("@nficha", numficha);
                sc.Parameters.AddWithValue("@linea", idLinea);
                db.Command(sc);
            }
            catch
            {
                throw;
            }
        }

        public async Task<Ficha> CargaFicha(string numficha, string salon)
        {
            Ficha ret = new Ficha();
            try
            {
                string sql = "select Fecha, IdCliente, FormaPago, Base, DescuentoPorc, DescuentoImp, Descuentos, Iva, Total, Cerrada " +
                    "from Fichas Where IdSalon=@sal and NFicha=@nficha";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@sal", salon);
                sc.Parameters.AddWithValue("@nficha", numficha);
                DataSet ds = await db.GetDataSet(sc, "ficha");
                DataRow filaF = ds.Tables["ficha"].Rows[0];
                ret.Base = double.Parse(filaF["Base"].ToString());
                ret.DescuentoImp = double.Parse(filaF["DescuentoImp"].ToString());
                ret.DescuentoPorc = double.Parse(filaF["DescuentoPorc"].ToString());
                ret.Fecha = DateTime.Parse(filaF["Fecha"].ToString());
                ret.FormaPago = filaF["FormaPago"].ToString();
                ret.IdCliente = int.Parse(filaF["IdCliente"].ToString());
                ret.IdSalon = int.Parse(salon);
                ret.Iva = double.Parse(filaF["Iva"].ToString());
                ret.NFicha = numficha;
                ret.Total = double.Parse(filaF["Total"].ToString());
                ret.TotalDescuentos = double.Parse(filaF["Descuentos"].ToString());
                ret.Cerrada = bool.Parse(filaF["Cerrada"].ToString());
                ret.Lineas = await this.CargaFicha_Lineas(numficha, salon);
            }
            catch { throw; }
            return ret;
        }

        public async Task<List<Ficha_Linea>> CargaFicha_Lineas(string numficha, string salon)
        {
            List<Ficha_Linea> ret = new List<Ficha_Linea>();
            try
            {
                string sql = "select fl.Linea, fl.Codigo, fl.IdServicio, s.Codigo as CodServicio, fl.Descripcion, " +
                    "s.Tipo, fl.Base, fl.DescuentoPorc, fl.DescuentoCant, fl.IvaPorc, fl.IvaCant, fl.Total, '' as Empleado " +
                    "from Fichas_Lineas fl " +
                    "inner join Servicios s on fl.IdServicio = s.IdServicio " +
                    "Where fl.IdSalon=@sal and fl.NFicha=@nficha";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@sal", salon);
                sc.Parameters.AddWithValue("@nficha", numficha);
                DataSet ds = await db.GetDataSet(sc, "lineas");
                for (int i = 0; i <= ds.Tables["lineas"].Rows.Count - 1; i++)
                {
                    ret.Add(this.getDatosLinea(ds.Tables["lineas"].Rows[i]));
                }
            }
            catch { throw; }
            return ret;
        }

        public async Task<List<Ficha_Linea>> CargaFicha_Lineas(string numficha, int salon)
        {
            List<Ficha_Linea> ret = new List<Ficha_Linea>();
            try
            {
                string sql = "select f.Linea, f.Codigo + '-' + us.Nombre + ' ' + us.Apellidos as Empleado, " +
                    "serv.Tipo, serv.Codigo + '-' + f.Descripcion as Descripcion, f.Base, f.DescuentoPorc, f.DescuentoCant, f.IvaPorc, f.IvaCant, f.Total, " +
                    "f.ComisionP1, f.ComisionP2, f.ComisionP3, f.ComisionP4, f.Codigo, serv.IdServicio, serv.Codigo as CodServicio " +
                    "from Fichas_Lineas f " +
                    "inner join Fichas fich on f.NFicha = fich.NFicha and f.IdSalon=fich.IdSalon " +
                    "inner join AspNetUsers us on f.Codigo = us.Codigo and fich.IdSalon = us.Salon " +
                    "inner join Salones s on us.Salon = s.IdSalon " +
                    "inner join Servicios serv on serv.IdEmpresa = s.IdEmpresa and serv.IdServicio=f.IdServicio " +
                    "where f.NFicha=@nficha and us.Salon=@sal " +
                    "order by f.Linea";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@nficha", numficha);
                sc.Parameters.AddWithValue("@sal", salon);
                DataSet ds = await db.GetDataSet(sc, "lineas");
                for (int i = 0; i <= ds.Tables["lineas"].Rows.Count - 1; i++)
                {
                    ret.Add(this.getDatosLinea(ds.Tables["lineas"].Rows[i]));
                }
            }
            catch { throw; }
            return ret;
        }

        private async Task<Servicio_Comision> getComision(Ficha_Linea flinea, SqlTransaction t = null)
        {
            Servicio_Comision ret = new Servicio_Comision();
            try
            {
                string sql = "Select ComisionP1,ComisionP2,ComisionP3,ComisionP4 " +
                    "from EmpleadosServicios where Codigo=@codigo and IdServicio=@idservicio";
                SqlCommand sc = new SqlCommand(sql);
                if (t != null)
                {
                    sc.Transaction = t;
                }
                sc.Parameters.AddWithValue("@codigo", flinea.Codigo);
                sc.Parameters.AddWithValue("@idservicio", flinea.IdServicio);
                DataSet ds = await db.GetDataSet(sc, "comision");
                if (ds.Tables["comision"].Rows.Count > 0)
                {
                    DataRow fila = ds.Tables["comision"].Rows[0];
                    double porc = double.Parse(fila["ComisionP1"].ToString());
                    ret.ComisionP1 = Math.Round((flinea.Base - flinea.DescuentoCant) * (porc / 100), 2);
                    porc = double.Parse(fila["ComisionP2"].ToString());
                    ret.ComisionP2 = Math.Round((flinea.Base - flinea.DescuentoCant) * (porc / 100), 2);
                    porc = double.Parse(fila["ComisionP3"].ToString());
                    ret.ComisionP3 = Math.Round((flinea.Base - flinea.DescuentoCant) * (porc / 100), 2);
                    porc = double.Parse(fila["ComisionP4"].ToString());
                    ret.ComisionP4 = Math.Round((flinea.Base - flinea.DescuentoCant) * (porc / 100), 2);
                }
            }
            catch { throw; }
            return ret;
        }

        private Ficha_Linea getDatosLinea(DataRow fila)
        {
            try
            {
                Ficha_Linea linea = new Ficha_Linea();
                linea.Linea = int.Parse(fila["linea"].ToString());
                linea.Codigo = fila["Codigo"].ToString();
                linea.IdServicio = int.Parse(fila["IdServicio"].ToString());
                linea.CodigoServicio = fila["CodServicio"].ToString();
                linea.Descripcion = fila["Descripcion"].ToString();
                linea.Empleado = fila["Empleado"].ToString();
                linea.Tipo = fila["Tipo"].ToString();
                linea.Base = double.Parse(fila["Base"].ToString());
                linea.DescuentoPorc = double.Parse(fila["DescuentoPorc"].ToString());
                linea.DescuentoCant = double.Parse(fila["DescuentoCant"].ToString());
                linea.IvaPorc = double.Parse(fila["IvaPorc"].ToString());
                linea.IvaCant = double.Parse(fila["IvaCant"].ToString());
                linea.Total = double.Parse(fila["Total"].ToString());
                return linea;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Ficha_Linea> getDatosLinea(int IdSalon, string nFicha, int nLinea)
        {
            try
            {
                string sql = "select fl.Linea, fl.Codigo, us.Nombre + ' ' + us.Apellidos as Empleado, fl.IdServicio, " +
                    "s.Codigo as CodServicio, fl.Descripcion, s.Tipo, fl.Base, fl.DescuentoPorc, " +
                    "fl.DescuentoCant, fl.IvaPorc, fl.IvaCant, fl.Total " +
                    "from Fichas_Lineas fl " +
                    "inner join Servicios s on fl.IdServicio = s.IdServicio " +
                    "inner join AspNetUsers us on fl.Codigo = us.Codigo and fl.IdSalon = us.Salon " +
                    "Where fl.IdSalon=@sal and fl.NFicha=@nficha and fl.Linea=@linea";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@sal", IdSalon);
                sc.Parameters.AddWithValue("@nficha", nFicha);
                sc.Parameters.AddWithValue("@linea", nLinea);
                DataSet ds = await db.GetDataSet(sc, "linea");
                return this.getDatosLinea(ds.Tables["linea"].Rows[0]);
            }
            catch
            {
                throw;
            }
        }

        public async Task<Ficha> guarda_Ficha(Ficha datos, SqlTransaction t = null)
        {
            bool localTransaction = false;
            try
            {
                string sql = "";
                if (t == null)
                {
                    t = db.GetTransaction();
                    localTransaction = true;
                }
                if (datos.NFicha.Equals("(sin guardar)"))
                {
                    try
                    {
                        sql = "select ISNULL(Max(Numero),0) + 1 from Fichas where Anio=@anio and Mes=@mes and IdSalon=@sal";
                        SqlCommand sc = new SqlCommand(sql);
                        sc.Transaction = t;
                        sc.Parameters.AddWithValue("@anio", datos.Fecha.Year);
                        sc.Parameters.AddWithValue("@mes", datos.Fecha.Month);
                        sc.Parameters.AddWithValue("@sal", datos.IdSalon);
                        DataSet ds = await db.GetDataSet(sc, "max");
                        sql = "insert into Fichas values(@nficha,@fecha,@anio,@mes,@numero,@idsalon,@idcliente,@formapago,@base," +
                            "@descuentoporc,@descuentoimp,@descuentos,@iva,@total,@pagado,@cambio,@cerrada)";
                        sc = new SqlCommand(sql);
                        sc.Transaction = t;
                        datos.NFicha = datos.Fecha.Year.ToString() + Extensions.Utils.FormatoFecha(datos.Fecha.Month.ToString()) + Extensions.Utils.FormatoCadena(ds.Tables["max"].Rows[0][0].ToString(), 3);
                        sc.Parameters.AddWithValue("@nficha", datos.NFicha);
                        sc.Parameters.AddWithValue("@fecha", datos.Fecha);
                        sc.Parameters.AddWithValue("@anio", datos.Fecha.Year);
                        sc.Parameters.AddWithValue("@mes", datos.Fecha.Month);
                        sc.Parameters.AddWithValue("@numero", ds.Tables["max"].Rows[0][0]);
                        sc.Parameters.AddWithValue("@idsalon", datos.IdSalon);
                        sc.Parameters.AddWithValue("@idcliente", datos.IdCliente);
                        sc.Parameters.AddWithValue("@formapago", datos.FormaPago);
                        sc.Parameters.AddWithValue("@base", datos.Base);
                        sc.Parameters.AddWithValue("@descuentoporc", datos.DescuentoPorc);
                        sc.Parameters.AddWithValue("@descuentoimp", datos.DescuentoImp);
                        sc.Parameters.AddWithValue("@descuentos", datos.TotalDescuentos);
                        sc.Parameters.AddWithValue("@iva", datos.Iva);
                        sc.Parameters.AddWithValue("@total", datos.Total);
                        sc.Parameters.AddWithValue("@pagado", datos.Pagado);
                        sc.Parameters.AddWithValue("@cambio", datos.Cambio);
                        sc.Parameters.AddWithValue("@cerrada", false);
                        db.Command(sc);
                        if (localTransaction)
                        {
                            t.Commit();
                            db.Close();
                        }
                    }
                    catch
                    {
                        datos.NFicha = "(sin guardar)";
                        throw;
                    }
                }
                else
                {
                    sql = "update Fichas set IdCliente=@idcliente, FormaPago=@formapago, Base=@base, DescuentoPorc=@descuentoporc, " +
                        "DescuentoImp=@descuentoimp, Descuentos=@descuentos, iva=@iva, Total=@total, Pagado=@pagado, Cambio=@cambio, " +
                        "Cerrada=@cerrada " +
                        "where NFicha=@nficha and IdSalon=@idsalon";
                    SqlCommand sc = new SqlCommand(sql);
                    sc.Transaction = t;
                    sc.Parameters.AddWithValue("@idcliente", datos.IdCliente);
                    sc.Parameters.AddWithValue("@formapago", datos.FormaPago);
                    sc.Parameters.AddWithValue("@base", datos.Base);
                    sc.Parameters.AddWithValue("@descuentoporc", datos.DescuentoPorc);
                    sc.Parameters.AddWithValue("@descuentoimp", datos.DescuentoImp);
                    sc.Parameters.AddWithValue("@descuentos", datos.TotalDescuentos);
                    sc.Parameters.AddWithValue("@iva", datos.Iva);
                    sc.Parameters.AddWithValue("@total", datos.Total);
                    sc.Parameters.AddWithValue("@pagado", datos.Pagado);
                    sc.Parameters.AddWithValue("@cambio", datos.Cambio);
                    sc.Parameters.AddWithValue("@cerrada", datos.Cerrada);
                    sc.Parameters.AddWithValue("@nficha", datos.NFicha);
                    sc.Parameters.AddWithValue("@idsalon", datos.IdSalon);
                    db.Command(sc);
                    if (localTransaction)
                    {
                        t.Commit();
                        db.Close();
                    }
                }
            }
            catch
            {
                try
                {
                    if (localTransaction)
                    {
                        t.Rollback();
                        db.Close();
                    }
                }
                catch { }
                throw;
            }
            return datos;
        }

        public async Task<Ficha_Linea> guardaLinea(Ficha_Linea linea, string nficha, int salon, double descCabecera, SqlTransaction t = null)
        {
            bool localTransaction = false;
            try
            {
                double descCant = linea.Base * (linea.DescuentoPorc / 100);
                if (linea.Tipo.Equals("Servicio"))
                {
                    //no se aplican descuentos a productos, sólo a servicios
                    descCant += (linea.Base - descCant) * (descCabecera / 100);
                }
                linea.DescuentoCant = Math.Round(descCant, 2);
                linea.IvaCant = Math.Round((linea.Base - linea.DescuentoCant) * (linea.IvaPorc / 100), 3);
                linea.Total = linea.Base - linea.DescuentoCant + linea.IvaCant;
                string sql = "";
                if (t == null)
                {
                    t = db.GetTransaction();
                    localTransaction = true;
                }
                if (linea.Linea == 0)
                {
                    sql = "select ISNULL(Max(Linea),0) + 1 from Fichas_Lineas where NFicha=@nficha and IdSalon=@sal";
                    SqlCommand sc = new SqlCommand(sql);
                    sc.Transaction = t;
                    sc.Parameters.AddWithValue("@nficha", nficha);
                    sc.Parameters.AddWithValue("@sal", salon);
                    DataSet ds = await db.GetDataSet(sc, "max");
                    linea.Linea = int.Parse(ds.Tables["max"].Rows[0][0].ToString());
                    sql = "insert into Fichas_Lineas values(@nficha,@idsalon,@linea,@codigo,@idservicio,@descripcion," +
                        "@base,@descuentoporc,@descuentocant,@ivaporc,@ivacant,@total,@comision1,@comision2,@comision3,@comision4)";
                    sc = new SqlCommand(sql);
                    sc.Transaction = t;
                    sc.Parameters.AddWithValue("@nficha", nficha);
                    sc.Parameters.AddWithValue("@idsalon", salon);
                    sc.Parameters.AddWithValue("@linea", linea.Linea);
                    sc.Parameters.AddWithValue("@codigo", linea.Codigo);
                    sc.Parameters.AddWithValue("@idservicio", linea.IdServicio);
                    sc.Parameters.AddWithValue("@descripcion", linea.Descripcion);
                    sc.Parameters.AddWithValue("@base", linea.Base);
                    sc.Parameters.AddWithValue("@descuentoporc", linea.DescuentoPorc);
                    sc.Parameters.AddWithValue("@descuentocant", linea.DescuentoCant);
                    sc.Parameters.AddWithValue("@ivaporc", linea.IvaPorc);
                    sc.Parameters.AddWithValue("@ivacant", linea.IvaCant);
                    sc.Parameters.AddWithValue("@total", linea.Total);
                    Servicio_Comision comisiones = await this.getComision(linea, t);
                    sc.Parameters.AddWithValue("@comision1", comisiones.ComisionP1);
                    sc.Parameters.AddWithValue("@comision2", comisiones.ComisionP2);
                    sc.Parameters.AddWithValue("@comision3", comisiones.ComisionP3);
                    sc.Parameters.AddWithValue("@comision4", comisiones.ComisionP4);
                    db.Command(sc);
                    if (localTransaction)
                    {
                        t.Commit();
                        db.Close();
                    }
                }
                else
                {
                    sql = "update Fichas_Lineas set Codigo=@codigo,IdServicio=@idservicio,Descripcion=@descripcion," +
                        "Base=@base,DescuentoPorc=@descuentoporc,DescuentoCant=@descuentocant,IvaPorc=@ivaporc," +
                        "IvaCant=@ivacant,Total=@total,ComisionP1=@comision1,ComisionP2=@comision2,ComisionP3=@comision3,ComisionP4=@comision4 " +
                        "where NFicha=@nficha and IdSalon=@idsalon and Linea=@linea ";
                    SqlCommand sc = new SqlCommand(sql);
                    sc.Transaction = t;
                    sc.Parameters.AddWithValue("@codigo", linea.Codigo);
                    sc.Parameters.AddWithValue("@idservicio", linea.IdServicio);
                    sc.Parameters.AddWithValue("@descripcion", linea.Descripcion);
                    sc.Parameters.AddWithValue("@base", linea.Base);
                    sc.Parameters.AddWithValue("@descuentoporc", linea.DescuentoPorc);
                    sc.Parameters.AddWithValue("@descuentocant", linea.DescuentoCant);
                    sc.Parameters.AddWithValue("@ivaporc", linea.IvaPorc);
                    sc.Parameters.AddWithValue("@ivacant", linea.IvaCant);
                    sc.Parameters.AddWithValue("@total", linea.Total);
                    Servicio_Comision comisiones = await this.getComision(linea);
                    sc.Parameters.AddWithValue("@comision1", comisiones.ComisionP1);
                    sc.Parameters.AddWithValue("@comision2", comisiones.ComisionP2);
                    sc.Parameters.AddWithValue("@comision3", comisiones.ComisionP3);
                    sc.Parameters.AddWithValue("@comision4", comisiones.ComisionP4);
                    sc.Parameters.AddWithValue("@nficha", nficha);
                    sc.Parameters.AddWithValue("@idsalon", salon);
                    sc.Parameters.AddWithValue("@linea", linea.Linea);
                    db.Command(sc);
                    if (localTransaction)
                    {
                        t.Commit();
                        db.Close();
                    }
                }
            }
            catch {
                try
                {
                    if (localTransaction)
                    {
                        t.Rollback();
                        db.Close();
                    }
                }
                catch { }
            }
            return linea;
        }

    }
}
