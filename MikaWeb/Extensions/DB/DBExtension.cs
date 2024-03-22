using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace MikaWeb.Extensions.DB
{
    public class DBExtension
    {

        string cn = "";
        SqlConnection _obCon = null;

        public DBExtension(string _cn)
        {
            cn = _cn;
        }

        public void Close()
        {
            try
            {
                if (_obCon != null)
                {
                    if (_obCon.State == ConnectionState.Open)
                    {
                        _obCon.Close();
                        _obCon = null;
                    }
                }
            }
            catch { }
        }

        public int Command(string sql, int timeout = 0)
        {
            int ret = 0;
            try
            {
                SqlConnection obCon = null;
                SqlCommand obCom = null;
                if (_obCon != null)
                {
                    obCom = new SqlCommand(sql, _obCon);
                }
                else
                {
                    obCon = new SqlConnection(cn);
                    obCom = new SqlCommand(sql, obCon);
                    obCon.Open();
                }
                if (timeout != 0)
                {
                    obCom.CommandTimeout = timeout;
                }
                ret = obCom.ExecuteNonQuery();
                if (obCon != null)
                {
                    obCon.Close();
                }
            }
            catch { throw; }
            return ret;
        }

        public int Command(string sql, SqlTransaction t, int timeout = 0)
        {
            int ret = 0;
            try
            {
                SqlCommand obCom = new SqlCommand(sql, _obCon);
                obCom.Transaction = t;
                if (timeout != 0)
                {
                    obCom.CommandTimeout = timeout;
                }
                ret = obCom.ExecuteNonQuery();
            }
            catch { throw; }
            return ret;
        }

        public int Command(SqlCommand obCom, int timeout = 0)
        {
            int ret = 0;
            try
            {
                SqlConnection obCon = null;
                if (_obCon != null)
                {
                    obCom.Connection = _obCon;
                }
                else
                {
                    obCon = new SqlConnection(cn);
                    obCom.Connection = obCon;
                    obCon.Open();
                }
                if (timeout != 0)
                {
                    obCom.CommandTimeout = timeout;
                }
                ret = obCom.ExecuteNonQuery();
                if (obCon != null)
                {
                    obCon.Close();
                }
            }
            catch { throw; }
            return ret;
        }

        public void CommitTransaction(SqlTransaction t)
        {
            try
            {
                t.Commit();
                _obCon.Close();
                _obCon = null;
            }
            catch { throw; }
        }

        public async Task<DataSet> GetDataSet(string sql, string tabla = "tabla1", int timeout = 0)
        {
            DataSet ret = new DataSet();
            try
            {
                SqlConnection obCon = null;
                SqlDataAdapter Ad = null;
                if (timeout == 0)
                {
                    if (_obCon == null)
                    {
                        Ad = new SqlDataAdapter(sql, cn);
                    }
                    else
                    {
                        Ad = new SqlDataAdapter(sql, _obCon);
                    }
                }
                else
                {
                    SqlCommand obCom = null;
                    if (_obCon == null)
                    {
                        obCon = new SqlConnection(cn);
                        obCom = new SqlCommand(sql, obCon);
                    }
                    else
                    {
                        obCom = new SqlCommand(sql, _obCon);
                    }
                    obCom.CommandTimeout = timeout;
                    Ad = new SqlDataAdapter(obCom);
                }
                await Task.Run(() => Ad.Fill(ret, tabla));
                if (obCon != null)
                {
                    obCon.Close();
                }
            }
            catch { throw; }
            return ret;
        }

        public DataSet GetDataSet(string sql, string tabla, SqlTransaction t, int timeOut = 0)
        {
            DataSet ret = new DataSet();
            try
            {
                SqlDataAdapter Ad = null;
                SqlCommand obCom = new SqlCommand(sql, _obCon);
                obCom.Transaction = t;
                obCom.CommandTimeout = timeOut;
                Ad = new SqlDataAdapter(obCom);
                Ad.Fill(ret, tabla);
            }
            catch { throw; }
            return ret;
        }

        public async Task<DataSet> GetDataSet(SqlCommand obCom, string tabla)
        {
            DataSet ret = new DataSet();
            try
            {
                if (_obCon == null)
                {
                    obCom.Connection = new SqlConnection(cn);
                }
                else
                {
                    obCom.Connection = _obCon;
                }
                SqlDataAdapter Ad = new SqlDataAdapter(obCom);
                await Task.Run(() => Ad.Fill(ret, tabla));
            }
            catch { throw; }
            return ret;
        }

        public SqlTransaction GetTransaction()
        {
            if(_obCon == null)
            {
                _obCon = new SqlConnection(cn);
                _obCon.Open();
            }
            return _obCon.BeginTransaction();
        }

        public void OpenConnection()
        {
            _obCon = new SqlConnection(cn);
            _obCon.Open();
        }


    }
}