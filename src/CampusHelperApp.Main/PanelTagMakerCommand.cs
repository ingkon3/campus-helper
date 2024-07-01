using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CampusHelperApp.Main.Infrastructure.Extensions;
using View = Autodesk.Revit.DB.View;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;

namespace CampusHelperApp.Main
{
    [Regeneration(RegenerationOption.Manual)]
    [Transaction(TransactionMode.Manual)]
    internal class PanelTagMakerCommand : IExternalCommand
    {
        const double toFeet = 1 / 30.48;
        public Result Execute(
    ExternalCommandData commandData,
    ref string message,
    ElementSet elements)
        {
            #region Variables
            UIApplication uiapp = commandData.Application;
            var app = uiapp.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var doc = uidoc.Document;
            #endregion

            string panelTagMakerMessage = String.Empty;

            IList<Element> selectedElements = uidoc.GetSelectedElements();
            if (selectedElements.Count == 0)
            {
                try
                {

                }
                catch { return Result.Cancelled; }

            }

            var activeView = doc.ActiveView;
            var scale = activeView.Scale;
            double offset = scale == 50 ? 25 * toFeet : scale == 100 ? 65 * toFeet : scale == 150 ? 80 * toFeet : scale == 200 ? 95 * toFeet : 100 * toFeet;
            double offsetMMD = scale == 50 ? 35 * toFeet : scale == 100 ? 70 * toFeet : scale == 150 ? 100 * toFeet : scale == 200 ? 120 * toFeet : 100 * toFeet;


            List<FamilyInstance> panels = new FilteredElementCollector(doc, activeView.Id).OfCategory(BuiltInCategory.OST_GenericModel)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(x => (x.Name.ToLower().Contains("jci")
                || x.Name.ToLower().Contains("tci")
                || x.Name.ToLower().Contains("pr")))
                .Where(x => !x.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString().ToLower().Contains("panel"))
                .OfType<FamilyInstance>()
                .ToList();

            List<FamilyInstance> panelsMMD = new FilteredElementCollector(doc, activeView.Id).OfCategory(BuiltInCategory.OST_GenericModel)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(x => x.Name.ToLower().Contains("m"))
                .Where(x => (!x.Name.ToLower().Contains("jci")
                && !x.Name.ToLower().Contains("tci")
                && !x.Name.ToLower().Contains("pr")))
                .Where(x => !x.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString().ToLower().Contains("panel"))
                .OfType<FamilyInstance>()
                .ToList();

            string tagPanelFamilyName = "TAG_Panels";
            string tagPanelTypeName = "Precast Mark";

            string tagPanelMMDFamilyName = "TAG_MMD_Panels";
            string tagPanelMMDTypeName = "Precast_name";

            #region тэг для выносок
            var tagGeneric = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_GenericModelTags)
                .OfType<FamilySymbol>()
                .FirstOrDefault();

            var tagPanels = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_GenericModelTags)
                .Where(x => x.get_Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM).AsString() == tagPanelFamilyName)
                .Where(x => x.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString() == tagPanelTypeName)
                .OfType<FamilySymbol>()
                .ToList();

            var tagMMDPanels = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_GenericModelTags)
                .Where(x => x.get_Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM).AsString() == tagPanelMMDFamilyName)
                .Where(x => x.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString() == tagPanelMMDTypeName)
                .OfType<FamilySymbol>()
                .ToList();

            var tagPanel = tagPanels.Count > 0 ? tagPanels.FirstOrDefault() : tagGeneric;
            var tagMMDPanel = tagMMDPanels.Count > 0 ? tagMMDPanels.FirstOrDefault() : tagGeneric;
            #endregion

            try
            {
                using (Transaction t = new Transaction(doc, "Create tag for panels"))
                {
                    t.Start();

                    CreateTagForPanels(doc, panels, tagPanel.Id, offset);
                    CreateTagForPanels(doc, panelsMMD, tagMMDPanel.Id, offsetMMD);

                    t.Commit();
                }

            }
            catch (Exception ex) { TaskDialog.Show("Error!!!", "Error!!!\n" + ex.Message); }

            panelTagMakerMessage += "Count tag for panels = " + panels.Count + "\n";
            panelTagMakerMessage += "Count tag for MMD panels = " + panelsMMD.Count;


            TaskDialog.Show("Test", panelTagMakerMessage);

            return Result.Succeeded;
        }

        private void CreateTagForPanels(Document doc, List<FamilyInstance> panels, ElementId tagId, double offset)
        {
            var activeView = doc.ActiveView;
            var scale = activeView.Scale;

            foreach (var panel in panels)
            {
                //TODO: отсортировать панели по ориентации (x, y)
                var panelBBox = panel.get_BoundingBox(activeView);
                var length = panelBBox.GetLength();
                var width = panelBBox.GetWidth();
                var center = panelBBox.GetCenter();
                var facingOrientation = panel.FacingOrientation;

                //TODO: рассчитать расположение тага с учетом расположения внутри или снаружи
                if (length > width)
                {
                    int k = 1;
                    if (panel.Name.ToLower().Contains("m")) { k = -1; }
                    var tagLocation = center + XYZ.BasisY * (width / 2 + offset) * facingOrientation.Y * k;
                    IndependentTag.Create(doc,
                        tagId,
                        doc.ActiveView.Id,
                        new Reference(panel),
                        false,
                        TagOrientation.Horizontal,
                        tagLocation);
                }
                else
                {
                    int k = -1;
                    if (panel.Name.ToLower().Contains("m")) { k = 1; }
                    var tagLocation = center - XYZ.BasisX * (length / 2 + offset) * facingOrientation.X * k;  //TODO: уточни * (-1)
                    IndependentTag.Create(doc,
                        tagId,
                        doc.ActiveView.Id,
                        new Reference(panel),
                        false,
                        TagOrientation.Vertical,
                        tagLocation);
                }

            }
        }
    }
}
