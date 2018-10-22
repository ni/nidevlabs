using NationalInstruments.Core;
using NationalInstruments.Design;
using NationalInstruments.SourceModel;

namespace ExamplePlugins.ExampleNode.Design
{
    /// <summary>
    /// The ViewModel for the Calculate Total Length Node
    /// The ViewModel controls the interaction with the user
    /// </summary>
    public class CalculateTotalLengthViewModel : NodeViewModel
    {
        /// <summary>
        /// Constructs a new instance
        /// </summary>
        /// <param name="model">The model element we are bound to</param>
        public CalculateTotalLengthViewModel(Node model) :
            base(model)
        {            
        }

        /// <summary>
        /// Returns the Uri (resource location) for our node foreground image
        /// </summary>
        protected override ResourceUri ForegroundUri
        {
            get { return new ResourceUri(GetType(), "Resources/Llama"); }
        }

        /// <summary>
        /// Returns the render data for our node's foreground image
        /// </summary>
        public override NineGridData ForegroundImageData
        {
            get { return new ViewModelIconData(this) { ImageUri = ForegroundUri }; }
        }
    }
}
