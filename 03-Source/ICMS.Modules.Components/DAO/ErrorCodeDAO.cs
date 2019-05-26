using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICMS.Modules.BaseComponents;
using ICMS.Modules.BaseComponents.Commons;

namespace ICMS.Modules.Components.DAO
{
    public class ErrorCodeDAO
    {

        public static ExecutionResult GetErrorInfo(string stationName)
        {
            SqlServerHelper _sqlServer = new SqlServerHelper();
            string sql = "select * from c_error_code_t where station_name='{0}'";
            return _sqlServer.GetDataSet(string.Format(sql, stationName));
        }
    }


}
