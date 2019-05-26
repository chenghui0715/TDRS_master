using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using ICMS.Modules.BaseComponents;
using ICMS.Modules.BaseComponents.Commons;
using ICMS.Modules.BaseComponents.IDAO;
using ICMS.Modules.Components.Commons;
using ICMS.Modules.Components.DAO;
using TLAgent.OpcLibrary;

namespace ICMS.Modules.Components.Controls
{
	internal class SecondVacuumController : ModuleController
	{
		private readonly VacuumDAO _dao;
		private readonly CheckRouteController _routeController;
		private EQItem _item;

        private const string Sn = "PG.LineA.SecondVacuumTest.Sn";
        private const string ProductType = "PG.LineA.SecondVacuumTest.ProductType";
        private const string ScanTouch = "PG.LineA.SecondVacuumTest.SecondVacuumScanTouch";
        private const string ScanFinish = "PG.Linea.SecondVacuumTest.ScanFinish";
        private const string DoFinish = "PG.LineA.SecondVacuumTest.DoFinish";
        private const string FirstVacuumValue = "PG.LineA.SecondVacuumTest.FirstVacuumTestValue";
        private const string SecondVacuumValue = "PG.LineA.SecondVacuumTest.SecondVacuumTestValue";
        private const string SecondTestQuality = "PG.LineA.SecondVacuumTest.IsOk";
        private const string SaveFinish = "PG.LineA.SecondVacuumTest.SaveFinish";
        private const string GoBadLine = "PG.LineA.SecondVacuumTest.GoBadLine";
        private const string GoGoodLine = "PG.LineA.SecondVacuumTest.GoGoodLine";
        private const string UserName = "PG.LineA.SecondVacuumTest.UserName";

		private UpdateStationController _updateStationController;

		public SecondVacuumController(EQItem item)
			: base(item)
		{

			Item = item;
			_dao = new VacuumDAO();
			_routeController = new CheckRouteController();
			_updateStationController = new UpdateStationController();
            var valList = new ArrayList();
            valList.Add(ScanTouch);
		    valList.Add(DoFinish);
            KepController = new KepController(Item, valList);

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
                #region 发送第一次真空度数据
                if (KepController.KepHelper != null && KepController.KepHelper.State)
                {
                    KepController.KepHelper.Write(ProductType, productType);
                    KepController.KepHelper.Write(Sn, sn);
                    exeResult = _dao.GetVacuumValueBySn(sn);
                    if (exeResult.Status)
                    {
                        var ds = (DataSet) exeResult.Anything;
                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            string vacuumValue = double.Parse(ds.Tables[0].Rows[0]["TEST_VALUE"].ToString()).ToString("#.##E+0");
                            if (vacuumValue == "1.01E-7")
                            {
                                vacuumValue = "<1E-6";
                            }
                            if (vacuumValue == "1.01E-2")
                            {
                                vacuumValue = ">1E-2";
                            }
                             
                            if (!string.IsNullOrWhiteSpace(vacuumValue))
                            {
                                KepController.KepHelper.Write(FirstVacuumValue, vacuumValue);
                            }
                        }
                        else
                        {
                            exeResult.Status = false;
                            exeResult.Message = "没有第一次真空度测试值记录！";
                            KepController.KepHelper.Write(GoBadLine, 1);return exeResult;
                        }
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
                #endregion
            }
            else
            {
                string message = exeResult.Message;
                exeResult = _dao.GetWipErrorFlagBySn(sn);
                DataSet ds = (DataSet)exeResult.Anything;
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    string errorFlag = (ds.Tables[0].Rows[0]["ERROR_FLAG"] ?? "").ToString();
                    if (errorFlag == "1")
                    {
                        KepController.KepHelper.Write(GoBadLine, 1);
                        exeResult.Message = message + "回废品线!";
                    }
                    else
                    {
                        KepController.KepHelper.Write(GoGoodLine, 1);
                        exeResult.Message = message + "产品回正常线!";
                    }
                }
            }
            return exeResult;
		}

        public override ExecutionResult SaveSn(object dataParam)
        {
            var exeResult = (ExecutionResult)dataParam;
            string stationName = exeResult.StationName;
            var sn = KepController.KepHelper.Read(Sn).ToString();
            var productType = KepController.KepHelper.Read(ProductType).ToString();
            var testValue = (KepController.KepHelper.Read(SecondVacuumValue ?? "")).ToString();
            int a = Convert.ToInt16(testValue.Substring(testValue.LastIndexOf("E") + 2));
            decimal b = Convert.ToDecimal(testValue.Substring(0, 4));
            testValue = (b / (Convert.ToInt64(Math.Pow(10, a)))).ToString();
            //var testValue = double.Parse(KepController.KepHelper.Read(SecondVacuumValue ?? "").ToString());
            var userName = (KepController.KepHelper.Read(UserName ?? "")).ToString();
            var errorFlag = (KepController.KepHelper.Read(SecondTestQuality) ?? 0).ToString();
            if (errorFlag == "False")
            {
                errorFlag = "0";
            }
            if (errorFlag == "True")
            {
                errorFlag = "1";
            }
            if (!string.IsNullOrEmpty(sn) && sn != "0")
            {
                exeResult = UpdateOrInsertTestData(sn, productType, stationName, testValue,userName);
                if (exeResult.Status)
                {
                    exeResult = _updateStationController.UpdateStationInfo(sn, stationName, errorFlag);
                    if (exeResult.Status)
                    {
                        exeResult = UpdateOrInsertLeakageData(sn, productType, testValue);
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
                            exeResult = _dao.UpdateP2Quality(sn, errorFlag);
                        }
                        
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
            else
            {
                exeResult.Message = "管号为空，请重新扫描！";
                exeResult.Status = false;
            }

            return exeResult;
        }

        public ExecutionResult UpdateOrInsertLeakageData(string sn, string productType, string testValue)
        {
            ExecutionResult exeResult = _dao.GetLeakageData(sn);
            if (exeResult.Status)
            {
                var ds = (DataSet)exeResult.Anything;
                if (ds != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        exeResult = _dao.UpdateP2LeakageData(testValue, sn);
                    }
                    else
                    {
                        exeResult = _dao.InsertP2LeakageData(productType, sn, testValue);
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

			return exeResult;

		}


	}
}
