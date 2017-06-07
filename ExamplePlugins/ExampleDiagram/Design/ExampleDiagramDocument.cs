using System;
using System.Collections.Generic;
using NationalInstruments;
using NationalInstruments.Shell;
using NationalInstruments.SourceModel.Envoys;
using ExamplePlugins.ExampleDiagram.SourceModel;

namespace ExamplePlugins.ExampleDiagram.Design
{
    /// <summary>
    /// Document type for our diagram editor.  The document type declares the document to the shell
    /// providing information about how to enable creation and editing.  It also factories instances
    /// of our document for editing.
    /// </summary>
    [ExportDocumentAndFileType(
        name: "Example Diagram Editor",
        modelDefinitionType: ExampleDiagramDefinition.ElementName,
        namespaceName: ExamplePluginsNamespaceSchema.ParsableNamespaceName,
        smallImage: "pack://application:,,,/ExamplePlugins.plugin;component/Resources/DocumentIconPaw16x16.png",
        largeImage: "pack://application:,,,/ExamplePlugins.plugin;component/Resources/DocumentIconPaw16x16.png",
        createNewSmallImage: "pack://application:,,,/ExamplePlugins.plugin;component/Resources/DocumentIconPaw16x16.png",
        createNewLargeImage: "pack://application:,,,/ExamplePlugins.plugin;component/Resources/DocumentIconPaw16x16.png",
        paletteImage: "pack://application:,,,/ExamplePlugins.plugin;component/Resources/DocumentIconPaw16x16.png",
        fileAssociationIcon: "Resources/TextEditor.ico",
        relativeImportance: 0.2,
        fileExtension: ".diagram",
        autoCreatesProject: true,
        defaultFileName: "Diagram Document")]
    public sealed partial class ExampleDiagramDocumentType : SourceFileDocumentType
    {
        /// <summary>
        /// Called to create a new instance of our document
        /// </summary>
        /// <returns></returns>
        public override Document CreateDocument(Envoy envoy)
        {
            return Host.CreateInstance<ExampleDiagramDocument>();
        }
    }

    /// <summary>
    /// This is the document for the diagram editor.  The document is the root view model for the editor.
    /// it is what owns and manages the editor.  It 
    /// </summary>
    public class ExampleDiagramDocument : SourceFileDocument
    {
        protected override IEnumerable<IDocumentEditControlInfo> CreateDefaultEditControls()
        {
            var diagramInfo = new DocumentEditControlInfo<ExampleDiagramEditControl>(
                    ExampleDiagramEditControl.UniqueId,
                    this,
                    DiagramDefinition.RootDiagram,
                    "Diagram",
                    ExampleDiagramEditControl.PaletteIdentifier,
                    "Resources/Diagram_32x32.png",
                    "Resources/Diagram_16x16.png")
            {
                ClipboardDataFormat = ExampleDiagramEditControl.ClipboardDataFormat
            };
            return diagramInfo.ToEnumerable();
        }

        public ExampleDiagramDefinition DiagramDefinition
        {
            get { return (ExampleDiagramDefinition)base.Definition; }
        }
    }
}
