using Ninject.Modules;
using CampusHelperApp.Core.Models;

namespace CampusHelperApp.Main;

public class DependencyInjectionManager : NinjectModule
{
    public override void Load()
    {
        Bind<IApplicationDataProperties>().To<ApplicationDataProperties>().InSingletonScope();
    }
}
