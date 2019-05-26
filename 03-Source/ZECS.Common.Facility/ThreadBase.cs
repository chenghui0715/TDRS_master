using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ZECS.Common.Facility
{
    public class ThreadRuntime
    {
        public static Dictionary<String, ThreadBase> ThreadNameToThreads = new Dictionary<string, ThreadBase>();
        public static bool ThreadStarted(ThreadBase thd)
        {
            if (thd == null)
                return false;
            if (String.IsNullOrEmpty(thd.ThreadName))
            {
                return false;
            }
            ThreadNameToThreads[thd.ThreadName] = thd;
            return true;
        }
        public static bool ThreadStoppped(ThreadBase thd)
        {
            if (thd == null)
                return false;
            try
            {
                ThreadNameToThreads.Remove(thd.ThreadName);
            }
            catch(Exception e)
            {
            }
            return true;
        }
    }
    public abstract class ThreadBase
    {
        public delegate void ExceptionOccured(Exception ex);

        /// <summary>
        /// 当线程的工作函数中有未处理的异常抛出时，触发此事件。
        /// </summary>
        public event ExceptionOccured OnExceptionOccured;

        protected int m_heartBeat = 0;
        protected bool m_bRun = false;
        protected bool m_bPause = false;
        private Thread m_thread;
        protected int m_nSleep = 100;
        protected AutoResetEvent m_WaitResetEvent = new AutoResetEvent(false);

        public virtual bool Start(int nSleep)
        {
            m_nSleep = nSleep;
            if (!m_bRun)
            {
                m_bRun = true;
                m_thread = new Thread(this.WorkThread);
                m_thread.IsBackground = true;
                m_thread.Name = m_ThreadName;
                m_thread.Priority = m_Priority;
                m_thread.Start();
                ThreadRuntime.ThreadStarted(this);
            }
            return m_bRun;
        }
        public virtual void Stop()
        {
            if (!m_bRun)
                return;
            m_bRun = false;
            if (m_thread != null)
            {
                m_thread.Abort();
                m_thread.Join();
            }
            m_thread = null;
            ThreadRuntime.ThreadStoppped(this);
        }
        public virtual void Abort()
        {
            if (!m_bRun)
                return;
            m_bRun = false;
            if (m_thread != null)
                m_thread.Abort();
            m_thread = null;
        }
        /// <summary>
        /// 暂停线程执行
        /// </summary>
        /// <returns></returns>
        public virtual bool Pause()
        {
            m_bPause = true;
            return true;
        }

        /// <summary>
        /// 恢复线程执行
        /// </summary>
        /// <returns></returns>
        public virtual bool Resume()
        {
            m_bPause = false;
            return true;
        }

        public int GetHeartBeat()
        {
            return m_heartBeat;
        }

        public Int32 HeartBeat
        {
            get { return m_heartBeat; }
            set { m_heartBeat = value; }
        }

        public AutoResetEvent WaitResetEvent
        {
            get {return m_WaitResetEvent; }
        }

        protected ThreadPriority m_Priority = ThreadPriority.Normal;
        public ThreadPriority Priority
        {
            get { return m_Priority; }
            set
            {
                m_Priority = value;
                if (m_thread != null)
                    m_thread.Priority = m_Priority;
            }
        }

        private string m_ThreadName;
        public String ThreadName
        {
            get 
            {
                return m_ThreadName;
            }
            set 
            {
                m_ThreadName = value;
                if (m_thread != null)
                    m_thread.Name = m_ThreadName;
            }
        }

        public int ThreadID
        {
            get
            {
                if (m_thread != null)
                    return m_thread.ManagedThreadId;
                return -1;
            }
        }

        protected virtual void WorkThread()
        {
            while (m_bRun)
            {
                try
                {
                    if (!m_bPause)
                    {
                        WorkFunc();
                    }
                }
                catch (ThreadAbortException e)
                {
                    //正在终止线程，暂不需处理
                }
                catch (Exception ex)
                {
                    m_heartBeat = 0;
                    if (OnExceptionOccured != null)
                    {
                        OnExceptionOccured(ex);
                    }
                }
                //Thread.Sleep(m_nSleep);
                m_WaitResetEvent.WaitOne(m_nSleep);
                m_heartBeat++;
            }
        }

        public virtual void WorkFunc() { }

        protected void RaiseExceptionOccuredEvent(Exception ex)
        {
            if (OnExceptionOccured != null)
            {
                OnExceptionOccured(ex);
            }
        }
    }
}
