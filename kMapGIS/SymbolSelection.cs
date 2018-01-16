using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using System.IO;

namespace kMapGIS
{
    public partial class SymbolSelection : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        public IStyleGalleryItem selectGalleryItem;
        public SymbolSelection()
        {
            InitializeComponent();

        }
        public void LoadSymbol(int index)
        {
            string directoryPath = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
            string styleFilePath = System.IO.Path.Combine(directoryPath, "ESRI.ServerStyle");
            MainSymbologyControl.LoadStyleFile(styleFilePath);
            switch (index)
            {
                case 1:
                    MainSymbologyControl.StyleClass = esriSymbologyStyleClass.esriStyleClassNorthArrows;
                    break;
                case 2:
                    MainSymbologyControl.StyleClass = esriSymbologyStyleClass.esriStyleClassScaleBars;
                    break;
            }
        }

        private void MainSymbologyControl_OnItemSelected(object sender, ISymbologyControlEvents_OnItemSelectedEvent e)
        {
            selectGalleryItem = (IStyleGalleryItem)e.styleGalleryItem;
        }

        private void MainSymbologyControl_OnDoubleClick(object sender, ISymbologyControlEvents_OnDoubleClickEvent e)
        {
            this.Close();
        }
    }
}