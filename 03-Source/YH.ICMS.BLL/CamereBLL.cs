using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using YH.ICMS.DAL;
using YH.ICMS.Entity;

namespace YH.ICMS.BLL
{
    
    public class CamereBLL
    {
        private string CP_TableName = "C_CameraParameter_T";
        private string PS_TableName = "C_ParametersSetting_T";
        CameraDAL m_CameraDAL = new CameraDAL();
        /// <summary>
        /// 获得数据列表
        /// </summary>
        /// <param name="strWhere">限制条件</param>
        /// <returns>DataSet</returns>
        public DataSet GetCameraList(string strWhere)
        {
            string sql = string.Format("SELECT [ID],[camera],[exposureMode],[exposureMax],[exposureMin],[exposureValue]FROM[dbo].[{0}]", CP_TableName);
            return m_CameraDAL.GetCameraInfo(sql);
        }

        public bool InsertCameraInfo(VM_CameraParameters vm_cp)
        {
            
            int count = 0;
            string sql = string.Format("INSERT INTO[dbo].[{0}]([camera],[exposureMode],[exposureMax],[exposureMin],[exposureValue]) VALUES('{1}','{2}','{3}','{4}','{5}'); ", CP_TableName, vm_cp.camera, vm_cp.exposureMode, vm_cp.exposureMax, vm_cp.exposureMin, vm_cp.exposureValue);
            count= m_CameraDAL.InsertCameraInfo(sql);
            return count>0? true : false;
        }

        public bool UpdateCameraInfo(VM_CameraParameters vm_cp)
        {
            int count = 0;
            string sql = string.Format("UPDATE [dbo].[{0}] SET [camera]={1} ,[exposureMode] = {2},[exposureMax] = {3},[exposureMin] ={4},[exposureValue] ={5} WHERE [ID]='{6}'", CP_TableName,  vm_cp.camera, vm_cp.exposureMode, vm_cp.exposureMax, vm_cp.exposureMin, vm_cp.exposureValue, vm_cp.ID);
            count = m_CameraDAL.UpdateCameraInfo(sql);
            return count > 0 ? true : false;
        }

        /// <summary>
        /// 获得数据列表
        /// </summary>
        /// <param name="strWhere">限制条件</param>
        /// <returns>DataSet</returns>
        public int DeleteCameraById(int  ID)
        {
            string sql = string.Format("delete  from  [{0}] where ID='{1}'", CP_TableName, ID);
            return m_CameraDAL.DeleteCameraById(sql);
        }


        #region-------------------------
        public bool InsertstationInfo(VM_Content vm_content)
        {
            int count = 0;
            string sql = string.Format("INSERT INTO[dbo].[{0}]([StartTag],[IsMulti],[Illumination],[Station]) VALUES({1},{2},{3},'{4}'); ", PS_TableName, vm_content.startTag, vm_content.isMulti, vm_content.illumination, vm_content.station);
            count = m_CameraDAL.InsertStationInfo(sql);
            return count > 0 ? true : false;
        }

        public bool GetStationInfo()
        {
            int count = 0;
            string sql = string.Format("select * from {0}; ", PS_TableName);
            count = m_CameraDAL.GetStationInfo(sql);
            return count > 0 ? true : false;
        }

        public DataSet GetStationInfoByDataSet()
        {
            string sql = string.Format("select * from {0}; ", PS_TableName);
            return m_CameraDAL.GetDataSetStationInfo(sql);
        }

        public int  DeleteStationInfo()
        {
            string sql = string.Format("delete  from {0}; ", PS_TableName);
            return m_CameraDAL.DeleteStationInfo(sql);
        }
        #endregion

    }
}
