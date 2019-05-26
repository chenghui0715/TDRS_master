using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Data;
using ICMS.Commons;
using ICMS.Modules.BaseComponents;

namespace ICMS.Modules.BaseComponents.Commons
{
    public class SqlServerHelper
	{
        public string conn  = ConfigurationHelper.GetLocalConfigValue("Global.DB");
		/// <summary>
		/// 一条增加修改删除 </summary>
		/// <param name="cmd"></param>
		/// <returns></returns>
		public  bool ExecCmd(string cmd)
		{
            SqlConnection con = new SqlConnection(conn);
			try
			{
				con.Open();
			}
			catch
			{
				return false;
			}
			SqlCommand com = null;
			try
			{
				com = con.CreateCommand();
				com.CommandText = cmd;
				int resultid=com.ExecuteNonQuery();
                if (resultid > 0)
                {                    
                    return true;  
                }
                else
                {
                    return false;
                }
				
			}
			catch (Exception e)
			{
				return false;
			}
			finally
			{
				con.Close();
			}
		}

		/// <summary>
		/// 查询返回DataSet
		/// </summary>
		/// <param name="cmd"></param>
		/// <returns></returns>
		public  DataSet GetResult(string cmd)
		{
            SqlConnection con = new SqlConnection(conn);
			DataSet ds = new DataSet();
			try
			{
				con.Open();
                SqlDataAdapter ada = new SqlDataAdapter(cmd, con);
				ada.Fill(ds);
			}
			catch (Exception e)
			{
				return null;
			}
			finally
			{
				con.Close();
			}
			return ds;
		}


		/// <summary>
		/// 事务处理方法
		/// </summary>
		/// <param name="cmd"></param>
		/// <returns></returns>
		public  ExecutionResult ExecuteCmd(string cmd,SqlConnection con,SqlTransaction sqlTransaction)
		{
			ExecutionResult exeResult = new ExecutionResult();
	
			SqlTransaction st = null;
			SqlCommand com;
			try
			{
				//st = con.BeginTransaction();//启用事务实现
				com = con.CreateCommand();
                com.Transaction = sqlTransaction;
				com.CommandText = cmd;
				com.ExecuteNonQuery();
				exeResult.Status = true;
				exeResult.Message = "OK";
			}
			catch (Exception e)
			{
				st.Rollback();
				exeResult.Message = e.Message;
				exeResult.Status = false;
			}

			return exeResult;
		}

		/// <summary>
		/// 增加修改删除返回自增的id
		/// </summary>
		/// <param name="cmd"></param>
		/// <returns></returns>
		public  ExecutionResult ExecuteCmd(string cmd)
		{
			ExecutionResult exeResult = new ExecutionResult();
			SqlConnection con = new SqlConnection(conn);
			try
			{
				con.Open();
			}
			catch (Exception e)
			{
				exeResult.Status = false;
				exeResult.Message = e.Message;
				return exeResult;
			}
			SqlTransaction st = null;
			SqlCommand com;
			try
			{
				st = con.BeginTransaction();//启用事务实现
				com = con.CreateCommand();
				com.Transaction = st;
				com.CommandText = cmd;
				com.ExecuteNonQuery();
				st.Commit();
				exeResult.Status = true;
				exeResult.Message = "OK";
			}
			catch (Exception e)
			{
				if (st != null)
					st.Rollback();
				exeResult.Status = false;
				exeResult.Message = e.Message;
			}
			finally
			{
				con.Close();
			}
			return exeResult;
		}

        /// <summary>
        /// 增加修改删除返回自增的id
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public ExecutionResult ExecuteCmdReturnId(string cmd)
        {
            ExecutionResult exeResult = new ExecutionResult();
            SqlConnection con = new SqlConnection(conn);
            try
            {
                con.Open();
            }
            catch (Exception e)
            {
                exeResult.Status = false;
                exeResult.Message = e.Message;
                return exeResult;
            }
            SqlTransaction st = null;
            SqlCommand com;
            try
            {
                st = con.BeginTransaction();//启用事务实现
                com = con.CreateCommand();
                com.Transaction = st;
                com.CommandText = cmd;
                com.ExecuteNonQuery();
                st.Commit();
                exeResult.Status = true;
                exeResult.Message = "OK";
            }
            catch (Exception e)
            {
                if (st != null)
                    st.Rollback();
                exeResult.Status = false;
                exeResult.Message = e.Message;
            }
            finally
            {
                con.Close();
            }
            return exeResult;
        }

		/// <summary>
		/// 查询返回DataSet
		/// </summary>
		/// <param name="cmd"></param>
		/// <returns></returns>
		public  ExecutionResult GetDataSet(string cmd)
		{
			ExecutionResult exeResult = new ExecutionResult();
			SqlConnection con = new SqlConnection(conn);
			DataSet ds = new DataSet();
			try
			{
				con.Open();
				SqlDataAdapter ada = new SqlDataAdapter(cmd, con);
				ada.Fill(ds);
				exeResult.Anything = ds;
				exeResult.Message = "OK";
				exeResult.Status = true;
			}
			catch (Exception e)
			{
				exeResult.Status = false;
				exeResult.Message = e.Message;
			}
			finally
			{
				con.Close();
			}
			return exeResult;
		}
	}

}
