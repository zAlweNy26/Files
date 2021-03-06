﻿using Files.Filesystem;
using Files.Helpers;
using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace Files.Dialogs
{
    public sealed partial class AddItemDialog : ContentDialog
    {
        public AddItemDialog()
        {
            InitializeComponent();
            AddItemsToList();
        }

        public List<AddListItem> AddItemsList = new List<AddListItem>();

        public void AddItemsToList()
        {
            AddItemsList.Clear();

            AddItemsList.Add(new AddListItem
            {
                Header = ResourceController.GetTranslation("AddDialogListFolderHeader"),
                SubHeader = ResourceController.GetTranslation("AddDialogListFolderSubHeader"),
                Icon = "\xE838",
                IsItemEnabled = true,
                ItemType = AddItemType.Folder
            });

            AddItemsList.Add(new AddListItem
            {
                Header = ResourceController.GetTranslation("AddDialogListTextFileHeader"),
                SubHeader = ResourceController.GetTranslation("AddDialogListTextFileSubHeader"),
                Icon = "\xE8A5",
                IsItemEnabled = true,
                ItemType = AddItemType.TextDocument
            });
            AddItemsList.Add(new AddListItem
            {
                Header = ResourceController.GetTranslation("AddDialogListBitmapHeader"),
                SubHeader = ResourceController.GetTranslation("AddDialogListBitmapSubHeader"),
                Icon = "\xEB9F",
                IsItemEnabled = true,
                ItemType = AddItemType.BitmapImage
            });
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Hide();
            CreateFile((e.ClickedItem as AddListItem).ItemType);
        }

        public static async void CreateFile(AddItemType itemType)
        {
            string currentPath = null;
            if (App.CurrentInstance.ContentPage != null)
            {
                currentPath = App.CurrentInstance.FilesystemViewModel.WorkingDirectory;
            }

            StorageFolderWithPath folderWithPath = await ItemViewModel.GetFolderWithPathFromPathAsync(currentPath);
            StorageFolder folderToCreateItem = folderWithPath.Folder;

            // Show rename dialog
            RenameDialog renameDialog = new RenameDialog();
            var renameResult = await renameDialog.ShowAsync();
            if (renameResult != ContentDialogResult.Primary)
            {
                return;
            }

            // Create file based on dialog result
            string userInput = renameDialog.storedRenameInput;
            try
            {
                switch (itemType)
                {
                    case AddItemType.Folder:
                        userInput = !string.IsNullOrWhiteSpace(userInput) ? userInput : ResourceController.GetTranslation("NewFolder");
                        await folderToCreateItem.CreateFolderAsync(userInput, CreationCollisionOption.GenerateUniqueName);
                        break;

                    case AddItemType.TextDocument:
                        userInput = !string.IsNullOrWhiteSpace(userInput) ? userInput : ResourceController.GetTranslation("NewTextDocument");
                        await folderToCreateItem.CreateFileAsync(userInput + ".txt", CreationCollisionOption.GenerateUniqueName);
                        break;

                    case AddItemType.BitmapImage:
                        userInput = !string.IsNullOrWhiteSpace(userInput) ? userInput : ResourceController.GetTranslation("NewBitmapImage");
                        await folderToCreateItem.CreateFileAsync(userInput + ".bmp", CreationCollisionOption.GenerateUniqueName);
                        break;
                }
            }
            catch (UnauthorizedAccessException)
            {
                await DialogDisplayHelper.ShowDialog(ResourceController.GetTranslation("AccessDeniedCreateDialog/Title"), ResourceController.GetTranslation("AccessDeniedCreateDialog/Text"));
            }
        }
    }

    public enum AddItemType
    {
        Folder = 0,
        TextDocument = 1,
        BitmapImage = 2,
        CompressedArchive = 3
    }

    public class AddListItem
    {
        public string Header { get; set; }
        public string SubHeader { get; set; }
        public string Icon { get; set; }
        public bool IsItemEnabled { get; set; }
        public AddItemType ItemType { get; set; }
    }
}