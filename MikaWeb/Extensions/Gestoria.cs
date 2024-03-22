using MikaWeb.Extensions.DB;
using MikaWeb.Models;
using MikaWeb.Models.AuxiliaryModels;
using MikaWeb.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace MikaWeb.Extensions
{
    public class Gestoria
    {
        DBExtension db = null;

        public Gestoria(DBExtension _db)
        {
            db = _db;
        }

        private async Task<List<GestoriaFicha>> DatosFichas(int IdSalon, int Anio, int Mes)
        {
            List<GestoriaFicha> ret = new List<GestoriaFicha>();
            try
            {
                int num = 1;
                string sql = "select f.NFicha, f.FormaPago, f.Base - f.Descuentos as Base, f.Iva, gf.FichaCierre " +
                    "from Fichas f " +
                    "left join Gestoria_Fichas gf on f.IdSalon=gf.IdSalon and f.Anio=gf.Anio and f.Mes=gf.Mes and f.NFicha=gf.FichaCaja " +
                    "where f.IdSalon=@sal and f.Anio=@anio and f.Mes=@mes " +
                    "order by f.NFicha";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@sal", IdSalon);
                sc.Parameters.AddWithValue("@anio", Anio);
                sc.Parameters.AddWithValue("@mes", Mes);
                DataSet ds = await db.GetDataSet(sc, "fichas");
                for(int i=0; i <= ds.Tables["fichas"].Rows.Count - 1; i++)
                {
                    DataRow fila = ds.Tables["fichas"].Rows[i];
                    GestoriaFicha f = new GestoriaFicha();
                    f.FichaCaja = fila["NFicha"].ToString();
                    f.FormaPago = fila["FormaPago"].ToString();
                    f.Base = double.Parse(fila["Base"].ToString());
                    f.Iva = double.Parse(fila["Iva"].ToString());
                    if (fila["FichaCierre"]==DBNull.Value)
                    {
                        f.FichaCierre = Anio.ToString() + Utils.FormatoFecha(Mes.ToString()) + Utils.FormatoCadena(num.ToString(), 3);
                        sql = "insert into Gestoria_Fichas Values(@sal,@anio,@mes,@fichacaja,@fichacierre,@incluido)";
                        sc = new SqlCommand(sql);
                        sc.Parameters.AddWithValue("@sal", IdSalon);
                        sc.Parameters.AddWithValue("@anio", Anio);
                        sc.Parameters.AddWithValue("@mes", Mes);
                        sc.Parameters.AddWithValue("@fichacaja", f.FichaCaja);
                        sc.Parameters.AddWithValue("@fichacierre", f.FichaCierre);
                        sc.Parameters.AddWithValue("@incluido", true);
                        db.Command(sc);
                        num += 1;
                    }
                    else
                    {
                        f.FichaCierre = Anio.ToString() + Utils.FormatoFecha(Mes.ToString()) + Utils.FormatoCadena(num.ToString(), 3);
                        num += 1;
                        sql = "Update Gestoria_Fichas set FichaCierre=@fichacierre " +
                            "where IdSalon=@sal and Anio=@anio and Mes=@mes and FichaCaja=@fichacaja ";
                        sc = new SqlCommand(sql);
                        sc.Parameters.AddWithValue("@fichacierre", f.FichaCierre);
                        sc.Parameters.AddWithValue("@sal", IdSalon);
                        sc.Parameters.AddWithValue("@anio", Anio);
                        sc.Parameters.AddWithValue("@mes", Mes);
                        sc.Parameters.AddWithValue("@fichacaja", f.FichaCaja);
                        db.Command(sc);
                    }
                    ret.Add(f);
                }
            }
            catch {
                throw;
            }
            return ret;
        }

        private async Task<List<GestoriaProduccion>> DatosProduccion(int IdSalon, int Anio, int Mes)
        {
            List<GestoriaProduccion> ret = new List<GestoriaProduccion>();
            try
            {
                DataTable dtLavado = await this.DatosProduccion_GetDatos(IdSalon, Anio, Mes, "Servicio", "Lavado");
                DataTable dtResto = await this.DatosProduccion_GetDatos(IdSalon, Anio, Mes, "Servicio", "Servicios");
                DataTable dtTecnicos = await this.DatosProduccion_GetDatos(IdSalon, Anio, Mes, "Servicio", "Tecnicos");
                DataTable dtProductos = await this.DatosProduccion_GetDatos(IdSalon, Anio, Mes, "Producto");
                ret = await this.DatosProduccion_Empleados(IdSalon, dtLavado, dtResto, dtTecnicos, dtProductos);
                foreach(GestoriaProduccion p in ret)
                {
                    foreach(DataRow f in dtLavado.Rows)
                    {
                        if (f["Codigo"].ToString().Equals(p.Codigo))
                        {
                            p.NServiciosL = int.Parse(f["N"].ToString());
                            p.ProdServiciosL = double.Parse(f["Produccion"].ToString());
                            p.ComisionServiciosL = this.DatosProduccion_Comision(f, p.ProdServiciosL, p.Paquetes.ServicioLE1, p.Paquetes.ServicioLE2, p.Paquetes.ServicioLE3, p.Paquetes.ServicioLE4);
                            break;
                        }
                    }
                    foreach (DataRow f in dtResto.Rows)
                    {
                        if (f["Codigo"].ToString().Equals(p.Codigo))
                        {
                            p.NServiciosR = int.Parse(f["N"].ToString());
                            p.ProdServiciosR = double.Parse(f["Produccion"].ToString());
                            p.ComisionServiciosR = this.DatosProduccion_Comision(f, p.ProdServiciosR, p.Paquetes.ServicioSE1, p.Paquetes.ServicioSE2, p.Paquetes.ServicioSE3, p.Paquetes.ServicioSE4);
                            break;
                        }
                    }
                    foreach (DataRow f in dtTecnicos.Rows)
                    {
                        if (f["Codigo"].ToString().Equals(p.Codigo))
                        {
                            p.NServiciosT = int.Parse(f["N"].ToString());
                            p.ProdServiciosT = double.Parse(f["Produccion"].ToString());
                            p.ComisionServiciosT = this.DatosProduccion_Comision(f, p.ProdServiciosT, p.Paquetes.ServicioTE1, p.Paquetes.ServicioTE2, p.Paquetes.ServicioTE3, p.Paquetes.ServicioTE4);
                            break;
                        }
                    }
                    foreach (DataRow f in dtProductos.Rows)
                    {
                        if (f["Codigo"].ToString().Equals(p.Codigo))
                        {
                            p.NProductos = int.Parse(f["N"].ToString());
                            p.ProdProductos = double.Parse(f["Produccion"].ToString());
                            p.ComisionProductos= this.DatosProduccion_Comision(f, p.ProdProductos, p.Paquetes.ProductoE1, p.Paquetes.ProductoE2, p.Paquetes.ProductoE3, p.Paquetes.ProductoE4);
                            break;
                        }
                    }
                    p.TotalComisiones = p.ComisionServiciosL + p.ComisionServiciosR + p.ComisionServiciosT + p.ComisionProductos;
                    p.TotalProduccion = p.ProdServiciosL + p.ProdServiciosR + p.ProdServiciosT + p.ProdProductos;
                }
                this.DatosProduccion_Guardar(IdSalon, Anio, Mes, ret);
            }
            catch { throw; }
            return ret;
        }

        private double DatosProduccion_Comision(DataRow fila, double produccion, double P1, double P2, double P3, double P4)
        {
            double ret = 0;
            try
            {
                if(produccion>=P4 && P4 != 0)
                {
                    ret = double.Parse(fila["P4"].ToString());
                }
                else
                {
                    if (produccion >= P3 && P3 != 0)
                    {
                        ret = double.Parse(fila["P3"].ToString());
                    }
                    else
                    {
                        if (produccion >= P2 && P2 != 0)
                        {
                            ret = double.Parse(fila["P2"].ToString());
                        }
                        else
                        {
                            if (produccion >= P1 && P1 != 0)
                            {
                                ret = double.Parse(fila["P1"].ToString());
                            }
                        }
                    }
                }
            }
            catch { throw; }
            return ret;
        }

        private async Task<List<GestoriaProduccion>> DatosProduccion_Empleados(int IdSalon, DataTable dtLavado, DataTable dtResto, DataTable dtTecnicos, DataTable dtproductos)
        {
            List<GestoriaProduccion> ret = new List<GestoriaProduccion>();
            try
            {
                List<string> codigos = new List<string>();
                foreach(DataRow f in dtLavado.Rows)
                {
                    codigos.Add(f["Codigo"].ToString());
                }
                foreach (DataRow f in dtResto.Rows)
                {
                    if (!codigos.Contains(f["Codigo"].ToString()))
                    {
                        codigos.Add(f["Codigo"].ToString());
                    }
                }
                foreach (DataRow f in dtTecnicos.Rows)
                {
                    if (!codigos.Contains(f["Codigo"].ToString()))
                    {
                        codigos.Add(f["Codigo"].ToString());
                    }
                }
                foreach (DataRow f in dtproductos.Rows)
                {
                    if (!codigos.Contains(f["Codigo"].ToString()))
                    {
                        codigos.Add(f["Codigo"].ToString());
                    }
                }
                if (codigos.Count > 0)
                {
                    string sql = "select u.Codigo, u.Nombre, u.Apellidos, ec.ProductosE1, ec.ProductosE2, ec.ProductosE3, ec.ProductosE4, " +
                        "ec.ServiciosLE1, ec.ServiciosLE2, ec.ServiciosLE3, ec.ServiciosLE4, " +
                        "ec.ServiciosSE1, ec.ServiciosSE2, ec.ServiciosSE3, ec.ServiciosSE4, " +
                        "ec.ServiciosTE1, ec.ServiciosTE2, ec.ServiciosTE3, ec.ServiciosTE4 " +
                        "from AspNetUsers u inner join EmpleadosComisiones ec on u.Salon = ec.Salon and u.Codigo = ec.Codigo " +
                        "where u.Salon=" + IdSalon + " and u.Codigo in (" + Utils.ListToString(codigos) + ") " +
                        "order by u.Codigo";
                    DataSet ds = await db.GetDataSet(sql, "emp");
                    foreach (DataRow f in ds.Tables["emp"].Rows)
                    {
                        GestoriaProduccion p = new GestoriaProduccion();
                        p.Codigo = f["Codigo"].ToString();
                        p.Empleado = p.Codigo + "-" + f["Nombre"].ToString() + " " + f["Apellidos"].ToString();
                        Empleado_Comisiones ec = new Empleado_Comisiones();
                        ec.ProductoE1 = double.Parse(f["ProductosE1"].ToString());
                        ec.ProductoE2 = double.Parse(f["ProductosE2"].ToString());
                        ec.ProductoE3 = double.Parse(f["ProductosE3"].ToString());
                        ec.ProductoE4 = double.Parse(f["ProductosE4"].ToString());
                        ec.ServicioLE1 = double.Parse(f["ServiciosLE1"].ToString());
                        ec.ServicioLE2 = double.Parse(f["ServiciosLE2"].ToString());
                        ec.ServicioLE3 = double.Parse(f["ServiciosLE3"].ToString());
                        ec.ServicioLE4 = double.Parse(f["ServiciosLE4"].ToString());
                        ec.ServicioSE1 = double.Parse(f["ServiciosSE1"].ToString());
                        ec.ServicioSE2 = double.Parse(f["ServiciosSE2"].ToString());
                        ec.ServicioSE3 = double.Parse(f["ServiciosSE3"].ToString());
                        ec.ServicioSE4 = double.Parse(f["ServiciosSE4"].ToString());
                        ec.ServicioTE1 = double.Parse(f["ServiciosTE1"].ToString());
                        ec.ServicioTE2 = double.Parse(f["ServiciosTE2"].ToString());
                        ec.ServicioTE3 = double.Parse(f["ServiciosTE3"].ToString());
                        ec.ServicioTE4 = double.Parse(f["ServiciosTE4"].ToString());
                        p.Paquetes = ec;
                        ret.Add(p);
                    }
                }
            }
            catch { throw; }
            return ret;
        }

        private async Task<Empleado_Comisiones> DatosProduccion_Empleados(int IdSalon, string codigo)
        {
            Empleado_Comisiones ec = new Empleado_Comisiones();
            try
            {
                string sql = "select ec.ProductosE1, ec.ProductosE2, ec.ProductosE3, ec.ProductosE4, " +
                    "ec.ProductosP1, ec.ProductosP2, ec.ProductosP3, ec.ProductosP4, " +
                    "ec.ServiciosLE1, ec.ServiciosLE2, ec.ServiciosLE3, ec.ServiciosLE4, " +
                    "ec.ServiciosLP1, ec.ServiciosLP2, ec.ServiciosLP3, ec.ServiciosLP4, " +
                    "ec.ServiciosSE1, ec.ServiciosSE2, ec.ServiciosSE3, ec.ServiciosSE4, " +
                    "ec.ServiciosSP1, ec.ServiciosSP2, ec.ServiciosSP3, ec.ServiciosSP4, " +
                    "ec.ServiciosTE1, ec.ServiciosTE2, ec.ServiciosTE3, ec.ServiciosTE4, " +
                    "ec.ServiciosTP1, ec.ServiciosTP2, ec.ServiciosTP3, ec.ServiciosTP4 " +
                    "from EmpleadosComisiones ec " +
                    "where ec.Salon=@sal and ec.Codigo=@codigo";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@sal", IdSalon);
                sc.Parameters.AddWithValue("@codigo", codigo);
                DataSet ds = await db.GetDataSet(sc, "paq");
                DataRow f = ds.Tables["paq"].Rows[0];
                ec.ProductoE1 = double.Parse(f["ProductosE1"].ToString());
                ec.ProductoE2 = double.Parse(f["ProductosE2"].ToString());
                ec.ProductoE3 = double.Parse(f["ProductosE3"].ToString());
                ec.ProductoE4 = double.Parse(f["ProductosE4"].ToString());
                ec.ProductoP1 = double.Parse(f["ProductosP1"].ToString());
                ec.ProductoP2 = double.Parse(f["ProductosP2"].ToString());
                ec.ProductoP3 = double.Parse(f["ProductosP3"].ToString());
                ec.ProductoP4 = double.Parse(f["ProductosP4"].ToString());
                ec.ServicioLE1 = double.Parse(f["ServiciosLE1"].ToString());
                ec.ServicioLE2 = double.Parse(f["ServiciosLE2"].ToString());
                ec.ServicioLE3 = double.Parse(f["ServiciosLE3"].ToString());
                ec.ServicioLE4 = double.Parse(f["ServiciosLE4"].ToString());
                ec.ServicioLP1 = double.Parse(f["ServiciosLP1"].ToString());
                ec.ServicioLP2 = double.Parse(f["ServiciosLP2"].ToString());
                ec.ServicioLP3 = double.Parse(f["ServiciosLP3"].ToString());
                ec.ServicioLP4 = double.Parse(f["ServiciosLP4"].ToString());
                ec.ServicioSE1 = double.Parse(f["ServiciosSE1"].ToString());
                ec.ServicioSE2 = double.Parse(f["ServiciosSE2"].ToString());
                ec.ServicioSE3 = double.Parse(f["ServiciosSE3"].ToString());
                ec.ServicioSE4 = double.Parse(f["ServiciosSE4"].ToString());
                ec.ServicioSP1 = double.Parse(f["ServiciosSP1"].ToString());
                ec.ServicioSP2 = double.Parse(f["ServiciosSP2"].ToString());
                ec.ServicioSP3 = double.Parse(f["ServiciosSP3"].ToString());
                ec.ServicioSP4 = double.Parse(f["ServiciosSP4"].ToString());
                ec.ServicioTE1 = double.Parse(f["ServiciosTE1"].ToString());
                ec.ServicioTE2 = double.Parse(f["ServiciosTE2"].ToString());
                ec.ServicioTE3 = double.Parse(f["ServiciosTE3"].ToString());
                ec.ServicioTE4 = double.Parse(f["ServiciosTE4"].ToString());
                ec.ServicioTP1 = double.Parse(f["ServiciosTP1"].ToString());
                ec.ServicioTP2 = double.Parse(f["ServiciosTP2"].ToString());
                ec.ServicioTP3 = double.Parse(f["ServiciosTP3"].ToString());
                ec.ServicioTP4 = double.Parse(f["ServiciosTP4"].ToString());
            }
            catch { throw; }
            return ec;
        }

        public async Task<GestoriaProduccion> DatosProduccion_Mes_Empleado(int anio, int mes, int salon, string codigo)
        {
            GestoriaProduccion p = new GestoriaProduccion();
            try
            {
                string sql = "select u.Nombre + ' ' + u.Apellidos as Empleado, p.NServiciosL, p.ServiciosL, p.ServComisionL, " +
                    "p.NServiciosR, p.ServiciosR, p.ServComisionR, p.NServiciosT, p.ServiciosT, p.ServComisionT," +
                    "p.NProductos, p.Productos, p.ProdComision, p.TotalProduccion, p.TotalComision " +
                    "from Gestoria_Produccion p " +
                    "inner join AspNetUsers u on p.Codigo = u.Codigo and p.IdSalon = u.Salon " +
                    "where p.IdSalon=@sal and p.Anio=@anio and p.Mes=@mes and p.Codigo=@codigo";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@sal", salon);
                sc.Parameters.AddWithValue("@anio", anio);
                sc.Parameters.AddWithValue("@mes", mes);
                sc.Parameters.AddWithValue("@codigo", codigo);
                DataSet ds = await db.GetDataSet(sc, "resumen");
                DataRow fila = ds.Tables["resumen"].Rows[0];
                p.Codigo = codigo;
                p.ComisionProductos = double.Parse(fila["ProdComision"].ToString());
                p.ComisionServiciosL = double.Parse(fila["ServComisionL"].ToString());
                p.ComisionServiciosR = double.Parse(fila["ServComisionR"].ToString());
                p.ComisionServiciosT = double.Parse(fila["ServComisionT"].ToString());
                p.Empleado = fila["Empleado"].ToString();
                p.NProductos = int.Parse(fila["NProductos"].ToString());
                p.NServiciosL = int.Parse(fila["NServiciosL"].ToString());
                p.NServiciosR = int.Parse(fila["NServiciosR"].ToString());
                p.NServiciosT = int.Parse(fila["NServiciosT"].ToString());
                p.ProdProductos = double.Parse(fila["Productos"].ToString());
                p.ProdServiciosL = double.Parse(fila["ServiciosL"].ToString());
                p.ProdServiciosR = double.Parse(fila["ServiciosR"].ToString());
                p.ProdServiciosT = double.Parse(fila["ServiciosT"].ToString());
                p.TotalComisiones = double.Parse(fila["TotalComision"].ToString());
                p.TotalProduccion = double.Parse(fila["TotalProduccion"].ToString());
                p.Paquetes = await this.DatosProduccion_Empleados(salon, codigo);
            }
            catch { throw; }
            return p;
        }

        public async Task<List<Ficha>> DatosProduccion_Mes_EmpleadoFichas(int anio, int mes, int salon, string codigo)
        {
            List<Ficha> ret = new List<Ficha>();
            try
            {
                string sql = "select f.Fecha, f.NFicha, c.Nombre, fl.Linea, s.Tipo, isnull(s.Grupo,'') as Grupo, fl.Descripcion, fl.Base - fl.DescuentoCant as Produccion " +
                    "from Fichas f " +
                    "inner join Clientes c on f.IdCliente = c.IdCliente and f.IdSalon = c.IdSalon " +
                    "inner join Fichas_Lineas fl on f.NFicha = fl.NFicha and f.IdSalon = fl.IdSalon " +
                    "inner join Servicios s on fl.IdServicio = s.IdServicio " +
                    "where f.Anio=@anio and f.Mes=@mes and f.IdSalon=@sal and fl.Codigo=@codigo " +
                    "order by f.Fecha, f.NFicha, fl.Linea";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@anio", anio);
                sc.Parameters.AddWithValue("@mes", mes);
                sc.Parameters.AddWithValue("@sal", salon);
                sc.Parameters.AddWithValue("@codigo", codigo);
                DataSet ds = await db.GetDataSet(sc, "lineas");
                Ficha fich = new Ficha();
                fich.NFicha = "";
                fich.Lineas = new List<Ficha_Linea>();
                for(int i=0; i <= ds.Tables["lineas"].Rows.Count - 1; i++)
                {
                    DataRow f = ds.Tables["lineas"].Rows[i];
                    if (!f["NFicha"].ToString().Equals(fich.NFicha))
                    {
                        if (fich.Lineas.Count > 0)
                        {
                            ret.Add(fich);
                        }
                        fich = new Ficha();
                        fich.NFicha = f["NFicha"].ToString();
                        fich.Lineas = new List<Ficha_Linea>();
                        fich.Fecha = DateTime.Parse(f["Fecha"].ToString());
                        fich.Cliente = f["Nombre"].ToString();
                    }
                    Ficha_Linea fl = new Ficha_Linea();
                    fl.Linea = int.Parse(f["Linea"].ToString());
                    fl.Tipo = f["Tipo"].ToString() + '/' + f["Grupo"].ToString();
                    if (fl.Tipo.EndsWith("/"))
                    {
                        fl.Tipo = fl.Tipo.Substring(0, fl.Tipo.Length - 1);
                    }
                    fl.Descripcion = f["Descripcion"].ToString();
                    fl.Base = double.Parse(f["Produccion"].ToString());
                    fich.Lineas.Add(fl);
                }
                if (fich.Lineas.Count > 0)
                {
                    ret.Add(fich);
                }
            }
            catch { throw; }
            return ret;
        }

        private async Task<DataTable> DatosProduccion_GetDatos(int IdSalon, int Anio, int Mes, string tipo, string grupo = "")
        {
            DataTable ret = null;
            try
            {
                SqlCommand sc = new SqlCommand();
                string sql = "select fl.Codigo, count(fl.Linea) as N, sum(fl.Base - fl.DescuentoCant) as Produccion, sum(fl.ComisionP1) as P1, sum(fl.ComisionP2) as P2, sum(fl.ComisionP3) as P3, sum(fl.ComisionP4) as P4 " +
                    "from Fichas f " +
                    "inner join Salones sal on f.IdSalon = sal.IdSalon " +
                    "inner join Fichas_Lineas fl on f.NFicha = fl.NFicha and f.IdSalon = fl.IdSalon " +
                    "inner join Servicios s1 on fl.IdServicio = s1.IdServicio and s1.IdEmpresa = sal.IdEmpresa " +
                    "where f.IdSalon=@sal and f.Anio=@anio and f.Mes=@mes ";
                sc.Parameters.AddWithValue("@sal", IdSalon);
                sc.Parameters.AddWithValue("@anio", Anio);
                sc.Parameters.AddWithValue("@mes", Mes);
                if (!string.IsNullOrEmpty(grupo))
                {
                    sql += "and s1.Tipo=@tipo and s1.Grupo=@grupo ";
                    sc.Parameters.AddWithValue("@tipo", tipo);
                    sc.Parameters.AddWithValue("@grupo", grupo);
                }
                else
                {
                    sql += "and s1.Tipo=@tipo ";
                    sc.Parameters.AddWithValue("@tipo", tipo);
                }
                sql += "group by fl.Codigo";
                sc.CommandText = sql;
                DataSet ds = await db.GetDataSet(sc, "prod");
                ret = ds.Tables["prod"];
            }catch { throw; }
            return ret;
        }

        private void DatosProduccion_Guardar(int IdSalon, int Anio, int Mes, List<GestoriaProduccion> datos)
        {
            try
            {
                string sql = "delete from Gestoria_Produccion where IdSalon=@sal and Anio=@anio and Mes=@mes";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@sal", IdSalon);
                sc.Parameters.AddWithValue("@anio", Anio);
                sc.Parameters.AddWithValue("@mes", Mes);
                db.Command(sc);
                sql = "Insert into Gestoria_Produccion values(@sal,@anio,@mes,@codigo,@nservl,@servl,@coml," +
                    "@nservr,@servr,@comr,@nprod,@prodp,@comp,@totalprod,@totalcom,@nservt,@servt,@comt)";
                foreach(GestoriaProduccion p in datos)
                {
                    sc = new SqlCommand(sql);
                    sc.Parameters.AddWithValue("@sal", IdSalon);
                    sc.Parameters.AddWithValue("@anio", Anio);
                    sc.Parameters.AddWithValue("@mes", Mes);
                    sc.Parameters.AddWithValue("@codigo", p.Codigo);
                    sc.Parameters.AddWithValue("@nservl", p.NServiciosL);
                    sc.Parameters.AddWithValue("@servl", p.ProdServiciosL);
                    sc.Parameters.AddWithValue("@coml", p.ComisionServiciosL);
                    sc.Parameters.AddWithValue("@nservr", p.NServiciosR);
                    sc.Parameters.AddWithValue("@servr", p.ProdServiciosR);
                    sc.Parameters.AddWithValue("@comr", p.ComisionServiciosR);
                    sc.Parameters.AddWithValue("@nprod", p.NProductos);
                    sc.Parameters.AddWithValue("@prodp", p.ProdProductos);
                    sc.Parameters.AddWithValue("@comp", p.ComisionProductos);
                    sc.Parameters.AddWithValue("@totalprod", p.TotalProduccion);
                    sc.Parameters.AddWithValue("@totalcom", p.TotalComisiones);
                    sc.Parameters.AddWithValue("@nservt", p.NServiciosT);
                    sc.Parameters.AddWithValue("@servt", p.ProdServiciosT);
                    sc.Parameters.AddWithValue("@comt", p.ComisionServiciosT);
                    db.Command(sc);
                }
            }
            catch { throw; }
        }

        public async Task<ViewModelGestoria> DatosResumen(ViewModelGestoria data, bool imprimir = false)
        {
            try
            {
                string sql = "select Cerrado, Ingresos, Gastos, IvaSoportado, IvaRepercutido, Saldo, SaldoFinal, " +
                    "IngTarjeta, IngEfectivo, IvaTarjeta, IvaEfectivo, Comisiones " +
                    "from Gestoria " +
                    "where IdSalon=@sal and Anio=@anio and Mes=@mes";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@sal", data.IdSalon);
                sc.Parameters.AddWithValue("@anio", data.Anio);
                sc.Parameters.AddWithValue("@mes", data.NMes);
                DataSet ds = await db.GetDataSet(sc, "check");
                if (ds.Tables["check"].Rows.Count==0)
                {
                    sql = "Insert into Gestoria Values(@idsalon,@anio,@mes,'False',0,0,0,0,0,0,0,0,0,0,0)";
                    sc = new SqlCommand(sql);
                    sc.Parameters.AddWithValue("@idsalon", data.IdSalon);
                    sc.Parameters.AddWithValue("@anio", data.Anio);
                    sc.Parameters.AddWithValue("@mes", data.NMes);
                    db.Command(sc);
                }
                else
                {
                    DataRow fila = ds.Tables["check"].Rows[0];
                    data.Cerrado = bool.Parse(fila["Cerrado"].ToString());
                    data.TotalIngresos = double.Parse(fila["Ingresos"].ToString());
                    data.Gastos = double.Parse(fila["Gastos"].ToString());
                    data.IvaSoportado = double.Parse(fila["IvaSoportado"].ToString());
                    data.IvaRepercutido = double.Parse(fila["IvaRepercutido"].ToString());
                    data.SaldoNeto = double.Parse(fila["Saldo"].ToString());
                    data.SaldoNetoCom = double.Parse(fila["SaldoFinal"].ToString());
                    data.Tarjeta = double.Parse(fila["IngTarjeta"].ToString());
                    data.Efectivo = double.Parse(fila["IngEfectivo"].ToString());
                    data.IvaT = double.Parse(fila["IvaTarjeta"].ToString());
                    data.IvaE = double.Parse(fila["IvaEfectivo"].ToString());
                    data.Comisiones = double.Parse(fila["Comisiones"].ToString());
                }
                if (!data.Cerrado && !imprimir)
                {
                    data.Fichas = await this.DatosFichas(data.IdSalon, data.Anio, data.NMes);
                    data.Producciones = await this.DatosProduccion(data.IdSalon, data.Anio, data.NMes);
                }
            }
            catch { throw; }
            return data;
        }

        private async Task<ViewModelGestoria> Gastos(ViewModelGestoria data)
        {
            try
            {
                string sql = "select Isnull(sum(Importe),0) as Gastos, ISNULL(sum(Iva),0) as IvaSoportado " +
                    "from Cajas c " +
                    "inner join Cajas_Gastos cg on c.NCaja = cg.NCaja and c.IdSalon = cg.IdSalon " +
                    "where c.IdSalon=@sal and c.Anio=@anio and c.Mes=@mes";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@sal", data.IdSalon);
                sc.Parameters.AddWithValue("@anio", data.Anio);
                sc.Parameters.AddWithValue("@mes", data.NMes);
                DataSet ds = await db.GetDataSet(sc, "gastos");
                data.Gastos = double.Parse(ds.Tables["gastos"].Rows[0]["Gastos"].ToString());
                data.IvaSoportado = double.Parse(ds.Tables["gastos"].Rows[0]["IvaSoportado"].ToString());
            }
            catch { throw; }
            return data;
        }

        public double RecalculoFichas(ViewModelGestoria data, IQueryable<Ficha> fichasCaja)
        {
            double ret = 0;
            try
            {
                string sql = "update Gestoria_Fichas set Incluido=@incl " +
                    "where IdSalon=@sal and Anio=@anio and Mes=@mes";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@incl", true);
                sc.Parameters.AddWithValue("@sal", data.IdSalon);
                sc.Parameters.AddWithValue("@anio", data.Anio);
                sc.Parameters.AddWithValue("@mes", data.NMes);
                db.Command(sc);
                List<string> lstExluir = new List<string>();
                var fEfectivo = fichasCaja.Where(r => r.FormaPago.Equals("Efectivo"));
                fEfectivo = fEfectivo.OrderByDynamic("Iva", DtOrderDir.Desc);
                Ficha[] lst = fEfectivo.ToArray<Ficha>();
                for(int i=0; i <= lst.Length - 1; i++)
                {
                    if (lst[i].Iva + ret <= data.Efectivo)
                    {
                        ret += lst[i].Iva;
                        lstExluir.Add(lst[i].NFicha);
                    }
                    else
                    {
                        break;
                    }
                }
                if (ret < data.Efectivo)
                {
                    fEfectivo = fEfectivo.OrderByDynamic("Iva", DtOrderDir.Asc);
                    lst = fEfectivo.ToArray<Ficha>();
                    for (int i = 0; i <= lst.Length - 1; i++)
                    {
                        if (lst[i].Iva + ret <= data.Efectivo)
                        {
                            ret += lst[i].Iva;
                            lstExluir.Add(lst[i].NFicha);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                if (lstExluir.Count > 0)
                {
                    foreach(string f in lstExluir)
                    {
                        sql = "update Gestoria_Fichas set Incluido=@incl, FichaCierre=@nfichcierre " +
                            "where IdSalon=@sal and Anio=@anio and Mes=@mes and FichaCaja=@nficha";
                        sc = new SqlCommand(sql);
                        sc.Parameters.AddWithValue("@incl", false);
                        sc.Parameters.AddWithValue("@nfichcierre", "");
                        sc.Parameters.AddWithValue("@sal", data.IdSalon);
                        sc.Parameters.AddWithValue("@anio", data.Anio);
                        sc.Parameters.AddWithValue("@mes", data.NMes);
                        sc.Parameters.AddWithValue("@nficha", f);
                        db.Command(sc);
                    }
                }
            }
            catch { throw; }
            return ret;
        }

        public async Task<ViewModelGestoria> Recuperar(ViewModelGestoria data)
        {
            try
            {
                db.OpenConnection();
                data = await this.DatosResumen(data);
                if (data.Fichas != null)
                {
                    data = await this.Gastos(data);
                    data = this.Totales(data);
                }
                db.Close();
            }
            catch             
            {
                try
                {
                    db.Close();
                }
                catch { }
                throw;
            }
            return data;
        }

        private ViewModelGestoria Totales(ViewModelGestoria data)
        {
            try
            {
                data.Tarjeta = 0;
                data.Efectivo = 0;
                data.IvaT = 0;
                data.IvaE = 0;
                data.Comisiones = 0;
                foreach (GestoriaFicha f in data.Fichas)
                {
                    if (f.FormaPago.Equals("Efectivo"))
                    {
                        data.Efectivo += f.Base;
                        data.IvaE += f.Iva;
                    }
                    else
                    {
                        data.Tarjeta += f.Base;
                        data.IvaT += f.Iva;
                    }
                }
                data.IvaRepercutido = data.IvaE + data.IvaT;
                data.TotalIngresos = data.Efectivo + data.Tarjeta;
                foreach(GestoriaProduccion p in data.Producciones)
                {
                    data.Comisiones += p.TotalComisiones;
                }
                data.SaldoNeto = data.TotalIngresos - data.Gastos + data.IvaSoportado;
                data.SaldoNetoCom = data.SaldoNeto - data.Comisiones;
                string sql = "Update Gestoria " +
                    "set Ingresos=@ing, Gastos=@gastos, IvaSoportado=@ivasop, IvaRepercutido=@ivarep, Saldo=@saldo, " +
                    "SaldoFinal=@saldof, IngTarjeta=@ingt, IngEfectivo=@inge, IvaTarjeta=@ivat, IvaEfectivo=@ivae, " +
                    "Comisiones=@com " +
                    "where IdSalon=@sal and Anio=@anio and Mes=@mes";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@ing", data.TotalIngresos);
                sc.Parameters.AddWithValue("@gastos", data.Gastos);
                sc.Parameters.AddWithValue("@ivasop", data.IvaSoportado);
                sc.Parameters.AddWithValue("@ivarep", data.IvaRepercutido);
                sc.Parameters.AddWithValue("@saldo", data.SaldoNeto);
                sc.Parameters.AddWithValue("@saldof", data.SaldoNetoCom);
                sc.Parameters.AddWithValue("@ingt", data.Tarjeta);
                sc.Parameters.AddWithValue("@inge", data.Efectivo);
                sc.Parameters.AddWithValue("@ivat", data.IvaT);
                sc.Parameters.AddWithValue("@ivae", data.IvaE);
                sc.Parameters.AddWithValue("@com", data.Comisiones);
                sc.Parameters.AddWithValue("@sal", data.IdSalon);
                sc.Parameters.AddWithValue("@anio", data.Anio);
                sc.Parameters.AddWithValue("@mes", data.NMes);
                db.Command(sc);
            }
            catch 
            {
                throw;
            }
            return data;
        }
    }
}
