using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ICMS.Modules.BaseComponents.Commons;

namespace ICMS.Modules.BaseComponents
{
    public partial class EQItem : UserControl
    {
        public readonly TcpController TcpController;

        public EQItem()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            TcpController = new TcpController(this);
            this.toolTip1.SetToolTip(this.EQMessage, EQMessage.Text);
        }
        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(MousePosition.X, MousePosition.Y);
            }
        }

        private void btnEQ_DoubleClick(object sender, EventArgs e)
        {
            EQSetupForm form = new EQSetupForm();
            form.ModuleInfo = TcpController.ModuleInfo;
            form.ShowDialog();
        }


        private void OnStartService(object sender, EventArgs e)
        {
            try
            {
                if (!TcpController.ConnectState)
                {
                    TcpController.StartService();

                }
                else
                {
                    TcpController.CloseService();
                }
            }
            catch (Exception s)
            {
                TcpController.ConnectState = false;
                MessageBox.Show(s.Message);
            }

        }

        private void OnStopService(object sender, EventArgs e)
        {
            TcpController.CloseService();
            TcpController.ConnectState = false;
        }


        private void EQMessage_MouseHover(object sender, EventArgs e)
        {
            this.toolTip1.SetToolTip(this.EQMessage, EQMessage.Text);}  
    }
}