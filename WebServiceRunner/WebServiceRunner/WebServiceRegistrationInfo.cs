using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace NationalInstruments.WebServiceRunner
{
    /// <summary>
    /// Provides registration information for a Web Service GLL
    /// It is expected that this file is next to the GLL that is being registered and has the same
    /// Name without extension as the GLL.
    /// For example TestLibrary.gll and TestLibrary.config
    /// </summary>
    public class WebServiceRegistrationInfo
    {
        /// <summary>
        /// The file extension to use for persisted registration information
        /// </summary>
        public static readonly string FileExtension = ".config";

        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(WebServiceRegistrationInfo));

        /// <summary>
        /// List of all registered VIs for the GLL
        /// </summary>
        public List<VIRegistrationInfo> RegisteredVIs { get; set; }

        /// <summary>
        /// Writes this registration info to a file as XML
        /// </summary>
        /// <param name="filePath">Full file path to write to</param>
        public void WriteToFile(string filePath)
        {
            using (TextWriter writer = new StreamWriter(filePath))
            {
                _serializer.Serialize(writer, this);
                writer.Close();
            }
        }

        /// <summary>
        /// Loads registration info from a file
        /// </summary>
        /// <param name="filePath">full file path to load from</param>
        /// <returns>the loaded registration info</returns>
        public static WebServiceRegistrationInfo LoadFromFile(string filePath)
        {
            try
            {
                WebServiceRegistrationInfo result = null;
                using (TextReader reader = new StreamReader(filePath))
                {
                    result = (WebServiceRegistrationInfo)_serializer.Deserialize(reader);
                    reader.Close();
                }
                return result;
            }
            catch (InvalidOperationException)
            {
            }
            return new WebServiceRegistrationInfo();
        }
    }

    /// <summary>
    /// The type of server side service that is being provided
    /// </summary>
    public enum WebServiceType
    {
        /// <summary>
        /// No public service is provided
        /// </summary>
        None,

        /// <summary>
        /// Gll export is an HTTP Get method
        /// </summary>
        HttpGetMethod
    }

    /// <summary>
    /// Provides Registration information for a single Web Service VI
    /// </summary>
    public class VIRegistrationInfo
    {
        /// <summary>
        /// The component name of the VI
        /// TestNS::SimpleVI.gvi
        /// </summary>
        public string VIComponentName { get; set; }

        /// <summary>
        /// The URL to use for the VI
        /// "Test/MyHttpGet"
        /// </summary>
        public string UrlPath { get; set; }

        /// <summary>
        /// Gets and sets the type of server side method the VI is
        /// </summary>
        public WebServiceType Type { get; set; }
    }
}