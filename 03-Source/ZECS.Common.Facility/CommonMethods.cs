using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZECS.Common.Facility
{
    public class CommonMethods
    {
        public static String CreateGUID()
        {
            string str = System.Guid.NewGuid().ToString();
            str = str.Replace("-", "");
            str = str.ToUpper();
            return str;
        }
    }
}
