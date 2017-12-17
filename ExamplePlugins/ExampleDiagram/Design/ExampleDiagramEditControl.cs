using System.Collections.Generic;
using NationalInstruments.Core;
using NationalInstruments.Design;
using NationalInstruments.Shell;

namespace ExamplePlugins.ExampleDiagram.Design
{
    public class ExampleDiagramEditControl : DocumentEditControl
    {
        /// <summary>
        /// UniqueId for document edit control.
        /// </summary>
        public const string UniqueId = "NI.VI.VIDiagramEditor";

        public const string PaletteIdentifier = "ExampleDiagramPalette";

        public static readonly string ClipboardDataFormat = ClipboardFormatHelper.RegisterClipboardFormat(DragDrop.NIDataFormatPrefix + PaletteIdentifier, "Example Diagram Editor");

        public ExampleDiagramEditControl()
        {
            DefaultStyleKey = typeof(ExampleDiagramEditControl);
        }
       

        protected override IDesignerToolViewModel CreateDefaultTool()
        {
            var tools = new List<IDesignerToolViewModel>();
            tools.Add(new NavigateToolViewModel { PanOnModifier = true });
            tools.Add(new PlacementViewModel());
            tools.Add(new WiringToolViewModel());
            tools.Add(new TextToolViewModel());
            tools.Add(new SelectionToolViewModel { AllowDelayedRules = SelectionToolDelayedRulesUse.Yes });
            return new AutoToolViewModel(tools);
        }

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        public override DesignerEditControl Designer
        {
            get
            {
                return (DesignerEditControl)GetTemplateChild("PART_designer");
            }
        }
    }
}
