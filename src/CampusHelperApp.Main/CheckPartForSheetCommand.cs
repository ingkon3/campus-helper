using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Ninject;
using CampusHelperApp.Main.Helpers;
using CampusHelperApp.UI.ViewModels;
using CampusHelperApp.UI.Views;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;
using CampusHelperApp.Main.Infrastructure.Extensions;

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
        // Создаем словарь для хранения соответствий названий блоков и параметров
        Dictionary<string, string> blockParameters = new Dictionary<string, string>
                                                             {
                                                                 { "212 A", "B_IS_PART_212A" },
                                                                 { "212 B", "B_IS_PART_212B" },
                                                                 { "212 C", "B_IS_PART_212C" },
                                                                 { "212 D", "B_IS_PART_212D" },
                                                                 { "213 A", "B_IS_PART_213A" },
                                                                 { "213 B", "B_IS_PART_213B" },
                                                                 { "214 A", "B_IS_PART_214A" },
                                                                 { "214 B", "B_IS_PART_214B" }
                                                             };

        List<string> corpusNames = new List<string>() { "212 A", "212 B", "212 C", "212 D", "213 A", "213 B", "214 A", "214 B" };

        List<string> partParameterNames = new List<string>() { "B_IS_PART_212A", "B_IS_PART_212B", "B_IS_PART_212C", "B_IS_PART_212D",
                "B_IS_PART_213A", "B_IS_PART_213B", "B_IS_PART_214A", "B_IS_PART_214B", "B_IS_WHOLE_BUILDING" };


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

            #region Get selected sheet
            IList<Element> selection = uidoc.GetSelectedElements();
            IList<ViewSheet> selectedSheet = selection.Where(e => e.GetType() == typeof(ViewSheet)).ToList().OfType<ViewSheet>().ToList();
            #endregion

            //TODO: сделать вывод сообщений. Может быть сделать окно
            try
            {
                int counter = 0;
                using (Transaction trans = new Transaction(doc, "Set PartName for sheets"))
                {
                    trans.Start();
                    foreach (var sheet in selectedSheet)
                    {
                        string sheetName = sheet.Name;
                        foreach (var corpusName in corpusNames)
                        {
                            if (sheetName.Contains(corpusName))
                            {
                                ElementFilter titleBlocksFilter = new ElementCategoryFilter(BuiltInCategory.OST_TitleBlocks);
                                var titleBlocks = sheet.GetDependentElements(titleBlocksFilter).Select(id => doc.GetElement(id));
                                foreach (var titleBlock in titleBlocks)
                                {
                                    SetParTitleBlock(titleBlock, corpusName, out bool isSet);
                                    if (isSet)
                                    {
                                        counter++;
                                    }
                                }
                            }
                        }
                    }
                    trans.Commit();
                }

                if(counter == 0)
                {
                    TaskDialog.Show("Message", $"Good!!!\nAll sheets were correct, there were no changes!!!");
                }
                else
                {
                    TaskDialog.Show("Message", $"Were no changes!!!\n Count sheets with changes = {counter}");
                }

            }
            catch (Exception ex) { TaskDialog.Show("Error!!!", $"Error!!!\n{ex.Message}"); }

            return Result.Succeeded;
        }


        // Метод для обработки каждого блока
        void SetParTitleBlock(Element titleBlock, string corpusName, out bool isSet)
        {
            isSet = false;
            if (blockParameters.TryGetValue(corpusName, out string parameterName))
            {
                Parameter partParameter = titleBlock.LookupParameter(parameterName);

                if (partParameter != null)
                {
                    SetPartParameterValues(titleBlock, partParameter, parameterName, out int counter);
                    if(counter > 0)
                    {
                        isSet = true;
                    }
                }
            }
        }


        // Метод для установки значений параметров блока
        void SetPartParameterValues(Element titleBlock, Parameter partParameter, string parameterName, out int counter)
        {
            counter = 0;
            foreach (var partParameterName in partParameterNames)
            {
                if (partParameterName != parameterName)
                {
                    if(titleBlock.LookupParameter(parameterName).AsInteger()!=0)
                    {
                        titleBlock.LookupParameter(partParameterName)?.Set(0);
                        counter++;
                    }
                }
            }

            if (partParameter.AsInteger() == 0)
            {
                partParameter.Set(1);
                counter++;
            }
        }

    }
}
