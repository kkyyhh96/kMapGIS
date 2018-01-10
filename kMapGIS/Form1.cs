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
    }
}