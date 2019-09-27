using NationalInstruments.Core;

namespace NationalInstruments.WebServiceRunner
{
    /// <summary>
    /// Base class for a single Web Service VI
    /// </summary>
    internal class RegisteredExecutable : Disposable
    {
        public RegisteredExecutable(
            ConnectionTypeManager connectionManger,
            WebServiceServer server,
            string componentPath,
            string viName,
            string urlPath)
        {
            ComponentPath = componentPath;
            Name = viName;
            ConnectionManager = connectionManger;
            Server = server;
            UrlPath = urlPath;
        }

        public string ComponentPath { get; }

        public string Name { get; }

        public string UrlPath { get; }

        public ConnectionTypeManager ConnectionManager { get; }

        public WebServiceServer Server { get; }

        public GllManager GllManager => ConnectionManager.GLLManager;
    }
}