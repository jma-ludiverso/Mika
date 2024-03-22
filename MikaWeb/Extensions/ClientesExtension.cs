using MikaWeb.Extensions.DB;
using MikaWeb.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace MikaWeb.Extensions
{
    public class ClientesExtension
    {

        DBExtension db;
        SqlTransaction t = null;

        public ClientesExtension(DBExtension _db)
        {
            db = _db;
        }

        public ClientesExtension(DBExtension _db, SqlTransaction _t)
        {
            db = _db;
            t = _t;
        }

        public void BorraHistoria(int IdCliente, int IdHistoria)
        {
            try
            {
                string sql = "Delete from Clientes_Historial where IdCliente=@idcliente and IdHistoria=@idhistoria";
                System.Data.SqlClient.SqlCommand sc = new System.Data.SqlClient.SqlCommand(sql);
                sc.Parameters.AddWithValue("@idcliente", IdCliente);
                sc.Parameters.AddWithValue("@idhistoria", IdHistoria);
                db.Command(sc);
            }
            catch { throw; }
        }

        public async Task<List<Cliente_Historia>> getHistorial(int IdCliente)
        {
            List<Cliente_Historia> ret = new List<Cliente_Historia>();
            try
            {
                string sql = "Select IdHistoria, Fecha, Descripcion From Clientes_Historial Where IdCliente=@id order by Fecha Desc";
                System.Data.SqlClient.SqlCommand sc = new System.Data.SqlClient.SqlCommand(sql);
                if (t != null)
                {
                    sc.Transaction = t;
                }
                sc.Parameters.AddWithValue("@id", IdCliente);
                DataSet ds = await db.GetDataSet(sc, "historial");
                for (int i = 0; i <= ds.Tables["historial"].Rows.Count - 1; i++)
                {
                    DataRow fila = ds.Tables["historial"].Rows[i];
                    Cliente_Historia c = new Cliente_Historia();
                    c.IdHistoria = int.Parse(fila["IdHistoria"].ToString());
                    c.Fecha = DateTime.Parse(fila["Fecha"].ToString());
                    c.Descripcion = fila["Descripcion"].ToString();
                    ret.Add(c);
                }
            }
            catch { throw; }
            return ret;
        }

        public async Task<Cliente_Historia> GuardaHistoria(int IdCliente, string desc)
        {
            Cliente_Historia ret = new Cliente_Historia();
            try
            {
                string sql = "select ISNULL(Max(IdHistoria),0) + 1 From Clientes_Historial Where IdCliente=@idcliente ";
                System.Data.SqlClient.SqlCommand sc = new System.Data.SqlClient.SqlCommand(sql);
                if (t != null)
                {
                    sc.Transaction = t;
                }
                sc.Parameters.AddWithValue("@idcliente", IdCliente);
                DataSet ds = await db.GetDataSet(sc, "max");
                ret.IdHistoria = int.Parse(ds.Tables["max"].Rows[0][0].ToString());
                ret.Fecha = System.DateTime.Now;
                sql = "Insert into Clientes_Historial VALUES(@idcliente,@idhistoria,@fecha,@desc)";
                sc = new System.Data.SqlClient.SqlCommand(sql);
                if (t != null)
                {
                    sc.Transaction = t;
                }
                sc.Parameters.AddWithValue("@idcliente", IdCliente);
                sc.Parameters.AddWithValue("@idhistoria", ret.IdHistoria);
                sc.Parameters.AddWithValue("@fecha", ret.Fecha);
                sc.Parameters.AddWithValue("@desc", desc);
                db.Command(sc);
                ret.Descripcion = desc;
            }
            catch { throw; }
            return ret;
        }

    }
}
