using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace CampusHelperApp.Main.Infrastructure.Extensions.SelectionExtensions
{
    public abstract class BaseSelectionFilter : ISelectionFilter
    {
        protected readonly Func<Element, bool> ValidateElement;
        protected readonly Func<Reference, bool> _validateReference;
        protected BaseSelectionFilter(Func<Element, bool> validateElement)
        {
            ValidateElement = validateElement;
        }

        public abstract bool AllowElement(Element elem);
        public abstract bool AllowReference(Reference reference, XYZ position);

    }
}
