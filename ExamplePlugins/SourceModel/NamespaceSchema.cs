using System.ComponentModel.Composition;
using System.Reflection;
using System.Xml.Linq;
using NationalInstruments.SourceModel.Persistence;

namespace ExamplePlugins
{
    /// <summary>
    /// Implements namespace versioning for elements in this assembly.
    /// </summary>
    [ParsableNamespaceSchema(ParsableNamespaceName, CurrentVersion)]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class ExamplePluginsNamespaceSchema : NamespaceSchema
    {
        /// <summary>
        /// Namespace name as an XNamespace
        /// </summary>
        public static readonly XNamespace XmlNamespace = XNamespace.Get(ParsableNamespaceName);

        /// <summary>
        /// Namespace name
        /// </summary>
        public const string ParsableNamespaceName = "http://www.ni.com/ExamplePlugins";

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ExamplePluginsNamespaceSchema()
            : base(Assembly.GetExecutingAssembly())
        {
        }

        /// <summary>
        /// The current version
        /// </summary>
        public const string CurrentVersion = "1.0.0";

        /// <inheritdoc/>
        public override string NamespaceName
        {
            get { return ParsableNamespaceName; }
        }

        /// <inheritdoc/>
        public override string FeatureSetName
        {
            get { return "Example Plugins"; }
        }
    }
}
