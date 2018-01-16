namespace kMapGIS
{
    partial class SymbolSelection
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SymbolSelection));
            this.ribbonPage2 = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.MainSymbologyControl = new ESRI.ArcGIS.Controls.AxSymbologyControl();
            this.ribbonControl1 = new DevExpress.XtraBars.Ribbon.RibbonControl();
            ((System.ComponentModel.ISupportInitialize)(this.MainSymbologyControl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl1)).BeginInit();
            this.SuspendLayout();
            // 
            // ribbonPage2
            // 
            this.ribbonPage2.Name = "ribbonPage2";
            this.ribbonPage2.Text = "ribbonPage2";
            // 
            // MainSymbologyControl
            // 
            this.MainSymbologyControl.Location = new System.Drawing.Point(12, 56);
            this.MainSymbologyControl.Name = "MainSymbologyControl";
            this.MainSymbologyControl.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("MainSymbologyControl.OcxState")));
            this.MainSymbologyControl.Size = new System.Drawing.Size(620, 360);
            this.MainSymbologyControl.TabIndex = 2;
            this.MainSymbologyControl.OnDoubleClick += new ESRI.ArcGIS.Controls.ISymbologyControlEvents_Ax_OnDoubleClickEventHandler(this.MainSymbologyControl_OnDoubleClick);
            this.MainSymbologyControl.OnItemSelected += new ESRI.ArcGIS.Controls.ISymbologyControlEvents_Ax_OnItemSelectedEventHandler(this.MainSymbologyControl_OnItemSelected);
            // 
            // ribbonControl1
            // 
            this.ribbonControl1.ExpandCollapseItem.Id = 0;
            this.ribbonControl1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.ribbonControl1.ExpandCollapseItem});
            this.ribbonControl1.Location = new System.Drawing.Point(0, 0);
            this.ribbonControl1.MaxItemId = 1;
            this.ribbonControl1.Name = "ribbonControl1";
            this.ribbonControl1.Size = new System.Drawing.Size(644, 50);
            // 
            // SymbolSelection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(644, 428);
            this.Controls.Add(this.MainSymbologyControl);
            this.Controls.Add(this.ribbonControl1);
            this.Name = "SymbolSelection";
            this.Ribbon = this.ribbonControl1;
            this.Text = "SymbolSelection";
            ((System.ComponentModel.ISupportInitialize)(this.MainSymbologyControl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.Ribbon.RibbonPage ribbonPage2;
        private ESRI.ArcGIS.Controls.AxSymbologyControl MainSymbologyControl;
        private DevExpress.XtraBars.Ribbon.RibbonControl ribbonControl1;
    }
}