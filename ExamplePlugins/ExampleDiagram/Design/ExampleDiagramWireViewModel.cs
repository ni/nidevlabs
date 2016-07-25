using NationalInstruments.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.SourceModel;
using NationalInstruments.Core;

namespace ExamplePlugins.ExampleDiagram.Design
{
    public class ExampleDiagramWireViewModel : WireViewModel
    {
        public static readonly PlatformColor ErrorClusterTypeColor = PlatformColor.FromArgb(0xFF, 0xA0, 0xA0, 0x17);

        public static readonly PlatformColor ErrorClusterTypeSecondaryColor = PlatformColor.FromArgb(0xFF, 0xD0, 0xD0, 0x94);

        public static readonly ITypeAssetProvider ErrorClusterAssets = new TypeAssetProvider(
                    typeof(ExamplePluginsNamespaceSchema), "Resources/Wire",
                    ErrorClusterTypeColor,
                    ErrorClusterTypeSecondaryColor,
                    "Banana");

        public ExampleDiagramWireViewModel(Wire element) : 
            base(element)
        {
        }

        public override bool IsBroken
        {
            get
            {
                return false;
            }
        }

        public override WireRenderInfoEnumerable WireRenderInfo
        {
            get
            {
                return ErrorClusterAssets.GetWireRenderInfo(4);
            }
        }
    }
}
