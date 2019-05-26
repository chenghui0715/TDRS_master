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
	public class HighPressureAgingDAO
	{
		private SqlServerHelper _sqlServerEq;
		public HighPressureAgingDAO()
		{
			
			_sqlServerEq = new SqlServerHelper();
			_sqlServerEq.conn = ConfigurationHelper.GetLocalConfigValue("HighVoltageDB");
		}



		public ExecutionResult GetPfCountInfo()
		{
            const string sql = "select Count(1) as NUMBER from PF_DATA where TEST_TIME IS NULL ";

			return _sqlServerEq.GetDataSet(string.Format(sql));

		}

        public ExecutionResult GetPfDataInfo(string sn)
        {
            const string sql = "select top 1 * from PF_DATA where SERIAL_NUMBER='{0}' order by CREATE_TIME DESC";

            return _sqlServerEq.GetDataSet(string.Format(sql,sn));

        }

		public ExecutionResult InsertPfDataInfo(string sn, string productType,string stationName)
		{

            string sql = "INSERT INTO PF_DATA (SERIAL_NUMBER,PRODUCT_TYPE,STATION_NAME,CREATE_TIME)VALUES('{0}','{1}','{2}',GETDATE()) ";
			return _sqlServerEq.ExecuteCmd(string.Format(sql, sn, productType,stationName));


		}

        public ExecutionResult UpdatePfDataInfo(string productType, string sn, string stationName)
		{

            string sql = "update PF_DATA  set PRODUCT_TYPE='{0}',STATION_NAME='{1}' ,CREATE_TIME=GETDATE() where SERIAL_NUMBER='{2}'and CREATE_TIME=(select max(CREATE_TIME) From PF_DATA where SERIAL_NUMBER='{2}' )  ";
			return _sqlServerEq.ExecuteCmd(string.Format(sql, productType, stationName,sn));
			
		}
	}
}
