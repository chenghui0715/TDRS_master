using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using ICMS.Modules.BaseComponents.Beans;
using ICMS.Modules.BaseComponents.IDAO;
using ScanSokectLibrary;


namespace ICMS.Modules.BaseComponents.Commons
{
    public class TcpController
    {
        private Thread _checkStatusThread;
        private TcpListener _tcpListener;
        private IPEndPoint _myServer;
        private System.Net.Sockets.Socket sock, accSock;
        //private UdpClient _senderUdpClient;
        private Thread _listenThread;
        private Thread _listenScanThread;
        private IPAddress _serverIp;
        private EndPoint _remoteIp;
        public ModuleController Module { set; get; }
        public ModuleEntity ModuleInfo { set; get; }
        private readonly EQItem _eqItem;
        public bool ConnectState { set; get; }
        /*
                public List<TcpController> TcpControllers { set; get; }
        */
        private ScanSokect _scanClient;
        public TcpController(EQItem eqItem)
        {
            _eqItem = eqItem;
        }
        public virtual void StartService()
        {
            if (_listenThread == null)
            {
                #region lableView客户端建立监听
                if (ModuleInfo.EqType == "1")
                {

                    IPAddress[] ip = Dns.GetHostAddresses(Dns.GetHostName());
                    foreach (IPAddress ipAddress in ip)
                    {
                        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                        {
                            _serverIp = ipAddress;
                        }
                    }

                    if (_serverIp != null && ModuleInfo.PortNumber != null)
                    {

                        _listenThread = new Thread(ListenForLableViewClients);
                        _listenThread.Start();
                        _eqItem.btnEQ.BackColor = Color.Orange;
                        _eqItem.EQMessage.Text = @"Listening...";
                        _eqItem.停止服务ToolStripMenuItem.Enabled = true;
                        _eqItem.启动服务ToolStripMenuItem.Enabled = false;
                        ConnectState = true;
                    }
                }
                #endregion

                #region 自动扫描仪客户端建立监听
                if (ModuleInfo.EqType == "2")
                {
                    if (ModuleInfo == null || ModuleInfo.ClientIp == "") return;
                    _scanClient = new ScanSokect(ModuleInfo.ClientIp, 9003, 9004);
                    _listenScanThread = new Thread(ListenForScanClientsConnectState);
                    _listenScanThread.Start();

                    if (Module.KepController != null)
                    {
                        Module.KepController.KepHelper.DatachangeEvent += new TLAgent.OpcLibrary.OpcHelper.DataChange(KepHelper_DatachangeEvent);
                    }
                }
                #endregion

                #region 采集器客户端监听
                else
                {
                    IPAddress[] ip = Dns.GetHostAddresses(Dns.GetHostName());
                    foreach (IPAddress ipAddress in ip)
                    {
                        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                        {
                            _serverIp = ipAddress;
                        }
                    }

                    if (_serverIp != null && ModuleInfo.PortNumber != null)
                    {
                        _tcpListener = new TcpListener(_serverIp, Convert.ToInt32(ModuleInfo.PortNumber));
                        _listenThread = new Thread(ListenForPadClients);
                        _listenThread.Start();
                        _eqItem.btnEQ.BackColor = Color.Orange;
                        _eqItem.EQMessage.Text = @"Listening...";
                        _eqItem.停止服务ToolStripMenuItem.Enabled = true;
                        _eqItem.启动服务ToolStripMenuItem.Enabled = false;
                        ConnectState = true;
                        if (Module.KepController != null)
                        {
                            Module.KepController.KepHelper.DatachangeEvent += new TLAgent.OpcLibrary.OpcHelper.DataChange(KepHelper_DatachangeEvent);
                        }
                    }
                }
                #endregion
            }
        }
        private void KepHelper_DatachangeEvent(Dictionary<string, object> dic)
        {
            try
            {
                ExecutionResult exeResult = new ExecutionResult();
                if (dic.Count > 0)
                {
                    /* 
                     * Scan1=X射线工位(PG.LineA.Scan1-1=正常,Scan2=第一次真空度测试，Scan3=喷漆,Scan4=第二次真空度
                     */
                    foreach (var o in dic)
                    {
                        if (o.Key != null)
                        {
                            //PLC触发的读取信号
                            if (o.Key.Equals("PG.LineA.Xray.XrayScan") || o.Key.Equals("PG.LineA.FirstVacuumTest.FirstVacuumScanTouch") || o.Key.Equals("PG.LineA.SecondVacuumTest.SecondVacuumScanTouch") || o.Key.Equals("PG.LineA.SprayPainting.SprayPaintingScan"))
                            {
                                if ((bool)dic[o.Key])
                                {
                                    if (_scanClient.ConnectState())
                                    {
                                        var scanData = _scanClient.Read();//触发固定扫描枪扫描二维码读取内容
                                        if (scanData.Length > 0)
                                        {
                                            //判斷要截取的字符串是否是數組和字母的組合
                                            string[] sn = scanData.Split(' ');
                                            if (sn.Length == 4)
                                            {
                                                if (sn[3].Length > 10)
                                                    scanData = sn[3].Substring(0, 10);
                                            }
                                            exeResult.Sn = scanData;
                                            exeResult.StationName = ModuleInfo.StationName;
                                            exeResult = Module.Check(exeResult);
                                            _eqItem.EQMessage.Text = exeResult.Message;
                                            _eqItem.EQMessage.Tag = exeResult.Message;
                                            _eqItem.EQMessage.ForeColor = !exeResult.Status ? Color.Red : Color.Blue;

                                        }
                                        else
                                        {
                                            exeResult.Status = false;
                                            exeResult.Message = "扫描的条码内容为空!";
                                        }
                                    }
                                    else
                                    {
                                        exeResult.Status = false;
                                        exeResult.Message = "未连接到扫描仪!";
                                    }
                                }
                            }
                            else if (o.Key.Equals("PG.LineA.Xray.Commit_To_MES") && (bool)(o.Value))//Xray PLC触发的保存信号
                            {
                                exeResult.StationName = ModuleInfo.StationName;
                                exeResult = Module.SaveSn(exeResult);
                                _eqItem.EQMessage.Text = exeResult.Message;
                                _eqItem.EQMessage.Tag = exeResult.Message;
                                _eqItem.EQMessage.ForeColor = !exeResult.Status ? Color.Red : Color.Blue;
                            }
                            else if (o.Key.Equals("PG.LineA.FirstVacuumTest.DoFinish") && (bool)(o.Value))//第一次真空度测试 PLC触发的保存信号
                            {
                                exeResult.StationName = ModuleInfo.StationName;
                                exeResult = Module.SaveSn(exeResult);
                                _eqItem.EQMessage.Text = exeResult.Message;
                                _eqItem.EQMessage.Tag = exeResult.Message;
                                _eqItem.EQMessage.ForeColor = !exeResult.Status ? Color.Red : Color.Blue;
                            }
                            else if (o.Key.Equals("PG.LineA.SecondVacuumTest.DoFinish") && (bool)(o.Value))//第二次真空度测试 PLC触发的保存信号
                            {
                                exeResult.StationName = ModuleInfo.StationName;
                                exeResult = Module.SaveSn(exeResult);
                                _eqItem.EQMessage.Text = exeResult.Message;
                                _eqItem.EQMessage.Tag = exeResult.Message;
                                _eqItem.EQMessage.ForeColor = !exeResult.Status ? Color.Red : Color.Blue;
                            }
                            else if (o.Key.Equals("PG.LineA.TouchResistance.DoFinish") && (bool)(o.Value))//接触电阻PLC触发的保存信号
                            {
                                exeResult.StationName = ModuleInfo.StationName;
                                exeResult = Module.SaveSn(exeResult);
                                _eqItem.EQMessage.Text = exeResult.Message;
                                _eqItem.EQMessage.Tag = exeResult.Message;
                                _eqItem.EQMessage.ForeColor = !exeResult.Status ? Color.Red : Color.Blue;
                            }
                            else if (o.Key.Equals("PG.LineA.VacuumRetest.DoFinish") && (bool)(o.Value))//真空度复测PLC触发的保存信号
                            {
                                exeResult.StationName = ModuleInfo.StationName;
                                exeResult = Module.SaveSn(exeResult);
                                _eqItem.EQMessage.Text = exeResult.Message;
                                _eqItem.EQMessage.Tag = exeResult.Message;
                                _eqItem.EQMessage.ForeColor = !exeResult.Status ? Color.Red : Color.Blue;
                            }
                            else if (o.Key.Equals("PG.LineA.Xray.LoginCommit") && (bool)(o.Value))//X射线PLC触发的登录验证信号
                            {
                                exeResult.StationName = ModuleInfo.StationName;
                                exeResult = Module.CheckLogin(exeResult);
                                _eqItem.EQMessage.Text = exeResult.Message;
                                _eqItem.EQMessage.Tag = exeResult.Message;
                                _eqItem.EQMessage.ForeColor = !exeResult.Status ? Color.Red : Color.Blue;
                            }
                        }
                    }

                }
            }
            catch (Exception e)
            {
                _eqItem.EQMessage.Text = e.Message;
                _eqItem.EQMessage.Tag = e.Message;
                _eqItem.EQMessage.ForeColor = Color.Red;

            }

        }
        public void CloseService()
        {
            Thread.Sleep(500);
            if (_tcpListener != null)
            {
                _tcpListener.Stop();
                _tcpListener = null;
            }

            if (_listenThread != null)
            {
                _listenThread.Abort();
                _listenThread.Join();
                _listenThread = null;
            }
            if (_scanClient != null)
            {
                _scanClient.DisConnected();
                _scanClient = null;
            }

            if (_checkStatusThread != null)
            {
                _checkStatusThread.Abort();
                _checkStatusThread.Join();
                _checkStatusThread = null;
            }
            if (_listenScanThread != null)
            {
                _listenScanThread.Abort();
                _listenScanThread.Join();
                _listenScanThread = null;
            }

            ConnectState = false;
            _eqItem.btnEQ.BackColor = Color.Red;
            _eqItem.启动服务ToolStripMenuItem.Enabled = true;
            _eqItem.停止服务ToolStripMenuItem.Enabled = false;
            _eqItem.EQMessage.Text = @"Service Stoped.";

        }


