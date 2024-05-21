using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;

namespace CampusHelperApp.Main.Infrastructure.Extensions.SelectionExtensions
{
    public class CurrentDocumentOption : IPickElementsOption
    {
        public List<Element> PickElements(UIDocument uiDocument, Func<Element, bool> validateElement)
        {
            return uiDocument.Selection.PickObjects(ObjectType.Element,
                SelectionFilterFactory.CreateElementSelectionFilter(validateElement))
                .Select(r => uiDocument.Document.GetElement(r.ElementId))
                .ToList();
        }
    }
}
