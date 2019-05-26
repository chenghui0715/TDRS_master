using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ICMS.Modules.BaseComponents;
using ICMS.Modules.BaseComponents.IDAO;
using ICMS.Modules.Components.Commons;
using ICMS.Modules.Components.DAO;

namespace ICMS.Modules.Components.Controls
{
	public class LightningImpulseEQController : ModuleController
	{
		private readonly LightningImpulseEQDAO _dao;
		private readonly CheckRouteController _routeController;
		private UpdateStationController _updateStationController;

		private EQItem _item;
		public LightningImpulseEQController(EQItem item)
			: base(item)
		{
			Item = item;
			_dao = new LightningImpulseEQDAO();
			_routeController = new CheckRouteController();
			_updateStationController = new UpdateStationController();
			//1)设备网络连接
			//2)PLC连接
		}
		public EQItem Item
		{
			get { return _item; }
			set { _item = value; }
		}


        public override ExecutionResult LableViewSave(object dataParam)
        {
            var exeResult = (ExecutionResult)dataParam;
                try
                {
                    exeResult = _dao.GetIvcTestDataInfo();
                    var dsIvc = (DataSet)exeResult.Anything;
                    if (dsIvc != null && dsIvc.Tables[0].Rows.Count > 0 && dsIvc.Tables[0].Rows.Count > 0)
                    {
                        string[] msgList = new string[3];
                        bool allStatus = true;
                        for (int i = 0; i < dsIvc.Tables[0].Rows.Count; i++)
                        {
                            string sn = dsIvc.Tables[0].Rows[i]["SERIAL_NUMBER"].ToString();
                            string errorFlag = dsIvc.Tables[0].Rows[i]["ERROR_FLAG"].ToString();
                            string productType = dsIvc.Tables[0].Rows[i]["PRODUCT_TYPE"].ToString();
                            int mode = int.Parse(dsIvc.Tables[0].Rows[i]["MODE"].ToString());
                            string positiveValue = dsIvc.Tables[0].Rows[i]["POSITIVE_VALUE"].ToString();
                            string negativeValue = dsIvc.Tables[0].Rows[i]["NEGATIVE_VALUE"].ToString();
                            string pDischargeNumber = dsIvc.Tables[0].Rows[i]["P_DISCHARGE_NUMBER"].ToString();
                            string nDischargeNumber = dsIvc.Tables[0].Rows[i]["N_DISCHARGE_NUMBER"].ToString();
                            string userName = dsIvc.Tables[0].Rows[i]["USER_NAME"].ToString();
                            string stationName = dsIvc.Tables[0].Rows[i]["STATION_NAME"].ToString();
                            DateTime testTime = DateTime.Parse(dsIvc.Tables[0].Rows[i]["TEST_TIME"].ToString()); 
                            string positiveValueF = "";
                            string negativeValueF = "";
                            #region 插入服务器150雷电冲击测试数据
                            for (int j = 0; j < mode; j++)
                            {
                                string number = (j+1).ToString();
                                string[] positiveValueC = positiveValue.Split(',');
                                string[] negativeValueC = negativeValue.Split(',');
                                if (positiveValueC.Length==mode)
                                {
                                    positiveValueF = positiveValueC[j];    
                                }if (positiveValueC.Length == mode)
                                {
                                    negativeValueF = negativeValueC[j];
                                }
                                exeResult = _dao.InsertLightingImpulseInfo(sn, productType, number, positiveValueF, negativeValueF, userName, testTime);
                            }
                            #endregion

                            if (errorFlag!="1")
                            {
                                exeResult = _dao.UpdateQualityImpluseInfo(sn, pDischargeNumber, nDischargeNumber);
                                if (!exeResult.Status)
                                {
                                    allStatus = false;
                                    exeResult.Message = "插入雷电冲击正负极击穿次数失败";
                                }
                                exeResult = _updateStationController.UpdateStationInfo(sn, stationName, errorFlag);
                                if (exeResult.Status)
                                {
                                    exeResult.Message = "管号：" + sn + "过站成功！";
                                }
                               
                            }
                            else
                            {
                                exeResult = _updateStationController.UpdateStationInfo(sn, stationName, errorFlag);
                                if (exeResult.Status)
                                {
                                    allStatus = false;exeResult.Message = "管号：" + sn + "标记不良！"; ;
                                }
                                
                            }
                            
                            msgList[i] = exeResult.Message ?? "";
                            exeResult.Status = allStatus;
                        }

                        exeResult.Message = msgList[0] + msgList[1] + msgList[2];
                        exeResult.Status = allStatus;
                    }

                }
                catch (Exception e)
                {
                    exeResult.Status = false;
                    exeResult.Message = e.Message;
                }

            
            

            return exeResult;
        }

        //public override ExecutionResult Check(object dataParam)
        //{var exeResult = (ExecutionResult)dataParam;
        //    try
        //    {
        //        string sn = exeResult.Sn;
        //        string stationName = exeResult.StationName;
        //        if (sn == "")
        //        {
        //            exeResult.Status = false;
        //            exeResult.Message = "管号为空!";
        //            return exeResult;
        //        }
        //        if (stationName == "")
        //        {
        //            exeResult.Status = false;
        //            exeResult.Message = "站点名为空!";
        //            return exeResult;
        //        }

        //        exeResult = _routeController.CheckSn(stationName, sn);
        //        if (exeResult.Status)
        //        {

        //            exeResult = _dao.GetIvDataInfo(sn);
        //            if (exeResult.Status)
        //            {
        //                var ds = (DataSet)exeResult.Anything;
        //                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
        //                {
        //                    string brokendownNumbe = ds.Tables[0].Rows[0]["BrokendownNumber"].ToString();
        //                    bool qualified = Boolean.Parse(ds.Tables[0].Rows[0]["Qualified"].ToString());
        //                    if (!string.IsNullOrWhiteSpace(brokendownNumbe))
        //                    {
        //                        exeResult = _dao.InsertQualityInfo(sn, brokendownNumbe, qualified);
        //                    }
        //                    else
        //                    {
        //                        exeResult.Status = false;
        //                        exeResult.Message = "首次击穿电压为空!";
        //                    }

        //                }
        //                else
        //                {
        //                    exeResult.Status = false;
        //                    exeResult.Message = "没有查询到雷电冲击记录!";
        //                }
        //            }
        //            if (!exeResult.Status) return exeResult;

        //            exeResult = _updateStationController.UpdateStationInfo(sn, stationName, "0");
        //            if (exeResult.Status)
        //            {
        //                exeResult.Message = "管号：" + sn + "过站成功!";
        //            }

        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        exeResult.Status = false;
        //        exeResult.Message = e.Message;
        //    }


        //    return exeResult;
        //}


	}
}
