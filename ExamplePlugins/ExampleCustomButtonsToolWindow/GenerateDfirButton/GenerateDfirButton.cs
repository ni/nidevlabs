using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using ExamplePlugins.Resources;
using NationalInstruments;
using NationalInstruments.Compiler;
using NationalInstruments.Composition;
using NationalInstruments.Core;
using NationalInstruments.Dfir;
using NationalInstruments.Linking;
using NationalInstruments.Shell;
using NationalInstruments.SourceModel;
using NationalInstruments.SourceModel.Envoys;
using MethodCall = NationalInstruments.Dfir.MethodCall;
using Node = NationalInstruments.Dfir.Node;
using Terminal = NationalInstruments.Dfir.Terminal;
using Wire = NationalInstruments.Dfir.Wire;

namespace ExamplePlugins.ExampleCustomButtonsToolWindow.GenerateDfirButton
{
    /// <summary>
    /// A custom button that generates DFIR and can read/write to it. This example reads the DFIR and writes the hierarchy to the console log output.
    /// </summary>
    [Export(typeof(CustomButton))]
    [PartMetadata(ExportIdentifier.ExportIdentifierKey, ProductLevel.Base)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GenerateDfirButton : CustomButton
    {
        public override void Initialize(ICompositionHost host)
        {
            host.VerifyArgumentIsNotNull(nameof(host));

            _host = host;
            Content = LocalizedStrings.GenerateDfirButtonName;
        }

        private const string DfirGenerationErrorMessageBoxText = "There was an internal error generating the DfirRoot";
        private const string WaitForCompileErrorMessageBoxText = "Please wait for compilation to finish before using this tool.";
        private ICompositionHost _host;

        /// <summary>
        /// The action to perform when the button is clicked.
        ///
        /// To avoid locking the UI when the button is clicked, do the button actions in a background <see cref="ScheduledActivity"/>.
        /// Call <see cref="AsyncHelpers.IgnoreAwait"/> on the returned <see cref="Task"/> to not wait on the activity to finish and return control
        /// to the user while the button code executes in the background.
        /// </summary>
        protected override void OnClick()
        {
            Document document = Document.GetActiveDocument(_host);
            var scheduledActivityManager = _host.GetSharedExportedValue<IScheduledActivityManager>();
            var activity = new BasicActivity<Task>(
                ActivityResourceAffinities.None,
                AsyncTaskPriority.WorkerHigh,
                "Generate DFIR",
                async () =>
                {
                    if (!CanDfirBeGeneratedForDocument(document))
                    {
                        return;
                    }
                    Tuple<DfirRoot, IDictionary<ExtendedQualifiedName, DfirRoot>> dfirRoots = await GenerateDfirForDocumentAndSubVIsAsync(_host, document.Envoy);
                    if (dfirRoots != null)
                    {
                        WriteHierarchyToConsoleLog(dfirRoots);
                    }
                });
            scheduledActivityManager.RunActivityAsync(activity).IgnoreAwait();
        }

        /// <summary>
        /// (Example) Additional code that can read and write to the DFIR. This example writes information about every node in the hierarchy to the console output
        /// </summary>
        private static void WriteHierarchyToConsoleLog(Tuple<DfirRoot, IDictionary<ExtendedQualifiedName, DfirRoot>> dfirRoots)
        {
            DfirRoot topDfir = dfirRoots.Item1;
            if (topDfir != null)
            {
                WriteBlockDiagramToConsoleLog(topDfir);
            }

            foreach (DfirRoot subVi in dfirRoots.Item2.Values)
            {
                if (subVi != null)
                {
                    Log.WriteLine();
                    Log.WriteLine($@"SubVI: {subVi.Name}");
                    WriteBlockDiagramToConsoleLog(subVi);
                }
            }
        }

        private static void WriteBlockDiagramToConsoleLog(DfirRoot dfirRoot)
        {
            foreach (Node node in dfirRoot.BlockDiagram.Nodes)
            {
                Log.WriteLine($@"Id {node.UniqueId} : {node.GetType().Name}");
            }
        }

        /// <summary>
        /// Generate DFIR for the document and for its subVIs.
        /// </summary>
        /// <param name="host">The composition host</param>
        /// <param name="documentEnvoy">The <see cref="Envoy"/> for the <see cref="Document"/> for which to generate DFIR</param>
        /// <returns>An awaitable task with the result DfirRoot(s)</returns>
        internal static async Task<Tuple<DfirRoot, IDictionary<ExtendedQualifiedName, DfirRoot>>> GenerateDfirForDocumentAndSubVIsAsync(ICompositionHost host, Envoy documentEnvoy)
        {
            var compileCancellationToken = new CompileCancellationToken();
            var progressToken = new ProgressToken();

            // Generate DFIR for the document
            DfirRoot sourceDfirRoot = await GenerateDfirAsync(host, documentEnvoy, progressToken, compileCancellationToken);
            if (sourceDfirRoot == null)
            {
                return null;
            }

            // Generate DFIR for subVIs
            IDictionary<ExtendedQualifiedName, DfirRoot> subDfirRoots = new Dictionary<ExtendedQualifiedName, DfirRoot>();
            await GetSubDfirRootsAsync(sourceDfirRoot, subDfirRoots, compileCancellationToken, progressToken, host);

            RemoveDebugPoints(sourceDfirRoot, compileCancellationToken);
            return new Tuple<DfirRoot, IDictionary<ExtendedQualifiedName, DfirRoot>>(sourceDfirRoot, subDfirRoots);
        }

        private static bool CanDfirBeGeneratedForDocument(Document document)
        {
            bool hasCompilerService = document?.Envoy?.QueryService<CompilerService>().FirstOrDefault() != null;
            bool hasTargetCompiler = document?.Envoy?.QueryInheritedService<ITargetCompilerServices>().FirstOrDefault()?.Compiler != null;
            return hasCompilerService && hasTargetCompiler;
        }

        private static async Task<DfirRoot> GenerateDfirAsync(
            ICompositionHost host,
            Envoy documentEnvoy,
            ProgressToken progressToken,
            CompileCancellationToken compileCancellationToken)
        {
            ExtendedQualifiedName topLevelDocumentName = documentEnvoy.CreateExtendedQualifiedName();
            TargetCompiler targetCompiler = documentEnvoy.QueryInheritedService<ITargetCompilerServices>().First().Compiler;
            AnyMocCompiler compiler = host.GetSharedExportedValue<AnyMocCompiler>();
            IReadOnlySymbolTable symbolTable = documentEnvoy.ComputeSymbolTable();

            // A specAndQName is used by the compiler to identify the document for which we're asking for DFIR.
            var specAndQName = new SpecAndQName(targetCompiler.CreateDefaultBuildSpec(topLevelDocumentName, symbolTable), topLevelDocumentName);

            try
            {
                DfirRoot sourceDfirRoot = await compiler.GetTargetDfirAsync(specAndQName, compileCancellationToken, progressToken);
                if (sourceDfirRoot == null)
                {
                    await ShowErrorMessageBoxAsync(host, WaitForCompileErrorMessageBoxText);
                }

                return sourceDfirRoot;
            }
            catch
            {
                await ShowErrorMessageBoxAsync(host, DfirGenerationErrorMessageBoxText);
                return null;
            }
        }

        /// <summary>
        /// Show an error message to the user.
        ///
        /// The message box needs to invoked on the UI thread. The button code was invoked in a background activity,
        /// so we queue a UIActivity that runs on the UI thread.
        /// </summary>
        /// <returns>Return the task so that calling code can wait for the user to dismiss the message box before continuing.</returns>
        private static Task ShowErrorMessageBoxAsync(ICompositionHost host, string messageBoxText)
        {
            var scheduledActivityManager = host.GetSharedExportedValue<IScheduledActivityManager>();
            return scheduledActivityManager.RunUIActivityAsync("Show an error message box", () =>
            {
                NIMessageBox.Show(messageBoxText, "DFIR Generation Error");
            });
        }

        /// <summary>
        /// Returns a dictionary that maps subVI names of method calls in the source <see cref="DfirRoot"/> to
        /// the corresponding <see cref="DfirRoot"/> objects.
        /// </summary>
        /// <param name="sourceDfirRoot">The parent document that does the method call</param>
        /// <param name="nameDictionary">The dictionary to fill out with names to dfir maps.</param>
        /// <param name="compileCancellationToken">The cancellation token</param>
        /// <param name="progressToken">The progress token</param>
        /// <param name="host">The composition host</param>
        /// <returns>The task to wait on</returns>
        private static async Task GetSubDfirRootsAsync(
            DfirRoot sourceDfirRoot,
            IDictionary<ExtendedQualifiedName, DfirRoot> nameDictionary,
            CompileCancellationToken compileCancellationToken,
            ProgressToken progressToken,
            ICompositionHost host)
        {
            if (sourceDfirRoot == null)
            {
                return;
            }

            // Maintain a queue of VIs to visit, and visit each one.
            var rootsToProcess = new Queue<DfirRoot>();
            // Add the top-level VI to the queue so it is visited first.
            rootsToProcess.Enqueue(sourceDfirRoot);
            while (rootsToProcess.Count > 0)
            {
                DfirRoot root = rootsToProcess.Dequeue();
                foreach (MethodCall node in root.BlockDiagram.GetAllNodes().OfType<MethodCall>())
                {
                    var specAndQName = new SpecAndQName(node.TargetBuildSpec, node.TargetName);
                    var compiler = host.GetSharedExportedValue<AnyMocCompiler>();
                    DfirRoot subDfirRoot;
                    try
                    {
                        subDfirRoot = await compiler.GetTargetDfirAsync(specAndQName, compileCancellationToken, progressToken);
                        RemoveDebugPoints(subDfirRoot, compileCancellationToken);
                    }
                    catch
                    {
                        await ShowErrorMessageBoxAsync(host, DfirGenerationErrorMessageBoxText);
                        return;
                    }
                    if (subDfirRoot == null)
                    {
                        continue;
                    }

                    if (nameDictionary.ContainsKey(node.TargetName))
                    {
                        // If the subVI has already been visited, then don't enqueue it to be visited again.
                        continue;
                    }

                    // The subVI has not been visited. Add the subVI to the queue of VIs to visit.
                    nameDictionary[node.TargetName] = subDfirRoot;
                    rootsToProcess.Enqueue(subDfirRoot);
                }
            }
        }

        /// <summary>
        /// An example of modifying the DFIR. Remove debug points, maybe because they are noise in the Dfir graph for our purposes.
        /// </summary>
        private static void RemoveDebugPoints(DfirRoot sourceDfirRoot, CompileCancellationToken compileCancellationToken)
        {
            if (sourceDfirRoot == null)
            {
                return;
            }

            if (compileCancellationToken.IsCancellationRequested)
            {
                CompileCanceledException.ThrowIt();
            }

            List<DebugPoint> debugPoints = sourceDfirRoot.GetAllNodesIncludingSelf().OfType<DebugPoint>().ToList();
            foreach (DebugPoint debugPoint in debugPoints)
            {
                foreach (Terminal terminal in debugPoint.Terminals)
                {
                    if (!terminal.IsConnected)
                    {
                        continue;
                    }

                    Terminal connectedTerminal = terminal.ConnectedTerminal;
                    var wire = connectedTerminal.ParentNode as Wire;
                    terminal.Disconnect();
                    wire?.RemoveOutput(connectedTerminal);
                }
                debugPoint.RemoveFromGraph();
            }
        }
    }
}
