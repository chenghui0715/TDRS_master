using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YH.ICMS.Common;
using YH.ICMS.Common.Enumeration;
using YH.ICMS.Entity;

namespace YH.TRDS.Schedule
{
    public class TaskScheduleBase: ThreadBase
    {
        public Direction m_CurrentDirection =Direction.EmptyDirection;
        public VM_TDRSInfo m_Config { get; set; }
        public MSSchedule MSController { get; set; }
        public bool Start()
        {
           
            if (NeedChangeMode())
            {
                // 模拟器尚未支持换贝  todo
                //ChangeBayTask = CreateChangeBayTask();
                //if (ChangeBayTask == null)
                //{
                //    return false;
                //}
                //DispatchTask_ChangeBay();
            }

            return base.Start(1000);
        }
        /// <summary>
        /// 判断是否需要切换当前作业模式
        /// </summary>
        /// <returns></returns>
        private bool NeedChangeMode()
        {
            if (m_Config == null)
                return false;
            if (m_Config.HeadingDirection == Direction.EmptyDirection)
                return false;
            if (m_CurrentDirection == Direction.EmptyDirection)
                return false;
            if (m_CurrentDirection != m_Config.HeadingDirection)
                return true;
            return false;
        }

        public override void WorkFunc()
        {
            if (IsFinished())
            {
                m_bRun = false;
                return;
            }
           
        }

        protected bool m_bFinished = false;
        internal bool IsFinished()
        {
            lock (this)
            {

                if (m_bFinished)
                    return true;

               
                if (m_bFinished)
                {
                    // DeleteEntryExitPBs();

                    // DeleteStsStatusMoveKind(QCName);
                    m_bRun = false;
                }
                return m_bFinished;
            }
        }

    }
}
