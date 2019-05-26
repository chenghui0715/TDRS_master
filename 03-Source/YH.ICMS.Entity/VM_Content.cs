using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YH.ICMS.Entity
{
   public class VM_Content
    {
        public string startTag { get; set; }
        public string isMulti { get; set; }
        public string illumination { get; set; }
        public string station { get; set; }
        public IList<VM_CameraParameters> CameraParameters { get; set; }

    }
}
