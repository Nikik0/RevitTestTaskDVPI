using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
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
    internal class WpfRevitUsage : IExternalCommand
    {
        public XYZ GetElementCenter(Element elem)
        {
            BoundingBoxXYZ bounding = elem.get_BoundingBox(null);
            XYZ center = (bounding.Max + bounding.Min) * 0.5;
            return center;
        }

        public XYZ GetElementCorner(Element elem) //получение угла элемента через диагонали (угол элемента как точка на диагонали)
        {

            BoundingBoxXYZ bounding = elem.get_BoundingBox(null);
            XYZ center = (bounding.Max + bounding.Min) * 0.5;
            XYZ cornerBottom = bounding.Max;
            XYZ cornerRight = bounding.Min;
            
            if (cornerBottom.X < 0) cornerBottom = new XYZ((cornerBottom.X + center.X) / 2.2, cornerBottom.Y, cornerBottom.Z); else cornerBottom = new XYZ((cornerBottom.X + center.X) / 1.8, cornerBottom.Y, cornerBottom.Z);
            if (cornerRight.Y > 0) cornerRight = new XYZ(cornerRight.X, (cornerRight.Y + center.Y) / 2.2, cornerRight.Z); else cornerRight = new XYZ(cornerRight.X, (cornerRight.Y + center.Y) / 1.8, cornerRight.Z);
            
            XYZ corner = new XYZ(cornerBottom.X, cornerRight.Y, 0);

            return corner;
        }

        public XYZ GetRoomCorner(Room room)
        {
            XYZ boundCorner = GetElementCorner(room);
            LocationPoint locPt = (LocationPoint)room.Location;
            XYZ roomCorner = new XYZ(boundCorner.X, boundCorner.Y, locPt.Point.Z);
            return roomCorner;
        }

        public XYZ GetRoomCenter(Room room)
        {
            XYZ boundCenter = GetElementCenter(room);
            LocationPoint locPt = (LocationPoint)room.Location;
            XYZ roomCenter = new XYZ(boundCenter.X, boundCenter.Y, locPt.Point.Z);
            return roomCenter;
        }

        public RoomTag CreateRoomTag(Document document, Room room, UV roomTagLocation) //создание стандартной марки в 1 помещении
        {
            View activeView = document.ActiveView;
     

            LinkElementId roomId = new LinkElementId(room.Id);
            try
            {
                using (Transaction transaction = new Transaction(document))
                {
                    transaction.Start("Create room tag");

                    RoomTag roomTag = document.Create.NewRoomTag(roomId, roomTagLocation, activeView.Id);
                    if (null == roomTag)
                    {
                        throw new Exception("Ошибка создания марки помещения");
                    }

                    //TaskDialog.Show("Revit", "Room tag created successfully.");

                    transaction.Commit();

                    return roomTag;
                }
            }
            catch (Exception e)
            {
                TaskDialog.Show("Ошибка", "Ошибка при создании Марки помещения. Проверьте загружено ли необходимое семейство.");
                return null;
            }
            
        }

        public void ChangeRoomTag(Document doc, RoomTag roomTag, Element newType) //изменение стандартной марки на нужную
        {
            try
            {
                using (Transaction transaction = new Transaction(doc))
                {
                    transaction.Start("Change room tag");

                    roomTag.ChangeTypeId(newType.Id);

                    transaction.Commit();

                }
            }
            catch (Exception e)
            {
                TaskDialog.Show("Ошибка", "Ошибка при изменении Марки помещения. Проверьте загружено ли необходимое семейство.");
            }
            
        }

        public void TagEveryRoom(Document doc) // проставление 2ух типов марок в каждом помещении
        {
            //System.Diagnostics.Debug.WriteLine("enter tag every room");
            
            RoomFilter filter = new RoomFilter();
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> rooms = collector.WherePasses(filter).ToElements();
            System.Diagnostics.Debug.WriteLine(rooms.Count);
            //ElementType elementType = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DuctFitting).OfClass(typeof(ElementType)).Cast<ElementType>().Where(x => x.FamilyName.Equals("Отвод прямоугольного сечения")).FirstOrDefault();
            /*
            FilteredElementCollector roomTagTypes = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_RoomTags).WhereElementIsElementType();

            Element centerType = roomTagTypes.ToElements()[2];

            Element cornerType = roomTagTypes.ToElements()[1];
            */
            ElementType centerTypeName = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_RoomTags).OfClass(typeof(ElementType)).Cast<ElementType>().Where(x => x.FamilyName.Equals("Марка наименования")).FirstOrDefault();
            ElementType cornerTypeName = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_RoomTags).OfClass(typeof(ElementType)).Cast<ElementType>().Where(x => x.FamilyName.Equals("Марка площади")).FirstOrDefault();

            Element centerType = centerTypeName as Element;
            Element cornerType = cornerTypeName as Element;


            foreach (Room ro in rooms)
            {
                //System.Diagnostics.Debug.WriteLine("room here");
                UV center = new UV(GetRoomCenter(ro).X, GetRoomCenter(ro).Y);
                UV corner = new UV(GetRoomCorner(ro).X, GetRoomCorner(ro).Y);

                RoomTag roomTagCenter = CreateRoomTag(doc, ro, center);

                ChangeRoomTag(doc, roomTagCenter, centerType);

                RoomTag roomTagCorner = CreateRoomTag(doc, ro, corner);

                ChangeRoomTag(doc, roomTagCorner, cornerType);

                //Console.WriteLine(ro.Name, ro.Area, center);

                //TaskDialog.Show(ro.Name, ro.Name);
               // System.Diagnostics.Debug.WriteLine(ro.Name);
            }
        }

        public void WallSectionCreate(UIDocument uidoc) //создание размера выбранной стены
        {
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            ICollection<ElementId> set = sel.GetElementIds();

            Wall wall = null;

            if (1 == set.Count)
            {
                foreach (ElementId e in set)
                {
                    Element elem = doc.GetElement(e);
                    wall = elem as Wall;
                }
            }

            if (null == wall)
            {
                TaskDialog.Show("Ошибка", "Выберите стену");
            } else
            {
                ViewFamilyType vft = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>().FirstOrDefault<ViewFamilyType>(x => ViewFamily.Section == x.ViewFamily);

                try
                {
                    using (Transaction tx = new Transaction(doc))
                    {
                        tx.Start("Create Wall Section View");

                        ViewSection.CreateSection(doc, vft.Id, GetSectionViewPerpendiculatToWall(wall));

                        tx.Commit();
                    }
                }
                catch (Exception e)
                {
                    TaskDialog.Show("Ошибка", "Ошибка при создании разреза");
                }
            }

        }

        public BoundingBoxXYZ GetSectionViewPerpendiculatToWall(Wall wall) //получение вида перпендикулярного выбранной стены в центральной точке
        {
            LocationCurve lc = wall.Location
              as LocationCurve;

            Transform curveTransform = lc.Curve
              .ComputeDerivatives(0.5, true);


            XYZ origin = curveTransform.Origin;
            XYZ viewdir = curveTransform.BasisX.Normalize();
            XYZ up = XYZ.BasisZ;
            XYZ right = up.CrossProduct(viewdir);


            Transform transform = Transform.Identity;
            transform.Origin = origin;
            transform.BasisX = right;
            transform.BasisY = up;
            transform.BasisZ = viewdir;

            BoundingBoxXYZ sectionBox = new BoundingBoxXYZ();
            sectionBox.Transform = transform;

            double d = wall.WallType.Width;
            BoundingBoxXYZ bb = wall.get_BoundingBox(null);
            double minZ = bb.Min.Z;
            double maxZ = bb.Max.Z;
            double h = maxZ - minZ;

            sectionBox.Min = new XYZ(-2 * d, -1, 0);
            sectionBox.Max = new XYZ(2 * d, h + 1, 5);

            return sectionBox;
        }

        public void MoveDuct(Document doc) // перемещение и соединение воздуховодов
        {
            UIDocument uidoc = new UIDocument(doc);
            
            Selection sel = uidoc.Selection;
            List<Duct> selectedDuct = new List<Duct>();

            ICollection<ElementId> set = sel.GetElementIds();
            System.Diagnostics.Debug.WriteLine(set.Count);
            if (2 == set.Count)
            {
                foreach (ElementId e in set)
                {
                    Element elem = doc.GetElement(e);
                    if (elem is Duct)
                    {
                        selectedDuct.Add(elem as Duct);
                    }
                }

                List<XYZ> pointsDuct = new List<XYZ>();

                foreach (Duct d in selectedDuct)        // получение начальных и конечных точек выбранных воздуховодов 
                {
                    LocationCurve locationCurve = d.Location as LocationCurve;

                    if (locationCurve != null)
                    {
                        Curve c = locationCurve.Curve;

                        pointsDuct.Add(new XYZ(c.GetEndPoint(0).X, c.GetEndPoint(0).Y, c.GetEndPoint(0).Z));
                        pointsDuct.Add(new XYZ(c.GetEndPoint(1).X, c.GetEndPoint(1).Y, c.GetEndPoint(1).Z));
                        //System.Diagnostics.Debug.WriteLine("point added x2");
                    }
                    
                }
                /*
                foreach (XYZ point in pointsDuct)
                {
                        System.Diagnostics.Debug.WriteLine("points:", point);
                }
                System.Diagnostics.Debug.WriteLine(pointsDuct.Count);
                */
                XYZ translationVector = new XYZ();
                translationVector = pointsDuct[0] - pointsDuct[2];      // вектор переноса второго воздуховода

                System.Diagnostics.Debug.WriteLine(translationVector);

                try
                {
                    using (Transaction tx = new Transaction(doc))
                    {
                        tx.Start("Create Duct and Duct fittings");
                        ElementTransformUtils.MoveElement(doc, selectedDuct[1].Id, translationVector);

                        ElementType elementType = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DuctFitting).OfClass(typeof(ElementType)).Cast<ElementType>().Where(x => x.FamilyName.Equals("Отвод прямоугольного сечения")).FirstOrDefault();

                        RoutingPreferenceManager rpm = selectedDuct[1].DuctType.RoutingPreferenceManager;

                        rpm.AddRule(RoutingPreferenceRuleGroupType.Elbows, new RoutingPreferenceRule(elementType.Id, "Duct Fitting"));

                        int routingPerenceGroupCnt = rpm.GetNumberOfRules(RoutingPreferenceRuleGroupType.Elbows);

                        if (routingPerenceGroupCnt > 1)
                        {
                            for (int i = 0; i < routingPerenceGroupCnt - 1; i++)
                            {
                                rpm.RemoveRule(RoutingPreferenceRuleGroupType.Elbows, 0);
                            }
                        }

                        Connector con1 = null;

                        Connector con2 = null;

                        for (int i = 0; i < selectedDuct.Count() - 1; i++)        // поиск подходящий для соединения коннекторов
                        {
                            ConnectorManager connectorMan1 = selectedDuct[0].ConnectorManager;
                            ConnectorManager connectorMan2 = selectedDuct[1].ConnectorManager;
                            ConnectorSet connectorSet1 = connectorMan1.Connectors;
                            ConnectorSet connectorSet2 = connectorMan2.Connectors;

                            ConnectorSetIterator csi1 = connectorSet1.ForwardIterator();

                            ConnectorSetIterator csi2 = connectorSet2.ForwardIterator();

                            con1 = null;

                            con2 = null;

                            double minDist = double.MaxValue;

                            foreach (Connector connector1 in connectorSet1)
                            {
                                foreach (Connector connector2 in connectorSet2)
                                {
                                    double dist = connector1.Origin.DistanceTo(connector2.Origin);
                                    if (dist < minDist)
                                    {
                                        con1 = connector1;
                                        con2 = connector2;
                                        minDist = dist;
                                    }

                                }
                            }

                        }

                        try
                        {
                            doc.Create.NewElbowFitting(con1, con2);
                        }

                        catch (Exception e)
                        {
                            TaskDialog.Show("Ошибка", "Не удалось создать соединение");
                        }

                        tx.Commit();
                    }
                } catch (Exception e)
                {
                    TaskDialog.Show("Ошибка", "Ошибка при работе с воздуховодами");
                }

            }

        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
        
            return Result.Succeeded;
        }
    }
  
}
