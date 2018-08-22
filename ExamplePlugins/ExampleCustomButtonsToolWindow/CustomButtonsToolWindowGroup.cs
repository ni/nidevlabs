﻿using System.ComponentModel.Composition;
using ExamplePlugins.Resources;
using NationalInstruments;
using NationalInstruments.Composition;
using NationalInstruments.Controls.Shell;
using NationalInstruments.Core;
using NationalInstruments.Shell;
using NationalInstruments.SourceModel.Persistence;

namespace ExamplePlugins.ExampleCustomButtonsToolWindow
{

    /// <summary>
    /// Tool launcher group for the window with cutom buttons 
    /// </summary>
    [ExportPushCommandContent]
    [PartMetadata(ExportIdentifier.ExportIdentifierKey, ProductLevel.Base)]
    [BindsToKeyword(DocumentEditControl.DocumentEditControlIdentifier)]
    [BindsToKeyword(ToolLauncherContentHelpers.ToolLauncherKeyword, PlatformFrameworkNamespaceSchema.ParsableNamespaceName)]
    public sealed class CustomButtonsToolWindowGroup : PushCommandContent
    {
        private static readonly string CustomButtonsGroupsCommandUniqueId = "NI.ExamplePlugins:CustomButtonsToolsGroupCommand".NotLocalized();
        
        private static readonly ICommandEx CustomButtonsToolWindowGroupCommand = new ShellRelayCommand()
        {
            LabelTitle = LocalizedStrings.CustomButtonsToolWindowGroupName,
            UniqueId = CustomButtonsGroupsCommandUniqueId
        };

        /// <inheritdoc />
        public override void CreateApplicationContent(ICommandPresentationContext context)
        {
            base.CreateApplicationContent(context);

            // Add to the tool launcher
            using (context.AddToolLauncherContent())
            {
                // Add a group
                using (context.AddGroup(CustomButtonsToolWindowGroupCommand))
                {
                    // Add the custom buttons tool window to the group
                    ICommandContentManager commandContentManager = ((CommandContentBuilder)context).Site.CommandContentManager;
                    context.Add(commandContentManager.GetCommandForWindow(CustomButtonsToolWindowType.WindowGuid));
                }
            }
        }
    }
}