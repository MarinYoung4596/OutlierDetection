/********************************************************************
	created:	2010/09/09
	project:    HYHX
	filename: 	\SQLServerDataAccess\SQLServerDBConvert.cs
	file ext:	cs
	author:		machine.miao@gmail.com
	purpose:	Data access
*********************************************************************/



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using OutlierDetection.SysUtil;

namespace OutlierDetection.SQLServerDataAccess
{
    class SQLServerDBConvert
    {
        /// <summary>
        /// Private constructor.
        /// This class cannot be instantiated.
        /// </summary>
        private SQLServerDBConvert()
        {
        }

        // private static double ErrorReadingDouble = -1.0;
        private static int ErrorReadingInt = -1;

        // tableName
        private static string tableName = Program.tableName;



        /// <summary>
        /// Parse statusData table
        /// </summary>
        /// <param name="p_Ds">Dataset</param>
        /// <returns>Parsing results</returns>
        public static List<DStatusPacket> RawDataToDStatusPacket(DataSet p_Ds)
        {
            // Resulting list
            List<DStatusPacket> packets = new List<DStatusPacket>();
            // Check if table exists or if table is empty
            if (p_Ds.Tables.Contains(tableName) == false || p_Ds.Tables[tableName].Rows.Count < 1)
            {
                return packets;
            }

            int Dimension = p_Ds.Tables[tableName].Columns.Count;

            // Convert each row in table "Neighbor" to a "DNeighborPacket" structure
            foreach (DataRow row in p_Ds.Tables[tableName].Rows)
            {
                try
                {
                    // Create a new DStatusPacket
                    DStatusPacket status_packet = new DStatusPacket();

                    // get the value of ID
                    int p_ID = int.Parse(row["ID"].ToString());
                    // add to the object attributes
                    status_packet.ID = p_ID;

                    // Convert every elements in the attributes
                    for(int i = 1; i < Dimension; i++)
                    {
                        int p_Ai;

                        // get the value of each element of attributes
                        if (DBNull.Value.Equals(row[i]))
                        {
                            p_Ai = ErrorReadingInt;
                        }
                        else
                        {
                            p_Ai = int.Parse(row[i].ToString());
                        }
                        // add to the structure
                        status_packet.Attributes.Add(p_Ai);
                    }
                   
                    // Add it to results
                    packets.Add(status_packet);
                }
                catch (Exception e)
                {
                    //TODO
                    //SystemLog.Log("SQLServerDBConvert", "RawDataToDStatusPacket", e.Message, null);
                }
            }

            return packets;
        }
    }
}
