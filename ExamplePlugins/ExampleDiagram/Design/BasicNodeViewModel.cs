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
    /// The ViewModel for the Basic Diagram Node
    /// The ViewModel controls the interaction with the user
    /// This node uses the standard static node view for the actual UI so there is not
    /// a UI class that needs to be implemented.
    /// </summary>
    public class BasicNodeViewModel : NodeViewModel
    {
        /// <summary>
        /// Constructs a new instance
        /// </summary>
        /// <param name="model">The model element we are bound to</param>
        public BasicNodeViewModel(Node model) :
            base(model)
        {
        }

        /// <summary>
        /// Returns the Uri (resource location) for our node foreground image
        /// </summary>
        protected override ViewModelUri ForegroundUri
        {
            get
            {
                // We are loading the vector ninegrid out of a resource
                // this will be rendered on the default node visual.
                return new ViewModelUri(GetType(), "Resources/Cow");
            }
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
