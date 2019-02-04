using System.ComponentModel.Composition;
using ExamplePlugins.ExampleBuildApplicationCommandLineTool.Resources;
using NationalInstruments.ComponentEditor.SourceModel;
using NationalInstruments.CommandLineInterface;

namespace ExamplePlugins.ExampleBuildApplicationCommandLineTool
{
    /// <summary>
    /// Command line tool that builds a library.
    /// </summary>
    [ExportCommandLineTool(
        typeToLocateResources: typeof(BuildLibraryCommandLineTool),
        resourceName: nameof(ExampleBuildApplicationCommandLineTool_LocalizedStrings),
        commandName: CommandName,
        commandHelpKey: nameof(ExampleBuildApplicationCommandLineTool_LocalizedStrings.BuildLibraryTool_Help),
        optionsHelpKeys: new string[]
        {
            ProjectPathArgumentPrototype,
            nameof(ExampleBuildApplicationCommandLineTool_LocalizedStrings.BuildLibraryTool_HelpOptionDescription_ProjectPath),
            ComponentNameArgumentPrototype,
            nameof(ExampleBuildApplicationCommandLineTool_LocalizedStrings.BuildLibraryTool_HelpOptionDescription_ComponentName),
            TargetNameArgumentPrototype,
            nameof(ExampleBuildApplicationCommandLineTool_LocalizedStrings.BuildLibraryTool_HelpOptionDescription_TargetName),
            SaveArgumentPrototype,
            nameof(ExampleBuildApplicationCommandLineTool_LocalizedStrings.BuildLibraryTool_HelpOptionDescription_Save)
        })]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class BuildLibraryCommandLineTool : BuildComponentCommandLineTool
    {
        /// <summary>
        /// The name of this command.
        /// </summary>
        public const string CommandName = "example-build-library";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BuildLibraryCommandLineTool() : base(CommandName)
        {
            AddOption(
                ProjectPathArgumentPrototype,
                ExampleBuildApplicationCommandLineTool_LocalizedStrings.BuildLibraryTool_HelpOptionDescription_ProjectPath,
                s => { ProjectPath = s; },
                required: true);
            AddOption(
                ComponentNameArgumentPrototype,
                ExampleBuildApplicationCommandLineTool_LocalizedStrings.BuildLibraryTool_HelpOptionDescription_ComponentName,
                s => { ComponentName = s; },
                required: true);
            AddOption(TargetNameArgumentPrototype, ExampleBuildApplicationCommandLineTool_LocalizedStrings.BuildLibraryTool_HelpOptionDescription_TargetName, s => { Target = s; });
            AddOption(SaveArgumentPrototype, ExampleBuildApplicationCommandLineTool_LocalizedStrings.BuildLibraryTool_HelpOptionDescription_Save, s => { Save = true; });
        }

        /// <inheritdoc/>
        public override ComponentType ComponentType => ComponentType.Library;
    }
}
