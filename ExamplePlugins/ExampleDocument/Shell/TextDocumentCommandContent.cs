using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using NationalInstruments.Composition;
using NationalInstruments.Core;
using NationalInstruments.Controls.Shell;
using NationalInstruments.Shell;
using NationalInstruments.ProjectExplorer.Design;
using ExamplePlugins.ExampleDocument.Model;

namespace ExamplePlugins.ExampleDocument.Shell
{
    [Export(typeof (IPushCommandContent))]
    public class TextDocumentCommandContent : PushCommandContent
    {
        public static readonly ICommandEx OpenInNotepadCommand = new ShellRelayCommand(OnOpenInNotepad)
        {
            UniqueId = "ExamplePlugins.OpenInNotepadCommand",
            LabelTitle = "Open In Notepad",
        };

        public static void OnOpenInNotepad(ICommandParameter parameter, ICompositionHost host, DocumentEditSite site)
        {
            var text = (site.EditControl.Document.Envoy.ReferenceDefinition as TextDocumentDefinition).Text;
            var fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, text);
            Process.Start("Notepad.exe", fileName);
        }

        public override void CreateContextMenuContent(ICommandPresentationContext context, PlatformVisual sourceVisual)
        {
            var projectItem = sourceVisual.DataContext as ProjectItemViewModel;
            if (projectItem != null && projectItem.Envoy != null)
            {
                try
                {
                    if (projectItem.Envoy.ReferenceDefinition != null)
                    {
                        var textDocument = projectItem.Envoy.ReferenceDefinition as TextDocumentDefinition;
                        if (textDocument != null)
                        {
                            context.Add(OpenInNotepadCommand);
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
            base.CreateContextMenuContent(context, sourceVisual);
        }
    }
}
