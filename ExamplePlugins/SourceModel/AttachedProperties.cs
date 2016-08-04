using System.Xml.Linq;
using NationalInstruments.DynamicProperties;
using NationalInstruments.SourceModel;
using NationalInstruments.SourceModel.Persistence;

namespace ExamplePlugins.SourceModel
{
    /// <summary>
    /// Attached properties allow you to add extra data to any modeling object.  These attached properties can be
    /// persisted with the model and modified as needed.
    /// Note: There is currently a limitation that in order to load a file all plug-ins which have added attached
    /// properties persisted into the file are required.  This limitation will be relaxed in an upcoming build.
    /// </summary>
    [ExposeAttachedProperties(typeof(ExampleAttachedProperties), ExamplePluginsNamespaceSchema.ParsableNamespaceName)]
    public class ExampleAttachedProperties : Element
    {
        /// <summary>
        /// The name of the attached property this type is providing
        /// </summary>
        public const string TagPropertyName = "ExampleTag";

        /// <summary>
        /// The actual attached property that is being defined
        /// </summary>
        public static readonly PropertySymbol TagPropertySymbol =
            ExposeEnlistedAttachedProperty(
                XName.Get(TagPropertyName, ExamplePluginsNamespaceSchema.ParsableNamespaceName), // The name of the property including the namespace
                PropertySerializers.StringSerializer, // The serializer to use when reading and writing the property
                string.Empty,                         // The default value of the property
                typeof(string));                      // The type of the properties value

        /// <summary>
        /// Gets the current value of the tag for the specified model element
        /// </summary>
        /// <param name="element">The model element to get the value of the tag for</param>
        /// <returns>The current value of the tag for the element</returns>
        public static string GetTag(Element element)
        {
            return (string)element.GetPropertyValue(TagPropertySymbol) ?? string.Empty;
        }

        /// <summary>
        /// Sets the tag on the specified element to the specified value
        /// </summary>
        /// <param name="element">The element to set the tag on</param>
        /// <param name="tagValue">The value to set the tag to</param>
        public static void SetTag(Element element, string tagValue)
        {
            element.SetPropertyValue(TagPropertySymbol, tagValue);
        }

        /// <summary>
        /// Removes the tag property from the specified element
        /// </summary>
        /// <param name="element">The element to remove the tag property from</param>
        public static void ClearTag(Element element)
        {
            element.ClearPropertyValue(TagPropertySymbol);
        }
    }
}
