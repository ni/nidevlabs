using System.Xml.Linq;
using NationalInstruments.DynamicProperties;
using NationalInstruments.SourceModel;
using NationalInstruments.SourceModel.Envoys;
using NationalInstruments.SourceModel.Persistence;

namespace ExamplePlugins.ExampleDocument.Model
{
    /// <summary>
    /// This is the model for the Text Document.  It is responsible for state management of all of the
    /// data of the text document.  It participates in save, load, undo and redo.  It must not have any user
    /// interface code in it.  The TextEditorDocument will bind to this model when the user edits the document.
    /// The document is responsible for managing user interactions.
    /// </summary>
    public class TextDocumentDefinition : Definition
    {
        /// <summary>
        /// This is the identifier used to tie this definition to the Document type which enables the
        /// editing of the definition.
        /// </summary>
        public static readonly EnvoyKeyword DefinitionType = new EnvoyKeyword(ElementName, ExamplePluginsNamespaceSchema.ParsableNamespaceName);

        // Define a PropertySymbol for all model properties that are settable
        // The symbol is used for generic discovery of all properties of model elements.  This is used for things
        // like search and most importantly Persistence
        public static readonly PropertySymbol TextSymbol = ExposeStaticProperty<TextDocumentDefinition>(
                "Text",
                obj => obj.Text,
                (obj, value) => obj.Text = (string)value,
                PropertySerializers.StringSerializer,
                string.Empty);

        /// <summary>
        /// This is the specific type identifier for the definition
        /// </summary>
        public const string ElementName = "TextDocumentDefinition";

        // The backing field for the text of the document
        private string _text;

        /// <summary>
        /// The constructor for the definition.  It is protected to avoid usage.
        /// New definitions should be created by calling the static Create method.
        /// </summary>
        protected TextDocumentDefinition()
        {
        }

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

        /// <summary>
        ///  String used to identify the document type in things such as SourceFileReference.
        /// </summary>
        /// <remarks>Overriding this is a good idea for child classes. By overriding and returning a constant,
        /// you will improve performance by avoiding the expensive reflection.</remarks>
        public override EnvoyKeyword ModelDefinitionType
        {
            get
            {
                return DefinitionType;
            }
        }

        /// <summary>
        /// Our create this.  This is used to create a new instance either programmatically, from load, and from the palette
        /// </summary>
        /// <param name="elementCreateInfo">creation information.  This tells us why we are being created (new, load, ...)</param>
        /// <returns>The newly created definition</returns>
        [XmlParserFactoryMethod(ElementName, ExamplePluginsNamespaceSchema.ParsableNamespaceName)]
        [ExportDefinitionFactory(ElementName, ExamplePluginsNamespaceSchema.ParsableNamespaceName)]
        public static TextDocumentDefinition Create(IElementCreateInfo elementCreateInfo)
        {
            var definition = new TextDocumentDefinition();
            definition.Init(elementCreateInfo);
            return definition;
        }

        /// <summary>
        /// Property to get and set the text of the document
        /// </summary>
        public string Text
        {
            get { return _text; }
            set
            {
                if (_text != value)
                {
                    // Here we "transact" the change which makes it undoable.  This will also mark the document as dirty.
                    var oldValue = _text;
                    _text = value;
                    TransactionRecruiter.EnlistPropertyItem(this, "Text", oldValue, _text, (v, _) => _text = v, TransactionHints.Semantic);
                }
            }
        }
    }
}
