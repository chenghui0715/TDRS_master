using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YH.ICMS.Common;

namespace YH.TRDS.Schedule
{
    public  class MSSchedule: ThreadBase
    {
        
        TaskScheduleBase m_TaskScheduler = null;
        public override void WorkFunc()
        {
            if (m_TaskScheduler != null
                && !m_TaskScheduler.IsFinished())
                return;
            StartScheduler();
        }

        private void StartScheduler()
        {

            return;
        }

        public bool Start()
        {
            string a = "100";
            return true;
        }

    }
}
