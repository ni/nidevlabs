using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExamplePlugins.Resources;
using NationalInstruments.CommandLineInterface;
using NationalInstruments.Compiler;
using NationalInstruments.ComponentEditor.SourceModel;
using NationalInstruments.Core;
using NationalInstruments.MocCommon.Components.BuildQueue.Model;
using NationalInstruments.SourceModel;

namespace ExamplePlugins.ExampleCommandLineInterfaceTool
{
    /// <summary>
    /// Monitor class to print build updates to the command line. Will create a build job for a <see cref="ComponentConfigurationReference"/>,
    /// add it to the job collection. Gives updates on child builds created, build errors, and provides a Task that will complete when the build finishes.
    /// </summary>
    internal class CommandLineBuildJobMonitor : Disposable
    {
        private readonly IJobCollection _jobCollection;
        private readonly IBuildQueueJob _rootJob;
        private readonly TaskCompletionSource<object> _jobFinishedCompletionSource;
        private PropertyChangedEventHandler _rootJobFinishedEventHandler;

        /// <inheritdoc />
        protected override void DisposeManagedResources()
        {
            _jobCollection.CollectionChanged -= JobCollectionOnCollectionChanged;
            if (_rootJob != null)
            {
                _rootJob.PropertyChanged -= _rootJobFinishedEventHandler;
            }
        }

        /// <summary>
        /// Constructs an instance of a <see cref="CommandLineBuildJobMonitor"/>. Creates a build job for a component build and
        /// begins monitoring the job collection for child jobs to be added.
        /// </summary>
        /// <param name="componentConfiguration">The <see cref="ComponentConfiguration"/> for the component build being monitored.</param>
        /// <param name="progressToken">Progress token to create the build job with.</param>
        /// <param name="cancellationTokenSource">Cancellation token source to create the build job with.</param>
        public CommandLineBuildJobMonitor(
            ComponentConfiguration componentConfiguration,
            ProgressToken progressToken,
            CancellationTokenSource<CompileCancellationToken> cancellationTokenSource)
        {
            _jobFinishedCompletionSource = new TaskCompletionSource<object>();
            _jobCollection = componentConfiguration.Host.GetSharedExportedValue<IJobCollection>();
            _jobCollection.CollectionChanged += JobCollectionOnCollectionChanged;
            _rootJob = CreateNewBuildJob(componentConfiguration, cancellationTokenSource, progressToken, _jobCollection);
            if (_rootJob == null)
            {
                throw new CommandLineOperationException(LocalizedStrings.BuildComponentTool_FailToStartBuildErrorMessage);
            }
            SubscribeToRootJobFinishedEvent();
        }

        /// <summary>
        /// Waits for the build to finish asynchronously and return whether the build succeeded
        /// Build is finished when the build state is set to Error, Failed, or Success. 
        /// </summary>
        /// <returns>True if the build succeeded (it is no longer running and the job state is not Failed or Error).</returns>
        public async Task<bool> WaitForBuildToFinishAsync()
        {
            await _jobFinishedCompletionSource.Task;
            return BuildSucceeded;
        }

        private bool BuildSucceeded => !IsJobIncomplete && _rootJob.State == JobState.Success;

        private bool IsJobIncomplete => _rootJob != null && (_rootJob.State == JobState.InProgress || _rootJob.State == JobState.NotStarted);

        private void SubscribeToRootJobFinishedEvent()
        {
            _rootJobFinishedEventHandler = (sender, args) => NotifyWhenRootBuildJobFinished(args);
            _rootJob.PropertyChanged += _rootJobFinishedEventHandler;
        }

        private void NotifyWhenRootBuildJobFinished(PropertyChangedEventArgs eventArgs)
        {
            if (!IsStatusChangedEvent(eventArgs))
            {
                return;
            }

            if (_rootJob.State == JobState.Error
                || _rootJob.State == JobState.Failed
                || _rootJob.State == JobState.Success
                || _rootJob.State == JobState.Canceled)
            {
                _jobFinishedCompletionSource.SetResult(null);
            }
        }

        private static IBuildQueueJob CreateNewBuildJob(
            ComponentConfiguration componentConfiguration,
            CancellationTokenSource<CompileCancellationToken> cancellationTokenSource,
            ProgressToken progressToken,
            IJobCollection jobCollection)
        {
            IBuildableComponentSubtype buildableComponentSubtype = (IBuildableComponentSubtype)componentConfiguration.ComponentSubtype;
            BuildId currentBuildId = buildableComponentSubtype.CreateBuildId(componentConfiguration);
            BuildQueueJobId buildQueueJobId = BuildQueueJobExtension.CreateBuildQueueJobId(currentBuildId);
            if (jobCollection.JobInProgress(buildQueueJobId))
            {
                throw new CommandLineOperationException("ID created for build already has an associated job that is in progress");
            }

            string outputFilePath = buildableComponentSubtype.GetOutputTopLevelFilePath(componentConfiguration);
            IBuildQueueJob job = jobCollection.CreateNewJob(
                buildQueueJobId,
                componentConfiguration.ComponentDefinition.ReferencingEnvoy,
                buildableComponentSubtype.XmlName,
                outputFilePath,
                cancellationTokenSource,
                progressToken);

            return job;
        }

        private static void JobCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e?.NewItems == null)
            {
                return;
            }

