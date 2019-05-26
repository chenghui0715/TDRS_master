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
	public class HighPressureAgingController : ModuleController
	{
		private readonly HighPressureAgingDAO _dao;
		private EQItem _item;
		private readonly CheckRouteController _routeController;

		public HighPressureAgingController(EQItem item)
			: base(item)
		{
			Item = item;
			_dao = new HighPressureAgingDAO();
			_routeController = new CheckRouteController();
			
			//1)设备网络连接
			//2)PLC连接
		}
		public EQItem Item
		{
			get { return _item; }
			set { _item = value; }
		}


		public override ExecutionResult Check(object dataParam)
		{
			var exeResult = (ExecutionResult)dataParam;
			try
			{
                string sn = exeResult.Sn;
                string stationName = exeResult.StationName;
			    bool mode = exeResult.IsAlive;
                if (sn == "")
                {
                    exeResult.Status = false;
                    exeResult.Message = "管号为空!";
                    return exeResult;
                }
                if (stationName == "")
                {
                    exeResult.Status = false;
                    exeResult.Message = "站点名为空!";
                    return exeResult;
                }
                exeResult = _routeController.CheckSn(stationName, sn,mode);
                if (!exeResult.Status)
                {
                    return exeResult;
                }
                #region 查询管型
                string productType = "";
                exeResult = _routeController.GetProductType(sn);
                if (exeResult.Status)
                {
                    productType = (string)exeResult.Anything;

                }
                if (!exeResult.Status)
                {
                    return exeResult;
                }

                #endregion
                if (exeResult.Status)
                {
                    exeResult = _dao.GetPfCountInfo();//获取老炼未测试的数据条数
                    var dsCount = (DataSet)exeResult.Anything;
                    int count = 0;
                    if (dsCount != null && dsCount.Tables[0].Rows.Count > 0)
                    {
                        count = int.Parse(dsCount.Tables[0].Rows[0]["NUMBER"].ToString());
                    }

                    if (count < 12)
                    {
                        exeResult = _dao.GetPfDataInfo(sn);
                        if (exeResult.Status)
                        {
                            var ds = (DataSet)exeResult.Anything;
                            if (ds != null && ds.Tables[0].Rows.Count > 0)
                            {
                                DateTime createTime = Convert.ToDateTime(ds.Tables[0].Rows[0]["CREATE_TIME"] ?? "");
                                createTime = createTime + System.TimeSpan.FromMinutes(5);
                                if (DateTime.Now > createTime)
                                {
                                    exeResult = _dao.InsertPfDataInfo(sn, productType, stationName);
                                    if (exeResult.Status)
                                    {
                                        exeResult.Message = "管号："+sn+"已成功插入PF_DATA数据库中";
                                    }
                                    else
                                    {
                                        exeResult.Message = "未能将管号：" + sn + "插入PF_DATA数据库中";
                                    }
                                }
                                else
                                {
                                    exeResult = _dao.UpdatePfDataInfo(productType, sn, stationName);
                                    if (exeResult.Status)
                                    {
                                        exeResult.Message = "管号：" + sn + "已更新到PF_DATA数据库中";
                                    }
                                    else
                                    {
                                        exeResult.Message = "未能将管号：" + sn + "更新到PF_DATA数据库中";
                                    }
                                }

                            }
                            else
                            {
                                exeResult = _dao.InsertPfDataInfo(sn, productType, stationName);
                                if (exeResult.Status)
                                {
                                    exeResult.Message = "管号：" + sn + "已成功插入PF_DATA数据库中";
                                }
                                else
                                {
                                    exeResult.Message = "未能将管号：" + sn + "插入PF_DATA数据库中";
                                }}
                        }
                        else
                        {
                            exeResult.Status = false;
                            exeResult.Message = "查询高压老炼数据库失败";
                        }
                    }
                    else
                    {
                        exeResult.Status = false;
                        exeResult.Message = "已经扫描" + count + "条数据，请等待高压老炼测试完成！";
                    }
                }
			}
			catch (Exception e)
			{
				exeResult.Status = false;
				exeResult.Message = e.Message;
			}return exeResult;
		}




	}
}
