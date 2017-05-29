using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using NationalInstruments;
using NationalInstruments.Composition;
using NationalInstruments.Core;
using NationalInstruments.Shell;
using NationalInstruments.Design;
using NationalInstruments.Controls.Shell;
using NationalInstruments.SourceModel.Envoys;
using ExamplePlugins.ExampleDocument.Model;

namespace ExamplePlugins.ExampleDocument.Shell
{
    /// <summary>
    /// Document type for our text editor.  The document type declares the document to the shell
    /// providing information about how to enable creation and editing.  It also factories instances
    /// of our document for editing.
    /// </summary>
    [ExportDocumentAndFileType(
        name: "Example Text Editor",
        modelDefinitionType: TextDocumentDefinition.ElementName,
        namespaceName: ExamplePluginsNamespaceSchema.ParsableNamespaceName,
        smallImage: "Resources/Paper_16x16.png",
        largeImage: "Resources/Paper.png",
        createNewSmallImage: "Resources/Paper_16x16.png",
        createNewLargeImage: "Resources/Paper.png",
        paletteImage: "Resources/Paper_40x40.xml",
        fileAssociationIcon: "Resources/TextEditor.ico",
        relativeImportance: 0.2,
        fileExtension: ".textdoc",
        autoCreatesProject: true,
        defaultFileName: "Document")]
    public sealed partial class TextDocumentType : SourceFileDocumentType
    {
        /// <summary>
        /// Called to create a new instance of our document
        /// </summary>
        /// <returns></returns>
        public override Document CreateDocument(Envoy envoy)
        {
            return Host.CreateInstance<TextDocument>();
        }
    }

    /// <summary>
    /// Our "Document" for the text document.  The document class manages the user interaction for the document as a whole
    /// </summary>
    public class TextDocument : DefinitionDocument
    {
        public const string EditorId = "ExamplePlugins:TextDocument.Editor";

        /// <summary>
        /// Constructs a new instance
        /// </summary>
        public TextDocument()
        {
        }

        protected override IEnumerable<IDocumentEditControlInfo> CreateDefaultEditControls()
        {
            // Create our only editor, in this case a simple text editor
            return new DocumentEditControlInfo<TextDocumentEditorView>(EditorId, this, Envoy.ReferenceDefinition, 
                "Text Editor", string.Empty, string.Empty, string.Empty).ToEnumerable();
        }

        /// <summary>
        /// A command which provides contextual information for the edit group
        /// </summary>
        public static readonly ICommandEx EditingGroupCommand = new ShellRelayCommand(ShellDocumentCommandHelpers.HandleNoop)
        {
            UniqueId = "ExamplePlugins.EditingGroupCommand",
            LargeImageSource = ResourceHelpers.LoadBitmapImage(typeof(TextDocument), "Resources/Placeholder_32x32.png"),
            SmallImageSource = ResourceHelpers.LoadBitmapImage(typeof(TextDocument), "Resources/Placeholder_16x16.png"),
            LabelTitle = "Editing"
        };

        /// <summary>
        /// A command to manage Cut operations
        /// </summary>
        private ICommandEx CutCommand = new ShellRelayCommand(HandleCut, CanCut)
        {
            UniqueId = "ExamplePlugins.CutCommand",
            LargeImageSource = ResourceHelpers.LoadBitmapImage(typeof(TextDocument), "Resources/Cut_32x32.png"),
            SmallImageSource = ResourceHelpers.LoadBitmapImage(typeof(TextDocument), "Resources/Cut_16x16.png"),
            LabelTitle = "Cut"
        };

        /// <summary>
        /// Command update method for the cut command
        /// Command update methods are called periodically to update the state of a command in the editor
        /// These need to be fast because they are called often
        /// </summary>
        /// <param name="parameter">The instance command parameter</param>
        /// <param name="host">The composition host for the editor session</param>
        /// <param name="site">The document edit site managing the current edit session</param>
        /// <returns></returns>
        private static bool CanCut(ICommandParameter parameter, ICompositionHost host, DocumentEditSite site)
        {
            // Redirect the command to the standard WPF command which will be processed by the text box
            return ApplicationCommands.Cut.CanExecute(parameter, KeyboardHelpers.FocusedElement as IInputElement);
        }

        /// <summary>
        /// Command handler for the cut command
        /// </summary>
        /// <param name="parameter">The command parameter for the command instance</param>
        /// <param name="host">The composition host for the editor session</param>
        /// <param name="site">The document edit site managing the current edit session</param>
        private static void HandleCut(ICommandParameter parameter, ICompositionHost host, DocumentEditSite site)
        {
            // Redirect the command to the standard WPF command which will be processed by the text box
            ApplicationCommands.Cut.Execute(parameter, KeyboardHelpers.FocusedElement as IInputElement);
        }

