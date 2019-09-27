using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using NationalInstruments.Composition;
using NationalInstruments.Core;
using NationalInstruments.Core.IO;

namespace NationalInstruments.WebServiceRunner
{
    /// <summary>
    /// Encapsulates the running a GLL Web Service server which runs deployed VIs in a built GLL
    /// </summary>
    public static class WebServiceRunner
    {
        private const string PreferenceNamespaceName = "http://www.ni.com/WebServiceRunner";

        /// <summary>
        /// User preference name for the UseStaticPort preference
        /// </summary>
        public const string GllLocationPreference = PreferenceNamespaceName + "/GllLocation";

        /// <summary>
        /// True to use the port defined by StaticPort. False to use a random open port.
        /// </summary>
        public static string GllLocation
        {
            get { return PreferencesHelper.Instance.GetPreference(GllLocationPreference, string.Empty); }
            set { PreferencesHelper.Instance.SetPreference(GllLocationPreference, value); }
        }

        private static GllManager _gllManager;
        private static ICompositionHost _host;
        private static WebServiceServer _httpServer;
        private static ConnectionTypeManager _connectionManager;
        private static List<ComponentLocation> _locations = new List<ComponentLocation>();
        private static SystemTrayManager _systemTrayManager = new SystemTrayManager();

        /// <summary>
        /// Set to true to restart the server application after shutdown of this process
        /// </summary>
        public static bool RestartServerOnClose { get; set; }

        /// <summary>
        /// Runs the server
        /// </summary>
        public static void RunServer()
        {
            var application = Application.Current;
            if (application == null)
            {
                application = new ServerApplication();
            }
            Log.Assert(0, application == Application.Current, "Failed to correctly create application");
            application.Dispatcher.BeginInvoke(new Action(() => ServeAsync().IgnoreAwait()));
            application.Run();

            _httpServer?.Dispose();
            _gllManager?.Dispose();
            _host?.Dispose();

            if (RestartServerOnClose)
            {
                var serverFileName = Assembly.GetEntryAssembly().Location;
                Process.Start(serverFileName);
            }
        }

        private static async Task ServeAsync()
        {
            Log.WriteLine("Starting Server");
            _locations.Add(new ComponentLocation(AssemblyExtensions.ApplicationDirectory, recursive: false, watchChanges: false));
            _host = CompositionHost.InitializeNewHost(null, _locations);

            _host.GetSharedExportedValue<ScheduledActivityManager>();

            _gllManager = new GllManager(_host);
            await _gllManager.InitializeAsync();

            // setup
            _connectionManager = new ConnectionTypeManager(_gllManager);
            _httpServer = new WebServiceServer();
            LoadConfigurations(GllLocation);
            _systemTrayManager.InstallTrayIcon();
            Log.WriteLine("Server Ready!");
        }

        private static void LoadConfigurations(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = LongPath.Combine(AssemblyExtensions.ApplicationDirectory, "WebServiceLibraries");
            }
            try
            {
                var allFiles = LongPathDirectory.EnumerateFiles(path, "*.config", System.IO.SearchOption.AllDirectories);
                foreach (var file in allFiles)
                {
                    try
                    {
                        var info = WebServiceRegistrationInfo.LoadFromFile(file);
                        var gllName = LongPath.ChangeExtension(file, ".gll");
                        if (info.RegisteredVIs != null)
                        {
                            foreach (var item in info.RegisteredVIs)
                            {
                                if (item.Type == WebServiceType.HttpGetMethod)
                                {
                                    var registeredExecutable = new RegisteredHttpGetVI(
                                        _connectionManager,
                                        _httpServer,
                                        gllName,
                                        item.VIComponentName,
                                        item.UrlPath);
                                    _connectionManager.AddConnectionType(registeredExecutable);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.LogError(0, e, $"Failed to load configuration: {file}");
                    }
                }
            }
            catch (Exception enumerateException)
            {
                Log.LogError(0, enumerateException, "Failed to enumerate configuration files");
            }
        }
    }
}