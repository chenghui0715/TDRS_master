using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Advantech.Adam;
using Advantech.Common;
using YH.ICMS.Common;
using YH.ICMS.Entity;

namespace YH.TRDS.Equipment
{
    public class VoltageController: ThreadBase
    {
        public delegate bool VoltageValueChanges(VM_VoltageInfo vi );
        public event VoltageValueChanges OnVoltageValueChanges;
        public VM_MSConfig Config { get; set; }
        public string Module = "";
        private int m_iCom, m_iAddr, m_iCount, m_iChTotal;
        private bool m_bStart;
        private byte[] m_byRange;
        private Adam4000Config m_adamConfig;
        private Adam4000Type m_Adam4000Type;
        private AdamCom adamCom;
        private string strReadCount;
        public VM_VoltageInfo vi { get; set; }
        public VM_VoltageCheckedInfo vci { get; set; }
        public VoltageController(VM_MSConfig msConfig)
        {
            Config = msConfig;
            m_iCom = 3;		// using COM4
            m_iAddr = 1;	// the slave address is 1
            m_iCount = 0;	// the counting start from 0
            m_bStart = false;
            m_Adam4000Type = Adam4000Type.Adam4019P; // the sample is for ADAM-4019P
            m_iChTotal = AnalogInput.GetChannelTotal(m_Adam4000Type);
            m_byRange = new byte[m_iChTotal];
            adamCom = new AdamCom(m_iCom);
            adamCom.Checksum = false; // disbale checksum

            Module = m_Adam4000Type.ToString();

            if (adamCom.OpenComPort())
            {
                // set COM port state, 9600,N,8,1
                adamCom.SetComPortState(Baudrate.Baud_9600, Databits.Eight, Parity.None, Stopbits.One);
                // set COM port timeout
                adamCom.SetComPortTimeout(500, 500, 0, 500, 0);
                m_iCount = 0; // reset the reading counter
                              // get module config
                if (!adamCom.Configuration(m_iAddr).GetModuleConfig(out m_adamConfig))
                {
                    adamCom.CloseComPort();
                    LogHelper.WriteErrorLog("Failed to get module config!");
                    return;
                }
                //
                RefreshChannelEnable();
                RefreshChannelRange();
                //
                m_bStart = true; // starting flag
            }
            else
            {
                LogHelper.WriteErrorLog("Failed to open COM port!");
            }
        }

        

        public override void WorkFunc()
        {

            GetVoltageValue();
        }


    

