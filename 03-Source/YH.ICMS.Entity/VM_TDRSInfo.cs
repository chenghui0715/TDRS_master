using System;
using System.Collections.Generic;
using YH.ICMS.Common.Enumeration;

namespace YH.ICMS.Entity
{
    public class VM_TDRSInfo
    {
        public  VM_MSStatus vm_MSStatus = new VM_MSStatus(Direction.入库方向);
        /// <summary>
        /// 车头方向
        /// </summary>
        public Direction HeadingDirection { get; set; }

        //private bool _mIsTubeLeave;
        //public bool IsTubeEnter {
        //    get
        //    {
        //        if (vm_MSStatus.GetTubeIsEnter())
        //        {
        //            _mIsTubeLeave = true;
        //        }
        //        return _mIsTubeLeave;
        //    }

        //    set
        //    {
        //        _mIsTubeLeave = value;

        //    }
        //}

        /// <summary>
        /// 端口所有信息
        /// </summary>
        public string PortInfo { get; set; }
        
        private string _byteArray;

        /// <summary>
        /// 输入IDI信号
        /// </summary>
        public string InputDiInfo
        {
            get
            {

                if (!String.IsNullOrWhiteSpace(PortInfo))
                {
                    return ConvertHexadecimalToBinarySystem(PortInfo);
                }
                return null;
            }

            set
            {
                _byteArray = value;

            }

        }

        /// <summary>
        /// 输出IDO信号
        /// </summary>
        public List<string> ListOutputInfo { get; set; }

        /// <summary>
        /// 将String转换成字节数组
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length];
            for (int i = 0; i < s.Length; i += 1)
            {
                buffer[i] = (byte)Convert.ToByte(s.Substring(i, 1), 16);
            }

            return buffer;
        }

        /// <summary>
        /// 十六进制数字转换成二进制，若二进制不满八位，则左补0至八位长度二进制
        /// </summary>
        /// <param name="str">十六进制数</param>
        /// <returns>八位二进制长度数据</returns>
        public string ConvertHexadecimalToBinarySystem(string str)
        {
            string ret = null;
            int binaryValue = 0;
            if (string.IsNullOrWhiteSpace(str))
                return null;

            try
            {
                binaryValue = System.Int32.Parse(str, System.Globalization.NumberStyles.HexNumber);
                // string a = System.Convert.ToString(binaryValue, 2);
                //byte[] binaryValue  =Encoding.Default.GetBytes(str);
                //这就需要补齐位数了PadLeft(int a,cha b),其中a为总共多少位，b为用什么补齐
                ret = Convert.ToString(binaryValue, 2).PadLeft(8, '0');
            }
            catch (Exception ex)
            {
                return ret;
            }

            return ret;
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CREATED { get; set; }

      
        public VM_MSStatus MSStatus(string InputDiInfo)
        {
           
            if (InputDiInfo != null)
            {
                byte[] buff = HexStringToByteArray(InputDiInfo);
                vm_MSStatus.MsStatusOne =buff[0].ToString()=="0"?false:true;
                vm_MSStatus.MsStatusTwo = buff[1].ToString() == "0" ? false : true;

                vm_MSStatus.MsStatusThree = buff[2].ToString() == "0" ? false : true;
                vm_MSStatus.MsStatusFour = buff[3].ToString() == "0" ? false : true;
                vm_MSStatus.BakupOne = buff[4].ToString() == "0" ? false : true;
                vm_MSStatus.BakupTwo =buff[5].ToString() == "0" ? false : true;
                vm_MSStatus.BakupThree = buff[6].ToString() == "0" ? false : true;
                vm_MSStatus.BakupFour = buff[7].ToString() == "0" ? false : true;

               
            }


            return vm_MSStatus;
        }

        public VM_TDRSInfo DeepCopy()
        {
            VM_TDRSInfo status = new VM_TDRSInfo();
            status.InputDiInfo = this.InputDiInfo ;
            status.PortInfo = this.PortInfo;
            return status;
        }
    }
}
