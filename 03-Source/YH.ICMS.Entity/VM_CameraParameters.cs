using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YH.ICMS.Entity
{   
    //相机曝光参数实体类
    public class VM_CameraParameters
    {

        public int ID { get; set; }
        /// <summary>
        /// 相机名称
        /// </summary>
        public string camera { get; set; }
        /// <summary>
        /// 曝光模式
        /// </summary>
        public string exposureMode { get; set; }
        /// <summary>
        /// 曝光上限
        /// </summary>
        public string exposureMax { get; set; }
        /// <summary>
        /// 曝光下限
        /// </summary>
        public string exposureMin { get; set; }
        /// <summary>
        /// 曝光值
        /// </summary>
        public string exposureValue { get; set; }
       
    }
}
