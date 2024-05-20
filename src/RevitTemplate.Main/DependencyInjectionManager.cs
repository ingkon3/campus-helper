using Ninject.Modules;
using RevitTemplate.Core.Models;

namespace RevitTemplate.Main;

public class DependencyInjectionManager : NinjectModule
{
    public override void Load()
    {
        Bind<IApplicationDataProperties>().To<ApplicationDataProperties>().InSingletonScope();
    }
}
