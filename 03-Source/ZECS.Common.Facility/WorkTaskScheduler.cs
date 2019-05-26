using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using System.Security;
using System.Collections.Concurrent;

namespace ZECS.Common.Facility
{
    /// <summary>
    /// 线程池任务调度 
    /// </summary>
    public class WorkTaskScheduler : IDisposable
    {
        public static WorkTaskScheduler Current { get; set; }

        /// <summary>
        /// 当出错的Task未观察到的异常将触发该事件
        /// /// </summary>
        public event EventHandler<UnobservedTaskExceptionEventArgs> UnobservedTaskException;

        private List<Exception> m_lstEx = new List<Exception>();

        public static WorkTaskScheduler Default
        {
            get { return m_Default; }
        }

        private static WorkTaskScheduler m_Default = new WorkTaskScheduler(50);

        public int Id { get; private set; }
        private int m_MaximumConcurrencyLevel = 0;
        /// <summary>
        /// 表示最大并发级别的一个整数
        /// </summary>
        public virtual int MaximumConcurrencyLevel
        {
            get { return m_MaximumConcurrencyLevel; }
            set
            {
                if (value > 0 && m_MaximumConcurrencyLevel != value)
                {
                    m_MaximumConcurrencyLevel = value;
                    if (m_lstThread.Count < m_MaximumConcurrencyLevel)
                    {
                        do
                        {
                            var thread = new WorkThread(this);
                            thread.ThreadException += (o, e) =>
                            {
                                lock (m_lstException)
                                {
                                    m_lstException.Add(e.Exception);
                                }
                            };
                            m_lstThread.Add(thread);
                        } while (m_lstThread.Count < this.MaximumConcurrencyLevel);
                    }
                }
            }
        }

        protected Thread CurrentThread { get; set; }
        protected bool IsRunning { get; set; }
        private object lockObj = new object();

        private List<WorkThread> m_lstThread = new List<WorkThread>();
        /// <summary>
        /// 执行的任务
        /// </summary>
        private List<WorkTask> m_lstTasks = new List<WorkTask>();
        /// <summary>
        /// 全局待执行的任务
        /// </summary>
        private ConcurrentQueue<WorkTask> g_QueueTasks = new ConcurrentQueue<WorkTask>();

        private List<Exception> m_lstException = new List<Exception>();

        public WorkTaskScheduler(int maximumConcurrencyLevel)
        {
            this.MaximumConcurrencyLevel = maximumConcurrencyLevel;
            this.IsRunning = true;
            this.CurrentThread = new Thread(new ThreadStart(OnDispatchingTask));
            this.CurrentThread.IsBackground = true;
            this.CurrentThread.Start();
        }

        protected virtual void OnDispatchingTask()
        {
            int iCount = 0;
            while (this.IsRunning)
            {
                try
                {
                    WorkTask task = null;
                    for (int i = 0; i < m_lstTasks.Count; i++)
                    {
                        task = m_lstTasks[i];
                        if (task == null)
                        {
                            lock (m_lstTasks)
                                m_lstTasks.RemoveAt(i);
                            continue;
                        }
                        if (task.ExecuteOnlyOnce || task.PlanStartTime.Ticks <= DateTime.Now.Ticks)
                        {
                            g_QueueTasks.Enqueue(task);
                            lock (m_lstTasks)
                                m_lstTasks.RemoveAt(i);
                        }
                    }

                    TryExecuteTask();
                }
                catch (Exception ex)
                {
                    m_lstException.Add(ex);
                }
                iCount++;
                if (iCount % 100 == 0 && m_lstException.Count > 0)
                {
                    AggregateException lst = null;
                    lock (m_lstException)
                    {
                        lst = new AggregateException(m_lstException);
                        m_lstException.Clear();
                    }
                    WorkTask task = WorkTask.CreateTask<AggregateException>(OnDispatchException, lst);
                    task.Start(this);
                }
                Thread.Sleep(1);
            }
        }

        protected internal bool TryDispatchWorkTask(out WorkTask task)
        {
            return g_QueueTasks.TryDequeue(out task);
        }

        protected internal WorkThread OnDispatchWorkThread()
        {
            WorkThread thread = null;
            for (int i = 0; i < m_lstThread.Count; i++)
            {
                if (m_lstThread[i].IsIdle)
                    return m_lstThread[i];
            }
            return thread;
        }

