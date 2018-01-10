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
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Controls;

namespace kMapGIS
{
    public partial class MainForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        string chartField = "";

        public MainForm()
        {
            InitializeComponent();
            MapLoad();
        }

        void MapLoad()
        {
            //���ص�ͼ
            string directoryPath = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
            string mxdFilePath = System.IO.Path.Combine(directoryPath, "YuhaoKangCode.mxd");
            MainMapControl.LoadMxFile(mxdFilePath);
            MainMapControl.ActiveView.Refresh();

            //���ͼ��
            IFeatureLayer featureLayer = MainMapControl.Map.get_Layer(3) as IFeatureLayer;
            IFields fields = featureLayer.FeatureClass.Fields;
            for (int i = 0; i < fields.FieldCount; i++)
            {
                repositoryItemComboBoxPropertySelect.Items.Add(fields.get_Field(i).Name);
            }
        }

        private void CopyToPageLayout()
        {
            //������������ͼ
            IObjectCopy objectCopy = new ObjectCopyClass();
            object copyFromMap = MainMapControl.Map;
            object copiedMap = objectCopy.Copy(copyFromMap);
            object copyToMap = MainPageLayoutControl.ActiveView.FocusMap;
            objectCopy.Overwrite(copiedMap, ref copyToMap);
            MainPageLayoutControl.ActiveView.Refresh();
        }

        private void tabPageControl_Click(object sender, EventArgs e)
        {
            //������ͼ
            if (tabPageControl.SelectedTabPage.Name == "tabPageLayout")
            {
                IActiveView activeView = (IActiveView)MainPageLayoutControl.ActiveView.FocusMap;
                IDisplayTransformation displayTransformation = activeView.ScreenDisplay.DisplayTransformation;
                displayTransformation.VisibleBounds = MainMapControl.Extent;
                MainPageLayoutControl.ActiveView.Refresh();
                CopyToPageLayout();
            }
        }

        private void MainMapControl_OnMouseDown(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseDownEvent e)
        {
            //��ͼ����
            if (barCheckPan.Checked)
            {
                MainMapControl.Pan();
            }
            IFeatureLayer featureLayer=null;
            //������ѡ��
            if (barCheckSelectVillage.Checked)
            {
                e = SelectFeatureTrackRec(e,"��������",ref featureLayer);
            }
            //����ѡ��
            if (barCheckSelectLand.Checked)
            {
                e = SelectFeatureTrackRec(e,"��������",ref featureLayer);
            }
        }

        private ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseDownEvent SelectFeatureTrackRec(ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseDownEvent e, string layerName, ref IFeatureLayer featureLayer)
        {
            //���ο�ѡ
            MainMapControl.Map.ClearSelection();
            IFeatureLayer selectFeatureLayer = null;
            for (int i = 0; i < MainMapControl.Map.LayerCount; i++)
            {
                featureLayer = MainMapControl.Map.get_Layer(i) as IFeatureLayer;
                featureLayer.Selectable = false;
                if (featureLayer.Name == layerName)
                {
                    featureLayer.Selectable = true;
                    selectFeatureLayer = featureLayer;
                }
            }
            //���ο�ѡ
            IEnvelope envelope = MainMapControl.TrackRectangle();
            IGeometry geometry = envelope as IGeometry;
            if (geometry.IsEmpty == true)
            {
                tagRECT r;
                r.left = e.x - 1;
                r.top = e.y - 1;
                r.right = e.x + 1;
                r.bottom = e.y - 1;
                MainMapControl.ActiveView.ScreenDisplay.DisplayTransformation.TransformRect(envelope, ref r, 4);
                envelope.SpatialReference = MainMapControl.ActiveView.FocusMap.SpatialReference;
            }
            MainMapControl.Map.SelectByShape(geometry, null, false);
            MainMapControl.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);

