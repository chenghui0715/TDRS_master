using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.ComponentModel.Design.Serialization;
using System.Runtime.Serialization;
using System.Reflection;
using ICMS.Modules.BaseComponents.Beans;

namespace ICMS.Modules.BaseComponents
{
    [Serializable]
    public class ExecutionResult
    {
        public bool IsAlive { get; set; }

        public virtual string StationName { get; set; }

        public virtual string Sn { get; set; }

        public virtual string ProductType { get; set; }

        public virtual int ErrorFlag { get; set; }

        public virtual string Message { get; set; }

        public virtual bool Status { get; set; }

        public virtual object Anything { get; set; }
    }
}
