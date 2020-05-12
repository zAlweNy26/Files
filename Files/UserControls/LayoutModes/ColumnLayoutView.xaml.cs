using Files.Filesystem;
using Microsoft.AppCenter.Utils.Files;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Files.UserControls.LayoutModes
{

    public sealed partial class ColumnLayoutView : BaseLayout
    {

        private CancellationTokenSource tokenSource;
        private CancellationTokenSource tokenSource2;
        public ListView CurrentListView { get; set; }

        public ColumnLayoutView()
        {
            this.InitializeComponent();
            CurrentListView = RootFileView;
            CurrentListView.SelectionChanged += RootFileView_SelectionChanged;
            if (!string.IsNullOrWhiteSpace(App.CurrentInstance.ViewModel.WorkingDirectory))
            {
                ReloadItemsForWorkingDirectory();
                RootFileView.ItemsSource = AssociatedViewModel.FilesAndFolders;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs eventArgs)
        {
            for (int i = 0 + 1; i < FileBladeView.Items.Count; i++)
            {
                Debug.WriteLine(i);
                FileBladeView.Items.RemoveAt(i);
                FileBladeView.ActiveBlades.RemoveAt(i);
            }
        }

        private void BladeView_BladeClosed(object sender, Microsoft.Toolkit.Uwp.UI.Controls.BladeItem e)
        {
            var indexofblade = FileBladeView.ActiveBlades.IndexOf(e);
            for (int i = indexofblade + 1; i < FileBladeView.ActiveBlades.Count; i++)
            {
                FileBladeView.ActiveBlades.RemoveAt(i);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CurrentListView.SelectionMode = ListViewSelectionMode.Multiple;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (CurrentListView.SelectionMode == ListViewSelectionMode.Multiple)
            {
                CurrentListView.SelectionMode = ListViewSelectionMode.Single;
            }
        }
        private void Grid_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                if (CurrentListView.SelectionMode == ListViewSelectionMode.Multiple)
                {
                    CurrentListView.SelectionMode = ListViewSelectionMode.Single;
                }
            }
        }


        private void Addresspath_ItemClick(object sender, ItemClickEventArgs e)
        {
            var address = e.ClickedItem as ListViewItem;
            for (int i = 0 + 1; i < FileBladeView.Items.Count; i++)
            {
                Debug.WriteLine(i);
                FileBladeView.Items.RemoveAt(i);
                FileBladeView.ActiveBlades.RemoveAt(i);
            }
            ReloadItemsForWorkingDirectory(address.Tag.ToString());
        }

        private void RootFileView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count() == 1 && CurrentListView.SelectionMode != ListViewSelectionMode.Multiple)
            {
                base.SelectedItem = e.AddedItems[0] as ListedItem;
                PreviewImage.Source = base.SelectedItem.FileImage;
                PreviewName.Text = base.SelectedItem.ItemName;
                PreviewType.Text = base.SelectedItem.ItemType;
                var item1 = e.AddedItems[0] as ListedItem;
                try
                {
                    if (item1.PrimaryItemAttribute == Windows.Storage.StorageItemTypes.Folder)
                    {
                        if (tokenSource2 != null)
                        {
                            tokenSource2.Cancel();
                        }
                        try
                        {
                            Debug.WriteLine(item1.ItemPath);
                        }
                        catch
                        {
                            Debug.WriteLine("Null!");
                        }


                        for (int i = item1.Tag + 1; i < FileBladeView.Items.Count; i++)
                        {
                            Debug.WriteLine(i);
                            FileBladeView.Items.RemoveAt(i);
                            FileBladeView.ActiveBlades.RemoveAt(i);
                        }
                        var blade = new BladeItem();
                        //FileBladeView.ActiveBlades.Add(blade);
                        blade.TitleBarVisibility = Visibility.Collapsed;
                        blade.Style = BladeStyle;
                        ReloadItemsForWorkingDirectory(item1.ItemPath);
                        foreach (ListedItem li in AssociatedViewModel.FilesAndFolders) { li.Tag = item1.Tag; }
                        var newlistview = new ListView
                        {
                            ItemsSource = AssociatedViewModel.FilesAndFolders,
                            ItemTemplate = Filev,
                            IsItemClickEnabled = true,
                            Tag = FileBladeView.Items.IndexOf(blade).ToString()
                        };
                        CurrentListView = newlistview;
                        newlistview.SelectionChanged += RootFileView_SelectionChanged;
                        blade.Content = newlistview;
                        FileBladeView.Items.Insert(item1.Tag + 1, blade);
                        for (int i = FileBladeView.Items.IndexOf(blade) + 1; i < FileBladeView.Items.Count; i++)
                        {
                            Debug.WriteLine(i);
                            FileBladeView.Items.RemoveAt(i);
                            FileBladeView.ActiveBlades.RemoveAt(i);
                        }
                        Debug.WriteLine(FileBladeView.Items.IndexOf(blade));
                    }
                }
                catch
                {

                }
            }
        }

        protected override void SetSelectedItemOnUi(ListedItem selectedItem)
        {
            BladeItem bladeitemToUse = FileBladeView.Items.First(x => ((x as BladeItem).Content as ListView).Items.Contains(selectedItem)) as BladeItem;
            (bladeitemToUse.Content as ListView).SelectedItem = selectedItem;
        }

        protected override void SetSelectedItemsOnUi(List<ListedItem> selectedItems)
        {
            throw new System.NotImplementedException();
        }

        public override void FocusSelectedItems()
        {
            throw new System.NotImplementedException();
        }

        protected override ListedItem GetItemFromElement(object element)
        {
            ListViewItem item = element as ListViewItem;
            return item.DataContext as ListedItem;
        }
    }
}
