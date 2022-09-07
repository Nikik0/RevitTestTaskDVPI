using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace RevitTestTaskDVPI
{
    /// <summary>
    /// Interaction logic for WPFTask1.xaml
    /// </summary>
    public partial class WPFTask1 : Window
    {
        public UIDocument uidoc { get; }

        public Document doc { get; }

        public WPFTask1(UIDocument UIdoc)
        {
            uidoc = UIdoc;
            doc = UIdoc.Document;

            InitializeComponent();
            JsonParser jsonParser = new JsonParser();
            JsonInfo jsonInfo = jsonParser.ReadJson();
            TextBox1.Text = jsonInfo.FirstBox;
            TextBox2.Text = jsonInfo.SecondBox;

        }

        private void AR_Click(object sender, RoutedEventArgs e)
        {
            WpfRevitUsage revit = new WpfRevitUsage();
            revit.TagEveryRoom(doc);
            //TaskDialog.Show("2222222", "ARclick");
            
        }

        private void KR_Click(object sender, RoutedEventArgs e)
        {
            //TaskDialog.Show("2222222", "RKclick");
            WpfRevitUsage revit = new WpfRevitUsage();
            revit.WallSectionCreate(uidoc);
        }

        private void OVK_Click(object sender, RoutedEventArgs e)
        {
            WpfRevitUsage revit = new WpfRevitUsage();
            revit.MoveDuct(doc);
            //TaskDialog.Show("2222222", "OVKclick");
        }

        private void TextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            JsonInfo jsonInfo = new JsonInfo();
            if (TextBox1.Text != null)
            {
                jsonInfo.FirstBox = TextBox1.Text;
            }
            else
            {
                jsonInfo.FirstBox = "0";
            }
            JsonParser jsonParser = new JsonParser();
            jsonParser.WriteJson(jsonInfo);
        }

        private void TextBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            JsonInfo jsonInfo = new JsonInfo();
            if (TextBox2.Text != null)
            {
                jsonInfo.SecondBox = TextBox2.Text;
            }
            else
            {
                jsonInfo.SecondBox = "0";
            }
            JsonParser jsonParser = new JsonParser();
            jsonParser.WriteJson(jsonInfo);
        }
    }
}
