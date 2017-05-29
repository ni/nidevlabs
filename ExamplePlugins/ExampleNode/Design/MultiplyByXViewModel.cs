using System.Collections.Generic;
using System.Linq;
using NationalInstruments.Composition;
using NationalInstruments.Core;
using NationalInstruments.Controls.Shell;
using NationalInstruments.Design;
using NationalInstruments.SourceModel;
using NationalInstruments.Shell;
using ExamplePlugins.ExampleNode.Model;

namespace ExamplePlugins.ExampleNode.Design
{
    /// <summary>
    /// The ViewModel for the Multiply By X Node
    /// The ViewModel controls the interaction with the user
    /// </summary>
    public class MultiplyByXViewModel : NodeViewModel
    {
        public static readonly ICommandEx ConfigureGroup = new ShellRelayCommand()
        {
            UniqueId = "ExamplePlugins.ConfigureGroup",
            LabelTitle = "Multiply By X Configuration"
        };

        public static readonly ICommandEx MultiplierCommand = new ShellSelectionRelayCommand(OnMultiplierChanged, OnUpdateMultiplierChanged)
        {
            UniqueId = "ExamplePlugins.MultiplierCommand",
            LabelTitle = "Multiplier",
            UIType =  UITypeForCommand.TextBox
        };

        /// <summary>
        /// Constructs a new instance
        /// </summary>
        /// <param name="model">The model element we are bound to</param>
        public MultiplyByXViewModel(Node model) :
            base(model)
        {            
        }

        /// <summary>
        /// Returns the Uri (resource location) for our node foreground image
        /// </summary>
        protected override ResourceUri ForegroundUri
        {
            get { return new ResourceUri(GetType(), "Resources/MultiplyByXNode"); }
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

            using (context.AddConfigurationPaneContent())
            {
                using (context.AddGroup(ConfigureGroup))
                {
                    context.Add(MultiplierCommand);
                }
            }
        }

        public static bool OnUpdateMultiplierChanged(ICommandParameter parameter, IEnumerable<IViewModel> selection, ICompositionHost host, DocumentEditSite site)
        {
            ((TextCommandParameter)parameter).Text = ((MultiplyByXViewModel)selection.First()).Multiplier.ToString();
            return true;
        }

        public static void OnMultiplierChanged(ICommandParameter parameter, IEnumerable<IViewModel> selection, ICompositionHost host, DocumentEditSite site)
        {
            ((MultiplyByXViewModel) selection.First()).Multiplier = double.Parse(((TextCommandParameter) parameter).Text);
        }

        /// <summary>
        /// Gets and sets the current Multiplier for the node
        /// </summary>
        public double Multiplier
        {
            get
            {
                return ((MultiplyByXNode)Model).Multiplier;
            }
            set
            {
                // When setting the multiplier we must use a transaction which makes the change undoable
                // and protects the model to multi-thread access
                using (
                    var transaction = Model.TransactionManager.BeginTransaction("Set Multiplier",
                        TransactionPurpose.User))
                {
                    ((MultiplyByXNode) Model).Multiplier = value;
                    transaction.Commit();
                }
            }
        }
    }
}
