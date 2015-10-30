/********************************************************************
	created:	2010/09/03
	project:    HYHX
	filename: 	\SQLServerDataAccess\SQLServerDBUtil.cs
	file ext:	cs
	author:		machine.miao@gmail.com
	purpose:	Data access
*********************************************************************/



using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
//using SensorNetworkManagement.SysUtil;



namespace OutlierDetection.SQLServerDataAccess
{
    /// <summary>
    /// DB
    /// </summary>
    class SQLServerDBUtil
    {
        public static String m_ServerString = "localhost";

        public System.String ServerString
        {
            get { return m_ServerString; }
            set { m_ServerString = value; }
        }


        //Local host
        public static String m_IdString = "sa";

        /// <summary>
        /// ID string
        /// </summary>
        public System.String IdString
        {
            get { return m_IdString; }
            set { m_IdString = value; }
        }


        //Local Host
        public static String m_PassString = "smart";

        /// <summary>
        /// Pass string
        /// </summary>
        public System.String PassString
        {
            get { return m_PassString; }
            set { m_PassString = value; }
        }


        public static String m_DbString = "Test_DB";

        /// <summary>
        /// DB string
        /// </summary>
        public System.String DbString
        {
            get { return m_DbString; }
            set { m_DbString = value; }
        }


        /// <summary>
        /// Construction method
        /// </summary>
        private SQLServerDBUtil()
        {

        }


        /// <summary>
        /// Get connection string
        /// </summary>
        /// <returns></returns>
        public static string GetConnectionString()
        {
            return "server=" + m_ServerString +
                      ";user id=" + m_IdString +
                      "; password=" + m_PassString +
                        "; database=" + m_DbString +
                          "; connect timeout=10";
            //"; pooling=false;charset=utf8";
        }


        #region Query

        /// <summary>
        /// Query DB
        /// Return dataset
        /// </summary>
        /// <param name="p_Sql">SQL</param>
        /// <param name="p_Parameters">Parameters</param>
        /// <param name="p_Name">DataTable Name</param>
        /// <returns>Data set</returns>
        public static DataSet ExecuteQuery(string p_Sql, SqlParameter[] p_Parameters, string p_Name)
        {

            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();

                    SqlDataAdapter da = new SqlDataAdapter(p_Sql, connection);
                    if (p_Parameters != null)
                    {
                        da.SelectCommand.Parameters.AddRange(p_Parameters);
                    }
                    da.Fill(ds, p_Name);

                    connection.Close();
                }
                catch (Exception e)
                {
                    //TODO
                    Console.WriteLine("SQLServerDBUtil\t" + "ExecuteQuery\t" + e.Message);
                }
                return ds;
            }
        }
        #endregion
    }
}
