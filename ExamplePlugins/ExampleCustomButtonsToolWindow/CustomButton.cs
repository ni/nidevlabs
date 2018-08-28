using System.Windows.Controls;
using NationalInstruments.Composition;
using NationalInstruments.Controls.Shell;

namespace ExamplePlugins.ExampleCustomButtonsToolWindow
{
    /// <summary>
    /// A custom button. Implement this abstract class and export it for your button to show up in the <see cref="CustomButtonsToolWindow"/>.
    /// Override <see cref="Button.OnClick"/> to define custom behavior when the button is clicked.
    /// </summary>
    public abstract class CustomButton : ShellButton
    {
        public abstract void Initialize(ICompositionHost host);
    }
}