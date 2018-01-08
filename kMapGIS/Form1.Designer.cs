namespace kMapGIS
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.axLicenseControl1 = new ESRI.ArcGIS.Controls.AxLicenseControl();
            this.MainMapControl = new ESRI.ArcGIS.Controls.AxMapControl();
            this.MainPageLayoutControl = new ESRI.ArcGIS.Controls.AxPageLayoutControl();
            this.mainTab = new System.Windows.Forms.TabControl();
            this.tabMap = new System.Windows.Forms.TabPage();
            this.tabPageLayout = new System.Windows.Forms.TabPage();
            this.MainTocControl = new ESRI.ArcGIS.Controls.AxTOCControl();
            this.MainToolbarControl = new ESRI.ArcGIS.Controls.AxToolbarControl();
            this.functionTab = new System.Windows.Forms.TabControl();
            this.tabSelectQuery = new System.Windows.Forms.TabPage();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            ((System.ComponentModel.ISupportInitialize)(this.axLicenseControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MainMapControl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MainPageLayoutControl)).BeginInit();
            this.mainTab.SuspendLayout();
            this.tabMap.SuspendLayout();
            this.tabPageLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MainTocControl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MainToolbarControl)).BeginInit();
            this.functionTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // axLicenseControl1
            // 
            this.axLicenseControl1.Enabled = true;
            this.axLicenseControl1.Location = new System.Drawing.Point(12, 12);
            this.axLicenseControl1.Name = "axLicenseControl1";
            this.axLicenseControl1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axLicenseControl1.OcxState")));
            this.axLicenseControl1.Size = new System.Drawing.Size(32, 32);
            this.axLicenseControl1.TabIndex = 0;
            // 
            // MainMapControl
            // 
            this.MainMapControl.Location = new System.Drawing.Point(6, 6);
            this.MainMapControl.Name = "MainMapControl";
            this.MainMapControl.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("MainMapControl.OcxState")));
            this.MainMapControl.Size = new System.Drawing.Size(613, 475);
            this.MainMapControl.TabIndex = 1;
            // 
            // MainPageLayoutControl
            // 
            this.MainPageLayoutControl.Location = new System.Drawing.Point(6, 6);
            this.MainPageLayoutControl.Name = "MainPageLayoutControl";
            this.MainPageLayoutControl.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("MainPageLayoutControl.OcxState")));
            this.MainPageLayoutControl.Size = new System.Drawing.Size(613, 475);
            this.MainPageLayoutControl.TabIndex = 2;
            // 
            // mainTab
            // 
            this.mainTab.Controls.Add(this.tabMap);
            this.mainTab.Controls.Add(this.tabPageLayout);
            this.mainTab.Location = new System.Drawing.Point(283, 12);
            this.mainTab.Name = "mainTab";
            this.mainTab.SelectedIndex = 0;
            this.mainTab.Size = new System.Drawing.Size(633, 513);
            this.mainTab.TabIndex = 3;
            this.mainTab.MouseClick += new System.Windows.Forms.MouseEventHandler(this.mainTab_MouseClick);
            // 
            // tabMap
            // 
            this.tabMap.Controls.Add(this.MainMapControl);
            this.tabMap.Location = new System.Drawing.Point(4, 22);
            this.tabMap.Name = "tabMap";
            this.tabMap.Padding = new System.Windows.Forms.Padding(3);
            this.tabMap.Size = new System.Drawing.Size(625, 487);
            this.tabMap.TabIndex = 0;
            this.tabMap.Text = "地图视图";
            this.tabMap.UseVisualStyleBackColor = true;
            // 
            // tabPageLayout
            // 
            this.tabPageLayout.Controls.Add(this.MainPageLayoutControl);
            this.tabPageLayout.Location = new System.Drawing.Point(4, 22);
            this.tabPageLayout.Name = "tabPageLayout";
            this.tabPageLayout.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageLayout.Size = new System.Drawing.Size(625, 487);
            this.tabPageLayout.TabIndex = 1;
            this.tabPageLayout.Text = "布局视图";
            this.tabPageLayout.UseVisualStyleBackColor = true;
            // 
            // MainTocControl
            // 
            this.MainTocControl.Location = new System.Drawing.Point(12, 50);
            this.MainTocControl.Name = "MainTocControl";
            this.MainTocControl.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("MainTocControl.OcxState")));
            this.MainTocControl.Size = new System.Drawing.Size(265, 475);
            this.MainTocControl.TabIndex = 4;
            // 
            // MainToolbarControl
            // 
            this.MainToolbarControl.Location = new System.Drawing.Point(12, 12);
            this.MainToolbarControl.Name = "MainToolbarControl";
            this.MainToolbarControl.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("MainToolbarControl.OcxState")));
            this.MainToolbarControl.Size = new System.Drawing.Size(265, 28);
            this.MainToolbarControl.TabIndex = 5;
            // 
            // functionTab
            // 
            this.functionTab.Controls.Add(this.tabSelectQuery);
            this.functionTab.Controls.Add(this.tabPage4);
            this.functionTab.Location = new System.Drawing.Point(918, 12);
            this.functionTab.Name = "functionTab";
            this.functionTab.SelectedIndex = 0;
            this.functionTab.Size = new System.Drawing.Size(320, 509);
            this.functionTab.TabIndex = 6;
            // 
            // tabSelectQuery
            // 
            this.tabSelectQuery.Location = new System.Drawing.Point(4, 22);
            this.tabSelectQuery.Name = "tabSelectQuery";
            this.tabSelectQuery.Padding = new System.Windows.Forms.Padding(3);
            this.tabSelectQuery.Size = new System.Drawing.Size(312, 483);
            this.tabSelectQuery.TabIndex = 0;
            this.tabSelectQuery.Text = "选择&查询";
            this.tabSelectQuery.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(312, 483);
            this.tabPage4.TabIndex = 1;
            this.tabPage4.Text = "tabPage4";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1250, 537);
            this.Controls.Add(this.functionTab);
            this.Controls.Add(this.MainToolbarControl);
            this.Controls.Add(this.MainTocControl);
            this.Controls.Add(this.mainTab);
            this.Controls.Add(this.axLicenseControl1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.axLicenseControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MainMapControl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MainPageLayoutControl)).EndInit();
            this.mainTab.ResumeLayout(false);
            this.tabMap.ResumeLayout(false);
            this.tabPageLayout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MainTocControl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MainToolbarControl)).EndInit();
            this.functionTab.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ESRI.ArcGIS.Controls.AxLicenseControl axLicenseControl1;
        private ESRI.ArcGIS.Controls.AxMapControl MainMapControl;
        private ESRI.ArcGIS.Controls.AxPageLayoutControl MainPageLayoutControl;
        private System.Windows.Forms.TabControl mainTab;
        private System.Windows.Forms.TabPage tabMap;
        private System.Windows.Forms.TabPage tabPageLayout;
        private ESRI.ArcGIS.Controls.AxTOCControl MainTocControl;
        private ESRI.ArcGIS.Controls.AxToolbarControl MainToolbarControl;
        private System.Windows.Forms.TabControl functionTab;
        private System.Windows.Forms.TabPage tabSelectQuery;
        private System.Windows.Forms.TabPage tabPage4;
    }
}

