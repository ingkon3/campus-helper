using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Ninject;
using CampusHelperApp.Main.Helpers;
using CampusHelperApp.UI.ViewModels;
using CampusHelperApp.UI.Views;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;

namespace CampusHelperApp.Main
{
    /// <summary>
    /// A sample ribbon command, demonstrates the possibility to bind Revit commands to ribbon buttons.
    /// </summary>
    /// <seealso cref="IExternalCommand" />
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class PanelMarkerCommand : IExternalCommand
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

            List<FamilyInstance> panels = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_GenericModel)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(x => (x.Name.ToLower().Contains("jci")
                || x.Name.ToLower().Contains("tci")
                || x.Name.ToLower().Contains("pr")))
                .Where(x => !x.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString().ToLower().Contains("panel"))
                .OfType<FamilyInstance>()
                .ToList();

            if ( panels.Count == 0 )
            {
                TaskDialog.Show("message", $"There aren't JCI, TCI & PR panels in the project!");

                return Result.Succeeded;
            }
            try
            {

                List<FamilyInstance> wrongPanels = FindWrongPanels(panels);

                if(wrongPanels.Count > 0 )
                {
                    using (Transaction trans = new Transaction(doc, "Mark Panels"))

                    {
                        trans.Start();
                        foreach (FamilyInstance wrongPanel in wrongPanels)
                        {
                            FamilySymbol wrongPanelSymbol = wrongPanel?.Symbol;
                            string SymbolFamilyMark = wrongPanelSymbol?.LookupParameter("SH_Family_Mark")?.AsString();
                            string SymbolMarkNumber = wrongPanelSymbol?.LookupParameter("S_MARK_NUMBER")?.AsString();
                            string SymbolPrecastName = SymbolFamilyMark + SymbolMarkNumber;

                            wrongPanel?.LookupParameter("S_Precast_Name")?.Set(SymbolPrecastName);
                        }

                        trans.Commit();
                    }

                    TaskDialog.Show("message", $"Count wrong Panel Mark = {wrongPanels.Count}");
                }
                else
                {
                    TaskDialog.Show("message", $"All Mark Panels Good!!!");
                }
            }
            catch (Exception ex) { TaskDialog.Show("Error!!!", $"Error!!!\n{ex.Message}"); }


            return Result.Succeeded;
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
