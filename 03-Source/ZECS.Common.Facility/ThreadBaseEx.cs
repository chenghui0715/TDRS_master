using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 
namespace ZECS.Common.Facility
{
    /// <summary>
    /// 使用线程池的工作线程
    /// </summary>
    public abstract class ThreadBaseEx : ThreadBase
    {
        //private WorkTask task = null;

        //public virtual bool Start(int nSleep)
        //{
        //    m_nSleep = nSleep;
        //    if (!m_bRun)
        //    {
        //        m_bRun = true;
        //        task = WorkTask.CreateTask(this.WorkThread);
        //        if (!string.IsNullOrEmpty(this.ThreadName))
        //            task.Name = this.ThreadName;
        //        task.ExecuteOnlyOnce = false;
        //        task.MillisecondsTimeOutInterval = nSleep;
        //        task.Start();
        //    }
        //    return m_bRun;
        //}

        //public delegate void ExceptionOccured(Exception ex);

        ///// <summary>
        ///// 当线程的工作函数中有未处理的异常抛出时，触发此事件。
        ///// </summary>
        //public event ExceptionOccured OnExceptionOccured;

        //protected int m_heartBeat = 0;
        //protected bool m_bRun = false;
        //protected bool m_bPause = false;
        //protected int m_nSleep = 100;

        //public virtual void Stop()
        //{
        //    if (!m_bRun)
        //        return;
        //    m_bRun = false;
        //}

        //public virtual void Abort()
        //{
        //    if (!m_bRun)
        //        return;
        //    m_bRun = false;
        //}
        ///// <summary>
        ///// 暂停线程执行
        ///// </summary>
        ///// <returns></returns>
        //public virtual bool Pause()
        //{
        //    m_bPause = true;
        //    return true;
        //}

        ///// <summary>
        ///// 恢复线程执行
        ///// </summary>
        ///// <returns></returns>
        //public virtual bool Resume()
        //{
        //    m_bPause = false;
        //    return true;
        //}

        //public int GetHeartBeat()
        //{
        //    return m_heartBeat;
        //}

        //public Int32 HeartBeat
        //{
        //    get { return m_heartBeat; }
        //    set { m_heartBeat = value; }
        //}

        //private string m_ThreadName;
        //public String ThreadName
        //{
        //    get
        //    {
        //        return m_ThreadName;
        //    }
        //    set
        //    {
        //        m_ThreadName = value;
        //    }
        //}

        //public int ThreadID
        //{
        //    get
        //    {
        //        return -1;
        //    }
        //}

        //protected virtual void WorkThread()
        //{
        //    //while (m_bRun)
        //    {
        //        try
        //        {
        //            if (!m_bPause)
        //            {
        //                WorkFunc();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            m_heartBeat = 0;
        //            RaiseExceptionOccuredEvent(ex);
        //        }
        //        //Thread.Sleep(m_nSleep);
        //        //m_WaitResetEvent.WaitOne(m_nSleep);
        //        m_heartBeat++;
        //    }
        //}

        //public virtual void WorkFunc() { }

        //protected void RaiseExceptionOccuredEvent(Exception ex)
        //{
        //    if (OnExceptionOccured != null)
        //    {
        //        OnExceptionOccured(ex);
        //    }
        //}
    }
}
