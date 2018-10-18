using NationalInstruments.VI.Design;
using NationalInstruments.Shell;
using ExamplePlugins.ExampleNode.Model;

namespace ExamplePlugins.ExampleNode.Design
{
    /// <summary>
    /// Our view model provider which maps model elements to their view models
    /// </summary>
    [ExportProvideViewModels(typeof(VIDiagramControl))]
    public class ExamplePluginsViewModelProvider : ViewModelProvider
    {
        public ExamplePluginsViewModelProvider()
        {
            AddSupportedModel<MultiplyByXNode>(e => new MultiplyByXViewModel(e));
            AddSupportedModel<CalculateTotalLengthNode>(e => new CalculateTotalLengthViewModel(e));
        }
    }
}
