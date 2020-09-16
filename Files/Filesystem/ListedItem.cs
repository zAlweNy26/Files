using ByteSizeLib;
using Files.Enums;
using Files.Helpers;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;
using static Files.Helpers.NativeFindStorageItemHelper;
using FileAttributes = System.IO.FileAttributes;

namespace Files.Filesystem
{
    public class ListedItem : ObservableObject
    {
        public StorageItemTypes PrimaryItemAttribute { get; set; }
        public bool ItemPropertiesInitialized { get; set; } = false;
        public string FolderTooltipText { get; set; }
        public string FolderRelativeId { get; set; }
        public bool LoadFolderGlyph { get; set; }
        public bool ContainsFilesOrFolders { get; set; }
        private bool _LoadFileIcon;

        public Uri FolderIconSource
        {
            get
            {
                return ContainsFilesOrFolders ? new Uri("ms-appx:///Assets/FolderIcon2.svg") : new Uri("ms-appx:///Assets/FolderIcon.svg");
            }
        }

        public Uri FolderIconSourceLarge
        {
            get
            {
                return ContainsFilesOrFolders ? new Uri("ms-appx:///Assets/FolderIcon2Large.svg") : new Uri("ms-appx:///Assets/FolderIconLarge.svg");
            }
        }

        public bool LoadFileIcon
        {
            get => _LoadFileIcon;
            set => SetProperty(ref _LoadFileIcon, value);
        }

        private bool _LoadUnknownTypeGlyph;

        public bool LoadUnknownTypeGlyph
        {
            get => _LoadUnknownTypeGlyph;
            set => SetProperty(ref _LoadUnknownTypeGlyph, value);
        }

        private bool _IsDimmed;

        public bool IsDimmed
        {
            get => _IsDimmed;
            set => SetProperty(ref _IsDimmed, value);
        }

        private CloudDriveSyncStatusUI _SyncStatusUI;

        public CloudDriveSyncStatusUI SyncStatusUI
        {
            get => _SyncStatusUI;
            set => SetProperty(ref _SyncStatusUI, value);
        }

        private BitmapImage _FileImage;

        public BitmapImage FileImage
        {
            get => _FileImage;
            set
            {
                if (value != null)
                {
                    SetProperty(ref _FileImage, value);
                }
            }
        }

        private string _ItemPath;

        public string ItemPath
        {
            get => _ItemPath;
            set => SetProperty(ref _ItemPath, value);
        }

        private string _ItemName;

        public string ItemName
        {
            get => _ItemName;
            set => SetProperty(ref _ItemName, value);
        }

        private string _ItemType;

        public string ItemType
        {
            get => _ItemType;
            set
            {
                if (value != null)
                {
                    SetProperty(ref _ItemType, value);
                }
            }
        }

        public string FileExtension { get; set; }
        public string FileSize { get; set; }
        public long FileSizeBytes { get; set; }

        public string ItemDateModified { get; private set; }
        public string ItemDateCreated { get; private set; }
        public string ItemDateAccessed { get; private set; }

        public DateTimeOffset ItemDateModifiedReal
        {
            get => _itemDateModifiedReal;
            set
            {
                ItemDateModified = GetFriendlyDate(value);
                _itemDateModifiedReal = value;
            }
        }

        private DateTimeOffset _itemDateModifiedReal;

        public DateTimeOffset ItemDateCreatedReal
        {
            get => _itemDateCreatedReal;
            set
            {
                ItemDateCreated = GetFriendlyDate(value);
                _itemDateCreatedReal = value;
            }
        }

        private DateTimeOffset _itemDateCreatedReal;

        public DateTimeOffset ItemDateAccessedReal
        {
            get => _itemDateAccessedReal;
            set
            {
                ItemDateAccessed = GetFriendlyDate(value);
                _itemDateAccessedReal = value;
            }
        }

        private DateTimeOffset _itemDateAccessedReal;

        public ListedItem(string folderRelativeId)
        {
            FolderRelativeId = folderRelativeId;
        }

