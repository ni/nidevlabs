using System.Collections.Generic;
using System.Threading.Tasks;
using NationalInstruments.SourceModel;
using NationalInstruments.SourceModel.Envoys;
using NationalInstruments.VI.SourceModel;
using ExamplePlugins.ExampleDiagram.Design;

namespace ExamplePlugins.ExampleDiagram.Shell
{
    /// <summary>
    /// Envoy service factory for our VI merge script provider
    /// </summary>
    [ExportEnvoyServiceFactory(typeof(IProvideMergeScriptData))]
    [BindsToModelDefinitionType(VirtualInstrument.VIModelDefinitionType)]
    public class VIMergeScriptDataServiceFactory : EnvoyServiceFactory
    {
        /// <inheritdoc />
        protected override EnvoyService CreateService()
        {
            return new VIMergeScriptDataService();
        }
    }

    /// <summary>
    /// Provides a mergescript to use when dragging a VI onto an an example diagram
    /// </summary>
    public class VIMergeScriptDataService : EnvoyService, IProvideMergeScriptData
    {
        /// <inheritdoc/>
        public virtual IEnumerable<MergeScriptData> MergeScriptData
        {
            get
            {
                if (AssociatedEnvoy != null)
                {
                    string mergeText =
                        $@"<MergeScript xmlns = ""http://www.ni.com/PlatformFramework"">
                             <MergeData Key = ""SerializationReason"" Value = ""Copy""/>
                             <MergeData Key = ""ElementLabel"" Value = ""{AssociatedEnvoy.Name}"" />
                             <MergeData Key = ""ElementSearchTypeTag"" Value = ""SourceFile::{AssociatedEnvoy.Name}"" />
                             <MergeItem Path = ""."" IsPrimary = ""True"" >
                               <VIReference Target=""{AssociatedEnvoy.Name}"" xmlns = ""http://www.ni.com/ExamplePlugins"" />
                             </MergeItem>
                           </MergeScript>";
                    yield return new MergeScriptData(
                        mergeText,
                        ExampleDiagramEditControl.ClipboardDataFormat,
                        ExampleDiagramEditControl.PaletteIdentifier);
                }
            }
        }

        /// <inheritdoc/>
        public virtual Task<IEnumerable<MergeScriptData>> GetFilteredMergeScriptsAsync(IMergeScriptFilter filter)
        {
            return ProvideMergeScriptDataHelpers.GetFilteredMergeScriptsAsync(MergeScriptData, filter);
        }
    }
}
