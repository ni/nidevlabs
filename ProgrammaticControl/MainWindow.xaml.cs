using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NationalInstruments.Compiler;
using NationalInstruments.Composition;
using NationalInstruments.Core;
using NationalInstruments.ExecutionFramework;
using NationalInstruments.Shell;
using NationalInstruments.SourceModel;
using NationalInstruments.SourceModel.Envoys;
using NationalInstruments.VI.Design;
using NationalInstruments.VI.SourceModel;

namespace ProgramaticControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Project _project;
        private ILockedSourceFile _lockedVIReference;
        private VIDocument _openedDocument;

        public MainWindow()
        {
            InitializeComponent();

            Host = CompositionHost.InitializeNewHost();
            DocumentManager = Host.GetSharedExportedValue<IDocumentManager>();
        }

        private ICompositionHost Host { get; set; }

        private IDocumentManager DocumentManager { get; set; }

        private async void OnOpenProject(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_projectPath.Text))
            {
                MessageBox.Show("Please specify a project path");
                return;
            }
            await OpenProjectAsync(_projectPath.Text);
        }

        private async Task OpenProjectAsync(string projectPath)
        {
            try
            {
                _statusText.Text = "Loading Project...";
                _project = await DocumentManager.OpenProjectAsync(false, null, projectPath);
                if (_project != null)
                {
                    _statusText.Text = "Project is loaded!";
                }
                else
                {
                    _statusText.Text = "Failed to load project!";
                }
            }
            catch (Exception e)
            {
                _statusText.Text = "Failed To Load Project Error:\n" + e.ToString();
            }
        }

        private async void OnLoadVI(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_viName.Text))
            {
                MessageBox.Show("Please specify the name of the VI to run.");
                return;
            }
            await OpenVIAsync(_viName.Text);
        }

        private async Task OpenVIAsync(string viName)
        {
            var fileReference = GetReferencingFile(viName);
            if (fileReference != null)
            {
                _statusText.Text = "Loading VI...";
                using (var filelock = await fileReference.Envoy.LoadAsync())
                {
                    _openedDocument = DocumentManager.BindDocument(fileReference.Envoy) as VIDocument;
                    _statusText.Text = "VI Loaded!";
                }
            }
        }

        /// <summary>
        /// Returns <see cref="IReferencedFileService"/> of the VI with documentName.
        /// </summary>
        /// <param name="documentName">Document name</param>
        /// <returns><see cref="IReferencedFileService"/></returns>
        /// <remarks>Must be called in dispatcher thread.</remarks>
        private IReferencedFileService GetReferencingFile(string viName)
        {
            if (DocumentManager != null)
            {
                foreach (var envoy in _project.GetDescendantsBreadthFirst(null).OfType<Envoy>())
                {
                    if (envoy.IsReferencedFileEnvoy() && !envoy.IsUnderNullTarget() && envoy.Name.Equals(new QualifiedName(viName)))
                    {
                        return envoy.GetReferencedFileService();
                    }
                }

            }
            return null;
        }

        private async void OnRunVI(object sender, RoutedEventArgs e)
        {
            await RunVIAsync();
        }

        private async Task RunVIAsync()
        {
            var viExecutionService = _openedDocument.QueryExecutionService();

            _lockedVIReference = _openedDocument.Envoy.GetReferencedFileService().KeepReferencedFileInMemory();

            // Compile VI
            _statusText.Text = "Compiling...";
            var compilerService = _openedDocument.Envoy.QueryInheritedService<CompilerService>().First();
            while (!compilerService.GetIsSemanticallyValid())
            {
                await Task.Yield();
            }

            await viExecutionService.CompileAndDeployAsync();

            if (viExecutionService.HasSourceErrors(MessageSeverity.NotReady, null))
            {
                _statusText.Text = "VI Not Ready Error";
                return;
            }

            // This will make sure the last run values retain their values after VI executes
            using (viExecutionService.FunctionDataspace.EnsureDataAvailability())
            {
                await viExecutionService.RunAsync();
                VIDoneExecuting();
            }
        }

        private void VIDoneExecuting()
        {
            IDataspace dataContext = null;
            var executionService = _openedDocument.QueryExecutionService();
            if (executionService != null && executionService.MasterExecutableFunction != null)
            {
                dataContext = executionService.FunctionDataspace;
            }

            var vi = _openedDocument.Envoy.ReferenceDefinition as VirtualInstrument;
            var dataset = vi.GetCurrentDataSet();

            string parameterInfo = string.Empty;
            foreach (var dataItem in vi.DataItems)
            {
                var value = dataContext != null ? dataContext.GetPropertyValue(dataItem.CompiledName) : dataset.GetPropertyValue(dataItem.CompiledName);
                parameterInfo += string.Format("Name: {0}\nDataType: {1}\nCallUsage: {2}\nPreferredCallDirection: {3}\nValue: {4}\n\n",
                    dataItem.Name, dataItem.DataType, dataItem.CallUsage, dataItem.PreferredDirection, value);
            }

            _runResults.Text = parameterInfo;
            _lockedVIReference.Dispose();
        }
    }

    public static class ToAddToFramework
    {
        public static async Task CompileAndDeployAsync(this IExecutionService executionService)
        {
            var viExecutionService = executionService as NationalInstruments.MocCommon.Execution.ExecutionService;
            var cancellationSource = CompileCancellationToken.CreateNewSource();
            await viExecutionService.CompileAndDeployHierarchyAsync(cancellationSource.Token, AsyncTaskPriority.WorkerHighest);
        }

        public static Task RunAsync(this IExecutionService executionService)
        {
            TaskCompletionSource<bool> result = new TaskCompletionSource<bool>();
            // Run the VI
            EventHandler<CurrentStateChangedEventArgs<ISimpleExecutionState>> viStateChangedEventHandler = null;
            viStateChangedEventHandler = ((s, a) =>
            {
                if (a.NewExecutionState.IsIdle() && a.OldExecutionState.IsReserved())
                {
                    executionService.MasterExecutableFunction.CurrentSimpleExecutionStateChanged -= viStateChangedEventHandler;
                    result.SetResult(true);
                }
            });
            executionService.MasterExecutableFunction.CurrentSimpleExecutionStateChanged += viStateChangedEventHandler;
            executionService.StartRun();
            return result.Task;
        }
    }
}