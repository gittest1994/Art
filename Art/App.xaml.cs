using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Windows;

namespace Art
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(App));

        protected override void OnStartup(StartupEventArgs e)
        {
            GlobalData.Init();

            log4net.Config.XmlConfigurator.Configure();

            log.Info("     ============ Strated Logging ===========     ");

            BlurWindow.SystemVersionInfo = GetSystemVersionInfo();

            if (GlobalData.Config.Skin != SkinType.Default)
                UpdateSkin(GlobalData.Config.Skin);

            if (!File.Exists("fav.txt"))
                File.AppendText("fav.txt");

            if (!Directory.Exists(GlobalData.Config.DataPath))
                GlobalData.Config.DataPath = Environment.CurrentDirectory + @"\data";

            if (!Directory.Exists(Environment.CurrentDirectory + @"\data"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\data");

            var junkFile = Directory.EnumerateFiles(GlobalData.Config.DataPath);
            foreach (var item in junkFile)
                File.Delete(item);


            base.OnStartup(e);
        }

        public void UpdateSkin(SkinType skin)
        {
            var skin0 = Resources.MergedDictionaries[0];
            skin0.MergedDictionaries.Clear();
            skin0.MergedDictionaries.Add(ResourceHelper.GetSkin(skin));
            Current.MainWindow?.OnApplyTemplate();
        }

        public static SystemVersionInfo GetSystemVersionInfo()
        {
            var managementClass = new ManagementClass("Win32_OperatingSystem");
            var instances = managementClass.GetInstances();
            foreach (var instance in instances)
            {
                if (instance["Version"] is string version)
                {
                    var nums = version.Split('.').Select(int.Parse).ToList();
                    var info = new SystemVersionInfo(nums[0], nums[1], nums[2]);
                    return info;
                }
            }
            return default(SystemVersionInfo);
        }
    }
}
