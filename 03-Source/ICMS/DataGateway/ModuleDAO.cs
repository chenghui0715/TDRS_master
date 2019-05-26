using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ICMS.Commons;
using ICMS.Modules.BaseComponents;
using ICMS.Modules.BaseComponents.Commons;

namespace ICMS.DataGateway
{
    class ModuleDAO
    {
        private SqlServerHelper sqlServerHelper;
       // private string connString = SqlServerHelper.conn;
        public ModuleDAO()
        {
            sqlServerHelper=new SqlServerHelper();
        }
        public DataSet GetModuleDs()
        {
            DataSet ds=new DataSet();
            string sql = @"SELECT M.ID,M.LINE_NAME,M.STATION_CODE,
(SELECT S.STATION_NAME FROM C_STATION_T S WHERE S.ID=M.STATION_CODE) 'STATION_NAME',M.ASSEMBLY_NAME,M.MODULE_NAMESPACE,
M.CLASS_NAME,M.PORT_NUMBER,EQ_TYPE,CLIENT_IP FROM C_MODULE_DEFINITION_T M ;";
            ds = sqlServerHelper.GetResult(sql);
            return ds;
        }
    }
}
