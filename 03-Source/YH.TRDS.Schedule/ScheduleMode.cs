using HslCommunication.ModBus;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using YH.ICMS.BLL;
using YH.ICMS.Common;
using YH.ICMS.Common.Enumeration;
using YH.ICMS.Entity;
using YH.TRDS.Equipment;
using System.Linq;

namespace YH.TRDS.Schedule
{

    
    public class ScheduleMode : ThreadBase
    {
        public ScheduleMode()
            {

            }
        public VM_TDRSInfo td { get; set; }
        private ModbusRtu busRtuClient = null;

        public delegate bool MessageStatusChanges(string  Message);
        public event MessageStatusChanges OnMessageStatusChanges;

        public delegate bool MsCountChanges(string nCount,string nPos);
        public event MsCountChanges OnMsCountChanges;
        public int n_MSOneCount = 0;
        public int n_MSTwoCount = 0;
        public int n_MSThreeCount = 0;
        public int n_MSFourCount = 0;
        /// <summary>
        /// 实时同步值
        /// </summary>
        public int n_VoltageLevel = 1;
        /// <summary>
        /// 根据界面上设置的时间同步值
        /// </summary>
        public int m_VoltageLevel = 1;


        private bool b_listenStatus = false;
        private bool b_ConnectStatus = false;
        private bool flag = false;

        private bool bStartCameraFlag = false;

        private bool bCameraHasOpened = false;

        public string m_StartCameraJson = null;
        public string m_StopCameraJson = null;
        public bool m_HaveTubeFlag = false;
        public DateTime datetime { get; set; }

        public DateTime SetParaDateTime { get; set; }
        public int   SetParaTimeout { get; set; }

        public VM_MsCountInfo m_MsCountInfo { get; set; }
        public VM_MSConfig m_Config { get; set; }

       


        Queue<VM_TDRSInfo> LstMsStatusQueue = new Queue<VM_TDRSInfo>();


        public Dictionary<string, string> m_HtDicPortToValue = new Dictionary<string, string>();

        public bool m_bCountFlag = false;

        public MSSchedule MSController { get; set; }

        public MSOutController MsOutController { get; set; }
        public VoltageBLL VoltageBLL { get; set; }

        /// <summary>
        /// 开启调度模块
        /// 1，连接TOSInterface
        /// 2，开启交互管理模块
        /// 3，开启各个QC的调度线程
        /// </summary>
        /// <param name="cfg">STSMS的配置信息</param>
        /// <returns>开启成功返回true，否则返回false，并在log中记录详细错误信息</returns>
        public bool Start(VM_MSConfig cfg)
        {
            if (cfg == null)
                return false;
            m_Config = cfg;

            if (!StartSocketListen())
            {
                LogHelper.WriteInfoLog("Start Socket Listen failed!");
                OutPutMessage("Start Socket Listen failed!");
                return false;
            }
            LogHelper.WriteInfoLog("Socket Listen connected.");
            OutPutMessage("Socket Listen connected.");

            if (!StartSchedulers(m_Config))
            {
                LogHelper.WriteInfoLog("Start scheduler failed!");
                OutPutMessage("Start scheduler failed!");
                return false;
            }
            LogHelper.WriteInfoLog("Schedule modle started!");
            OutPutMessage("Schedule modle started!");
            base.Start(2);
            //if (StartListenOpenCameraInfo(m_Config))
            //{

            //}
            if (!StartVoltageInfo(m_Config))
            {
                LogHelper.WriteInfoLog("Start VoltageController failed!");
                OutPutMessage("Start VoltageController failed!");
                return false;
            }
            LogHelper.WriteInfoLog("VoltageController modle started!");
            OutPutMessage("VoltageController modle started!");
            return true;
        }

        private bool StartVoltageInfo(VM_MSConfig m_Config)
        {
            VoltageController vc = new VoltageController(m_Config);
            vc.OnVoltageValueChanges+= new VoltageController.VoltageValueChanges(PrintVoltageValueChanged);
            vc.Start(1000);
            return true;

        }

        private bool StartSchedulers(VM_MSConfig m_Config)
        {
            if (m_Config == null)
                return false;
            MSController ms = new MSController(0,m_Config);
            ms.Start(2);
            ms.OnIDIInputStatusChanges += new MSController.IDIInputStatusChanges(GetStatusChanged);
            m_HtDicPortToValue = ms.m_HtDicPortToValue;
            if(MsOutController == null)
                MsOutController = new MSOutController(0, m_Config);
            TaskScheduleBase m_TaskSchedule = new TaskScheduleBase();
            return m_TaskSchedule.Start();
        }


