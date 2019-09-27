using System;
using System.Windows;
using NationalInstruments.Core;

namespace NationalInstruments.WebServiceRunner
{
    /// <summary>
    /// WPF Application for the Web Service Sever application
    /// The application manages the UI thread for the process
    /// </summary>
    internal class ServerApplication : Application
    {
        private class ServerApplicationPreferencesInfo : IPreferencesApplicationInfo
        {
            public string Name => "Gll Web Service Server";

            public Version Version => new Version(1, 0, 0);
        }

        public ServerApplication()
        {
            PreferencesHelper.Initialize(new ServerApplicationPreferencesInfo());

            ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            PreferencesHelper.Instance.Save();
            base.OnExit(e);
        }
    }
}