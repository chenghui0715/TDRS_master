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

namespace ICMS.Modules.Components.Controls
{
    class XRayController : ModuleController
    {
        private const string ProductSerial = "PG.LineA.Xray.Serial_Number_From_MES";

        private const string Sn = "PG.LineA.Xray.Sn_From_MES";
        private const string SN1 = "PG.LineA.Xray.Sn_To_MES1";
        private const string SN2 = "PG.LineA.Xray.Sn_To_MES2";
        private const string SN3 = "PG.LineA.Xray.Sn_To_MES3";

        private const string NG1 = "PG.LineA.Xray.Ng1_To_MES";
        private const string NG2 = "PG.LineA.Xray.Ng2_To_MES";
        private const string NG3 = "PG.LineA.Xray.Ng3_To_MES";

        private const string WARN1 = "PG.LineA.Xray.Warn1_To_MES";
        private const string WARN2 = "PG.LineA.Xray.Warn2_To_MES";
        private const string WARN3 = "PG.LineA.Xray.Warn3_To_MES";

        private const string SaveA = "PG.LineA.Xray.SaveA";
        private const string SaveB = "PG.LineA.Xray.SaveB";
        private const string SaveC = "PG.LineA.Xray.SaveC";

        private const string Commit = "PG.LineA.Xray.Commit_To_MES";

        private const string XrayScan = "PG.LineA.Xray.XrayScan";
        private const string GoBadLine = "PG.LineA.Xray.XrayScan_GoBadLine_Signal";


        private const string UserName = "PG.LineA.Xray.UserName";
        private const string PassWord = "PG.LineA.Xray.PassWord";
        private const string LoginCommit = "PG.LineA.Xray.LoginCommit";
        private const string Allowlogin = "PG.LineA.Xray.Allowlogin";
        private const string Forbitlogin = "PG.LineA.Xray.Forbitlogin";

        private const string GunDaoXiangTao = "PG.LineA.Xray.ROLLING_GUIDE_SLEEVE";
        private const string ScanFinish = "PG.LineA.Xray.ScanFinish_From_MES";

       

        private readonly XRayDAO _dao;
        private readonly CheckRouteController _routeController;
        private readonly UpdateStationController _updateStationController;
        private EQItem _item;
        public XRayController(EQItem item)
            : base(item)
        {
            Item = item;
            _dao = new XRayDAO();
            _routeController = new CheckRouteController();
            _updateStationController = new UpdateStationController();
            var valList = new ArrayList { XrayScan, Commit, LoginCommit };
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
                        //给pLC发送管号、管型编号(序号)
                        KepController.KepHelper.Write(ProductSerial, productSerial);
                        var productSerialok = (KepController.KepHelper.Read(ProductSerial) ?? "").ToString();
                        if (productSerial != productSerialok)
                        {
                            exeResult.Status = false;
                            exeResult.Message = "发送序号失败!";
                            return exeResult;
                        }
                        KepController.KepHelper.Write(Sn, sn);
                        var snok = (KepController.KepHelper.Read(Sn) ?? "").ToString();
                        if (sn != snok)
                        {
                            exeResult.Status = false;
                            exeResult.Message = "发送管号失败!";
                            return exeResult;
                        }
                        var gunDao = (KepController.KepHelper.Read(GunDaoXiangTao) ?? "").ToString();
                        if (gunDao=="True")
                        {
                            gunDao = "N";
                        }
                        if (gunDao == "False")
                        {
                            gunDao = "Y";
                        }
                        exeResult = _dao.UpdateGunDaoQuality(sn, gunDao);
                        if (exeResult.Status)
                        {
                            KepController.KepHelper.Write(ScanFinish, 1);
                        }
                        if (exeResult.Status)
                        {
                            exeResult.Message = "管号：" + sn + "过站成功!";
                        }
                        else
                        {
                            exeResult.Status = false;
                            exeResult.Message = "未发出扫描完成信号!";
                            return exeResult;
                        }
                       
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
                            exeResult.Message = "产品标记不良"+message + "回废品线!";
                        }
                        else
                        {
                            KepController.KepHelper.Write(GoBadLine, 1);
                            exeResult.Message = "流程不正确" + message + "废品线!";
                        }
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