        private void ListenForLableViewClients()
        {
            try
            {
                _checkStatusThread = new Thread(ListenForLableViewClientsConnectState);
                _checkStatusThread.Start();
                while (true)
                {
                    if (!_tcpListener.Pending())
                    {
                        _myServer = new IPEndPoint(_serverIp, Convert.ToInt32(ModuleInfo.PortNumber));
                        sock = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream,
                                                             ProtocolType.Tcp);
                        sock.Bind(_myServer);
                        TcpClient client = _tcpListener.AcceptTcpClient();


                        //_remoteIp = client.Client.RemoteEndPoint;
                        //if (_senderUdpClient == null)
                        //{
                        //    string[] ips = _remoteIp.ToString().Split(':');
                        //    if (ips.Length == 2)
                        //    {
                        //        _senderUdpClient = new UdpClient();
                        //        _senderUdpClient.Connect(ips[0], int.Parse(ModuleInfo.PortNumber));
                        //    }
                        //}
                        var clientThread = new Thread(HandleClientComm);
                        clientThread.Start(client);
                        _eqItem.btnEQ.BackColor = Color.ForestGreen;
                    }

                }

            }
            catch (Exception e)
            {

            }
        }

        private void ListenForLableViewClientsConnectState()
        {

            while (true)
            {
                try
                {
                    Thread.Sleep(5000);
                    if (_remoteIp != null)
                    {
                        string[] ips = _remoteIp.ToString().Split(':');
                        if (ips.Length == 2)
                        {
                            if (Ping(ips[0]))
                            {
                                _eqItem.btnEQ.BackColor = Color.ForestGreen;
                                _eqItem.停止服务ToolStripMenuItem.Enabled = true;
                                _eqItem.启动服务ToolStripMenuItem.Enabled = false;
                                ConnectState = true;
                            }
                            else
                            {
                                _eqItem.btnEQ.BackColor = Color.Red;
                                _eqItem.EQMessage.Text = @"连接LableView客户端失败！";
                                _eqItem.停止服务ToolStripMenuItem.Enabled = false;
                                _eqItem.启动服务ToolStripMenuItem.Enabled = true;
                                ConnectState = false;
                            }

                        }


                    }
                    //else
                    //{
                    //    _eqItem.btnEQ.BackColor = Color.Red;
                    //    _eqItem.EQMessage.Text = @"连接LableView客户端失败！";
                    //    _eqItem.停止服务ToolStripMenuItem.Enabled = false;
                    //    _eqItem.启动服务ToolStripMenuItem.Enabled = true;
                    //    ConnectState = false;}
                }
                catch
                {

                }
            }


        }

