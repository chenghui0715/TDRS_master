using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ICMS.Modules.BaseComponents;
using ICMS.Modules.BaseComponents.Beans;
using ICMS.Modules.BaseComponents.Commons;
using ICMS.Modules.BaseComponents.IDAO;
using ICMS.Modules.Components.DAO;
using System.Data;
using ICMS.Modules.Components.Commons;
using TLAgent.OpcLibrary;

namespace ICMS.Modules.Components.Controls
{
    public class FirstManOperationController : ModuleController
    {

        private readonly CheckRouteController _routeController;
        private const string ProductSerial = "PG.LineA.FirstMan.Serial_Number_From_MES";
        private const string Sn = "PG.LineA.FirstMan.Sn_From_MES";
        private const string ProductType = "PG.LineA.FirstMan.Product_Type_From_MES";
        private const string Pallet = "PG.LineA.FirstMan.Pallet_From_MES";
        private const string ScanFinish = "PG.LineA.FirstMan.ScanFinish_From_MES";
        private const string ErrorCodeDescCount = "PG.LineA.FirstMan.ErrorCode_Count_From_MES";

        private const string IsUpsetting = "PG.LineA.FirstMan.Upsetting_From_MES";
        private const string IsSprayPainting = "PG.LineA.FirstMan.SprayPainting_From_MES";
        private const string IsXray = "PG.LineA.FirstMan.Xray_From_MES";
        private const string IsFirstVacuum = "PG.LineA.FirstMan.First_Vacuum_From_MES";
        private const string AutoRun = "PG.LineA.FirstMan.AutoRun1";
        private const string ScanCount = "PG.LineA.FirstMan.ScanCount";
        private const string BadMode = "PG.LineA.FirstMan.BadMode";

        private EQItem _item;
        ExecutionResult _exeResult = new ExecutionResult();//1)设备网络连接

        private UpdateStationController _updateStationController;
        public FirstManOperationController(EQItem item)
            : base(item)
        {

            Item = item;
            _routeController = new CheckRouteController();
            _updateStationController = new UpdateStationController();
            KepController = new KepController(Item, new ArrayList());


        }

