using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ICMS.Modules.BaseComponents;
using ICMS.Modules.BaseComponents.Commons;

namespace ICMS.Modules.Components.DAO
{
	public class CommonsDAO
	{

        private SqlServerHelper _sqlServer = new SqlServerHelper();
		public ExecutionResult GetWipInfo(string sn)
		{
			string sql = "select * from C_WIP_TRACKING_T w,C_MO_T m where w.MO_NO=m.MO_NO and W.SERIAL_NUMBER='{0}'";

			return _sqlServer.GetDataSet(string.Format(sql, sn));

		}

        public ExecutionResult GetQualityInfo(string sn)
        {
            string sql = "select * from C_QUALITY_TEST_T WHERE SERIAL_NUMBER='{0}'";

            return _sqlServer.GetDataSet(string.Format(sql, sn));
        }


        public ExecutionResult InsertQuality(string sn, string isSprayPainting, string isUpsetting)
        {

            string sql = "INSERT INTO C_QUALITY_TEST_T (SERIAL_NUMBER,SPRAY_PAINT,UPSETTING,CREATE_TIME) VALUES ({0},'{1}','{2}',GETDATE()) ";
            return _sqlServer.ExecuteCmd(string.Format(sql, sn, isSprayPainting, isUpsetting));

        }

        public ExecutionResult UpdateQuality(string sn, string isSprayPainting, string isUpsetting)
        {

            string sql = "update C_QUALITY_TEST_T set SERIAL_NUMBER='{0}',SPRAY_PAINT='{1}',UPSETTING='{2}',CREATE_TIME=GETDATE() where SERIAL_NUMBER='{0}'";
            return _sqlServer.ExecuteCmd(string.Format(sql, sn,isSprayPainting,isUpsetting));

        }

		public ExecutionResult GetNextstation(string sn, string stationName)
		{
			string sql = @"SELECT a.*,b.STATION_NAME NEXT_STATION_NAME FROM 
								(SELECT s.STATION_NAME, r.NEXT_STATION_CODE FROM dbo.C_STATION_T s, dbo.C_ROUTE_CONTROL_T r,dbo.C_WIP_TRACKING_T w
								WHERE s.id=r.STATION_CODE
								AND s.STATION_NAME='{0}'
								and r.ROUTE_CODE=w.ROUTE_CODE
								AND w.SERIAL_NUMBER='{1}') a LEFT JOIN dbo.C_STATION_T b ON a.NEXT_STATION_CODE=b.id";
			return _sqlServer.GetDataSet(string.Format(sql, stationName, sn));

		}


		public ExecutionResult UpdateStation(string stationName, string nextStationName,string errorFlag, string sn)
		{

			string sql = "update C_WIP_TRACKING_T set STATION_NAME='{0}', NEXT_STATION='{1}',ERROR_FLAG='{2}' where SERIAL_NUMBER='{3}' ";
			return _sqlServer.ExecuteCmd(string.Format(sql, stationName, nextStationName, errorFlag,sn));

		}

        public ExecutionResult Updateflag(string errorFlag, string sn)
        {

            string sql = "update C_WIP_TRACKING_T set ERROR_FLAG={0} where SERIAL_NUMBER='{1}' ";
            return _sqlServer.ExecuteCmd(string.Format(sql, errorFlag, sn));

        }

		public ExecutionResult InsertWipLog(string sn, string errorFlag)
		{

			string sql = "INSERT INTO C_WIP_LOG_T (SERIAL_NUMBER,MO_NO,STATION_NAME,LINE_NAME,ERROR_FLAG,CREATE_TIME) SELECT SERIAL_NUMBER,MO_NO,STATION_NAME,LINE_NAME,{0},GETDATE() FROM dbo.C_WIP_TRACKING_T WHERE SERIAL_NUMBER='{1}' ";
			return _sqlServer.ExecuteCmd(string.Format(sql, errorFlag, sn));

		}

        public ExecutionResult UpdateWipLog(string sn, string stationName, string errorFlag)
        {

            string sql = "update C_WIP_LOG_T set ERROR_FLAG={2} WHERE SERIAL_NUMBER='{0}' and STATION_NAME='{1}' ";
            return _sqlServer.ExecuteCmd(string.Format(sql, sn,stationName,errorFlag));

        }

        public ExecutionResult GetWipLogInfo(string sn, string stationName)
        {

            string sql = "SElECT * FROM C_WIP_LOG_T WHERE SERIAL_NUMBER='{0}' AND STATION_NAME='{1}' ";
            return _sqlServer.GetDataSet(string.Format(sql, sn, stationName));

        }
	}
}
