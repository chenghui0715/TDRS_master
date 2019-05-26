using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using YH.ICMS.Common;

namespace YH.ICMS.DAL
{
    public class VoltageDAL
    {
        string TdrsDb = ConfigurationManager.AppSettings["DemoKey"];
        #region -------相机------
        /// <summary>
        /// 获取电压参数
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataSet GetVoltageParaInfo(string sql)
        {
            return SQLDBHelper.Query(sql);
        }



        // <summary>
        /// 根据ID删除电压参数
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int DeleteVoltageParaId(string sql)
        {
            return SQLDBHelper.ExecuteSql(sql);
        }

        /// <summary>
        /// 插入电压参数
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int InsertCameraInfo(string sql)
        {
            return SQLDBHelper.ExecuteSql(sql);
        }
        /// <summary>
        /// 更新电压参数
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int UpdateVoltageParaInfo(string sql)
        {
            return SQLDBHelper.ExecuteSql(sql);
        }

        #endregion
    }
}
