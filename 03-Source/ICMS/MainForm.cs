using DevExpress.XtraBars;
using DevExpress.XtraBars.Helpers;
using DevExpress.XtraSplashScreen;
using HslCommunication.ModBus;
using ICMS.Commons;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using YH.ICMS.Common;
using YH.ICMS.Common.Enumeration;
using YH.ICMS.Entity;
using YH.TRDS.Equipment;
using YH.TRDS.Schedule;

namespace ICMS
{
    public partial class MainForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
      
        private bool EffectTimeflag = false;
        
        public static string m_SqlConnect = "";
        public string m_Message { get; set; }
        public VM_MSConfig m_vmMSConfig { get; set; }

        public ScheduleMode m_ScheduleMode { get; set; }

        #region ------初始化化数据-------
        /// <summary>
        /// 获取主机地址
        /// </summary>
        private void GetHostIPAddress()
        {
            //IPAddress[] ip = Dns.GetHostAddresses(Dns.GetHostName());
            //foreach (IPAddress ipAddress in ip)
            //{
            //    if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
            //    {
            //        if (m_vmMSConfig == null)
            //        {
            //            m_vmMSConfig = LoadConfig();
            //        }
            //        //m_vmMSConfig.IP = ipAddress.ToString();
            //        //XMLSave(m_vmMSConfig);
            //        btnMessage.Caption = @"服务器地址:" + ipAddress + " 端口号:"+m_vmMSConfig.Port;

            //    }
            //}
            if (m_vmMSConfig == null)
            {
                m_vmMSConfig = LoadConfig();
            }
            btnMessage.Caption = @"服务器地址:" + m_vmMSConfig.IP + " 端口号:" + m_vmMSConfig.Port;

        }

        /// <summary>
        /// 打开TDRS服务
        /// </summary>
        private void OpenTDRS()
        {
            //ScheduleMode m_ScheduleMode = new ScheduleMode();
            if(m_ScheduleMode==null)
                m_ScheduleMode = new ScheduleMode();
            m_ScheduleMode.OnMessageStatusChanges += new ScheduleMode.MessageStatusChanges(SetMessageInfo);
            m_ScheduleMode.OnMsCountChanges += new ScheduleMode.MsCountChanges(PrintCountToUI);
            m_ScheduleMode.Start(m_vmMSConfig);

            //需要在开启模块之后开启
            LoadSetParaTime();
        }

        private bool PrintCountToUI(string nCount, string nPos)
        {
            if (nPos == "A")
            {
                label3.Text = nCount;
            }
            else if(nPos == "B")
            {
                label4.Text = nCount;
            }else if(nPos == "C")
            {
                label5.Text = nCount;
            }else if (nPos == "D")
            {
                label6.Text = nCount;
            }
            return true;
        }
        #endregion
        public MainForm()
        {
            InitializeComponent();
            SkinHelper.InitSkinGallery(skins, true);
            m_SqlConnect = ConfigHelper.ReadConfig("DemoKey");
            GetHostIPAddress();
           
        }

        private bool SetMessageInfo(string message)
        {
            m_Message = message;
            return true;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SplashScreenManager.CloseForm(false);
        }



        private void MainForm_Load(object sender, EventArgs e)
        {
            
            LoadCheckBox();
            m_vmMSConfig = LoadConfig();
            if (m_vmMSConfig == null)
                LogHelper.WriteErrorLog("MSConfig配置文件加载失败！");
            #region--------加载combox---------
            LoadComboxInfo();
            #endregion
            Control.CheckForIllegalCrossThreadCalls = false;
            //#region------注册开机自启动------
            //if (m_vmMSConfig.AutoStart == "true")
            //{
            //    ////设置开机自启动 
            //    //MessageBox.Show("设置开机自启动，需要修改注册表", "提示");
            //    string path = Application.ExecutablePath;
            //    RegistryKey rk = Registry.LocalMachine;
            //    RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            //    rk2.SetValue("ICMS", path);
            //    rk2.Close();
            //    rk.Close();
            //}
            //else if (m_vmMSConfig.AutoStart == "false")
            //{
            //    //MessageBox.Show("取消开机自启动，需要修改注册表", "提示");

            //    string path = Application.ExecutablePath;
            //    RegistryKey rk = Registry.LocalMachine;
            //    RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            //    rk2.DeleteValue("ICMS.exe", false);
            //    rk2.Close();
            //    rk.Close();
            //}
            //#endregion

            InitScrollShow();
            timer1.Start();

            #region-------开启TDRS主程序--------
            OpenTDRS();
            #endregion

           


        }

        private void LoadSetParaTime()
        {
            m_ScheduleMode.SetParaDateTime = DateTime.Now;
            m_ScheduleMode.SetParaTimeout = int.Parse(this.txtEffectiveTime.Text.ToString()) * 60;
            EffectTimeflag = true;
        }

