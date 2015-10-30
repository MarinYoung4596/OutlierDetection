using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OutlierAnalysis.SysUtil
{
    public class DPoint : IComparable
    {
        private int m_MoteID;

        public int MoteID
        {
            set { m_MoteID = value; }
            get { return m_MoteID; }
        }

		
        private int m_NumLOF;

        public int NumLOF
        {
            set { m_NumLOF = value; }
            get { return m_NumLOF; }
        }

		
        private int m_NumFastABOD;

        public int NumFastABOD
        {
            set { m_NumFastABOD = value; }
            get { return m_NumFastABOD; }
        }

		private int m_NumNoise;
		
		public int NumNoise
		{
			set { m_NumNoise = value; }
			get { return m_NumNoise; }
		}
		
		private int m_Sum;
		
		public int Sum
		{
			set { m_Sum = value; }
			get { return m_Sum; }
		}
		
        public DPoint()
        {
            this.MoteID = -1;
            this.NumLOF = 0;
            this.NumFastABOD = 0;
			this.NumNoise = 0;
			this.Sum = 0;
        }
		
        public void sumNum()
        {
            this.Sum = this.NumLOF + this.NumFastABOD;// +this.NumNoise;
        }
		
		public int CompareTo(Object other)
        {
            if (!(other is DPoint)) throw new ArgumentException("Argument not a DPoint", "right");

            DPoint rightOther = (DPoint)other;

            return CompareTo(rightOther);
        }

        public int CompareTo(DPoint other)
        {
            return this.Sum.CompareTo(other.Sum);
        }
    }
}
