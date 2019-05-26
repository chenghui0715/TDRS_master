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
    public class SecondVacuumDAO
    {
        private SqlServerHelper _sqlServerDefault;
        private SqlServerHelper _sqlServerEq;
        public SecondVacuumDAO()
        {
            _sqlServerDefault = new SqlServerHelper();
            _sqlServerEq = new SqlServerHelper();
            _sqlServerEq.conn = ConfigurationHelper.GetLocalConfigValue("PFTEST");
        }

        public DataSet GetWipLog(string sn)
        {
            const string sql = "select * from C_WIP_LOG_T WHERE  SERIAL_NUMBER='{0}'";
            DataSet ds = new DataSet();
            ds = _sqlServerDefault.GetResult(string.Format(sql, sn));
            return ds;
        }

        public DataSet GetTestData(string sn,string stationName)
        {
            const string sql = "select * from C_TEST_DATA_T WHERE  SERIAL_NUMBER='{0}'AND STATION_NAME='{1}'";
            DataSet ds = new DataSet();
            ds = _sqlServerDefault.GetResult(string.Format(sql, sn,stationName));
            return ds;
        }


        public DataSet GetVacuumValueBySn(string sn)
        {
            const string sql = "select * from C_TEST_DATA_T WHERE SERIAL_NUMBER='{0}'";
            DataSet ds = new DataSet();
            ds = _sqlServerDefault.GetResult(string.Format(sql, sn));
            return ds;
        }

      
         public ExecutionResult  UpdateWipLog(int errorFlag, string sn)
        {
            ExecutionResult exeResult = new ExecutionResult();
            string sql = "update C_WIP_LOG_T set ERROR_FLAG='{0}' where SERIAL_NUMBER='{1}' ";
            bool result = _sqlServerDefault.ExecCmd(string.Format(sql, errorFlag, sn));
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


         public ExecutionResult UpdateTestData(string testValue,string sn,string stationName)
        {
            ExecutionResult exeResult = new ExecutionResult();
            string sql = "update C_TEST_DATA_T set TEST_VALUE='{0}' where SERIAL_NUMBER='{1}'and STATION_NAME='{2}' ";
            bool result = _sqlServerDefault.ExecCmd(string.Format(sql, testValue, sn,stationName));
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


       
        public ExecutionResult InsertVacuumErrorLog(string mono, string sn, string errorCode, string errorCount,string stationName,string createUser,DateTime dateTime )
        {
            ExecutionResult exeResult = new ExecutionResult();
            string sql = "INSERT INTO C_ERROR_LOG_T (MO_NO,SERIAL_NUMBER,ERROR_CODE,ERROR_COUNT,STATION_NAME,CREATE_USER,CREATE_TIME)VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}') ";
            bool result = _sqlServerDefault.ExecCmd(string.Format(sql, mono, sn, errorCode, errorCount, stationName, createUser,dateTime));
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


        public ExecutionResult InsertVacuumTestData(string productType, string sn, string stationName, string testItem, string testValue, DateTime dateTime)
        {
            ExecutionResult exeResult = new ExecutionResult();
            string sql = "INSERT INTO C_TEST_DATA_T (PRODUCT_TYPE,SERIAL_NUMBER,STATION_NAME,TEST_ITEM,TEST_VALUE,CREATE_TIME)VALUES('{0}','{1}','{2}','{3}','{4}','{5}') ";
            bool result = _sqlServerDefault.ExecCmd(string.Format(sql, productType, sn, stationName, testItem, testValue, dateTime));
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
