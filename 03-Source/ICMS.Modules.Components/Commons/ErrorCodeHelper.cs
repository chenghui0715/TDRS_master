using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using ICMS.Modules.BaseComponents;
using System.Data;
using ICMS.Modules.Components.DAO;
namespace ICMS.Modules.Components.Commons
{
    public class ErrorCodeHelper
    {
        public static Dictionary<string, string> DescAddrDic = new Dictionary<string, string>();
        public static Dictionary<string, string> ValueAddrDic1 = new Dictionary<string, string>();
        public static Dictionary<string, string> ValueAddrDic2 = new Dictionary<string, string>();
        public static Dictionary<string, string> ValueAddrDic3 = new Dictionary<string, string>();
        public static Dictionary<string, string> ErrorCodeDic = new Dictionary<string, string>();
        //public static Dictionary<string, string> ErrorCodeMapPlcDic = new Dictionary<string, string>();
        public static ExecutionResult GetErrorCodeInfo(string stationName)
        {

            ExecutionResult exeResult = new ExecutionResult();
            //查询站别不良参数表
            exeResult = ErrorCodeDAO.GetErrorInfo(stationName);
            if (exeResult.Status)
            {
                DataSet ds = (DataSet)exeResult.Anything;
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    ErrorCodeDic.Clear();
                    DescAddrDic.Clear();
                    //ErrorCodeMapPlcDic.Clear();
                    ValueAddrDic1.Clear();
                    ValueAddrDic2.Clear();
                    ValueAddrDic3.Clear();
                    //设置不良代码与不良描述的键值对关系
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        string errorCode = ds.Tables[0].Rows[i]["ERROR_CODE"].ToString();
                        string errorDesc = ds.Tables[0].Rows[i]["ERROR_DESC"].ToString();
                        if (!ErrorCodeDic.ContainsKey(errorCode))
                        {
                            ErrorCodeDic.Add(errorCode, errorDesc);
                            //设置默认的PLC文本与值的地址键值对关系
                            string value = "PG.LineA.FirstMan.BadType" + (i + 1);
                            string addr1 = "PG.LineA.Xray.ABadType" + (i + 1);
                            string addr2 = "PG.LineA.Xray.BBadType" + (i + 1);
                            string addr3 = "PG.LineA.Xray.CBadType" + (i + 1);
                            DescAddrDic.Add(errorCode, value);
                            ValueAddrDic1.Add(errorCode, addr1);
                            ValueAddrDic2.Add(errorCode, addr2);
                            ValueAddrDic3.Add(errorCode, addr3);
                        }

                    }
                }
                else
                {
                    exeResult.Status = false;
                    exeResult.Message = "没有查询到不良参数信息，请找相关人员维护！";
                }
            }

            return exeResult;
        }



    }
}
