using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;

namespace RevitTestTaskDVPI
{
    internal class PluginMain : IExternalApplication
    {
        Result IExternalApplication.OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            string pluginTabName = "Плагин по заданию", assemblyLocation = Assembly.GetExecutingAssembly().Location, iconsPath = Path.GetDirectoryName(assemblyLocation) + @"\icons\";
            application.CreateRibbonTab(pluginTabName);

            RibbonPanel panel = application.CreateRibbonPanel(pluginTabName, "Плагин основной");

            panel.AddItem(new PushButtonData(nameof(WPFShow), "Задание 1", assemblyLocation, typeof(WPFShow).FullName)
            {
                LargeImage = new BitmapImage(new Uri(iconsPath + "1.png"))
            });

            panel.AddItem(new PushButtonData(nameof(Task2CoordinatePick), "Задание 2", assemblyLocation, typeof(Task2CoordinatePick).FullName)
            {
                LargeImage = new BitmapImage(new Uri(iconsPath + "2.png"))
            });

            return Result.Succeeded;
        }
    }
    
}
