using System.Collections.Generic;
using System.Xml.Linq;
using NationalInstruments.DataTypes;
using NationalInstruments.SourceModel;
using NationalInstruments.SourceModel.Persistence;

namespace ExamplePlugins.ExampleDiagram.SourceModel
{
    /// <summary>
    /// This is the model for a basic node.  This node has 4 terminals that can be wired together.  It correctly saves and loads
    /// but does not have any configuration.
    /// </summary>
    public class BasicNode : Node
    {
        // this node has a fixed set of terminals and they are stored as fields
        private Terminal _input1Terminal;
        private Terminal _input2Terminal;
        private Terminal _output1Terminal;
        private Terminal _output2Terminal;

        /// <summary>
        /// This is the specific type identifier for the node.  T
        /// This is what will identify the node in persisted content.
        /// </summary>
        public const string ElementName = "BasicNode";

        /// <summary>
        /// The standard constructor.  To construct a new instance use the static Create method to enable
        /// two pass creation
        /// </summary>
        protected BasicNode()
        {
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
                Description = "A Basic Example Node for the Example Diagram",
                Name = "Basic Node",
                InstanceName = "Basic Node"
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
            if (terminal == _input1Terminal)
            {
                return new Documentation() { Name = "Input 1" };
            }
            if (terminal == _input2Terminal)
            {
                return new Documentation() { Name = "Input 2" };
            }
            if (terminal == _output1Terminal)
            {
                return new Documentation() { Name = "Result 1" };
            }
            return new Documentation() { Name = "Result 1" };
        }

        /// <summary>
        /// This an exported factory method used to construct instances of this node.
        /// This is used to create a new instance either programmatically, from load, and from the palette (merge script)
        /// </summary>
        /// <param name="info">creation information.  This tells us why we are being created (new, load, ...)</param>
        /// <returns>The newly created node</returns>
        [XmlParserFactoryMethod(ElementName, ExamplePluginsNamespaceSchema.ParsableNamespaceName)]
        public static BasicNode Create(IElementCreateInfo info)
        {
            var node = new BasicNode();
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
            _input1Terminal = new NodeTerminal(Direction.Unknown, PFTypes.Void, "Input 1", TerminalHotspots.CreateInputTerminalHotspot(TerminalSize.Small, 0));
            _input2Terminal = new NodeTerminal(Direction.Unknown, PFTypes.Void, "Input 1", TerminalHotspots.CreateInputTerminalHotspot(TerminalSize.Small, 3));
            _output1Terminal = new NodeTerminal(Direction.Unknown, PFTypes.Void, "Output 1", TerminalHotspots.CreateOutputTerminalHotspot(TerminalSize.Small, Width, 0));
            _output2Terminal = new NodeTerminal(Direction.Unknown, PFTypes.Void, "Output 1", TerminalHotspots.CreateOutputTerminalHotspot(TerminalSize.Small, Width, 3));
            OnComponentInserted(_input1Terminal);
            OnComponentInserted(_input2Terminal);
            OnComponentInserted(_output1Terminal);
            OnComponentInserted(_output2Terminal);
        }

        /// <summary>
        /// Returns all of the "Components" of this node.  In this case our components are just our terminals.
        /// </summary>
        public override IEnumerable<Element> Components
        {
            get
            {
                yield return _input1Terminal;
                yield return _input2Terminal;
                yield return _output1Terminal;
                yield return _output2Terminal;
            }
        }
    }
}