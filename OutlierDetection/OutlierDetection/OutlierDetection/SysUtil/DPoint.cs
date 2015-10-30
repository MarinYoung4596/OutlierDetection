using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;



namespace OutlierDetection.SysUtil
{
    public class DPoint : IComparable
    {
        /// <summary>
        /// Point ID
        /// </summary>
        private int m_ID;

        public int ID
        {
            get { return m_ID; }
            set { m_ID = value; }
        }


        /// <summary>
        /// Point value
        /// </summary>
        private double m_Value;

        public double Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }


        /// <summary>
        /// Default Construction Method
        /// </summary>
        public DPoint()
        {
            this.ID = -1;
            this.Value = -1;
        }


        /// <summary>
        /// Construction Method
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Value"></param>
        public DPoint(int ID, double Value)
        {
            this.ID = ID;
            this.Value = Value;
        }

        #region IComparable 成员
        public int CompareTo(Object other)
        {
            if (!(other is DPoint)) throw new ArgumentException("Argument not a DPoint", "right");

            DPoint rightOther = (DPoint)other;

            return CompareTo(rightOther);
        }

        public int CompareTo(DPoint other)
        {
            return this.Value.CompareTo(other.Value);
        }
        #endregion
    }
}