        #region

        public bool PrintVoltageValueChanged(VM_VoltageInfo vi)
        {
            decimal retValue = 0;
            if (vi == null)
                return false;
            decimal voltageValue = 0;
            string v0= vi.V0.ToString().Replace("V", "");
            string v1 = vi.V1.ToString().Replace("V", "");
            string v2 = vi.V2.ToString().Replace("V", "");
            if (decimal.Parse(v0) < 0)
            {
                v0 = "0";
            }
            if (decimal.Parse(v1) < 0)
            {
                v1 = "0";
            }
            if (decimal.Parse(v2) < 0)
            {
                v2 = "0";
            }
            voltageValue = (decimal.Parse(v0) + decimal.Parse(v1) + decimal.Parse(v2))/3;
            retValue = voltageValue;
            if (VoltageBLL == null)
                VoltageBLL = new VoltageBLL();
            IList<VM_VoltageParam> vm_vp = VoltageBLL.GetVoltageDsToListInfo();
            vm_vp = vm_vp.Where(a => a.PreVoltage <= retValue && a.CurVoltage > retValue).ToList();
            if(vm_vp == null|| vm_vp.Count<=0)
                return false;
            foreach (VM_VoltageParam item in vm_vp)
            {
                n_VoltageLevel= item.VoltageLevel;
            }
            return true;
        }
        public bool GetStatusChanged(VM_TDRSInfo tdrsInfo)
        {

            if (tdrsInfo == null)
                return false;
            lock (LstMsStatusQueue)
            {
                
                if (tdrsInfo.InputDiInfo != "00001111")
                {
                    LstMsStatusQueue.Enqueue(tdrsInfo);
                    LogHelper.WriteInfoLog(tdrsInfo.InputDiInfo);
                }
                
            }
            
            return false;
        }

        

        
        public override void WorkFunc()
        {
            if (StartCalTimeOut())
            {
                if(bStartCameraFlag == true)
                {
                    LogHelper.WriteInfoLog("超时关闭相机");
                    CloseCamera();
                }
            }
            if (StartParaTimeOut())
            {
                m_VoltageLevel = n_VoltageLevel;
                LogHelper.WriteInfoLog("输出电压等级："+ m_VoltageLevel);
            }
            StartCalCount();
            
            //判断一号磁钢大于等于3
            StartCamera();
            StopCamera();
        }
        /// <summary>
        /// 开启更新相机参数的时间
        /// </summary>
        /// <returns></returns>
        private bool StartParaTimeOut()
        {
            TimeSpan timeSpan = DateTime.Now - SetParaDateTime;
            //正在执行拍照的时候不更新相机参数
            if (!bCameraHasOpened)
            {
                if (timeSpan.TotalSeconds >= SetParaTimeout && timeSpan.TotalSeconds <= SetParaTimeout + 100)
                {
                    if (!bStartCameraFlag)
                    {
                        SetParaDateTime = DateTime.Now;
                        return true;
                    }
                    else
                    {
                        bCameraHasOpened = true;
                    }

                }
            }
            else
            {
                if (!bStartCameraFlag)
                {
                    bCameraHasOpened = false;
                    SetParaDateTime = DateTime.Now;

                    return true;
                }
            }    
            return false;
        }

        private void StopCamera()
        {
            //2号和3号脉冲相等；四号大于
            if (n_MSFourCount>0 && n_MSThreeCount==n_MSTwoCount)
            {
                if (bStartCameraFlag)
                {
                    //关闭相机门
                    MsOutController.WriteCloseDo0();
                    LogHelper.WriteInfoLog("正常流程关闭IDO7");
                    StartSendStopedCameraCommand();
                    bStartCameraFlag = false;
                    ClearMSCount();
                }
            }
        }

        private void StartCamera()
        {
            //输出开机信号时间
            if (n_MSOneCount >= 3 && n_MSTwoCount>0 )
            {
                //记录开机信号时间
                if (!bStartCameraFlag)
                {
                    //开启相机门
                    MsOutController.WriteOpenDo0();
                    StartSendStartCameraCommand();
                    bStartCameraFlag = true;
                    //StartCalTimeOut(DateTime.Now);
                    datetime = DateTime.Now;
                }
            }
        }

