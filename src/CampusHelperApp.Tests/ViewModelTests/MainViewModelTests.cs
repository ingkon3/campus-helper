using CampusHelperApp.UI.ViewModels;

namespace CampusHelperApp.Tests.ViewModelTests;

public class MainViewModelTests
{
    [Fact]
    public void Test()
    {
        var viewModel = new MainWindowViewModel();
        Assert.NotNull(viewModel);
    }
}
