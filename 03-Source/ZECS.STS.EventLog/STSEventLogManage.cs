using System;
using System.Threading;
using YH.ICMS.Common;
using YH.ICMS.Common.Enumeration;
using YH.ICMS.Entity;

namespace ZECS.STS.EventLog
{
    /// <summary>
    /// 总的管理类，提供对外接口，串联各个类。
    /// </summary>
    public class STSEventLogManage : SingletonBase<STSEventLogManage>
    {
        ThreadSaveSTSEvent m_ThreadSaveSTSEvent;
        
        /// <summary>
        /// 开启TDRSEvent管理模块
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            String strMsg = "";
            m_ThreadSaveSTSEvent = new ThreadSaveSTSEvent();
            m_ThreadSaveSTSEvent.ThreadName = "Thread_EventLog";
            if (!m_ThreadSaveSTSEvent.Start(2))
            {
                strMsg = String.Format("[0] Start EventLog failed.");
                LogHelper.WriteErrorLog(strMsg);
                m_ThreadSaveSTSEvent = null;
                return false;
            }

            VM_TDRSInfo stsEventRecord = new VM_TDRSInfo();
            stsEventRecord.HeadingDirection =Direction.入库方向;
            stsEventRecord.InputDiInfo =null;
            stsEventRecord.CREATED = DateTime.Now;
            Instance.AddEventLog(stsEventRecord);
            strMsg = "STSEventLog Modle started! ";
            LogHelper.WriteInfoLog(strMsg);
            return true;
        }

        /// <summary>
        /// 停止STSEvent管理模块
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {
            VM_TDRSInfo stsEventRecord = new VM_TDRSInfo();
            stsEventRecord.HeadingDirection = Direction.入库方向 ;
            stsEventRecord.PortInfo = null;
            stsEventRecord.CREATED = DateTime.Now;
            Instance.AddEventLog(stsEventRecord);
            String strMsg = "Stop STSEventLog modle.";
            LogHelper.WriteInfoLog(strMsg);
            Thread.Sleep(2);
            if (m_ThreadSaveSTSEvent != null)
                m_ThreadSaveSTSEvent.Stop();
           
            return true;
        }

        public bool AddEventLog(VM_TDRSInfo stsEventRecord)
        {
            if (stsEventRecord == null)
                return false;
            //队列的特点就是先进先出
            //入队  将对象添加到 System.Collections.Generic.Queue<T> 的结尾处。
            return m_ThreadSaveSTSEvent.EventLogCreated(stsEventRecord);
        }

    }
}
