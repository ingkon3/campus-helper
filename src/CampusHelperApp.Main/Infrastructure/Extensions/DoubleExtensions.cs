using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampusHelperApp.Main.Infrastructure.Extensions
{
    public static class DoubleExtensions
    {
        public static double ToRadians(this double value) => value * (Math.PI) / 180;
    }
}
