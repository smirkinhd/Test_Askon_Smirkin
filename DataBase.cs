using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace Test_Askon_Smirkin
{
    internal class DataBase
    {
        SqlConnection conn = new SqlConnection (@"Data Source=DESKTOP-PIOVIIS\SQLEXPRESS; Initial Catalog = askon_test;Integrated Security=True; ");

        public void openConnection()
        {
            if(conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }
        }

        public void closeConnection()
        {
            if (conn.State == System.Data.ConnectionState.Open)
            {
                conn.Close();
            }
        }

        public SqlConnection getConnection()
        {
            return conn;
        }

    }
}
