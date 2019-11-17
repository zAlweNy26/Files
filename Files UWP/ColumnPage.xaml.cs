using Files.Filesystem;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Files
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ColumnPage : Page
    {
        public static NavigationView nv;
        public ObservableCollection<ListedItem> FilesAndFolders { get; set; } = new ObservableCollection<ListedItem>();
        public ListView CurrentListView { get; set; }

        public static string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        public static string DocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public static string DownloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";
        public static string OneDrivePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\OneDrive";
        public static string PicturesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        public static string MusicPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        public static string VideosPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
        private CancellationTokenSource tokenSource;
        private string folderpath;
        private IReadOnlyList<StorageFile> storageItems;
        private StorageFolder folder;
        private CancellationTokenSource tokenSource2;
        public ListedItem selecteditem;

        public ColumnPage()
        {
            this.InitializeComponent();

            nv = navView;
            PopulateNavViewWithExternalDrives();
            CurrentListView = RootFileView;
            CurrentListView.SelectionChanged += RootFileView_SelectionChanged;
            if (App.CurrentFolder != null)
            {
                dothis();
            }
        }

        private async void dothis()
        {
            RootFileView.ItemsSource = await getitems(App.CurrentFolder, 0);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();

            dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager,

            DataRequestedEventArgs>(this.ShareTextHandler);
        }
        public async void PopulateNavViewWithExternalDrives()
        {
            var knownRemDevices = new ObservableCollection<string>();
            foreach (var f in await KnownFolders.RemovableDevices.GetFoldersAsync())
            {
                var path = f.Path;
                knownRemDevices.Add(path);
            }

            var driveLetters = DriveInfo.GetDrives().Select(x => x.RootDirectory.Root).ToList();

            if (!driveLetters.Any()) return;

            driveLetters.ToList().ForEach(roots =>
            {
                try
                {
                    if (roots.Name == @"C:\") return;
                    var content = string.Empty;
                    SymbolIcon icon;
                    if (knownRemDevices.Contains(roots.Name))
                    {
                        content = $"Removable Drive ({roots.Name})";
                        icon = new SymbolIcon((Symbol)0xE88E);
                    }
                    else
                    {
                        content = $"Local Disk ({roots.Name})";
                        icon = new SymbolIcon((Symbol)0xEDA2);
                    }
                    nv.MenuItems.Add(new NavigationViewItem()
                    {
                        Content = content,
                        Icon = icon,
                        Tag = roots.Name
                    });
                }
                catch (UnauthorizedAccessException e)
                {
                    Debug.WriteLine(e.Message);
                }
            });
        }
        private void BladeView_BladeClosed(object sender, Microsoft.Toolkit.Uwp.UI.Controls.BladeItem e)
        {
            var indexofblade = FileBladeView.ActiveBlades.IndexOf(e);
            for (int i = indexofblade + 1; i < FileBladeView.ActiveBlades.Count; i++)
            {
                FileBladeView.ActiveBlades.RemoveAt(i);
            }
        }

        //private async void ListView_ItemClick(object sender, ItemClickEventArgs e)
        //{
        //    selecteditem = e.ClickedItem as ListedItem;
        //    PreviewImage.Source = selecteditem.FileImg;
        //    PreviewName.Text = selecteditem.FileName;
        //    PreviewType.Text = selecteditem.FileType;
        //    var item1 = e.ClickedItem as ListedItem;
        //    try
        //    {
        //        if (item1.FileType == "Folder")
        //        {
        //            if (tokenSource2 != null)
        //            {
        //                tokenSource2.Cancel();
        //            }
        //            try
        //            {
        //                Debug.WriteLine(item1.FilePath);
        //            }
        //            catch
        //            {
        //                Debug.WriteLine("Null!");
        //            }
        //            //var item = e.ClickedItem as ListViewItem;
        //            ////var lv = item.FindParent<ListView>();
        //            ////var type = item.ContentTemplateRoot as ListViewItem;
        //            //var type = ItemsControl.ItemsControlFromItemContainer(item);
        //            //Debug.WriteLine(type.Tag.ToString());
        //            //var parent = lv.FindParent<BladeItem>();
        //            //Debug.WriteLine(parent.Name);
        //            ////var parent = item.Parent as BladeItem;
        //            //var indexof = FileBladeView.Items.IndexOf(parent);
        //            Debug.WriteLine(item1.tag.ToString());
        //            for (int i = item1.tag + 1; i < FileBladeView.Items.Count; i++)
        //            {
        //                Debug.WriteLine(i);
        //                FileBladeView.Items.RemoveAt(i);
        //                FileBladeView.ActiveBlades.RemoveAt(i);
        //            }
        //            var blade = new BladeItem();
        //            //FileBladeView.ActiveBlades.Add(blade);
        //            blade.TitleBarVisibility = Visibility.Collapsed;
        //            blade.Style = BladeStyle;

        //            var newlistview = new ListView
        //            {
        //                ItemsSource = await getitems(item1.FilePath, item1.tag + 1),
        //                ItemTemplate = Filev,
        //                IsItemClickEnabled = true,
        //                Tag = FileBladeView.Items.IndexOf(blade).ToString()
        //            };
        //            newlistview.ItemClick += ListView_ItemClick;
        //            blade.Content = newlistview;
        //            FileBladeView.Items.Insert(item1.tag + 1, blade);
        //            for (int i = FileBladeView.Items.IndexOf(blade) + 1; i < FileBladeView.Items.Count; i++)
        //            {
        //                Debug.WriteLine(i);
        //                FileBladeView.Items.RemoveAt(i);
        //                FileBladeView.ActiveBlades.RemoveAt(i);
        //            }
        //            Debug.WriteLine(FileBladeView.Items.IndexOf(blade));
        //        }
        //    }
        //    catch
        //    {

        //    }
        //}
        private async Task<ObservableCollection<ListedItem>> getitems(string path, int v)
        {
            var split1 = path.Split(@"\");
            Addresspath.Items.Clear();
            string pathtoconjoin = "";
            for (int i = 0; i < split1.Count(); i++)
            {
                pathtoconjoin += split1[i] + @"\";
                Debug.WriteLine(split1[i]);
                if (split1.Count() == 1)
                {
                    Addresspath.Items.Add(new ListViewItem
                    {
                        Tag = path,
                        Content = split1[i]
                    });
                }
                else
                {
                    Addresspath.Items.Add(new ListViewItem
                    {
                        Tag = pathtoconjoin,
                        Content = split1[i]
                    });
                }
            }

            Debug.WriteLine("Getting Items...");
            tokenSource2 = new CancellationTokenSource();
            CancellationToken token2 = tokenSource2.Token;
            try
            {
                var fileitems = new ObservableCollection<ListedItem>();
                var folder = await StorageFolder.GetFolderFromPathAsync(path);
                var items = await folder.GetItemsAsync();
                int filesCountSnapshot = items.Count;
                foreach (var item in items)
                {
                    if (token2.IsCancellationRequested)
                    {
                        return null;
                    }
                    if (item.IsOfType(StorageItemTypes.File))
                    {

                        var file = item as StorageFile;
                        Debug.WriteLine(file.DisplayName);
                        var img = new BitmapImage();
                        try
                        {
                            var stream = await file.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.SingleItem, 200, Windows.Storage.FileProperties.ThumbnailOptions.UseCurrentScale);
                            await img.SetSourceAsync(stream);
                        }
                        catch
                        {
                            Debug.WriteLine("?");
                        }
                        var name = file.DisplayName;
                        var type = file.DisplayType;
                        try
                        {
                            fileitems.Add(new ListedItem(file.FolderRelativeId)
                            { FileImg = img, FileName = name, FileDateReal = file.DateCreated, FileType = file.DisplayType, FilePath = file.Path, tag = v });
                        }
                        catch
                        {
                            Debug.WriteLine("?");
                        }
                    }
                    if (item.IsOfType(StorageItemTypes.Folder))
                    {
                        var file = item as StorageFolder;
                        var img = new BitmapImage();
                        try
                        {
                            var stream = await file.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.DocumentsView, 200, Windows.Storage.FileProperties.ThumbnailOptions.None);
                            await img.SetSourceAsync(stream);
                        }
                        catch
                        {

                        }
                        var name = file.DisplayName;
                        var type = "Folder";
                        fileitems.Add(new ListedItem(file.FolderRelativeId) { FileImg = img, FileName = name, FileDateReal = file.DateCreated, FileType = type, FilePath = file.Path, tag = v });
                    }
                }
                return fileitems;
            }
            catch
            {
                return null;
            }
        }

        private async void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {

            var item = args.InvokedItem;
            var itemContainer = args.InvokedItemContainer;
            if (tokenSource != null)
            {
                tokenSource.Cancel();
                FilesAndFolders.Clear();
            }
            for (int i = 0 + 1; i < FileBladeView.Items.Count; i++)
            {
                Debug.WriteLine(i);
                FileBladeView.Items.RemoveAt(i);
                FileBladeView.ActiveBlades.RemoveAt(i);
            }
            if (item.ToString() == "Home")
            {

            }
            else if (item.ToString() == "Desktop")
            {
                RootFileView.ItemsSource = await getitems(DesktopPath, 0);
            }
            else if (item.ToString() == "Documents")
            {
                RootFileView.ItemsSource = await getitems(DocumentsPath, 0);
            }
            else if (item.ToString() == "Downloads")
            {
                RootFileView.ItemsSource = await getitems(DownloadsPath, 0);
            }
            else if (item.ToString() == "Pictures")
            {
                RootFileView.ItemsSource = await getitems(PicturesPath, 0);
            }
            else if (item.ToString() == "Music")
            {
                RootFileView.ItemsSource = await getitems(MusicPath, 0);
            }
            else if (item.ToString() == "Videos")
            {
                RootFileView.ItemsSource = await getitems(VideosPath, 0);
            }
            else if (item.ToString() == "Local Disk (C:\\)")
            {
                RootFileView.ItemsSource = await getitems(@"C:\", 0);
            }
            else if (item.ToString() == "OneDrive")
            {
                RootFileView.ItemsSource = await getitems(OneDrivePath, 0);
            }

            else
            {
                var tagOfInvokedItem = (nv.MenuItems[nv.MenuItems.IndexOf(itemContainer)] as NavigationViewItem).Tag;

                if (StorageFolder.GetFolderFromPathAsync(tagOfInvokedItem.ToString()) != null)
                    RootFileView.ItemsSource = await getitems(tagOfInvokedItem.ToString(), 0);
            }
        }

        private void CutButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Grid_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.P)
            {
                if (selecteditem != null)
                {
                    //await FilePropDiag.ShowAsync();
                }
            }
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                if (CurrentListView.SelectionMode == ListViewSelectionMode.Multiple)
                {
                    CurrentListView.SelectionMode = ListViewSelectionMode.Single;
                }
            }
        }

        private async void Addresspath_ItemClick(object sender, ItemClickEventArgs e)
        {
            var address = e.ClickedItem as ListViewItem;
            for (int i = 0 + 1; i < FileBladeView.Items.Count; i++)
            {
                Debug.WriteLine(i);
                FileBladeView.Items.RemoveAt(i);
                FileBladeView.ActiveBlades.RemoveAt(i);
            }
            RootFileView.ItemsSource = await getitems(address.Tag.ToString(), 0);
        }

        private async void RootFileView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count() == 1 && CurrentListView.SelectionMode != ListViewSelectionMode.Multiple)
            {
                selecteditem = e.AddedItems[0] as ListedItem;
                PreviewImage.Source = selecteditem.FileImg;
                PreviewName.Text = selecteditem.FileName;
                PreviewType.Text = selecteditem.FileType;
                var item1 = e.AddedItems[0] as ListedItem;
                try
                {
                    if (item1.FileType == "Folder")
                    {
                        if (tokenSource2 != null)
                        {
                            tokenSource2.Cancel();
                        }
                        try
                        {
                            Debug.WriteLine(item1.FilePath);
                        }
                        catch
                        {
                            Debug.WriteLine("Null!");
                        }
                        //var item = e.ClickedItem as ListViewItem;
                        ////var lv = item.FindParent<ListView>();
                        ////var type = item.ContentTemplateRoot as ListViewItem;
                        //var type = ItemsControl.ItemsControlFromItemContainer(item);
                        //Debug.WriteLine(type.Tag.ToString());
                        //var parent = lv.FindParent<BladeItem>();
                        //Debug.WriteLine(parent.Name);
                        ////var parent = item.Parent as BladeItem;
                        //var indexof = FileBladeView.Items.IndexOf(parent);
                        Debug.WriteLine(item1.tag.ToString());
                        for (int i = item1.tag + 1; i < FileBladeView.Items.Count; i++)
                        {
                            Debug.WriteLine(i);
                            FileBladeView.Items.RemoveAt(i);
                            FileBladeView.ActiveBlades.RemoveAt(i);
                        }
                        var blade = new BladeItem();
                        //FileBladeView.ActiveBlades.Add(blade);
                        blade.TitleBarVisibility = Visibility.Collapsed;
                        blade.Style = BladeStyle;

                        var newlistview = new ListView
                        {
                            ItemsSource = await getitems(item1.FilePath, item1.tag + 1),
                            ItemTemplate = Filev,
                            IsItemClickEnabled = true,
                            Tag = FileBladeView.Items.IndexOf(blade).ToString()
                        };
                        CurrentListView = newlistview;
                        newlistview.SelectionChanged += RootFileView_SelectionChanged;
                        blade.Content = newlistview;
                        FileBladeView.Items.Insert(item1.tag + 1, blade);
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

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            navView.IsPaneOpen = !navView.IsPaneOpen;
        }


        private async void OpenItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mfi = sender as MenuFlyoutItem;
                var optionitem = mfi.Tag.ToString();
                Debug.WriteLine(optionitem);
                var file = await StorageFile.GetFileFromPathAsync(optionitem);
                var launch = Launcher.LaunchFileAsync(file, new LauncherOptions { DisplayApplicationPicker = true });
            }
            catch
            {

            }
        }
        private void ShareTextHandler(DataTransferManager sender, DataRequestedEventArgs e)

        {

            DataRequest request = e.Request;

            request.Data.Properties.Title = "File Share";

            request.Data.Properties.Description = $"From {Package.Current.DisplayName}";

            request.Data.SetStorageItems(storageItems);

        }
        private async void NavigationViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (selecteditem != null && selecteditem.FileType != "Folder")
            {
                try
                {
                    var file = await StorageFile.GetFileFromPathAsync(selecteditem.FilePath);
                    var launch = Launcher.LaunchFileAsync(file, new LauncherOptions { DisplayApplicationPicker = true });
                }
                catch
                {

                    
                }
            }
        }

        private async void ShareButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (selecteditem != null && selecteditem.FileType != "Folder")
            {
                try
                {
                    var file = await StorageFile.GetFileFromPathAsync(selecteditem.FilePath);
                    var list = new List<StorageFile>();
                    list.Add(file);
                    this.storageItems = list;
                    DataTransferManager.ShowShareUI();

                }
                catch
                {

                }
            }
        }
    }
}
