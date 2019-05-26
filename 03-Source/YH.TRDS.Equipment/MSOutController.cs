using Automation.BDaq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YH.ICMS.Common;
using YH.ICMS.Entity;

namespace YH.TRDS.Equipment
{
    public class MSOutController: ThreadBase
    {
        private Automation.BDaq.InstantDoCtrl instantDoCtrl1;
        public MSOutController()
        {
        }

        public MSOutController(int deviceNumber, VM_MSConfig msConfig)
        {
            if (instantDoCtrl1 == null)
                instantDoCtrl1 = new InstantDoCtrl();
            instantDoCtrl1.SelectedDevice = new DeviceInformation(deviceNumber);
        }

        public bool WriteOpenDo0()
        {
            ErrorCode err = ErrorCode.Success;
            err = instantDoCtrl1.Write(0, (byte)128);
            if (err != ErrorCode.Success)
            {
                HandleError(err);
                return false;
            }
            return true;
        }

        public bool WriteCloseDo0()
        {
            ErrorCode err = ErrorCode.Success;

            err = instantDoCtrl1.Write(0, (byte)0);
            if (err != ErrorCode.Success)
            {
                HandleError(err);
                return false;
            }
            return true;
        }

        private void HandleError(ErrorCode err)
        {
            if ((err >= ErrorCode.ErrorHandleNotValid) && (err != ErrorCode.Success))
            {
               // MessageBox.Show("Sorry ! Some errors happened, the error code is: " + err.ToString());
            }
        }


    }


   
}
