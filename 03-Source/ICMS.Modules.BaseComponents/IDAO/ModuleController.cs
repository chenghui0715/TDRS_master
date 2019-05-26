using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICMS.Modules.BaseComponents.Commons;
using TLAgent.OpcLibrary;

namespace ICMS.Modules.BaseComponents.IDAO
{
    public class ModuleController
    {
        public KepController KepController { get; set; }
        public ModuleController(EQItem item)
        {

        }
        public virtual ExecutionResult Check(object dataParam) { return new ExecutionResult(); }

        public virtual ExecutionResult LableViewSave(object dataParam) { return new ExecutionResult { Status = true }; }

        public virtual ExecutionResult SaveSn(object dataParam) { return new ExecutionResult { Status = true }; }

        public virtual ExecutionResult CheckLogin(object dataParam) { return new ExecutionResult { Status = true }; }

    }
}