        public static string GetFriendlyDate(DateTimeOffset d)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            var elapsed = DateTimeOffset.Now - d;

            string returnformat = Enum.Parse<TimeStyle>(localSettings.Values[LocalSettings.DateTimeFormat].ToString()) == TimeStyle.Application ? "D" : "g";

            if (elapsed.TotalDays > 7)
            {
                return d.ToString(returnformat);
            }
            else if (elapsed.TotalDays > 2)
            {
                return string.Format(ResourceController.GetTranslation("DaysAgo"), elapsed.Days);
            }
            else if (elapsed.TotalDays > 1)
            {
                return string.Format(ResourceController.GetTranslation("DayAgo"), elapsed.Days);
            }
            else if (elapsed.TotalHours > 2)
            {
                return string.Format(ResourceController.GetTranslation("HoursAgo"), elapsed.Hours);
            }
            else if (elapsed.TotalHours > 1)
            {
                return string.Format(ResourceController.GetTranslation("HourAgo"), elapsed.Hours);
            }
            else if (elapsed.TotalMinutes > 2)
            {
                return string.Format(ResourceController.GetTranslation("MinutesAgo"), elapsed.Minutes);
            }
            else if (elapsed.TotalMinutes > 1)
            {
                return string.Format(ResourceController.GetTranslation("MinuteAgo"), elapsed.Minutes);
            }
            else
            {
                return string.Format(ResourceController.GetTranslation("SecondsAgo"), elapsed.Seconds);
            }
        }

        public bool IsRecycleBinItem => this is RecycleBinItem;
        public bool IsShortcutItem => this is ShortcutItem;
        public bool IsLinkItem => IsShortcutItem && ((ShortcutItem)this).IsUrl;

