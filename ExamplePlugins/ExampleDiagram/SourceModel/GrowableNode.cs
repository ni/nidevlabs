using System.Collections.Generic;
using System.Xml.Linq;
using NationalInstruments.DataTypes;
using NationalInstruments.DynamicProperties;
using NationalInstruments.SourceModel;
using NationalInstruments.SourceModel.Persistence;

namespace ExamplePlugins.ExampleDiagram.SourceModel
{
    /// <summary>
    /// This is the model of a simple vertically growable node.  When the size of this node changes
    /// terminals are added or removed to match the new size of the node.  Other than size configuration
    /// this node nodes not have any other properties.
    /// </summary>
    public class GrowableNode : VerticalGrowNode, IViewVerticalGrowNode
    {
        private const string ElementName = "GrowableNode";

        /// <summary>
        /// This is the standard property for vertical grow nodes which manages the terminals
        /// </summary>
        public static readonly PropertySymbol NodeTerminalsPropertySymbol =
            ExposeReadOnlyVariableNodeTerminalsProperty<GrowableNode>(PropertySerializers.NodeTerminalsAllVariableReferenceSerializer);

        /// <summary>
        /// This is the standard property which mananges the number of vertical chunks
        /// </summary>
        public static readonly PropertySymbol VerticalChunkCountPropertySymbol =
            ExposeVerticalChunkCountProperty<GrowableNode>(1);

        /// <summary>
        /// The standard constructor.  To construct a new instance use the static Create method to enable
        /// two pass creation
        /// </summary>
        protected GrowableNode()
        {
            // Set the Width. The height is set when SetVerticalChunkCount is called.
            Width = 40;

            // Add the fixed terminals
            AddComponent(new NodeTerminal(Direction.Output, NITypes.Void, "element", TerminalHotspots.CreateOutputTerminalHotspot(TerminalSize.Small, Width, 0)));
            this.SetVerticalChunkCount(1, GrowNodeResizeDirection.Bottom);
            this.RecalculateNodeHeight();
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
        /// This an exported factory method used to construct instances of this node.
        /// This is used to create a new instance either programmatically, from load, and from the palette (merge script)
        /// </summary>
        /// <param name="info">creation information.  This tells us why we are being created (new, load, ...)</param>
        /// <returns>The newly created node</returns>
        [XmlParserFactoryMethod(ElementName, ExamplePluginsNamespaceSchema.ParsableNamespaceName)]
        public static GrowableNode Create(IElementCreateInfo elementCreateInfo)
        {
            var node = new GrowableNode();
            node.Init(elementCreateInfo);
            return node;
        }

        /// <summary>
        /// The number of terminals that this node has that are static and aren't part of the growable area.
        /// </summary>
        public override int FixedTerminalCount
        {
            get { return 1; }
        }

        /// <summary>
        /// Smallest possible value of VerticalChunkCount if all of the terminals were unwired.
        /// Usually equal to 1, or sometimes 0.
        /// </summary>
        public override int MinimumVerticalChunkCount
        {
            get { return 2; }
        }


        /// <summary>
        /// The area at the top of the grow node that is used as a space for the terminals that don't belong
        /// to a chunk- only fixed terminals.  Note that if TopMargin is 0, we can still have fixed terminals
        /// because the minimum chunk count might be >= 1.  The offsets of the fixed terminals never change.
        /// </summary>
        public float TopMargin
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// The margin between the bottom of the last chunk and the bottom of the node.
        /// </summary>
        public float BottomMargin
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Calculates the height for a given chunk
        /// </summary>
        /// <param name="chunkIndex">the chunk index (useful for dynamic chunk nodes; you can pass -1 if it's not a dynamic chunk node)</param>
        /// <returns>the height of the chunk in question</returns>
        public float GetVerticalChunkHeight(int chunkIndex)
        {
            return 20;
        }

        /// <summary>
        /// Calculates the vertical offset from the top of the node of a given chunk.
        /// When all terminals are the same height, this should be TopMargin + (chunkIndex * GetVerticalChunkHeight(-1))
        /// </summary>
        /// <param name="chunkIndex">0-based index of the chunk in question.</param>
        /// <returns>The vertical offset from the top of the node of the chunk in question</returns>
        public float OffsetForVerticalChunk(int chunkIndex)
        {
            return TopMargin + chunkIndex * this.GetFixedSizeVerticalChunkHeight();
        }

        /// <summary>
        /// Calculates the height of a node for a given chunk count.
        /// When all terminals are the same height, this should be TopMargin + (chunkCount * GetVerticalChunkHeight(-1))
        /// </summary>
        /// <param name="chunkCount">Count of chunks</param>
        /// <returns>The height in pixels of the node for the given chunk count</returns>
        public float NodeHeightForVerticalChunkCount(int chunkCount)
        {
            return TopMargin + chunkCount * this.GetFixedSizeVerticalChunkHeight() + BottomMargin;
        }

        /// <inheritdoc/>
        public float TerminalHeight
        {
            get { return Template == ViewElementTemplate.List ? StockDiagramGeometries.LargeTerminalHeight : StockDiagramGeometries.StandardTerminalHeight; }
        }

        /// <inheritdoc/>
        public float TerminalHotspotVerticalOffset
        {
            get { return TerminalHotspots.HotspotVerticalOffsetForTerminalSize(TerminalSize.Small); }
        }

        /// <summary>
        /// Creates a new chunk of terminals for a given chunk.
        /// </summary>
        /// <param name="chunkIndex">the chunk index (useful for dynamic chunk nodes; you can pass -1 if it's not a dynamic chunk node)</param>
        /// <returns>new terminals created by this function</returns>
        public override IList<WireableTerminal> CreateTerminalsForVerticalChunk(int chunkIndex)
        {
            return new List<WireableTerminal>
            {
                new NodeTerminal(Direction.Input, NITypes.Void, "element", TerminalHotspots.CreateInputTerminalHotspot(TerminalSize.Small, 0))
            };
        }

        /// <summary>
        /// The number of terminals in a chunk.
        /// </summary>
        /// <param name="chunkIndex">the chunk index (useful for dynamic chunk nodes; you can pass -1 if it's not a dynamic chunk node)</param>
        /// <returns>the number of terminals</returns>
        public override int GetNumberOfTerminalsInVerticalChunk(int chunkIndex)
        {
            return 1;
        }
    }
}
