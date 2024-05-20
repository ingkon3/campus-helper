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
    public class CheckPartForSheetCommand : IExternalCommand
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
            TaskDialog.Show("Hello", "it's CheckPartForSheetCommand");

            return Result.Succeeded;
        }
    }
}