        public static ListedItem GetItemRepresentation(WIN32_FIND_DATA findData, string pathRoot = null)
        {
            if (((FileAttributes)findData.dwFileAttributes & FileAttributes.Hidden) != FileAttributes.Hidden && ((FileAttributes)findData.dwFileAttributes & FileAttributes.System) != FileAttributes.System)
            {
                if (((FileAttributes)findData.dwFileAttributes & FileAttributes.Directory) != FileAttributes.Directory)
                {
                    return GetListedItemFromFile(findData, pathRoot);
                }
                else if (((FileAttributes)findData.dwFileAttributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    if (findData.cFileName != "." && findData.cFileName != "..")
                    {
                        return GetListedItemFromFolder(findData, pathRoot);
                    }
                }
            }
            return null;
        }

        public static async Task<ListedItem> GetItemRepresentation(IStorageItem item, string pathRoot, Type PageType = null)
        {
            if (item.IsOfType(StorageItemTypes.Folder))
            {
                return await GetListedItemFromFolder(item as StorageFolder, pathRoot);
            }
            else
            {
                return await GetListedItemFromFile(item as StorageFile, pathRoot, true, PageType);
            }
        }

        private static ListedItem GetListedItemFromFile(WIN32_FIND_DATA findData, string pathRoot)
        {
            var itemPath = Path.Combine(pathRoot, findData.cFileName);

            string itemName;
            if (App.AppSettings.ShowFileExtensions && !findData.cFileName.EndsWith(".lnk") && !findData.cFileName.EndsWith(".url"))
            {
                itemName = findData.cFileName; // never show extension for shortcuts
            }
            else
            {
                if (findData.cFileName.StartsWith("."))
                {
                    itemName = findData.cFileName; // Always show full name for dotfiles.
                }
                else
                {
                    itemName = Path.GetFileNameWithoutExtension(itemPath);
                }
            }

            FileTimeToSystemTime(ref findData.ftLastWriteTime, out SYSTEMTIME systemModifiedDateOutput);
            var itemModifiedDate = new DateTime(
                systemModifiedDateOutput.Year, systemModifiedDateOutput.Month, systemModifiedDateOutput.Day,
                systemModifiedDateOutput.Hour, systemModifiedDateOutput.Minute, systemModifiedDateOutput.Second, systemModifiedDateOutput.Milliseconds,
                DateTimeKind.Utc);

            FileTimeToSystemTime(ref findData.ftCreationTime, out SYSTEMTIME systemCreatedDateOutput);
            var itemCreatedDate = new DateTime(
                systemCreatedDateOutput.Year, systemCreatedDateOutput.Month, systemCreatedDateOutput.Day,
                systemCreatedDateOutput.Hour, systemCreatedDateOutput.Minute, systemCreatedDateOutput.Second, systemCreatedDateOutput.Milliseconds,
                DateTimeKind.Utc);

            FileTimeToSystemTime(ref findData.ftLastAccessTime, out SYSTEMTIME systemLastAccessOutput);
            var itemLastAccessDate = new DateTime(
                systemLastAccessOutput.Year, systemLastAccessOutput.Month, systemLastAccessOutput.Day,
                systemLastAccessOutput.Hour, systemLastAccessOutput.Minute, systemLastAccessOutput.Second, systemLastAccessOutput.Milliseconds,
                DateTimeKind.Utc);

            long itemSizeBytes = findData.GetSize();
            var itemSize = ByteSize.FromBytes(itemSizeBytes).ToBinaryString().ConvertSizeAbbreviation();
            string itemType = ResourceController.GetTranslation("ItemTypeFile");
            string itemFileExtension = null;

            if (findData.cFileName.Contains('.'))
            {
                itemFileExtension = Path.GetExtension(itemPath);
                itemType = itemFileExtension.Trim('.') + " " + itemType;
            }

            bool itemFolderImgVis = false;
            BitmapImage icon = new BitmapImage();
            bool itemThumbnailImgVis;
            bool itemEmptyImgVis;

            itemEmptyImgVis = true;
            itemThumbnailImgVis = false;

            if (findData.cFileName.EndsWith(".lnk") || findData.cFileName.EndsWith(".url"))
            {
                if (App.Connection != null)
                {
                    var response = App.Connection.SendMessageAsync(new ValueSet() {
                        { "Arguments", "FileOperation" },
                        { "fileop", "ParseLink" },
                        { "filepath", itemPath } }).AsTask().Result;

                    if (response.Status == Windows.ApplicationModel.AppService.AppServiceResponseStatus.Success
                        && response.Message.ContainsKey("TargetPath"))
                    {
                        var isUrl = findData.cFileName.EndsWith(".url");
                        string target = (string)response.Message["TargetPath"];
                        bool containsFilesOrFolders = false;

                        if ((bool)response.Message["IsFolder"])
                        {
                            containsFilesOrFolders = CheckForFilesFolders(target);
                        }

                        return new ShortcutItem(null)
                        {
                            PrimaryItemAttribute = (bool)response.Message["IsFolder"] ? StorageItemTypes.Folder : StorageItemTypes.File,
                            FileExtension = itemFileExtension,
                            FileImage = !(bool)response.Message["IsFolder"] ? icon : null,
                            LoadFileIcon = !(bool)response.Message["IsFolder"] && itemThumbnailImgVis,
                            LoadUnknownTypeGlyph = !(bool)response.Message["IsFolder"] && !isUrl && itemEmptyImgVis,
                            LoadFolderGlyph = (bool)response.Message["IsFolder"],
                            ItemName = itemName,
                            ItemDateModifiedReal = itemModifiedDate,
                            ItemDateAccessedReal = itemLastAccessDate,
                            ItemDateCreatedReal = itemCreatedDate,
                            ItemType = ResourceController.GetTranslation(isUrl ? "ShortcutWebLinkFileType" : "ShortcutFileType"),
                            ItemPath = itemPath,
                            FileSize = itemSize,
                            FileSizeBytes = itemSizeBytes,
                            TargetPath = target,
                            Arguments = (string)response.Message["Arguments"],
                            WorkingDirectory = (string)response.Message["WorkingDirectory"],
                            RunAsAdmin = (bool)response.Message["RunAsAdmin"],
                            IsUrl = isUrl,
                            ContainsFilesOrFolders = containsFilesOrFolders
                        };
                    }
                }
            }
            else
            {
                return new ListedItem(null)
                {
                    PrimaryItemAttribute = StorageItemTypes.File,
                    FileExtension = itemFileExtension,
                    LoadUnknownTypeGlyph = itemEmptyImgVis,
                    FileImage = icon,
                    LoadFileIcon = itemThumbnailImgVis,
                    LoadFolderGlyph = itemFolderImgVis,
                    ItemName = itemName,
                    ItemDateModifiedReal = itemModifiedDate,
                    ItemDateAccessedReal = itemLastAccessDate,
                    ItemDateCreatedReal = itemCreatedDate,
                    ItemType = itemType,
                    ItemPath = itemPath,
                    FileSize = itemSize,
                    FileSizeBytes = itemSizeBytes
                };
            }
            return null;
        }

        private static ListedItem GetListedItemFromFolder(WIN32_FIND_DATA findData, string pathRoot)
        {
            FileTimeToSystemTime(ref findData.ftLastWriteTime, out SYSTEMTIME systemTimeOutput);
            var itemDate = new DateTime(
                systemTimeOutput.Year,
                systemTimeOutput.Month,
                systemTimeOutput.Day,
                systemTimeOutput.Hour,
                systemTimeOutput.Minute,
                systemTimeOutput.Second,
                systemTimeOutput.Milliseconds,
                DateTimeKind.Utc);
            var itemPath = Path.Combine(pathRoot, findData.cFileName);

            return new ListedItem(null)
            {
                PrimaryItemAttribute = StorageItemTypes.Folder,
                ItemName = findData.cFileName,
                ItemDateModifiedReal = itemDate,
                ItemType = ResourceController.GetTranslation("FileFolderListItem"),
                LoadFolderGlyph = true,
                FileImage = null,
                LoadFileIcon = false,
                ItemPath = itemPath,
                LoadUnknownTypeGlyph = false,
                FileSize = null,
                FileSizeBytes = 0,
                ContainsFilesOrFolders = CheckForFilesFolders(itemPath)
                //FolderTooltipText = tooltipString,
            };
        }

        private static async Task<ListedItem> GetListedItemFromFolder(StorageFolder folder, string pathRoot)
        {
            var basicProperties = await folder.GetBasicPropertiesAsync();

            return new ListedItem(folder.FolderRelativeId)
            {
                PrimaryItemAttribute = StorageItemTypes.Folder,
                ItemName = folder.Name,
                ItemDateModifiedReal = basicProperties.DateModified,
                ItemType = folder.DisplayType,
                LoadFolderGlyph = true,
                FileImage = null,
                LoadFileIcon = false,
                ItemPath = string.IsNullOrEmpty(folder.Path) ? Path.Combine(pathRoot, folder.Name) : folder.Path,
                LoadUnknownTypeGlyph = false,
                FileSize = null,
                FileSizeBytes = 0
            };
        }

        private static async Task<ListedItem> GetListedItemFromFile(StorageFile file, string pathRoot, bool suppressThumbnailLoading = false, Type PageType = null)
        {
            var basicProperties = await file.GetBasicPropertiesAsync();

            // Display name does not include extension
            var itemName = string.IsNullOrEmpty(file.DisplayName) || App.AppSettings.ShowFileExtensions ?
                file.Name : file.DisplayName;
            var itemDate = basicProperties.DateModified;
            var itemPath = string.IsNullOrEmpty(file.Path) ? Path.Combine(pathRoot, file.Name) : file.Path;
            var itemSize = ByteSize.FromBytes(basicProperties.Size).ToBinaryString().ConvertSizeAbbreviation();
            var itemSizeBytes = basicProperties.Size;
            var itemType = file.DisplayType;
            var itemFolderImgVis = false;
            var itemFileExtension = file.FileType;

            BitmapImage icon = new BitmapImage();
            bool itemThumbnailImgVis;
            bool itemEmptyImgVis;

            if (!(PageType == typeof(GridViewBrowser)))
            {
                try
                {
                    var itemThumbnailImg = suppressThumbnailLoading ? null :
                        await file.GetThumbnailAsync(ThumbnailMode.ListView, 40, ThumbnailOptions.UseCurrentScale);
                    if (itemThumbnailImg != null)
                    {
                        itemEmptyImgVis = false;
                        itemThumbnailImgVis = true;
                        icon.DecodePixelWidth = 40;
                        icon.DecodePixelHeight = 40;
                        await icon.SetSourceAsync(itemThumbnailImg);
                    }
                    else
                    {
                        itemEmptyImgVis = true;
                        itemThumbnailImgVis = false;
                    }
                }
                catch
                {
                    itemEmptyImgVis = true;
                    itemThumbnailImgVis = false;
                    // Catch here to avoid crash
                }
            }
            else
            {
                try
                {
                    var itemThumbnailImg = suppressThumbnailLoading ? null :
                        await file.GetThumbnailAsync(ThumbnailMode.ListView, 80, ThumbnailOptions.UseCurrentScale);
                    if (itemThumbnailImg != null)
                    {
                        itemEmptyImgVis = false;
                        itemThumbnailImgVis = true;
                        icon.DecodePixelWidth = 80;
                        icon.DecodePixelHeight = 80;
                        await icon.SetSourceAsync(itemThumbnailImg);
                    }
                    else
                    {
                        itemEmptyImgVis = true;
                        itemThumbnailImgVis = false;
                    }
                }
                catch
                {
                    itemEmptyImgVis = true;
                    itemThumbnailImgVis = false;
                }
            }

            if (file.Name.EndsWith(".lnk") || file.Name.EndsWith(".url"))
            {
                // This shouldn't happen, StorageFile api does not support shortcuts
                Debug.WriteLine("Something strange: StorageFile api returned a shortcut");
            }
            else
            {
                return new ListedItem(file.FolderRelativeId)
                {
                    PrimaryItemAttribute = StorageItemTypes.File,
                    FileExtension = itemFileExtension,
                    LoadUnknownTypeGlyph = itemEmptyImgVis,
                    FileImage = icon,
                    LoadFileIcon = itemThumbnailImgVis,
                    LoadFolderGlyph = itemFolderImgVis,
                    ItemName = itemName,
                    ItemDateModifiedReal = itemDate,
                    ItemType = itemType,
                    ItemPath = itemPath,
                    FileSize = itemSize,
                    FileSizeBytes = (long)itemSizeBytes
                };
            }

            return null;
        }


        /// <summary>
        /// This function is used to determine whether or not a folder has any contents.
        /// </summary>
        /// <param name="targetPath">The path to the target folder</param>
        ///
        private static bool CheckForFilesFolders(string targetPath)
        {
            FINDEX_INFO_LEVELS findInfoLevel = FINDEX_INFO_LEVELS.FindExInfoBasic;
            int additionalFlags = FIND_FIRST_EX_LARGE_FETCH;

            IntPtr hFile = FindFirstFileExFromApp(targetPath + "\\*.*", findInfoLevel, out WIN32_FIND_DATA _, FINDEX_SEARCH_OPS.FindExSearchNameMatch, IntPtr.Zero, additionalFlags);
            FindNextFile(hFile, out _);
            var result = FindNextFile(hFile, out _);
            FindClose(hFile);
            return result;
        }
    }

    public class RecycleBinItem : ListedItem
    {
        public RecycleBinItem(string folderRelativeId) : base(folderRelativeId)
        {
        }

        // For recycle bin elements (path + name)
        public string ItemOriginalPath { get; set; }
    }

    public class ShortcutItem : ListedItem
    {
        public ShortcutItem(string folderRelativeId) : base(folderRelativeId)
        {
        }

        // For shortcut elements (.lnk and .url)
        public string TargetPath { get; set; }

        public string Arguments { get; set; }
        public string WorkingDirectory { get; set; }
        public bool RunAsAdmin { get; set; }
        public bool IsUrl { get; set; }
    }
}