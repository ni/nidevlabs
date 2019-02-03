using System.ComponentModel.Composition;
using NationalInstruments.Composition;
using NationalInstruments.Shell;

namespace ExamplePlugins.ExampleCustomButtonsToolWindow
{
    /// <summary>
    /// View model for the Custom Buttons window
    /// </summary>
    internal class CustomButtonsToolWindowViewModel : ToolWindowViewModelBase
    {
        /// <summary>
        /// Constructor for the Custom Buttons window view model
        /// </summary>
        public CustomButtonsToolWindowViewModel()
        {
            CreateViewHandler = () =>
            {
                var toolWindow = new CustomButtonsToolWindow
                {
                    DataContext = this
                };
                toolWindow.Initialize(Host);
                return toolWindow;
            };
        }

        /// <summary>
        /// Our composition host.  The composition host provides access to the rest of the system.  There is one
        /// host per instance of our editor and should never be stored globally.  It manages all of the plug-ins
        /// in the system and well as provide access to the framework objects.
        ///
        /// The host is imported using MEF.
        /// </summary>
        [Import]
        internal ICompositionHost Host { get; set; }
    }
}