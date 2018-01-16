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
using stdole;

namespace kMapGIS
{
    public partial class MainForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        string chartField = "";
        SymbolSelection symbolSelection;

        public MainForm()
        {
            InitializeComponent();
            MapLoad();
        }

        void MapLoad()
        {
            //加载地图
            string directoryPath = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
            string mxdFilePath = System.IO.Path.Combine(directoryPath, "YuhaoKangCode.mxd");
            MainMapControl.LoadMxFile(mxdFilePath);
            MainMapControl.ActiveView.Refresh();

            //添加图层
            IFeatureLayer featureLayer = MainMapControl.Map.get_Layer(3) as IFeatureLayer;
            IFields fields = featureLayer.FeatureClass.Fields;
            for (int i = 0; i < fields.FieldCount; i++)
            {
                repositoryItemComboBoxPropertySelect.Items.Add(fields.get_Field(i).Name);
            }

            //添加查询的属性
            repositoryItemComboBoxCountry.Items.Add("全部村庄");
            repositoryItemComboBoxProperty.Items.Add("人口数");
            repositoryItemComboBoxProperty.Items.Add("人均耕地");

            int layerIndex = 2; string fieldName = "权属名称";
            GetValueFromField(layerIndex, fieldName, 1);
            layerIndex = 2; fieldName = "地类名";
            GetValueFromField(layerIndex, fieldName, 2);
        }

        private void GetValueFromField(int layerIndex, string fieldName, int comboxIndex)
        {
            //获取字段下所有的值
            IFeatureLayer featureLayerLand = MainMapControl.Map.get_Layer(layerIndex) as IFeatureLayer;
            IDataset dataSet = (IDataset)featureLayerLand.FeatureClass;
            IQueryDef queryDef = ((IFeatureWorkspace)dataSet.Workspace).CreateQueryDef();
            queryDef.Tables = dataSet.Name;
            queryDef.SubFields = string.Format("DISTINCT ({0})", fieldName);
            ICursor cursor = queryDef.Evaluate();
            IRow row = cursor.NextRow();
            while (row != null)
            {
                switch (comboxIndex)
                {
                    case 1:
                        repositoryItemComboBoxCountry.Items.Add(row.get_Value(0).ToString());
                        break;
                    case 2:
                        repositoryItemComboBoxProperty.Items.Add(row.get_Value(0).ToString());
                        break;
                }
                row = cursor.NextRow();
            }
        }

        private void CopyToPageLayout()
        {
            //拷贝到布局视图
            IObjectCopy objectCopy = new ObjectCopyClass();
            object copyFromMap = MainMapControl.Map;
            object copiedMap = objectCopy.Copy(copyFromMap);
            object copyToMap = MainPageLayoutControl.ActiveView.FocusMap;
            objectCopy.Overwrite(copiedMap, ref copyToMap);
            MainPageLayoutControl.ActiveView.Refresh();
        }

        private void tabPageControl_Click(object sender, EventArgs e)
        {
            //布局视图
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
            //地图漫游
            if (barCheckPan.Checked)
            {
                MainMapControl.Pan();
            }
            IFeatureLayer featureLayer = null;
            //行政村选择
            if (barCheckSelectVillage.Checked)
            {
                e = SelectFeatureTrackRec(e, "行政区面", ref featureLayer);
            }
            //地类选择
            if (barCheckSelectLand.Checked)
            {
                e = SelectFeatureTrackRec(e, "土地利用", ref featureLayer);
            }

        }
        private ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseDownEvent SelectFeatureTrackRec(ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseDownEvent e, string layerName, ref IFeatureLayer featureLayer)
        {
            //矩形框选
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
            //矩形框选
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
            //获取已经选择的要素
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

            //获取要素的信息
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
            //获取已经选择的要素
            IFields fields = featureLayer.FeatureClass.Fields;
            for (int i = 0; i < fields.FieldCount; i++)
            {
                dataTable.Columns.Add(fields.get_Field(i).Name);
            }

            IFeatureCursor featureCursor = featureLayer.Search(null, true);
            IFeature feature = featureCursor.NextFeature();

            //获取要素的信息
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
            //全图显示
            this.MainMapControl.Extent = this.MainMapControl.FullExtent;
        }

