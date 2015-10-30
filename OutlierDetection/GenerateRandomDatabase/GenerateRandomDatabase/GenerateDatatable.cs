using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Diagnostics;


namespace GenerateRandomDatabase
{
    class GenerateDatatable
    {
        private static string databaseName = "Test_DB";


        /// <summary>
        /// Execute sql command
        /// </summary>
        /// <param name="strConn"></param>
        /// <param name="DatabaseName"></param>
        /// <param name="sql"></param>
        private static void ExecuteSql(string strConn, string sql)
        {
            SqlConnection myConnection = new SqlConnection(strConn);
            SqlCommand myCommand = new SqlCommand(sql, myConnection); //
            myCommand.Connection.Open();
            myCommand.Connection.ChangeDatabase(databaseName);
            try
            {
                myCommand.ExecuteNonQuery();  //
            }
            finally
            {
                myCommand.Connection.Close();
            }
        }


        //private static void createDatabase(string strConn)
        //{
        //    conn.ConnectionString = strConn;
        //    if (conn.State != ConnectionState.Open)
        //        conn.Open();
        //    string sql = "CREATE DATABASE TestDB ON PRIMARY"
        //        + "(name = Test_DB, filename = 'D:\\Database\\Test_DB.mdf')"
        //        + "(name = Test_DB_Log, filename = 'D:\\Database\\Test_DB_Log.log')";
        //    cmd = new SqlCommand(sql, conn);
        //    try
        //    {
        //        cmd.ExecuteNonQuery();
        //    }
        //    catch(SqlException e)
        //    {
        //        Console.WriteLine(e.Message);
        //    }
        //}


        private static void createDataTable(string tableName, string strConn, int dimension)
        {
            string strCmd = String.Format("CREATE TABLE {0} ", tableName);
            strCmd += "(ID INTEGER IDENTITY(1,1) NOT NULL,";
            for (int i = 1; i <= dimension; i++)
            {
                strCmd += String.Format("A{0} INTEGER,", i);
            }
            strCmd = strCmd.Remove(strCmd.Length - 1);
            strCmd += ")";
            ExecuteSql(strConn, strCmd);
        }


        private static void insertRandomObjIntoTable(string tableName, string strConn, int nDimension)
        {
            Random ran = new Random(System.Guid.NewGuid().GetHashCode());
            int n;

            string sql = String.Format("INSERT INTO {0} VALUES(", tableName);
            for (int j = 0; j < nDimension; j++)
            {
                n = ran.Next(0, 50);
                sql += n + ",";
            }
            sql = sql.Remove(sql.Length - 1); // remove last ',' in the sql
            sql += ")";

            ExecuteSql(strConn, sql);
        }


        private static void insertOutlierObjIntoTable(string tableName, string strConn, int nDimension)
        {
            Random ran = new Random(System.Guid.NewGuid().GetHashCode());
            int n;

            string sql = String.Format("INSERT INTO {0} VALUES(", tableName);
            for (int j = 0; j < nDimension; j++)
            {
                n = ran.Next(-25, 25);
                sql += n + ",";
            }
            sql = sql.Remove(sql.Length - 1);
            sql += ")";

            ExecuteSql(strConn, sql);
        }


        public static void generateDatabase(string tableName, string strConn, int nLength, int nOutlier, int nDimension)
        {
            createDataTable(tableName, strConn, nDimension);
            for (int i = 1; i <= nLength; i++)
            {
                if (i % 50 == 0)
                {
                    insertOutlierObjIntoTable(tableName, strConn, nDimension);
                    continue;
                }
                insertRandomObjIntoTable(tableName, strConn, nDimension);
            }
        }


        static void Main(string[] args)
        {
            string strConn = "server = localhost;" +
                             "user ID = sa;" +
                             "password = smart;" +
                             "database = Test_DB;" +
                             "connect timeout = 10";
            
            int nLength = 1000, nOutlier = 50, nDimension = 75;
            string tableName = String.Format("TestTable_{0}", nDimension);
            generateDatabase(tableName, strConn, nLength, nOutlier, nDimension);

            Console.WriteLine("Done!");
        }
    }
}
