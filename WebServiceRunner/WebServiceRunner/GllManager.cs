using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using NationalInstruments.Composition;
using NationalInstruments.Core;
using NationalInstruments.ExecutionFramework;
using NationalInstruments.NativeTarget.Runtime;

namespace NationalInstruments.WebServiceRunner
{
    /// <summary>
    /// Manages the set of GLLs that are providing web service VIs
    /// </summary>
    internal class GllManager : Disposable
    {
        private LocalNativeRuntimeExecutionTarget _gllExecutionTarget;

        private readonly ConcurrentDictionary<string, GLLInstance> _loadedGlls = new ConcurrentDictionary<string, GLLInstance>();
        private readonly PriorityCriticalSection _openGllCS = new PriorityCriticalSection();

        public GllManager(ICompositionHost host)
        {
            Host = host;
        }

        public ICompositionHost Host { get; }

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            foreach (var gll in _loadedGlls)
            {
                gll.Value.Dispose();
            }
            _gllExecutionTarget.Uninitialize();
        }

        public async Task InitializeAsync()
        {
            await ContinueOnBackground("Initialize");
            _gllExecutionTarget = LocalNativeRuntimeExecutionTarget.CreateStandaloneInstance(Host);
            // TODO: We need a custom native dialog helper to ensure that we do not show dialogs on the server
            NativeDialogSupport.SetNativeDialogHelper(new NativeDialogHelper(Host.Dispatcher));
        }

        public async Task<GLLInstance> OpenGLLAsync(string gllPath)
        {
            GLLInstance instance;
            if (_loadedGlls.TryGetValue(gllPath, out instance))
            {
                return instance;
            }
            using (await _openGllCS.WaitForTurnAsync())
            {
                await ContinueOnBackground("Open Gll");
                if (_loadedGlls.TryGetValue(gllPath, out instance))
                {
                    return instance;
                }
                var componentReference = await _gllExecutionTarget.LoadExecutableGllComponentByPathAsync(gllPath);
                instance = new GLLInstance(this, componentReference);
                _loadedGlls[gllPath] = instance;
                return instance;
            }
        }

        /// <summary>
        /// Causes the continuation following the usage of async/await to run on the activity manager.
        /// </summary>
        /// <param name="description">The description for the action to run.</param>
        /// <returns>A <see cref="ActivityAwaitable"/> that is awaitable.</returns>
        public ActivityAwaitable ContinueOnBackground(string description)
        {
            var activityManager = Host.GetSharedExportedValue<ScheduledActivityManager>();
            return ActivityHelpers.ContinueOnBackground(activityManager, AsyncTaskPriorityToken.WorkerNormal, description);
        }
    }

    /// <summary>
    /// Manages a single opened / loaded GLL
    /// </summary>
    internal class GLLInstance : Disposable
    {
        private readonly LocalGllComponentExecutable _component;
        private readonly GllManager _gllManager;
        private readonly Dictionary<string, OpenedHttpMethodInfo> _initializedMethods = new Dictionary<string, OpenedHttpMethodInfo>();
        private readonly PriorityCriticalSection _openCS = new PriorityCriticalSection();

        public GLLInstance(GllManager gllManager, LocalGllComponentExecutable component)
        {
            _gllManager = gllManager;
            _component = component;
        }

        public async Task<OpenedHttpMethodInfo> OpenMethodAsync(string executableUserName)
        {
            await _gllManager.ContinueOnBackground("Open Method");

            OpenedHttpMethodInfo methodInfo = null;
            using (await _openCS.WaitForTurnAsync())
            {
                if (!_initializedMethods.TryGetValue(executableUserName, out methodInfo))
                {
                    var openedVI = await _component.OpenVIAsync(executableUserName);
                    methodInfo = new OpenedHttpMethodInfo((ITopLevelPanelExecutable)openedVI);
                    _initializedMethods.Add(executableUserName, methodInfo);
                }
            }
            return methodInfo;
        }
    }

    /// <summary>
    /// Manages a single opened VI from a GLL
    /// </summary>
    internal class OpenedHttpMethodInfo
    {
        public OpenedHttpMethodInfo(ITopLevelPanelExecutable executable)
        {
            Executable = executable;
            executable.CurrentSimpleExecutionStateChanged += Executable_CurrentSimpleExecutionStateChanged;
            DataAvailabilityLock = executable.Dataspace.EnsureDataAvailability();
        }

        private readonly PriorityCriticalSection _lock = new PriorityCriticalSection();
        private TaskCompletionSource<bool> _executionComplete;

        public ITopLevelPanelExecutable Executable { get; }

        public IDisposable DataAvailabilityLock { get; }

        public Task<IPriorityCriticalSectionToken> WaitForTurnAsync() => _lock.WaitForTurnAsync();

        public async Task RunAsync()
        {
            _executionComplete = new TaskCompletionSource<bool>();
            await Executable.BeginRunAsync();
            await _executionComplete.Task;
            Executable.Dataspace.Update(PeriodicUpdateCondition.UpdateAlways);
        }

        private void Executable_CurrentSimpleExecutionStateChanged(object sender, CurrentStateChangedEventArgs<ISimpleExecutionState> e)
        {
            if (e.OldExecutionState.IsRunningTopLevel() && !e.NewExecutionState.IsRunningTopLevel())
            {
                _executionComplete.TrySetResult(true);
            }
        }
    }
}