        private void OnDispatchException(AggregateException lst)
        {
            if (UnobservedTaskException != null)
                UnobservedTaskException(this, new UnobservedTaskExceptionEventArgs(lst));
        }

        public WorkThread[] GetScheduledThreads()
        {
            return m_lstThread.ToArray();
        }

        public IEnumerable<WorkTask> GetScheduledTasks()
        {
            return m_lstTasks;
        }

        public long TotalTaskCount()
        {
            long iCount = 0;
            iCount += m_lstTasks.Count;
            iCount += g_QueueTasks.Count;
            return iCount;
        }

        public bool Exists(string taskName)
        {
            if (string.IsNullOrEmpty(taskName))
                return false;
            return m_lstTasks.Exists(obj => obj.Name == taskName);
        }

        public WorkTask TryGetTask(string taskName)
        {
            if (string.IsNullOrEmpty(taskName))
                return null;
            return m_lstTasks.Find(obj => obj.Name == taskName);
        }

        /// <summary>
        /// 将 WorkTask 排队到计划程序中
        /// </summary>
        /// <param name="task"></param>
        protected internal virtual void QueueTask(WorkTask task)
        {
            if (task.ExecuteOnlyOnce || task.PlanStartTime.Ticks <= DateTime.Now.Ticks)
            {
                if (!TryExecuteTask(task))
                    g_QueueTasks.Enqueue(task);
            }
            else
            {
                lock (m_lstTasks)
                    m_lstTasks.Add(task);
            }
        }

