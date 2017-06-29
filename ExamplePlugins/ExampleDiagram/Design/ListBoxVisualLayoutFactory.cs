using NationalInstruments.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.Core;
using ExamplePlugins.ExampleDiagram.Shell;
using NationalInstruments.Design;
using System.Windows.Data;
using NationalInstruments.Controls.Shell;

namespace ExamplePlugins.ExampleDiagram.Design
{
    public class ListBoxLayoutFactory : VisualLayoutFactory
    {
        private static readonly Lazy<ListBoxLayoutFactory> _forConfigurationPane = new Lazy<ListBoxLayoutFactory>();

        /// <summary>
        /// Gets the default facotry for controls in the configuration pane
        /// </summary>
        public static ICommandVisualLayoutFactory ForConfigurationPane => _forConfigurationPane.Value;

        public ListBoxLayoutFactory()
        {
            CreateLabel = false;
        }

        public override IVisualCollectionProvider GetVisualCollectionProvider(PlatformVisual visual)
        {
            var listBox = (ListBox)GetCommandSourceForVisual(visual);
            return new CollectionOrderVisualCollectionProvider(listBox.Items);
        }

        public override ICommandVisualFactory GetDefaultVisualFactory(ICommandEx command)
        {
            return ListBoxItemFactory.ForConfigurationPane;
        }

        protected override PlatformVisual CreateVisual()
        {
            // Arbitrarily setting max to 120 to encourage a visible vertical scrollbar.
            return new ListBox() { MaxHeight = 120 };
        }
    }

    public class ListBoxItemFactory : VisualFactory
    {
        private static readonly Lazy<ListBoxItemFactory> _forConfigurationPane = new Lazy<ListBoxItemFactory>();

        public static ICommandVisualFactory ForConfigurationPane => _forConfigurationPane.Value;

        public ListBoxItemFactory()
        {
            CreateLabel = false;
        }

        protected override PlatformVisual CreateVisual()
        {
            return new ListBoxItem();
        }

        public override void Attach(ICommandEx command, PlatformVisual visual, ICommandAttachContext context)
        {
            var listBoxItem = (ListBoxItem)GetCommandSourceForVisual(visual);
            listBoxItem.Command = command;
            listBoxItem.CommandParameter = new ChoiceCommandParameter(command);
            base.Attach(command, visual, context);
        }
    }
}
