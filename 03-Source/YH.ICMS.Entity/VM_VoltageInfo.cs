using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YH.ICMS.Entity
{
    public class VM_VoltageInfo
    {
        public string V0 { get; set; }
        public string V1 { get; set; }

        public string V2 { get; set; }

        public string V3 { get; set; }
        public string V4 { get; set; }
        public string V5 { get; set; }
        public string V6 { get; set; }
        public string V7 { get; set; }
        

    }

    public class VM_VoltageCheckedInfo
    {
        public bool FlagV0 { get; set; }
        public bool FlagV1 { get; set; }
            
        public bool FlagV2 { get; set; }
               
        public bool FlagV3 { get; set; }
        public bool FlagV4 { get; set; }
        public bool FlagV5 { get; set; }
        public bool FlagV6 { get; set; }
        public bool FlagV7 { get; set; }


    }
}
