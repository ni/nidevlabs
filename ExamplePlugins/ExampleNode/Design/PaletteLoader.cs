using System.ComponentModel.Composition;
using NationalInstruments;
using NationalInstruments.Composition;
using NationalInstruments.VI.Design;
using NationalInstruments.Core;
using NationalInstruments.SourceModel;

namespace ExamplePlugins.ExampleNode.Design
{
    /// <summary>
    /// Palette loader which adds our palette to the VI block diagram
    /// </summary>
    [Export(typeof(IPaletteLoader))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [ExportMetadata(PaletteController.PaletteIdentifier, VIDiagramControl.PaletteIdentifier)]
    [PartMetadata(ExportIdentifier.ExportIdentifierKey, ProductLevel.Elemental)]
    public class ExamplePluginsDiagramPaletteLoader : ResourcePaletteLoader
    {
        /// <inheritdoc />
        protected override string ResourcePath
        {
            get
            {
                return "ExamplePlugins.Resources.DiagramPalette.xml";
            }
        }
    }
}