        private void LoadCheckBox()
        {
            if (m_vmMSConfig == null)
                return;
            checkEditRebootStart.Checked = m_vmMSConfig.AutoStart.ToString() == "true" ? true:false;
            
            if (checkEditRebootStart.Checked)
            {
                checkEditRebootStart.Text = "允许开机自启动";
            }
            else
            {
                checkEditRebootStart.Text = "禁止开机自启动";
            }
        }

        private void LoadComboxInfo()
        {
            //加载Combox下所有的枚举类型
            foreach (var v in typeof(Direction).GetFields())
            {
                if (v.FieldType.IsEnum == true)
                {
                    this.comboBox1.Items.Add(v.Name);
                }
            }

            //默认加载出入站名称
            for (int i = 0; i < this.comboBox1.Items.Count; i++)
            {
                if (m_vmMSConfig.TrainDirection== this.comboBox1.Items[i].ToString())
                    this.comboBox1.SelectedIndex = i;
            }

        }

        private void OnExitSystem(object sender, ItemClickEventArgs e)
        {
            Application.Exit();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool result = MessageHelper.ConfirmYesNo("确定退出系统吗?");
            if (result)
            {
                //_listenThread = new Thread(StopServices);
                //_listenThread.Start(true);
                StopServices(true);
                GC.Collect();
                Thread.Sleep(1000);
                System.Diagnostics.Process.GetCurrentProcess().Kill();
                //关闭所有网络监听
            }
            else
            {
                e.Cancel = true;
            }
        }
        private void OnStartAll(object sender, ItemClickEventArgs e)
        {

        }

        private Thread _listenThread;


        private void OnCheckedChange(object sender, ItemClickEventArgs e)
        {
            if (barCheckItem1.Checked)
            {
                this.barCheckItem1.LargeGlyph = new System.Drawing.Bitmap(System.Windows.Forms.Application.StartupPath + @"\Images\Play2.ico");
                barCheckItem1.Caption = @"全部启动";
                _listenThread = new Thread(StopServices);
                _listenThread.Start(false);
            }
            else
            {
                this.barCheckItem1.LargeGlyph = new System.Drawing.Bitmap(System.Windows.Forms.Application.StartupPath + @"\Images\stop.ico");
                barCheckItem1.Caption = @"全部停止";
                _listenThread = new Thread(StartServices);
                _listenThread.Start();
            }
        }

        private void StartServices()
        {

        }

        private void StopServices(object closeForm)
        {

        }

        private void OnSettingForm(object sender, ItemClickEventArgs e)
        {
            new SetupForm().ShowDialog();

        }

        private void barButtonItemSetting_ItemClick(object sender, ItemClickEventArgs e)
        {
            new ParametersForm().ShowDialog();
        }



        #region ------事件触发--------
        /// <summary>
        /// 设备监控界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItemMonitor_ItemClick(object sender, ItemClickEventArgs e)
        {
            xtraTabControl1.TabPages.TabControl.TabIndex = 1;
        }
        /// <summary>
        /// 调试界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItemDebug_ItemClick(object sender, ItemClickEventArgs e)
        {
            xtraTabControl1.SelectedTabPageIndex = 2;
        }

