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
    public class BothDocumentOption : IPickElementsOption
    {
        public List<Element> PickElements(UIDocument uiDocument, Func<Element, bool> validateElement)
        {
            Document doc = uiDocument.Document;
            IList<Reference> references = uiDocument.Selection.PickObjects(ObjectType.PointOnElement,
                SelectionFilterFactory.CreateLinkableSelectionFilter(doc, validateElement));
            List<Element> elements = new List<Element>();

            foreach (Reference reference in references)
            {
                if (doc.GetElement(reference.ElementId) is RevitLinkInstance linkInstance)
                {
                    var element = linkInstance.GetLinkDocument().GetElement(reference.LinkedElementId);
                    elements.Add(element);
                }
                else
                {
                    elements.Add(doc.GetElement(reference.ElementId));
                }
            }
            return elements;
        }
    }
}