        /// <summary>
        /// 尝试将以前排队到此计划程序中的WorkTask取消排队
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        protected internal virtual bool TryDequeue(WorkTask task)
        {
            if (task.IsAlive)
            {
                task.Status = TaskStatus.Canceled;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 尝试在此计划程序上执行提供的WorkTask
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        protected bool TryExecuteTask(WorkTask task)
        {
            lock (lockObj)
            {
                var thread = OnDispatchWorkThread();
                if (thread != null && thread.Start(task))
                {
                    return true;
                }
                return false;
            }
        }

        protected void TryExecuteTask()
        {
            if (g_QueueTasks.Count == 0)
                return;
            lock (lockObj)
            {
                for (int i = 0; i < m_lstThread.Count; i++)
                {
                    if (!m_lstThread[i].IsIdle)
                        continue;
                    WorkTask task = null;
                    if (!TryDispatchWorkTask(out task))
                        break;
                    if (!m_lstThread[i].Start(task))
                    {
                        g_QueueTasks.Enqueue(task);
                    }
                }
            }
        }

        public void Dispose()
        {
            try
            {
                if (this.CurrentThread != null && this.CurrentThread.IsAlive)
                    this.CurrentThread.Abort();
                this.CurrentThread = null;
            }
            catch { }
        }
    }

    public class WorkTask : IAsyncResult
    {
        protected Action DynamicInvoke;

        public object[] MethodArgs { get; protected set; }
        public object ReturnValue { get; protected set; }

        public string Name { get; set; }

        public int MillisecondsTimeOutInterval { get; set; }

        public bool ExecuteOnlyOnce { get; set; }
        /// <summary>
        /// 获取此实例的唯一 ID
        /// </summary>
        public long Id { get; private set; }
        /// <summary>
        /// 如果任务由于被取消而完成，则为 true；否则为 false
        /// </summary>
        public bool IsCanceled { get { return this.Status == TaskStatus.Canceled; } }
        public bool IsRunning { get { return this.Status == TaskStatus.Running; } }
        /// <summary>
        /// 如果任务已完成，则为 true；否则为 false
        /// </summary>
        public bool IsCompleted { get { return this.Status == TaskStatus.RanToCompletion; } }
        /// <summary>
        /// 如果任务引发了未经处理的异常，则为 true；否则为 false
        /// </summary>
        public bool IsFaulted { get { return this.Status == TaskStatus.Faulted; } }

        public bool IsAlive
        {
            get
            {
                if (this.IsCompleted || this.IsFaulted || this.IsCanceled)
                    return false;
                return true;
            }
        }

        private TaskStatus m_TaskStatus = TaskStatus.Created;
        public TaskStatus Status
        {
            get { return m_TaskStatus; }
            protected internal set
            {
                if (m_TaskStatus != value)
                {
                    m_TaskStatus = value;
                    switch (m_TaskStatus)
                    {
                        case TaskStatus.WaitingForActivation:
                            this.CreateTime = DateTime.Now;
                            break;
                        case TaskStatus.WaitingToRun:
                            this.ActivationTime = DateTime.Now;
                            break;
                        case TaskStatus.Running:
                            this.StartTime = DateTime.Now;
                            //this.WaitTime = (this.StartTime - this.CreateTime).TotalMilliseconds;
                            break;
                        case TaskStatus.RanToCompletion:
                        case TaskStatus.Faulted:
                        case TaskStatus.Canceled:
                            this.EndTime = DateTime.Now;
                            //this.RunTime = (this.EndTime - this.StartTime).TotalMilliseconds;
                            break;
                    }
                    if (m_Callback != null
                        && !this.IsAlive)
                    {
                        m_Callback(this);
                    }
                    //else if (CallbackStatic != null && this.IsAlive)
                    //{
                    //    CallbackStatic(this);
                    //}
                }
            }
        }
        /// <summary>
        /// 获取用户定义的对象，它限定或包含关于异步操作的信息
        /// </summary>
        public object AsyncState { get; private set; }
        /// <summary>
        /// 获取用于等待异步操作完成的 System.Threading.WaitHandle
        /// </summary>
        public WaitHandle AsyncWaitHandle { get { return reset; } }
        /// <summary>
        /// 获取一个值，该值指示异步操作是否同步完成
        /// </summary>
        public bool CompletedSynchronously { get; private set; }

        public Exception InnerException { get; private set; }

        private ManualResetEvent reset = new ManualResetEvent(false);
        /// <summary>
        /// 任务创建时间
        /// </summary>
        public DateTime CreateTime { get; private set; }
        /// <summary>
        /// 开始激活的时间（已经分配空闲线程）
        /// </summary>
        public DateTime ActivationTime { get; private set; }
        public DateTime PlanStartTime { get; set; }
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }

        public double RunTime //{ get; private set; }
        {
            get
            {
                if (m_TaskStatus == TaskStatus.Running)
                {
                    return (DateTime.Now - this.StartTime).TotalMilliseconds;
                }
                else if (m_TaskStatus > TaskStatus.Running)
                {
                    return (this.EndTime - this.StartTime).TotalMilliseconds;
                }
                return 0;
            }
        }

        public double WaitTime //{ get; private set; }
        {
            get
            {
                if (m_TaskStatus >= TaskStatus.Running)
                {
                    return (this.StartTime - this.CreateTime).TotalMilliseconds;
                }
                return (DateTime.Now - this.CreateTime).TotalMilliseconds;
            }
        }

        public string Description { get; set; }

        public object Tags { get; set; }

        protected WorkTaskScheduler TaskScheduler { get; set; }

        private static long CurrentTaskID = 1;

        public static AsyncCallback CallbackStatic;

        protected WorkTask(Delegate d, params object[] args)
            : base()
        {
            this.ExecuteOnlyOnce = true;
            this.MillisecondsTimeOutInterval = 1000;
            this.Status = TaskStatus.Created;

            this.Name = string.Format("{0}.{1}", d.Method.DeclaringType.FullName, d.Method.Name);
            this.MethodArgs = args;

            this.Id = CurrentTaskID++;
        }

        /// <summary>
        /// 同步执行
        /// </summary>
        public void RunSynchronously()
        {
            OnDynamicInvoke(true);
        }

        private AsyncCallback m_Callback = null;

        public void Start()
        {
            this.Start(WorkTaskScheduler.Default);
        }

        /// <summary>
        /// 启动任务，并将它安排到当前的计划队列中去 
        /// </summary>
        /// <returns></returns>
        public void Start(AsyncCallback callback)
        {
            this.Start(WorkTaskScheduler.Default, callback);
        }

        public void Start(WorkTaskScheduler scheduler, AsyncCallback callback = null)
        {
            if (this.Status == TaskStatus.Created || !this.IsAlive)
            {
                m_Callback = callback;
                this.Status = TaskStatus.WaitingForActivation;
                scheduler.QueueTask(this);
                this.TaskScheduler = scheduler;
            }
        }

        /// <summary>
        /// 等待完成执行过程
        /// </summary>
        public void Wait()
        {
            this.reset.Reset();
            if (this.Status <= TaskStatus.Running)
            {
                this.reset.WaitOne();
            }
        }

        public bool GetResult(out object result)
        {
            bool ret = false;
            this.reset.Reset();
            if (this.Status <= TaskStatus.Running)
            {
                ret = this.reset.WaitOne(0);
            }
            else
                ret = true;
            result = this.ReturnValue;
            return ret;
        }

        /// <summary>
        /// 等待完成执行过程
        /// </summary>
        /// <param name="millisecondsTimeout">等待的毫秒数，或为 System.Threading.Timeout.Infinite (-1)，表示无限期等待</param>
        /// <returns>如果在分配的时间内完成执行，则为 true；否则为 false</returns>
        public bool Wait(int millisecondsTimeout)
        {
            this.reset.Reset();
            if (this.Status <= TaskStatus.Running)
            {
                return this.reset.WaitOne(millisecondsTimeout);
            }
            return true;
        }

        public bool WaitResult<TResult>(out TResult result)
        {
            return WaitResult<TResult>(Timeout.Infinite, out result);
        }

        public bool WaitResult<TResult>(int millisecondsTimeout, out TResult result)
        {
            bool ret = false;
            this.reset.Reset();
            if (this.Status <= TaskStatus.Running)
            {
                ret = this.reset.WaitOne(millisecondsTimeout);
            }
            else
            {
                ret = true;
            }
            if (ret)
                result = (TResult)this.ReturnValue;
            else
                result = default(TResult);
            return ret;
        }

        public void Cancel()
        {
            if (!this.ExecuteOnlyOnce)
                this.ExecuteOnlyOnce = true;
        }

        public void Abort()
        {
            try
            {
                if (this.Status == TaskStatus.Running)
                {
                    m_CurrentThread.Abort();
                }
                if (this.IsAlive)
                {
                    this.Status = TaskStatus.Canceled;
                }
            }
            catch { this.Status = TaskStatus.Faulted; }
        }

        private System.Threading.Thread m_CurrentThread = null;

        protected internal void OnDynamicInvoke(bool isSynchronously = false)
        {
            try
            {
                if (this.Status != TaskStatus.WaitingToRun)
                {
                    this.InnerException = new Exception("TaskStatus is not WaitingToRun");
                    this.InnerException.Source = this.Name + this.Description;
                    this.Status = TaskStatus.Faulted;
                    return;
                }
                m_CurrentThread = System.Threading.Thread.CurrentThread;
                this.Status = TaskStatus.Running;
                if (DynamicInvoke != null)
                    DynamicInvoke();
                this.CompletedSynchronously = isSynchronously;
                this.Status = TaskStatus.RanToCompletion;
                if (!this.ExecuteOnlyOnce)
                {
                    int iSleep = this.MillisecondsTimeOutInterval - (int)(this.EndTime - this.StartTime).TotalMilliseconds;
                    if (iSleep > 0)
                        this.PlanStartTime = DateTime.Now.AddMilliseconds(iSleep);
                    else
                        this.PlanStartTime = DateTime.MinValue;
                    this.Start(this.TaskScheduler, m_Callback);
                }
            }
            catch (System.Exception ex)
            {
                this.InnerException = ex;
                this.InnerException.Source = this.Name + this.Description;
                this.Status = TaskStatus.Faulted;
            }
            finally
            {
                this.reset.Set();
            }
        }

        public bool Equals(WorkTask task)
        {
            if (task != null && task.Name == this.Name)
            {
                return true;
            }
            return false;
        }

        public static WorkTask CreateTask(Delegate method, params object[] args)
        {
            WorkTask task = new WorkTask(method, args);
            task.DynamicInvoke = (delegate()
            {
                task.ReturnValue = method.DynamicInvoke(task.MethodArgs);
            });
            return task;
        }

        public static WorkTask CreateTask(Action method)
        {
            WorkTask task = new WorkTask(method);
            task.DynamicInvoke = (delegate()
            {
                method();
            });
            return task;
        }

        public static WorkTask CreateTask<T1>(Action<T1> method, params object[] args)
        {
            if (args == null || args.Length != 1)
                throw new ArgumentOutOfRangeException();

            WorkTask task = new WorkTask(method, args);
            task.DynamicInvoke = (delegate()
            {
                method((T1)task.MethodArgs[0]);
            });
            return task;
        }

        public static WorkTask CreateTask<T1, T2>(Action<T1, T2> method, params object[] args)
        {
            if (args == null || args.Length != 2)
                throw new ArgumentOutOfRangeException();

            WorkTask task = new WorkTask(method, args);
            task.DynamicInvoke = (delegate()
            {
                method((T1)task.MethodArgs[0]
                    , (T2)task.MethodArgs[1]);
            });
            return task;
        }

        public static WorkTask CreateTask<TResult>(Func<TResult> method)
        {
            WorkTask task = new WorkTask(method);
            task.DynamicInvoke = (delegate()
            {
                task.ReturnValue = method();
            });
            return task;
        }

        public static WorkTask CreateTask<T1, TResult>(Func<T1, TResult> method, params object[] args)
        {
            if (args == null || args.Length != 1)
                throw new ArgumentOutOfRangeException();

            WorkTask task = new WorkTask(method, args);
            task.DynamicInvoke = (delegate()
            {
                task.ReturnValue = method((T1)task.MethodArgs[0]);
            });
            return task;
        }

        public static WorkTask CreateTask<T1, T2, TResult>(Func<T1, T2, TResult> method, params object[] args)
        {
            if (args == null || args.Length != 2)
                throw new ArgumentOutOfRangeException();

            WorkTask task = new WorkTask(method, args);
            task.DynamicInvoke = (delegate()
            {
                task.ReturnValue = method((T1)task.MethodArgs[0]
                                  , (T2)task.MethodArgs[1]);
            });
            return task;
        }

        public static WorkTask CreateTask<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> method, params object[] args)
        {
            if (args == null || args.Length != 3)
                throw new ArgumentOutOfRangeException();

            WorkTask task = new WorkTask(method, args);
            task.DynamicInvoke = (delegate()
            {
                task.ReturnValue = method((T1)task.MethodArgs[0]
                                  , (T2)task.MethodArgs[1]
                                  , (T3)task.MethodArgs[2]);
            });
            return task;
        }

