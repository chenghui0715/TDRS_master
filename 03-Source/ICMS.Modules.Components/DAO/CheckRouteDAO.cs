using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ICMS.Modules.BaseComponents;
using ICMS.Modules.BaseComponents.Commons;

namespace ICMS.Modules.Components.DAO
{
    public class CheckRouteDAO
    {
        private SqlServerHelper _sqlServer;

        public CheckRouteDAO()
        {
            _sqlServer = new SqlServerHelper();
        }

        public DataSet GetWipInfo(string sn)
        {
            string sql = "select * from C_WIP_TRACKING_T w,C_MO_T m where w.MO_NO=m.MO_NO and W.SERIAL_NUMBER='{0}'";
            DataSet ds = new DataSet();
            ds = _sqlServer.GetResult(string.Format(sql, sn));
            return ds;
        }

        public DataSet GetNextstation(string sn,string stationName)
        {
            string sql ="SELECT ss.STATION_NAME from (SELECT S.STATION_NAME, C.NEXT_STATION_CODE FROM C_WIP_TRACKING_T T, C_ROUTE_CONTROL_T C, C_STATION_T S WHERE T.ROUTE_CODE = C.ROUTE_CODE AND T.SERIAL_NUMBER = '{0}' AND S.ID = C.STATION_CODE) tt, C_STATION_T ss WHERE tt.NEXT_STATION_CODE = ss.ID AND tt.STATION_NAME = '{1}'";
            DataSet ds = new DataSet();
            ds = _sqlServer.GetResult(string.Format(sql, sn,stationName));
            return ds;
        }


        public ExecutionResult UpdateNextStation(string stationName ,string sn)
        {
            ExecutionResult exeResult=new ExecutionResult();
            string sql = "update C_WIP_TRACKING_T set NEXT_STATION='{0}' where SERIAL_NUMBER='{1}' ";
            bool result = _sqlServer.ExecCmd(string.Format(sql, stationName,sn));
            if (result)
            {
                exeResult.Status = true;
            }
            else
            {
                exeResult.Status = false;
            }
            return exeResult;
        }
    }
}