        private void ListenForPadClients()
        {
            try
            {
                _tcpListener.Start();
                _checkStatusThread = new Thread(ListenForPadClientsConnectState);
                _checkStatusThread.Start();
                while (true)
                {
                    if (!_tcpListener.Pending())
                    {

                        TcpClient client = _tcpListener.AcceptTcpClient();
                        //_remoteIp = client.Client.RemoteEndPoint;
                        //if (_senderUdpClient == null)
                        //{
                        //    string[] ips = _remoteIp.ToString().Split(':');
                        //    if (ips.Length == 2)
                        //    {
                        //        _senderUdpClient = new UdpClient();
                        //        _senderUdpClient.Connect(ips[0], int.Parse(ModuleInfo.PortNumber));
                        //    }
                        //}
                        var clientThread = new Thread(HandleClientComm);
                        clientThread.Start(client);
                        _eqItem.btnEQ.BackColor = Color.ForestGreen;
                    }

                }

            }
            catch (Exception e)
            {

            }
        }

        private void ListenForPadClientsConnectState()
        {

            while (true)
            {
                try
                {
                    Thread.Sleep(5000);
                    if (_remoteIp != null)
                    {
                        string[] ips = _remoteIp.ToString().Split(':');
                        if (ips.Length == 2)
                        {
                            if (Ping(ips[0]))
                            {
                                _eqItem.btnEQ.BackColor = Color.ForestGreen;
                                _eqItem.停止服务ToolStripMenuItem.Enabled = true;
                                _eqItem.启动服务ToolStripMenuItem.Enabled = false;
                                ConnectState = true;
                            }
                            else
                            {
                                _eqItem.btnEQ.BackColor = Color.Red;
                                _eqItem.EQMessage.Text = @"连接数据采集器失败！";
                                _eqItem.停止服务ToolStripMenuItem.Enabled = false;
                                _eqItem.启动服务ToolStripMenuItem.Enabled = true;
                                ConnectState = false;
                            }

                        }
                    }
                }
                catch
                {

                }
            }


        }

