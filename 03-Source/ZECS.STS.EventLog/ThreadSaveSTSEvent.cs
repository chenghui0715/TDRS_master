using System;
using System.Collections.Generic;
using System.IO;
using YH.ICMS.Common;
using YH.ICMS.Entity;
//using ZECS.STS.DBDataAccess.EventLog;
//using ZECS.STS.DataModle;

namespace ZECS.STS.EventLog
{
    /// <summary>
    /// 将STS Event记录到数据库的线程。
    /// </summary>
    public class ThreadSaveSTSEvent: ThreadBase
    {

        public ThreadSaveSTSEvent(): base()
        {
            ThreadName = "ThreadSaveSTSEvent";
        }
       // EventLogAccessor m_EventLogAccessor = new EventLogAccessor();
        Queue<VM_TDRSInfo> LstEventLogQueue=new Queue<VM_TDRSInfo>();
        private VM_TDRSInfo m_stsEventRecord = new VM_TDRSInfo();
        public List<VM_TDRSInfo> m_LstTDRSInfo { get; set; }
        public VM_TDRSInfo tdrs = new VM_TDRSInfo();

        /// <summary>
        /// 记录事件线程
        /// </summary>
        public override void WorkFunc()
        {   
            SaveSTSEventLogToDb();
        }

        /// <summary>
        /// 保存STSEventLog至数据库
        /// </summary>
        /// <returns>保存成功Yes,保存失败No</returns>
        private bool SaveSTSEventLogToDb()
        {
            if (LstEventLogQueue.Count <= 0)
                return false;
            List<VM_TDRSInfo> lstTempUpdated = new List<VM_TDRSInfo>();
            lock (LstEventLogQueue)
            {
                lstTempUpdated.AddRange(LstEventLogQueue);
                LstEventLogQueue.Clear();
            }
            foreach (VM_TDRSInfo stsEventRecord in lstTempUpdated)
            {
                //StatusInfo(stsEventRecord);
                //////////////if (stsEventRecord.InputDiInfo != "00001111")
                //////////////{
                //////////////    LogHelper.WriteInfoLog(stsEventRecord.InputDiInfo);
                //////////////}
                
                
               // m_EventLogAccessor.TrolleyTaskCreated(stsEventRecord);
            }
            return true;
        }




        /// <summary>
        /// 添加事件记录至队列
        /// </summary>
        /// <param name="stsEventRecord">记录事件内容</param>
        /// <returns>创建成功为YES，创建失败为NO</returns>
        public bool EventLogCreated(VM_TDRSInfo stsEventRecord)
        {
            if (stsEventRecord == null)
                return false;
            if (m_stsEventRecord == null)
                m_stsEventRecord = new VM_TDRSInfo();
            if (stsEventRecord.InputDiInfo == m_stsEventRecord.InputDiInfo)
                return false;
            
            m_stsEventRecord = stsEventRecord.DeepCopy();

            lock (LstEventLogQueue)
            {
                LstEventLogQueue.Enqueue(stsEventRecord);
            }
            return true;
        }

        
    }
}
