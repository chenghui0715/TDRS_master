using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ICMS.Modules.BaseComponents.Beans
{
    public class ModuleEntity
    {
        public virtual string Id { set; get; }
        public virtual string LineName { set; get; }
        public virtual string StationCode { set; get; }
        public virtual string StationName { set; get; }
        public virtual string AssemblyName { set; get; }
        public virtual string ModuleNameSpace { set; get; }
        public virtual string ClassName { set; get; }
		public virtual string ClientIp { get; set; }
        public virtual string PortNumber { set; get; }
        public virtual string EqType { set; get; }
    }}
