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
    public class XRayDAO
    {
        private SqlServerHelper _sqlServer;
        private SqlServerHelper _sqlServerIMes;

        public XRayDAO()
        {
            _sqlServer = new SqlServerHelper();
            _sqlServerIMes = new SqlServerHelper();
            _sqlServerIMes.conn = ConfigurationHelper.GetLocalConfigValue("IMesDB");
        }

        public ExecutionResult GetUserInfo(string userName,string passWord)
        {
            _sqlServerIMes.conn = ConfigurationHelper.GetLocalConfigValue("IMesDB");
            const string sql = "SELECT USER_ID,USER_NAME,PASSWORD,ROLE_ID FROM C_USER_T WHERE USER_ID='{0}' AND PASSWORD='{1}'";

            return _sqlServerIMes.GetDataSet(string.Format(sql, userName,passWord));

        }


        public ExecutionResult UpdateXrayQuality(string sn, string errorFlag)
        {

            string sql = "update C_QUALITY_TEST_T set SERIAL_NUMBER='{0}',XRAY='{1}' where SERIAL_NUMBER='{0}'";
            return _sqlServer.ExecuteCmd(string.Format(sql, sn, errorFlag));

        }

        public ExecutionResult UpdateGunDaoQuality(string sn, string errorFlag)
        {

            string sql = "update C_QUALITY_TEST_T set SERIAL_NUMBER='{0}',ROLLING_GUIDE_SLEEVE='{1}' where SERIAL_NUMBER='{0}'";
            return _sqlServer.ExecuteCmd(string.Format(sql, sn, errorFlag));

        }

        public ExecutionResult GetWipErrorFlagBySn(string sn)
        {
            const string sql = "select * from C_WIP_TRACKING_T WHERE SERIAL_NUMBER='{0}'";

            return _sqlServer.GetDataSet(string.Format(sql, sn));

        }
        public ExecutionResult InsertWarningInfo(string sn, string stationName, List<string> errorCodes)
        {
            ExecutionResult exeResult = new ExecutionResult();
            string sql = "insert into C_WARNING_T (MO_NO,SERIAL_NUMBER,STATION_NAME,ERROR_CODE,CREATE_TIME) SELECT MO_NO,SERIAL_NUMBER,'{0}','{1}',GETDATE() FROM dbo.C_WIP_TRACKING_T WHERE SERIAL_NUMBER='{2}'";
            foreach (var errorCode in errorCodes)
            {
                exeResult = _sqlServer.ExecuteCmd(string.Format(sql, stationName, errorCode, sn));
                if (!exeResult.Status)
                {
                    return exeResult;
                }
            }
            return exeResult;
        }

        public ExecutionResult InsertErrorLogInfo(string sn, string stationName, List<string> errorCodes,string userName)
        {
            ExecutionResult exeResult = new ExecutionResult();
            string sql = "insert into C_ERROR_LOG_T (MO_NO,SERIAL_NUMBER,STATION_NAME,ERROR_COUNT,ERROR_CODE,CREATE_USER,CREATE_TIME) SELECT MO_NO,SERIAL_NUMBER,'{0}','1','{1}','{3}',GETDATE() FROM dbo.C_WIP_TRACKING_T WHERE SERIAL_NUMBER='{2}'";
            foreach (var errorCode in errorCodes)
            {
                string sql1 = "SELECT * FROM C_ERROR_LOG_T WHERE  ERROR_CODE='{0}' and SERIAL_NUMBER='{1}' ";
                exeResult = _sqlServer.GetDataSet(string.Format(sql1, errorCode, sn));
                if (exeResult.Status)
                {
                    var ds = (DataSet)exeResult.Anything;
                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        string sql2 = "update C_ERROR_LOG_T set ERROR_COUNT=((select ERROR_COUNT from C_ERROR_LOG_T where  ERROR_CODE='{0}' and SERIAL_NUMBER='{1}')+1),CREATE_USER='{2}',CREATE_TIME=GETDATE() where  ERROR_CODE='{0}' and SERIAL_NUMBER='{1}'";
                        exeResult = _sqlServer.ExecuteCmd(string.Format(sql2, errorCode, sn,userName));
                        if (!exeResult.Status)
                        {
                            exeResult.Message = "更新error_count栏位数据失败！";
                            return exeResult;
                        }
                    }
                    else
                    {
                        exeResult = _sqlServer.ExecuteCmd(string.Format(sql, stationName, errorCode, sn, userName));
                        if (!exeResult.Status)
                        {
                            return exeResult;
                        }
                    }
                    
                }
                else
                {
                    return exeResult;
                }
               

            }
            return exeResult;
        }
    }
}
