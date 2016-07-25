using System.Threading.Tasks;
using NationalInstruments.Compiler;
using NationalInstruments.Dfir;
using NationalInstruments.Dfir.Plugin;
using NationalInstruments.Compiler.SemanticAnalysis;
using NationalInstruments.Core;
using NationalInstruments.DataTypes;

namespace ExamplePlugins.ExampleNode.Model
{
    /// <summary>
    /// The DFIR node for the Multiply By X node
    /// </summary>
    public class MultiplyByXDfirNode : PluginNode
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
        /// <param name="node">node to type prop</param>
        /// <param name="semanticAnalysisAccess">Helper interface to access aspects of type propagation</param>
        /// <param name="cancellationToken">Token indicating whether compile has been cancelled.</param>
        public override Task DoTypePropagationAsync(Node node, ITypePropagationAccessor semanticAnalysisAccess, CompileCancellationToken cancellationToken)
        {
            // this simple example requires all inputs to be wired and forces all types to double
            foreach (Terminal terminal in node.Terminals)
            {
                if (terminal.Direction == Direction.Input)
                {
                    terminal.TestRecommendedTerminalConnected();
                }
                terminal.DataType = PFTypes.Double;
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
            Constant constNode = Constant.Create(diagram, _multiplier, PFTypes.Double);

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

        /// <inheritdoc/>
        public override bool SynchronizeAtNodeBoundaries
        {
            get { return false; }
        }

        /// <inheritdoc/>
        public override DecomposeStrategy DecomposeWhen(ISemanticAnalysisTargetInfo targetInfo)
        {
            return DecomposeStrategy.AfterSemanticAnalysis;
        }
    }
}
