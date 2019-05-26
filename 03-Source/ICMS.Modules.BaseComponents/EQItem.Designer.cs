namespace ICMS.Modules.BaseComponents
{
    partial class EQItem
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.EqTitleMessage = new System.Windows.Forms.Label();
            this.EQMessage = new System.Windows.Forms.Label();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.启动服务ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.停止服务ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnEQ = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // EqTitleMessage
            // 
            this.EqTitleMessage.BackColor = System.Drawing.Color.RoyalBlue;
            this.EqTitleMessage.Dock = System.Windows.Forms.DockStyle.Top;
            this.EqTitleMessage.Font = new System.Drawing.Font("Verdana", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EqTitleMessage.ForeColor = System.Drawing.Color.White;
            this.EqTitleMessage.Location = new System.Drawing.Point(0, 0);
            this.EqTitleMessage.Name = "EqTitleMessage";
            this.EqTitleMessage.Size = new System.Drawing.Size(237, 26);
            this.EqTitleMessage.TabIndex = 1;
            this.EqTitleMessage.Text = "Title";
            this.EqTitleMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // EQMessage
            // 
            this.EQMessage.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.EQMessage.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.EQMessage.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EQMessage.ForeColor = System.Drawing.Color.Blue;
            this.EQMessage.Location = new System.Drawing.Point(0, 154);
            this.EQMessage.Name = "EQMessage";
            this.EQMessage.Size = new System.Drawing.Size(237, 23);
            this.EQMessage.TabIndex = 3;
            this.EQMessage.Text = "Message";
            this.EQMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.EQMessage, "EQMessage.Text");
            this.EQMessage.MouseHover += new System.EventHandler(this.EQMessage_MouseHover);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.启动服务ToolStripMenuItem,
            this.停止服务ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(125, 48);
            // 
            // 启动服务ToolStripMenuItem
            // 
            this.启动服务ToolStripMenuItem.Name = "启动服务ToolStripMenuItem";
            this.启动服务ToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.启动服务ToolStripMenuItem.Text = "启动服务";
            this.启动服务ToolStripMenuItem.Click += new System.EventHandler(this.OnStartService);
            // 
            // 停止服务ToolStripMenuItem
            // 
            this.停止服务ToolStripMenuItem.Enabled = false;
            this.停止服务ToolStripMenuItem.Name = "停止服务ToolStripMenuItem";
            this.停止服务ToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.停止服务ToolStripMenuItem.Text = "停止服务";
            this.停止服务ToolStripMenuItem.Click += new System.EventHandler(this.OnStopService);
            // 
            // btnEQ
            // 
            this.btnEQ.BackColor = System.Drawing.Color.Red;
            this.btnEQ.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnEQ.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnEQ.Image = global::ICMS.Modules.BaseComponents.Properties.Resources.eq;
            this.btnEQ.Location = new System.Drawing.Point(0, 26);
            this.btnEQ.Name = "btnEQ";
            this.btnEQ.Size = new System.Drawing.Size(237, 128);
            this.btnEQ.TabIndex = 5;
            this.btnEQ.UseVisualStyleBackColor = false;
            this.btnEQ.Click += new System.EventHandler(this.btnEQ_DoubleClick);
            this.btnEQ.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
            // 
            // toolTip1
            // 
            this.toolTip1.ShowAlways = true;
            // 
            // EQItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnEQ);
            this.Controls.Add(this.EQMessage);
            this.Controls.Add(this.EqTitleMessage);
            this.Name = "EQItem";
            this.Size = new System.Drawing.Size(237, 177);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Label EqTitleMessage;
        public System.Windows.Forms.Label EQMessage;
        public System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        public System.Windows.Forms.ToolStripMenuItem 启动服务ToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem 停止服务ToolStripMenuItem;
        public System.Windows.Forms.Button btnEQ;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