        private void GetVoltageValue()
        {
            float[] fVals;
            int[] iVals;
            Adam4000_ChannelStatus[] status;
            VM_VoltageInfo vi = new VM_VoltageInfo();
            m_iCount++;
            strReadCount = "Polling " + m_iCount.ToString() + " times...";
            if (m_adamConfig.Format == Adam4000_DataFormat.TwosComplementHex)
            {

                //if (adamCom.AnalogInput(m_iAddr).GetValues(8, out iVals))
                //{
                //    vi.V0 = "0x" + iVals[0].ToString("X04");
                //    vi.V1 = "0x" + iVals[1].ToString("X04");
                //    vi.V2 = "0x" + iVals[2].ToString("X04");
                //    vi.V3 = "0x" + iVals[3].ToString("X04");
                //    vi.V4 = "0x" + iVals[4].ToString("X04");
                //    vi.V5 = "0x" + iVals[5].ToString("X04");
                //    vi.V6 = "0x" + iVals[6].ToString("X04");
                //    vi.V7 = "0x" + iVals[7].ToString("X04");
                //}
                //else
                //{
                //    vi.V0 = "Failed to get!";
                //    vi.V1 = "Failed to get!";
                //    vi.V2 = "Failed to get!";
                //    vi.V3 = "Failed to get!";
                //    vi.V4 = "Failed to get!";
                //    vi.V5 = "Failed to get!";
                //    vi.V6 = "Failed to get!";
                //    vi.V7 = "Failed to get!";
                //}
            }
            else
            {
                if (adamCom.AnalogInput(m_iAddr).GetValues(8, out fVals, out status))
                {
                    string a0 = "";
                    string a1 = "";
                    string a2 = "";
                    string a3 = "";
                    string a4 = "";
                    string a5 = "";
                    string a6 = "";
                    string a7 = "";
                    if (vi == null)
                        vi = new VM_VoltageInfo();
                    RefreshValue(ref a0, status[0], fVals[0], m_byRange[0]);
                    vi.V0 = a0;
                    RefreshValue(ref a1, status[1], fVals[1], m_byRange[1]);
                    vi.V1 = a1;
                    RefreshValue(ref a2, status[2], fVals[2], m_byRange[2]);
                    vi.V2 = a2;
                    RefreshValue(ref a3, status[3], fVals[3], m_byRange[3]);
                    vi.V3 = a3;
                    RefreshValue(ref a4, status[4], fVals[4], m_byRange[4]);
                    vi.V4 = a4;
                    RefreshValue(ref a5, status[5], fVals[5], m_byRange[5]);
                    vi.V5 = a5;
                    RefreshValue(ref a6, status[6], fVals[6], m_byRange[6]);
                    vi.V6 = a6;
                    RefreshValue(ref a7, status[7], fVals[7], m_byRange[7]);
                    vi.V7 = a7;
                    ///传送电压变化值
                    PrintVoltageValueInfo(vi);
                }
                else
                {
                    vi.V0 = "0";
                    vi.V1 = "0";
                    vi.V2 = "0";
                    vi.V3 = "0";
                    vi.V4 = "0";
                    vi.V5 = "0";
                    vi.V6 = "0";
                    vi.V7 = "0";
                }
            }
        }

        private void RefreshChannelEnable()
        {
            bool[] bEnabled;
            if(vci==null)
             vci = new VM_VoltageCheckedInfo();
            if (adamCom.AnalogInput(m_iAddr).GetChannelEnabled(8, out bEnabled))
            {
                vci.FlagV0 = bEnabled[0];
                vci.FlagV1 = bEnabled[1];
                vci.FlagV2 = bEnabled[2];
                vci.FlagV3 = bEnabled[3];
                vci.FlagV4 = bEnabled[4];
                vci.FlagV5 = bEnabled[5];
                vci.FlagV6 = bEnabled[6];
                vci.FlagV7 = bEnabled[7];
                
            }
            else
                LogHelper.WriteErrorLog("GetChannelEnabled() failed");
        }

        private void RefreshChannelRange()
        {
            byte byRange;
            int iIdx;

            for (iIdx = 0; iIdx < m_iChTotal; iIdx++)
            {
                if (adamCom.AnalogInput(m_iAddr).GetInputRange(iIdx, out byRange))
                    m_byRange[iIdx] = byRange;
                else
                {
                    LogHelper.WriteErrorLog("GetRangeCode() failed");
                    break;
                }
            }
        }

        private void RefreshValue(ref string i_txtCh, Adam4000_ChannelStatus i_status, float i_fValue, byte i_byRange)
        {
            if (i_status == Adam4000_ChannelStatus.Normal)
            {
                if (m_adamConfig.Format == Adam4000_DataFormat.EngineerUnit)
                    i_txtCh= i_fValue.ToString(AnalogInput.GetFloatFormat(m_Adam4000Type, i_byRange)) + " " + AnalogInput.GetUnitName(m_Adam4000Type, i_byRange);
                else if (m_adamConfig.Format == Adam4000_DataFormat.Percent)
                    i_txtCh = i_fValue.ToString("#0.00") + " %";
            }
            else
                i_txtCh = i_status.ToString();
        }

        protected bool PrintVoltageValueInfo(VM_VoltageInfo vi)
        {
            if (OnVoltageValueChanges != null)
                return OnVoltageValueChanges(vi);
            else
                return false;
        }

    }
}
