using System.ComponentModel.Composition;
using System.Reflection;
using NationalInstruments.SourceModel.Persistence;

namespace FanPlugin.SourceModel
{
    /// <summary>
    /// Implements namespace versioning for elements in this assembly.
    /// </summary>
    [ParsableNamespaceSchema(ParsableNamespaceName, CurrentVersion)]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class DemoPanelNamespaceSchema : NamespaceSchema
    {
        // This is the custom part of the implementation. The namespace helps uniquely identify your
        // custom types for the parser.
        public const string ParsableNamespaceName = "http://www.ni-tech.com/FanDemo";

        public DemoPanelNamespaceSchema() : 
            base(Assembly.GetExecutingAssembly())
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

        public override string FeatureSetName
        {
            get { return "Demo Controls"; }
        }
    }
}
