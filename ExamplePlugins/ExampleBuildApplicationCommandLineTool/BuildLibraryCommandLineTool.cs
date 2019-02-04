using System.ComponentModel.Composition;
using ExamplePlugins.Resources;
using NationalInstruments.ComponentEditor.SourceModel;
using NationalInstruments.CommandLineInterface;

namespace ExamplePlugins.ExampleBuildApplicationCommandLineTool
{
    /// <summary>
    /// Command line tool that builds a library.
    /// </summary>
    [ExportCommandLineTool(
        typeToLocateResources: typeof(BuildLibraryCommandLineTool),
        commandName: CommandName,
        commandHelpKey: nameof(LocalizedStrings.BuildLibraryTool_Help),
        optionsHelpKeys: new string[]
        {
            ProjectPathArgumentPrototype,
            nameof(LocalizedStrings.BuildLibraryTool_HelpOptionDescription_ProjectPath),
            ComponentNameArgumentPrototype,
            nameof(LocalizedStrings.BuildLibraryTool_HelpOptionDescription_ComponentName),
            TargetNameArgumentPrototype,
            nameof(LocalizedStrings.BuildLibraryTool_HelpOptionDescription_TargetName),
            SaveArgumentPrototype,
            nameof(LocalizedStrings.BuildLibraryTool_HelpOptionDescription_Save)
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
                LocalizedStrings.BuildLibraryTool_HelpOptionDescription_ProjectPath,
                s => { ProjectPath = s; },
                required: true);
            AddOption(
                ComponentNameArgumentPrototype,
                LocalizedStrings.BuildLibraryTool_HelpOptionDescription_ComponentName,
                s => { ComponentName = s; },
                required: true);
            AddOption(TargetNameArgumentPrototype, LocalizedStrings.BuildLibraryTool_HelpOptionDescription_TargetName, s => { Target = s; });
            AddOption(SaveArgumentPrototype, LocalizedStrings.BuildLibraryTool_HelpOptionDescription_Save, s => { Save = true; });
        }

        /// <inheritdoc/>
        public override ComponentType ComponentType => ComponentType.Library;
    }
}
