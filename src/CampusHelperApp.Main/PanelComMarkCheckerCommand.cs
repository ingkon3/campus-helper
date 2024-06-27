using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using View = Autodesk.Revit.DB.View;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;

namespace CampusHelperApp.Main
{
    [Regeneration(RegenerationOption.Manual)]
    [Transaction(TransactionMode.Manual)]
    public class PanelComMarkCheckerCommand : IExternalCommand
    {
        public const double toFeet = 1 / 30.48;

        /// <summary>
        /// Executes the specified Revit command <see cref="ExternalCommand"/>.
        /// The main Execute method (inherited from IExternalCommand) must be public.
        /// </summary>
        /// <param name="commandData">The command data / context.</param>
        /// <param name="message">The message.</param>
        /// <param name="elements">The elements.</param>
        /// <returns>The result of command execution.</returns>
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            Dictionary<string, List<string>> blockGrids = new Dictionary<string, List<string>>
                                                                 {
                                                                     { "212A", new List<string>(){"A","F", "18", "27"} },
                                                                     { "212B", new List<string>(){"L","Q", "18", "27"} },
                                                                     { "212C", new List<string>(){"V","Z1", "18", "27"} },
                                                                     { "212D", new List<string>(){"Q","V", "08", "20"} },
                                                                     { "213A", new List<string>(){"A","F", "01", "10"} },
                                                                     { "213B", new List<string>(){"F","L", "08", "20"} },
                                                                     { "214A", new List<string>(){"L","Q", "01", "10"} },
                                                                     { "214B", new List<string>(){"V","Z1", "01", "10"} },
                                                                 };
            #region Variables
            UIApplication uiapp = commandData.Application;
            var app = uiapp.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var doc = uidoc.Document;
            #endregion

            List<FamilyInstance> panels = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_GenericModel)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(x => (x.Name.ToLower().Contains("jci")
                || x.Name.ToLower().Contains("tci")
                || x.Name.ToLower().Contains("pr")))
                .Where(x => !x.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString().ToLower().Contains("panel"))
                .OfType<FamilyInstance>()
                .ToList();

            var grids = new FilteredElementCollector(doc).OfClass(typeof(Grid)).OfType<Grid>().ToList();


            List<FamilyInstance> wrongPanels = new List<FamilyInstance>();
            string messageTest = string.Empty;
            try
            {
                string viewName = "3d Panels";
                View threeDView = null;
                if (!ViewExists(viewName, doc))
                {
                    Create3DView(ref doc, viewName);
                }

                FindView(doc, viewName, ref threeDView);

                using (Transaction trans = new Transaction(doc, "Isolate Panel (wrong Comment)"))

                {
                    trans.Start();
                    foreach (var blocks in blockGrids)
                    {
                        List<XYZ> intersectionPoints = new List<XYZ>();
                        foreach (var grid in grids)
                        {
                            var gridsInBlock = grids.Where(x => blocks.Value.Contains(x.Name)).ToList();

                            var blockCurves = gridsInBlock.Select(x => x.Curve).ToList();

                            FindIntersectionCurves(blockCurves, ref intersectionPoints);
                        }

                        GetMaxMinPoint(intersectionPoints, out XYZ minPoint, out XYZ maxPoint);

                        var bbox = new BoundingBoxXYZ();

                        bbox.Min = new XYZ(minPoint.X, minPoint.Y, 0);
                        bbox.Max = new XYZ(maxPoint.X, maxPoint.Y, 3000 * toFeet);


                        Autodesk.Revit.DB.Outline outline = new Autodesk.Revit.DB.Outline(bbox.Min, bbox.Max);
                        BoundingBoxIntersectsFilter boundingBoxIntersectsFilter = new BoundingBoxIntersectsFilter(outline, -15 * toFeet);

                        var panelsId = panels.Select(x => x.Id).ToList();
                        List<FamilyInstance> panelsIntersectBoundingBox = new FilteredElementCollector(doc, panelsId)
                                                                          .WherePasses(boundingBoxIntersectsFilter)
                                                                          .ToElements()
                                                                          .OfType<FamilyInstance>()
                                                                          .ToList();


                        List<FamilyInstance> wrongPanelsInBlock = FindWrongPanels(blocks.Key, panelsIntersectBoundingBox);
                        foreach (var panel in wrongPanelsInBlock)
                        {
                            wrongPanels.Add(panel);
                        }

                        //визуализация неправильных панелей
                        if (wrongPanels.Count > 0)
                        {

                            if (threeDView.IsTemporaryHideIsolateActive())
                            {
                                TemporaryViewMode tempView = TemporaryViewMode.TemporaryHideIsolate;
                                threeDView.DisableTemporaryViewMode(tempView);

                            }

                            var wrongPanelBlockIds = wrongPanels.Select(x => x.Id).ToList();
                            threeDView.IsolateElementsTemporary(wrongPanelBlockIds);
                        }
                    }
                    var wrongPanelIds = wrongPanels.Select(x => x.Id).Distinct().ToList();
                    messageTest = wrongPanelIds.Count > 0 ? $"Count wrong panel Comment = {wrongPanelIds.Count}" : "For all panels Mark and Comments good!!!";
                    TaskDialog.Show("message", messageTest);

                    trans.Commit();

                    if (wrongPanels.Count > 0) { uidoc.ActiveView = threeDView; }
                }
            }
            catch (Exception ex) { TaskDialog.Show("Error!", $"Error!!!\n{ex.Message}"); }


