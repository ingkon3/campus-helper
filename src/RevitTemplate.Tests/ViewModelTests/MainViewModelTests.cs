using RevitTemplate.UI.ViewModels;

namespace RevitTemplate.Tests.ViewModelTests;

public class MainViewModelTests
{
    [Fact]
    public void Test()
    {
        var viewModel = new MainWindowViewModel();
        Assert.NotNull(viewModel);
    }
}
