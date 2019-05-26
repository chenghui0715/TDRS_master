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
	public class LightningImpulseController : ModuleController
	{
		private readonly LightningImpulseDAO _dao;
		private readonly CheckRouteController _routeController;
		private EQItem _item;
		public LightningImpulseController(EQItem item)
			: base(item)
		{
			Item = item;
			_dao = new LightningImpulseDAO();
			_routeController = new CheckRouteController();
			new UpdateStationController();
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
                    return exeResult;}

                #endregion
				if (exeResult.Status)
				{
                    exeResult = _dao.GetIvcCountInfo();
                    var dsCount = (DataSet)exeResult.Anything;
                    int count =0;
                    if (dsCount != null && dsCount.Tables[0].Rows.Count > 0)
				    {
                        count = int.Parse(dsCount.Tables[0].Rows[0]["NUMBER"].ToString());
				    }
				    
				    if (count<3)
				    {
                        exeResult = _dao.GetIVDataInfo(sn);
                        if (exeResult.Status)
                        {
                            var ds = (DataSet)exeResult.Anything;
                            if (ds != null && ds.Tables[0].Rows.Count > 0)
                            {
                                DateTime createTime = Convert.ToDateTime(ds.Tables[0].Rows[0]["CREATE_TIME"] ?? "");
                                createTime = createTime + System.TimeSpan.FromMinutes(5);
                                if (DateTime.Now > createTime)
                                {
                                    exeResult = _dao.InsertIVDataInfo(sn, productType, stationName);
                                    if (exeResult.Status)
                                    {
                                        exeResult.Message = "管号：" + sn + "已成功插入IVCTEST_DATA数据库中";
                                    }
                                    else
                                    {
                                        exeResult.Message = "未能将管号：" + sn + "插入IVCTEST_DATA数据库中";
                                    }
                                }
                                else
                                {
                                    exeResult = _dao.UpdateIVDataInfo(productType, sn, stationName);
                                    if (exeResult.Status)
                                    {
                                        exeResult.Message = "管号：" + sn + "已成功更新IVCTEST_DATA数据库中";
                                    }
                                    else
                                    {
                                        exeResult.Message = "未能将管号：" + sn + "更新IVCTEST_DATA数据库中失败";
                                    }
                                }

                            }
                            else
                            {
                                exeResult = _dao.InsertIVDataInfo(sn, productType, stationName);
                                if (exeResult.Status)
                                {
                                    exeResult.Message = "管号：" + sn + "已成功插入IVCTEST_DATA数据库中";
                                }
                                else
                                {
                                    exeResult.Message = "未能将管号：" + sn + "插入IVCTEST_DATA数据库中";
                                }
                            }
                        }
                        else
                        {
                            exeResult.Status = false;
                            exeResult.Message = "查询雷电冲击数据库失败";
                        }
				    }
				    else
				    {
                        exeResult.Status = false;
                        exeResult.Message = "已经扫描"+count+"条数据，请等待雷电冲击测试完成！";
				    }

				    
                    
				}
			}
			catch (Exception e)
			{
				exeResult.Status = false;
				exeResult.Message = e.Message;
			}


			return exeResult;
		}

	}
}