        public static WorkTask CreateTask<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> method, params object[] args)
        {
            if (args == null || args.Length != 4)
                throw new ArgumentOutOfRangeException();

            WorkTask task = new WorkTask(method, args);
            task.DynamicInvoke = (delegate()
            {
                task.ReturnValue = method((T1)task.MethodArgs[0]
                                  , (T2)task.MethodArgs[1]
                                  , (T3)task.MethodArgs[2]
                                  , (T4)task.MethodArgs[3]);
            });
            return task;
        }

        public static WorkTask CreateTask<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> method, params object[] args)
        {
            if (args == null || args.Length != 5)
                throw new ArgumentOutOfRangeException();

            WorkTask task = new WorkTask(method, args);
            task.DynamicInvoke = (delegate()
            {
                task.ReturnValue = method((T1)task.MethodArgs[0]
                                  , (T2)task.MethodArgs[1]
                                  , (T3)task.MethodArgs[2]
                                  , (T4)task.MethodArgs[3]
                                  , (T5)task.MethodArgs[4]);
            });
            return task;
        }

        public static WorkTask CreateTask<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> method, params object[] args)
        {
            if (args == null || args.Length != 6)
                throw new ArgumentOutOfRangeException();

            WorkTask task = new WorkTask(method, args);
            task.DynamicInvoke = (delegate()
            {
                task.ReturnValue = method((T1)task.MethodArgs[0]
                                  , (T2)task.MethodArgs[1]
                                  , (T3)task.MethodArgs[2]
                                  , (T4)task.MethodArgs[3]
                                  , (T5)task.MethodArgs[4]
                                  , (T6)task.MethodArgs[5]);
            });
            return task;
        }
    }

    /// <summary>
    /// 线程池工作线程
    /// </summary>
    public class WorkThread
    {
        private Thread m_Thread;
        /// <summary>
        /// 获取此实例线程的唯一 ID
        /// </summary>
        public int Id
        {
            get
            {
                if (m_Thread != null)
                    return m_Thread.ManagedThreadId;
                return 0;
            }
        }
        /// <summary>
        /// 当前线程的状态
        /// </summary>
        public ThreadState ThreadState
        {
            get
            {
                if (m_Thread != null)
                    return m_Thread.ThreadState;
                return ThreadState.Unstarted;
            }
        }
        /// <summary>
        /// 当前任务
        /// </summary>
        public WorkTask Task { get; private set; }
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastChanged { get; private set; }
        /// <summary>
        /// 线程是否运行
        /// </summary>
        public bool IsRunnig
        {
            get
            {
                if ((this.ThreadState & ThreadState.Stopped) == ThreadState.Stopped
                    || (this.ThreadState & ThreadState.Unstarted) == ThreadState.Unstarted
                    || (this.ThreadState & ThreadState.Aborted) == ThreadState.Aborted)
                {
                    return false;
                }
                return true;
            }
        }
        private bool m_IsWaiting = false;
        /// <summary>
        /// 线程是否空闲
        /// </summary>
        public bool IsIdle
        {
            get
            {
                if (this.m_IsWaiting &&
                    ((this.ThreadState & ThreadState.WaitSleepJoin) == ThreadState.WaitSleepJoin))
                {
                    return true;
                }

                if (!this.IsRunnig)
                    return true;
                return false;
            }
        }

        protected WorkTaskScheduler TaskScheduler { get; private set; }

        public WorkThread(WorkTaskScheduler taskScheduler)
        {
            this.TaskScheduler = taskScheduler;
        }
        /// <summary>
        /// 总运行任务次数
        /// </summary>
        public ulong RunTaskCount { get; private set; }
        /// <summary>
        /// 总运行时间
        /// </summary>
        public double TotalRuntime { get; private set; }
        /// <summary>
        /// 平均每个任务执行时间
        /// </summary>
        public double AverageTime
        {
            get
            {
                if (this.RunTaskCount > 0)
                    return this.TotalRuntime / this.RunTaskCount;
                return 0;
            }
        }

        private ManualResetEvent reset = new ManualResetEvent(false);
        private object lockObject = new object();
        /// <summary>
        /// 开始新任务
        /// </summary>
        /// <param name="task"></param>
        /// <returns>线程挂起、结束时,返回true, 否则失败</returns>
        public bool Start(WorkTask task)
        {
            lock (lockObject)
            {
                if (task == null || !this.IsIdle)
                {
                    return false;
                }
                m_IsWaiting = false;
                task.Status = TaskStatus.WaitingToRun;
                this.Task = task;
                reset.Set();
                if (!this.IsRunnig)
                {
                    m_Thread = new Thread(new ThreadStart(Working));
                    m_Thread.Name = "ThreadPool";
                    m_Thread.IsBackground = true;
                    m_Thread.Start();
                }
                return true;
            }
        }

        public event ThreadExceptionEventHandler ThreadException;

        public void Abort()
        {
            try
            {
                if (m_Thread != null && m_Thread.IsAlive)
                    m_Thread.Abort();
            }
            catch { }
            m_Thread = null;
        }

        private void Working()
        {
            try
            {
                do
                {
                    m_IsWaiting = false;
                    this.LastChanged = DateTime.Now;
                    if (this.Task != null)
                    {
                        this.RunTaskCount++;
                        this.Task.OnDynamicInvoke();
                        this.TotalRuntime += this.Task.RunTime;

                        if (this.Task.IsFaulted
                            && this.Task.InnerException != null
                            && ThreadException != null)
                        {
                            ThreadException(this, new ThreadExceptionEventArgs(this.Task.InnerException));
                        }
                    }

                    WorkTask task = null;
                    if (this.TaskScheduler.TryDispatchWorkTask(out task))
                    {
                        if (task.Status == TaskStatus.WaitingForActivation)
                            task.Status = TaskStatus.WaitingToRun;
                        this.Task = task;
                    }
                    else
                    {
                        reset.Reset();
                        m_IsWaiting = true;
                        if (!reset.WaitOne(60000))
                        {
                            break;
                        }
                    }
                } while (true);
            }
            catch (System.Exception ex)
            {
                if (ThreadException != null)
                    ThreadException(this, new ThreadExceptionEventArgs(ex));
            }
        }
    }
}
