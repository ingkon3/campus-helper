using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace CampusHelperApp.Main.Infrastructure.Extensions.SelectionExtensions
{
    public interface IPickElementsOption
    {
        List<Element> PickElements(UIDocument uiDocument, Func<Element, bool> validateElement);
    }
}
