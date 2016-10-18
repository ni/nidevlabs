using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.VI.Design;
using NationalInstruments.Shell;
using ExamplePlugins.ExampleDiagram.SourceModel;
using NationalInstruments.SourceModel;
using NationalInstruments.MocCommon.SourceModel;
using NationalInstruments.MocCommon.Design;

namespace ExamplePlugins.ExampleDiagram.Design
{
    /// <summary>
    /// Our view model provider which maps model elements to their view models
    /// </summary>
    [ExportProvideViewModels(typeof(ExampleDiagramEditControl))]
    public class ExamplePluginsViewModelProvider : ViewModelProvider
    {
        public ExamplePluginsViewModelProvider()
        {
            AddSupportedModel<BasicNode>(e => new BasicNodeViewModel(e));
            AddSupportedModel<GrowableNode>(e => new GrowableNodeViewModel(e));
            AddSupportedModel<InteractiveNode>(e => new InteractiveNodeViewModel(e));
            AddSupportedModel<Wire>(e => new ExampleDiagramWireViewModel(e));
            AddSupportedModel<Comment>(e => new CommentViewModel(e));
        }
    }
}