        private void xtraTabControl1_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)

        {

            //通过不同的TabPage名称，加载对应方法

            if (e.Page.Name == "xtraTabPage1")

            {
                xtraTabControl1.SelectedTabPageIndex = 1;
            }

            else if (e.Page.Name == "xtraTabPage2")

            {
                xtraTabControl1.SelectedTabPageIndex = 2;

            }

        }

       





        #endregion

        #region
        public static VM_MSConfig LoadConfig()
        {
            String sConfigFile = ConfigHelper.GetConfigFile();
            Type type = typeof(VM_MSConfig);
            FileStream fs = null;
            try
            {
                fs = new FileStream(sConfigFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                XmlSerializer serializer = new XmlSerializer(type);
                VM_MSConfig cfg = (VM_MSConfig)serializer.Deserialize(fs);
                if (cfg == null)
                    return null;
                return cfg;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
        }

        public static void XMLSave(VM_MSConfig msConfig)
        {
            String sConfigFile =ConfigHelper.GetConfigFile();
            msConfig.Save(sConfigFile);
        }
        /// <summary>
        /// 生成XML
        /// </summary>
        public void MakeXml()
        {

            VM_MSConfig vm = new VM_MSConfig();
            vm.AutoStart = "true";
            //vm.AutoStop = "false";
            vm.IP = "127.0.0.1";
            vm.Port = "8888";
            vm.SqlConnect = "Data Source = 127.0.0.1; Initial Catalog = TRDSDB; Persist Security Info=True;User ID = sa; Password=nexadia";
            vm.TrainDirection = "入库方向";
            //vm.DDFlag =;
            //vm.ComIndex =;
            //vm.Com = "COM3";
            //vm.BaudRate = 9600;
            //vm.DataBits = 8;
            //vm.StopBits = "无";
            XMLSave(vm);
        }
        #endregion



        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(comboBox1.SelectedItem.ToString()))
                MessageBoxHelper.ShowInformationMessageBox("出入库模式设置失败！");
            m_vmMSConfig.TrainDirection = comboBox1.SelectedItem.ToString();
            XMLSave(m_vmMSConfig);
            m_Message = "列车当前运行方向为：" + m_vmMSConfig.TrainDirection;
            // ConfigHelper.WriteConfig("Direction", comboBox1.SelectedItem.ToString());
          //  MessageBoxHelper.ShowInformationMessageBox("您已将列车模式设置为：" + comboBox1.SelectedItem.ToString() + "！");
        }

        private void checkEditRebootStart_CheckedChanged(object sender, EventArgs e)
        {
            if (checkEditRebootStart.Checked)
            {
                checkEditRebootStart.Text = "允许开机自启动";
                if (m_vmMSConfig == null)
                    return;
                m_vmMSConfig.AutoStart = "true";
                m_Message = "允许开机自启动";

            }
            else
            {
                checkEditRebootStart.Text = "禁止开机自启动";
                if (m_vmMSConfig == null)
                    return;
                m_vmMSConfig.AutoStart = "false";
                m_Message = "禁止开机自启动";
            }

            XMLSave(m_vmMSConfig);
        }



        private int mPostionX, mPostionY;
        private int mHeight, mWidth;
        private double mTime = 0;
        //private Label mLabel = new Label();

        // 初始化label显示
        private void InitScrollShow()
        {
            mHeight = panel1.Height; ;
            mWidth = panel1.Width;
            label2.Font = new Font("宋体", 50);
            mHeight -= label2.Font.Height;  //label显示需要减去本身的高度
            mPostionX = mWidth;
            mPostionY = mHeight;
            label2.Location = new Point(mPostionX, mPostionY);
            label2.BackColor = Color.OrangeRed;
            label2.Text = m_Message;
            label2.AutoSize = true;
            panel1.Controls.Add(label2);
            label2.Visible = true;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            MSOutController ms = new MSOutController(0,m_vmMSConfig);
            ms.WriteCloseDo0();
            LogHelper.WriteInfoLog("手动关机IDO7");
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {

        }

        private void btnSetEffectTime_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.txtEffectiveTime.Text))
            {
                MessageBoxHelper.ShowInformationMessageBox("不允许时间设置为空,请重新设置时间！");
                return;
            }
            else
            {
               
                m_ScheduleMode.SetParaDateTime = DateTime.Now;
                m_ScheduleMode.SetParaTimeout = int.Parse(this.txtEffectiveTime.Text.ToString()) * 60;
                EffectTimeflag = true;
                MessageBoxHelper.ShowInformationMessageBox("设置时间间隔为："+txtEffectiveTime.Text+"分钟");
            }
        }

        private void txtEffectiveTime_KeyPress(object sender, KeyPressEventArgs e)
        {
            //第一步：判断输入的是否是数字——char.IsNumber(e.KeyChar)
            //如果是数字，可以输入（e.Handled = false;）
            //如果不是数字，则判断是否是小数点
           if (char.IsNumber(e.KeyChar))
            {
                e.Handled = false;
            }
            else
            {
                //判断输入的是否是小数点，或中文状态下的句号，或者是退格键
                //如果是小数点，循环判断每个字符是不是小数点，如果存在不能输入，如果不存在允许输入
                //如果是退格键，允许输入——if (e.KeyChar == '\b')
                //如果不是小数点也不是退格键，不允许输入
                if (e.KeyChar == Convert.ToChar("。") || e.KeyChar == Convert.ToChar("."))
                {
                    int i_d = 0;
                    for (int i = 0; i < txtEffectiveTime.Text.Length; i++)
                    {
                        if (txtEffectiveTime.Text.Substring(i, 1) == ".")
                        {
                            e.Handled = true;
                            i_d++;
                            return;
                        }
                    }
                    if (i_d == 0)
                    {
                        e.KeyChar = Convert.ToChar(".");//设置按键输入的值为"."
                        e.Handled = false;
                    }
                }
                else if (e.KeyChar == '\b')
                {
                    e.Handled = false;
                }

                else
                {
                    e.Handled = true;
                }
            }
        }

        // 设置底栏从右向左滚动显示
        private void ScrollShow()
        {
            mPostionX = mPostionX - 3;
            label2.Location = new Point(mPostionX, mPostionY);
            if (mPostionX <= -label2.Size.Width)
            {
                mPostionX = mWidth;
            }
            label2.Text = m_Message;
            label2.Visible = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            mTime += 0.01;
            ScrollShow();

        }
    }
}