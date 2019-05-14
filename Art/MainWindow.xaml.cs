using HandyControl.Controls;
using HandyControl.Data;
using log4net;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Art
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : BlurWindow
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        CancellationTokenSource ts = new CancellationTokenSource();

        internal static MainWindow main;
        public MainWindow()
        {
            InitializeComponent();

            main = this;

            log4net.Config.XmlConfigurator.Configure();

            AppDomain appDomain = AppDomain.CurrentDomain;
            appDomain.UnhandledException += AppDomain_UnhandledException;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listbox.ItemsSource);
            view.Filter = UseFilter;

            ((ViewModel)DataContext).loadArtists();
            ((ViewModel)DataContext).loadNude();
        }

        private bool UseFilter(object obj)
        {
            if (string.IsNullOrEmpty(txtSearch.Text))
                return true;
            else
                return ((obj as ViewModel.ArtistData).Name.IndexOf(txtSearch.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void AppDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            log.Error(e.ExceptionObject);
        }
        private void TxtSearch_SearchStarted(object sender, HandyControl.Data.FunctionEventArgs<string> e)
        {
            if (string.IsNullOrEmpty(txtSearch.Text)) return;

            CollectionViewSource.GetDefaultView(listbox.ItemsSource).Refresh();
        }
        private void StackPanel_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button && button.Tag is SkinType tag)
            {
                popupConfig.IsOpen = false;
                if (tag.Equals(GlobalData.Config.Skin)) return;
                GlobalData.Config.Skin = tag;
                GlobalData.Save();
                ((App)Application.Current).UpdateSkin(tag);
            }
        }

        private void setNudeStyle(bool IsChecked)
        {
            Style style;
            if (IsChecked)
                style = TryFindResource("ToggleButtonDanger") as Style;
            else
                style = TryFindResource("ToggleButtonPrimary") as Style;

            ButtonNude.Style = style;
        }
        private void ButtonConfig_Click(object sender, RoutedEventArgs e)
        {
            popupConfig.IsOpen = true;
        }

        private void CoverViewItem_Selected(object sender, RoutedEventArgs e)
        {
            var selectedCover = sender as CoverViewItem;
            ContentPresenter content = FindVisualChild<ContentPresenter>(selectedCover);

            DataTemplate data = content.ContentTemplate;
            Image selectedImg = (Image)data.FindName("ImageHeader", content);

            var file = ShellFile.FromFilePath(selectedImg.Tag.ToString());
            try
            {
                var country = string.Empty;
                if (file.Properties.System.Keywords.Value[1].Equals("Empty"))
                    country = "Location Unknown";
                else
                    country = file.Properties.System.Keywords.Value[1];

                shArtist.Status = file.Properties.System.Subject.Value;
                shTitle.Status = file.Properties.System.Title.Value;
                shCountry.Status = country;
                shCity.Status = file.Properties.System.Keywords.Value[0];
                shGallery.Status = file.Properties.System.Comment.Value;
                shDate.Status = file.Properties.System.Keywords.Value[9] ?? file.Properties.System.Keywords.Value[8];

            }
            catch (IndexOutOfRangeException)
            {

            }

        }

        private void CoverViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ObservableCollection<ViewModel.ImageData> items = cover.ItemsSource as ObservableCollection<ViewModel.ImageData>;

            ImageViewer.Items = items;
            new ImageViewer().ShowDialog();
        }

        private void MnuConfig_Click(object sender, RoutedEventArgs e)
        {
            var browser = new CommonOpenFileDialog();
            browser.IsFolderPicker = true;
            browser.InitialDirectory = GlobalData.Config.DataPath;

            if(browser.ShowDialog() == CommonFileDialogResult.Ok)
            {
                GlobalData.Config.DataPath = browser.FileName;
                GlobalData.Save();
            }
        }

        private void ButtonNude_Checked(object sender, RoutedEventArgs e)
        {
            setNudeStyle((bool)ButtonNude.IsChecked);
        }

        private async void CbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (txtSearch != null && !string.IsNullOrEmpty(txtSearch.Text)) txtSearch.Text = string.Empty;
            switch (cbFilter.SelectedIndex)
            {
                case 0:
                    ((ViewModel)DataContext).loadArtists();
                    break;
                case 1:
                    ((ViewModel)DataContext).loadCountry();
                    break;
                case 2:
                    ((ViewModel)DataContext).loadCity();
                    break;
                case 3:
                    ((ViewModel)DataContext).loadGallery();
                    break;
                case 4:
                    GC.Collect();
                    var progressTitle = new Progress<int>(percent =>
                    {
                        prg.Value = percent;
                    });
                    ts?.Cancel();
                    ts = new CancellationTokenSource();
                    await ((ViewModel)DataContext).loadTitle(progressTitle, ts.Token, prg);
                    break;
                default:
                    break;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var info = sender as MenuItem;
            if (info.Header.Equals("Set as Desktop"))
                SetDesktopWallpaper(info.Tag.ToString(), true);
            else if (info.Header.Equals("Go to Location"))
                Process.Start("explorer.exe", "/select, \"" + info.Tag + "\"");
            else if (info.Header.Equals("Full Screen"))
            {
                var imgBrowser = new ImageBrowser(new Uri(info.Tag.ToString(), UriKind.Absolute));
                imgBrowser.ResizeMode = ResizeMode.CanResize;
                imgBrowser.Show();
            }
            else if(info.Header.Equals("Add To Favorite"))
            {
                if (File.Exists("fav.txt"))
                {
                    var lines = File.ReadAllLines("fav.txt").Any(x => x.Equals(info.Tag.ToString().Trim()));
                    if (!lines)
                    {
                        File.AppendAllText("fav.txt", info.Tag.ToString().Trim() + Environment.NewLine);
                        Growl.Info(new GrowlInfo
                        {
                            Message = "Added to Favorite", ConfirmStr = "Confirm", ShowDateTime = false
                        });
                    }
                    else
                    {
                        Growl.Warning(new GrowlInfo
                        {
                            Message = "This image exist in favorite",
                            ConfirmStr = "Confirm",
                            ShowDateTime = false
                        });
                    }
                }
            } else if (info.Header.Equals("Remove From Favorite"))
            {
                var lines = File.ReadAllLines("fav.txt").ToList();
                lines.Remove(info.Tag.ToString().Trim());
                File.WriteAllLines("fav.txt", lines.ToArray());
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SystemParametersInfo(uint uiAction, uint uiParam, String pvParam, uint fWinIni);

        private const uint SPI_SETDESKWALLPAPER = 0x14;
        private const uint SPIF_UPDATEINIFILE = 0x1;
        private const uint SPIF_SENDWININICHANGE = 0x2;
        public void SetDesktopWallpaper(string file_name, bool update_registry)
        {
            try
            {
                // If we should update the registry,
                // set the appropriate flags.
                uint flags = 0;
                if (update_registry)
                    flags = SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE;

                // Set the desktop background to this file.
                if (!SystemParametersInfo(SPI_SETDESKWALLPAPER,
                    0, file_name, flags))
                {
                    HandyControl.Controls.MessageBox.Error("SystemParametersInfo failed.", "Error");
                }
            }
            catch (Exception)
            {
                HandyControl.Controls.MessageBox.Error("Error displaying picture ", "Error");
            }
        }

        private void CancelTaskButton_Click(object sender, RoutedEventArgs e)
        {
            ts?.Cancel();
            Growl.Info(new GrowlInfo
            {
                Message = "Task Canceled!",
                ShowDateTime = false,
                ShowCloseButton = false
            });
        }

        private async void Listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listbox.SelectedIndex == -1) return;
            switch (cbFilter.SelectedIndex)
            {
                case 0:
                    GC.Collect();

                    ts?.Cancel();
                    ts = new CancellationTokenSource();
                    await ((ViewModel)DataContext).LoadFolder(ts.Token, listbox, ButtonNude);
                    break;
                case 1:
                    GC.Collect();

                    var progressCountry = new Progress<int>(percent =>
                    {
                        prg.Value = percent;
                    });
                    ts?.Cancel();
                    ts = new CancellationTokenSource();
                    await ((ViewModel)DataContext).LoadCategory(progressCountry,ts.Token,1,prg,listbox,ButtonNude);
                    break;
                case 2:
                    GC.Collect();

                    var progressCity = new Progress<int>(percent =>
                    {
                        prg.Value = percent;
                    });
                    ts?.Cancel();
                    ts = new CancellationTokenSource();
                    await ((ViewModel)DataContext).LoadCategory(progressCity, ts.Token, 0, prg, listbox, ButtonNude);
                    break;
                case 3:
                    GC.Collect();

                    var progressGallery = new Progress<int>(percent =>
                    {
                        prg.Value = percent;
                    });
                    ts?.Cancel();
                    ts = new CancellationTokenSource();
                    await ((ViewModel)DataContext).LoadCategory(progressGallery, ts.Token, 2, prg, listbox, ButtonNude);
                    break;
                case 4:
                    dynamic selectedItem = listbox.SelectedItems[0];
                    await ((ViewModel)DataContext).LoadFolder(selectedItem.Tag, listbox, ButtonNude);
                    break;
            }
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private async void Tab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(tab.SelectedIndex == 1)
            {
                GC.Collect();
                ts?.Cancel();
                ts = new CancellationTokenSource();
                await((ViewModel)DataContext).LoadFavorite(ts.Token);
            }
        }
    }
}
