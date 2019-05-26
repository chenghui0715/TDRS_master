using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YH.ICMS.Entity
{
    public class VM_MsCountInfo
    {
        private int _MsOneCount;
        public int MsOneCount
        {
            get { return _MsOneCount; }
            set { _MsOneCount = value; }
        }

        private int _MsTwoCount;
        public int MsTwoCount
        {
            get { return _MsTwoCount; }
            set { _MsTwoCount = value; }
        }

        private int _MsThreeCount;
        public int MsThreeCount
        {
            get { return _MsThreeCount; }
            set { _MsThreeCount = value; }
        }

        private int _MsFourCount;
        public int MsFourCount
        {
            get { return _MsFourCount; }
            set { _MsFourCount = value; }
        }

    }

}
