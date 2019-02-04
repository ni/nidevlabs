using System.ComponentModel.Composition;
using ExamplePlugins.ExampleBuildApplicationCommandLineTool.Resources;
using NationalInstruments.CommandLineInterface;
using NationalInstruments.ComponentEditor.SourceModel;

namespace ExamplePlugins.ExampleBuildApplicationCommandLineTool
{
    /// <summary>
    /// Command line tool that builds an application.
    /// </summary>
    [ExportCommandLineTool(
        typeToLocateResources: typeof(BuildApplicationCommandLineTool),
        resourceName: nameof(ExampleBuildApplicationCommandLineTool_LocalizedStrings),
        commandName: CommandName,
        commandHelpKey: nameof(ExampleBuildApplicationCommandLineTool_LocalizedStrings.BuildApplicationTool_Help),
        optionsHelpKeys: new string[]
        {
            ProjectPathArgumentPrototype,
            nameof(ExampleBuildApplicationCommandLineTool_LocalizedStrings.BuildApplicationTool_HelpOptionDescription_ProjectPath),
            ComponentNameArgumentPrototype,
            nameof(ExampleBuildApplicationCommandLineTool_LocalizedStrings.BuildApplicationTool_HelpOptionDescription_ComponentName),
            TargetNameArgumentPrototype,
            nameof(ExampleBuildApplicationCommandLineTool_LocalizedStrings.BuildApplicationTool_HelpOptionDescription_TargetName),
            SaveArgumentPrototype,
            nameof(ExampleBuildApplicationCommandLineTool_LocalizedStrings.BuildApplicationTool_HelpOptionDescription_Save)
        })]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class BuildApplicationCommandLineTool : BuildComponentCommandLineTool
    {
        /// <summary>
        /// The name of this command.
        /// </summary>
        public const string CommandName = "build-application";

        /// <summary>
        /// Constructs a new <see cref="BuildApplicationCommandLineTool"/>.
        /// </summary>
        public BuildApplicationCommandLineTool() : base(CommandName)
        {
            AddOption(
                ProjectPathArgumentPrototype,
                ExampleBuildApplicationCommandLineTool_LocalizedStrings.BuildApplicationTool_HelpOptionDescription_ProjectPath,
                s => { ProjectPath = s; },
                required: true);
            AddOption(
                ComponentNameArgumentPrototype,
                ExampleBuildApplicationCommandLineTool_LocalizedStrings.BuildApplicationTool_HelpOptionDescription_ComponentName,
                s => { ComponentName = s; },
                required: true);
            AddOption(TargetNameArgumentPrototype, ExampleBuildApplicationCommandLineTool_LocalizedStrings.BuildApplicationTool_HelpOptionDescription_TargetName, s => { Target = s; });
            AddOption(SaveArgumentPrototype, ExampleBuildApplicationCommandLineTool_LocalizedStrings.BuildApplicationTool_HelpOptionDescription_Save, s => { Save = true; });
        }

        /// <inheritdoc/>
        public override ComponentType ComponentType => ComponentType.Application;
    }
}
