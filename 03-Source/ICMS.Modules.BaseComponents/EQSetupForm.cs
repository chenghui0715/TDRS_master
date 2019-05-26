using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ICMS.Modules.BaseComponents.Beans;

namespace ICMS.Modules.BaseComponents
{
    public partial class EQSetupForm: Form
    {
        public delegate void ChangeStationEventHandler(SettingEntity entity);
        public event EQSetupForm.ChangeStationEventHandler ChangeEvent;
        public EQSetupForm()
        {
            InitializeComponent();
        }

        public virtual ModuleEntity ModuleInfo { set; get; }
        private void OnSave(object sender, EventArgs e)
        {
            SettingEntity entity = new SettingEntity();
            entity.StationName = txtStationName.Text;
            //entity.Address = txtAddress.Text;
            entity.PortNo = txtPortNo.Text;
            if (ChangeEvent != null)
            {
                this.ChangeEvent(entity);
            }
        }

        private void OnCancel(object sender, EventArgs e)
        {
            this.Close();
        }

        private void EQSetupForm_Load(object sender, EventArgs e)
        {
            if (ModuleInfo != null)
            {
                txtStationName.Text = ModuleInfo.StationName;
                txtPortNo.Text = ModuleInfo.PortNumber;
                txtModuleFile.Text = ModuleInfo.AssemblyName;
                txtModuleNameSpace.Text = ModuleInfo.ModuleNameSpace;
                txtClassName.Text = ModuleInfo.ClassName;
            }
        }
    }
}
