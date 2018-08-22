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
using MethodCall = NationalInstruments.Dfir.MethodCall;
using Node = NationalInstruments.Dfir.Node;
using Wire = NationalInstruments.Dfir.Wire;

namespace ExamplePlugins.ExampleCustomButtonsToolWindow.GenerateDfirButton
{
    /// <summary>
    ///     A custom button that generates DFIR and can read/write to it. This example reads the DFIR and writes the hierarchy to the console output.
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
        /// To avoid locking up the UI when the button is clicked, do the button actions in a background <see cref="ScheduledActivity"/>.
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
                    Tuple<DfirRoot, IDictionary<ExtendedQualifiedName, DfirRoot>> dfirRoots = await GenerateDfirForDocumentAndSubVIsAsync(_host, document);
                    if (dfirRoots != null)
                    {
                        WriteHierarchyToConsoleOutput(dfirRoots);
                    }
                });
            scheduledActivityManager.RunActivityAsync(activity).IgnoreAwait();
        }

        /// <summary>
        /// (Example) Additional code that can read and write to the DFIR. This example writes information about every node in the hierarchy to the console output
        /// </summary>
        private static void WriteHierarchyToConsoleOutput(Tuple<DfirRoot, IDictionary<ExtendedQualifiedName, DfirRoot>> dfirRoots)
        {
            DfirRoot topDfir = dfirRoots.Item1;
            if (topDfir != null)
            {
                WriteBlockDiagramToConsoleOutput(topDfir);
            }

            foreach (DfirRoot subVi in dfirRoots.Item2.Values)
            {
                if (subVi != null)
                {
                    Log.WriteLine();
                    Log.WriteLine($@"SubVI: {subVi.Name}");
                    WriteBlockDiagramToConsoleOutput(subVi);
                }
            }
        }

        private static void WriteBlockDiagramToConsoleOutput(DfirRoot dfirRoot)
        {
            foreach (Node node in dfirRoot.BlockDiagram.Nodes)
            {
                Log.WriteLine($@"Id {node.UniqueId} : {node.GetType().Name}");
            }
        }

        /// <summary>
        ///     Generate DFIR for the document and for its subVIs.
        /// </summary>
        /// <param name="host">The composition host</param>
        /// <param name="document">The document for which to generate DFIR</param>
        /// <returns>An awaitable task with the result DfirRoot(s)</returns>
        internal static async Task<Tuple<DfirRoot, IDictionary<ExtendedQualifiedName, DfirRoot>>> GenerateDfirForDocumentAndSubVIsAsync(ICompositionHost host, Document document)
        {
            if (!CanDfirBeGeneratedForDocument(document))
            {
                return null;
            }

            var compileCancellationToken = new CompileCancellationToken();
            var progressToken = new ProgressToken();

            DfirRoot sourceDfirRoot = await GetTargetDfirAsync(host, document, progressToken, compileCancellationToken);
            if (sourceDfirRoot == null)
            {
                return null;
            }
            
            // Generate Dfir for subVIs
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

        private static async Task<DfirRoot> GetTargetDfirAsync(
            ICompositionHost host,
            Document document,
            ProgressToken progressToken,
            CompileCancellationToken compileCancellationToken)
        {
            ExtendedQualifiedName absoluteTargetName = document.Envoy.CreateExtendedQualifiedName();
            TargetCompiler targetCompiler = document.Envoy.QueryInheritedService<ITargetCompilerServices>().First().Compiler;
            var service = host.GetSharedExportedValue<AnyMocCompiler>();
            IReadOnlySymbolTable symbolTable = document.Envoy.ComputeSymbolTable();

            DfirRoot sourceDfirRoot;
            var specAndQName = new SpecAndQName(targetCompiler.CreateDefaultBuildSpec(absoluteTargetName, symbolTable), absoluteTargetName);
            try
            {
                sourceDfirRoot = await service.GetTargetDfirAsync(specAndQName, compileCancellationToken, progressToken);
            }
            catch
            {
                await ShowErrorMessageBoxAsync(host, DfirGenerationErrorMessageBoxText);
                return null;
            }

            if (sourceDfirRoot == null)
            {
                await ShowErrorMessageBoxAsync(host, WaitForCompileErrorMessageBoxText);
                return null;
            }

            return sourceDfirRoot;
        }

        private static Task ShowErrorMessageBoxAsync(ICompositionHost host, string messageBoxText)
        {
            // The message box needs to invoked on the UI thread. The button code was invoked in a background activity, so we queue a UIActivity that 
            // runs on the UI thread. Return the task so that the calling code can wait for the user to dismiss the message box before continuing.
            var scheduledActivityManager = host.GetSharedExportedValue<IScheduledActivityManager>();
            return scheduledActivityManager.RunUIActivityAsync("Show an error message box", () =>
            {
                NIMessageBox.Show(messageBoxText, "DFIR Generation Error");
            });
        }

        /// <summary>
        ///     Returns a dictionary that maps sub-function target names of method calls in the source dfirRoot to
        ///     dfir root objects.
        /// </summary>
        /// <param name="sourceDfirRoot">the parent dfir root that does the method call</param>
        /// <param name="nameDictionary">the dictionary to fill out with names to dfir maps.</param>
        /// <param name="compileCancellationToken">the cancellation token</param>
        /// <param name="progressToken">the progress token</param>
        /// <param name="host">the composition host</param>
        /// <returns>The task to wait on</returns>
        private static async Task GetSubDfirRootsAsync(DfirRoot sourceDfirRoot, 
            IDictionary<ExtendedQualifiedName, DfirRoot> nameDictionary,
            CompileCancellationToken compileCancellationToken, 
            ProgressToken progressToken, 
            ICompositionHost host)
        {
            if (sourceDfirRoot == null)
            {
                return;
            }
            var rootsToProcess = new Queue<DfirRoot>();
            rootsToProcess.Enqueue(sourceDfirRoot);
            while (rootsToProcess.Count > 0)
            {
                DfirRoot root = rootsToProcess.Dequeue();
                foreach (MethodCall node in root.BlockDiagram.GetAllNodes().OfType<MethodCall>())
                {
                    var specAndQName = new SpecAndQName(node.TargetBuildSpec, node.TargetName);
                    var service = host.GetSharedExportedValue<AnyMocCompiler>();
                    DfirRoot subDfirRoot;
                    try
                    {
                        subDfirRoot = await service.GetTargetDfirAsync(specAndQName, compileCancellationToken, progressToken);
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
                    if (!nameDictionary.ContainsKey(node.TargetName))
                    {
                        nameDictionary[node.TargetName] = subDfirRoot;
                        rootsToProcess.Enqueue(subDfirRoot);
                    }
                }
            }
        }

        /// <summary>
        /// As an example, remove debug points because they happen to be noise in the Dfir graph that we don't want.  
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

            var debugPoints = sourceDfirRoot.GetAllNodesIncludingSelf().OfType<DebugPoint>().ToList();
            foreach (var debugPoint in debugPoints)
            {
                foreach (var terminal in debugPoint.Terminals)
                {
                    if (terminal.IsConnected)
                    {
                        var connectedTerminal = terminal.ConnectedTerminal;
                        var wire = connectedTerminal.ParentNode as Wire;
                        terminal.Disconnect();
                        wire?.RemoveOutput(connectedTerminal);
                    }
                }
                debugPoint.RemoveFromGraph();
            }
        }
    }
}