        private void StartCalCount()
        {
            if (td == null)
                td = new VM_TDRSInfo();
            //if (LstMsStatusQueue != null && LstMsStatusQueue.Count > 0)
            //{
                

            //}
            //LogHelper.WriteInfoLog("出队列前队列成员数：" + LstMsStatusQueue.Count);
            td = LstMsStatusQueue.Dequeue();
            //LogHelper.WriteInfoLog("出队列后队列成员数：" + LstMsStatusQueue.Count + "出的队列为" + td.InputDiInfo);
            if (td == null || td.InputDiInfo == "00001111")
                return;
            LogHelper.WriteInfoLog("队列中出来的信号:" + td.InputDiInfo);
            string retResult = CalNumberMs(td);
            if (retResult == "1")
            {
                n_MSOneCount = n_MSOneCount + 1;
                LogHelper.WriteInfoLog("开启一号磁钢计数结果为: " + n_MSOneCount);
                //非开相机状态时，有234号脉冲则清空；开机状态时，234号有脉冲则不清空
                if ((n_MSTwoCount > 0 || n_MSThreeCount > 0 || n_MSFourCount > 0) && !bStartCameraFlag)
                {
                    ClearMSCount();
                    LogHelper.WriteInfoLog("一号磁钢有信号,但二三四号磁钢有计数清空所有计数！：非开相机状态时，有234号脉冲则清空；开机状态时，234号有脉冲则不清空");
                }
            }
            else if (retResult == "2")
            {
                if (n_MSOneCount > 0)
                {
                    // 对其它磁钢计数
                    CalOtherMSCount(td);
                    string ret = string.Format("开启其他磁钢计数结果：二号磁钢：{0}  ；三号磁钢：{1} ； 四号磁钢 ：{2} ", n_MSTwoCount, n_MSThreeCount, n_MSFourCount);
                    LogHelper.WriteInfoLog(ret);
                    if (n_MSOneCount < 3 || (n_MSTwoCount == 0 && n_MSOneCount >= 3))
                    {
                        if (n_MSThreeCount > 0 || n_MSFourCount > 0)
                        {
                            ClearMSCount();
                            LogHelper.WriteInfoLog("由于1号磁感小于3 或者 1号磁钢大于等于3且2号磁钢脉冲等于0，如果三号四号有脉冲 则清空所有计数！");
                        }
                    }
                }
            }
            else if (retResult == "0")
            {
                LogHelper.WriteInfoLog("所有磁钢信号为空，不作任何处理！");
            }
        }

        private void CalOtherMSCount(VM_TDRSInfo td)
        {
            if (td == null)
                return;
            if (td.MSStatus(td.InputDiInfo).MsStatusTwo == true)
            {
                n_MSTwoCount = n_MSTwoCount + 1;
            }
            else if(td.MSStatus(td.InputDiInfo).MsStatusThree == true)
            {
                n_MSThreeCount = n_MSThreeCount + 1; 
            }
            else if (td.MSStatus(td.InputDiInfo).MsStatusFour == true)
            {
                n_MSFourCount = n_MSFourCount + 1;
            }

        }

        private string CalNumberMs(VM_TDRSInfo td)
        {
            //"0"代表没有任何磁钢数据过来，1代表有一号信号过来 ，“2”代表有234信号过来
            string ret = "0";
            if (td == null)
                 ret= "0";
            else if (td.MSStatus(td.InputDiInfo).MsStatusOne == true)
            {
                ret = "1";
            }
            else if(td.InputDiInfo=="00001111")
            {
                ret = "0";
            }
            else if (td.MSStatus(td.InputDiInfo).MsStatusTwo == true || td.MSStatus(td.InputDiInfo).MsStatusThree == true || td.MSStatus(td.InputDiInfo).MsStatusFour == true)
            {
                ret = "2";
            }
            
            return ret;

        }



        

        /// <summary>
        /// 入库流程
        /// </summary>
        /// <param name="tdrsInfo"></param>
        //public void Enter(VM_TDRSInfo tdrsInfo)
        //{
        //    ////判断入库的车才开始计数
        //    //if (tdrsInfo.IsTubeEnter == false)
        //    //{
        //    //    LogHelper.WriteWarningLog("当前车没有入库的车，所有磁钢不计数");

        //    //    return;
        //    //}

        //    //RecordMsCountInfo(tdrsInfo);
        //    if (tdrsInfo.MSStatus(tdrsInfo.InputDiInfo).MsStatusOne)
        //    {
        //        n_MSOneCount = n_MSOneCount + 1;
        //        OnMsCountChanges(n_MSOneCount+"", "A");
        //    }
        //    else if (tdrsInfo.MSStatus(tdrsInfo.InputDiInfo).MsStatusTwo)
        //    {
              
