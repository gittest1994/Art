using log4net;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Art
{
    public class ViewModel
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public class ImageData
        {
            public string TagName { get; set; }
            public ImageSource ImageSource { get; set; }
        }

        public class ArtistData
        {
            public string Name { get; set; }
            public string Tag { get; set; }
        }

        ObservableCollection<string> nudeData = new ObservableCollection<string>();

        public ObservableCollection<ImageData> Images { get; } = new ObservableCollection<ImageData>();

        public ObservableCollection<ArtistData> ArtistNames { get; } = new ObservableCollection<ArtistData>();

        public void loadArtists()
        {
            ArtistNames.Clear();
            foreach (var item in Directory.EnumerateDirectories(GlobalData.Config.DataPath))
            {
                ArtistNames.Add(new ArtistData { Name = item.Replace(Path.GetDirectoryName(item) + Path.DirectorySeparatorChar, "") });
            }
        }

        public void loadCity()
        {
            ArtistNames.Clear();
            var cityResource = Properties.Resources.city;
            var cityItems = cityResource.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in cityItems)
                ArtistNames.Add(new ArtistData { Name = item });
            
        }
        public void loadCountry()
        {
            ArtistNames.Clear();

            var countryResource = Properties.Resources.country;
            var countryItem = countryResource.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in countryItem)
                ArtistNames.Add(new ArtistData { Name = item });
        }

        public void loadGallery()
        {
            ArtistNames.Clear();

            var galleryResource = Properties.Resources.gallery;
            var galleryItem = galleryResource.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in galleryItem)
                ArtistNames.Add(new ArtistData { Name = item });
        }

        public void loadNude()
        {
            nudeData.Clear();

            var nudeResource = Properties.Resources.nudes;
            var nudeItem = nudeResource.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in nudeItem)
                nudeData.Add(item);
        }

        public async Task loadTitle(IProgress<int> progress, CancellationToken ct, ProgressBar prg)
        {
            ArtistNames.Clear();

            var mprogress = 0;
            prg.Value = 0;
            int totalFiles = Directory.EnumerateFiles(GlobalData.Config.DataPath, "*.jpg", SearchOption.AllDirectories).Count();

            foreach (var line in Directory.EnumerateFiles(GlobalData.Config.DataPath, "*.jpg", SearchOption.AllDirectories))
            {
                if (!ct.IsCancellationRequested)
                {
                    mprogress += 1;
                    progress.Report((mprogress * 100 / totalFiles));

                    var item = ShellFile.FromFilePath(line);

                    ArtistNames.Add(new ArtistData {
                        Name = item.Properties.System.Title.Value,
                        Tag = line
                    });
                    await Task.Delay(5);
                }
            }
        }

        public Task<BitmapImage> loadImage(string Path)
        {
            return Task.Run(() =>
            {

                var bitmap = new BitmapImage();
                using (var stream = new FileStream(Path, FileMode.Open, FileAccess.Read))
                {
                    bitmap.BeginInit();
                    bitmap.DecodePixelHeight = 300;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
                return bitmap;
            });
        }

        public async Task LoadFolder(string path, ListBox listBox, ToggleButton ButtonNude)
        {
            try
            {
                Images.Clear();

                bool isNude = false;
                dynamic selectedItem = listBox.SelectedItems[0];
                isNude = false;
                if(ButtonNude.IsChecked == true)
                {
                    foreach (var item in nudeData)
                    {
                        if (item.Equals(Path.GetFileNameWithoutExtension(path)))
                        {
                            isNude = true;
                            break;
                        }
                    }
                }
                if (isNude)
                    return;
                Images.Add(new ImageData { ImageSource = await loadImage(path), TagName = path });


            }
            catch (NullReferenceException e)
            {
                log.Error("LoadFolderTitle " + Environment.NewLine + e.Message);
            }
            catch (Exception ex)
            {
                log.Error("LoadFolderTitle " + Environment.NewLine + ex.Message);
            }
        }

        public async Task LoadFolder(CancellationToken ct, ListBox listbox, ToggleButton ButtonNude)
        {
            try
            {
                bool isNude = false;
                dynamic selectedItem = listbox.SelectedItems[0];
                foreach (var path in Directory.EnumerateFiles(GlobalData.Config.DataPath + @"\" + selectedItem.Name, "*.jpg"))
                {
                    isNude = false;

                    if (!ct.IsCancellationRequested)
                    {
                        if(ButtonNude.IsChecked == true)
                        {
                            foreach (var item in nudeData)
                            {
                                if (item.Equals(Path.GetFileNameWithoutExtension(path)))
                                {
                                    isNude = true;
                                    break;
                                }
                            }
                        }
                        if (isNude)
                            return;

                        await Dispatcher.CurrentDispatcher.InvokeAsync(async () =>
                        {
                            Images.Add(new ImageData { ImageSource = await loadImage(path), TagName = path });
                        }, DispatcherPriority.Background);
                    }
                }
            }
            catch (NullReferenceException e)
            {
                log.Error("LoadFolder " + Environment.NewLine + e.Message);
            }
            catch (Exception ex)
            {
                log.Error("LoadFolder " + Environment.NewLine + ex.Message);
            }
        }

        int TotalItem = 0;
        private async Task<FileInfo[]> GetFileListAsync(string path)
        {
            FileInfo[] allFiles = null;
            await Task.Run(() =>
            {
                var dir = new DirectoryInfo(path);
                allFiles = dir.GetFiles("*.jpg*", SearchOption.AllDirectories);
            });
            TotalItem = allFiles.Count();
            return allFiles;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="progress">IProgress for Report Task Status</param>
        /// <param name="ct">Cancellation Token</param>
        /// <param name="KeywordIndex">City = [0], Country = [1], Gallery = [2]</param>
        /// <param name="prg">ProgressBar</param>
        /// <param name="listBox">ListBox</param>
        /// <param name="ButtonNude">Button Nude</param>
        /// <returns></returns>
        public async Task LoadCategory(IProgress<int> progress, CancellationToken ct, int KeywordIndex, ProgressBar prg, ListBox listBox, ToggleButton ButtonNude)
        {
            try
            {
                Images.Clear();

                var mprogress = 0;
                prg.Value = 0;
                dynamic check;
                bool isNude = false;
                foreach (var file in await GetFileListAsync(GlobalData.Config.DataPath))
                {
                    dynamic selectedItem = listBox.SelectedItems[0];

                    isNude = false;
                    mprogress += 1;
                    progress.Report(mprogress * 100 / TotalItem);
                    if (!ct.IsCancellationRequested)
                    {
                        var item = ShellFile.FromFilePath(file.FullName);
                        if(ButtonNude.IsChecked == true)
                        {
                            foreach (var itemx in nudeData)
                            {
                                if (itemx.Equals(Path.GetFileNameWithoutExtension(file.FullName)))
                                {
                                    isNude = true;
                                    break;
                                }
                            }
                        }

                        if (isNude)
                            return;

                        if (KeywordIndex == 2)
                            check = item?.Properties.System.Comment.Value ?? "null";
                        else
                            check = item?.Properties.System.Keywords.Value?[KeywordIndex] ?? "null";

                        await Dispatcher.CurrentDispatcher.InvokeAsync(async () => {

                            if (check.Equals(selectedItem.Name))
                            {
                                Images.Add(new ImageData { ImageSource = await loadImage(file.FullName), TagName = file.FullName });
                            }

                        }, DispatcherPriority.Background);

                    }
                    

                }

            }
            catch (NullReferenceException e)
            {
                log.Error("LoadCategory " + Environment.NewLine + e.Message);
            }
            catch (Exception ex)
            {
                log.Error("LoadCategory " + Environment.NewLine + ex.Message);
            }
        }
    }
}
