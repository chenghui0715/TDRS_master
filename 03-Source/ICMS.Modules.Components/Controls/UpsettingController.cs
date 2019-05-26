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
	public class UpsettingController : ModuleController
	{
		private EQItem _item;
		private readonly CheckRouteController _routeController;
		private UpdateStationController _updateStationController;
		public UpsettingController(EQItem item)
			: base(item)
		{
			Item = item;
			_routeController = new CheckRouteController();
			_updateStationController = new UpdateStationController();
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
					
					exeResult = _updateStationController.UpdateStationInfo(sn, stationName, "0");
					if (exeResult.Status)
					{
						exeResult.Message = "管号：" + sn + "过站成功!";
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
