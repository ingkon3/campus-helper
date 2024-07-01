using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Ninject;
using CampusHelperApp.Core.Models;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;

namespace CampusHelperApp.Main
{
    /// <summary>
    /// The main application defined in this add-in
    /// </summary>
    /// <seealso cref="IExternalApplication" />
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class App : IExternalApplication
    {
        private UIControlledApplication _uiControlledApplication;

        /// <summary>
        /// Represents the singleton instance of the dependency injection container.
        /// </summary>
        public static IKernel ServiceLocator { get; private set; }

        /// <summary>
        /// Called when [startup].
        /// </summary>
        /// <param name="application">The UI control application.</param>
        /// <returns></returns>
        /// ReSharper disable once ParameterHidesMember
        public Result OnStartup(UIControlledApplication application)
        {
            this._uiControlledApplication = application;
            _uiControlledApplication.ControlledApplication.ApplicationInitialized +=
                ControlledApplicationOnApplicationInitialized;

            InitializeDependencies();
            InitializeRibbon();

            try
            {
                // TODO: add you code here
            }
            catch (Exception ex)
            {
                TaskDialog.Show($"Error in {nameof(OnStartup)} method", ex.ToString());
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        /// <summary>
        /// Called when [shutdown].
        /// </summary>
        /// <param name="application">The application.</param>
        public Result OnShutdown(UIControlledApplication application)
        {
            try
            {
                // TODO: add you code here
            }
            catch (Exception ex)
            {
                TaskDialog.Show($"Error in {nameof(OnShutdown)} method", ex.ToString());
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        private void ControlledApplicationOnApplicationInitialized(object sender, ApplicationInitializedEventArgs e)
        {
            // TODO: Here you can activate your Dockable Pane. (If applicable)
            var appDataProperties = ServiceLocator.Get<IApplicationDataProperties>();
            _uiControlledApplication.ViewActivated += appDataProperties.OnViewActivatedSubscriber;
        }

        private void InitializeRibbon()
        {
            // TODO declare your ribbon items here
            var ribbonItems = new List<RibbonHelper.RibbonButton>
            {
                new RibbonHelper.RibbonButton<PanelMarkerCommand>
                {
                    Text     = Resources.MainButtonName,
                    Tooltip  = Resources.MainButtonTooltip,
                    IconName = "Resources.Icons.MarkPanel.png",
                },
                new RibbonHelper.RibbonButton<ShowPanelCommand>
                {
                    Text     = Resources.SecondButtonName,
                    Tooltip  = Resources.SecondButtonTooltip,
                    IconName = "Resources.Icons.ShowWrongPanel.png",
                },
                new RibbonHelper.RibbonButton<CheckPartForSheetCommand>
                {
                    Text     = Resources.ThirdButtonName,
                    Tooltip  = Resources.ThirdButtonTooltip,
                    IconName = "Resources.Icons.PartForSheet.png",
                },
                new RibbonHelper.RibbonButton<PanelSetComMarkCommand>
                {
                    Text     = Resources.FourthButtonName,
                    Tooltip  = Resources.FourthButtonTooltip,
                    IconName = "Resources.Icons.SetComMark.png",
                },
                new RibbonHelper.RibbonButton<PanelComMarkCheckerCommand>
                {
                    Text     = Resources.FifthButtonName,
                    Tooltip  = Resources.FifthButtonToolTip,
                    IconName = "Resources.Icons.ComMarkChecker.png",
                },
                new RibbonHelper.RibbonButton<PanelTagMakerCommand>
                {
                    Text     = Resources.SixthButtonName,
                    Tooltip  = Resources.SixthButtonToolTip,
                    IconName = "Resources.Icons.PanelTagMaker.png",
                },
            };

            RibbonHelper.AddButtons(_uiControlledApplication,
                                    ribbonItems,
                                    ribbonPanelName: Resources.RibbonPanelName,
                                    ribbonTabName: Resources.RibbonTabName);
        }

        private void InitializeDependencies()
        {
            ServiceLocator = new StandardKernel();
            ServiceLocator.Load(new DependencyInjectionManager());
        }
    }
}