        //        n_MSTwoCount = n_MSTwoCount + 1;
        //        OnMsCountChanges(n_MSTwoCount + "", "B");
        //        ///当2号磁钢有信号 且1号磁钢脉冲数超过3 则启动相机
        //        if (n_MSOneCount >= 3)
        //        {
        //            if (!bStartCameraFlag)
        //            {
        //                //开启相机门
        //                MsOutController.WriteOpenDo0();
        //                StartSendStartCameraCommand();
        //                bStartCameraFlag = true;
        //                //StartCalTimeOut(DateTime.Now);
        //                datetime = DateTime.Now;
        //            }

        //        }

        //    }else if (tdrsInfo.MSStatus(tdrsInfo.InputDiInfo).MsStatusThree)
        //    {
        //        n_MSThreeCount = n_MSThreeCount + 1;
        //        OnMsCountChanges(n_MSThreeCount + "", "C");
        //    }
        //    else if (tdrsInfo.MSStatus(tdrsInfo.InputDiInfo).MsStatusFour)
        //    {
        //        //当1号磁钢计数为0时，四号磁钢不在计数
        //        if (n_MSOneCount == 0)
        //        {
        //            return;
        //        }
                    
        //        n_MSFourCount = n_MSFourCount + 1;
        //        OnMsCountChanges(n_MSFourCount + "", "D");
        //        //1号和2号脉冲数小于等于2个，且四号磁钢有脉冲则关闭相机
        //        if ((n_MSOneCount - n_MSTwoCount) <= 2 && (n_MSOneCount - n_MSTwoCount) >= -2)
        //        {
        //            if (bStartCameraFlag)
        //            {
        //                //关闭相机门
        //                MsOutController.WriteCloseDo0();
        //                StartSendStopedCameraCommand();
        //                bStartCameraFlag = false;
        //                ClearMSCount();
        //            }
        //        }

        //    }
        //}

        

        private bool StartCalTimeOut()
        {
    
            TimeSpan timeSpan =DateTime.Now-datetime;
            if (timeSpan.TotalSeconds>m_Config.TimeOut && timeSpan.TotalSeconds < 60)
                return true;
            return false;
   
        }

       

        private void StartSendStartCameraCommand()
        {
            #region--------获取启动或关闭Json-------------
            GetCameraInfo(CameraStatus.Start, ref m_StartCameraJson);
            Send(m_StartCameraJson);
            LogHelper.WriteInfoLog(m_StartCameraJson);
            OutPutMessage(m_StartCameraJson);

            #endregion
            LogHelper.WriteInfoLog("发送相机开机指令！");
            OutPutMessage("发送相机开机指令！");
        }

        private void StartSendStopedCameraCommand()
        {
            GetCameraInfo(CameraStatus.Stop, ref m_StopCameraJson);
            LogHelper.WriteInfoLog(m_StopCameraJson);
            Send(m_StopCameraJson);
            OutPutMessage(m_StopCameraJson);
            LogHelper.WriteInfoLog("发送相机关机指令！");
            OutPutMessage("发送相机关机指令！");
        }

        /// <summary>
        /// 清除MS计数
        /// </summary>
        public void ClearMSCount()
        {
            n_MSOneCount = 0;
            OnMsCountChanges(n_MSOneCount + "", "A");
            n_MSTwoCount = 0;
            OnMsCountChanges(n_MSTwoCount + "", "B");
            n_MSThreeCount = 0;
            OnMsCountChanges(n_MSThreeCount + "", "C");
            n_MSFourCount = 0;
            OnMsCountChanges(n_MSFourCount + "", "D");
            LogHelper.WriteInfoLog("计数已清空！");
            OutPutMessage("计数已清空！");
        }

        public void CloseCamera()
        {
            bStartCameraFlag = false;
            MsOutController.WriteCloseDo0();
            LogHelper.WriteInfoLog("超时流程关闭IDO7");
            StartSendStopedCameraCommand();
            ClearMSCount();
        }
        #endregion

