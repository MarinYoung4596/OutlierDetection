/********************************************************************
	created:	2010/08/27
	project:    HYHX
	filename: 	\SysUtil\DStatusPacket.cs
	file ext:	cs
	author:		liukebin2006@gmail.com
	purpose:	Status packet definition
*********************************************************************/



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;



namespace OutlierDetection.SysUtil
{
    public class DStatusPacket
    {
        /// <summary>
        /// ID
        /// </summary>
        private int m_ID;

        public int ID
        {
            get { return m_ID; }
            set { m_ID = value; }
        }


        /// <summary>
        /// attributes List
        /// </summary>
        private List<int> m_Attributes;

        public List<int> Attributes
        {
            get { return m_Attributes; }
            set { m_Attributes = value; }
        }


        /// <summary>
        /// Default constructor
        /// </summary>
        public DStatusPacket()
        {
            this.ID = -1;
            this.Attributes = new List<int>();
        }


        /// <summary>
        ///  overload operator -
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns>result</returns>
        public static DStatusPacket operator -(DStatusPacket A, DStatusPacket B)
        {
            DStatusPacket result = new DStatusPacket();
            
            for (int i = 0; i < A.Attributes.Count(); i++ )
            {
                result.Attributes.Add(A.Attributes[i] - B.Attributes[i]);
            }
            return result;
        }
    }
    

    public class StatusPacketSortByTimeComparer : IComparer<DStatusPacket>
    {

        #region IComparer<DStatusPacket> 成员

        public int Compare(DStatusPacket A, DStatusPacket B)
        {
            if (A.ID != B.ID)
            {
                throw new Exception("Only packets with the same ID are comparable!");
            }
            else
            {
                return A.ID.CompareTo(B.ID);
            }  
        }

        #endregion
    }

    public class DStatus : DStatusPacket
    {
        /// <summary>
        /// index in the database
        /// </summary>
        private int m_index;

        public int Index
        {
            get { return m_index; }
            set { m_index = value; }
        }

    }
}