            return Result.Succeeded;
        }


        private void FindIntersectionCurves(List<Curve> curves, ref List<XYZ> intersectionPoints)
        {
            for (int i = 0; i < curves.Count - 1; i++)
            {
                for (int j = i + 1; j < curves.Count; j++)
                {
                    Curve curve1 = curves[i];
                    Curve curve2 = curves[j];

                    if (curve1.Intersect(curve2, out IntersectionResultArray intersectionResult) == SetComparisonResult.Overlap)
                    {
                        IntersectionResult intersection = intersectionResult.Cast<IntersectionResult>().FirstOrDefault();
                        XYZ intersectionPoint = intersection.XYZPoint;
                        intersectionPoints.Add(intersectionPoint);
                    }
                }
            }
        }


        public void GetMaxMinPoint(List<XYZ> intersectionPoints, out XYZ minPoint, out XYZ maxPoint)
        {
            // Находим наибольшее и наименьшее значения координат X и Y среди всех точек
            double maxX = intersectionPoints.Max(p => p.X);
            double maxY = intersectionPoints.Max(p => p.Y);
            double minX = intersectionPoints.Min(p => p.X);
            double minY = intersectionPoints.Min(p => p.Y);

            // Находим угловые точки прямоугольника
            minPoint = intersectionPoints.FirstOrDefault(p => Math.Round(p.X) == Math.Round(minX) && Math.Round(p.Y) == Math.Round(minY));
            maxPoint = intersectionPoints.FirstOrDefault(p => Math.Round(p.X) == Math.Round(maxX) && Math.Round(p.Y) == Math.Round(maxY));
        }


        public List<FamilyInstance> FindWrongPanels(string blockName, List<FamilyInstance> panels)
        {
            List<FamilyInstance> wrongPanels = new List<FamilyInstance>();

            foreach (FamilyInstance panel in panels)
            {
                var comment = panel.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString();
                var mark = panel.get_Parameter(BuiltInParameter.DOOR_NUMBER).AsString();

                if (comment != blockName || mark != blockName)
                {
                    wrongPanels.Add(panel);
                }
            }

            return wrongPanels;

        }


        private void Create3DView(ref Document document, string viewName)
        {
            View threeDView = null;
            if (ViewExists(viewName, document))
            {
                return;
            }


            // Find a 3D view type
            var viewFamilyTypes = new FilteredElementCollector(document)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .Where(vftype => vftype.ViewFamily == ViewFamily.ThreeDimensional);

            using (var t = new Transaction(document, "Create 3D View"))
            {
                t.Start();
                // Create a new View3D
                View3D view3D = View3D.CreateIsometric(document, viewFamilyTypes.First().Id);
                if (view3D is object)
                {
                    view3D.Name = viewName;
                }

                t.Commit();
            }
        }


        private void FindView(Document document, string viewName, ref View view)
        {
            if (ViewExists(viewName, document))
            {
                IList<View> viewList = new FilteredElementCollector(document)
                    .OfClass(typeof(View))
                    .Cast<View>()
                    .Where(v => v.IsTemplate == false)
                    .ToList();
                view = viewList.Where(v => v.Name == viewName).FirstOrDefault();
            }
        }


        private bool ViewExists(string ViewName, Document document)
        {
            bool retval = false;
            IList<View3D> viewList = new FilteredElementCollector(document).OfClass(typeof(View3D)).Cast<View3D>().Where(v => v.IsTemplate == false).ToList();
            foreach (View3D view in viewList)
            {
                if ((view.Name ?? "") == (ViewName ?? ""))
                {
                    retval = true;
                }
            }

            return retval;
        }
    }
}
