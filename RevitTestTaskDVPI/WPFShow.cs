using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace RevitTestTaskDVPI
{
    [Transaction(TransactionMode.Manual)]
    public class WPFShow : IExternalCommand
    {
        
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;

            WPFTask1 wpfTask1 = new WPFTask1(uiDoc);

            wpfTask1.ShowDialog();

            return Result.Succeeded;
        }
    }
}
