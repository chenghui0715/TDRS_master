using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ICMS.Modules.BaseComponents;
using ICMS.Modules.BaseComponents.Commons;

namespace ICMS.Modules.Components.DAO
{
    public class VacuumRetestDAO
    {
        private SqlServerHelper _sqlServerDefault;
        public VacuumRetestDAO()
		{
			_sqlServerDefault = new SqlServerHelper();
		}

        public ExecutionResult GetVacuumValueBySn(string sn)
        {
            const string sql = "select * from C_TEST_DATA_T WHERE SERIAL_NUMBER='{0}'";

            return _sqlServerDefault.GetDataSet(string.Format(sql, sn));

        }

        public ExecutionResult GetFirstErrorFlag(string sn, string firstStationName)
        {
            const string sql = "select w.ERROR_FLAG from  C_WIP_LOG_T w where  W.STATION_NAME='{0}' AND W.SERIAL_NUMBER='{1}' order by CREATE_TIME DESC ";
            return _sqlServerDefault.GetDataSet(string.Format(sql,firstStationName,sn));
        }

        public ExecutionResult GetSecondErrorFlag(string sn, string secondStationName){
            const string sql = "select w.ERROR_FLAG from  C_WIP_LOG_T w where  W.STATION_NAME='{0}' AND W.SERIAL_NUMBER='{1}' order by CREATE_TIME DESC";
            return _sqlServerDefault.GetDataSet(string.Format(sql, secondStationName, sn));
        }

        public ExecutionResult GetTestData(string sn, string stationName)
        {
            const string sql = "select * from C_TEST_DATA_T WHERE  STATION_NAME='{1}' AND SERIAL_NUMBER='{0}'";

            return _sqlServerDefault.GetDataSet(string.Format(sql, sn, stationName));

        }

        public ExecutionResult GetLeakageData(string sn)
        {
            const string sql = "select * from C_LEAKAGE_T WHERE  SERIAL_NUMBER='{0}'";

            return _sqlServerDefault.GetDataSet(string.Format(sql, sn));

        }

        public ExecutionResult UpdateLeakageData(string sn,double testValue)
        {
            string count ="";
            
            string sql = "select count,PRE_LAST_TIME,LAST_TIME from C_LEAKAGE_T where serial_number='{0}'";

            ExecutionResult exeResult = _sqlServerDefault.GetDataSet(string.Format(sql, sn));
            if (exeResult.Status)
            {
                var ds = (DataSet) exeResult.Anything;
                if (ds!=null&&ds.Tables.Count>0&&ds.Tables[0].Rows.Count>0)
                {
                    count = ds.Tables[0].Rows[0]["COUNT"].ToString();
                    //DateTime preLastTime = DateTime.Parse(ds.Tables[0].Rows[0]["PRE_LAST_TIME"].ToString()??"");
                    DateTime lastTime = DateTime.Parse(ds.Tables[0].Rows[0]["LAST_TIME"].ToString() ?? "");
                    string colName = "";
                    if (int.Parse(count) >= 0)
                    {
                        switch (count)
                        {
                            case "0":
                                colName = "P1";
                                break;
                            case "1":
                                colName = "P2";
                                break;
                            case "2":
                                colName = "P3";
                                break;
                            case "3":
                                colName = "P4";
                                break;
                            case "4":
                                colName = "P5";
                                break;
                            case "5":
                                colName = "P6";
                                break;
                            case "6":
                                colName = "P7";
                                break;
                            case "7":
                                colName = "P8";
                                break;
                            case "8":
                                colName = "P9";
                                break;
                            case "9":
                                colName = "P10";
                                break;
                        }
                        if (colName != "")
                        {
                            if ( !string.IsNullOrWhiteSpace(lastTime.ToString()))
                            {
                                int newCount = int.Parse(count) + 1;
                                string sql1 = "update C_LEAKAGE_T set {0}='{1}',PRE_LAST_TIME='{2}',LAST_TIME=GETDATE(),COUNT='{3}' where serial_number='{4}'  ";
                                exeResult = _sqlServerDefault.ExecuteCmd(string.Format(sql1, colName, testValue, lastTime, newCount, sn));if (exeResult.Status)
                                {
                                    exeResult.Message = "成功插入漏率真空度测试值！";
                                } 
                            }
                            
                        }
                        
                    }
                    
                }
            }
           
           
            

            return exeResult;
        }

        public ExecutionResult InsertLeakageData(string sn,string productType,double testValue)
        {

            string sql = "INSERT INTO C_LEAKAGE_T (SERIAL_NUMBER,P1,PRODUCT_TYPE,COUNT,LAST_TIME)VALUES('{0}','{1}','{2}','1',GETDATE()) ";
            var exeResult = _sqlServerDefault.ExecuteCmd(string.Format(sql,  sn, testValue,productType));


            if (exeResult.Status)
            {
                exeResult.Message = "成功插入漏率真空度测试值！";
            }

            return exeResult;
        }


        public ExecutionResult UpdateTestData(double testValue, string sn, string stationName,string userName)
        {
            string sql = "update C_TEST_DATA_T set TEST_VALUE='{0}',CREATE_TIME=GETDATE(),USER_ID='{3}' where SERIAL_NUMBER='{1}'and STATION_NAME='{2}' ";
            ExecutionResult exeResult = _sqlServerDefault.ExecuteCmd(string.Format(sql, testValue, sn, stationName, userName));
           

            if (exeResult.Status)
            {
                exeResult.Message = "成功更新真空度测试值！";
            }

            return exeResult;
        }

        public ExecutionResult InsertVacuumTestData(string productType, string sn, string stationName, double testValue,string userName)
        {

            string sql = "INSERT INTO C_TEST_DATA_T (PRODUCT_TYPE,SERIAL_NUMBER,STATION_NAME,TEST_ITEM,TEST_VALUE,USER_ID,CREATE_TIME)VALUES('{0}','{1}','{2}','{3}','{4}','{5}',GETDATE()) ";
            var exeResult = _sqlServerDefault.ExecuteCmd(string.Format(sql, productType, sn, stationName, stationName, testValue,userName));
            if (exeResult.Status)
            {
                exeResult.Message = "成功插入真空度测试值！";
            }

            return exeResult;
        }

        public ExecutionResult InsertWipLog(string sn, string errorFlag)
        {

            string sql = "INSERT INTO C_WIP_LOG_T (SERIAL_NUMBER,MO_NO,STATION_NAME,LINE_NAME,ERROR_FLAG,CREATE_TIME) SELECT SERIAL_NUMBER,MO_NO,STATION_NAME,LINE_NAME,{0},GETDATE() FROM dbo.C_WIP_TRACKING_T WHERE SERIAL_NUMBER='{1}' ";
            return _sqlServerDefault.ExecuteCmd(string.Format(sql, errorFlag, sn));

        }

        public ExecutionResult UpdateWipLog(string sn, string stationName, string errorFlag)
        {

            string sql = "update C_WIP_LOG_T set ERROR_FLAG={2} WHERE SERIAL_NUMBER='{0}' and STATION_NAME='{1}' ";
            return _sqlServerDefault.ExecuteCmd(string.Format(sql, sn, stationName, errorFlag));

        }

        public ExecutionResult GetWipLogInfo(string sn, string stationName)
        {

            string sql = "SElECT * FROM C_WIP_LOG_T WHERE SERIAL_NUMBER='{0}' AND STATION_NAME='{1}' ";
            return _sqlServerDefault.GetDataSet(string.Format(sql, sn, stationName));

        }
    }
}
