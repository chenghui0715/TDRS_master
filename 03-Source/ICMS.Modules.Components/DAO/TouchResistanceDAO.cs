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
    public class TouchResistanceDAO
    {
        private SqlServerHelper _sqlServerDefault;

		public TouchResistanceDAO()
		{
			_sqlServerDefault = new SqlServerHelper();
		}

		public ExecutionResult GetTestData(string sn)
		{
			const string sql = "select * from C_TOUCH_RESISTANCE_T WHERE SERIAL_NUMBER='{0}'";

			return _sqlServerDefault.GetDataSet(string.Format(sql, sn));

		}

        public ExecutionResult GetQualityData(string sn)
        {
            const string sql = "select * from C_QUALITY_TEST_T WHERE SERIAL_NUMBER='{0}'";

            return _sqlServerDefault.GetDataSet(string.Format(sql, sn));

        }

        public ExecutionResult InsertQualityDataAll(string sn, string dianZuValue, string ziBiLiValue, string fanLiValue, string mode)
        {
            if (mode == "True")
            {
                string sql = "INSERT INTO C_QUALITY_TEST_T (SERIAL_NUMBER,CONTACT_RESISTANCE,CLOSING_FORCE,COUNTER_FORCE)VALUES('{0}','{1}','{2}','{3}') ";
                var exeResult = _sqlServerDefault.ExecuteCmd(string.Format(sql, sn, dianZuValue, ziBiLiValue, fanLiValue));
                if (exeResult.Status)
                {
                    exeResult.Message = "所有数据插入质量测试表成功！";
                }

                return exeResult;
            }
            else
            {
                string sql = "INSERT INTO C_QUALITY_TEST_T (SERIAL_NUMBER,CONTACT_RESISTANCE)VALUES('{0}','{1}') ";
                var exeResult = _sqlServerDefault.ExecuteCmd(string.Format(sql, sn, dianZuValue));
                if (exeResult.Status)
                {
                    exeResult.Message = "接触电阻值插入质量测试表成功！";
                }

                return exeResult;
            }

        }

        public ExecutionResult UpdateQualityDataAll(string sn, string dianZuValue, string ziBiLiValue, string fanLiValue, string mode)
        {
            if (mode == "全部测试")
            {
                string sql = "update C_QUALITY_TEST_T set SERIAL_NUMBER='{0}', CONTACT_RESISTANCE='{1}',CLOSING_FORCE='{2}',COUNTER_FORCE='{3}'  where SERIAL_NUMBER='{0}'";
                ExecutionResult exeResult = _sqlServerDefault.ExecuteCmd(string.Format(sql, sn, dianZuValue, ziBiLiValue, fanLiValue));
                if (exeResult.Status)
                {
                    exeResult.Message = "所有数据更新质量测试表成功！";
                }

                return exeResult;
            }
            else
            {
                string sql = "update C_QUALITY_TEST_T set SERIAL_NUMBER='{0}', CONTACT_RESISTANCE='{1}'  where SERIAL_NUMBER='{0}'";
                ExecutionResult exeResult = _sqlServerDefault.ExecuteCmd(string.Format(sql, sn, dianZuValue));
                if (exeResult.Status)
                {
                    exeResult.Message = "接触电阻值更新质量测试表成功！";
                }

                return exeResult;
            }
           
        }

        public ExecutionResult InsertTouchDataAll(string sn, string productType, string dianZuValue, string dianZuIsOk, string ziBiLiValue, string ziBiLiIsOk, string fanLiValue, string fanLiIsOk, string mode, string userName)
		{
            if (mode == "全部测试")
            {
                string sql = "INSERT INTO C_TOUCH_RESISTANCE_T (SERIAL_NUMBER,PRODUCT_TYPE,DIANZU_VALUE,DIANZU_ISOK,ZIBILI_VALUE,ZIBILI_ISOK,FANLI_VALUE,FANLI_ISOK,MODE,USER_ID,DATA_TIME)VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}',GETDATE()) ";
                var exeResult = _sqlServerDefault.ExecuteCmd(string.Format(sql, sn, productType, dianZuValue, dianZuIsOk, ziBiLiValue, ziBiLiIsOk, fanLiValue, fanLiIsOk, mode,userName));
                if (exeResult.Status){
                    exeResult.Message = "成功插入接触电阻测试表！";
                }

                return exeResult;
                
            }
            else
            {
                string sql = "INSERT INTO C_TOUCH_RESISTANCE_T (SERIAL_NUMBER,PRODUCT_TYPE,DIANZU_VALUE,DIANZU_ISOK,MODE,USER_ID,DATA_TIME)VALUES('{0}','{1}','{2}','{3}','{4}','{5}',GETDATE()) ";
                var exeResult = _sqlServerDefault.ExecuteCmd(string.Format(sql, sn, productType, dianZuValue, dianZuIsOk,  mode,userName));
                if (exeResult.Status)
                {
                    exeResult.Message = "成功插入接触电阻测试表！";
                }

                return exeResult;
            }
            
		}

        public ExecutionResult UpdateTouchDataAll(string sn, string productType, string dianZuValue, string dianZuIsOk, string ziBiLiValue, string ziBiLiIsOk, string fanLiValue, string fanLiIsOk, string mode, string userName)
        {
            if (mode == "全部测试")
            {
                string sql = "update C_TOUCH_RESISTANCE_T set SERIAL_NUMBER='{0}',PRODUCT_TYPE='{1}', DIANZU_VALUE='{2}', DIANZU_ISOK='{3}',ZIBILI_VALUE='{4}', ZIBILI_ISOK='{5}',FANLI_VALUE='{6}', FANLI_ISOK='{7}',MODE='{8}' ,USER_ID='{9}',DATA_TIME=GETDATE() where SERIAL_NUMBER='{0}'";
                ExecutionResult exeResult = _sqlServerDefault.ExecuteCmd(string.Format(sql, sn, productType, dianZuValue, dianZuIsOk, ziBiLiValue, ziBiLiIsOk, fanLiValue, fanLiIsOk, mode,userName));
                if (exeResult.Status)
                {
                    exeResult.Message = "所有数据更新接触电阻测试表成功！";
                }

                return exeResult;
            }
            else
            {
                string sql = "update C_TOUCH_RESISTANCE_T set SERIAL_NUMBER='{0}',PRODUCT_TYPE='{1}', DIANZU_VALUE='{2}', DIANZU_ISOK='{3}',MODE='{4}',USER_ID='{5}',DATA_TIME=GETDATE()  where SERIAL_NUMBER='{0}'";
                ExecutionResult exeResult = _sqlServerDefault.ExecuteCmd(string.Format(sql, sn, productType, dianZuValue, dianZuIsOk, mode,userName));
                if (exeResult.Status)
                {
                    exeResult.Message = "接触电阻值更新接触电阻测试表成功！";
                }

                return exeResult;
            }
            
            

        }

    }
}
