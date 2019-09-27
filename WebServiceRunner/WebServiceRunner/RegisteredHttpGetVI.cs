using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.Core;
using NationalInstruments.DataValues;
using NationalInstruments.NativeTarget.Runtime;

namespace NationalInstruments.WebServiceRunner
{
    /// <summary>
    /// Represents a single HTTP Get VI that is registered with the server
    /// </summary>
    internal class RegisteredHttpGetVI : RegisteredExecutable
    {
        public RegisteredHttpGetVI(
            ConnectionTypeManager connectionManger,
            WebServiceServer server,
            string componentPath,
            string viName,
            string urlPath)
            : base(connectionManger, server, componentPath, viName, urlPath)
        {
            Server.RegisterPath(new HttpGetHandlingRequestPath(this));
        }
    }

    /// <summary>
    /// Request handler for a single HTTP Get Web Service VI
    /// </summary>
    internal class HttpGetHandlingRequestPath : RequestHandler
    {
        private RegisteredHttpGetVI _registerdVI;

        public HttpGetHandlingRequestPath(RegisteredHttpGetVI registerdVI)
            : base(registerdVI.UrlPath, RequestPathMatchingMode.MatchRequestStart)
        {
            _registerdVI = registerdVI;
        }

        protected internal override async Task ProcessContentResponseAsync(string requestString, HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                List<Tuple<string, string>> viParameters = new List<Tuple<string, string>>();
                var parameterStart = requestString.IndexOf('?');
                if (parameterStart != -1)
                {
                    var parameters = requestString.Substring(parameterStart + 1).Split('&');
                    foreach (var parameter in parameters)
                    {
                        var keyAndValue = parameter.Split('=');
                        if (keyAndValue?.Length == 2)
                        {
                            viParameters.Add(new Tuple<string, string>(keyAndValue[0], keyAndValue[1]));
                        }
                    }
                }
                var gll = await _registerdVI.GllManager.OpenGLLAsync(_registerdVI.ComponentPath);

                bool error = false;
                response.Headers.Add(HttpResponseHeader.CacheControl, "no-cache");
                var methodInfo = await gll.OpenMethodAsync(_registerdVI.Name);
                using (await methodInfo.WaitForTurnAsync())
                {
                    var dataspace = (LocalNativeDataspace)methodInfo.Executable.Dataspace;
                    var nameDicionary = dataspace.PropertyNames.ToDictionary(name => name.ToUpperInvariant(), name => name);
                    for (int i = 0; i < viParameters.Count; ++i)
                    {
                        string matchedCase;
                        if (nameDicionary.TryGetValue(viParameters[i].Item1.ToUpperInvariant(), out matchedCase))
                        {
                            viParameters[i] = new Tuple<string, string>(matchedCase, viParameters[i].Item2);
                        }
                        else
                        {
                            error = true;
                            response.StatusCode = (int)HttpStatusCode.BadRequest;
                            response.StatusDescription = $"Parameter {viParameters[i].Item1} is not valid";
                            break;
                        }
                        dataspace.SetPropertyValue(viParameters[i].Item1, viParameters[i].Item2);
                    }
                    if (!error)
                    {
                        await methodInfo.RunAsync();
                    }
                    if (!error)
                    {
                        if (dataspace.PropertyNames.Contains("ErrorOut"))
                        {
                            var errorOut = dataspace["ErrorOut"].Value as ICluster;
                            if (errorOut != null && (bool)errorOut[0] != true)
                            {
                                error = true;
                                response.StatusCode = (int)errorOut[1];
                                response.StatusDescription = (string)errorOut[2];
                            }
                        }
                    }
                    if (!error)
                    {
                        string responseName;
                        if (nameDicionary.TryGetValue("RESPONSE", out responseName))
                        {
                            var result = dataspace.GetPropertyValue(responseName);
                            var resultJson = JsonDataValueSerializer.SerializeValue(result).ToString();
                            var content = Encoding.UTF8.GetBytes(resultJson);

                            response.ContentType = "text/plain";
                            response.ContentLength64 = content.Length;
                            response.ContentEncoding = Encoding.UTF8;
                            response.StatusCode = (int)HttpStatusCode.OK;
                            using (var output = response.OutputStream)
                            {
                                output.Write(content, 0, content.Length);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.LogError(0, e, "HTTP Get Error");
                response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            }
        }
    }
}