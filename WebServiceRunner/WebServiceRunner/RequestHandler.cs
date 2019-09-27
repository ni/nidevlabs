using NationalInstruments.Core;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NationalInstruments.WebServiceRunner
{
    /// <summary>
    /// Request path matching modes to use when matching a request to a handler
    /// </summary>
    public enum RequestPathMatchingMode
    {
        /// <summary>
        /// The request string must match the request path completely
        /// </summary>
        MatchWholeRequest,

        /// <summary>
        /// The incoming request should start with the forwarding request path
        /// to be forwarded.
        /// </summary>
        MatchRequestStart
    }

    /// <summary>
    /// HTTP request handler which handles requests for a certain path segment
    /// </summary>
    public abstract class RequestHandler : Disposable
    {
        private readonly char[] _urlSegmentSeperators = { '/', '\\', '?' };

        /// <summary>
        /// Constructs a new instance of <see cref="RequestHandler"/>
        /// </summary>
        /// <param name="requestPath">The request path to forward</param>
        protected RequestHandler(string requestPath)
        {
            MatchingMode = RequestPathMatchingMode.MatchRequestStart;
            RequestPath = requestPath;
            RequestPathParts = requestPath.Split(_urlSegmentSeperators);
        }

        /// <summary>
        /// Constructs a new instance of <see cref="RequestHandler"/>
        /// </summary>
        /// <param name="requestPath">The request path to forward</param>
        /// <param name="matchingMode">The mode to use to match the request path</param>
        protected RequestHandler(string requestPath, RequestPathMatchingMode matchingMode)
        {
            MatchingMode = matchingMode;
            RequestPath = requestPath;
            RequestPathParts = requestPath.Split(_urlSegmentSeperators);
        }

        /// <summary>
        /// Called to initialize this instance once before being added to the list of forward request paths.
        /// </summary>
        public virtual void Initialize()
        {
        }

        /// <summary>
        /// The requesting path to use for matching
        /// </summary>
        public string RequestPath { get; }

        /// <summary>
        /// The split parts requesting path to use for matching
        /// </summary>
        private string[] RequestPathParts { get; }

        /// <summary>
        /// The matching mode to use
        /// </summary>
        public RequestPathMatchingMode MatchingMode { get; }

        /// <summary>
        /// Called to process the incoming request
        /// </summary>
        /// <param name="requestString">the remapped and trimmed request string</param>
        /// <param name="request">Current HTTP request</param>
        /// <param name="response">Receives the response</param>
        /// <returns>Task to await for the response to be completed</returns>
        protected internal abstract Task ProcessContentResponseAsync(string requestString, HttpListenerRequest request, HttpListenerResponse response);

        /// <summary>
        /// Called to see if this item matches the requesting path
        /// </summary>
        /// <param name="requestPath">the requesting path to check</param>
        /// <returns>true if the requesting path matches</returns>
        public bool Match(string requestPath)
        {
            switch (MatchingMode)
            {
                case RequestPathMatchingMode.MatchRequestStart:
                    {
                        var requestParts = requestPath.Split(_urlSegmentSeperators);
                        if (RequestPathParts.Length > requestParts.Length)
                        {
                            return false;
                        }
                        for (int x = 0; x < RequestPathParts.Length; ++x)
                        {
                            if (!RequestPathParts[x].Equals(requestParts[x], StringComparison.InvariantCultureIgnoreCase))
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                case RequestPathMatchingMode.MatchWholeRequest:
                    return requestPath.Equals(RequestPath, StringComparison.InvariantCultureIgnoreCase);
            }
            throw new InvalidOperationException("Unknown matching mode");
        }
    }
}