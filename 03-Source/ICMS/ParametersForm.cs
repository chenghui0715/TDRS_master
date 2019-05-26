using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YH.ICMS.BLL;
using YH.ICMS.Common;
using YH.ICMS.Entity;

namespace ICMS
{
    public partial class ParametersForm : DevExpress.XtraEditors.XtraForm
    {
        public string sqlConnect = "";
        public ParametersForm()
        {
            InitializeComponent();
        }

        private void ParametersForm_Load(object sender, EventArgs e)
        {
            sqlConnect =MainForm.m_SqlConnect;
            RefreshCameraUI();
            RefreshStationUI();
            RefreshVoltageUI();
        }

        private void RefreshVoltageUI()
        {
            DataTable dt = new DataTable();
            VoltageBLL voltageBLL = new VoltageBLL();
            DataSet ds = voltageBLL.GetVoltageList("");
            dt = ds.Tables[0];
            dataGridView3.DataSource = null;
            dataGridView3.Rows.Clear();
            dataGridView3.DataSource = dt;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            CamereBLL camereBLL = new CamereBLL();
            VM_CameraParameters vm_cp = new VM_CameraParameters();
            vm_cp.camera = textEdit1.Text.ToString();
            vm_cp.exposureMax = textEditExposureMax.Text.ToString();
            vm_cp.exposureMin = textEditExposureMin.Text.ToString();
            vm_cp.exposureMode = textEditExposureMode.Text.ToString();
            vm_cp.exposureValue = textEditExposureValue.Text.ToString();
            if (camereBLL.InsertCameraInfo(vm_cp))
            {
                RefreshCameraUI();
            }
            else
            {
                MessageBox.Show("插入数据失败！");
            }
            
        }

        public void RefreshCameraUI()
        {
            DataTable dt = new DataTable();
            CamereBLL camereBLL = new CamereBLL();
            DataSet ds= camereBLL.GetCameraList("");
            dt = ds.Tables[0];
            dataGridView1.DataSource = null;
            dataGridView1.Rows.Clear();
            dataGridView1.DataSource = dt;
        }

        public void RefreshStationUI()
        {
            DataTable dt = new DataTable();
            CamereBLL camereBLL = new CamereBLL();
            DataSet ds = camereBLL.GetStationInfoByDataSet();
            dt = ds.Tables[0];
            dataGridView2.DataSource = null;
            dataGridView2.Rows.Clear();
            dataGridView2.DataSource = dt;
        }


        private void btnUpdate_Click(object sender, EventArgs e)
        {

            VM_CameraParameters vm_cp = new VM_CameraParameters();
            CamereBLL camereBLL = new CamereBLL();
            int nId = GetSelectID();
            if (nId==0)
            {
                MessageBoxHelper.ShowInformationMessageBox("数据为空，不支持更新！");
                return;
            }
            vm_cp.ID = nId;
            vm_cp.camera = textEdit1.Text.ToString();
            vm_cp.exposureMax = textEditExposureMax.Text.ToString();
            vm_cp.exposureMin = textEditExposureMin.Text.ToString();
            vm_cp.exposureMode = textEditExposureMode.Text.ToString();
            vm_cp.exposureValue = textEditExposureValue.Text.ToString();
            camereBLL.UpdateCameraInfo(vm_cp);
            RefreshCameraUI();
        }

        private int GetSelectID()
        {
            if (dataGridView1.CurrentRow == null)
                return 0;
            CamereBLL camereBLL = new CamereBLL();
            DataGridViewRow dgvr = dataGridView1.CurrentRow;
            int ID = int.Parse(dgvr.Cells["ID"].Value.ToString());
            return ID;
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if(dataGridView1.CurrentRow == null)
                return;
            DataGridViewRow dgvr = dataGridView1.CurrentRow;
            textEdit1.Text = dgvr.Cells["camera"].Value.ToString();
            textEditExposureMax.Text = dgvr.Cells["exposureMax"].Value.ToString();
            textEditExposureMin.Text  = dgvr.Cells["exposureMin"].Value.ToString();
            textEditExposureMode.Text = dgvr.Cells["exposureMode"].Value.ToString();
            textEditExposureValue.Text = dgvr.Cells["exposureValue"].Value.ToString();

        }

        private void btndelete_Click(object sender, EventArgs e)
        {

            if (dataGridView1.CurrentRow == null)
                return;
            CamereBLL camereBLL = new CamereBLL();
            DataGridViewRow dgvr = dataGridView1.CurrentRow;
            int ID = int.Parse(dgvr.Cells["ID"].Value.ToString());
            camereBLL.DeleteCameraById(ID);
            RefreshCameraUI();
        }

