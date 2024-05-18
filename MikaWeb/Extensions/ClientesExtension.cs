using MikaWeb.Extensions.DB;
using MikaWeb.Models;
using NPOI.SS.Formula.Functions;
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

        public ClientesExtension(DBExtension _db)
        {
            db = _db;
        }

        private async Task<bool> CheckClientData(Cliente data, SqlTransaction t)
        {
            try
            {
                SqlCommand sc = new SqlCommand();
                sc.Transaction = t;
                sc.Parameters.AddWithValue("@salon", data.IdSalon);
                sc.Parameters.AddWithValue("@nombre", data.Nombre);
                string sql = "Select count(1) from Clientes where IdSalon=@salon and nombre=@nombre";
                if (data.IdCliente != -1)
                {
                    sql += " and IdCliente<>@id";
                    sc.Parameters.AddWithValue("@id", data.IdCliente);
                }
                sc.CommandText = sql;
                DataSet dsCheck = await db.GetDataSet(sc, "check");
                if (int.Parse(dsCheck.Tables["check"].Rows[0][0].ToString()) != 0)
                {
                    return false;
                }else
                {
                    return true;
                }
            }
            catch
            {
                throw;
            }
        }

        public void DeleteRecordData(int IdCliente, int IdHistoria)
        {
            try
            {
                string sql = "Delete from Clientes_Historial where IdCliente=@idcliente and IdHistoria=@idhistoria";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("@idcliente", IdCliente);
                sc.Parameters.AddWithValue("@idhistoria", IdHistoria);
                db.Command(sc);
            }
            catch { throw; }
        }

        public async Task<Cliente> GetClientData(int id)
        {
            try
            {
                db.OpenConnection();
                Cliente ret = new Cliente();
                string sql = "select IdSalon, Nombre, Telefono, Email From Clientes where IdCliente=@id";
                SqlCommand sc = new SqlCommand(sql);
                sc.Parameters.AddWithValue("id", id);
                DataSet ds = await db.GetDataSet(sc, "datos");
                DataRow fila = ds.Tables["datos"].Rows[0];
                ret.IdCliente = id;
                ret.IdSalon = int.Parse(fila["IdSalon"].ToString());
                ret.Nombre = fila["Nombre"].ToString();
                ret.Telefono = fila["Telefono"].ToString();
                ret.Email = fila["Email"].ToString();
                ret.Historial = await this.GetRecordData(id);
                db.Close();
                return ret;
            }
            catch
            {
                throw;
            }
        }

        private async Task<int> GetId(SqlTransaction t)
        {
            try
            {
                string sql = "select Max(IdCliente) + 1 From Clientes ";
                SqlCommand sc = new SqlCommand(sql);
                sc.Transaction = t;
                DataSet ds = await db.GetDataSet(sc, "max");
                return int.Parse(ds.Tables["max"].Rows[0][0].ToString());
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<Cliente_Historia>> GetRecordData(int IdCliente, SqlTransaction t = null)
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

        public async Task<bool> SaveClient(Cliente data, string newRecordData = "")
        {
            SqlTransaction t = null;
            try
            { 
                t = db.GetTransaction();
                string sql = "";
                SqlCommand sc = new SqlCommand();
                sc.Transaction = t;
                if (data.IdCliente == -1)
                {
                    //si se trata de un cliente nuevo
                    bool ret = await this.CheckClientData(data, t);
                    if (!ret)
                    {
                        //si el cliente se ha dado de alta en android y coincide el nombre se pone un sufijo y se deja continuar
                        if (data.Nuevo)
                        {
                            data.Nombre += " [app Android]";
                        }
                        else
                        {
                            throw new Exception("Ya existe un cliente con el mismo nombre");
                        }                      
                    }
                    data.IdCliente = await this.GetId(t);
                    sql = "insert into Clientes Values(@IdCliente,@Idsalon,@nombre,@telefono,@email)";
                    sc.Parameters.AddWithValue("@Idsalon", data.IdSalon);
                }
                else
                {
                    bool ret = await this.CheckClientData(data, t);
                    if (!ret)
                    {
                        throw new Exception("Ya existe un cliente con el mismo nombre");
                    }
                    sql = "update Clientes Set Nombre=@nombre,Telefono=@telefono,Email=@email where IdCliente=@IdCliente";
                }
                sc.Parameters.AddWithValue("@IdCliente", data.IdCliente);
                sc.Parameters.AddWithValue("@nombre", data.Nombre);
                sc.Parameters.AddWithValue("@telefono", data.Telefono);
                sc.Parameters.AddWithValue("@email", data.Email);
                sc.CommandText = sql;
                db.Command(sc);
                if (!string.IsNullOrEmpty(newRecordData))
                {
                    Cliente_Historia hits = await this.SaveRecordData(data.IdCliente, newRecordData, t);
                }
                db.CommitTransaction(t);
                return true;
            }
            catch 
            {
                try
                {
                    if (t != null)
                    {
                        t.Rollback();
                    }
                    db.Close();
                }
                catch { };
                throw;
            }
        }

        public async Task<Cliente_Historia> SaveRecordData(int IdCliente, string desc, SqlTransaction t = null)
        {
            Cliente_Historia ret = new Cliente_Historia();
            try
            {
                string sql = "select ISNULL(Max(IdHistoria),0) + 1 From Clientes_Historial Where IdCliente=@idcliente ";
                SqlCommand sc = new SqlCommand(sql);
                if (t != null)
                {
                    sc.Transaction = t;
                }
                sc.Parameters.AddWithValue("@idcliente", IdCliente);
                DataSet ds = await db.GetDataSet(sc, "max");
                ret.IdHistoria = int.Parse(ds.Tables["max"].Rows[0][0].ToString());
                ret.Fecha = System.DateTime.Now;
                sql = "Insert into Clientes_Historial VALUES(@idcliente,@idhistoria,@fecha,@desc)";
                sc = new SqlCommand(sql);
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
