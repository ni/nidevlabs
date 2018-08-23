using System.Collections.Generic;
using System.ComponentModel.Composition;
using NationalInstruments.Composition;
using NationalInstruments.Core;

namespace ExamplePlugins.ExampleCustomButtonsToolWindow
{
    /// <summary>
    /// Interaction logic for CustomButtonsToolWindow.xaml
    /// </summary>
    internal partial class CustomButtonsToolWindow
    {
        /// <summary>
        /// Constructor for the tool window
        /// </summary>
        public CustomButtonsToolWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialize the control and load the custom buttons.
        /// </summary>
        /// <param name="host">The composition host</param>
        internal void Initialize(ICompositionHost host)
        {
            IEnumerable<CustomButton> customButtons;
            try
            {
                customButtons = host.GetNonSharedExportedValues<CustomButton>();
            }
            catch (CompositionException e)
            {
                Log.WriteLine(e.Message);
                return;
            }

            foreach (CustomButton customButton in customButtons)
            {
                customButton.Initialize(host);
                Panel.Children.Add(customButton);
            }
        }
    }
}