        public override ExecutionResult SaveSn(object dataParam)
        {

            var exeResult = (ExecutionResult)dataParam;
            bool mode = exeResult.IsAlive;
            exeResult.Status = true;
            string flag = (string)exeResult.Anything;

            string stationName = exeResult.StationName;

            string [] msgList=new string[3];
            bool allStatus = true;
            bool ng1, ng2, ng3;
            bool warn1, warn2, warn3;
            var snA = KepController.KepHelper.Read(SN1).ToString() ?? "0";
            var snB = KepController.KepHelper.Read(SN2).ToString() ?? "0";
            var snC = KepController.KepHelper.Read(SN3).ToString() ?? "0";
            var userName = KepController.KepHelper.Read(UserName).ToString();
            #region  第一个Sn处理过程
            if (!string.IsNullOrWhiteSpace(snA) && !snA.Equals("0"))
            {
                #region 查询x射线工位不良描述
                if (ErrorCodeHelper.ErrorCodeDic.Count <= 0 || ErrorCodeHelper.DescAddrDic.Count <= 0)
                {
                    exeResult = ErrorCodeHelper.GetErrorCodeInfo("X射线工位");
                    
                }
                
                #endregion
                string strA = snA;
                if (exeResult.Status)
                {
                    if (exeResult.Status)
                    {
                        ng1 = bool.Parse(KepController.KepHelper.Read(NG1).ToString());
                       warn1 = bool.Parse(KepController.KepHelper.Read(WARN1).ToString());
                        if (ng1||warn1)
                        {
                            //读取各个不良信息
                            List<string> errorCodelistA = new List<string>();
                            foreach (var key in ErrorCodeHelper.ValueAddrDic1.Keys)
                            {
                                bool errorCodeKep = bool.Parse(KepController.KepHelper.Read(ErrorCodeHelper.ValueAddrDic1[key]).ToString());
                                if (errorCodeKep.Equals(true))
                                {
                                    errorCodelistA.Add(key);
                                }
                            }
                            if (errorCodelistA.Count > 0)
                            {
                                //为真插入log记录
                                if (ng1.Equals(true))
                                {
                                    exeResult = _dao.InsertErrorLogInfo(strA, stationName, errorCodelistA,userName);
                                    if (exeResult.Status)
                                    {
                                        exeResult.ErrorFlag = 1;
                                    }

                                }
                                //为真插入警告记录
                                if (warn1.Equals(true))
                                {
                                    exeResult = _dao.InsertWarningInfo(strA, stationName, errorCodelistA);
                                    if (exeResult.Status)
                                    {
                                        exeResult.ErrorFlag = 0;
                                    }
                                }
                            }
                        }
                        flag = exeResult.ErrorFlag.ToString();
                        
                        //跟新工位名称和错误标记
                        exeResult = _updateStationController.UpdateStationInfo(strA, stationName, flag);
                        //exeResult.Status = true;
                        if (exeResult.Status)
                        {
                            if (flag == "1")
                            {
                                flag = "N";
                            }
                            if (flag == "0")
                            {
                                flag = "Y";
                            }
                             
                        }
                        exeResult = _dao.UpdateXrayQuality(strA, flag);
                        
                        if (exeResult.Status)
                        {
                            //给第一个管号发送保存完成信号
                            KepController.KepHelper.Write(SaveA, 1);

                        }
                        if (exeResult.Status && flag == "0")
                        {
                            exeResult.Message = "管号：" + snA + "过站成功!";
                        }
                        if (exeResult.Status && flag == "1")
                        {

                            exeResult.Message = "管号：" + snA + "存在不良，请下线处理!";
                            allStatus = false;
                        }

                    }
                }
                
            }
            else
            {
                exeResult.Message = "第一列管号不存在！";
                allStatus = false;
            }
            msgList[0] = exeResult.Message ?? "";
            #endregion
            
            #region 第二个Sn处理过程

            if (!string.IsNullOrWhiteSpace(snB) && !snB.Equals("0"))
            {
                #region 查询x射线工位不良描述
                if (ErrorCodeHelper.ErrorCodeDic.Count <= 0 || ErrorCodeHelper.DescAddrDic.Count <= 0)
                {
                    exeResult = ErrorCodeHelper.GetErrorCodeInfo("X射线工位");
                }
                
                #endregion
                
                string strB = snB;
                if (exeResult.Status){
                    exeResult = _routeController.CheckSn(stationName, strB,mode);
                    if (exeResult.Status)
                    {
                        ng2 = bool.Parse(KepController.KepHelper.Read(NG2).ToString());
                        warn2 = bool.Parse(KepController.KepHelper.Read(WARN2).ToString());
                        if (ng2||warn2)
                        {
                            //读取各个不良信息
                            List<string> errorCodelistB = new List<string>();
                            foreach (var key in ErrorCodeHelper.ValueAddrDic2.Keys)
                            {
                                bool errorCodeKep = bool.Parse(KepController.KepHelper.Read(ErrorCodeHelper.ValueAddrDic2[key]).ToString());
                                if (errorCodeKep.Equals(true))
                                {
                                    errorCodelistB.Add(key);
                                }
                            }
                            if (errorCodelistB.Count > 0)
                            {
                                //为真插入log记录
                                if (ng2.Equals(true))
                                {
                                    exeResult = _dao.InsertErrorLogInfo(strB, stationName, errorCodelistB, userName);
                                    if (exeResult.Status)
                                    {
                                        exeResult.ErrorFlag = 1;
                                    }

                                }
                                //为真插入警告记录
                                if (warn2.Equals(true))
                                {
                                    exeResult = _dao.InsertWarningInfo(strB, stationName, errorCodelistB);
                                    if (exeResult.Status)
                                    {
                                        exeResult.ErrorFlag = 0;
                                    }
                                }
                            }
                        }
                        flag = exeResult.ErrorFlag.ToString();
                       
                        //跟新工位名称和错误标记
                        exeResult = _updateStationController.UpdateStationInfo(strB, stationName, flag);
                        //exeResult.Status = true;
                        if (exeResult.Status)
                        {
                            if (flag == "1")
                            {
                                flag = "N";
                            }
                            if (flag == "0")
                            {
                                flag = "Y";
                            }
                            
                        }
                        exeResult = _dao.UpdateXrayQuality(strB, flag);
                        if (exeResult.Status)
                        {
                            //给第二个管号发送保存完成信号
                            KepController.KepHelper.Write(SaveB, 1);
                        }
                        if (exeResult.Status && flag == "0")
                        {
                            exeResult.Message = exeResult.Message + "管号：" + snB + "过站成功!";
                        }
                        if (exeResult.Status && flag == "1")
                        {

                            exeResult.Message = exeResult.Message + "管号：" + snB + "存在不良，请下线处理!";
                            allStatus = false;
                        }
                    }
                }
           
            }
            else
            {
                exeResult.Message ="第二列管号不存在！";
                allStatus = false;
            }
            msgList[1] = exeResult.Message ?? "";
            #endregion
            
            #region 第三个Sn处理过程

            if (!string.IsNullOrWhiteSpace(snC) && !snC.Equals("0"))
            {
                #region 查询x射线工位不良描述
                if (ErrorCodeHelper.ErrorCodeDic.Count <= 0 || ErrorCodeHelper.DescAddrDic.Count <= 0)
                {
                    exeResult = ErrorCodeHelper.GetErrorCodeInfo("X射线工位");
                }
                if (!exeResult.Status)
                {
                    return exeResult;
                }
                #endregion
                
                string strC = snC;
                exeResult = _routeController.CheckSn(stationName, strC,mode);
                if (exeResult.Status)
                {
                   ng3 = bool.Parse(KepController.KepHelper.Read(NG3).ToString()) ;
                   warn3 =bool.Parse(KepController.KepHelper.Read(WARN3).ToString());
                 

                    if ( ng3||warn3)
                    {
                        //读取各个不良信息
                        List<string> errorCodelistC = new List<string>();
                        foreach (var key in ErrorCodeHelper.ValueAddrDic3.Keys)
                        {
                            bool errorCodeKep = bool.Parse(KepController.KepHelper.Read(ErrorCodeHelper.ValueAddrDic3[key]).ToString());
                            if (errorCodeKep.Equals(true))
                            {
                                errorCodelistC.Add(key);
                            }
                        }
                        if (errorCodelistC.Count > 0)
                        {
                            //为真插入log记录
                            if (ng3.Equals(true))
                            {
                                exeResult = _dao.InsertErrorLogInfo(strC, stationName, errorCodelistC, userName);
                                if (exeResult.Status)
                                {
                                    exeResult.ErrorFlag = 1;
                                }

                            }
                            //为真插入警告记录
                            if (warn3.Equals(true))
                            {
                                exeResult = _dao.InsertWarningInfo(strC, stationName, errorCodelistC);
                                if (exeResult.Status)
                                {
                                    exeResult.ErrorFlag = 0;
                                }
                            }}
                    }
                    flag = exeResult.ErrorFlag.ToString();
                    //跟新工位名称和错误标记
                    exeResult = _updateStationController.UpdateStationInfo(strC, stationName, flag);
                    //exeResult.Status = true;
                    if (exeResult.Status)
                    {
                        if (flag == "1")
                        {
                            flag = "N";
                        }
                        if (flag == "0")
                        {
                            flag = "Y";
                        }
                        
                    }
                    exeResult = _dao.UpdateXrayQuality(strC, flag);
                    if (exeResult.Status)
                    {
                        //给第三个管号发送保存完成信号
                        KepController.KepHelper.Write(SaveC, 1);
                    }
                    if (exeResult.Status && flag == "0")
                    {
                        exeResult.Message =  exeResult.Message+"管号：" + snC + "过站成功!";
                    }
                    if (exeResult.Status && flag == "1")
                    {
                        
                        exeResult.Message = exeResult.Message+ "管号：" + snC + "存在不良，请下线处理!";
                        allStatus = false;
                    }
                }
            }
            else
            {
                exeResult.Message ="第三列管号不存在！";
                allStatus = false;
            }
            #endregion
            exeResult.Status = allStatus;
            msgList[2] = exeResult.Message ?? "";
            exeResult.Message = msgList[0]+msgList[1]+msgList[2];
            return exeResult;
        }

         public override ExecutionResult CheckLogin(object dataParam)
         {
             var exeResult = (ExecutionResult)dataParam;
             string userName = KepController.KepHelper.Read(UserName).ToString();
             string passWord = KepController.KepHelper.Read(PassWord).ToString();
             if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(passWord))
             {
                 exeResult = _dao.GetUserInfo(userName, passWord);
                 if (exeResult.Status)
                 {
                     var ds = (DataSet) exeResult.Anything;
                     if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                     {
                         KepController.KepHelper.Write(Allowlogin, 1);
                         exeResult.Message = "用户名：" + userName + "登录成功!";
                     }
                     else
                     {
                         KepController.KepHelper.Write(Forbitlogin, 1);
                         exeResult.Message = "用户名：" + userName + "密码不匹配!";
                     }

                 }
             }
             else
             {
                 exeResult.Status = false;exeResult.Message = "用户名与密码不完全!";
             }
             return exeResult;
         }
    }
}