            LoadTableInDataGridFromSelect(selectFeatureLayer);
            return e;
        }

        private void LoadTableInDataGridFromSelect(IFeatureLayer featureLayer)
        {
            DataTable dataTable = new DataTable();
            dataTable.TableName = featureLayer.Name;
            //��ȡ�Ѿ�ѡ���Ҫ��
            IFeatureSelection featureSelection = featureLayer as IFeatureSelection;
            IFields fields = featureLayer.FeatureClass.Fields;
            for (int i = 0; i < fields.FieldCount; i++)
            {
                dataTable.Columns.Add(fields.get_Field(i).Name);
            }

            ISelectionSet selectionSet = featureSelection.SelectionSet;
            ICursor cursor;
            selectionSet.Search(null, false, out cursor);
            IFeatureCursor featureCursor = cursor as IFeatureCursor;
            IFeature feature = featureCursor.NextFeature();

            //��ȡҪ�ص���Ϣ
            while (feature != null)
            {
                string[] feature_info = new string[fields.FieldCount];
                for (int i = 0; i < fields.FieldCount; i++)
                {
                    feature_info[i] = feature.get_Value(i).ToString();
                }

                dataTable.Rows.Add(feature_info);
                feature = featureCursor.NextFeature();
            }
            gridView1.Columns.Clear();
            gridControl.DataSource = dataTable;
        }

        private void LoadTableInDataGrid(IFeatureLayer featureLayer)
        {
            gridControl.DataSource = null;
            DataTable dataTable = new DataTable();
            dataTable.TableName = featureLayer.Name;
            //��ȡ�Ѿ�ѡ���Ҫ��
            IFields fields = featureLayer.FeatureClass.Fields;
            for (int i = 0; i < fields.FieldCount; i++)
            {
                dataTable.Columns.Add(fields.get_Field(i).Name);
            }

            IFeatureCursor featureCursor= featureLayer.Search(null, true);
            IFeature feature = featureCursor.NextFeature();

            //��ȡҪ�ص���Ϣ
            while (feature != null)
            {
                string[] feature_info = new string[fields.FieldCount];
                for (int i = 0; i < fields.FieldCount; i++)
                {
                    feature_info[i] = feature.get_Value(i).ToString();
                }

                dataTable.Rows.Add(feature_info);
                feature = featureCursor.NextFeature();
            }
            gridView1.Columns.Clear();
            gridControl.DataSource = dataTable;
        }

        private void barButtonFullExtent_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //ȫͼ��ʾ
            this.MainMapControl.Extent = this.MainMapControl.FullExtent;
        }

        private void barButtonZoomIn_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //�Ŵ�
            IEnvelope envelope = MainMapControl.Extent;
            envelope.Expand(0.5, 0.5, true);
            MainMapControl.Extent = envelope;
            MainMapControl.ActiveView.Refresh();
        }

        private void barButtonZoomOut_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //��С
            IEnvelope envelope = MainMapControl.Extent;
            envelope.Expand(1.5, 1.5, true);
            MainMapControl.Extent = envelope;
            MainMapControl.ActiveView.Refresh();
        }

        private void MainMapControl_OnMouseMove(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseMoveEvent e)
        {
            //״̬��
            int mapScale = Convert.ToInt32(MainMapControl.Map.MapScale);
            esriUnits mapUnit = MainMapControl.Map.MapUnits;

            string statusInformation = string.Format("������Ϣ: X={0}, Y={1} {2} \t ������: 1:{3}", e.x, e.y, mapUnit, mapScale);
            barStaticStatus.Caption = statusInformation;
        }

        private void barButtonSave_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //�����ͼ�ĵ�
            IMapDocument mapDocument = new MapDocumentClass();
            mapDocument.New(MainMapControl.DocumentFilename);
            mapDocument.ReplaceContents(MainMapControl.Map as IMxdContents);
            mapDocument.Save(mapDocument.UsesRelativePaths, true);
            mapDocument.Close();
        }

        private void barButtonClearSelection_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //���ѡ��
            MainMapControl.Map.ClearSelection();
            MainMapControl.ActiveView.Refresh();
        }

        private void MainTOCControl_OnMouseDown(object sender, ESRI.ArcGIS.Controls.ITOCControlEvents_OnMouseDownEvent e)
        {
            if (e.button == 2)
            {
                esriTOCControlItem item = esriTOCControlItem.esriTOCControlItemNone;
                IBasicMap basicMap = null;
                object unk = null;
                object data = null;
                ILayer layer = null;
                MainTOCControl.HitTest(e.x, e.y, ref item, ref basicMap, ref layer, ref unk, ref data);
                popupMenuTOCControl.ShowPopup(Control.MousePosition);
            }
        }

        private void barButtonOpenLayerTable_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            esriTOCControlItem item = esriTOCControlItem.esriTOCControlItemNone;
            IBasicMap basicMap = null;
            object unk = null;
            object data = null;
            ILayer layer = null;
            MainTOCControl.GetSelectedItem(ref item, ref  basicMap, ref layer, ref unk, ref data);
            IFeatureLayer featureLayer = layer as IFeatureLayer;

            LoadTableInDataGrid(featureLayer);
        }

        private void barButtonSelectTable_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            DataTable dt = gridControl.DataSource as DataTable;
            IFeatureLayer selectLayer = null;
            for (int i = 0; i < MainMapControl.Map.LayerCount; i++)
            {
                IFeatureLayer layer = MainMapControl.Map.get_Layer(i) as IFeatureLayer;
                if (dt.TableName == layer.Name)
                {
                    selectLayer = layer;
                }
            }

            int[] selectRows = gridView1.GetSelectedRows();
            IFeatureSelection featureSelection = selectLayer as IFeatureSelection;
            foreach (int index in selectRows)
            {
                DataRowView row = gridView1.GetRow(index) as DataRowView;
                IFeature feature = selectLayer.FeatureClass.GetFeature(Convert.ToInt32(row[0]));
                featureSelection.SelectionSet.Add(Convert.ToInt32(row[0]));
            }
            MainMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, MainMapControl.ActiveView.Extent);
        }

        private void barButtonBarChart_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //��ȡͼ��
            IGeoFeatureLayer geoFeatureLayer = MainMapControl.Map.get_Layer(3) as IGeoFeatureLayer;

            //��Ⱦ����
            IChartRenderer chartRender = new ChartRendererClass();
            IRendererFields renderFields = chartRender as IRendererFields;

            //��ȡ����
            IDataStatistics dataStatistics = null;
            IFeatureCursor featureCursor = null;
            IQueryFilter queryFilter = new QueryFilterClass();
            featureCursor = geoFeatureLayer.Search(queryFilter, true);

            //��ȡ��ǰ����ɫ
            IFeature feature = featureCursor.NextFeature();
            ISymbol preSymbol = geoFeatureLayer.Renderer.get_SymbolByFeature(feature);
            IColor preColor = (preSymbol as IFillSymbol).Color;

            //�ҵ����ֵ
            renderFields.AddField(barEditItemPropertySelect.EditValue.ToString());
            dataStatistics = new DataStatisticsClass();
            dataStatistics.Cursor = featureCursor as ICursor;
            dataStatistics.Field = barEditItemPropertySelect.EditValue.ToString();

            double maxProperty = dataStatistics.Statistics.Maximum;

            //���ű���
            IChartSymbol chartSymbol = null;
            IFillSymbol fillSymbol = null;
            IMarkerSymbol markerSymbol = null;

            //����ͼ��С
            IBarChartSymbol barChartSymbol = new BarChartSymbolClass();
            barChartSymbol.Width = 6;

            chartSymbol = barChartSymbol as IChartSymbol;
            markerSymbol = barChartSymbol as IMarkerSymbol;
            markerSymbol.Size = 30;

            chartSymbol.MaxValue = maxProperty;
            ISymbolArray symbolArray = barChartSymbol as ISymbolArray;

            //����ͼ��ɫ
            IRgbColor rgbColor = new RgbColorClass();
            rgbColor.Red = 0;
            rgbColor.Green = 0;
            rgbColor.Blue = 255;

            fillSymbol = new SimpleFillSymbolClass();
            fillSymbol.Color = rgbColor;

            symbolArray.AddSymbol(fillSymbol as ISymbol);
            chartRender.ChartSymbol = barChartSymbol as IChartSymbol;

            //��ͼ��ɫ
            rgbColor = new RgbColorClass();
            rgbColor.Red = 255;
            rgbColor.Green = 0;
            rgbColor.Blue = 0;
            fillSymbol = new SimpleFillSymbolClass();
            fillSymbol.Color = rgbColor;
            chartRender.BaseSymbol =preSymbol as ISymbol;

            //������Ⱦ
            chartRender.UseOverposter = false;
            chartRender.CreateLegend();
            geoFeatureLayer.Renderer = chartRender as IFeatureRenderer;
            MainMapControl.Refresh();
            MainTOCControl.Update();

        }

        private void barButtonUniqueValue_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            IGeoFeatureLayer geoFeatureLayer = MainMapControl.Map.get_Layer(3) as IGeoFeatureLayer;

            IUniqueValueRenderer uniqueRender = new UniqueValueRendererClass();
            uniqueRender.FieldCount = 1;
            uniqueRender.set_Field(0, barEditItemPropertySelect.EditValue.ToString());

            IQueryFilter queryFilter = new QueryFilterClass();
            IRandomColorRamp randomColorRamp = new RandomColorRampClass();
            randomColorRamp.Size = geoFeatureLayer.FeatureClass.FeatureCount(queryFilter);

            bool success = false;
            randomColorRamp.CreateRamp(out success);
            IEnumColors enumColors = randomColorRamp.Colors;

            IFeatureCursor featureCursor = geoFeatureLayer.FeatureClass.Search(queryFilter, true);

            IFeature feature = featureCursor.NextFeature();
            int fieldIndex = feature.Fields.FindField(barEditItemPropertySelect.EditValue.ToString());
            object codeValue = null;
            while (feature != null)
            {
                codeValue = feature.get_Value(fieldIndex);
                barEditItemPropertySelect.EditValue.ToString();
                feature = featureCursor.NextFeature();

                IFillSymbol fillSymbol=new SimpleFillSymbolClass();
                fillSymbol.Color = enumColors.Next();
                uniqueRender.AddValue(codeValue.ToString(), "", fillSymbol as ISymbol);
            }

            geoFeatureLayer.Renderer = uniqueRender as IFeatureRenderer;
            MainMapControl.Refresh();
            MainTOCControl.Update();
        }

        private void barButtonGraduatedColors_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            IGeoFeatureLayer geoFeatureLayer = MainMapControl.Map.get_Layer(3) as IGeoFeatureLayer;

            IBasicHistogram basicHistogram = new BasicTableHistogramClass();

        }

        private void barButtonPieChart_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //��ȡͼ��
            IGeoFeatureLayer geoFeatureLayer = MainMapControl.Map.get_Layer(3) as IGeoFeatureLayer;

            //��Ⱦ����
            IChartRenderer chartRender = new ChartRendererClass();
            IRendererFields renderFields = chartRender as IRendererFields;

            //��ȡ����
            IDataStatistics dataStatistics = null;
            IFeatureCursor featureCursor = null;
            IQueryFilter queryFilter = new QueryFilterClass();
            featureCursor = geoFeatureLayer.Search(queryFilter, true);

            //��ȡ��ǰ����ɫ
            IFeature feature = featureCursor.NextFeature();
            ISymbol preSymbol = geoFeatureLayer.Renderer.get_SymbolByFeature(feature);
            IColor preColor = (preSymbol as IFillSymbol).Color;

            //�ҵ����ֵ
            renderFields.AddField(barEditItemPropertySelect.EditValue.ToString());
            dataStatistics = new DataStatisticsClass();
            dataStatistics.Cursor = featureCursor as ICursor;
            dataStatistics.Field = barEditItemPropertySelect.EditValue.ToString();

            double maxProperty = dataStatistics.Statistics.Maximum;

            //���ű���
            IChartSymbol chartSymbol = null;
            IFillSymbol fillSymbol = null;
            IMarkerSymbol markerSymbol = null;

            //��ͼ����
            IPieChartSymbol pieChartSymbol = new PieChartSymbolClass();
            pieChartSymbol.Clockwise = true;
            pieChartSymbol.UseOutline = true;
            ILineSymbol lineSymbol = new SimpleLineSymbolClass();

            IRgbColor rgbColor = new RgbColorClass();
            rgbColor.Red = 100;
            rgbColor.Green = 205;
            rgbColor.Blue = 30;
            lineSymbol.Color = rgbColor;
            lineSymbol.Width = 1;
            pieChartSymbol.Outline = lineSymbol;

            chartSymbol = pieChartSymbol as IChartSymbol;
            markerSymbol = pieChartSymbol as IMarkerSymbol;
            markerSymbol.Size = 30;

            chartSymbol.MaxValue = maxProperty;
            ISymbolArray symbolArray = pieChartSymbol as ISymbolArray;

            //����ͼ��ɫ
            IRgbColor rgbColor1 = new RgbColorClass();
            rgbColor1.Red = 0;
            rgbColor1.Green = 0;
            rgbColor1.Blue = 255;

            fillSymbol = new SimpleFillSymbolClass();
            fillSymbol.Color = rgbColor1;

            symbolArray.AddSymbol(fillSymbol as ISymbol);
            chartRender.ChartSymbol = pieChartSymbol as IChartSymbol;

            //��ͼ��ɫ
            chartRender.BaseSymbol = preSymbol as ISymbol;

            //������Ⱦ
            chartRender.UseOverposter = false;
            chartRender.CreateLegend();
            geoFeatureLayer.Renderer = chartRender as IFeatureRenderer;
            MainMapControl.Refresh();
            MainTOCControl.Update();
        }
    }
}