        private void btnInsertStation_Click(object sender, EventArgs e)
        {
            CamereBLL camereBLL = new CamereBLL();
            if(camereBLL.GetStationInfo())
            {
                MessageBox.Show("已有站点信息，请先清除再创建！");
                return;
            }
            VM_Content vm_content = new VM_Content();
            vm_content.startTag = textEditStartTag.Text.ToString();
            vm_content.isMulti = textEditIsMulti.Text.ToString();
            vm_content.illumination = textEditIllumination.Text.ToString();
            vm_content.station = textEditStation.Text.ToString();
           
            if (camereBLL.InsertstationInfo(vm_content))
            {
                RefreshStationUI();
            }
            else
            {
                MessageBox.Show("插入数据失败！");
            }
        }

        private void btnStationDelete_Click(object sender, EventArgs e)
        {
            CamereBLL camereBLL = new CamereBLL();
            camereBLL.DeleteStationInfo();
            RefreshStationUI();
        }



        private void btnAddVoltage_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCurVoltage.Text) || string.IsNullOrWhiteSpace(txtPreVoltage.Text) || string.IsNullOrWhiteSpace(txtVoltageLevel.Text))
            {
                MessageBoxHelper.ShowInformationMessageBox("存在数值为空，请填全！");
                return;
            }
            if(decimal.Parse(txtCurVoltage.Text.ToString()) < decimal.Parse( txtPreVoltage.Text))
            {
                MessageBoxHelper.ShowInformationMessageBox("前值必须小于后值，请修改！");
                return;
            }
            if(txtVoltageLevel.Text =="0")
            {
                MessageBoxHelper.ShowInformationMessageBox("电压等级不能为0，请修改！");
                return;
            }
            VoltageBLL voltageBLL = new VoltageBLL();
            VM_VoltageParam vm_vp = new VM_VoltageParam();
            vm_vp.CurVoltage =decimal.Parse( txtCurVoltage.Text);
            vm_vp.PreVoltage = decimal.Parse( txtPreVoltage.Text);
            vm_vp.VoltageLevel = int.Parse(txtVoltageLevel.Text);
            voltageBLL.GetVoltageList("");
            if (voltageBLL.InsertVoltageParam(vm_vp))
            {
                RefreshVoltageUI();
            }
        }

        private void btnVoltageDelete_Click(object sender, EventArgs e)
        {
            

            if (dataGridView3.CurrentRow == null)
                return;
            VoltageBLL voltageBLL = new VoltageBLL();
            DataGridViewRow dgvr = dataGridView3.CurrentRow;
            int ID = int.Parse(dgvr.Cells["ID"].Value.ToString());
            voltageBLL.DeleteVoltageParamById(ID);
            RefreshVoltageUI();
        }

        private void txtVoltageLevel_KeyPress(object sender, KeyPressEventArgs e)
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
                 if (e.KeyChar == '\b')
                {
                    e.Handled = false;
                }

                else
                {
                    e.Handled = true;
                }
            }
        }

        private void txtPreVoltage_KeyPress(object sender, KeyPressEventArgs e)
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
                //判断输入的是否是小数点，或中文状态下的句号，或者是退格键
                //如果是小数点，循环判断每个字符是不是小数点，如果存在不能输入，如果不存在允许输入
                //如果是退格键，允许输入——if (e.KeyChar == '\b')
                //如果不是小数点也不是退格键，不允许输入
                if (e.KeyChar == Convert.ToChar("。") || e.KeyChar == Convert.ToChar("."))
                {
                    int i_d = 0;
                    for (int i = 0; i < txtPreVoltage.Text.Length; i++)
                    {
                        if (txtPreVoltage.Text.Substring(i, 1) == ".")
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
                else if(e.KeyChar == '\b')
                {
                    e.Handled = false;
                }

                else
                {
                    e.Handled = true;
                }
            }

           
        }

        private void txtCurVoltage_KeyPress(object sender, KeyPressEventArgs e)
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
                //判断输入的是否是小数点，或中文状态下的句号，或者是退格键
                //如果是小数点，循环判断每个字符是不是小数点，如果存在不能输入，如果不存在允许输入
                //如果是退格键，允许输入——if (e.KeyChar == '\b')
                //如果不是小数点也不是退格键，不允许输入
                if (e.KeyChar == Convert.ToChar("。") || e.KeyChar == Convert.ToChar("."))
                {
                    int i_d = 0;
                    for (int i = 0; i < txtCurVoltage.Text.Length; i++)
                    {
                        if (txtCurVoltage.Text.Substring(i, 1) == ".")
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
                else if(e.KeyChar == '\b')
                {
                    e.Handled = false;
                }

                else
                {
                    e.Handled = true;
                }
            }
        }
    }
}