            foreach (object newItem in e.NewItems)
            {
                var job = (IBuildQueueJob)newItem;
                bool isChildJob = !BuildQueueJobId.IsRootJob(job.BuildQueueJobId);
                if (isChildJob)
                {
                    WriteBeginChildBuildMessage(job.AssociatedEnvoy.Name.Last);
                    job.PropertyChanged += OnChildBuildJobPropertyChanged;
                }
                else
                {
                    WriteBeginRootBuildMessage(job.AssociatedEnvoy.Name.Last);
                    job.PropertyChanged += OnRootBuildJobPropertyChanged;
                }
            }
        }

        private static void WriteBeginChildBuildMessage(string componentName)
        {
            CommandLineInterfaceApplication.WriteLine(
                string.Format(
                    CultureInfo.CurrentCulture,
                    "   " + LocalizedStrings.BuildComponentTool_ChildBuildStartMessage,
                    componentName,
                    CommandLineHelpers.GetFullDateTimeString()));
        }

        private static void WriteBeginRootBuildMessage(string componentName)
        {
            CommandLineInterfaceApplication.WriteLine(
                string.Format(
                    CultureInfo.CurrentCulture,
                    LocalizedStrings.BuildComponentTool_RootBuildStartMessage,
                    componentName));
        }

        private static void OnChildBuildJobPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            BuildJobPropertyChanged(sender, eventArgs, isChildBuild: true);
        }

        private static void OnRootBuildJobPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            BuildJobPropertyChanged(sender, eventArgs, isChildBuild: false);
        }

        private static void BuildJobPropertyChanged(object sender, PropertyChangedEventArgs eventArgs, bool isChildBuild)
        {
            string spacingString = isChildBuild ? "   " : string.Empty;
            var job = (IBuildQueueJob)sender;
            if (!IsStatusChangedEvent(eventArgs))
            {
                return;
            }

            bool buildFinished = false;
            if (job.State == JobState.Error || job.State == JobState.Failed)
            {
                WriteErrorsFromJob(job);
                buildFinished = true;
            }
            else if (job.State == JobState.Success)
            {
                if (isChildBuild)
                {
                    WriteChildBuildFinishedMessage(job.AssociatedEnvoy.Name.Last);
                }
                buildFinished = true;
            }

            if (buildFinished)
            {
                if (isChildBuild)
                {
                    job.PropertyChanged -= OnChildBuildJobPropertyChanged;
                }
                else
                {
                    job.PropertyChanged -= OnRootBuildJobPropertyChanged;
                }
                return;
            }

            // We've already printed an update for the first build step (starting the build), so we don't want to print it again here.
            if (job.CurrentStepIndex != 0)
            {
                CommandLineInterfaceApplication.WriteLine($"{spacingString}{job.AssociatedEnvoy.Name.Last} -- {job.CurrentActionDisplayName}. -- {CommandLineHelpers.GetFullDateTimeString()}");
            }
        }

        private static void WriteChildBuildFinishedMessage(string componentName)
        {
            string message = string.Format(
                CultureInfo.CurrentCulture,
                "   " + LocalizedStrings.BuildComponentTool_ChildBuildSuccess,
                componentName,
                CommandLineHelpers.GetFullDateTimeString());
            CommandLineInterfaceApplication.WriteLine(message);
        }

        private static bool IsStatusChangedEvent(PropertyChangedEventArgs eventArgs)
        {
            return eventArgs.PropertyName == BuildQueueJobExtension.StatusPropertyName;
        }

        private static void WriteErrorsFromJob(IBuildQueueJob job)
        {
            var componentEnvoy = job.AssociatedEnvoy;
            IEnumerable<MessageInfo> errors;
            if (ComponentExtensions.ComponentHasBuildOrCompileErrors(componentEnvoy, out errors))
            {
                CommandLineInterfaceApplication.WriteError(CreateComponentBuildErrorMessage(componentEnvoy.Name.Last, errors.ToList()));
            }
            else
            {
                CommandLineInterfaceApplication.WriteError(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        LocalizedStrings.BuildComponentTool_BuildFailedWithoutErrorMessages,
                        componentEnvoy.Name.Last));
            }
        }

        /// <summary>
        /// Create a user visible error message for the command line build from a list of <see cref="MessageInfo"/>
        /// </summary>
        /// <param name="componentName">The name of the component the errors came from.</param>
        /// <param name="errors">A list of <see cref="MessageInfo"/> containing the errors to print.</param>
        /// <returns>A localized error string.</returns>
        internal static string CreateComponentBuildErrorMessage(string componentName, IList<MessageInfo> errors)
        {
            var errorMessageBuilder = new StringBuilder();
            errorMessageBuilder.AppendLine(LocalizedStrings.BuildComponentTool_BuildErrorMessagesHeader);
            var indentedErrorMessageBuilder = new StringBuilder();
            indentedErrorMessageBuilder.Append(
                string.Join(
                    Environment.NewLine + Environment.NewLine,
                    errors.Select(BuildErrorInfo)));

            string indentedErrors = indentedErrorMessageBuilder.ToString();
            errorMessageBuilder.Append(indentedErrors);
            return errorMessageBuilder.ToString();
        }

        private static string BuildErrorInfo(MessageInfo error)
        {
            return string.Join(
                Environment.NewLine + '\t',
                string.Concat('\t', LocalizedStrings.BuildComponentTool_BuildErrorSource, error.Message.ReportingElementName),
                LocalizedStrings.BuildComponentTool_BuildErrorMessage,
                IndentBlock(error.Message.FormattedText));
        }

        private static string IndentBlock(string text)
        {
            return text.Replace("\n", "\n\t");
        }
    }
}