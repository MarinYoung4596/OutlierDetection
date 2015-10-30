/********************************************************************
	created:	2010/09/09
	project:    HYHX
	filename: 	\SQLServerDataAccess\SQLServerDataProvider.cs
	file ext:	cs
	author:		machine.miao@gmail.com
	purpose:	Provide data for the application
*********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using OutlierDetection.SysUtil;


namespace OutlierDetection.SQLServerDataAccess
{
    /// <summary>
    /// Class to provide data for others
    /// </summary>
    class SQLServerDataProvider
    {
        //用于记录取数据库的长度
        private int length;

        //存储所有的点集合
        private List<DStatusPacket> pointsList;


        /// <summary>
        /// A private constructor.
        /// This class cannot be instantiated.
        /// </summary>
        public SQLServerDataProvider(string table_name, int length)
        {
            this.length = length;
            this.FetchStatusPackets(table_name, length);
        }


        /// <summary>
        /// Fetches the status packets.
        /// </summary>
        /// <param name="id">package id</param>
        /// <returns>Collection of status packets</returns>
        public void FetchStatusPackets(string table_name, int nLength)
        {
            if (pointsList != null) 
                pointsList.Clear();
            DataSet ds = this.FetchRawData(table_name, nLength);
            this.pointsList =  SQLServerDBConvert.RawDataToDStatusPacket(ds).ToList();
        }


        /// <summary>
        /// Fetches the raw data.
        /// <param name="table_name">Name of the table.</param>
        /// <returns>Raw dataset</returns>
        private DataSet FetchRawData(string table_name, int nLength)
        {
            string p_Sql = string.Format("SELECT TOP {1} * FROM {0} ORDER BY ID", table_name, nLength);
            return SQLServerDBUtil.ExecuteQuery(p_Sql, null, table_name);
        }


        /// <summary>
        /// get data by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public DStatusPacket getDataByID(int index)
        {
            return this.pointsList[index];
        }


        /// <summary>
        /// get pointsList
        /// </summary>
        /// <returns></returns>
        public List<DStatusPacket> getPointsList()
        {
            return pointsList.ToList();
        }
    }

    enum PacketType
    {
        SENSINGPACKET,
        NEIGOBORPACKET,
        STATUSPACKET,
        MOBILEPACKET
    }
}
