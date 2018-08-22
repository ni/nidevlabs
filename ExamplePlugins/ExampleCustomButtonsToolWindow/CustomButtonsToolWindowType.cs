using System;
using System.ComponentModel.Composition;
using NationalInstruments.Composition;
using NationalInstruments.Shell;

namespace ExamplePlugins.ExampleCustomButtonsToolWindow
{
    /// <summary>
    ///     Control type for the Custom Buttons window
    /// </summary>
    [Export(typeof(IToolWindowType))]
    [ExportMetadata("UniqueID", WindowGuidText)]
    [Name("(Example) Custom Button Window")]
    [ExportMetadata("SmallImagePath", "")]
    [ExportMetadata("LargeImagePath", "")]
    [ExportMetadata("DefaultCreationTime", ToolWindowCreationTime.UserRequested)]
    [ExportMetadata("DefaultCreationMode", ToolWindowCreationMode.Pinned)]
    [ExportMetadata("DefaultLocation", ToolWindowLocation.Bottom)]
    [ExportMetadata("AssociatedDocumentType", "")]
    [ExportMetadata("Weight", 0.5)]
    [ExportMetadata("ForceOpenPinned", true)]
    [ExportMetadata("VisibleInApplicationMenu", VisibilityOption.ToolLauncher)]
    internal class CustomButtonsToolWindowType : ToolWindowType
    {
        private const string WindowGuidText = "{29487F0A-2E72-4A1E-BFD6-5A42B37655E8}";

        /// <summary>
        ///     The Custom Buttons window GUID.
        /// </summary>
        /// <remarks>
        ///     DO NOT COPY THE GUID IF YOU ARE COPYING THIS CODE! CREATE A NEW GUID AND USE THAT INSTEAD.
        /// </remarks>
        internal static readonly Guid WindowGuid = new Guid(WindowGuidText);

        /// <inheritdoc />
        public override IToolWindowViewModel CreateToolWindow(ToolWindowEditSite editSite)
        {
            var viewModel = Host.CreateInstance<CustomButtonsToolWindowViewModel>();
            viewModel.EditSite = editSite;
            return viewModel;
        }
    }
}