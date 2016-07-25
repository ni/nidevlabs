using System.ComponentModel.Composition;
using NationalInstruments;
using NationalInstruments.Composition;
using NationalInstruments.Core;
using NationalInstruments.SourceModel;

namespace ExamplePlugins.ExampleDiagram.Design
{
    /// <summary>
    /// Palette loader which adds our palette to the example diagram
    /// </summary>
    [Export(typeof(IPaletteLoader))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [ExportMetadata(PaletteController.PaletteIdentifier, ExampleDiagramEditControl.PaletteIdentifier)]
    [PartMetadata(ExportIdentifier.ExportIdentifierKey, ProductLevel.Elemental)]
    public class ExampleDiagramPaletteLoader : ResourcePaletteLoader
    {
        /// <inheritdoc />
        protected override string ResourcePath
        {
            get
            {
                return "ExamplePlugins.Resources.ExampleDiagramNodes.xml";
            }
        }
    }
}
