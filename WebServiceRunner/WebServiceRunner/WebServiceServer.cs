using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Web;
using NationalInstruments.Core;

namespace NationalInstruments.WebServiceRunner
{
    /// <summary>
    /// A simple HTTP listener which listens for HTTP requests and forwards them to the appropriate
    /// registered handlers.
    /// </summary>
    public class WebServiceServer : Disposable
    {
        private const string PreferenceNamespaceName = "http://www.ni.com/WebServiceServer";

        /// <summary>
        /// User preference name for the UseStaticPort preference
        /// </summary>
        public const string UseStaticPortPreference = PreferenceNamespaceName + "/UseStaticWebServerPort";

        /// <summary>
        /// True to use the port defined by StaticPort. False to use a random open port.
        /// </summary>
        public static bool UseStaticPort
        {
            get { return PreferencesHelper.Instance.GetPreference(UseStaticPortPreference, true); }
            set { PreferencesHelper.Instance.SetPreference(UseStaticPortPreference, value); }
        }

        /// <summary>
        /// User preference name of the StaticPort preference
        /// </summary>
        public const string StaticPortPreference = PreferenceNamespaceName + "/StaticWebServerPort";

        /// <summary>
        /// Static port to use when UseStaticPort is true. If using the static port fails the application will fall-back to using a random port
        /// </summary>
        public static int StaticPort
        {
            get { return PreferencesHelper.Instance.GetPreference(StaticPortPreference, 8080); }
            set { PreferencesHelper.Instance.SetPreference(StaticPortPreference, value); }
        }

        /// <summary>
        /// User preference name of the LocalHostOnly preference
        /// </summary>
        public const string LocalHostOnlyPreference = PreferenceNamespaceName + "/LocalHostOnly";

        /// <summary>
        /// True to list to localhost only preferences
        /// </summary>
        public static bool LocalHostOnly
        {
            get { return PreferencesHelper.Instance.GetPreference(LocalHostOnlyPreference, true); }
            set { PreferencesHelper.Instance.SetPreference(LocalHostOnlyPreference, value); }
        }

        private readonly List<RequestHandler> _requestHandlers = new List<RequestHandler>();

        /// <summary>
        /// Gets the server.
        /// </summary>
        public HttpListener Server { get; internal set; }

        /// <summary>
        /// Gets the port that this web server is using
        /// </summary>
        public int Port { get; private set; }

        private bool IsInitialized { get; set; }

        /// <summary>
        /// Registers a URL path to forward to a different server with the given forwarding path
        /// </summary>
        /// <param name="path">The path to add</param>
        public void RegisterPath(RequestHandler path)
        {
            path.Initialize();
            lock (_requestHandlers)
            {
                _requestHandlers.Add(path);
            }
            StartServer();
        }

        /// <summary>
        /// Unregisters a forwarding path with this server
        /// </summary>
        /// <param name="toRemove">the forwarding path to unregister</param>
        public void UnregisterPath(RequestHandler toRemove)
        {
            if (toRemove == null)
            {
                return;
            }
            lock (_requestHandlers)
            {
                _requestHandlers.Remove(toRemove);
            }
            toRemove.Dispose();
        }

        /// <inheritdoc />
        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            StopServer();
            List<RequestHandler> items;
            lock (_requestHandlers)
            {
                items = _requestHandlers.ToList();
                _requestHandlers.Clear();
            }
            foreach (var item in items)
            {
                item.Dispose();
            }
        }

        private void StartServer()
        {
            if (IsInitialized)
            {
                return;
            }
            var port = StaticPort;
            var prefixAddress = LocalHostOnly ? WebServerConstants.LocalHostHttpIPAddress : "http://+";
            if (UseStaticPort)
            {
                try
                {
                    StartServer(port, prefixAddress);
                }
                catch (HttpListenerException)
                {
                }
            }
            if (Server == null)
            {
                // Fall-back to using a random port. UX will be displayed on affected documents
                // Try to get hold of an available port dynamically for Web Server to listen to.
                StartServerOnAvailablePort(prefixAddress);
            }
            if (Server == null && !LocalHostOnly)
            {
                // Fall-back to using localhost only. User may not have permissions to open a port
                // for any connection.
                prefixAddress = WebServerConstants.LocalHostHttpIPAddress;
                StartServerOnAvailablePort(prefixAddress);
            }
            if (Server != null)
            {
                Server.BeginGetContext((r) => DoAcceptHttpClientCallbackAsync(r).IgnoreAwait(), Server);
            }
        }

        private void StartServerOnAvailablePort(string prefixAddress)
        {
            int numberOfAttempts = 5;
            var address = IPAddress.Parse(WebServerConstants.LocalHostTcpAddress);
            while (numberOfAttempts > 0)
            {
                var port = GetAvailablePort(address);
                try
                {
                    StartServer(port, prefixAddress);
                    break;
                }
                catch (HttpListenerException)
                {
                    numberOfAttempts--;
                }
            }
        }

        private void StartServer(int port, string prefixAddress)
        {
            var httpServer = new HttpListener();
            httpServer.Prefixes.Add($"{prefixAddress}:{port}/");
            httpServer.Start();
            Server = httpServer;
            Port = port;
            IsInitialized = true;
        }

        private void StopServer()
        {
            if (IsInitialized)
            {
                IsInitialized = false;
                if (Server != null)
                {
                    Server.Close();
                    ((IDisposable)Server).Dispose();
                    Server = null;
                }
            }
        }

        /// <summary>
        /// Gets an available (unused) port on the local machine for the given address
        /// </summary>
        /// <param name="address">The address to get port for</param>
        /// <returns>A currently available port</returns>
        public static int GetAvailablePort(IPAddress address)
        {
            // Passing '0' to port dynamically chooses an available port.
            var tcpServer = new TcpListener(address, 0);
            tcpServer.Start();
            var port = ((IPEndPoint)tcpServer.LocalEndpoint).Port;
            tcpServer.Stop();
            return port;
        }

        /// <summary>
        /// The callback called when there is an incoming HTTP connection.
        /// This looks for the service requested and passes the parameters to the appropriate method for further processing.
        /// </summary>
        /// <param name="asyncResult">The async result.</param>
        public async Task DoAcceptHttpClientCallbackAsync(IAsyncResult asyncResult)
        {
            if (Server != null && Server.IsListening)
            {
                HttpListenerResponse response = null;
                try
                {
                    Server.BeginGetContext((r) => DoAcceptHttpClientCallbackAsync(r).IgnoreAwait(), Server);

                    var context = Server.EndGetContext(asyncResult);
                    var request = context.Request;
                    response = context.Response;
                    string trimmedUrl = HttpUtility.UrlDecode(request.RawUrl.TrimStart('/'));

                    RequestHandler foundItem = null;
                    lock (_requestHandlers)
                    {
                        foreach (var item in _requestHandlers)
                        {
                            if (item.Match(trimmedUrl))
                            {
                                foundItem = item;
                                break;
                            }
                        }
                    }
                    if (foundItem != null)
                    {
                        await foundItem.ProcessContentResponseAsync(trimmedUrl, request, response);
                    }
                    else
                    {
                        Log.WriteLine($"Request Not Found");
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                    }
                }
                catch (Exception e) when (ExceptionHelper.ShouldExceptionBeCaught(e))
                {
                    Log.WriteLine($"Server Error");
                    if (response != null)
                    {
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                }
                finally
                {
                    if (response != null)
                    {
                        try
                        {
                            response.Close();
                        }
                        catch (HttpListenerException)
                        {
                            // If the server closes the connection before the response is closed, this exception would be thrown.
                        }
                    }
                }
            }
        }
    }
}