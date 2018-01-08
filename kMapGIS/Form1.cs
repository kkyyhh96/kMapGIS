using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;

namespace kMapGIS
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            MapLoad();
        }

        void MapLoad()
        {
            string directoryPath = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
            string mxdFilePath = Path.Combine(directoryPath, "YuhaoKangCode.mxd");
            MainMapControl.LoadMxFile(mxdFilePath);
            MainMapControl.ActiveView.Refresh();
        }

        private void mainTab_MouseClick(object sender, MouseEventArgs e)
        {
            //如果是布局视图
            if (mainTab.SelectedTab.Name == "tabPageLayout")
            {
                IActiveView activeView = (IActiveView)MainPageLayoutControl.ActiveView.FocusMap;
                IDisplayTransformation displayTransformation = activeView.ScreenDisplay.DisplayTransformation;
                displayTransformation.VisibleBounds = MainMapControl.Extent;
                MainPageLayoutControl.ActiveView.Refresh();
                CopyToPageLayout();
            }
        }

        //拷贝到布局视图
        private void CopyToPageLayout()
        {
            IObjectCopy objectCopy = new ObjectCopyClass();
            object copyFromMap = MainMapControl.Map;
            object copiedMap = objectCopy.Copy(copyFromMap);
            object copyToMap = MainPageLayoutControl.ActiveView.FocusMap;
            objectCopy.Overwrite(copiedMap, ref copyToMap);
            MainPageLayoutControl.ActiveView.Refresh();
        }
    }
}