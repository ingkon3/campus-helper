using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace CampusHelperApp.Main.Infrastructure.Extensions.SelectionExtensions
{
    public class LinkableSelectionFilter : BaseSelectionFilter
    {
        private readonly Document _doc;

        public LinkableSelectionFilter(
            Document doc,
            Func<Element, bool> validateElement) : base(validateElement)
        {
            _doc = doc;
        }

        public override bool AllowElement(Element elem) => true;

        public override bool AllowReference(Reference reference, XYZ position)
        {
            if (_doc.GetElement(reference.ElementId) is RevitLinkInstance linkInstance)
            {
                var element = linkInstance.GetLinkDocument()
                    .GetElement(reference.LinkedElementId);
                return ValidateElement(element);
            }
            return ValidateElement(_doc.GetElement(reference.ElementId));
        }
    }
}
