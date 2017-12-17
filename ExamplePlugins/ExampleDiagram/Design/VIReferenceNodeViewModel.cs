using System.Windows.Media;
using ExamplePlugins.ExampleDiagram.SourceModel;
using NationalInstruments.Core;
using NationalInstruments.Design;
using NationalInstruments.SourceModel;

namespace ExamplePlugins.ExampleDiagram.Design
{
    /// <summary>
    /// ViewModel for the VIReference Node
    /// </summary>
    class VIReferenceNodeViewModel : NodeViewModel
    {
        /// <summary>
        /// Constructs a new instance
        /// </summary>
        /// <param name="model">The model element we are bound to</param>
        public VIReferenceNodeViewModel(Node model) :
            base(model)
        {
        }

        /// <summary>
        /// Called to create our custom view
        /// </summary>
        /// <param name="parent">Parent visual</param>
        /// <returns>newly created view</returns>
        public override PlatformVisual CreateView(PlatformVisual parent)
        {
            var view = new VIReferenceNodeView();
            view.DataContext = this;
            return view;
        }

        /// <summary>
        /// Gets the icon to draw.  This will be the icon of the VI we are referencing
        /// </summary>
        public ImageSource Icon
        {
            get
            {
                return ((VIReferenceNode)Model).Icon;
            }
        }

        /// <inheritdoc />
        public override void ModelPropertyChanged(Element modelElement, string propertyName, TransactionItem transactionItem)
        {
            base.ModelPropertyChanged(modelElement, propertyName, transactionItem);
            if (propertyName == "Icon")
            {
                // Notify the view that the icon changed
                NotifyPropertyChanged("Icon");
            }
        }
    }
}