        private void ListenForScanClientsConnectState()
        {

            while (true)
            {
                Thread.Sleep(5000);
                try
                {
                    if (_scanClient == null) continue;
                    if (!_scanClient.ConnectState())
                    {

                        if (_scanClient.Connect())
                        {
                            _eqItem.btnEQ.BackColor = Color.ForestGreen;
                            _eqItem.EQMessage.Text = @"listening...";
                            _eqItem.停止服务ToolStripMenuItem.Enabled = true;
                            _eqItem.启动服务ToolStripMenuItem.Enabled = false;
                            ConnectState = true;
                        }
                        else
                        {
                            _eqItem.btnEQ.BackColor = Color.Red;
                            _eqItem.EQMessage.Text = @"连接扫描仪失败！";
                            _eqItem.停止服务ToolStripMenuItem.Enabled = false;
                            _eqItem.启动服务ToolStripMenuItem.Enabled = true;
                            ConnectState = false;

                        }

                    }

                }
                catch (Exception ex)
                {
                    if (_scanClient != null)
                        _scanClient.DisConnected();
                }
            }
        }
        private static bool Ping(string ip)
        {
            System.Net.NetworkInformation.Ping p = new System.Net.NetworkInformation.Ping();
            System.Net.NetworkInformation.PingOptions options = new System.Net.NetworkInformation.PingOptions();
            options.DontFragment = true;
            string data = "Test Data!";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 1000; // Timeout 时间，单位：毫秒  
            System.Net.NetworkInformation.PingReply reply = p.Send(ip, timeout, buffer, options);
            if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                return true;
            else
                return false;
        }
        protected virtual void HandleClientComm(object client)
        {
            var tcpClient = (TcpClient)client;
            _remoteIp = tcpClient.Client.RemoteEndPoint;

            UdpClient _senderUdpClient = null;
            string[] ips = _remoteIp.ToString().Split(':');
            if (ips.Length == 2)
            {
                _senderUdpClient = new UdpClient();
                _senderUdpClient.Connect(ips[0], int.Parse(ModuleInfo.PortNumber));
            }

            while (true)
            {
                try
                {

                    var message = new byte[2500];
                    if (tcpClient == null || !tcpClient.Connected) return;
                    var clientStream = tcpClient.GetStream();
                    int bytesRead = clientStream.Read(message, 0, 2500);
                    if (bytesRead != 0)
                    {
                        //逻辑处理
                        ExecutionResult result = null;
                        switch (ModuleInfo.EqType)
                        {
                            #region       //对方电脑设备发来的指令
                            case "1":
                                string clientData = Encoding.ASCII.GetString(message);//接收字符串
                                if (clientData.Length > 0)
                                {
                                    // clientData = clientData.Substring(0, 10);
                                    result = new ExecutionResult
                                    {
                                        Sn = clientData,
                                        StationName = ModuleInfo.StationName.Substring(0, ModuleInfo.StationName.IndexOf('('))
                                    };

                                    result = Module.LableViewSave(result); //此处为调用模块的接口
                                    if (result != null)
                                    {
                                        _eqItem.EQMessage.Text = result.Message;
                                        _eqItem.EQMessage.Tag = result.Message;
                                        _eqItem.EQMessage.ForeColor = !result.Status ? Color.Red : Color.Blue;

                                    }

                                }

                                break;
                            #endregion

                            #region//采集器发送的指令
                            default:
                                var binary = new XmlSerializer(typeof(ExecutionResult));
                                var stream = new MemoryStream(message);
                                result = binary.Deserialize(stream) as ExecutionResult;
                                if (result != null)
                                {
                                    result.StationName = ModuleInfo.StationName;
                                    string[] sn = result.Sn.Split(' ');
                                    if (sn.Length == 4)
                                    {
                                        result.Sn = sn[3];
                                        result.ProductType =sn[2];
                                    }
                                    result = Module.Check(result); //此处为调用模块的接口
                                    if (result != null)
                                    {
                                        result.Anything = null;
                                        _eqItem.EQMessage.Text = result.Message;
                                        _eqItem.EQMessage.Tag = result.Message;
                                        _eqItem.EQMessage.ForeColor = !result.Status ? Color.Red : Color.Blue;
                                    }

                                    stream.Close();
                                    stream = new MemoryStream();
                                    if (result != null) binary.Serialize(stream, result);
                                    byte[] buffer = stream.ToArray();
                                    if (_senderUdpClient != null && _senderUdpClient.Client.Connected)
                                    {
                                        _senderUdpClient.Send(buffer, buffer.Length);
                                    }
                                    else
                                    {
                                        _eqItem.EQMessage.Text = "未连接客户端!";
                                        _eqItem.EQMessage.ForeColor = Color.Red;
                                    }
                                    stream.Close();
                                }
                                break;
                            #endregion
                        }
                    }
                    else
                    {
                        _remoteIp = null;
                        if (_senderUdpClient != null)
                        {
                            _senderUdpClient.Client.Close();
                            _senderUdpClient = null;
                        }
                        ConnectState = true;
                        tcpClient.Close();
                        _eqItem.btnEQ.BackColor = Color.Orange;
                        _eqItem.EQMessage.Text = @"Listening...";
                        _eqItem.停止服务ToolStripMenuItem.Enabled = true;
                        _eqItem.启动服务ToolStripMenuItem.Enabled = false;
                        _eqItem.EQMessage.Tag = "";
                        _eqItem.EQMessage.ForeColor = Color.Blue;

                    }

                }
                catch (Exception e)
                {
                    _eqItem.EQMessage.Text = e.Message;
                    _eqItem.EQMessage.Tag = e.Message;
                    _eqItem.EQMessage.ForeColor = Color.Red;
                }
            }


        }


    }
}
