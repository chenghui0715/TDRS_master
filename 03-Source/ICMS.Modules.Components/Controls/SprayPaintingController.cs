using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ICMS.Modules.BaseComponents;
using ICMS.Modules.BaseComponents.Commons;
using ICMS.Modules.BaseComponents.IDAO;
using ICMS.Modules.Components.Commons;
using ICMS.Modules.Components.DAO;

namespace ICMS.Modules.Components.Controls
{
	public class SprayPaintingController : ModuleController
	{
        private const string SprayPaintingTouch = "PG.LineA.SprayPainting.SprayPaintingScan";
        private const string GoPallet = "PG.LineA.SprayPainting.GoPallet";
        private const string ScanFinish = "PG.LineA.SprayPainting.ScanFinish";
        private const string ProductSerial = "PG.LineA.SprayPainting.Serial_Number_From_MES";
		private readonly CheckRouteController _routeController;
		private EQItem _item;
		private readonly UpdateStationController _updateStationController;
		public SprayPaintingController(EQItem item)
			: base(item)
		{
			Item = item;
			
			_routeController = new CheckRouteController();
			_updateStationController = new UpdateStationController();
			var valList = new ArrayList();
            valList.Add(SprayPaintingTouch);
			KepController = new KepController(item, valList);
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
                    #region 查询管型编号
                    string productSerial = "";

                    if (exeResult.Status)
                    {
                        exeResult = _routeController.GetProductSerial(sn);

                        if (exeResult.Status)
                        {
                            productSerial = (string)exeResult.Anything;
                        }
                        if (!exeResult.Status)
                        {
                            return exeResult;
                        }
                    }

                    #endregion
                    if (KepController.KepHelper != null && KepController.KepHelper.State)
                    {
                        KepController.KepHelper.Write(ProductSerial, productSerial);
                        var productSerialok = (KepController.KepHelper.Read(ProductSerial) ?? "").ToString();
                        if (productSerial != productSerialok)
                        {
                            exeResult.Status = false;
                            exeResult.Message = "发送管型序号失败!";
                            return exeResult;
                        }

                        exeResult = _updateStationController.UpdateStationInfo(sn, stationName, "0");
                        if (exeResult.Status)
                        {
                            KepController.KepHelper.Write(ScanFinish, 1);//资料更新成功，发送扫描完成
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
				else
				{
				    KepController.KepHelper.Write(GoPallet,1);//流程正确回托盘
				    exeResult.Status = false;
				    exeResult.Message = exeResult.Message;
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
