using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ICMS.Modules.BaseComponents;
using ICMS.Modules.Components.DAO;

namespace ICMS.Modules.Components.Commons
{
    public class UpdateStationController
    {
        private CommonsDAO _dao = new CommonsDAO();

        public ExecutionResult UpdateStationInfo(string sn, string currentStation, string errorFlag)
        {
            ExecutionResult exeResult = _dao.GetNextstation(sn, currentStation);

            if (exeResult.Status)
            {
                //不良处理流程
                if (errorFlag == "1")
                {

                    exeResult = _dao.Updateflag(errorFlag, sn);
                    if (exeResult.Status)
                    {
                        //exeResult = _dao.InsertWipLog(sn, errorFlag);
                        exeResult = _dao.GetWipLogInfo(sn, currentStation);
                        var dsWipLog = (DataSet)exeResult.Anything;
                        if (dsWipLog != null && dsWipLog.Tables.Count > 0 && dsWipLog.Tables[0].Rows.Count > 0)
                        {
                            exeResult = _dao.UpdateWipLog(sn, currentStation, errorFlag);
                        }
                        else
                        {
                            exeResult = _dao.InsertWipLog(sn, errorFlag);
                        }
                    }
                    else
                    {
                        exeResult.Status = false;
                        exeResult.Message = "管号:" + sn + "标记不良失败!";
                    }
                }
                else //良品处理流程
                {
                    var ds = (DataSet) exeResult.Anything;
                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        string nextStation = (ds.Tables[0].Rows[0]["NEXT_STATION_NAME"] ?? "").ToString();
                        exeResult = _dao.UpdateStation(currentStation, nextStation, errorFlag, sn);
                        if (exeResult.Status)
                        {
                            exeResult = _dao.GetWipLogInfo(sn, currentStation);
                            var dsWipLog = (DataSet) exeResult.Anything;
                            if (dsWipLog != null && dsWipLog.Tables.Count > 0 && dsWipLog.Tables[0].Rows.Count > 0)
                            {
                                exeResult = _dao.UpdateWipLog(sn, currentStation, errorFlag);
                            }
                            else
                            {
                                exeResult = _dao.InsertWipLog(sn, errorFlag);
                            }

                        }
                    }
                    else
                    {
                        exeResult.Status = false;
                        exeResult.Message = "获取管号:" + sn + "流程信息失败!";
                    }
                }
            }
            else
            {
                exeResult.Status = false;
                exeResult.Message = "获取当前工位以及下一站工位信息失败!";
            }

            return exeResult;
        }


        public ExecutionResult InsertSnToQulity(string sn, string isSprayPainting, string isUpsetting)
        {
            ExecutionResult exeResult = _dao.GetQualityInfo(sn);

            if (exeResult.Status)
            {
                if (isUpsetting == "True")
                {
                    isUpsetting = "Y";
                }
                else
                {
                    isUpsetting = "N";
                }
                if (isSprayPainting == "True")
                {
                    isSprayPainting = "Y";
                }
                else
                {
                    isSprayPainting = "N";
                }
                //更新quality表
                var ds = (DataSet) exeResult.Anything;
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                   
                    exeResult = _dao.UpdateQuality(sn, isSprayPainting, isUpsetting);
                }
                else
                {
                    exeResult = _dao.InsertQuality(sn, isSprayPainting, isUpsetting);
                }

                
            }
            return exeResult;
        }
    }
}
		

