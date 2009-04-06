using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace DasBackupTool.Ui
{
    public static class ListViewBehaviors
    {
        public static readonly DependencyProperty IsAutoUpdatingColumnWidthsProperty = DependencyProperty.RegisterAttached("IsAutoUpdatingColumnWidths", typeof(bool), typeof(ListViewBehaviors), new UIPropertyMetadata(false, OnIsAutoUpdatingColumnWidthsChanged));

        public static bool GetIsAutoUpdatingColumnWidths(ListView listView)
        {
            return (bool)listView.GetValue(IsAutoUpdatingColumnWidthsProperty);
        }

        public static void SetIsAutoUpdatingColumnWidths(ListView listView, bool value)
        {
            listView.SetValue(IsAutoUpdatingColumnWidthsProperty, value);
        }

        private static void OnIsAutoUpdatingColumnWidthsChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ListView listView = o as ListView;
            if ((null != listView) && (e.NewValue is bool))
            {
                DependencyPropertyDescriptor descriptor = DependencyPropertyDescriptor.FromProperty(ListView.ItemsSourceProperty, typeof(ListView));
                if ((bool)e.NewValue)
                {
                    descriptor.AddValueChanged(listView, OnListViewItemsSourceValueChanged);
                }
                else
                {
                    descriptor.RemoveValueChanged(listView, OnListViewItemsSourceValueChanged);
                }
            }
        }

        private static void OnListViewItemsSourceValueChanged(object sender, EventArgs e)
        {
            ListView listView = sender as ListView;
            if (null != listView)
            {
                GridView gridView = listView.View as GridView;
                if (null != gridView)
                {
                    UpdateColumnWidths(gridView);
                }
            }
        }

        private static void UpdateColumnWidths(GridView gridView)
        {
            foreach (var column in gridView.Columns)
            {
                if (double.IsNaN(column.Width))
                {
                    column.Width = 0;
                    column.Width = double.NaN;
                }
            }
        }
    }
}
