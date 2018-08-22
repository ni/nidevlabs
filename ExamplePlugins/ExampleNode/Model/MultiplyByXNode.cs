using System.Collections.Generic;
using System.Xml.Linq;
using NationalInstruments.Compiler;
using NationalInstruments.DynamicProperties;
using NationalInstruments.DataTypes;
using NationalInstruments.SourceModel;
using NationalInstruments.SourceModel.Persistence;

namespace ExamplePlugins.ExampleNode.Model
{
    /// <summary>
    /// The model for the MultiplyByX Node
    /// The model is responsible for the non UI management of of the node
    /// This includes, managing state (current value, undo, redo, ...), Persistence, And SourceLevel DFIR generation
    /// 
    /// This is derived from Node (The base class for a node on a Diagram) and 
    /// IDfirBuilderNodeProvider (Interface which says this node generates its own DFIR)
    /// </summary>
    [ExportOverloadInformation(NationalInstruments.Dfir.CompoundArithmeticNode.MultiplyOverloadGroup)]
    public class MultiplyByXNode : Node, IDfirBuilderNodeProvider
    {
        // Our Private Fields
        private double _multiplier;
        private Terminal _inputTerminal;
        private Terminal _outputTerminal;

        // Define a PropertySymbol for all model properties that are settable
        // The symbol is used for generic discovery of all properties of model elements.  This is used for things
        // like search and most importantly Persistence
        public static readonly PropertySymbol MultiplierSymbol = ExposeStaticProperty<MultiplyByXNode>(
                "Multiplier",
                obj => obj.Multiplier,
                (obj, value) => obj.Multiplier = (double)value,
                PropertySerializers.DoubleSerializer,
                10.0);

        /// <summary>
        /// This is the specific type identifier for the node
        /// </summary>
        public const string ElementName = "MultiplyByX";

        /// <summary>
        /// The standard constructor.  To construct a new instance use the static Create method to enable
        /// two pass creation
        /// </summary>
        protected MultiplyByXNode()
        {
            _multiplier = 10.0;
        }

        /// <summary>
        /// Creates the documentation for this node.
        /// Note: These string should be loaded from a localized resource
        /// </summary>
        /// <returns>the documentation</returns>
        protected override IDocumentation CreateDocumentation()
        {
            return new Documentation()
            {
                Description = "Multiplies the input by the configured value",
                Name = "Multiply By X",
                InstanceName = "Multiply By X"
            };
        }

        /// <summary>
        /// Creates the documentation for one of our terminals
        /// Note: These string should be loaded from a localized resource
        /// </summary>
        /// <param name="terminal">The terminal to create documentation for</param>
        /// <returns>the documentation</returns>
        public override IDocumentation CreateDocumentationForTerminal(Terminal terminal)
        {
            if (terminal == _inputTerminal)
            {
                return new Documentation() { Name = "Input" };
            }
            return new Documentation() { Name = "Result" };
        }

        /// <summary>
        /// Property to get and set the Multiplier property (What value we are going to multiply the input by)
        /// </summary>
        public double Multiplier
        {
            get { return _multiplier; }
            set
            {
                // This "transacts" the setting of a new value.  This enables undo / redo and triggers a recompile of the VI
                var oldValue = _multiplier;
                _multiplier = value;
                TransactionRecruiter.EnlistPropertyItem(this, "Multiplier", oldValue, _multiplier, (v, _) => _multiplier = v, TransactionHints.Semantic);
            }
        }
        
        /// <summary>
        /// Our create this.  This is used to create a new instance either programmatically, from load, and from the palette
        /// </summary>
        /// <param name="info">creation information.  This tells us why we are being created (new, load, ...)</param>
        /// <returns>The newly created node</returns>
        [XmlParserFactoryMethod(ElementName, ExamplePluginsNamespaceSchema.ParsableNamespaceName)]
        public static MultiplyByXNode Create(IElementCreateInfo info)
        {
            var node = new MultiplyByXNode();
            node.Init(info);
            return node;
        }

        /// <summary>
        /// Returns the persistence name of this node
        /// </summary>
        public override XName XmlElementName
        {
            get
            {
                return XName.Get(ElementName, ExamplePluginsNamespaceSchema.ParsableNamespaceName);
            }
        }

        /// <summary>
        /// Performs node initialization
        /// </summary>
        /// <param name="info">Information about why the node is being created</param>
        protected override void Init(IElementCreateInfo info)
        {
            base.Init(info);
            Width = StockDiagramGeometries.StandardNodeWidth;
            Height = StockDiagramGeometries.StandardNodeHeight;

            _inputTerminal = new NodeTerminal(Direction.Input, PFTypes.Double, "Input", TerminalHotspots.CreateInputTerminalHotspot(TerminalSize.Small, 0));
            _outputTerminal = new NodeTerminal(Direction.Output, PFTypes.Double, "Result", TerminalHotspots.CreateOutputTerminalHotspot(TerminalSize.Small, Width, 0));
            OnComponentInserted(_inputTerminal);
            OnComponentInserted(_outputTerminal);
        }

        /// <summary>
        /// Returns all of the "Components" of this node.  In this case our components are just our terminals.
        /// </summary>
        public override IEnumerable<Element> Components
        {
            get
            {
                yield return _inputTerminal;
                yield return _outputTerminal;
            }
        }

        /// <summary>
        /// Obtain a Dfir node that represents the plugin node. This method will create a Dfir node with
        /// no parent. The builder will place the node on the correct diagram.
        /// </summary>
        /// <param name="terminalInfo">The association map between source model terminals and Dfir terminals.</param>
        /// <param name="helper">Helpers for building the plugin node's Dfir</param>
        /// <returns>The plugin that provides the implementation for this node.</returns>
        public NationalInstruments.Dfir.Node BuildDfirNode(NationalInstruments.Dfir.Plugin.PluginNodeTerminalInfo terminalInfo, IPluginNodeDfirBuilderHelper helper)
        {
            return new MultiplyByXDfirNode(terminalInfo, _multiplier);
        }
    }
}
