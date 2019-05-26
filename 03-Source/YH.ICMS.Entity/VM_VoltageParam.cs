using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YH.ICMS.Entity
{
    public  class VM_VoltageParam
    {
        public int ID { get; set; }

        public int VoltageLevel { get; set; }

        public decimal PreVoltage { get; set; }

        public decimal CurVoltage { get; set; }
    }
}