        private void barButtonZoomIn_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //放大
            IEnvelope envelope = MainMapControl.Extent;
            envelope.Expand(0.5, 0.5, true);
            MainMapControl.Extent = envelope;
            MainMapControl.ActiveView.Refresh();
        }

        private void barButtonZoomOut_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //缩小
            IEnvelope envelope = MainMapControl.Extent;
            envelope.Expand(1.5, 1.5, true);
            MainMapControl.Extent = envelope;
            MainMapControl.ActiveView.Refresh();
        }

        private void MainMapControl_OnMouseMove(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseMoveEvent e)
        {
            //状态栏
            int mapScale = Convert.ToInt32(MainMapControl.Map.MapScale);
            esriUnits mapUnit = MainMapControl.Map.MapUnits;

            string statusInformation = string.Format("坐标信息: X={0}, Y={1} {2} \t 比例尺: 1:{3}", e.x, e.y, mapUnit, mapScale);
            barStaticStatus.Caption = statusInformation;
        }

        private void barButtonSave_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //保存地图文档
            IMapDocument mapDocument = new MapDocumentClass();
            mapDocument.New(MainMapControl.DocumentFilename);
            mapDocument.ReplaceContents(MainMapControl.Map as IMxdContents);
            mapDocument.Save(mapDocument.UsesRelativePaths, true);
            mapDocument.Close();
        }

        private void barButtonClearSelection_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //清空选择
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
            Dictionary<string, IRgbColor> dicFieldColor = new Dictionary<string, IRgbColor>();
            IRgbColor rgbColor = new RgbColorClass();
            rgbColor.Red = 255; rgbColor.Green = 0; rgbColor.Blue = 0;
            dicFieldColor.Add("人口密度归一化", rgbColor);
            rgbColor = new RgbColorClass();
            rgbColor.Red = 0; rgbColor.Green = 255; rgbColor.Blue = 0;
            dicFieldColor.Add("建筑用地归一化", rgbColor);
            rgbColor = new RgbColorClass();
            rgbColor.Red = 0; rgbColor.Green = 0; rgbColor.Blue = 255;
            dicFieldColor.Add("坡度归一化", rgbColor);
            rgbColor = new RgbColorClass();
            rgbColor.Red = 255; rgbColor.Green = 255; rgbColor.Blue = 0;
            dicFieldColor.Add("坡度标准差归一化", rgbColor);
            rgbColor = new RgbColorClass();
            rgbColor.Red = 255; rgbColor.Green = 0; rgbColor.Blue = 255;
            dicFieldColor.Add("海拔归一化", rgbColor);

            //获取图层
            IGeoFeatureLayer geoFeatureLayer = MainMapControl.Map.get_Layer(3) as IGeoFeatureLayer;

            //渲染变量
            IChartRenderer chartRender = new ChartRendererClass();
            IRendererFields renderFields = chartRender as IRendererFields;

            //获取数据
            IDataStatistics dataStatistics = null;
            IFeatureCursor featureCursor = null;
            IQueryFilter queryFilter = new QueryFilterClass();
            featureCursor = geoFeatureLayer.Search(queryFilter, true);

            //获取先前的颜色
            IFeature feature = featureCursor.NextFeature();
            ISymbol preSymbol = geoFeatureLayer.Renderer.get_SymbolByFeature(feature);
            IColor preColor = (preSymbol as IFillSymbol).Color;

            double maxProperty = 0; double maxTemp = 0;
            //找到最大值
            foreach (KeyValuePair<string, IRgbColor> _keyValue in dicFieldColor)
            {
                renderFields.AddField(_keyValue.Key);
                dataStatistics = new DataStatisticsClass();
                dataStatistics.Cursor = featureCursor as ICursor;
                dataStatistics.Field = _keyValue.Key;

                maxTemp = dataStatistics.Statistics.Maximum;
                if (maxTemp >= maxProperty)
                    maxProperty = maxTemp;
            }

            //符号变量
            IChartSymbol chartSymbol = null;
            IFillSymbol fillSymbol = null;
            IMarkerSymbol markerSymbol = null;

            //条形图大小
            IBarChartSymbol barChartSymbol = new BarChartSymbolClass();
            barChartSymbol.Width = 10;

            chartSymbol = barChartSymbol as IChartSymbol;
            markerSymbol = barChartSymbol as IMarkerSymbol;
            markerSymbol.Size = 30;

            chartSymbol.MaxValue = maxProperty;
            ISymbolArray symbolArray = barChartSymbol as ISymbolArray;

            foreach (KeyValuePair<string, IRgbColor> _keyValue in dicFieldColor)
            {
                //条形图颜色
                fillSymbol = new SimpleFillSymbolClass();
                fillSymbol.Color = _keyValue.Value;
                symbolArray.AddSymbol(fillSymbol as ISymbol);
                chartRender.ChartSymbol = barChartSymbol as IChartSymbol;
            }


            //底图颜色
            rgbColor = new RgbColorClass();
            rgbColor.Red = 255;
            rgbColor.Green = 0;
            rgbColor.Blue = 0;
            fillSymbol = new SimpleFillSymbolClass();
            fillSymbol.Color = rgbColor;
            chartRender.BaseSymbol = preSymbol as ISymbol;

            //进行渲染
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

                IFillSymbol fillSymbol = new SimpleFillSymbolClass();
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
            object dataFrequency, dataValues;

            ITableHistogram tableHistogram = new BasicTableHistogramClass();
            IBasicHistogram basicHistogram = (IBasicHistogram)tableHistogram;
            tableHistogram.Field = barEditItemPropertySelect.EditValue.ToString();
            tableHistogram.Table = geoFeatureLayer.FeatureClass as ITable;

            basicHistogram.GetHistogram(out dataValues, out dataFrequency);
            IClassifyGEN classifyGEN = new EqualIntervalClass();
            classifyGEN.Classify(dataValues, dataFrequency, 5);

            double[] classes = classifyGEN.ClassBreaks as double[];
            int classesCount = classes.GetUpperBound(0);
            IClassBreaksRenderer classBreaksRenderer = new ClassBreaksRendererClass();

            classBreaksRenderer.Field = barEditItemPropertySelect.EditValue.ToString();
            classBreaksRenderer.BreakCount = classesCount;
            classBreaksRenderer.SortClassesAscending = true;

            IHsvColor fromColor = new HsvColorClass();
            fromColor.Hue = 0; fromColor.Saturation = 50;
            fromColor.Value = 96;
            IHsvColor toColor = new HsvColorClass();
            toColor.Hue = 80;
            toColor.Saturation = 100;
            toColor.Value = 96;
            bool ok;

            //产生色带
            IAlgorithmicColorRamp algorithmicCR = new AlgorithmicColorRampClass();
            algorithmicCR.Algorithm = esriColorRampAlgorithm.esriHSVAlgorithm;
            algorithmicCR.FromColor = fromColor;
            algorithmicCR.ToColor = toColor;
            algorithmicCR.Size = classesCount;
            algorithmicCR.CreateRamp(out ok);

            //获得颜色
            IEnumColors enumColors = algorithmicCR.Colors;
            for (int breakIndex = 0; breakIndex <= classesCount - 1; breakIndex++)
            {
                IColor color = enumColors.Next();

                ISimpleFillSymbol simpleFill = new SimpleFillSymbolClass();
                simpleFill.Color = color;
                simpleFill.Style = esriSimpleFillStyle.esriSFSSolid;
                classBreaksRenderer.set_Symbol(breakIndex, (ISymbol)simpleFill);
                classBreaksRenderer.set_Break(breakIndex, classes[breakIndex + 1]);
            }
            geoFeatureLayer.Renderer = (IFeatureRenderer)classBreaksRenderer;
            MainMapControl.Refresh();
            MainTOCControl.Update();
        }

        private void barButtonPieChart_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Dictionary<string, IRgbColor> dicFieldColor = new Dictionary<string, IRgbColor>();
            IRgbColor rgbColor = new RgbColorClass();
            rgbColor.Red = 255; rgbColor.Green = 0; rgbColor.Blue = 0;
            dicFieldColor.Add("人口密度归一化", rgbColor);
            rgbColor = new RgbColorClass();
            rgbColor.Red = 0; rgbColor.Green = 255; rgbColor.Blue = 0;
            dicFieldColor.Add("建筑用地归一化", rgbColor);
            rgbColor = new RgbColorClass();
            rgbColor.Red = 0; rgbColor.Green = 0; rgbColor.Blue = 255;
            dicFieldColor.Add("坡度归一化", rgbColor);
            rgbColor = new RgbColorClass();
            rgbColor.Red = 255; rgbColor.Green = 255; rgbColor.Blue = 0;
            dicFieldColor.Add("坡度标准差归一化", rgbColor);
            rgbColor = new RgbColorClass();
            rgbColor.Red = 255; rgbColor.Green = 0; rgbColor.Blue = 255;
            dicFieldColor.Add("海拔归一化", rgbColor);

            //获取图层
            IGeoFeatureLayer geoFeatureLayer = MainMapControl.Map.get_Layer(3) as IGeoFeatureLayer;

            //渲染变量
            IChartRenderer chartRender = new ChartRendererClass();
            IRendererFields renderFields = chartRender as IRendererFields;

            //获取数据
            IDataStatistics dataStatistics = null;
            IFeatureCursor featureCursor = null;
            IQueryFilter queryFilter = new QueryFilterClass();
            featureCursor = geoFeatureLayer.Search(queryFilter, true);

            //获取先前的颜色
            IFeature feature = featureCursor.NextFeature();
            ISymbol preSymbol = geoFeatureLayer.Renderer.get_SymbolByFeature(feature);
            IColor preColor = (preSymbol as IFillSymbol).Color;

            double maxProperty = 0; double maxTemp = 0;
            //找到最大值
            foreach (KeyValuePair<string, IRgbColor> _keyValue in dicFieldColor)
            {
                //找到最大值
                renderFields.AddField(_keyValue.Key);
                dataStatistics = new DataStatisticsClass();
                dataStatistics.Cursor = featureCursor as ICursor;
                dataStatistics.Field = _keyValue.Key;

                maxTemp = dataStatistics.Statistics.Maximum;
                if (maxTemp >= maxProperty)
                    maxProperty = maxTemp;
            }
            //符号变量
            IChartSymbol chartSymbol = null;
            IFillSymbol fillSymbol = null;
            IMarkerSymbol markerSymbol = null;

            //饼图属性
            IPieChartSymbol pieChartSymbol = new PieChartSymbolClass();
            pieChartSymbol.Clockwise = true;
            pieChartSymbol.UseOutline = true;
            ILineSymbol lineSymbol = new SimpleLineSymbolClass();

            rgbColor = new RgbColorClass();
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

            //条形图颜色
            IRgbColor rgbColor1 = new RgbColorClass();
            rgbColor1.Red = 0;
            rgbColor1.Green = 0;
            rgbColor1.Blue = 255;


            foreach (KeyValuePair<string, IRgbColor> _keyValue in dicFieldColor)
            {
                //饼图颜色
                fillSymbol = new SimpleFillSymbolClass();
                fillSymbol.Color = _keyValue.Value;
                symbolArray.AddSymbol(fillSymbol as ISymbol);
                chartRender.ChartSymbol = pieChartSymbol as IChartSymbol;
            }

            //底图颜色
            chartRender.BaseSymbol = preSymbol as ISymbol;

            //进行渲染
            chartRender.UseOverposter = false;
            chartRender.CreateLegend();
            geoFeatureLayer.Renderer = chartRender as IFeatureRenderer;
            MainMapControl.Refresh();
            MainTOCControl.Update();
        }

        private void MakeLegend(IEnvelope envelope)
        {
            //地图图例
            IGraphicsContainer graphicsContainer = MainPageLayoutControl.PageLayout as IGraphicsContainer;
            IMapFrame mapFrame = graphicsContainer.FindFrame(MainPageLayoutControl.ActiveView.FocusMap) as IMapFrame;

            UID id = new UID();
            id.Value = "esriCarto.Legend";
            IMapSurroundFrame mapSurroundFrame = mapFrame.CreateSurroundFrame(id, null);
            //如果先前存在图例则删除
            IElement deleteElement = MainPageLayoutControl.FindElementByName("Legend");
            if (deleteElement != null)
            {
                graphicsContainer.DeleteElement(deleteElement);
            }

            //设置图例背景
            ISymbolBackground symbolBackground = new SymbolBackgroundClass();
            IFillSymbol fillSymbol = new SimpleFillSymbolClass();
            ILineSymbol lineSymbol = new SimpleLineSymbolClass();

            RgbColor colorLine = new RgbColorClass();
            colorLine.Red = 240; colorLine.Green = 240; colorLine.Blue = 240;
            RgbColor colorFill = new RgbColorClass();
            colorFill.Red = 240; colorFill.Green = 240; colorFill.Blue = 240;

            fillSymbol.Outline = lineSymbol;
            fillSymbol.Color = colorFill;
            symbolBackground.FillSymbol = fillSymbol;
            mapSurroundFrame.Background = symbolBackground;

            IElement element = mapSurroundFrame as IElement;
            element.Geometry = envelope as IGeometry;
            IMapSurround mapSurround = mapSurroundFrame.MapSurround;
            ILegend legend = mapSurround as ILegend;
            legend.ClearItems();
            legend.Title = "图例";
            for (int i = 0; i < MainPageLayoutControl.ActiveView.FocusMap.LayerCount; i++)
            {
                ILegendItem legendItem = new HorizontalLegendItemClass();
                legendItem.Layer = MainPageLayoutControl.ActiveView.FocusMap.get_Layer(i);
                legendItem.ShowDescriptions = false;
                legendItem.Columns = 1;
                legendItem.ShowHeading = true;
                legendItem.ShowLabels = true;
                legend.AddItem(legendItem);
            }
            graphicsContainer.AddElement(element, 0);
            MainPageLayoutControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

        }

        private void AddScaleBar(IEnvelope envelope, IStyleGalleryItem styleGalleryItem)
        {
            //地图图例
            IGraphicsContainer graphicsContainer = MainPageLayoutControl.PageLayout as IGraphicsContainer;
            IMapFrame mapFrame = graphicsContainer.FindFrame(MainPageLayoutControl.ActiveView.FocusMap) as IMapFrame;

            IMapSurroundFrame mapSurroundFrame = new MapSurroundFrameClass();
            mapSurroundFrame.MapFrame = mapFrame;
            mapSurroundFrame.MapSurround = (IMapSurround)styleGalleryItem.Item;

            IElement element = MainPageLayoutControl.FindElementByName("ScaleBar");
            if (element != null)
            {
                graphicsContainer.DeleteElement(element);
            }

            IElementProperties elementProperties = null;
            element = (IElement)mapSurroundFrame;
            element.Geometry = (IGeometry)envelope;

            elementProperties = element as IElementProperties;
            elementProperties.Name = "ScaleBar";
            graphicsContainer.AddElement(element, 0);
            MainPageLayoutControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        private void AddNorthArrow(IEnvelope envelope, IStyleGalleryItem styleGalleryItem)
        {
            //地图图例
            IGraphicsContainer graphicsContainer = MainPageLayoutControl.PageLayout as IGraphicsContainer;
            IMapFrame mapFrame = graphicsContainer.FindFrame(MainPageLayoutControl.ActiveView.FocusMap) as IMapFrame;

            IMapSurroundFrame mapSurroundFrame = new MapSurroundFrameClass();
            mapSurroundFrame.MapFrame = mapFrame;
            INorthArrow northArrow = new MarkerNorthArrowClass();
            northArrow = styleGalleryItem.Item as INorthArrow;
            northArrow.Size = envelope.Width * 50;

            mapSurroundFrame.MapSurround = (IMapSurround)northArrow;
            IElement element = MainPageLayoutControl.FindElementByName("NorthArrows");
            if (element != null)
            {
                graphicsContainer.DeleteElement(element);
            }
            IElementProperties elementProperties = null;
            element = (IElement)mapSurroundFrame;
            element.Geometry = (IGeometry)envelope;
            elementProperties = element as IElementProperties;
            elementProperties.Name = "NorthArrows";
            graphicsContainer.AddElement(element, 0);
            MainPageLayoutControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        private void AddGrid()
        {
            /*
            //添加格网
            IMap map = MainPageLayoutControl.ActiveView.FocusMap;
            IMeasuredGrid measureGrid = new MeasuredGridClass();

            measureGrid.FixedOrigin = false;
            measureGrid.Units = map.MapUnits;
            measureGrid.XIntervalSize = 5;
            measureGrid.YIntervalSize = 5;

            IGridLabel gridLabel = new FormattedGridLabelClass();
            IFormattedGridLabel formattedGridLabel = new FormattedGridLabelClass();
            INumericFormat numericFormat = new NumericFormatClass();
            numericFormat.AlignmentOption = esriNumericAlignmentEnum.esriAlignLeft;
            numericFormat.RoundingOption = esriRoundingOptionEnum.esriRoundNumberOfDecimals;
            numericFormat.RoundingValue = 0;
            numericFormat.ZeroPad = true;
            formattedGridLabel.Format = numericFormat as INumberFormat;
            gridLabel = formattedGridLabel as IGridLabel;
            StdFont font = new stdole.StdFontClass();
            font.Name = "Times New Roman";
            font.Size = 25;
            gridLabel.Font = font as IFontDisp;

            IMapGrid mapGrid = new MeasuredGridClass();
            mapGrid = measureGrid as IMapGrid;

            mapGrid.LabelFormat = gridLabel;
            IGraphicsContainer graphicsContainer = MainPageLayoutControl.PageLayout as IGraphicsContainer;
            IFrameElement frameElement = graphicsContainer.FindFrame(map);
            IMapFrame mapFrame = frameElement as IMapFrame;

            IMapGrids mapGrids = null;
            mapGrids = mapFrame as IMapGrids;
            mapGrids.AddMapGrid(mapGrid);
            MainPageLayoutControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewBackground, null, null);
            */
            IMap map = MainPageLayoutControl.ActiveView.FocusMap;
            IGraticule graticule = new GraticuleClass();

            //格网线符号样式
            ICartographicLineSymbol lineSymbol;
            lineSymbol = new CartographicLineSymbolClass();
            lineSymbol.Cap = esriLineCapStyle.esriLCSButt;
            lineSymbol.Width = 1;
            IRgbColor rgbColorLineSymbol = new RgbColorClass();
            rgbColorLineSymbol.Red = 166; rgbColorLineSymbol.Green = 187; rgbColorLineSymbol.Blue = 208;
            lineSymbol.Color = rgbColorLineSymbol;

            //格网边框样式
            ISimpleMapGridBorder simpleMapGridBorder = new SimpleMapGridBorderClass();
            ISimpleLineSymbol simpleLineSymbol = new SimpleLineSymbolClass();
            simpleLineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
            IRgbColor rgbColorSimpleLine = new RgbColorClass();
            rgbColorSimpleLine.Red = 100; rgbColorSimpleLine.Green = 255; rgbColorSimpleLine.Blue = 0;
            simpleLineSymbol.Color = rgbColorSimpleLine;
            simpleLineSymbol.Width = 2;
            simpleMapGridBorder.LineSymbol = simpleLineSymbol as ILineSymbol;
            graticule.Border = simpleMapGridBorder as IMapGridBorder;
            graticule.SetTickVisibility(true, true, true, true);

            //格网刻度样式
            graticule.TickLength = 15;
            lineSymbol = new CartographicLineSymbolClass();
            lineSymbol.Cap = esriLineCapStyle.esriLCSButt;
            lineSymbol.Width = 1;
            rgbColorLineSymbol.Red = 255; rgbColorLineSymbol.Green = 187; rgbColorLineSymbol.Blue = 208;
            lineSymbol.Color = rgbColorLineSymbol;
            graticule.TickMarkSymbol = null;
            graticule.TickLineSymbol = lineSymbol;
            graticule.SetTickVisibility(true, true, true, true);

            //格网次级刻度
            graticule.SubTickCount = 5;
            graticule.SubTickLength = 10;
            lineSymbol = new CartographicLineSymbolClass();
            lineSymbol.Cap = esriLineCapStyle.esriLCSButt;
            lineSymbol.Width = 0.1;
            rgbColorLineSymbol.Red = 166; rgbColorLineSymbol.Green = 187; rgbColorLineSymbol.Blue = 208;
            lineSymbol.Color = rgbColorLineSymbol;
            graticule.SubTickLineSymbol = lineSymbol;
            graticule.SetSubTickVisibility(true, true, true, true);

            //格网标签样式
            IGridLabel gridLabel = graticule.LabelFormat;
            gridLabel.LabelOffset = 15;
            stdole.StdFont font = new stdole.StdFont();
            font.Name = "Arial";
            font.Size = 16;
            graticule.LabelFormat.Font = font as stdole.IFontDisp;
            graticule.Visible = true;

            //创建格网
            IMeasuredGrid measureGrid = new MeasuredGridClass();
            IProjectedGrid projectedGrid = measureGrid as IProjectedGrid;
            projectedGrid.SpatialReference = map.SpatialReference;
            measureGrid = graticule as IMeasuredGrid;

            double maxX, maxY, minX, minY;
            projectedGrid.SpatialReference.GetDomain(out minX, out maxX, out minY, out maxY);
            measureGrid.FixedOrigin = true;
            measureGrid.Units = map.MapUnits;
            measureGrid.XIntervalSize = (maxX - minX) / 200;
            measureGrid.XOrigin = minX;
            measureGrid.YIntervalSize = (maxY - minY) / 200;
            measureGrid.YOrigin = minY;

            IGraphicsContainer graphicsContainer = MainPageLayoutControl.ActiveView as IGraphicsContainer;
            IMapFrame mapFrame = graphicsContainer.FindFrame(map) as IMapFrame;
            IMapGrids mapGrids = mapFrame as IMapGrids;
            mapGrids.AddMapGrid(graticule);
            MainPageLayoutControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewBackground, null, null);
        }

        private void AddChartName(IEnvelope envelope)
        {
            stdole.IFontDisp font;
            font = new stdole.StdFontClass() as stdole.IFontDisp;
            font.Name = "宋体";

            ITextSymbol textSymbol = new TextSymbolClass();
            textSymbol.Size = 80;
            textSymbol.Font = font;

            ITextElement textElement = new TextElementClass();
            textElement.Symbol = textSymbol;
            textElement.Text = "横车镇土地利用图";

            IElement element = textElement as IElement;
            element.Geometry = envelope as IGeometry;

            IGraphicsContainer graphicsContainer = MainPageLayoutControl.ActiveView as IGraphicsContainer;
            graphicsContainer.AddElement(element, 0);
            MainPageLayoutControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }
        private void MainPageLayoutControl_OnMouseDown(object sender, IPageLayoutControlEvents_OnMouseDownEvent e)
        {
            if (barCheckItemLegend.Checked)
            {
                //图例
                IEnvelope envelope = TrackRectangle(e);
                MakeLegend(envelope);
                barCheckItemLegend.Checked = false;
            }
            else if (barCheckItemArrow.Checked)
            {
                //指北针
                IEnvelope envelope = TrackRectangle(e);
                SymbolSelection symbolSelection = new SymbolSelection();
                symbolSelection.LoadSymbol(1);
                symbolSelection.ShowDialog();

                AddNorthArrow(envelope, symbolSelection.selectGalleryItem);
                barCheckItemArrow.Checked = false;
            }
            else if (barCheckItemScale.Checked)
            {
                //比例尺
                IEnvelope envelope = TrackRectangle(e);
                SymbolSelection symbolSelection = new SymbolSelection();
                symbolSelection.LoadSymbol(2);
                symbolSelection.ShowDialog();

                AddScaleBar(envelope, symbolSelection.selectGalleryItem);
                barCheckItemScale.Checked = false;
            }
            else if (barCheckItemGrid.Checked)
            {
                //地图格网
                AddGrid();
                barCheckItemGrid.Checked = false;
            }
            else if (barCheckItemChartName.Checked)
            {
                //地图图名
                IEnvelope envelope = TrackRectangle(e);
                AddChartName(envelope);
                barCheckItemChartName.Checked = false;
            }
        }

        private IEnvelope TrackRectangle(IPageLayoutControlEvents_OnMouseDownEvent e)
        {
            //矩形框选
            IEnvelope envelope = MainPageLayoutControl.TrackRectangle();
            IGeometry geometry = envelope as IGeometry;
            if (geometry.IsEmpty == true)
            {
                tagRECT r;
                r.left = e.x - 1;
                r.top = e.y - 1;
                r.right = e.x + 1;
                r.bottom = e.y - 1;
                MainPageLayoutControl.ActiveView.ScreenDisplay.DisplayTransformation.TransformRect(envelope, ref r, 4);
                envelope.SpatialReference = MainPageLayoutControl.ActiveView.FocusMap.SpatialReference;
            }
            return envelope;
        }

        private void barButtonItemSpatialCompute_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            IFeatureLayer featureLayer = MainMapControl.get_Layer(3) as FeatureLayer;
            IFeatureClass featureClass = featureLayer.FeatureClass;
            IQueryFilter queryFilter = new QueryFilterClass();
            IFeatureCursor featureCursor = featureClass.Update(queryFilter, true);

            double pop = Convert.ToDouble(barEditItemPopulation.EditValue.ToString());
            double build = Convert.ToDouble(barEditItemBuilding.EditValue.ToString());
            double slope = Convert.ToDouble(barEditItemSlope.EditValue.ToString());
            double slope_std = Convert.ToDouble(barEditItemSlopeStd.EditValue.ToString());
            double altitude = Convert.ToDouble(barEditItemAltitude.EditValue.ToString());

            IFeature feature = featureCursor.NextFeature();
            int populationIndex = featureCursor.FindField("人口密度归一化");
            int buildingIndex = featureCursor.FindField("建筑用地归一化");
            int slopeIndex = featureCursor.FindField("坡度归一化");
            int slopeStdIndex = featureCursor.FindField("坡度标准差归一化");
            int altitudeIndex = featureCursor.FindField("海拔归一化");
            int scoreIndex = featureCursor.FindField("总分");

            while (feature != null)
            {
                double pop_t = Convert.ToDouble(feature.get_Value(populationIndex));
                double build_t = Convert.ToDouble(feature.get_Value(buildingIndex));
                double slope_t = Convert.ToDouble(feature.get_Value(slopeIndex));
                double slope_std_t = Convert.ToDouble(feature.get_Value(slopeStdIndex));
                double altitude_t = Convert.ToDouble(feature.get_Value(altitudeIndex));

                double totalScore = pop * pop_t + build * build_t + slope * slope_t + slope_std * slope_std_t + altitude * altitude_t;
                feature.set_Value(scoreIndex, totalScore);
                featureCursor.UpdateFeature(feature);
                feature = featureCursor.NextFeature();
            }
        }

        private void barButtonItemStatistic_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            double sum = 0;
            if (barEditItemProperty.EditValue.ToString() != "人口数" && barEditItemProperty.EditValue.ToString() != "人均耕地")
            {
                //统计地类面积
                IGeoFeatureLayer featureLayer = MainMapControl.get_Layer(2) as IGeoFeatureLayer;
                IFeatureClass featureClass = featureLayer.FeatureClass;

                if (barEditItemCountry.EditValue.ToString() != "全部村庄")
                {
                    //分村落统计
                    IQueryFilter queryFilter = new QueryFilterClass();
                    queryFilter.WhereClause = string.Format("\"{0}\"=\'{1}\' AND \"{2}\"=\'{3}\'", "权属名称", barEditItemCountry.EditValue.ToString(), "地类名", barEditItemProperty.EditValue.ToString());

                    IFeatureCursor featureCursor = featureClass.Search(queryFilter, true);
                    IFeature feature = featureCursor.NextFeature();

                    int fieldIndex;
                    fieldIndex = feature.Fields.FindField("Shape_Area");
                    while (feature != null)
                    {
                        sum += Convert.ToDouble(feature.get_Value(fieldIndex));
                        feature = featureCursor.NextFeature();
                    }
                    barStaticItemStatisticsResult.Caption = string.Format("{0}{1}总面积为:{2}平方米", barEditItemCountry.EditValue.ToString(), barEditItemProperty.EditValue.ToString(), Convert.ToInt32(sum).ToString());
                }
                else
                {
                    //全村统计
                    IQueryFilter queryFilter = new QueryFilterClass();

                    IFeatureCursor featureCursor = featureClass.Search(queryFilter, true);
                    IFeature feature = featureCursor.NextFeature();

                    int fieldIndex;
                    fieldIndex = feature.Fields.FindField("Shape_Area");
                    while (feature != null)
                    {
                        sum += Convert.ToDouble(feature.get_Value(fieldIndex));
                        feature = featureCursor.NextFeature();
                    }
                    barStaticItemStatisticsResult.Caption = string.Format("全部村庄{0}总面积为:{1}平方米", barEditItemProperty.EditValue.ToString(), Convert.ToInt32(sum).ToString());
                }
            }
            else if (barEditItemProperty.EditValue.ToString() == "人口数")
            {
                //统计人口数量
                IGeoFeatureLayer featureLayer = MainMapControl.get_Layer(3) as IGeoFeatureLayer;
                IFeatureClass featureClass = featureLayer.FeatureClass;

                if (barEditItemCountry.EditValue.ToString() != "全部村庄")
                {
                    //各村落人口数量
                    IQueryFilter queryFilter = new QueryFilterClass();
                    queryFilter.WhereClause = string.Format("\"{0}\"=\'{1}\'", "权属名称", barEditItemCountry.EditValue.ToString());

                    IFeatureCursor featureCursor = featureClass.Search(queryFilter, true);
                    IFeature feature = featureCursor.NextFeature();

                    int fieldIndex;
                    fieldIndex = feature.Fields.FindField("population");
                    sum = Convert.ToDouble((feature.get_Value(fieldIndex)));
                    barStaticItemStatisticsResult.Caption = string.Format("{0}总人口数为:{1}人", barEditItemCountry.EditValue.ToString(), Convert.ToInt32(sum).ToString());
                }
                else
                {
                    //全部人口数量
                    IQueryFilter queryFilter = new QueryFilterClass();

                    IFeatureCursor featureCursor = featureClass.Search(queryFilter, true);
                    IFeature feature = featureCursor.NextFeature();
                    int fieldIndex;
                    fieldIndex = feature.Fields.FindField("population");
                    while (feature != null)
                    {
                        sum += Convert.ToDouble(feature.get_Value(fieldIndex));
                        feature = featureCursor.NextFeature();
                    }
                    barStaticItemStatisticsResult.Caption = string.Format("{0}总人口数为:{1}人", barEditItemCountry.EditValue.ToString(), Convert.ToInt32(sum).ToString());
                }
            }
            else
            {
                //人均耕地面积
                IGeoFeatureLayer featureLayerLand = MainMapControl.get_Layer(2) as IGeoFeatureLayer;
                IFeatureClass featureClassLand = featureLayerLand.FeatureClass;

                IGeoFeatureLayer featureLayer= MainMapControl.get_Layer(3) as IGeoFeatureLayer;
                IFeatureClass featureClass= featureLayer.FeatureClass;
                if (barEditItemCountry.EditValue.ToString() != "全部村庄")
                {
                    //各村人均耕地面积
                    //耕地面积
                    IQueryFilter queryFilterLand = new QueryFilterClass();
                    queryFilterLand.WhereClause = string.Format("\"{0}\"=\'{1}\' AND (\"{2}\"=\'{3}\' OR \"{4}\"=\'{5}\')",
                        "权属名称", barEditItemCountry.EditValue.ToString(), "地类名", "水田", "地类名", "旱地");

                    IFeatureCursor featureCursorLand = featureClassLand.Search(queryFilterLand, true);
                    IFeature featureLand = featureCursorLand.NextFeature();

                    int fieldIndex;
                    fieldIndex = featureLand.Fields.FindField("Shape_Area");
                    while (featureLand != null)
                    {
                        sum += Convert.ToDouble(featureLand.get_Value(fieldIndex));
                        featureLand = featureCursorLand.NextFeature();
                    }

                    IQueryFilter queryFilter = new QueryFilterClass();
                    queryFilter.WhereClause = string.Format("\"{0}\"=\'{1}\'", "权属名称", barEditItemCountry.EditValue.ToString());

                    IFeatureCursor featureCursor = featureClass.Search(queryFilter, true);
                    IFeature feature = featureCursor.NextFeature();

                    fieldIndex = feature.Fields.FindField("population");
                    double popu=Convert.ToDouble((feature.get_Value(fieldIndex)));
                    sum = sum/popu;

                    barStaticItemStatisticsResult.Caption = string.Format("{0}人均耕地面积为:{1}平方米/人", barEditItemCountry.EditValue.ToString(), Convert.ToInt32(sum).ToString());
                }
                else
                {
                    IQueryFilter queryFilter = new QueryFilterClass();

                    IFeatureCursor featureCursor = featureClass.Search(queryFilter, true);
                    IFeature feature = featureCursor.NextFeature();
                    int fieldIndex;
                    fieldIndex = feature.Fields.FindField("population");
                    while (feature != null)
                    {
                        sum += Convert.ToDouble(feature.get_Value(fieldIndex));
                        feature = featureCursor.NextFeature();
                    }
                    barStaticItemStatisticsResult.Caption = string.Format("{0}总人口数为:{1}人", barEditItemCountry.EditValue.ToString(), Convert.ToInt32(sum).ToString());
                }
            }

        }

        private void barButtonItemColorBuffer_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }
    }
}