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
   public class LightningImpulseEQDAO
    {
       private SqlServerHelper _sqlServerDefault;
        private SqlServerHelper _sqlServerEq;
        public LightningImpulseEQDAO()
        {
            _sqlServerDefault = new SqlServerHelper();
            _sqlServerEq = new SqlServerHelper();
            _sqlServerEq.conn = ConfigurationHelper.GetLocalConfigValue("ImpluseDB");
        }

       

        public ExecutionResult GetIvDataInfo(string sn)
        {
            const string sql = "select * from IVCTEST_DATA where SerialNumber='{0}'";
     
            return _sqlServerEq.GetDataSet(string.Format(sql, sn));
   
        }

        public ExecutionResult GetIvcTestDataInfo()
        {
            const string sql = "select top 3  * from IVCTEST_DATA order by TEST_TIME DESC ";

            return _sqlServerEq.GetDataSet(string.Format(sql));

        }

        public ExecutionResult InsertQualityInfo(string sn, string voltage, bool qualified)
        {
       
            string sql = "INSERT INTO C_QUALITY_TEST_T (SERIAL_NUMBER,POSITIVE,NEGATIVE)VALUES('{0}','{1}','{2}') ";
            return _sqlServerDefault.ExecuteCmd(string.Format(sql, sn, voltage, qualified));
           
        }

        public ExecutionResult InsertQualityImpluseInfo(string sn, string positive, string negaTive)
        {

            string sql = "INSERT INTO C_QUALITY_TEST_T (SERIAL_NUMBER,POSITIVE,NEGATIVE)VALUES('{0}','{1}','{2}') ";
            return _sqlServerDefault.ExecuteCmd(string.Format(sql, sn, positive, negaTive));

        }

        public ExecutionResult UpdateQualityImpluseInfo(string sn, string positive, string negaTive)
        {

            string sql = "UPDATE C_QUALITY_TEST_T set POSITIVE='{0}' ,NEGATIVE='{1}' where SERIAL_NUMBER='{2}'";
            return _sqlServerDefault.ExecuteCmd(string.Format(sql, positive, negaTive, sn));

        }

        public ExecutionResult InsertLightingImpulseInfo(string sn, string productType,string number,string positiveNumber,string negativeNumber,string userName,DateTime testTime)
        {

            string sql = "INSERT INTO C_LIGHTNING_IMPULSE_T (SERIAL_NUMBER,PRODUCT_TYPE,VOLTAGE,POSITIVE,NEGATIVE,CREATE_USER,CREATE_TIME)VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}') ";
            return _sqlServerDefault.ExecuteCmd(string.Format(sql, sn, productType, number, positiveNumber, negativeNumber, userName, testTime));

        }

       
    }
}
