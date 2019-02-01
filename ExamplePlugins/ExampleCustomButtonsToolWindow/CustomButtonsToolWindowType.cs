using System.ComponentModel.Composition;
using NationalInstruments.Composition;
using NationalInstruments.Shell;

namespace ExamplePlugins.ExampleCustomButtonsToolWindow
{
    /// <summary>
    /// Control type for the Custom Buttons window
    /// </summary>
    /// <remarks>
    /// The "Name" defines the title of the tool. The framework will use the "DisplayName" resource from a resx file with the same name.
    /// For example, this example tool will use the name defined in Resources/CustomButtonsToolWindow.resx.
    /// 
    /// The "Weight" defines where the tool appears in the list. Weights from LabVIEWFamilyApplicationFeatureSet.CreateApplicationContent
    ///     Output .1
    ///     Errors .2
    ///     Build Queue .21
    ///     Separator .25
    ///     C .3
    ///     Separator .35
    ///     Algorithm Estimates .4
    ///     Convert to fixed point .41
    ///     Sampling probes .42
    ///     Timing violations .43
    ///     Separator .5
    ///     Schedule .6
    /// </remarks>
    [Export(typeof(IToolWindowType))]
    [ExportMetadata("UniqueID", WindowGuidText)]
    [Name(typeof(CustomButtonsToolWindow))]
    [ExportMetadata("SmallImagePath", "")]
    [ExportMetadata("LargeImagePath", "")]
    [ExportMetadata("DefaultCreationTime", ToolWindowCreationTime.UserRequested)]
    [ExportMetadata("DefaultCreationMode", ToolWindowCreationMode.Pinned)]
    [ExportMetadata("DefaultLocation", ToolWindowLocation.Bottom)]
    [ExportMetadata("AssociatedDocumentType", "")]
    [ExportMetadata("Weight", 0.8)]
    [ExportMetadata("ForceOpenPinned", true)]
    [ExportMetadata("VisibleInApplicationMenu", VisibilityOption.ToolLauncher)]
    public class CustomButtonsToolWindowType : ToolWindowType
    {
        /// <summary>
        /// The Custom Buttons window GUID.
        /// </summary>
        /// <remarks>
        /// DO NOT COPY THE GUID IF YOU ARE COPYING THIS CODE! CREATE A NEW GUID AND USE THAT INSTEAD.
        /// </remarks>
        private const string WindowGuidText = "{29487F0A-2E72-4A1E-BFD6-5A42B37655E8}";

        /// <inheritdoc />
        public override IToolWindowViewModel CreateToolWindow(ToolWindowEditSite editSite)
        {
            // Use CreateInstance to create the CustomButtonsToolWindowViewModel so that the Host is imported correctly.
            var viewModel = Host.CreateInstance<CustomButtonsToolWindowViewModel>();
            viewModel.EditSite = editSite;
            return viewModel;
        }
    }
}