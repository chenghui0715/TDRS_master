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
	public class SecondVacuumManOperationController : ModuleController
	{
		private readonly CheckRouteController _routeController;
		private UpdateStationController _updateStationController;
        private const string ProductSerial = "PG.LineA.SecondMan.Serial_Number_From_MES";
        private const string Pallet = "PG.LineA.SecondMan.Pallet_From_MES";
        private const string ScanFinish = "PG.LineA.SecondMan.ScanFinish_From_MES";
        private const string AutoRun = "PG.LineA.SecondMan.AutoRun2";
        private const string ScanCount = "PG.LineA.SecondMan.ScanCount";
        private const string BadMode = "PG.LineA.SecondMan.BadMode";
		//private readonly SecondVacuumManOperationDAO _dao;
		private EQItem _item;
		public SecondVacuumManOperationController(EQItem item)
			: base(item)
		{
			Item = item;//_dao = new SecondVacuumManOperationDAO();
			_updateStationController = new UpdateStationController();
			_routeController = new CheckRouteController();
            KepController = new KepController(Item, new ArrayList());
			//1)设备网络连接
			//2)PLC连接
		}
		public EQItem Item{
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
                string cproductType = exeResult.ProductType;
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
                    #region 查询管型
                    string productType = "";
                    if (exeResult.Status)
                    {
                        exeResult = _routeController.GetProductType(sn);
                        if (exeResult.Status)
                        {
                            productType = (string)exeResult.Anything;
                            if (productType != cproductType)
                            {
                                exeResult.Message = "产线管型:" + cproductType + "与上线维护管型:" + productType + "不一致";
                                exeResult.Status = false;
                                return exeResult;
                            }}
                        if (!exeResult.Status)
                        {
                            return exeResult;
                        }
                    }
                    #endregion
                    #region 查询托盘大小

                    string pallet = "";
                    if (exeResult.Status)
                    {
                        exeResult = _routeController.GetPalletByProductType(productType);
                        if (exeResult.Status)
                        {
                            pallet = (string)exeResult.Anything;
                            if (pallet == "1")
                            {
                                pallet = "True";
                            }
                            else
                            {
                                pallet = "False";
                            }
                        }
                        if (!exeResult.Status)
                        {
                            return exeResult;
                        }
                    }
                    #endregion
					if (KepController.KepHelper != null && KepController.KepHelper.State)
					{
                        var autorun = (KepController.KepHelper.Read(AutoRun) ?? "").ToString();
                        if (autorun == "False")
                        {
                            exeResult.Status = false;
                            exeResult.Message = "线体不在自动运行模式，无法扫描录入!";
                            return exeResult;
                        }
                        var badMode = (KepController.KepHelper.Read(BadMode) ?? "").ToString();
                        if (badMode == "True")
                        {
                            exeResult.Status = false;
                            exeResult.Message = "线体在废品上线模式，无法扫描录入!";
                            return exeResult;
                        }
                        var productSerialIsOk = (KepController.KepHelper.Read(ProductSerial) ?? "").ToString();

                        if (productSerialIsOk == productSerial)
                        {
                            var palletisok = (KepController.KepHelper.Read(Pallet) ?? "").ToString();
                            if (palletisok == "True")
                            {
                                int scanCount = int.Parse(KepController.KepHelper.Read(ScanCount).ToString());
                                if (scanCount >= 12)
                                {
                                    exeResult.Status = false;
                                    exeResult.Message = "大托盘已装满" + scanCount + "支，无法再次扫描录入!";
                                    return exeResult;
                                }
                            }
                            if (palletisok == "False")
                            {
                                int scanCount = int.Parse(KepController.KepHelper.Read(ScanCount).ToString());
                                if (scanCount >= 24)
                                {
                                    exeResult.Status = false;
                                    exeResult.Message = "小托盘已装满" + scanCount + "支，无法再次扫描录入!";
                                    return exeResult;
                                }
                            }
                        }
                        if (productSerialIsOk != productSerial)
                        {
                            int scanCount = int.Parse(KepController.KepHelper.Read(ScanCount).ToString());
                            if (scanCount != 0)
                            {
                                exeResult.Status = false;
                                exeResult.Message = "一个托盘内不允许放不同管型!";
                                return exeResult;
                            }
                        }
                        KepController.KepHelper.Write(ProductSerial, productSerial);
                        KepController.KepHelper.Write(Pallet, pallet);
                        var productSerialok = (KepController.KepHelper.Read(ProductSerial) ?? "").ToString();
                        if (productSerial != productSerialok)
                        {
                            exeResult.Status = false;
                            exeResult.Message = "发送管型序号失败!";
                            return exeResult;
                        }
                        var palletok = (KepController.KepHelper.Read(Pallet) ?? "").ToString();
                        if (pallet != palletok)
                        {
                            exeResult.Status = false;
                            exeResult.Message = "发送托盘类型失败!";
                            return exeResult;
                        }
                        
                        KepController.KepHelper.Write(ScanFinish, 1);
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
