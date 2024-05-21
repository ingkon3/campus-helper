using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CampusHelperApp.Main.Infrastructure.Extensions.SelectionExtensions;

namespace CampusHelperApp.Main.Infrastructure.Extensions
{
    /// <summary>
    /// This class is used for creating extension method for pick elements in Revit UI
    /// </summary>
    public static class UIDocumentExtensions
    {
        public enum SelectionOption
        {
            Current,
            Link,
            Both
        }
        public static List<Element> PickElements(
            this UIDocument uiDocument, Func<Element, bool> validateElement, IPickElementsOption pickElementsOption)
        {
            return pickElementsOption.PickElements(uiDocument, validateElement);
        }


        public static List<Element> GetSelectedElements(
            this UIDocument uiDocument)
        {
            return uiDocument.Selection.GetElementIds()
                .Select(id => uiDocument.Document.GetElement(id))
                .ToList();
        }
    }
}
