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
	class VisualInspectionController : ModuleController
	{
	
		private const string NextStation = "Next_Station";
		private const string ProductType = "Product_Type";
		private readonly CheckRouteController _routeController;
		private UpdateStationController _updateStationController;
		private EQItem _item;
		public VisualInspectionController(EQItem item)
			: base(item)
		{
			Item = item;
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
				if (exeResult.Status)
				{
					var productType = (string)exeResult.Anything;
					string nextStation = "";
					exeResult = _routeController.GetNextStation(sn, stationName);
					if (exeResult.Status)
					{
						nextStation = (string)exeResult.Anything;
					}
					else
					{
						return exeResult;
					}
					if (KepController.KepHelper != null && KepController.KepHelper.State)
					{
						KepController.KepHelper.Write(ProductType, productType);
						KepController.KepHelper.Write(NextStation, nextStation);
						var type = (KepController.KepHelper.Read(ProductType) ?? "").ToString();
						var station = (KepController.KepHelper.Read(NextStation) ?? "").ToString();
						//if (productType != type)
						//{
						//    _exeResult.Status = false;
						//    _exeResult.Message = "发送管型失败!";
						//    return _exeResult;
						//}

						//if (nextStation != station)
						//{
						//    _exeResult.Status = false;
						//    _exeResult.Message = "发送下一站失败!";
						//    return _exeResult;
						//}

						exeResult = _updateStationController.UpdateStationInfo(sn, stationName, "0");
						if (exeResult.Status)
						{
							exeResult.Message = "管号：" + sn + "过站成功!";
						}

					}
					else
					{
						exeResult.Status = false;
						exeResult.Message = "未连接KEP服务器!";
						return exeResult;
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
