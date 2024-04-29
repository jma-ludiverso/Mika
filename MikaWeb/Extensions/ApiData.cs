using MikaWeb.Extensions.DB;
using MikaWeb.Models.API;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using static NPOI.HSSF.Util.HSSFColor;

namespace MikaWeb.Extensions
{
    public class ApiData
    {

        private readonly DBConfig _dbConfig;

        public ApiData(DBConfig dbConfig) 
        {
            _dbConfig = dbConfig;
        }

        private async Task<List<Models.Cliente>> GetClientes(DBExtension db, int salon)
        {
            try
            {
                List<Models.Cliente> ret = new List<Models.Cliente>();
                string sql = "SELECT IdCliente, Nombre, ISNULL(Telefono,'') as Telefono, IsNull(Email,'') as Email " +
                    "from Clientes " +
                    "where IdSalon=@salon";
                System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql);
                command.Parameters.AddWithValue("@salon", salon);
                DataSet ds = await db.GetDataSet(command, "clientes");
                foreach (DataRow row in ds.Tables["clientes"].Rows)
                {
                    ret.Add(new Models.Cliente()
                    {
                        IdSalon = salon,
                        IdCliente = int.Parse(row["IdCliente"].ToString()),
                        Nuevo = false,
                        Nombre = row["Nombre"].ToString(),
                        Email = row["Email"].ToString(),
                        Telefono = row["Telefono"].ToString(),
                        Historial = await this.GetClientesHistorial(db, int.Parse(row["IdCliente"].ToString()))
                    });
                }
                return ret;
            }
            catch
            {
                throw;
            }
        }

