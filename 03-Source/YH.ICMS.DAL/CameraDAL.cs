using System.Configuration;
using System.Data;
using YH.ICMS.Common;

namespace YH.ICMS.DAL
{
    public class CameraDAL
    {
        string TdrsDb = ConfigurationManager.AppSettings["DemoKey"];
        #region -------相机------
        /// <summary>
        /// 获取相机参数
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataSet GetCameraInfo(string sql)
        {
            return SQLDBHelper.Query( sql);
        }

        

        // <summary>
        /// 根据ID删除相机参数
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int DeleteCameraById(string sql)
        {
            return SQLDBHelper.ExecuteSql(sql);
        }

        /// <summary>
        /// 插入相机参数
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int  InsertCameraInfo(string sql)
        {
            return SQLDBHelper.ExecuteSql(sql);
        }
        /// <summary>
        /// 更新相机参数
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int UpdateCameraInfo(string sql)
        {
            return SQLDBHelper.ExecuteSql(sql);
        }

        #endregion

        #region-------站点------
        /// <summary>
        /// 插入站点参数
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int InsertStationInfo(string sql)
        {
            return SQLDBHelper.ExecuteSql(sql);
        }

        /// <summary>
        /// 获取站点数据
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int GetStationInfo(string sql)
        {
            DataSet ds = new DataSet();
            ds = SQLDBHelper.Query(sql);
            if (ds == null || ds.Tables[0].Rows.Count == 0|| ds.Tables[0].Rows==null)
                return 0;
            return 1;
        }


        public DataSet GetDataSetStationInfo(string sql)
        {
            return SQLDBHelper.Query(sql);
        }
        public int  DeleteStationInfo(string sql)
        {
            return SQLDBHelper.ExecuteSql(sql);
        }

        
        #endregion

    }
}
