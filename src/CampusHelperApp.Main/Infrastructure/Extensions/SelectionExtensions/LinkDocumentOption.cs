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
    public class LinkDocumentOption : IPickElementsOption
    {
        public List<Element> PickElements(UIDocument uiDocument, Func<Element, bool> validateElement)
        {
            Document doc = uiDocument.Document;
            return uiDocument.Selection.PickObjects(ObjectType.LinkedElement,
                SelectionFilterFactory.CreateLinkableSelectionFilter(doc, validateElement))
                .Select(r => (doc.GetElement(r.ElementId) as RevitLinkInstance)?.GetLinkDocument().GetElement(r.LinkedElementId))
                .ToList();
        }
    }
}
