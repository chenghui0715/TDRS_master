using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ICMS.Modules.BaseComponents;
using ICMS.Modules.Components.DAO;

namespace ICMS.Modules.Components.Commons
{
	public class CheckRouteController
	{

		private CommonsDAO _dao = new CommonsDAO();
		public ExecutionResult CheckSn(string stationName, string sn,bool mode)
		{
			//流程逻辑代码
			string rproductType = "";
			ExecutionResult exeResult = _dao.GetWipInfo(sn);
		    exeResult.Message ="管号:"+sn+"流程验证OK!";
			DataSet ds;
			if (exeResult.Status){
				ds = (DataSet)exeResult.Anything;
			}
			else
			{
				return exeResult;
			}

			#region 1)管号验证

			if (ds == null || ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0)
			{
				exeResult.Status = false;
				exeResult.Message = "管号:" + sn + "不存在!";
				return exeResult;
			}

			#endregion

			#region 2)任务状态验证


			string taskStatus = ds.Tables[0].Rows[0]["TASK_STATUS"].ToString();
			string errorFlag = (ds.Tables[0].Rows[0]["ERROR_FLAG"] ?? "").ToString();
			string nextStation = (ds.Tables[0].Rows[0]["NEXT_STATION"] ?? "").ToString();
			string currentStation = (ds.Tables[0].Rows[0]["STATION_NAME"] ?? "").ToString();
			rproductType = (ds.Tables[0].Rows[0]["PRODUCT_TYPE"] ?? "").ToString();

			if (taskStatus != "正在执行")
			{
				exeResult.Status = false;
				exeResult.Message = "管号：" + sn + " 状态为" + taskStatus + "!";
				return exeResult;
			}

			#endregion.

			#region 3）当前工位验证


			if (errorFlag.Equals("1"))
			{
				exeResult.Status = false;
				exeResult.Message = "管号：" + sn + "已经打不良!";
				return exeResult;

			}


			if (rproductType == "")
			{
				exeResult.Status = false;
				exeResult.Message = "管号：" + sn + "管型为空!";
				return exeResult;
			}

			if (currentStation == "")
			{
				exeResult.Status = false;
				exeResult.Message = "管号流程异常，请找管理员维护!";
				return exeResult;
			}
		    if (mode==false)//正常流程全测
		    {
                if (nextStation == "")
                {
                    exeResult = GetNextStation(sn, currentStation);
                    if (exeResult.Status)
                    {
                        var nstation = (string)exeResult.Anything;
                        if (nstation == "")
                        {
                            exeResult.Status = false;
                            exeResult.Message = "管号：" + sn + "流程已经结束!";
                            return exeResult;
                        }
                        else if (currentStation != stationName)
                        {
                            exeResult.Status = false;
                            exeResult.Message = "管号：" + sn + "流程错误，请放入" + currentStation;
                            return exeResult;

                        }

                    }
                    else
                    {
                        return exeResult;
                    }
                }
                else if (nextStation != stationName)
                {
                    exeResult.Status = false;
                    exeResult.Message = "管号：" + sn + "流程错误，请放入" + nextStation;
                    return exeResult;
                }
		    }
		    else//正常流程，部分测试
		    {
                if (nextStation == "")
                {
                    exeResult = GetNextStation(sn, currentStation);
                    if (exeResult.Status)
                    {
                        var nstation = (string)exeResult.Anything;
                        if (nstation == "")
                        {
                            exeResult.Status = false;
                            exeResult.Message = "管号：" + sn + "流程已经结束!";
                            return exeResult;
                        }
                    }
                    else
                    {
                        return exeResult;
                    }
                }
		    }
			

			exeResult.Status = true;
			exeResult.Message = "流程正确!";
			exeResult.Anything = rproductType;

			#endregion
			return exeResult;
		}

	    public ExecutionResult GetProductSerial(string sn)
	    {

	        var exeResult = RouteDAO.GetProductSerialInfo(sn);
	        if (exeResult.Status)
	        {
	            var ds = (DataSet) exeResult.Anything;

	            if (ds!=null&&ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
	            {

	                string productSerial =ds.Tables[0].Rows[0]["PRODUCT_SERIAL"].ToString();
                    exeResult.Anything = productSerial;
	            }
	        }
	        return exeResult;
	    }

        public ExecutionResult GetProductType(string sn)
        {
            var exeResult = RouteDAO.GetProductTypeInfo(sn);
            if (exeResult.Status)
            {
                var ds = (DataSet)exeResult.Anything;

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {

                    string productType = ds.Tables[0].Rows[0]["PRODUCT_TYPE"].ToString();
                    exeResult.Anything = productType;
                }
            }
            return exeResult;
        }


       
            public ExecutionResult  GetPalletByProductType(string productType)
        {
            var exeResult = RouteDAO.GetPalletInfo(productType);
            if (exeResult.Status)
            {
                var ds = (DataSet)exeResult.Anything;

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {

                    string palletSize = ds.Tables[0].Rows[0]["PALLET_SIZE"].ToString();
                    exeResult.Anything = palletSize;
                }
            }
            return exeResult;
        }
	    public ExecutionResult GetNextStation(string sn, string currentStation)
		{
			var exeResult = _dao.GetNextstation(sn, currentStation);
			if (exeResult.Status)
			{
				var ds = (DataSet)exeResult.Anything;
				if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
				{
					string nextStation = (ds.Tables[0].Rows[0]["NEXT_STATION_NAME"] ?? "").ToString();
					exeResult.Anything = nextStation;
				}
				else
				{
					exeResult.Status = false;
					exeResult.Message = "管号：" + sn + "流程异常!";
					return exeResult;
				}
			}

			return exeResult;
		}
	}
}
