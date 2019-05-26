using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using YH.ICMS.Common;
using YH.ICMS.DAL;
using YH.ICMS.Entity;

namespace YH.ICMS.BLL
{
    public class VoltageBLL
    {

        private string CV_TableName = "C_VoltageParameter_T";

        VoltageDAL m_VoltageDAL = new VoltageDAL();
        /// <summary>
        /// 获得数据列表
        /// </summary>
        /// <param name="strWhere">限制条件</param>
        /// <returns>DataSet</returns>
        public DataSet GetVoltageList(string strWhere)
        {
            string sql = string.Format("SELECT [ID] , [PreVoltage] ,[CurVoltage] ,[VoltageLevel]  FROM[dbo].[{0}] order by ID desc", CV_TableName);
            return m_VoltageDAL.GetVoltageParaInfo(sql);
        }
        public IList<VM_VoltageParam> GetVoltageDsToListInfo()
        {
            IList<VM_VoltageParam> lst = IListDataSet.DataSetToIList<VM_VoltageParam>(GetVoltageList(""), 0);
            return lst;
        }
         


        public bool InsertVoltageParam(VM_VoltageParam vm_vp)
        {

            int count = 0;
            string sql = string.Format("INSERT INTO[dbo].[{0}]([VoltageLevel],[PreVoltage],[CurVoltage]) VALUES('{1}','{2}','{3}'); ", CV_TableName,  vm_vp.VoltageLevel, vm_vp.PreVoltage, vm_vp.CurVoltage);
            count = m_VoltageDAL.InsertCameraInfo(sql);
            return count > 0 ? true : false;
        }

        public bool UpdateVoltageParam(VM_VoltageParam vm_vp)
        {
            int count = 0;
            string sql = string.Format("UPDATE [dbo].[{0}] SET [VoltageLevel] = {1},[PreVoltage] = {2},[CurVoltage] ={3} WHERE [ID]='{1}'", CV_TableName, vm_vp.VoltageLevel, vm_vp.PreVoltage, vm_vp.CurVoltage);
            count = m_VoltageDAL.UpdateVoltageParaInfo(sql);
            return count > 0 ? true : false;
        }

        /// <summary>
        /// 获得数据列表
        /// </summary>
        /// <param name="strWhere">限制条件</param>
        /// <returns>DataSet</returns>
        public int DeleteVoltageParamById(int ID)
        {
            string sql = string.Format("delete  from  [{0}] where ID='{1}'", CV_TableName, ID);
            return m_VoltageDAL.DeleteVoltageParaId(sql);
        }
    }
}
