using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitTestTaskDVPI
{
    [Transaction(TransactionMode.Manual)]
    internal class Task2CoordinatePick : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;

            Document doc = uiDoc.Document;

            Selection selection = uiDoc.Selection;

            View activeView = doc.ActiveView;

            if (!(activeView is ViewPlan))
            {
                TaskDialog errorDialog = new TaskDialog("Ошибка")
                {
                    MainInstruction = "Данная команда предназначена только для работы на планах",
                    CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No
                };

                errorDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Открыть первый попавшийся план");

                errorDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Завершить работу команды");

                TaskDialogResult dialogResult = errorDialog.Show();


                if (dialogResult == TaskDialogResult.CommandLink1 || dialogResult == TaskDialogResult.Yes)
                {
                    activeView = new FilteredElementCollector(doc).OfClass(typeof(ViewPlan)).FirstElement() as ViewPlan;

                    uiDoc.ActiveView = activeView;
                }
                else if (dialogResult == TaskDialogResult.No)
                {
                    return Result.Cancelled;
                }
                else
                {
                    return Result.Failed;
                }
            }

            ObjectSnapTypes snapType = ObjectSnapTypes.Centers | ObjectSnapTypes.Midpoints;

            XYZ currentPoint = selection.PickPoint(snapType, "Укажите точку");

            const double inchToMm = 25.4;       // приведение координат к метрической системе

            const double footToMeter = 12 * inchToMm;

            XYZ currentPointMm = new XYZ(currentPoint.X * footToMeter, currentPoint.Y * footToMeter, currentPoint.Z * footToMeter);

            TaskDialog.Show("Координата точки", currentPointMm.ToString());


            return Result.Succeeded;
        }

    }
}
