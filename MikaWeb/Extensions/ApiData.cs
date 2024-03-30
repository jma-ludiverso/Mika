using Microsoft.Data.SqlClient;
using MikaWeb.Extensions.DB;
using MikaWeb.Models.API;
using System.Data;
using System.Threading.Tasks;

namespace MikaWeb.Extensions
{
    public class ApiData
    {

        private readonly DBConfig _dbConfig;

        public ApiData(DBConfig dbConfig) 
        {
            _dbConfig = dbConfig;
        }

        public async Task<ServerData> GetServerData(int salon)
        {
            try
            {
                ServerData ret = new ServerData();
                DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
                string sql = "select e.IdEmpresa, e.Nombre, e.CIF, e.Direccion, e.CP, e.Ciudad, e.Telefono, e.Email " +
                    "from Empresas e " +
                    "inner join Salones s on s.IdEmpresa=e.IdEmpresa " +
                    "where s.IdSalon=@salon";
                System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql);
                command.Parameters.AddWithValue("@salon", salon);
                DataSet ds = await db.GetDataSet(command, "empresa");
                DataRow row = ds.Tables["empresa"].Rows[0];

                ret.DatosEmpresa = new Empresa() { IdEmpresa = int.Parse(row["IdEmpresa"].ToString()), 
                    CIF = row["CIF"].ToString(),
                    Ciudad = row["Ciudad"].ToString(),
                    CP = row["CP"].ToString(),
                    Direccion = row["Direccion"].ToString(),
                    Email = row["Email"].ToString(),
                    Nombre = row["Nombre"].ToString(),
                    Telefono = row["Telefono"].ToString()
                };

                return ret;
            }
            catch 
            {
                throw;
            }
        }
    }

}
