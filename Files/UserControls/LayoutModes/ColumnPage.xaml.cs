using Files.Filesystem;
using Files.Interacts;
using Files.UserControls;
using Files.UserControls.LayoutModes;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Files
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ColumnPage : IShellPage
    {
        public static Microsoft.UI.Xaml.Controls.NavigationView nv;

        Frame IShellPage.ContentFrame => rootItemFrame;

        Interaction IShellPage.InteractionOperations => interactionOperation;

        ItemViewModel IShellPage.ViewModel => viewModel;

        BaseLayout IShellPage.ContentPage => GetContentOrNull();

        Control IShellPage.OperationsControl => null;

        Type IShellPage.CurrentPageType => rootItemFrame.SourcePageType;

        INavigationControlItem IShellPage.SidebarSelectedItem { get => nv.SelectedItem as INavigationControlItem; set => nv.SelectedItem = value; }

        INavigationToolbar IShellPage.NavigationToolbar => simpleToolbar;


        public ColumnPage()
        {
            this.InitializeComponent();

            nv = navView;
            App.CurrentInstance = this as IShellPage;
            App.CurrentInstance.NavigationToolbar.PathControlDisplayText = "New tab";
            App.CurrentInstance.NavigationToolbar.CanGoBack = false;
            App.CurrentInstance.NavigationToolbar.CanGoForward = false;
        }

        private BaseLayout GetContentOrNull()
        {
            if ((rootItemFrame.Content as BaseLayout) != null)
            {
                return rootItemFrame.Content as BaseLayout;
            }
            else
            {
                return null;
            }
        }

        private ItemViewModel viewModel = null;
        private Interaction interactionOperation = null;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel = new ItemViewModel();
            interactionOperation = new Interaction();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
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

        private async void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            //(App.CurrentInstance.OperationsControl as RibbonArea).RibbonViewModel.HomeItems.isEnabled = false;
            //(App.CurrentInstance.OperationsControl as RibbonArea).RibbonViewModel.ShareItems.isEnabled = false;
            string NavigationPath = ""; // path to navigate

            if (args.InvokedItem == null)
            {
                return;
            }

            switch ((args.InvokedItemContainer.DataContext as INavigationControlItem).ItemType)
            {
                case NavigationControlItemType.Location:
                    {
                        var ItemPath = (args.InvokedItemContainer.DataContext as INavigationControlItem).Path; // Get the path of the invoked item

                        if (ItemPath.Equals("Home", StringComparison.OrdinalIgnoreCase)) // Home item
                        {
                            App.CurrentInstance.ContentFrame.Navigate(typeof(YourHome), "New tab", new SuppressNavigationTransitionInfo());

                            return; // cancel so it doesn't try to Navigate to a path
                        }
                        else // Any other item
                        {
                            NavigationPath = args.InvokedItemContainer.Tag.ToString();
                        }

                        break;
                    }
                case NavigationControlItemType.OneDrive:
                    {
                        NavigationPath = App.AppSettings.OneDrivePath;
                        break;
                    }
                default:
                    {
                        var clickedItem = args.InvokedItemContainer;

                        NavigationPath = clickedItem.Tag.ToString();

                        App.CurrentInstance.NavigationToolbar.PathControlDisplayText = clickedItem.Tag.ToString();

                        break;
                    }
            }

            App.InteractionViewModel.IsPageTypeNotHome = true; // show controls that were hidden on the home page
            App.CurrentInstance.NavigationToolbar.PathControlDisplayText = App.CurrentInstance.ViewModel.WorkingDirectory;
            App.CurrentInstance.ContentFrame.Navigate(typeof(ColumnLayoutView), NavigationPath, new SuppressNavigationTransitionInfo());
        }


        private void Button_Click_2(object sender, RoutedEventArgs e) => navView.IsPaneOpen = !navView.IsPaneOpen;

        private void NavigationViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (App.CurrentInstance.ContentPage.IsItemSelected && App.CurrentInstance.ContentPage.SelectedItem.PrimaryItemAttribute == StorageItemTypes.File)
            {
                interactionOperation.OpenItem_Click(null, null);
            }
        }

    }
}
