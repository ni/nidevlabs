using ExamplePlugins.ExampleDiagram.SourceModel;
using NationalInstruments.Core;
using NationalInstruments.Design;
using NationalInstruments.SourceModel;

namespace ExamplePlugins.ExampleDiagram.Design
{
    /// <summary>
    /// The view model for the example growable node
    /// This class provides interaction logic and rendering information
    /// </summary>
    public class GrowableNodeViewModel : GrowNodeViewModel
    {
        /// <summary>
        /// Constructs a new instance
        /// </summary>
        /// <param name="element">The associated model element</param>
        public GrowableNodeViewModel(GrowableNode element) : 
            base(element)
        {
        }

        /// <summary>
        /// Returns the Uri (resource location) for our node foreground image
        /// </summary>
        protected override ResourceUri ForegroundUri
        {
            get
            {
                // We are loading the vector ninegrid out of a resource
                // this will be rendered on the default node visual.
                return new ResourceUri(GetType(), "Resources/Llama");
            }
        }

        /// <summary>
        /// Returns the render data for our node's foreground image
        /// </summary>
        public override NineGridData ForegroundImageData
        {
            get { return new ViewModelIconData(this) { ImageUri = ForegroundUri, VerticalAlignment = System.Windows.VerticalAlignment.Top }; }
        }
    }
}
