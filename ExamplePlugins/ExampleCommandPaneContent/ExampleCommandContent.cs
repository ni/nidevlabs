using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NationalInstruments;
using NationalInstruments.Composition;
using NationalInstruments.Controls.Shell;
using NationalInstruments.Core;
using NationalInstruments.DataTypes;
using NationalInstruments.Design;
using NationalInstruments.MocCommon.SourceModel;
using NationalInstruments.ProjectExplorer.Design;
using NationalInstruments.Shell;
using NationalInstruments.SourceModel;
using NationalInstruments.VI.SourceModel;

namespace ExamplePlugins.ExampleCommandPaneContent
{
    /// <summary>
    /// This export is a push command content provider.  Push command providers have the opportunity to add
    /// content to the command bars and toolbars for just about anything.  In this case we are going to add content to the
    /// command bar of the random number primitive.
    /// </summary>
    [ExportPushCommandContent]
    public class ExampleCommandContent : PushCommandContent
    {
        /// <summary>
        /// This command is used to specify information about the command group that we will add commands to
        /// </summary>
        public static readonly ICommandEx ExampleItemsGroup = new ShellRelayCommand(ShellDocumentCommandHelpers.HandleNoop)
        {
            LabelTitle = "Example Buttons"
        };

        /// <summary>
        /// This is the definition of the command which will multiply the output of the random number by 10
        /// </summary>
        public static readonly ICommandEx MultipleCommand = new ShellSelectionRelayCommand(OnMultipleBy10, CanMultiplyBy10)
        {
            LabelTitle = "Multiply By 10",
            LargeImageSource = ResourceHelpers.LoadBitmapImage(typeof(ExampleCommandContent), "Resources/10x.png")
        };

        public static readonly ICommandEx OpenInNotepadCommand = new ShellRelayCommand(OnOpenInNotepad)
        {
            LabelTitle = "(Plug-in) Open In Notepad",
        };

        public static readonly ICommandEx OpenInNotepadMenuCommand = new ShellRelayCommand(OnOpenInNotepad)
        {
            LabelTitle = "(Plug-in) Open In Notepad",
            MenuParent = MenuPathCommands.EditMenu
        };

        public static readonly ICommandEx TerminalCommand = new ShellRelayCommand(OnShowTerminalType)
        {
            LabelTitle = "(Plug-in) Terminal Type",
        };

        public static bool CanMultiplyBy10(ICommandParameter parameter, IEnumerable<IViewModel> selection, ICompositionHost host, DocumentEditSite site)
        {
            return true;
        }

        /// <summary>
        /// This is the command handler that will add the code to multiple the output of the random number primitive by 10
        /// This is called when the user presses the Multiply By 10 button
        /// </summary>
        /// <param name="parameter">The command parameter associated with instance of the command</param>
        /// <param name="selection">The current selection</param>
        /// <param name="host">The Composition host for the session</param>
        /// <param name="site">The document edit site which is managing the edit session</param>
        public static void OnMultipleBy10(ICommandParameter parameter, IEnumerable<IViewModel> selection, ICompositionHost host, DocumentEditSite site)
        {
            // The selected item will be the random number node since we only add this command for random numbers
            var node = selection.First().Model as Node;
            if (node == null)
            {
                return;
            }

            // Start a transaction and add content to multiply the output by 10
            using (var transaction = node.TransactionManager.BeginTransaction("Multiple By 10", TransactionPurpose.User))
            {
                // Create a multiple node, position it nicely to the right ot the random number, and add it to the same diagram
                var multiply = Multiply.Create(ElementCreateInfo.ForNew);
                multiply.TopLeft = new SMPoint(node.Bounds.Right + StockDiagramGeometries.StandardNodeWidth, node.Top + (2 * StockDiagramGeometries.GridSize));
                node.Diagram.AddNode(multiply);
                
                // Wire the random number output to the first input on the multiple node
                node.Diagram.WireWithin((NodeTerminal)node.OutputTerminals.First(), (NodeTerminal)multiply.InputTerminals.First());

                // Create a Double Numeric Constant with an initial value of 10.0
                var literalBuilder = LiteralBuilder.GetLiteralBuilder(host);
                var context = new CreateLiteralContext(PFTypes.Double, 10.0);
                var literal = literalBuilder.CreateLiteral(context);

                // Position the constant nicely and add it to the diagram
                literal.TopLeft = new SMPoint(node.Left + StockDiagramGeometries.TinyNodeWidth, node.Bounds.Bottom + (2 * StockDiagramGeometries.GridSize));
                node.Diagram.AddNode(literal);

                // Wire the constant to the multiply node
                node.Diagram.WireWithin(literal.OutputTerminal, (NodeTerminal) multiply.InputTerminals.ElementAt(1));

                // Commit the transaction to finish the operation
                transaction.Commit();
            }
        }

        public static void OnOpenInNotepad(ICommandParameter parameter, ICompositionHost host, DocumentEditSite site)
        {
            var fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, MergeScriptBuilder.Create(new[] { site.EditControl.Document.Envoy.ReferenceDefinition }, host).ToString());
            Process.Start("Notepad.exe", fileName);
        }

        public static void OnShowTerminalType(ICommandParameter parameter, ICompositionHost host, DocumentEditSite site)
        {
            var viewModel = parameter.QueryService<NodeTerminalViewModel>().FirstOrDefault();
            if (viewModel != null)
            {
                NIMessageBox.Show("The terminal type is: " + viewModel.DataType.ToString());
            }
        }

        public override void CreateApplicationContent(ICommandPresentationContext context)
        {
            base.CreateApplicationContent(context);
            context.Add(OpenInNotepadMenuCommand);
        }

        /// <summary>
        /// This is called to add content for an element when it is singularly selected.
        /// </summary>
        /// <param name="context">The current presentation context</param>
        public override void CreateCommandContentForElement(ICommandPresentationContext context)
        {
            if (context.Selection != null && context.Selection.Any() && context.Selection.First().Model != null)
            {
                if (((Element)context.Selection.First().Model).SpecificKind == "NI.LabVIEW.VI:RandomNumber")
                {
                    using (context.AddConfigurationPaneContent())
                    {
                        using (context.AddGroup(ExampleItemsGroup))
                        {
                            context.Add(MultipleCommand, ButtonFactory.ForConfigurationPane);
                        }
                    }
                }
            }
            // Don't forget to call the base
            base.CreateCommandContentForElement(context);
        }

        public override void CreateContextMenuContent(ICommandPresentationContext context, PlatformVisual sourceVisual)
        {
            var projectItem = sourceVisual.DataContext as ProjectItemViewModel;
            if (projectItem != null && projectItem.Envoy != null)
            {
                try
                {
                    var loadedEnvoy = projectItem.Envoy.Project.GetLinkedEnvoys(projectItem.Envoy).Where(e => e.ReferenceDefinition != null).FirstOrDefault();
                    if (loadedEnvoy != null)
                    {
                        var viDocument = loadedEnvoy.ReferenceDefinition as VirtualInstrument;
                        if (loadedEnvoy.ReferenceDefinition != null)
                        {
                            context.Add(OpenInNotepadCommand);
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
            var terminal = sourceVisual.DataContext as NodeTerminalViewModel;
            if (terminal != null)
            {
                context.Add(new ShellCommandInstance(TerminalCommand, terminal));                
            }
            base.CreateContextMenuContent(context, sourceVisual);
        }
    }
}
