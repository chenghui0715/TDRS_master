using YH.ICMS.Common.Enumeration;

namespace YH.ICMS.Entity
{
    public  class VM_TDRSConfig
    {
        public Direction HeadDirection { get; set; }

        public string SqlConnnect { get; set; }

        public bool RegistAutoStartFlag { get; set; }

        public bool CancleRegistAutoStartFlag { get; set; }
    }
}
