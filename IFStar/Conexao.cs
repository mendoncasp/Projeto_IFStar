using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace IFStar
{
    public class Conexao
    {
        SqlConnection conn = new SqlConnection();
        public string connectionString = @"Data Source=DESKTOP-QBCN7D6\SQLEXPRESS;Initial Catalog=dbIFStar;Integrated Security=True";

        //Construtor
        public Conexao()
        {
            conn.ConnectionString = @"Data Source=DESKTOP-QBCN7D6\SQLEXPRESS;Initial Catalog=dbIFStar;Integrated Security=True";
        }

        //Método Conectar
        public SqlConnection Conectar()
        {
            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }

            return conn;
        }

        //Método Desconectar
        public void Desconectar()
        {
            if (conn.State == System.Data.ConnectionState.Open)
            {
                conn.Close();
            }
        }
    }
}