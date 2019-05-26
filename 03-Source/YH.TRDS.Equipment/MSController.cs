using Automation.BDaq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YH.ICMS.Common;
using YH.ICMS.Common.Enumeration;
using YH.ICMS.Entity;
using ZECS.STS.EventLog;

namespace YH.TRDS.Equipment
{
    public class MSController : ThreadBase
    {
        

        public delegate bool IDIInputStatusChanges(VM_TDRSInfo vm_tdrs);
        public event IDIInputStatusChanges OnIDIInputStatusChanges;
        #region -------private---------
        //private Label[] m_portNum;
        //private Label[] m_portHex;
        //private PictureBox[,] m_pictrueBox;
        private const int m_startPort = 0;
        private const int m_portCountShow = 4;
        public InstantDiCtrl instantDiCtrl1=null;

        #endregion

        #region-------public----------
        public Dictionary<string, string> m_HtDicPortToValue = new Dictionary<string, string>();
        public VM_TDRSInfo tdrs = new VM_TDRSInfo();
        public VM_MSConfig Config { get; set; }
        #endregion

        #region-------Load---------
        public MSController()
        {
            if (instantDiCtrl1 == null)
                instantDiCtrl1 = new InstantDiCtrl();
        }

        public MSController(int deviceNumber, VM_MSConfig msConfig)
        {
            if (instantDiCtrl1 == null)
                instantDiCtrl1 = new InstantDiCtrl();
            instantDiCtrl1.SelectedDevice = new DeviceInformation(deviceNumber);
            Config = msConfig;
            STSEventLogManage.Instance.Start();
            
        }

        public void start()
        {
            if (!instantDiCtrl1.Initialized)
            {
                MessageBoxHelper.ShowInformationMessageBox("No device be selected or device open failed!");
                //this.Close();
                return;
            }
        }

        private void InstantDiForm_Load(object sender, EventArgs e)
        {
            //The default device of project is demo device, users can choose other devices according to their needs. 
            if (!instantDiCtrl1.Initialized)
            {
                MessageBoxHelper.ShowInformationMessageBox("No device be selected or device open failed!");
                //this.Close();
                return;
            }
        }


        public override void WorkFunc()
        {
            GetStatus();
        }

        void GetStatus()
        {
            // read Di port state
            byte portData = 0;
            ErrorCode err = ErrorCode.Success;

            for (int i = 0; (i + m_startPort) < instantDiCtrl1.Features.PortCount && i < m_portCountShow; ++i)
            {
                err = instantDiCtrl1.Read(i + m_startPort, out portData);
                if (err != ErrorCode.Success)
                {
                    //timer1.Enabled = false;
                    HandleError(err);
                    return;
                }
                m_HtDicPortToValue[(i + m_startPort).ToString()] = portData.ToString("X2");
               // m_portNum[i].Text = (i + m_startPort).ToString();
                //m_portHex[i].Text = portData.ToString("X2");
                //加入设备状态变化的列表
                if (i == 1)
                {
                    if (tdrs == null)
                    {
                        VM_TDRSInfo tdrs = new VM_TDRSInfo();
                    }
                    tdrs.HeadingDirection = (Direction)Enum.Parse(typeof(Direction), Config.TrainDirection);
                    tdrs.PortInfo = portData.ToString("X2");
                    tdrs.InputDiInfo = tdrs.InputDiInfo;
                    tdrs.CREATED = DateTime.Now;
                    bool flag = false;
                    flag = STSEventLogManage.Instance.AddEventLog(tdrs);
                    tdrs.MSStatus(tdrs.InputDiInfo);
                    if (flag)
                    {
                        //信号产生变化后，通知外部
                        IDIInputInfo();
                    }
                }

                // Set picture box state
                for (int j = 0; j < 8; ++j)
                {
                    //m_pictrueBox[i, j].Image = imageList1.Images[(portData >> j) & 0x1];
                    //m_pictrueBox[i, j].Invalidate();
                }
            }
        }
        #region-------Event-------



        /// <summary>
        /// 处理失败后退出
        /// </summary>
        /// <param name="err"></param>
        private void HandleError(ErrorCode err)
        {
            if ((err >= ErrorCode.ErrorHandleNotValid) && (err != ErrorCode.Success))
            {
                MessageBoxHelper.ShowInformationMessageBox("Sorry ! Some errors happened, the error code is: " + err.ToString());
            }
        }
        #endregion

        protected bool IDIInputInfo()
        {
            if (OnIDIInputStatusChanges != null)
                return OnIDIInputStatusChanges(tdrs);
            else
                return false;
        }

        
    }
    #endregion


}
