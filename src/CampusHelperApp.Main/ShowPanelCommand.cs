using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Ninject;
using CampusHelperApp.Main.Helpers;
using CampusHelperApp.UI.ViewModels;
using CampusHelperApp.UI.Views;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;
using View = Autodesk.Revit.DB.View;
using System.Windows.Controls;

namespace CampusHelperApp.Main
{
    /// <summary>
    /// A sample ribbon command, demonstrates the possibility to bind Revit commands to ribbon buttons.
    /// </summary>
    /// <seealso cref="IExternalCommand" />
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ShowPanelCommand : IExternalCommand
    {
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
            #region Variables
            UIApplication uiapp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            #endregion

            List<FamilyInstance> panels = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(x => (x.Name.ToLower().Contains("jci")
                || x.Name.ToLower().Contains("tci")
                || x.Name.ToLower().Contains("pr")))
                .Where(x => !x.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString().ToLower().Contains("panel"))
                .OfType<FamilyInstance>()
                .ToList();

            if (panels.Count == 0)
            {
                TaskDialog.Show("message", $"There aren't JCI, TCI & PR panels in the project!");

                return Result.Succeeded;
            }

            try
            {
                List<FamilyInstance> wrongPanels = FindWrongPanels(panels);

                if (wrongPanels.Count > 0)
                {
                    string viewName = "3d Panels";
                    View threeDView = null;
                    if (!ViewExists(viewName, doc))
                    {
                        Create3DView(ref doc, viewName);
                    }

                    FindView(doc, viewName, ref threeDView);

                    uidoc.ActiveView = threeDView;

                    using (Transaction trans = new Transaction(doc, "Isolate Panel (wrong Mark)"))

                    {
                        trans.Start();

                        IList<ElementId> wrongPanelIds = wrongPanels.Select(p => p.Id).ToList();
                        threeDView.IsolateElementsTemporary(wrongPanelIds);

                        trans.Commit();
                    }

                    TaskDialog.Show("message", $"Count wrong Panel Mark = {wrongPanels.Count}");
                }

                else TaskDialog.Show("message", $"All Mark Panels Good!!!");

            }
            catch (Exception ex) { TaskDialog.Show("Error!!!", $"Error!!!\n{ex.Message}"); }


            return Result.Succeeded;
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


        public List<FamilyInstance> FindWrongPanels(List<FamilyInstance> panels)
        {
            List<FamilyInstance> wrongPanels = new List<FamilyInstance>();

            foreach (FamilyInstance panel in panels)
            {
                FamilySymbol panelSymbol = panel?.Symbol;
                string InstancePrecastName = panel?.LookupParameter("S_Precast_Name")?.AsString();

                string SymbolType = panelSymbol?.LookupParameter("S_TYPE_GENERAL")?.AsString();
                string SymbolFamilyMark = panelSymbol?.LookupParameter("SH_Family_Mark")?.AsString();
                string SymbolMarkNumber = panelSymbol?.LookupParameter("S_MARK_NUMBER")?.AsString();

                string SymbolPrecastName = SymbolFamilyMark + SymbolMarkNumber;

                if (InstancePrecastName != SymbolPrecastName)
                {
                    wrongPanels.Add(panel);
                }
            }

            return wrongPanels;

        }
    }
}
