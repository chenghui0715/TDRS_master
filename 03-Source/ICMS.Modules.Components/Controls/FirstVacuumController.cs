using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using ICMS.Modules.BaseComponents;
using ICMS.Modules.BaseComponents.Commons;
using ICMS.Modules.BaseComponents.IDAO;
using ICMS.Modules.Components.Commons;
using ICMS.Modules.Components.DAO;
using TLAgent.OpcLibrary;

namespace ICMS.Modules.Components.Controls
{
	class FirstVacuumController : ModuleController
	{
		private readonly CheckRouteController _routeController;
		private VacuumDAO _dao;
		private const string Sn = "PG.LineA.FirstVacuumTest.Sn";
        private const string ProductSerial = "PG.Linea.FirstVacuumTest.Serial_Number";
        private const string ScanTouch = "PG.LineA.FirstVacuumTest.FirstVacuumScanTouch";
        private const string ScanFinish = "PG.Linea.FirstVacuumTest.ScanFinish";
        private const string DoFinish = "PG.LineA.FirstVacuumTest.DoFinish";
        private const string ProductType = "PG.LineA.FirstVacuumTest.ProductType";
        private const string FirstVacuumValue = "PG.LineA.FirstVacuumTest.FirstVacuumTestValue";
        private const string FirstTestQuality = "PG.LineA.FirstVacuumTest.IsOk";
        private const string SaveFinish = "PG.LineA.FirstVacuumTest.SaveFinish";
        private const string GoBadLine = "PG.LineA.FirstVacuumTest.GoBadLine";
        private const string GoGoodLine = "PG.LineA.FirstVacuumTest.GoGoodLine";
        private const string UserName = "PG.LineA.FirstVacuumTest.UserName";
		private readonly UpdateStationController _updateStationController;
		private EQItem _item;

		public FirstVacuumController(EQItem item)
			: base(item)
		{
			_routeController = new CheckRouteController();
			Item = item;
			_updateStationController = new UpdateStationController();
			_dao = new VacuumDAO();
            var valList = new ArrayList { ScanTouch, DoFinish };
			KepController = new KepController(item,valList);
		

		}

		public EQItem Item
		{
			get { return _item; }
			set { _item = value; }
		}

		public override ExecutionResult Check(object dataParam)
		{
			var exeResult = (ExecutionResult)dataParam;
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
                        exeResult.Message = "管号:" + sn + "流程验证成功!";
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
                        exeResult.Message = "管号:" + sn + "流程验证成功!";
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
                    KepController.KepHelper.Write(ProductType, productType);
                    KepController.KepHelper.Write(Sn, sn);
                    var productSerialok = (KepController.KepHelper.Read(ProductSerial) ?? "").ToString();
                    if (productSerial != productSerialok)
                    {
                        exeResult.Status = false;
                        exeResult.Message = "发送管型序号失败!";
                        return exeResult;
                    }

                    var productTypeok = (KepController.KepHelper.Read(ProductType) ?? "").ToString();
                    if (productType != productTypeok)
                    {
                        exeResult.Status = false;
                        exeResult.Message = "发送管型失败!";
                        return exeResult;
                    }

                    var snok = (KepController.KepHelper.Read(Sn) ?? "").ToString();
                    if (sn != snok)
                    {
                        exeResult.Status = false;
                        exeResult.Message = "发送管号失败!";
                        return exeResult;
                    }
                    KepController.KepHelper.Write(ScanFinish, 1);
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
			    string message = exeResult.Message;
			    exeResult = _dao.GetWipErrorFlagBySn(sn);
                DataSet ds= (DataSet)exeResult.Anything;
			    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
			    {
			        string errorFlag = (ds.Tables[0].Rows[0]["ERROR_FLAG"] ?? "").ToString();
                    if (errorFlag == "1")
                    {
                        KepController.KepHelper.Write(GoBadLine, 1);
                        exeResult.Message = message+"回废品线!";
                    }
                    else
                    {
                        KepController.KepHelper.Write(GoBadLine, 1);
                        exeResult.Message = message + "产品流程不对回废品线!";
                    }
			    }  
			}
			return exeResult;
		}