        public EQItem Item
        {
            get { return _item; }
            set { _item = value; }
        }
        public override ExecutionResult Check(object dataParam)
        {
            _exeResult = (ExecutionResult)dataParam;
            try
            {
                string sn = _exeResult.Sn;
                string stationName = _exeResult.StationName;
                bool mode = _exeResult.IsAlive;
                string cproductType = _exeResult.ProductType;
                if (sn == "")
                {
                    _exeResult.Status = false;
                    _exeResult.Message = "管号为空!";
                    return _exeResult;
                }
                if (stationName == "")
                {
                    _exeResult.Status = false;
                    _exeResult.Message = "站点名为空!";
                    return _exeResult;
                }

                _exeResult = _routeController.CheckSn(stationName, sn,mode);
                if (_exeResult.Status)
                {
                    #region 查询x射线工位不良描述
                    if (ErrorCodeHelper.ErrorCodeDic.Count <= 0 || ErrorCodeHelper.DescAddrDic.Count <= 0)
                    {
                        _exeResult = ErrorCodeHelper.GetErrorCodeInfo("X射线工位");
                    }
                    if (!_exeResult.Status)
                    {
                        return _exeResult;
                    }
                    #endregion

                    #region 查询是否墩粗、喷漆
                    string isUpsetting ="False";
                    string isSprayPainting = "False";
                    string isXray = "True";
                    string isFirstVacuum = "True";
                    if (_exeResult.Status)
                    {
                        _exeResult = RouteDAO.GetRouteInfo(sn);
                        DataSet ds = _exeResult.Anything as DataSet;
                        if (ds != null && ds.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                string name = dr["STATION_NAME"].ToString();
                                if (name == "喷漆")
                                {
                                    isSprayPainting = "True";   
                                }
                                if (name == "墩粗")
                                {
                                    isUpsetting = "True";
                                }
                                if (name == "第一次真空度测试")
                                {
                                    isFirstVacuum = "False";
                                }
                                if (name == "X射线工位")
                                {
                                    isXray = "False";
                                }
                            }
                        }
                        if (!_exeResult.Status)
                        {
                            _exeResult.Status = false;
                            _exeResult.Message = "未找到管号：" + sn + "对应的流程!";
                            return _exeResult;
                        }
                    }
                    #endregion

                    #region 查询管型编号
                    string productSerial = "";
                    
                    if (_exeResult.Status)
                    {
                        _exeResult = _routeController.GetProductSerial(sn);
                        
                        if (_exeResult.Status)
                        {
                            productSerial = (string)_exeResult.Anything;
                        }
                        if (!_exeResult.Status)
                        {
                            return _exeResult;
                        }
                    }
                  
                    #endregion

                    #region 查询管型
                    string productType = "";
                    if (_exeResult.Status)
                    {
                        _exeResult = _routeController.GetProductType(sn);
                        if (_exeResult.Status)
                        {
                            productType = (string)_exeResult.Anything;
                            if (productType!=cproductType)
                            {
                                _exeResult.Message = "产线管型:"+cproductType+"与上线维护管型:"+productType +"不一致";
                                _exeResult.Status = false;
                                return _exeResult;
                            }
                        }
                        if (!_exeResult.Status)
                        {
                            return _exeResult;
                        }
                    }
                    #endregion

                    #region 查询托盘大小

                    string pallet = "";
                    if (_exeResult.Status)
                    {
                        _exeResult = _routeController.GetPalletByProductType(productType);
                        if (_exeResult.Status)
                        {
                            pallet = (string)_exeResult.Anything;
                            if (pallet=="1")
                            {
                                pallet ="True";
                            }
                            else
                            {
                                pallet = "False";
                            }
                        }
                        if (!_exeResult.Status)
                        {
                            return _exeResult;
                        }
                    }
                    #endregion

                    if (KepController.KepHelper != null && KepController.KepHelper.State)
                    {   //给pLC发送管型、管号、管型编号、是否墩粗、是否喷漆
                        var autorun = (KepController.KepHelper.Read(AutoRun) ?? "").ToString();
                        if (autorun == "False")
                        {
                            _exeResult.Status = false;
                            _exeResult.Message = "线体不在自动运行模式，无法扫描录入!";
                            return _exeResult;
                        }

                        var badMode = (KepController.KepHelper.Read(BadMode) ?? "").ToString();
                        if (badMode == "True")
                        {
                            _exeResult.Status = false;
                            _exeResult.Message = "线体在废品上线模式，无法扫描录入!";
                            return _exeResult;
                        }
                        var productSerialIsOk= (KepController.KepHelper.Read(ProductSerial) ?? "").ToString();

                        if (productSerialIsOk == productSerial)
                        {
                            var palletisok = (KepController.KepHelper.Read(Pallet) ?? "").ToString();
                            if (palletisok == "True")
                            {
                                int scanCount = int.Parse(KepController.KepHelper.Read(ScanCount).ToString());
                                if (scanCount >= 12)
                                {
                                    _exeResult.Status = false;
                                    _exeResult.Message = "大托盘已装满" + scanCount + "支，无法再次扫描录入!";
                                    return _exeResult;
                                }
                            }
                            if (palletisok == "False")
                            {
                                int scanCount = int.Parse(KepController.KepHelper.Read(ScanCount).ToString());
                                if (scanCount >= 24)
                                {
                                    _exeResult.Status = false;
                                    _exeResult.Message = "小托盘已装满" + scanCount + "支，无法再次扫描录入!";
                                    return _exeResult;
                                }
                            }
                        }
                        if (productSerialIsOk != productSerial)
                        {
                            int scanCount = int.Parse(KepController.KepHelper.Read(ScanCount).ToString());
                            if (scanCount!=0)
                            {
                                _exeResult.Status = false;
                                _exeResult.Message = "一个托盘内不允许放不同管型!";
                                return _exeResult;
                            }
                        }
                       
                        KepController.KepHelper.Write(ProductSerial, productSerial);
                        KepController.KepHelper.Write(ProductType, productType);
                        KepController.KepHelper.Write(Pallet, pallet);
                        KepController.KepHelper.Write(IsSprayPainting,isSprayPainting);
                        KepController.KepHelper.Write(IsUpsetting, isUpsetting);
                        KepController.KepHelper.Write(IsFirstVacuum, isFirstVacuum);
                        KepController.KepHelper.Write(IsXray, isXray); 
                        //给PLC发送不良信息描述
                        foreach (var key in ErrorCodeHelper.ErrorCodeDic.Keys)
                        {
                            string wkey = ErrorCodeHelper.DescAddrDic[key];
                            // string wval = ErrorCodeHelper.ErrorCodekeyDic[key];
                            string wval = key.Substring(key.LastIndexOf('-')+1);
                            KepController.KepHelper.Write(wkey, wval );
                        }
                        KepController.KepHelper.Write(ErrorCodeDescCount, ErrorCodeHelper.ErrorCodeDic.Count);
                        #region 判断发送PLC值是否正确
                        var productSerialok = (KepController.KepHelper.Read(ProductSerial) ?? "").ToString();
                        if (productSerial != productSerialok)
                        {
                            _exeResult.Status = false;
                            _exeResult.Message = "发送管型序号失败!";
                            return _exeResult;
                        }
                        var productTypeok = (KepController.KepHelper.Read(ProductType) ?? "").ToString();
                        if (productType != productTypeok)
                        {
                            _exeResult.Status = false;
                            _exeResult.Message = "发送管型失败!";
                            return _exeResult;
                        }
                        var palletok = (KepController.KepHelper.Read(Pallet) ?? "").ToString();
                        if (pallet != palletok)
                        {
                            _exeResult.Status = false;
                            _exeResult.Message = "发送托盘类型失败!";
                            return _exeResult;
                        }
                        var isSprayPaintingok = (KepController.KepHelper.Read(IsSprayPainting) ?? "").ToString();
                        if (isSprayPainting != isSprayPaintingok)
                        {
                            _exeResult.Status = false;
                            _exeResult.Message = "发送是否喷漆失败!";
                            return _exeResult;
                        }

                        var isUpsettingok = (KepController.KepHelper.Read(IsUpsetting) ?? "").ToString();
                        if (isUpsetting != isUpsettingok)
                        {
                            _exeResult.Status = false;
                            _exeResult.Message = "发送是否墩粗失败!";
                            return _exeResult;
                        }

                        var isFirstVacuumok = (KepController.KepHelper.Read(IsFirstVacuum) ?? "").ToString();
                        if (isFirstVacuum != isFirstVacuumok)
                        {
                            _exeResult.Status = false;
                            _exeResult.Message = "发送是否第一次真空度失败!";
                            return _exeResult;
                        }

                        var isXrayok = (KepController.KepHelper.Read(IsXray) ?? "").ToString();
                        if (isXray != isXrayok)
                        {
                            _exeResult.Status = false;
                            _exeResult.Message = "发送是否x射线失败!";
                            return _exeResult;
                        }

                        var errorCodeDescCountok = (KepController.KepHelper.Read(ErrorCodeDescCount) ?? "").ToString();
                        if (ErrorCodeHelper.ErrorCodeDic.Count.ToString() != errorCodeDescCountok)
                        {
                            _exeResult.Status = false;
                            _exeResult.Message = "发送不良描述总数失败!";
                            return _exeResult;
                        }
                        #endregion

                       
                        if (_exeResult.Status)
                        {
                            _exeResult = _updateStationController.InsertSnToQulity(sn, isSprayPainting, isUpsetting);
                        }
                        if (_exeResult.Status)
                        {
                            _exeResult = _updateStationController.UpdateStationInfo(sn, stationName, "0");}
                       
                        
                        KepController.KepHelper.Write(ScanFinish, 1);
                        if (_exeResult.Status)
                        {
                            _exeResult.Message = "管号：" + sn + "过站成功!";
                        }
                        
                    }
                    else
                    {
                        _exeResult.Status = false;
                        _exeResult.Message = "未连接KEP服务器!";
                        return _exeResult;
                    }
                }
            }
            catch (Exception e)
            {
                _exeResult.Status = false;
                _exeResult.Message = e.Message;
            }
            return _exeResult;
        }
            
    }
}