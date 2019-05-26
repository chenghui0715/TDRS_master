using System;
using System.Data;
using System.Data.SqlClient;
using ICMS.Commons;
using ICMS.Modules.BaseComponents;
using ICMS.Modules.BaseComponents.Commons;

namespace ICMS.Modules.Components.DAO
{
	public class VacuumDAO
	{
		private SqlServerHelper _sqlServerDefault;
		private SqlServerHelper _sqlServerEq;
		public VacuumDAO()
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

        public ExecutionResult GetWipErrorFlagBySn(string sn)
        {
            const string sql = "select * from C_WIP_TRACKING_T WHERE SERIAL_NUMBER='{0}'";

            return _sqlServerDefault.GetDataSet(string.Format(sql, sn));

        }


		public ExecutionResult GetVacuumValueBySn(string sn)
		{
			const string sql = "select * from C_TEST_DATA_T WHERE SERIAL_NUMBER='{0}'";

			return _sqlServerDefault.GetDataSet(string.Format(sql, sn));

		}


		public ExecutionResult UpdateWipLog(int errorFlag, string sn)
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

		public ExecutionResult InsertVacuumErrorLog(string mono, string sn, string errorCode, string errorCount, string stationName, string createUser, DateTime dateTime)
		{
			ExecutionResult exeResult = new ExecutionResult();
			string sql = "INSERT INTO C_ERROR_LOG_T (MO_NO,SERIAL_NUMBER,ERROR_CODE,ERROR_COUNT,STATION_NAME,CREATE_USER,CREATE_TIME)VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}') ";
			bool result = _sqlServerDefault.ExecCmd(string.Format(sql, mono, sn, errorCode, errorCount, stationName, createUser, dateTime));
			if (result)
			{
				exeResult.Status = true;
			}
			else
			{
				exeResult.Status = false;}
			return exeResult;
		}

        public ExecutionResult GetLeakageData(string sn )
        {
            const string sql = "select * from C_LEAKAGE_T WHERE   SERIAL_NUMBER='{0}'";

            return _sqlServerDefault.GetDataSet(string.Format(sql, sn));
        }

        public ExecutionResult UpdateP1LeakageData(string testValue, string sn)
        {
            string sql = "update C_LEAKAGE_T set P1='{0}',LAST_TIME=GETDATE(),COUNT='1' where SERIAL_NUMBER='{1}' ";
            ExecutionResult exeResult = _sqlServerDefault.ExecuteCmd(string.Format(sql, testValue, sn));
            if (exeResult.Status)
            {
                exeResult.Message = "成功更新真空度测试值！";
            }

            return exeResult;
        }

        public ExecutionResult InsertP1LeakageData(string productType, string sn,  string testValue)
        {

            string sql = "INSERT INTO C_LEAKAGE_T (PRODUCT_TYPE,SERIAL_NUMBER,P1,LAST_TIME,COUNT)VALUES('{0}','{1}','{2}',GETDATE(),'1') ";
            var exeResult = _sqlServerDefault.ExecuteCmd(string.Format(sql, productType, sn,testValue));
            if (exeResult.Status)
            {
                exeResult.Message = "成功插入真空度测试值！";
            }

            return exeResult;
        }

        public ExecutionResult UpdateP2LeakageData(string testValue, string sn)
        {
            string sql = "update C_LEAKAGE_T set P2='{0}',PRE_LAST_TIME=GETDATE(),COUNT='2' where SERIAL_NUMBER='{1}' ";
            ExecutionResult exeResult = _sqlServerDefault.ExecuteCmd(string.Format(sql, testValue, sn));
            if (exeResult.Status)
            {
                exeResult.Message = "成功更新真空度测试值！";
            }

            return exeResult;
        }

        public ExecutionResult InsertP2LeakageData(string productType, string sn, string testValue)
        {

            
            string sql1 = "select LAST_TIME FROM C_LEAKAGE WHERE SERIAL_NUMBER='{0}' ";
            var exeResult1 = _sqlServerDefault.GetDataSet(string.Format(sql1,sn));
            DateTime pre = DateTime.Now;
            if (exeResult1.Status)
            {
                var ds = (DataSet) exeResult1.Anything;
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    pre = DateTime.Parse(ds.Tables[0].Rows[0]["LAST_TIME"].ToString());
                }
            }
            string sql = "INSERT INTO C_LEAKAGE_T (PRODUCT_TYPE,SERIAL_NUMBER,P2,LAST_TIME,COUNT,PRE_LAST_TIME)VALUES('{0}','{1}','{2}',GETDATE(),'2','{3}') ";
            var exeResult = _sqlServerDefault.ExecuteCmd(string.Format(sql, productType, sn, testValue, pre));
            if (exeResult.Status)
            {
                exeResult.Message = "成功插入真空度测试值！";
            }