        private async Task<List<Models.Cliente_Historia>> GetClientesHistorial(DBExtension db, int cliente)
        {
            try
            {
                List<Models.Cliente_Historia> ret = new List<Models.Cliente_Historia>();
                string sql = "Select IdHistoria, Fecha, Descripcion from Clientes_Historial where IdCliente=@cliente";
                System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql);
                command.Parameters.AddWithValue("@cliente", cliente);
                DataSet ds = await db.GetDataSet(command, "historial");
                foreach (DataRow row in ds.Tables["historial"].Rows)
                {
                    ret.Add(new Models.Cliente_Historia()
                    {
                        IdHistoria = int.Parse(row["IdHistoria"].ToString()), 
                        Fecha = DateTime.Parse(row["Fecha"].ToString()),
                        Descripcion = row["Descripcion"].ToString(), 
                        Nueva = false
                    });
                }
                return ret;
            }
            catch 
            {
                throw;
            }
        }

        private async Task<Salon> GetEmpleadosData(DBExtension db, Salon data, string userId)
        {
            try
            {
                string sql = "SELECT Id,UserName,Email,PhoneNumber,Activo,Apellidos,Nombre,IsAdmin,Codigo " +
                    "FROM AspNetUsers " +
                    "WHERE Salon=@salon and Id<>@id";
                System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql);
                command.Parameters.AddWithValue("@salon", data.Id);
                command.Parameters.AddWithValue("@id", userId);
                DataSet ds = await db.GetDataSet(command, "empleados");
                foreach(DataRow row in ds.Tables["empleados"].Rows)
                {
                    data.Empleados.Add(new Models.Empleado()
                    {
                        Activo = bool.Parse(row["Activo"].ToString()),
                        Administrador = bool.Parse(row["IsAdmin"].ToString()),
                        Apellidos = row["Apellidos"].ToString(), 
                        Codigo = row["Codigo"].ToString(),
                        Email = row["Email"].ToString(),
                        Id = row["Id"].ToString(),
                        Nombre = row["Nombre"].ToString(),
                        PhoneNumber = row["PhoneNumber"].ToString(),
                        Salon = data.Id,
                        UserName = row["UserName"].ToString()
                    });
                }
                sql = "SELECT Codigo,ProductosE1,ProductosE2,ProductosE3,ProductosE4,ProductosP1,ProductosP2,ProductosP3,ProductosP4," +
                    "ServiciosLE1,ServiciosLE2,ServiciosLE3,ServiciosLE4,ServiciosLP1,ServiciosLP2,ServiciosLP3,ServiciosLP4," +
                    "ServiciosSE1,ServiciosSE2,ServiciosSE3,ServiciosSE4,ServiciosSP1,ServiciosSP2,ServiciosSP3,ServiciosSP4," +
                    "ServiciosTE1,ServiciosTE2,ServiciosTE3,ServiciosTE4,ServiciosTP1,ServiciosTP2,ServiciosTP3,ServiciosTP4 " +
                    "FROM EmpleadosComisiones " +
                    "where Salon=@salon";
                command = new System.Data.SqlClient.SqlCommand(sql);
                command.Parameters.AddWithValue("@salon", data.Id);
                ds = await db.GetDataSet(command, "comisiones");
                foreach (DataRow row in ds.Tables["comisiones"].Rows)
                {
                    data.EmpleadosComisiones.Add(new Models.Empleado_Comisiones()
                    {
                        Codigo = row["Codigo"].ToString(),
                        IdSalon = data.Id,
                        ProductoE1 = double.Parse(row["ProductosE1"].ToString()),
                        ProductoE2 = double.Parse(row["ProductosE2"].ToString()),
                        ProductoE3 = double.Parse(row["ProductosE3"].ToString()),
                        ProductoE4 = double.Parse(row["ProductosE4"].ToString()),
                        ProductoP1 = double.Parse(row["ProductosP1"].ToString()),
                        ProductoP2 = double.Parse(row["ProductosP2"].ToString()),
                        ProductoP3 = double.Parse(row["ProductosP3"].ToString()),
                        ProductoP4 = double.Parse(row["ProductosP4"].ToString()),
                        ServicioLE1 = double.Parse(row["ServiciosLE1"].ToString()),
                        ServicioLE2 = double.Parse(row["ServiciosLE2"].ToString()),
                        ServicioLE3 = double.Parse(row["ServiciosLE3"].ToString()),
                        ServicioLE4 = double.Parse(row["ServiciosLE4"].ToString()),
                        ServicioLP1 = double.Parse(row["ServiciosLP1"].ToString()),
                        ServicioLP2 = double.Parse(row["ServiciosLP2"].ToString()),
                        ServicioLP3 = double.Parse(row["ServiciosLP3"].ToString()),
                        ServicioLP4 = double.Parse(row["ServiciosLP4"].ToString()),
                        ServicioSE1 = double.Parse(row["ServiciosSE1"].ToString()),
                        ServicioSE2 = double.Parse(row["ServiciosSE2"].ToString()),
                        ServicioSE3 = double.Parse(row["ServiciosSE3"].ToString()),
                        ServicioSE4 = double.Parse(row["ServiciosSE4"].ToString()),
                        ServicioSP1 = double.Parse(row["ServiciosSP1"].ToString()),
                        ServicioSP2 = double.Parse(row["ServiciosSP2"].ToString()),
                        ServicioSP3 = double.Parse(row["ServiciosSP3"].ToString()),
                        ServicioSP4 = double.Parse(row["ServiciosSP4"].ToString()),
                        ServicioTE1 = double.Parse(row["ServiciosTE1"].ToString()),
                        ServicioTE2 = double.Parse(row["ServiciosTE2"].ToString()),
                        ServicioTE3 = double.Parse(row["ServiciosTE3"].ToString()),
                        ServicioTE4 = double.Parse(row["ServiciosTE4"].ToString()),
                        ServicioTP1 = double.Parse(row["ServiciosTP1"].ToString()),
                        ServicioTP2 = double.Parse(row["ServiciosTP2"].ToString()),
                        ServicioTP3 = double.Parse(row["ServiciosTP3"].ToString()),
                        ServicioTP4 = double.Parse(row["ServiciosTP4"].ToString()), 
                        Comisiones = await this.GetServiciosComisiones(db, row["Codigo"].ToString())
                    });
                }
                return data;
            }
            catch
            {
                throw;
            }
        }

        public async Task<ServerData> GetServerData(int salon, string userId)
        {
            DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
            try
            {
                ServerData ret = new ServerData();
                db.OpenConnection();
                string sql = "select e.IdEmpresa, e.Nombre, e.CIF, e.Direccion, e.CP, e.Ciudad, e.Telefono, e.Email, " +
                    "s.Nombre as NombreSalon, s.Direccion as DireccionSalon, s.Telefono as TelefonoSalon " +
                    "from Empresas e " +
                    "inner join Salones s on s.IdEmpresa=e.IdEmpresa " +
                    "where s.IdSalon=@salon";
                System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql);
                command.Parameters.AddWithValue("@salon", salon);
                DataSet ds = await db.GetDataSet(command, "empresa");
                DataRow row = ds.Tables["empresa"].Rows[0];

                ret.DatosEmpresa = new Empresa() { 
                    IdEmpresa = int.Parse(row["IdEmpresa"].ToString()), 
                    CIF = row["CIF"].ToString(),
                    Ciudad = row["Ciudad"].ToString(),
                    CP = row["CP"].ToString(),
                    Direccion = row["Direccion"].ToString(),
                    Email = row["Email"].ToString(),
                    Nombre = row["Nombre"].ToString(),
                    Telefono = row["Telefono"].ToString()
                };

                ret.DatosSalon = new Salon()
                {
                    Id = salon,
                    IdEmpresa = ret.DatosEmpresa.IdEmpresa,
                    Nombre = row["NombreSalon"].ToString(),
                    Direccion = row["DireccionSalon"].ToString(),
                    Telefono = row["Telefono"].ToString(),
                    Empleados = new System.Collections.Generic.List<Models.Empleado>(), 
                    EmpleadosComisiones = new System.Collections.Generic.List<Models.Empleado_Comisiones>()
                };
                ret.DatosSalon = await this.GetEmpleadosData(db, ret.DatosSalon, userId);
                ret.ListaClientes = await this.GetClientes(db, salon);
                ret.ListaServicios = await this.GetServicios(db, ret.DatosEmpresa.IdEmpresa);
                db.Close();
                return ret;
            }
            catch 
            {
                try
                {
                    db.Close();
                }catch { }
                throw;
            }
        }

        private async Task<List<Models.Servicio>> GetServicios(DBExtension db, int empresa)
        {
            try
            {
                List<Models.Servicio> ret = new List<Models.Servicio>();
                string sql = "SELECT IdServicio, Codigo, Tipo, ISNULL(Grupo,'') As Grupo, Nombre, Precio, IvaPorc, IvaCant, PVP " +
                    "from Servicios " +
                    "where IdEmpresa=@empresa and Activo=@activo ";
                System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql);
                command.Parameters.AddWithValue("@empresa", empresa);
                command.Parameters.AddWithValue("@activo", true);
                DataSet ds = await db.GetDataSet(command, "servicios");
                foreach (DataRow row in ds.Tables["servicios"].Rows)
                {
                    ret.Add(new Models.Servicio()
                    {
                        IdEmpresa = empresa,
                        Activo = true,
                        IdServicio = int.Parse(row["IdServicio"].ToString()),
                        Codigo = row["Codigo"].ToString(),
                        Tipo = row["Tipo"].ToString(),
                        Grupo = row["Grupo"].ToString(),
                        Nombre = row["Nombre"].ToString(),
                        Precio = double.Parse(row["Precio"].ToString()),
                        IvaPorc = double.Parse(row["IvaPorc"].ToString()),
                        IvaCant = double.Parse(row["IvaCant"].ToString()),
                        PVP = double.Parse(row["PVP"].ToString())
                    });
                }
                return ret;
            }
            catch
            {
                throw;
            }
        }

        private async Task<List<Models.Servicio_Comision>> GetServiciosComisiones(DBExtension db, string codigo)
        {
            try
            {
                List<Models.Servicio_Comision> ret = new List<Models.Servicio_Comision>();
                string sql = "select IdServicio, ComisionP1, ComisionP2, ComisionP3, ComisionP4 " +
                    "from EmpleadosServicios " +
                    "where Codigo=@codigo";
                System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql);
                command.Parameters.AddWithValue("@codigo", codigo);
                DataSet ds = await db.GetDataSet(command, "comisiones");
                foreach (DataRow row in ds.Tables["comisiones"].Rows)
                {
                    ret.Add(new Models.Servicio_Comision()
                    {
                        IdServicio = int.Parse(row["IdServicio"].ToString()),
                        ComisionP1 = double.Parse(row["ComisionP1"].ToString()),
                        ComisionP2 = double.Parse(row["ComisionP2"].ToString()),
                        ComisionP3 = double.Parse(row["ComisionP3"].ToString()),
                        ComisionP4 = double.Parse(row["ComisionP4"].ToString())
                    });
                }
                return ret;
            }
            catch
            {
                throw;
            }
        }

    }

}
