using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace CampusHelperApp.Main.Infrastructure.Extensions.SelectionExtensions
{
    public static class SelectionFilterFactory
    {
        public static ElementSelectionFilter CreateElementSelectionFilter(Func<Element, bool> validateElement)
        {
            return new ElementSelectionFilter(validateElement);
        }
        public static LinkableSelectionFilter CreateLinkableSelectionFilter(Document doc, Func<Element, bool> validateElement)
        {
            return new LinkableSelectionFilter(doc, validateElement);
        }
    }
}
