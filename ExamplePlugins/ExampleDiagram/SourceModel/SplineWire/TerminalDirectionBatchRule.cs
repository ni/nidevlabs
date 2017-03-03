using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NationalInstruments.SourceModel;

namespace ExamplePlugins.ExampleDiagram.SourceModel
{
    /// <summary>
    /// Batch rule which ensures that the terminal directions are set based on how the user creates wires
    /// This is assuming that the user wires from output to input
    /// </summary>
    public class TerminalDirectionBatchRule : BatchRule
    {
        /// <inheritdoc/>
        public override ModelBatchRuleExecuteLevels InitializeForTransaction(IRuleInitializeContext context)
        {
            // Indicated that this batch rule should run at each stage of the transaction and just just when
            // the operation completes. 
            return ModelBatchRuleExecuteLevels.Intermediate;
        }

        /// <inheritdoc/>
        protected override void ExecuteCore(TransactionItemCollection transactions, IRuleExecuteContext context)
        {
            // Look for the begin wiring tag which is set when a wiring operation is started from a terminal
            var startTag = context.Tags.GetFirstTag<StartWiringTerminalTransactionTag>();
            if (startTag != null)
            {
                // Make sure the source node terminal is an output terminal
                startTag.Terminal.ConnectedTerminal.Direction = Direction.Output;
            }
            // Look for the end wiring tag which is set when the wiring operation completes
            var endTag = context.Tags.GetFirstTag<EndWiringTransactionTag>();
            if (endTag != null)
            {
                // See if the wire was ended on a terminal
                var terminal = endTag.End as Terminal;
                if (terminal != null)
                {
                    // Make sure the terminal is set to be a input terminal
                    terminal.Direction = Direction.Input;
                }
            }
        }
    }
}
