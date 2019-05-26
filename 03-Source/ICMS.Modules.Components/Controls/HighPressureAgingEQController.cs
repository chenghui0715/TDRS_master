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
	public class HighPressureAgingEQController : ModuleController
	{
		private readonly CheckRouteController _routeController;
		private UpdateStationController _updateStationController;
		private readonly HighPressureAgingEQDAO _dao;
		private EQItem _item;
		public HighPressureAgingEQController(EQItem item)
			: base(item)
		{
			Item = item;
			_dao = new HighPressureAgingEQDAO();
			_updateStationController = new UpdateStationController();
			_routeController = new CheckRouteController();
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
                exeResult = _dao.GetPfDataInfo();
                var dsPf = (DataSet)exeResult.Anything;
                if (dsPf != null && dsPf.Tables[0].Rows.Count > 0 && dsPf.Tables[0].Rows.Count > 0)
                {
                    string[] msgList = new string[6];
                    bool allStatus = true;
                    for (int i = 0; i < dsPf.Tables[0].Rows.Count; i++)
                    {
                        string sn = dsPf.Tables[0].Rows[i]["SERIAL_NUMBER"].ToString();
                        string productType = dsPf.Tables[0].Rows[i]["PRODUCT_TYPE"].ToString();
                        string stationName = dsPf.Tables[0].Rows[i]["STATION_NAME"].ToString();
                        string firstDischargeValue = dsPf.Tables[0].Rows[i]["FIRST_DISCHARGE_VALUE"].ToString();
                        string errorFlag = dsPf.Tables[0].Rows[i]["ERROR_FLAG"].ToString();
                        string userName = dsPf.Tables[0].Rows[i]["USER_NAME"].ToString();
                        DateTime testTime = DateTime.Parse(dsPf.Tables[0].Rows[i]["TEST_TIME"].ToString());
                        
                            if (errorFlag != "1")
                            {
                                errorFlag = "0";
                                exeResult = _dao.UpdateQualityHighDischargeInfo(sn, firstDischargeValue, errorFlag);
                                if (!exeResult.Status)
                                {
                                    allStatus = false;
                                    exeResult.Message = "插入首次击穿电压值失败";
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
                                    allStatus = false;
                                    exeResult.Message = "管号：" + sn + "标记不良！"; ;
                                }
                            }  
                        msgList[i] = exeResult.Message ?? "";
                        exeResult.Status = allStatus;
                    }

                    exeResult.Message = msgList[0] + msgList[1] + msgList[2]+msgList[3] + msgList[4] + msgList[5];
                    exeResult.Status = allStatus;
                }
                else
                {
                    exeResult.Message ="没查询到老炼测试完成数据";
                    exeResult.Status = false;
                }

            }
            catch (Exception e)
            {
                exeResult.Status = false;exeResult.Message = e.Message;
            }
            return exeResult;
        }    
	}
}