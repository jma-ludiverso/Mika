using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MikaWeb.Extensions.DB;
using MikaWeb.Models;
using MikaWeb.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MikaWeb.ViewComponents
{
    public class FichasViewComponent : ViewComponent
    {

        private readonly DBConfig _dbConfig;

        public FichasViewComponent(IOptions<DBConfig> dbConf)
        {
            _dbConfig = dbConf.Value;
        }

        public async Task<IViewComponentResult> InvokeAsync(int Id)
        {
            List<ViewModelFicha> ret = new List<ViewModelFicha>();
            try
            {
                DBExtension db = new DBExtension(_dbConfig.MikaWebContextConnection);
                string sql = "select top 10 f.NFicha, f.Fecha, f.Total, us.Nombre + ' ' + us.Apellidos as Empleado, fl.Descripcion, fl.Total as TotalLinea " +
                    "from Fichas f " +
                    "inner join Fichas_Lineas fl on f.NFicha = fl.NFicha and f.IdSalon = fl.IdSalon " +
                    "inner join AspNetUsers us on us.Codigo = fl.Codigo and us.Salon = fl.IdSalon " +
                    "where f.IdCliente=@cli " +
                    "order by f.Fecha desc, f.NFicha desc, fl.Linea";
                System.Data.SqlClient.SqlCommand sc = new System.Data.SqlClient.SqlCommand(sql);
                sc.Parameters.AddWithValue("@cli", Id);
                DataSet ds = await db.GetDataSet(sc, "fichas");
                if (ds.Tables["fichas"].Rows.Count > 0)
                {
                    ViewModelFicha mf = new ViewModelFicha();
                    Ficha f = new Ficha
                    {
                        IdCliente = Id,
                        NFicha = ds.Tables["fichas"].Rows[0]["NFicha"].ToString(),
                        Fecha = DateTime.Parse(ds.Tables["fichas"].Rows[0]["Fecha"].ToString()),
                        Total = double.Parse(ds.Tables["fichas"].Rows[0]["Total"].ToString()),
                        Lineas = new List<Ficha_Linea>()
                    };
                    mf.Datos = f;
                    for (int i = 0; i <= ds.Tables["fichas"].Rows.Count - 1; i++)
                    {
                        DataRow fila = ds.Tables["fichas"].Rows[i];
                        if (!fila["NFicha"].ToString().Equals(f.NFicha))
                        {
                            ret.Add(mf);
                            mf = new ViewModelFicha();
                            f = new Ficha
                            {
                                NFicha = fila["NFicha"].ToString(),
                                Fecha = DateTime.Parse(fila["Fecha"].ToString()),
                                Total = double.Parse(fila["Total"].ToString()),
                                Lineas = new List<Ficha_Linea>()
                            };
                            mf.Datos = f;
                        }
                        Ficha_Linea fl = new Ficha_Linea
                        {
                            Empleado = fila["Empleado"].ToString(),
                            Descripcion = fila["Descripcion"].ToString(),
                            Total = double.Parse(fila["TotalLinea"].ToString())
                        };
                        f.Lineas.Add(fl);
                    }
                    ret.Add(mf);
                }
                else
                {
                    ViewModelFicha mf = new ViewModelFicha();
                    Ficha f = new Ficha
                    {
                        IdCliente = Id,
                        Lineas = new List<Ficha_Linea>()
                    };
                    mf.Datos = f;
                    ret.Add(mf);
                    ViewBag.StatusMessage = "No se han encontrado fichas para el cliente";
                }
            }
            catch (Exception ex)
            {
                ViewBag.StatusMessage = "Error: " + ex.Message;
            }
            return View(ret);
        }

    }
}
