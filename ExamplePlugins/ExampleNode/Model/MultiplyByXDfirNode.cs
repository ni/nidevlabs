using System.Threading.Tasks;
using NationalInstruments.Compiler;
using NationalInstruments.Dfir;
using NationalInstruments.Dfir.Plugin;
using NationalInstruments.Compiler.SemanticAnalysis;
using NationalInstruments.Core;
using NationalInstruments.DataTypes;
using NationalInstruments.CommonModel;

namespace ExamplePlugins.ExampleNode.Model
{
    /// <summary>
    /// The DFIR node for the Multiply By X node
    /// </summary>
    public class MultiplyByXDfirNode : PluginNode, IOverloadGroup
    {
        // The multiplier to use when generating code
        private double _multiplier;

        /// <summary>
        /// Constructs a new instance (used by our source model node)
        /// </summary>
        /// <param name="terminalInfo">Our terminal information</param>
        /// <param name="multiplier">The multiplier to use for code generation</param>
        public MultiplyByXDfirNode(PluginNodeTerminalInfo terminalInfo, double multiplier) :
            base(null, terminalInfo)
        {
            _multiplier = multiplier;
        }

        /// <summary>
        /// Constructor used when making a copy of this node
        /// </summary>
        /// <param name="parentNode">The parent node of this node (the diagram)</param>
        /// <param name="nodeToCopy">The source node</param>
        /// <param name="nodeCopyInfo">Information to forward to the copy</param>
        /// <param name="multiplier">The multiplier to use</param>
        public MultiplyByXDfirNode(Node parentNode, MultiplyByXDfirNode nodeToCopy, NodeCopyInfo nodeCopyInfo, double multiplier)
            : base(parentNode, nodeToCopy, nodeCopyInfo)
        {
            _multiplier = multiplier;
        }

        /// <summary>
        /// Called to perform type propagation (semantic analysis)
        /// </summary>
        /// <param name="semanticAnalysisAccess">Helper interface to access aspects of type propagation</param>
        /// <param name="cancellationToken">Token indicating whether compile has been cancelled.</param>
        public override Task DoTypePropagationAsync(ITypePropagationAccessor semanticAnalysisAccess, CompileCancellationToken cancellationToken)
        {
            // this simple example requires all inputs to be wired and forces all types to double
            foreach (Terminal terminal in Terminals)
            {
                if (terminal.Direction == Direction.Input)
                {
                    terminal.TestRequiredTerminalConnected();
                }
                terminal.DataType = NITypes.Double;
            }
            return AsyncHelpers.CompletedTask;
        }

        /// <summary>
        /// Makes a copy of this node and adds it to the specified parent
        /// </summary>
        /// <param name="newParentNode">parent node for the new node</param>
        /// <param name="copyInfo">Copy information to forward</param>
        /// <returns>The newly created copy</returns>
        protected override Node CopyNodeInto(Node newParentNode, NodeCopyInfo copyInfo)
        {
            var copiedNode = new MultiplyByXDfirNode(newParentNode, this, copyInfo, _multiplier);
            return copiedNode;
        }

        /// <summary>
        /// Performs the code generation
        /// </summary>
        /// <param name="diagram">The diagram this node is on</param>
        /// <param name="terminalAssociator">Our terminal association manager</param>
        /// <param name="targetInfo">Semantic analysis target</param>
        /// <param name="cancellationToken">Cancelation token</param>
        public override Task DecomposeAsync(Diagram diagram, DecompositionTerminalAssociator terminalAssociator, ISemanticAnalysisTargetInfo targetInfo, CompileCancellationToken cancellationToken)
        {
            // Create a constant with our multiplier
            Constant constNode = Constant.Create(diagram, _multiplier, NITypes.Double);

            // Add a multiply primitive
            MultiplyPrimitive prim = MultiplyPrimitive.Create(diagram);

            // connect our input terminal to the first input of the multiply node
            terminalAssociator.AssociateTerminalByName("Input", prim.XTerminal);

            // Wire the constant to the second input of the multiply node
            Wire wire = Wire.Create(diagram, new Terminal[] {constNode.OutputTerminal, prim.YTerminal});

            // Connect the multiply output to our output
            terminalAssociator.AssociateTerminalByName("Result", prim.XTimesYTerminal);

            return AsyncHelpers.CompletedTask;
        }

        /// <summary>
        /// Gets whether the each node decomposition should be housed within a frame.
        /// </summary>
        /// <remarks>
        /// Standard nodes synchronize at node boundaries and do not start to execute until all inputs have arrived, but this can limit what transformations can be performed on scripted code.
        /// </remarks>
        public override bool SynchronizeAtNodeBoundaries
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the overload group this node is associated with. If a node does not participate in Node Overloading this will return null.
        /// </summary>
        public string OverloadGroup
        {
            get
            {
                return NationalInstruments.Dfir.CompoundArithmeticNode.MultiplyOverloadGroup;
            }
        }

        /// <summary>
        /// Gets the globally unique identifier for this Dfir node within this overload group. If a node does not participate in Node Overloading this will return null.
        /// </summary>
        /// <remarks>
        /// Overload disambiguation will not allow nodes from the same OverloadGroupSubId to compete with each other.
        /// </remarks>
        public string OverloadGroupSubId
        {
            get { return typeof(MultiplyByXDfirNode).ToString(); }
        }

        /// <summary>
        /// original node that this node is overloading
        /// </summary>
        public Node OriginalNode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the SideIndex for a terminal that is owned by this Node.  This is needed so the overload disambiguator
        /// can know which terminals correspond to each other in different nodes/VIs.
        /// </summary>
        /// <param name="terminal">The terminal, which is owned by this node.</param>
        /// <returns>The SideIndex in the source model for this terminal.</returns>
        public SideIndex GetSideIndexFromTerminal(Terminal terminal)
        {
            return OverloadHelpers.GetSideIndexFromTerminalDefault(Terminals, terminal);
        }

        /// <summary>
        /// Gets when the node needs to be decomposed.
        /// </summary>
        /// <param name="targetInfo">Target on which this node will be decomposed.</param>
        /// <returns>When the node should decompose.</returns>
        public override DecomposeStrategy DecomposeWhen(ISemanticAnalysisTargetInfo targetInfo)
        {
            return DecomposeStrategy.AfterSemanticAnalysis;
        }
    }
}
