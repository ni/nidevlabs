using System.Collections.Generic;
using System.Xml.Linq;
using NationalInstruments.Compiler;
using NationalInstruments.DataTypes;
using NationalInstruments.SourceModel;
using NationalInstruments.SourceModel.Persistence;

namespace ExamplePlugins.ExampleNode.Model
{
    /// <summary>
    /// This is derived from Node (The base class for a node on a Diagram) and 
    /// IDfirBuilderNodeProvider (Interface which says this node generates its own DFIR)
    ///
    /// This node shows one way to call a .NET method at runtime.  In general, using the .NET
    /// Interface document is a much easier way to do this.  However, if you need custom
    /// type propagation (semanatic analysis) code, this is an option.
    ///
    /// This node assumes the method is in an assembly that is already loaded.  If this is not
    /// the case, undefined behavior will result.
    /// </summary>
    public class CalculateTotalLengthNode : Node, IDfirBuilderNodeProvider
    {
        // Our Private Fields
        private Terminal _inputNamesTerminal;
        private Terminal _inputExtraNameTerminal;
        private Terminal _outputTerminal;

        /// <summary>
        /// This is the specific type identifier for the node
        /// </summary>
        public const string ElementName = "CalculateTotalLength";

        /// <summary>
        /// Creates the documentation for this node.
        /// Note: These string should be loaded from a localized resource
        /// </summary>
        /// <returns>the documentation</returns>
        protected override IDocumentation CreateDocumentation()
        {
            return new Documentation()
            {
                Description = "Calculates the total length of all the strings passed in",
                Name = "Calculate Total Length",
                InstanceName = "Calculate Total Length"
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
            if (terminal == _inputNamesTerminal)
            {
                return new Documentation() { Name = "names" };
            }
            else if (terminal == _inputExtraNameTerminal)
            {
                return new Documentation() { Name = "extra name" };
            }
            return new Documentation() { Name = "length" };
        }

        /// <summary>
        /// Our creation method.  This is used to create a new instance either programmatically, from load, and from the palette
        /// </summary>
        /// <param name="info">creation information.  This tells us why we are being created (new, load, ...)</param>
        /// <returns>The newly created node</returns>
        [XmlParserFactoryMethod(ElementName, ExamplePluginsNamespaceSchema.ParsableNamespaceName)]
        public static CalculateTotalLengthNode Create(IElementCreateInfo info)
        {
            var node = new CalculateTotalLengthNode();
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

            _inputNamesTerminal = new NodeTerminal(Direction.Input, PFTypes.StringArray1D, "names", TerminalHotspots.Input1);
            _inputExtraNameTerminal = new NodeTerminal(Direction.Input, PFTypes.String, "extra name", TerminalHotspots.Input2);
            _outputTerminal = new NodeTerminal(Direction.Output, PFTypes.Int32, "length", TerminalHotspots.CreateOutputTerminalHotspot(TerminalSize.Small, Width, 0));
            OnComponentInserted(_inputNamesTerminal);
            OnComponentInserted(_inputExtraNameTerminal);
            OnComponentInserted(_outputTerminal);
        }

        /// <summary>
        /// Returns all of the "Components" of this node.  In this case our components are just our terminals.
        /// </summary>
        public override IEnumerable<Element> Components
        {
            get
            {
                yield return _inputNamesTerminal;
                yield return _inputExtraNameTerminal;
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
            return new CalculateTotalLengthDfirNode(terminalInfo);
        }
    }
}