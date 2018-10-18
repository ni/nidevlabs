using System.Linq;
using System.Threading.Tasks;
using NationalInstruments.Compiler;
using NationalInstruments.Compiler.SemanticAnalysis;
using NationalInstruments.Core;
using NationalInstruments.DataTypes;
using NationalInstruments.Dfir;
using NationalInstruments.Dfir.Plugin;

namespace ExamplePlugins.ExampleNode.Model
{
    /// <summary>
    /// The DFIR node for the Calculate Total Length node
    /// See notes on the <see cref="CalculateTotalLengthNode"/> class for caveats.
    /// </summary>
    public class CalculateTotalLengthDfirNode : PluginNode
    {
        /// <summary>
        /// Constructs a new instance (used by our source model node)
        /// </summary>
        /// <param name="terminalInfo">Our terminal information</param>
        public CalculateTotalLengthDfirNode(PluginNodeTerminalInfo terminalInfo) : base(null, terminalInfo)
        {
        }

        /// <summary>
        /// Constructor used when making a copy of this node
        /// </summary>
        /// <param name="parentNode">The parent node of this node (the diagram)</param>
        /// <param name="nodeToCopy">The source node</param>
        /// <param name="nodeCopyInfo">Information to forward to the copy</param>
        public CalculateTotalLengthDfirNode(Node parentNode, CalculateTotalLengthDfirNode nodeToCopy, NodeCopyInfo nodeCopyInfo)
            : base(parentNode, nodeToCopy, nodeCopyInfo)
        {
        }

        /// <summary>
        /// Called to perform type propagation (semantic analysis)
        /// </summary>
        /// <param name="node">node to type prop</param>
        /// <param name="typePropagationAccessor">Helper interface to access aspects of type propagation</param>
        /// <param name="cancellationToken">Token indicating whether compile has been cancelled.</param>
        public override Task DoTypePropagationAsync(Node node, ITypePropagationAccessor typePropagationAccessor, CompileCancellationToken cancellationToken)
        {
            GetTerminalByName("names").DataType = PFTypes.StringArray1D;
            GetTerminalByName("extra name").DataType = PFTypes.String;
            GetTerminalByName("length").DataType = PFTypes.Int32;
            // this simple example requires all inputs to be wired
            foreach (Terminal terminal in node.Terminals)
            {
                if (terminal.Direction == Direction.Input)
                {
                    terminal.TestRequiredTerminalConnected();
                }
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
            var copiedNode = new CalculateTotalLengthDfirNode(newParentNode, this, copyInfo);
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
            NIType methodType = CreateMethodType();
            var methodCall = MethodCall.CreateWithErrorTerminals(diagram, methodType.GetDeclaringType(), methodType);
            methodCall.UpdateTerminals();
            terminalAssociator.AssociateTerminalByName("names", methodCall.GetTerminalByName("names"));
            terminalAssociator.AssociateTerminalByName("extra name", methodCall.GetTerminalByName("extraName"));
            terminalAssociator.AssociateTerminalByName("length", methodCall.OutputTerminals.Last());
            return AsyncHelpers.CompletedTask;
        }

        private NIType CreateMethodType()
        {
            System.Reflection.MethodBase method = typeof(CalculateTotalLengthCallback).GetMethod(nameof(CalculateTotalLengthCallback.CalculateTotalLength));
            NIType declaringType = DotNetNITypeHelper.CreateNITypeForDotNetType(typeof(CalculateTotalLengthCallback));
            NIType functionType = method.ConvertDotNetMethodInfoToNITypeDeprecated(declaringType);
            functionType = NIMethodCallTarget.AddDoesNotImplementMocCompilerAttribute(functionType);
            return functionType;
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
