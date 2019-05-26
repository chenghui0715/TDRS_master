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
	public class VacuumRetestController : ModuleController
	{

        private const string Sn = "PG.LineA.VacuumRetest.Sn";

        private const string ProductType = "PG.LineA.VacuumRetest.ProductType";

        private const string ScanFinish = "PG.LineA.VacuumRetest.ScanFinish";

        private const string UnableScan = "PG.LineA.VacuumRetest.UnableScan";

        private const string VacuumRetestValue = "PG.LineA.VacuumRetest.VacuumRetestValue";

        private const string RetestQuality = "PG.LineA.VacuumRetest.IsOK";

        private const string DoFinish = "PG.LineA.VacuumRetest.DoFinish";

        private const string SaveFinish = "PG.LineA.VacuumRetest.SaveFinish";

        private const string FirstVacuumValue = "PG.LineA.VacuumRetest.FirstVacuumTestValue";

        private const string UserName = "PG.LineA.VacuumRetest.UserName";
		private VacuumRetestDAO _dao;
        private readonly CheckRouteController _routeController;
		private ExecutionResult _exeResult = new ExecutionResult();
		private EQItem _item;

		public VacuumRetestController(EQItem item)
			: base(item)
		{
			Item = item;
            _routeController = new CheckRouteController();
			KepController = new KepController(Item, new ArrayList());
            _dao=new VacuumRetestDAO();//1)设备网络连接
			//2)PLC连接
            var valList = new ArrayList { DoFinish };
            KepController = new KepController(item, valList);
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
				if (sn == "")
				{
					exeResult.Status = false;
					exeResult.Message = "管号为空!";
					return _exeResult;
				}
				if (stationName == "")
				{
					exeResult.Status = false;
					exeResult.Message = "站点名为空!";
					return _exeResult;
				}
                //#region 查询管型
                //string productType = "";
                //exeResult = _routeController.GetProductType(sn);
                //if (exeResult.Status){
                //    productType = (string)exeResult.Anything??"";
                //}
                //if (!exeResult.Status)
                //{
                //    return exeResult;
                //}
                //#endregion
                
                if (KepController.KepHelper != null && KepController.KepHelper.State)
                {
                    var unable = (KepController.KepHelper.Read(UnableScan) ?? "").ToString();
                    if (unable == "True")
                    {
                        exeResult.Status = false;
                        exeResult.Message = "真空度复测站有件，请处理完再次进行扫描!";
                        return exeResult;
                    }
                    KepController.KepHelper.Write(ProductType, exeResult.ProductType);
                    var productTypeok = (KepController.KepHelper.Read(ProductType) ?? "").ToString();
                    if (exeResult.ProductType != productTypeok)
                    {
                        exeResult.Status = false;
                        exeResult.Message = "发送管型失败!";//    return exeResult;
                    }
                    KepController.KepHelper.Write(Sn, sn);
                    var snok = (KepController.KepHelper.Read(Sn) ?? "").ToString();
                    if (sn != snok)
                    {
                        exeResult.Status = false;
                        exeResult.Message = "发送管号失败!";
                        return exeResult;
                    }
                    KepController.KepHelper.Write(ScanFinish, 1);
                    exeResult.Status = true;
                    exeResult.Message = "管号：" + sn + "扫描成功!";
                }
                else
                {
                    exeResult.Status = false;
                    exeResult.Message = "未连接KEP服务器!";
                    return exeResult;
                }
			}
			catch
				(Exception e)
			{
				exeResult.Status = false;
				exeResult.Message = e.Message;
			}
			return exeResult;
		}

        public override ExecutionResult SaveSn(object dataParam)
        {
            var exeResult = (ExecutionResult)dataParam;
            string stationName = exeResult.StationName;
            var sn = KepController.KepHelper.Read(Sn).ToString();
            var productType = KepController.KepHelper.Read(ProductType).ToString();
            var userName = (KepController.KepHelper.Read(UserName ?? "")).ToString();
            if (KepController.KepHelper != null && KepController.KepHelper.State)
            {
                var testValue = double.Parse(KepController.KepHelper.Read(VacuumRetestValue ?? "").ToString());
                var errorFlag = (KepController.KepHelper.Read(RetestQuality) ?? 0).ToString();
                if (errorFlag == "False")
                {
                    errorFlag = "0";
                }
                if (errorFlag == "True")
                {
                    errorFlag = "1";
                }
                exeResult = UpdateOrInsertTestData(sn, productType, stationName, testValue,errorFlag,userName);
                if (exeResult.Status)
                {
                    exeResult = UpdateOrInsertLeakageData(sn, productType, testValue);
                }
                if (exeResult.Status)
                {
                    //发送保存完成信号
                    KepController.KepHelper.Write(SaveFinish, 1);
                }
                if (exeResult.Status)
                {
                    exeResult.Message = "管号：" + sn + "真空度复测成功!";
                }
              
            }
            else
            {
                exeResult.Status = false;
                exeResult.Message = "未连接KEP服务器!";
                return exeResult;
            }
            return exeResult;
        }


		public ExecutionResult UpdateOrInsertTestData(string sn, string productType, string stationName, double testValue,string  errorFlag,string userName)
		{
			ExecutionResult exeResult = _dao.GetTestData(sn, stationName);
			if (exeResult.Status)
			{
				var ds = (DataSet)exeResult.Anything;
				if (ds != null && ds.Tables.Count > 0)
				{
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        exeResult = _dao.UpdateTestData(testValue, sn, stationName, userName);
                    }
                    else
                    {
                        exeResult = _dao.InsertVacuumTestData(productType, sn, stationName, testValue, userName);
                    }
				}
				else
				{
					exeResult.Status = false;
					exeResult.Message = "查询真空度测试数据失败!";
				}
			}
		    if (exeResult.Status)
		    {
                exeResult = _dao.GetWipLogInfo(sn, stationName);
                var dsWipLog = (DataSet)exeResult.Anything;
                if (dsWipLog != null && dsWipLog.Tables.Count > 0 && dsWipLog.Tables[0].Rows.Count > 0)
                {
                    exeResult = _dao.UpdateWipLog(sn, stationName, errorFlag);
                }
                else
                {
                    exeResult = _dao.InsertWipLog(sn, errorFlag);
                }
		    }
            
			return exeResult;
		}

        public ExecutionResult UpdateOrInsertLeakageData(string sn, string productType, double testValue)
        {
            ExecutionResult exeResult = _dao.GetLeakageData(sn);
            if (exeResult.Status)
            {
                var ds = (DataSet)exeResult.Anything;
                if (ds != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        exeResult = _dao.UpdateLeakageData(sn,testValue);
                    }
                    else
                    {
                        exeResult = _dao.InsertLeakageData(sn,productType, testValue);
                    }
                }
                else
                {
                    exeResult.Status = false;
                    exeResult.Message = "查询漏率真空度测试数据失败!";
                }
            }
            

            return exeResult;
        }
	}
}
