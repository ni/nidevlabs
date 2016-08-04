using System.Xml.Linq;
using NationalInstruments.SourceModel;
using NationalInstruments.SourceModel.Persistence;

namespace ExamplePlugins.ExampleDiagram.SourceModel
{
    public class ExampleRootDiagram : RootDiagram
    {
        /// <summary>
        /// This is the specific type identifier for the definition
        /// </summary>
        public const string ElementName = "ExampleRootDiagram";

        /// <summary>
        /// Returns the persistence name of this node
        /// </summary>
        public override XName XmlElementName
        {
            get
            {
                return XName.Get(ElementName, ExamplePluginsNamespaceSchema.ParsableNamespaceName);
            }
        }

        [XmlParserFactoryMethod(ElementName, ExamplePluginsNamespaceSchema.ParsableNamespaceName)]
        public static ExampleRootDiagram Create(IElementCreateInfo elementCreateInfo)
        {
            var diagram = new ExampleRootDiagram();
            diagram.Init(elementCreateInfo);
            return diagram;
        }
    }

}
