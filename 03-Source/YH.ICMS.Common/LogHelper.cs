using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YH.ICMS.Common
{
    public class LogHelper
    {
        
        /// <summary>
        /// 写Txt日志 到当前程序根目录
        /// </summary>
        /// <param name="strLog"></param>
        public static void WriteInfoLog(string strLog)
        {
            string sFilePath = Environment.CurrentDirectory + "\\" + DateTime.Now.ToString("yyyyMM");
            string sFileName = "CheckSpecialStrsLog" + DateTime.Now.ToString("dd") + ".log";
            sFileName = sFilePath + "\\" + sFileName; //文件的绝对路径
            if (!Directory.Exists(sFilePath))//验证路径是否存在
            {
                Directory.CreateDirectory(sFilePath);
                //不存在则创建
            }
            FileStream fs;
            StreamWriter sw;
            if (File.Exists(sFileName))
            //验证文件是否存在，有则追加，无则创建
            {
                fs = new FileStream(sFileName, FileMode.Append, FileAccess.Write);
            }
            else
            {
                fs = new FileStream(sFileName, FileMode.Create, FileAccess.Write);
            }
            sw = new StreamWriter(fs);
            string str = string.Format("LOG_Info TDRS 【时间】 {0}：  【信息】： {1} ", DateTime.Now.ToString("yyyy-MM-dd  HH:mm:ss fff"), strLog);
            //OutPutMessage(str);
            sw.WriteLine(str);
            sw.Close();
            fs.Close();
        }

        public static void WriteErrorLog(string strLog)
        {
            string sFilePath = Environment.CurrentDirectory + "\\" + DateTime.Now.ToString("yyyyMM");
            string sFileName = "CheckSpecialStrsLog" + DateTime.Now.ToString("dd") + ".log";
            sFileName = sFilePath + "\\" + sFileName; //文件的绝对路径
            if (!Directory.Exists(sFilePath))//验证路径是否存在
            {
                Directory.CreateDirectory(sFilePath);
                //不存在则创建
            }
            FileStream fs;
            StreamWriter sw;
            if (File.Exists(sFileName))
            //验证文件是否存在，有则追加，无则创建
            {
                fs = new FileStream(sFileName, FileMode.Append, FileAccess.Write);
            }
            else
            {
                fs = new FileStream(sFileName, FileMode.Create, FileAccess.Write);
            }
            sw = new StreamWriter(fs);
            string str = string.Format("LOG_Error TDRS 【时间】 {0}：  【错误】： {1} ", DateTime.Now.ToString("yyyy-MM-dd  HH:mm:ss fff"), strLog);
            //OutPutMessage(str);
            sw.WriteLine(str);
            sw.Close();
            fs.Close();
        }


        public static void WriteWarningLog(string strLog)
        {
            string sFilePath = Environment.CurrentDirectory + "\\" + DateTime.Now.ToString("yyyyMM");
            string sFileName = "CheckSpecialStrsLog" + DateTime.Now.ToString("dd") + ".log";
            sFileName = sFilePath + "\\" + sFileName; //文件的绝对路径
            if (!Directory.Exists(sFilePath))//验证路径是否存在
            {
                Directory.CreateDirectory(sFilePath);
                //不存在则创建
            }
            FileStream fs;
            StreamWriter sw;
            if (File.Exists(sFileName))
            //验证文件是否存在，有则追加，无则创建
            {
                fs = new FileStream(sFileName, FileMode.Append, FileAccess.Write);
            }
            else
            {
                fs = new FileStream(sFileName, FileMode.Create, FileAccess.Write);
            }
            sw = new StreamWriter(fs);
            string str = string.Format("LOG_Warning TDRS 【时间】 {0}：  【警告】： {1} ", DateTime.Now.ToString("yyyy-MM-dd  HH:mm:ss fff"), strLog);
            //OutPutMessage(str);
            sw.WriteLine(str);
            sw.Close();
            fs.Close();
        }

        
    }
}
