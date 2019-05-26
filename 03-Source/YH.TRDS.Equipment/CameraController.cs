using HslCommunication;
using HslCommunication.ModBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YH.ICMS.Common;
using YH.ICMS.Entity;

namespace YH.TRDS.Equipment
{
    public class CameraController : ThreadBase
    {
        private ModbusRtu busRtuClient = null;
        public VM_MSConfig Config { get; set; }
        public CameraController(VM_MSConfig msConfig)
        {
            Config = msConfig;

            busRtuClient?.Close();
            busRtuClient = new ModbusRtu(Config.Station);
            busRtuClient.AddressStartWithZero = Config.DDFlag;
            busRtuClient.IsStringReverse = Config.ComFlag;
            try
            {
                busRtuClient.SerialPortInni(sp =>
                {
                    sp.PortName = Config.Com;
                    sp.BaudRate = Config.BaudRate;
                    sp.DataBits = Config.DataBits;
                    sp.StopBits = Config.StopBits == 0 ? System.IO.Ports.StopBits.None : (Config.StopBits == 1 ? System.IO.Ports.StopBits.One : System.IO.Ports.StopBits.Two);
                    sp.Parity = Config.Parity == 0 ? System.IO.Ports.Parity.None : (Config.Parity == 1 ? System.IO.Ports.Parity.Odd : System.IO.Ports.Parity.Even);
                });
                busRtuClient.Open();
                LogHelper.WriteInfoLog("busRtuClient Open  Succeed！");
            }
            catch (Exception ex)
            {
                LogHelper.WriteInfoLog(ex.Message);
            }
        }

        public override void WorkFunc()
        {
            GetCameraOpenStatus();
        }

        
        private void GetCameraOpenStatus()
        {
            readResultRender(busRtuClient.ReadCoil("1"), "1");
        }


        /// <summary>
        /// 统一的读取结果的数据解析，显示
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <param name="address"></param>
        /// <param name="textBox"></param>
        private void readResultRender<T>(OperateResult<T> result, string address)
        {
            if (result.IsSuccess)
            {
                //textBox.AppendText(DateTime.Now.ToString("[HH:mm:ss] ") + $"[{address}] {result.Content}{Environment.NewLine}");
                LogHelper.WriteInfoLog(DateTime.Now.ToString("[HH:mm:ss] ") + $"[{address}] {result.Content}{Environment.NewLine}");
            }
            else
            {
                LogHelper.WriteErrorLog(DateTime.Now.ToString("[HH:mm:ss] ") + $"[{address}] 读取失败{Environment.NewLine}原因：{result.ToMessageShowString()}");
                //MessageBox.Show(DateTime.Now.ToString("[HH:mm:ss] ") + $"[{address}] 读取失败{Environment.NewLine}原因：{result.ToMessageShowString()}");
            }
        }

    }
}
