using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

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


    }
}
