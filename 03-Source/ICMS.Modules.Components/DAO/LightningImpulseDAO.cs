using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ICMS.Commons;
using ICMS.Modules.BaseComponents;
using ICMS.Modules.BaseComponents.Commons;

namespace ICMS.Modules.Components.DAO
{
   public class LightningImpulseDAO
    {
        private SqlServerHelper _sqlServerDefault;
        private SqlServerHelper _sqlServerEq;
        public LightningImpulseDAO()
        {
            _sqlServerDefault = new SqlServerHelper();
            _sqlServerEq = new SqlServerHelper();
            _sqlServerEq.conn = ConfigurationHelper.GetLocalConfigValue("ImpluseDB");
        }



        public ExecutionResult GetIVDataInfo(string sn)
        {
            const string sql = "select top 1 * from IVCTEST_DATA where SERIAL_NUMBER='{0}' order by CREATE_TIME DESC";

            return _sqlServerEq.GetDataSet(string.Format(sql, sn));

        }
        public ExecutionResult GetIvcCountInfo()
        {
            const string sql = "select Count(1) as NUMBER from IVCTEST_DATA where TEST_TIME IS NULL";

            return _sqlServerEq.GetDataSet(string.Format(sql));

        }
        

        public ExecutionResult InsertIVDataInfo(string sn, string productType,string stationName)
        {

            string sql = "INSERT INTO IVCTEST_DATA (SERIAL_NUMBER,PRODUCT_TYPE,STATION_NAME,CREATE_TIME)VALUES('{0}','{1}','{2}',GETDATE()) ";
            return _sqlServerEq.ExecuteCmd(string.Format(sql, sn, productType, stationName));
           
        }

        public ExecutionResult UpdateIVDataInfo(string productType, string sn,string stationName)
        {
            string sql = "update IVCTEST_DATA set PRODUCT_TYPE='{0}',STATION_NAME='{1}' ,CREATE_TIME=GETDATE() where SERIAL_NUMBER='{2}'and CREATE_TIME=(select max(CREATE_TIME) From IVCTEST_DATA where SERIAL_NUMBER='{2}' ) ";
            return _sqlServerEq.ExecuteCmd(string.Format(sql, productType,stationName, sn ));

        }
    }
}
