using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reflection;
using NationalInstruments;
using NationalInstruments.Composition;
using NationalInstruments.VI.Design;
using NationalInstruments.Core;
using NationalInstruments.SourceModel.Envoys;
using NationalInstruments.SourceModel;

namespace FanControl
{
    /// <summary>
    /// This class is required to insert your custom control into the front panel palette.
    /// </summary>
    [Export(typeof(IPaletteLoader))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [ExportMetadata(PaletteController.PaletteIdentifier, VIPanelControl.PaletteIdentifier)]
    [PartMetadata(ExportIdentifier.ExportIdentifierKey, ProductLevel.Elemental)]
    internal class DemoPaletteLoader : ResourcePaletteLoader
    {
        /// <inheritdoc />
        protected override string ResourcePath
        {
            get
            {
                return "FanDemo.Resources.DemoPanelPalette.xml";
            }
        }
    }
}
