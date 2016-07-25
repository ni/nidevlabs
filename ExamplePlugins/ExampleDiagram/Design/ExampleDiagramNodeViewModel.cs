using System;
using System.Linq;
using NationalInstruments.Composition;
using NationalInstruments.Core;
using NationalInstruments.Controls.Shell;
using NationalInstruments.Design;
using NationalInstruments.SourceModel;
using NationalInstruments.Shell;
using ExamplePlugins.ExampleNode.Model;

namespace ExamplePlugins.ExampleDiagram.Design
{
    /// <summary>
    /// The ViewModel for the Example Diagram Node
    /// The ViewModel controls the interaction with the user
    /// </summary>
    public class ExampleDiagramNodeViewModel : NodeViewModel
    {
        /// <summary>
        /// Constructs a new instance
        /// </summary>
        /// <param name="model">The model element we are bound to</param>
        public ExampleDiagramNodeViewModel(Node model) :
            base(model)
        {
        }

        /// <summary>
        /// Returns the Uri (resource location) for our node foreground image
        /// </summary>
        protected override ViewModelUri ForegroundUri
        {
            get { return new ViewModelUri(GetType(), "Resources/Cow"); }
        }

        /// <summary>
        /// Returns the render data for our node's foreground image
        /// </summary>
        public override NineGridData ForegroundImageData
        {
            get { return new ViewModelIconData(this) { ImageUri = ForegroundUri }; }
        }

        /// <summary>
        /// Creates the content to put into the command bar when the node is selected
        /// </summary>
        /// <param name="context">The current presentation context</param>
        public override void CreateCommandContent(ICommandPresentationContext context)
        {
            base.CreateCommandContent(context);
        }
    }
}