        /// <summary>
        /// A command to manage Copy operations
        /// </summary>
        private ICommandEx CopyCommand = new ShellRelayCommand(HandleCopy, CanCopy)
        {
            UniqueId = "ExamplePlugins.CopyCommand",
            LargeImageSource = ResourceHelpers.LoadBitmapImage(typeof(TextDocument), "Resources/Copy_32x32.png"),
            SmallImageSource = ResourceHelpers.LoadBitmapImage(typeof(TextDocument), "Resources/Copy_16x16.png"),
            LabelTitle = "Copy"
        };

        /// <summary>
        /// Command update method for the copy command
        /// Command update methods are called periodically to update the state of a command in the editor
        /// These need to be fast because they are called often
        /// </summary>
        /// <param name="parameter">The instance command parameter</param>
        /// <param name="host">The composition host for the editor session</param>
        /// <param name="site">The document edit site managing the current edit session</param>
        /// <returns></returns>
        private static bool CanCopy(ICommandParameter parameter, ICompositionHost host, DocumentEditSite site)
        {
            // Redirect the command to the standard WPF command which will be processed by the text box
            return ApplicationCommands.Copy.CanExecute(parameter, KeyboardHelpers.FocusedElement as IInputElement);
        }

        /// <summary>
        /// Command handler for the copy command
        /// </summary>
        /// <param name="parameter">The command parameter for the command instance</param>
        /// <param name="host">The composition host for the editor session</param>
        /// <param name="site">The document edit site managing the current edit session</param>
        private static void HandleCopy(ICommandParameter parameter, ICompositionHost host, DocumentEditSite site)
        {
            // Redirect the command to the standard WPF command which will be processed by the text box
            ApplicationCommands.Copy.Execute(parameter, KeyboardHelpers.FocusedElement as IInputElement);
        }

        /// <summary>
        /// A command to manage Paste operations
        /// </summary>
        private ICommandEx PasteCommand = new ShellRelayCommand(HandlePaste, CanPaste)
        {
            UniqueId = "ExamplePlugins.PasteCommand",
            LargeImageSource = ResourceHelpers.LoadBitmapImage(typeof(TextDocument), "Resources/Paste_32x32.png"),
            SmallImageSource = ResourceHelpers.LoadBitmapImage(typeof(TextDocument), "Resources/Paste_16x16.png"),
            LabelTitle = "Paste"
        };

        /// <summary>
        /// Command update method for the paste command
        /// Command update methods are called periodically to update the state of a command in the editor
        /// These need to be fast because they are called often
        /// </summary>
        /// <param name="parameter">The instance command parameter</param>
        /// <param name="host">The composition host for the editor session</param>
        /// <param name="site">The document edit site managing the current edit session</param>
        /// <returns></returns>
        private static bool CanPaste(ICommandParameter parameter, ICompositionHost host, DocumentEditSite site)
        {
            // Redirect the command to the standard WPF command which will be processed by the text box
            return ApplicationCommands.Paste.CanExecute(parameter, KeyboardHelpers.FocusedElement as IInputElement);
        }

        /// <summary>
        /// Command handler for the paste command
        /// </summary>
        /// <param name="parameter">The command parameter for the command instance</param>
        /// <param name="host">The composition host for the editor session</param>
        /// <param name="site">The document edit site managing the current edit session</param>
        private static void HandlePaste(ICommandParameter parameter, ICompositionHost host, DocumentEditSite site)
        {
            // Redirect the command to the standard WPF command which will be processed by the text box
            ApplicationCommands.Paste.Execute(parameter, KeyboardHelpers.FocusedElement as IInputElement);
        }

        /// <summary>
        /// Determines if the standard editor toolbar should be shown.  In this case we do not need it.
        /// </summary>
        public override bool ShowToolBar
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Called to create the command content for this document.  This content will be displayed in the "Home"
        /// tab when this document is active
        /// </summary>
        /// <param name="context">The current presentation context</param>
        protected override void CreateCommandContentForDocument(ICommandPresentationContext context)
        {
            base.CreateCommandContentForDocument(context);

            // Add a group with cut, copy, and paste commands
            using (context.AddConfigurationPaneContent())
            {
                using (context.AddGroup(EditingGroupCommand))
                {
                    context.Add(PasteCommand);
                    context.Add(CutCommand);
                    context.Add(CopyCommand);
                }
            }
        }
    }
}
