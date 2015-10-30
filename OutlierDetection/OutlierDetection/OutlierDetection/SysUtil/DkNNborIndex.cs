using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OutlierDetection.SysUtil
{
    public class DkNNborIndex
    {
        private int m_index;

        public int Index
        {
            get { return m_index; }
            set { m_index = value; }
        }

        private List<int> m_kNNIndex;

        public List<int> kNNIndex
        {
            get { return m_kNNIndex; }
            set { m_kNNIndex = value; }
        }

        public DkNNborIndex(int index, List<int> kNN)
        {
            this.Index = index;
            this.kNNIndex = kNN;
        }
    }
}
