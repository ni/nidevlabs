namespace NationalInstruments.WebServiceRunner
{
    /// <summary>
    /// Common web server constants
    /// </summary>
    public static class WebServerConstants
    {
        /// <summary>
        /// TCP address for the local host 127.0.0.1
        /// </summary>
        public const string LocalHostTcpAddress = "127.0.0.1";

        /// <summary>
        /// IP address for the local host "localhost"
        /// </summary>
        public const string LocalHostIPAddress = "localhost";

        /// <summary>
        /// Default HTTP address for a local host
        /// </summary>
        public static readonly string LocalHostHttpIPAddress = $"http://{LocalHostIPAddress}";
    }
}