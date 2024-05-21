using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampusHelperApp.Main.Infrastructure.Extensions.SelectionExtensions
{
    public static class PickElementsOptionFactory
    {
        public static CurrentDocumentOption CreateCurrentDocumentOption() => new CurrentDocumentOption();
        public static LinkDocumentOption CreateLinkDocumentOption() => new LinkDocumentOption();
        public static BothDocumentOption CreateBothDocumentOption() => new BothDocumentOption();
    }
}
