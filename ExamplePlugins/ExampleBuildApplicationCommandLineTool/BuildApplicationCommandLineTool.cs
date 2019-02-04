using System.ComponentModel.Composition;
using ExamplePlugins.Resources;
using NationalInstruments.CommandLineInterface;
using NationalInstruments.ComponentEditor.SourceModel;

namespace ExamplePlugins.ExampleBuildApplicationCommandLineTool
{
    /// <summary>
    /// Command line tool that builds an application.
    /// </summary>
    [ExportCommandLineTool(
        typeToLocateResources: typeof(BuildApplicationCommandLineTool),
        commandName: CommandName,
        commandHelpKey: nameof(LocalizedStrings.BuildApplicationTool_Help),
        optionsHelpKeys: new string[]
        {
            ProjectPathArgumentPrototype,
            nameof(LocalizedStrings.BuildApplicationTool_HelpOptionDescription_ProjectPath),
            ComponentNameArgumentPrototype,
            nameof(LocalizedStrings.BuildApplicationTool_HelpOptionDescription_ComponentName),
            TargetNameArgumentPrototype,
            nameof(LocalizedStrings.BuildApplicationTool_HelpOptionDescription_TargetName),
            SaveArgumentPrototype,
            nameof(LocalizedStrings.BuildApplicationTool_HelpOptionDescription_Save)
        })]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class BuildApplicationCommandLineTool : BuildComponentCommandLineTool
    {
        /// <summary>
        /// The name of this command.
        /// </summary>
        public const string CommandName = "example-build-application";

        /// <summary>
        /// Constructs a new <see cref="BuildApplicationCommandLineTool"/>.
        /// </summary>
        public BuildApplicationCommandLineTool() : base(CommandName)
        {
            AddOption(
                ProjectPathArgumentPrototype,
                LocalizedStrings.BuildApplicationTool_HelpOptionDescription_ProjectPath,
                s => { ProjectPath = s; },
                required: true);
            AddOption(
                ComponentNameArgumentPrototype,
                LocalizedStrings.BuildApplicationTool_HelpOptionDescription_ComponentName,
                s => { ComponentName = s; },
                required: true);
            AddOption(TargetNameArgumentPrototype, LocalizedStrings.BuildApplicationTool_HelpOptionDescription_TargetName, s => { Target = s; });
            AddOption(SaveArgumentPrototype, LocalizedStrings.BuildApplicationTool_HelpOptionDescription_Save, s => { Save = true; });
        }

        /// <inheritdoc/>
        public override ComponentType ComponentType => ComponentType.Application;
    }
}
