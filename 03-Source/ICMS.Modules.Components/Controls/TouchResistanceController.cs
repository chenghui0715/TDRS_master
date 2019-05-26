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
    class TouchResistanceController : ModuleController
    {
        private const string Sn = "PG.LineA.TouchResistance.Sn";
        
        private const string ProductType = "PG.LineA.TouchResistance.ProductType";

        private const string ScanFinish = "PG.LineA.TouchResistance.ScanFinish";

        private const string UnableScan = "PG.LineA.TouchResistance.UnableScan";
        private const string DoFinish = "PG.LineA.TouchResistance.DoFinish";

        private const string SaveFinish = "PG.LineA.TouchResistance.SaveFinish";

        private const string DianZuValue = "PG.LineA.TouchResistance.DianZu";
        private const string ZiBiLiValue = "PG.LineA.TouchResistance.ZiBiLi";
        private const string FanLiValue = "PG.LineA.TouchResistance.FanLi";
        private const string ZiBiLiIsOk = "PG.LineA.TouchResistance.ZiBiLiIsOk";
        private const string FanLiIsOk = "PG.LineA.TouchResistance.FanLiIsOk";
        private const string DianZuIsOk = "PG.LineA.TouchResistance.DianZuIsOk";
        private const string Mode = "PG.LineA.TouchResistance.Mode";
        private const string UserName = "PG.LineA.TouchResistance.UserName";
        private const string AutoRun = "PG.LineA.SecondMan.AutoRun2";
        private readonly CheckRouteController _routeController;
		private UpdateStationController _updateStationController;
        private EQItem _item;
        private TouchResistanceDAO _dao;
        public TouchResistanceController(EQItem item): base(item)
        {
            Item = item;
			
            _routeController = new CheckRouteController();
			_updateStationController=new UpdateStationController();
           
            _dao = new TouchResistanceDAO();
            //1)设备网络连接
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
            string sn = exeResult.Sn;
            string stationName = exeResult.StationName;
		    bool modeAll = exeResult.IsAlive;
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
            //模式选择条件： false:完全测试 ;true：部分测试
            exeResult = _routeController.CheckSn(stationName, sn, modeAll);
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
                        if (productType != cproductType)
                        {
                            exeResult.Message = "产线管型:" + cproductType + "与上线维护管型:" + productType + "不一致";
                            exeResult.Status = false;
                            return exeResult;
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
                    var unable = (KepController.KepHelper.Read(UnableScan) ?? "").ToString();
                    if (unable == "True")
                    {
                        exeResult.Status = false;
                        exeResult.Message = "接触电阻站有件，请处理完再次进行扫描!";
                        return exeResult;
                    }
                    KepController.KepHelper.Write(ProductType, productType);
                    KepController.KepHelper.Write(Sn, sn);
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
                }}
             return exeResult;
		}

        public override ExecutionResult SaveSn(object dataParam)
        {
            var exeResult = (ExecutionResult)dataParam;
            string stationName = exeResult.StationName;
            var sn = KepController.KepHelper.Read(Sn).ToString();
            var productType = KepController.KepHelper.Read(ProductType).ToString();
            var dianZuValue = Convert.ToDouble(KepController.KepHelper.Read(DianZuValue ?? "")).ToString("0.00");
            var dianZuIsOk = (KepController.KepHelper.Read(DianZuIsOk ?? "")).ToString();
            if (dianZuIsOk=="True")
            {
                dianZuIsOk = "N";
            }
            else
            {
                dianZuIsOk = "Y";
            }
            var ziBiLiValue = (KepController.KepHelper.Read(ZiBiLiValue) ?? 0).ToString();
            var ziBiLiIsOk = (KepController.KepHelper.Read(ZiBiLiIsOk ?? "")).ToString();
            if (ziBiLiIsOk == "True")
            {
                ziBiLiIsOk = "N";
            }
            else
            {
                ziBiLiIsOk = "Y";
            }
            var fanLiValue = (KepController.KepHelper.Read(FanLiValue) ?? 0).ToString();
            var fanLiIsOk = (KepController.KepHelper.Read(FanLiIsOk ?? "")).ToString();
            if (fanLiIsOk == "True")
            {
                fanLiIsOk = "N";
            }
            else
            {
                fanLiIsOk = "Y";
            }
            var userName = (KepController.KepHelper.Read(UserName ?? "")).ToString();
            var mode = (KepController.KepHelper.Read(Mode ?? "")).ToString();
            if (mode == "True")
            {
                mode = "全部测试";
            }else
            {
                mode = "只测电阻";
            }
            var errorFlag = "0";

            if (!string.IsNullOrEmpty(sn) && sn != "0")
            {
                exeResult = UpdateOrInsertTouchDataAll(sn, productType, dianZuValue, dianZuIsOk, ziBiLiValue, ziBiLiIsOk, fanLiValue, fanLiIsOk, mode, userName);

                if (exeResult.Status)
                {
                    exeResult = UpdateOrInsertQualityAll(sn, dianZuValue, ziBiLiValue, fanLiValue, mode);
                    if (exeResult.Status)
                    {
                        //判断是否为完全测试模式 为true是部分测试模式
                        if (exeResult.IsAlive == false)
                        {
                            exeResult = _updateStationController.UpdateStationInfo(sn, stationName, errorFlag);
                        }
                        if (exeResult.Status)
                        {
                            //发送保存完成信号
                            KepController.KepHelper.Write(SaveFinish, 1);
                        }
                        if (exeResult.Status)
                        {
                            exeResult.Message = "管号：" + sn + "过站成功!";
                        } 

                        
                    }
                    else
                    {
                        exeResult.Message = "质量测试报表插入接触电阻数据失败!";
                        exeResult.Status = false;
                    }
                    
                }
                else
                {
                    exeResult.Message = "接触电阻表数据更新失败!";
                    exeResult.Status = false;
                }
            }
            else
            {
                exeResult.Message = "管号为空，请重新扫描！";
                exeResult.Status = false;
            }

            return exeResult;
        }
        /// <summary>
        ///   插入与更新质量测试表
        /// </summary>
        /// <param name="sn">管号</param>
        /// <param name="dianZuValue">接触电阻值</param>
        /// <param name="ziBiLiValue">自闭力值</param>
        /// <param name="fanLiValue">反力值</param>
        /// <param name="mode">是否为完全自动模式</param>
        /// <returns></returns>
        public ExecutionResult UpdateOrInsertQualityAll(string sn, string dianZuValue, string ziBiLiValue, string fanLiValue, string mode)
        {
            ExecutionResult exeResult = _dao.GetQualityData(sn);
            if (exeResult.Status)
            {
                var ds = (DataSet)exeResult.Anything;
                if (ds != null&&ds.Tables[0].Rows.Count>0 && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        exeResult = _dao.UpdateQualityDataAll(sn, dianZuValue, ziBiLiValue, fanLiValue, mode);
                        if (!exeResult.Status)
                        {
                            return exeResult;
                        }
                    }
                   
                }
                else
                {
                    exeResult = _dao.InsertQualityDataAll(sn, dianZuValue, ziBiLiValue, fanLiValue, mode);
                    if (!exeResult.Status)
                    {
                        return exeResult;
                    }
                }
            }
            return exeResult;
        }

      
        /// <summary>
        ///   插入与更新接触电阻测试表
        /// </summary>
        /// <param name="sn">管号</param>
        /// <param name="productType">管型</param>
        /// <param name="dianZuValue">接触电阻值</param>
        /// <param name="dianZuIsOk">接触电阻是否合格</param>
        /// <param name="ziBiLiValue">自闭力值</param>
        /// <param name="ziBiLiIsOk">自闭力是否合格</param>
        /// <param name="fanLiValue">反力值</param>
        /// <param name="fanLiIsOk">反力是否合格</param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public ExecutionResult UpdateOrInsertTouchDataAll(string sn, string productType, string dianZuValue, string dianZuIsOk, string ziBiLiValue, string ziBiLiIsOk, string fanLiValue, string fanLiIsOk, string mode,string userName)
        {
            ExecutionResult exeResult = _dao.GetTestData(sn);
            if (exeResult.Status)
            {
                var ds = (DataSet) exeResult.Anything;
                if (ds != null && ds.Tables[0].Rows.Count > 0 && ds.Tables.Count > 0)
                {
                    exeResult = _dao.UpdateTouchDataAll(sn, productType, dianZuValue, dianZuIsOk, ziBiLiValue,
                                                           ziBiLiIsOk, fanLiValue, fanLiIsOk, mode, userName);
                    if (!exeResult.Status)
                    {
                        return exeResult;
                    }
                    
                }
                else
                {
                    exeResult = _dao.InsertTouchDataAll(sn, productType, dianZuValue, dianZuIsOk, ziBiLiValue,
                                                                 ziBiLiIsOk, fanLiValue, fanLiIsOk, mode, userName);
                    if (!exeResult.Status)
                    {
                        return exeResult;
                    }
                }
            }
            
            return exeResult;
        }
    }
}