        private bool StartSocketListen()
        {
            try
            {
                if (!b_ConnectStatus)
                {
                    b_ConnectStatus = true;
                    b_listenStatus = true;
                    //点击开始监听时 在服务端创建一个负责监听IP和端口号的Socket
                    Socket socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPAddress ip = IPAddress.Parse(m_Config.IP);
                    //创建对象端口
                    IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32(m_Config.Port));

                    socketWatch.Bind(point);//绑定端口号
                    LogHelper.WriteInfoLog("Listen Success!");
                    OutPutMessage("Listen Success!");
                    socketWatch.Listen(2);//设置监听

                    //创建监听线程
                    Thread thread = new Thread(Listen);
                    thread.IsBackground = true;
                    thread.Start(socketWatch);

                }
            }
            catch (Exception ex)
            {
                string a = ex.Message;
                return false;
            }
            return true;
        }
        #region
        void ShowMsg(string msg)
        {
            LogHelper.WriteInfoLog(msg + "\r\n");
            OutPutMessage(msg);
        }

        /// <summary>
        /// 等待客户端的连接 并且创建与之通信的Socket
        /// </summary>
        Socket socketSend;

        void Listen(object o)
        {
            try
            {
                Socket socketWatch = o as Socket;
                while (b_listenStatus)
                {
                    socketSend = socketWatch.Accept();//等待接收客户端连接
                    ShowMsg(socketSend.RemoteEndPoint.ToString() + ":" + "Connect Success!");
                    //开启一个新线程，执行接收消息方法
                    Thread r_thread = new Thread(Received);
                    r_thread.IsBackground = true;
                    r_thread.Start(socketSend);
                }
            }
            catch { }
        }

        /// <summary>
        /// 服务器端不停的接收客户端发来的消息
        /// </summary>
        /// <param name="o"></param>
        void Received(object o)
        {
            try
            {
                Socket socketSend = o as Socket;
                while (true)
                {
                    //客户端连接服务器成功后，服务器接收客户端发送的消息
                    byte[] buffer = new byte[1024 * 1024 * 3];
                    //实际接收到的有效字节数
                    int len = socketSend.Receive(buffer);
                    if (len == 0)
                    {
                        break;
                    }
                    string str = Encoding.UTF8.GetString(buffer, 0, len);
                    ShowMsg(socketSend.RemoteEndPoint + ":" + str );
                    flag = true;
                }
            }
            catch { }
        }

        /// <summary>
        /// 服务器向客户端发送消息
        /// </summary>
        /// <param name="str"></param>
        void Send(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return;
            if (flag == false)
                return;
            byte[] buffer = Encoding.UTF8.GetBytes(str);
            socketSend.Send(buffer);
        }
        #endregion



        /// <summary>
        /// 获取相机序列化数据
        /// </summary>
        /// <returns>反馈序列化数据</returns>
        private List<VM_Content> GetCameraInfo(CameraStatus flag, ref string test)
        {
            IList<VM_CameraParameters> camerList = new List<VM_CameraParameters>();
            CamereBLL camereBLL = new CamereBLL();
            DataSet ds = new DataSet();
            ds = camereBLL.GetCameraList("");
            camerList = IListDataSet.DataSetToIList<VM_CameraParameters>(ds, 0);
            //if (m_VoltageLevel == 0)
            //{
            //    camerList = camerList.Where(s => s.exposureMode == "0").ToList();
            //}
            //else
            //{
            //    camerList = camerList.Where(s => s.exposureMode == m_VoltageLevel.ToString()).ToList();
            //}
            camerList = camerList.Where(s => s.exposureMode == m_VoltageLevel.ToString()).ToList();
            IList <VM_Content> camerList2 = new List<VM_Content>();
            if (flag == CameraStatus.Start)
            {
                camerList2.Add(new VM_Content { startTag = "Y", isMulti = "N", illumination = "1", station = "heifei", CameraParameters = camerList });
            }
            else if (flag == CameraStatus.Stop)
            {
                camerList2.Add(new VM_Content { startTag = "N", isMulti = "N", illumination = "1", station = "heifei", CameraParameters = camerList });
            }
            else
            {
                camerList2.Add(new VM_Content { startTag = "N", isMulti = "N", illumination = "1", station = "heifei", CameraParameters = camerList });
            }

            //序列化
            var lists = Newtonsoft.Json.JsonConvert.SerializeObject(camerList2);
            test = lists.ToString();
            //反序列化
            //Newtonsoft.Json.JsonConvert.DeserializeObject<List<VM_Content>>(lists);
            return null;
        }

        protected bool OutPutMessage(string message)
        {
            if (OnMessageStatusChanges != null)
                return OnMessageStatusChanges(message);
            else
                return false;
        }


        protected bool PrintMsCount(string nCount,string nPos)
        {
            if (OnMsCountChanges != null)
                return OnMsCountChanges(nCount, nPos);
            else
                return false;
        }

    }



}
