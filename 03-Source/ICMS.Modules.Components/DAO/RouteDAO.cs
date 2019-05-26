using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICMS.Modules.BaseComponents;
using ICMS.Modules.BaseComponents.Commons;

namespace ICMS.Modules.Components.DAO
{
    public class RouteDAO
    {
        public static ExecutionResult GetRouteInfo(string sn)
        {
            SqlServerHelper _sqlServer = new SqlServerHelper();
            string sql = "select s.STATION_NAME from C_STATION_T s right join(select * FRom C_ROUTE_CONTROL_T where ROUTE_CODE=(select m.ROUTE_CODE from C_MO_T m where m.MO_NO=(select w.MO_NO from C_WIP_TRACKING_T w where w.SERIAL_NUMBER='{0}' ) ))C on s.ID=C.STATION_CODE";
            return _sqlServer.GetDataSet(string.Format(sql, sn));
        }

        public static ExecutionResult GetProductSerialInfo(string sn)
        {
            SqlServerHelper _sqlServer = new SqlServerHelper();
            string sql = "select p.PRODUCT_SERIAL FROM C_PRODUCT_SERIAL_MAP_T p where p.PRODUCT_TYPE=( select w.PRODUCT_TYPE from C_WIP_TRACKING_T w where w.SERIAL_NUMBER='{0}' ) ";
            return _sqlServer.GetDataSet(string.Format(sql, sn));
        }

        public static ExecutionResult GetProductTypeInfo(string sn)
        {
            SqlServerHelper _sqlServer = new SqlServerHelper();
            string sql = "select m.PRODUCT_TYPE from C_MO_T m where m.MO_NO=(select w.MO_NO from C_WIP_TRACKING_T w where w.SERIAL_NUMBER='{0}') ";
            return _sqlServer.GetDataSet(string.Format(sql, sn));//update 2015/10/16 ch ＋W.
        }

         public static ExecutionResult GetPalletInfo(string productType)
        {
            SqlServerHelper _sqlServer = new SqlServerHelper();
            string sql = "SELECT * FROM C_PRODUCT_SERIAL_MAP_T where PRODUCT_TYPE='{0}'";
            return _sqlServer.GetDataSet(string.Format(sql, productType));
        }

        
    }
}
