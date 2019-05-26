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
    public class HighPressureAgingEQDAO
    {
        private SqlServerHelper _sqlServerDefault;
        private SqlServerHelper _sqlServerEq;
        public HighPressureAgingEQDAO()
        {_sqlServerDefault = new SqlServerHelper();
            _sqlServerEq = new SqlServerHelper();
            _sqlServerEq.conn = ConfigurationHelper.GetLocalConfigValue("HighVoltageDB");
        }

    
        public ExecutionResult GetPfDataInfo()
        {
           const string sql = "select top 6  * from PF_DATA where TEST_TIME IS NOT NULL order by TEST_TIME DESC ";
           
           return _sqlServerEq.GetDataSet(string.Format(sql));
            
        }



        public ExecutionResult UpdateQualityHighDischargeInfo(string sn, string firstDischargeValue, string errorFlag)
        {

            string sql = "UPDATE C_QUALITY_TEST_T set BREAKDOWN_VOLTAGE='{0}' ,PRESSURE_DETECTION='{1}' where SERIAL_NUMBER='{2}'";
            return _sqlServerDefault.ExecuteCmd(string.Format(sql, firstDischargeValue, errorFlag, sn));

        }

        public ExecutionResult InsertQualityInfo(string sn, string voltage, bool qualified)
        {
            ExecutionResult exeResult = new ExecutionResult();
            string sql = "INSERT INTO C_QUALITY_TEST_T (SERIAL_NUMBER,BREAKDOWN_VOLTAGE,PRESSURE_DETECTION)VALUES('{0}','{1}','{2}') ";
            bool result = _sqlServerDefault.ExecCmd(string.Format(sql,sn,voltage,qualified));
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
