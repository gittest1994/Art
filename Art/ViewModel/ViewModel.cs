using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Art
{
    public class ViewModel
    {
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
    }
}