            return exeResult;
        }

		public ExecutionResult GetTestData(string sn, string stationName)
		{
			const string sql = "select * from C_TEST_DATA_T WHERE  STATION_NAME='{1}' AND SERIAL_NUMBER='{0}'";

			return _sqlServerDefault.GetDataSet(string.Format(sql, sn, stationName));

		}

        public ExecutionResult UpdateP1Quality(string sn, string errorFlag)
        {

            string sql = "update C_QUALITY_TEST_T set SERIAL_NUMBER='{0}',P1='{1}' where SERIAL_NUMBER='{0}'";
            return _sqlServerDefault.ExecuteCmd(string.Format(sql, sn, errorFlag));

        }

        public ExecutionResult UpdateP2Quality(string sn, string errorFlag)
        {

            string sql = "update C_QUALITY_TEST_T set SERIAL_NUMBER='{0}',P2='{1}' where SERIAL_NUMBER='{0}'";
            return _sqlServerDefault.ExecuteCmd(string.Format(sql, sn, errorFlag));

        }


		public ExecutionResult UpdateTestData(string testValue, string sn, string stationName, string userName)
		{
            string sql = "update C_TEST_DATA_T set TEST_VALUE='{0}',CREATE_TIME=GETDATE(),USER_ID='{3}' where SERIAL_NUMBER='{1}'and STATION_NAME='{2}' ";
			ExecutionResult exeResult = _sqlServerDefault.ExecuteCmd(string.Format(sql, testValue, sn, stationName,userName));
			if (exeResult.Status)
			{
				exeResult.Message = "成功更新真空度测试值！";
			}

			return exeResult;
		}

        public ExecutionResult InsertVacuumTestData(string productType, string sn, string stationName, string testValue, string userName)
		{

            string sql = "INSERT INTO C_TEST_DATA_T (PRODUCT_TYPE,SERIAL_NUMBER,STATION_NAME,TEST_ITEM,TEST_VALUE,USER_ID,CREATE_TIME)VALUES('{0}','{1}','{2}','{3}','{4}','{5}',GETDATE()) ";
			var exeResult = _sqlServerDefault.ExecuteCmd(string.Format(sql, productType, sn, stationName, stationName, testValue,userName));
			if (exeResult.Status)
			{
				exeResult.Message = "成功插入真空度测试值！";
			}

			return exeResult;
		}

        

        //public ExecutionResult InsertLeakageData(string productType, string sn, string stationName, string testValue, string userName)
        //{

        //    string sql = "INSERT INTO C_TEST_DATA_T (PRODUCT_TYPE,SERIAL_NUMBER,STATION_NAME,TEST_ITEM,TEST_VALUE,USER_ID,CREATE_TIME)VALUES('{0}','{1}','{2}','{3}','{4}','{5}',GETDATE()) ";
        //    var exeResult = _sqlServerDefault.ExecuteCmd(string.Format(sql, productType, sn, stationName, stationName, testValue, userName));
        //    if (exeResult.Status)
        //    {
        //        exeResult.Message = "成功插入真空度测试值！";
        //    }

        //    return exeResult;
        //}



		public ExecutionResult TestTransationFunc()
		{
			string sql1 = "insert into TEST_DATA values('AGAN2','25','12345');";
			string sql2 = "insert into TEST_DATA_T values('AGAN3','TESTDATA');";
			ExecutionResult result = new ExecutionResult();
			SqlConnection conn = new SqlConnection(_sqlServerDefault.conn);
			try
			{
				conn.Open();
				SqlTransaction sqlTransaction = conn.BeginTransaction();
				result = _sqlServerDefault.ExecuteCmd(sql1, conn, sqlTransaction);
				if (result.Status)
				{
					result = _sqlServerDefault.ExecuteCmd(sql2, conn, sqlTransaction);
					if (result.Status)
					{
						sqlTransaction.Commit();
					}
					else
					{
						sqlTransaction.Rollback();
					}
				}
				else
				{
					sqlTransaction.Rollback();
				}
			}
			catch (Exception e)
			{
				result.Message = e.Message;
				result.Status = false;
				return result;
			}
			finally
			{
				conn.Close();
			}

			return null;
		}
	}
}