		public ExecutionResult UpdateOrInsertTestData(string sn, string productType, string stationName, string testValue,string userName)
		{
			ExecutionResult exeResult = _dao.GetTestData(sn, stationName);
			if (exeResult.Status)
			{
				var ds = (DataSet)exeResult.Anything;
				if (ds != null && ds.Tables.Count > 0)
				{
					if (ds.Tables[0].Rows.Count > 0)
					{
						exeResult = _dao.UpdateTestData(testValue, sn, stationName,userName);
					}
					else
					{
						exeResult = _dao.InsertVacuumTestData(productType, sn, stationName, testValue,userName);
					}
				}
				else
				{
					exeResult.Status = false;
					exeResult.Message = "查询真空度测试数据失败!";
				}
			}
			return exeResult;
		}


        public ExecutionResult UpdateOrInsertLeakageData(string sn, string productType,  string testValue)
        {
            ExecutionResult exeResult = _dao.GetLeakageData(sn);
            if (exeResult.Status)
            {
                var ds = (DataSet)exeResult.Anything;
                if (ds != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        exeResult = _dao.UpdateP1LeakageData(testValue, sn);
                    }
                    else
                    {
                        exeResult = _dao.InsertP1LeakageData(productType, sn, testValue);
                    }
                }
                else
                {
                    exeResult.Status = false;
                    exeResult.Message = "查询真空度测试数据失败!";
                }
            }
            return exeResult;
        }

       

	    public override ExecutionResult SaveSn(object dataParam)
	    {
            var exeResult = (ExecutionResult)dataParam;
            string stationName = exeResult.StationName;
            var sn= KepController.KepHelper.Read(Sn).ToString();
            var productType = KepController.KepHelper.Read(ProductType).ToString();
            //var testValue = double.Parse(KepController.KepHelper.Read(FirstVacuumValue ?? "").ToString());
            var testValue = KepController.KepHelper.Read(FirstVacuumValue ?? "").ToString();
            var userName = (KepController.KepHelper.Read(UserName ?? "")).ToString();
            int a = Convert.ToInt16(testValue.Substring(testValue.LastIndexOf("E") + 2));
            decimal b = Convert.ToDecimal(testValue.Substring(0, 4));
            testValue = (b / (Convert.ToInt64(Math.Pow(10, a)))).ToString();
            
            var errorFlag = (KepController.KepHelper.Read(FirstTestQuality) ?? 0).ToString();
	        if (errorFlag=="False")
	        {
	            errorFlag = "0";
	        }
            if (errorFlag == "True")
	        {
                errorFlag = "1";
	        }
	        if (!string.IsNullOrEmpty(sn)&&sn!="0")
	        {
                exeResult = UpdateOrInsertTestData(sn, productType, stationName, testValue,userName);
                if (exeResult.Status)
                {
                    exeResult = _updateStationController.UpdateStationInfo(sn, stationName, errorFlag);
                    if (exeResult.Status)
                    {
                        exeResult = UpdateOrInsertLeakageData(sn,productType, testValue);
                        if (exeResult.Status)
                        {
                            if (errorFlag == "0")
                            {
                                errorFlag = "Y";
                            }
                            if (errorFlag == "1")
                            {
                                errorFlag = "N";
                            }
                            exeResult = _dao.UpdateP1Quality(sn, errorFlag);
                        }
                        if (exeResult.Status)
                        {
                            //给第一个管号发送保存完成信号
                            KepController.KepHelper.Write(SaveFinish, 1);
                        }
                        if (exeResult.Status)
                        {
                            exeResult.Message = "管号：" + sn + "过站成功!";
                        }
                    }
                    
                }
	        }
	        else
	        {
                exeResult.Message = "管号为空，请重新扫描！"  ;
	            exeResult.Status = false;
	        }
            
            return exeResult;
	    }
	}
}
