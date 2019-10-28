using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExamplePlugins.Resources;
using NationalInstruments;
using NationalInstruments.CommandLineInterface;
using NationalInstruments.Compiler;
using NationalInstruments.ComponentEditor.SourceModel;
using NationalInstruments.Core;
using NationalInstruments.Core.IO;
using NationalInstruments.Linking;
using NationalInstruments.MocCommon.Components.SourceModel;
using NationalInstruments.Shell;
using NationalInstruments.SourceModel;
using NationalInstruments.SourceModel.Envoys;

namespace ExamplePlugins.ExampleCommandLineInterfaceTool
{
    /// <summary>
    /// Generic tool for building a component
    /// </summary>
    public abstract class BuildComponentCommandLineTool : CommandLineTool
    {
        /// <summary>
        /// The prototype for the project argument.
        /// </summary>
        protected internal const string ProjectPathArgumentPrototype = "p|path=";

        /// <summary>
        /// The prototype for the component name argument.
        /// </summary>
        protected internal const string ComponentNameArgumentPrototype = "n|name=";

        /// <summary>
        /// The prototype for the target name argument.
        /// </summary>
        protected const string TargetNameArgumentPrototype = "t|target=";

        /// <summary>
        /// The prototype for the save argument.
        /// </summary>
        protected const string SaveArgumentPrototype = "s|save";

        /// <summary>
        /// The project to load.
        /// </summary>
        protected string ProjectPath { get; set; }

        /// <summary>
        /// The name of the component to build.
        /// </summary>
        protected string ComponentName { get; set; }

        /// <summary>
        /// The target that contains the component to build.
        /// </summary>
        protected string Target { get; set; }

        /// <summary>
        /// Whether to save the component after building.
        /// </summary>
        protected bool Save { get; set; }

        /// <summary>
        /// Constructs a new <see cref="BuildComponentCommandLineTool"/>.
        /// </summary>
        /// <param name="commandName">The name of the command</param>
        protected BuildComponentCommandLineTool(string commandName) : base(commandName)
        {
        }

        /// <summary>
        /// The type of component that is supported by the tool.
        /// </summary>
        public abstract ComponentType ComponentType { get; }

        /// <inheritdoc />
        protected override void ThrowIfOptionNotSupported()
        {
            ThrowIfProjectPathDoesNotExist();
            ThrowIfComponentNameDoesNotEndInProperExtension();
        }

        private void ThrowIfComponentNameDoesNotEndInProperExtension()
        {
            if (!ComponentName.EndsWith(ComponentDefinition.FileExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                string message = string.Format(
                    CultureInfo.CurrentCulture,
                    LocalizedStrings.BuildComponentTool_ComponentNameHasWrongExtension,
                    ComponentName,
                    ComponentDefinition.FileExtension);
                throw new CommandLineOperationException(message, showToolHelp: true);
            }
        }

        private void ThrowIfProjectPathDoesNotExist()
        {
            if (!LongPathFile.Exists(ProjectPath))
            {
                string message = string.Format(
                    CultureInfo.CurrentCulture,
                    LocalizedStrings.BuildComponentTool_InvalidProjectPath,
                    ProjectPath);
                throw new CommandLineOperationException(message, showToolHelp: true);
            }
        }

        /// <inheritdoc />
        protected override async Task<int> RunAsync(IEnumerable<string> extraArguments, ProjectAndHostCreator projectAndHostCreator)
        {
            if (!LongPath.IsPathRooted(ProjectPath))
            {
                ProjectPath = LongPath.GetFullPath(LongPath.Combine(Environment.CurrentDirectory, ProjectPath));
            }

            Project project = await projectAndHostCreator.OpenProjectAsync(ProjectPath);
            CommandLineInterfaceApplication.WriteLineVerbose($"Opened project at {ProjectPath}");

            Envoy componentEnvoy = await ResolveComponentToBuildAsync(project);
            if (componentEnvoy == null)
            {
                return 1;
            }

            bool buildSucceeded = await LoadAndBuildComponentEnvoyAsync(componentEnvoy);
            return buildSucceeded ? 0 : 1;
        }

        private async Task<Envoy> ResolveComponentToBuildAsync(Project project)
        {
            ITargetScopeService targetScopeService;
            if (string.IsNullOrEmpty(Target))
            {
                targetScopeService = GetSingleTargetInProject(project);
                if (targetScopeService == null)
                {
                    string errorMessage = await CreateTargetNotProvidedErrorMessageAsync(project);
                    CommandLineInterfaceApplication.WriteError(errorMessage);
                    return null;
                }
            }
            else
            {
                targetScopeService = GetTargetScopeServiceByName(project, Target);
                if (targetScopeService == null)
                {
                    string errorMessage = await CreateResolveTargetErrorMessageAsync(project, Target);
                    CommandLineInterfaceApplication.WriteError(errorMessage);
                    return null;
                }
            }

            return await ResolveComponentOnTargetAsync(ComponentName, targetScopeService);
        }

        private static ITargetScopeService GetSingleTargetInProject(Project project)
        {
            ITargetScopeService targetScopeService;
            project.GetTargets(GetTargetsOptions.ExcludeNonUserManaged).TryGetSingleElement(out targetScopeService);
            return targetScopeService;
        }

        private async Task<string> CreateTargetNotProvidedErrorMessageAsync(Project project)
        {
            var errorMessageBuilder = new StringBuilder(LocalizedStrings.BuildComponentTool_TargetNotProvided);
            await AppendAvailableTargetsForProvidedComponentNameErrorMessageAsync(project, errorMessageBuilder);
            return errorMessageBuilder.ToString();
        }

        private async Task<bool> AppendAvailableTargetsForProvidedComponentNameErrorMessageAsync(Project project, StringBuilder errorMessageBuilder)
        {
            bool foundTarget = false;
            string validTargetNamesMessage = await GetTargetNamesForProvidedComponentNameAsync(project);
            if (!string.IsNullOrEmpty(validTargetNamesMessage))
            {
                string validTargetNamesHeader = string.Format(CultureInfo.CurrentCulture,
                    LocalizedStrings.BuildComponentTool_ValidTargetNamesHeader,
                    ComponentName);
                errorMessageBuilder.AppendLine()
                    .AppendLine(validTargetNamesHeader)
                    .AppendLine(validTargetNamesMessage);
                foundTarget = true;
            }

            return foundTarget;
        }

        private async Task<string> GetTargetNamesForProvidedComponentNameAsync(Project project)
        {
            IEnumerable<ITargetScopeService> targets = project.GetTargets(GetTargetsOptions.ExcludeNonUserManaged);
            var targetNames = new List<string>();
            foreach (var target in targets)
            {
                ITargetScopeService targetbuildBuildSpecScope = target.GetDefaultBuildSpecScope();
                IEnumerable<Envoy> matchingComponents =
                    (await targetbuildBuildSpecScope.TargetScope.ResolveAsync(new QualifiedName(ComponentName)))
                    .Where(envoy => envoy.IsComponentDefinitionEnvoy());

                if (matchingComponents.HasExactly(1))
                {
                    targetNames.Add('\t' + target.GetScopeDisplayName());
                }
                else if (matchingComponents.HasMoreThan(1))
                {
                    throw new CommandLineOperationException("Multiple components matching the provided name were found. This likely indicates a corrupt project file.");
                }
            }
            return string.Join(Environment.NewLine, targetNames);
        }

        private static ITargetScopeService GetTargetScopeServiceByName(Project project, string targetName)
        {
            IEnumerable<ITargetScopeService> userManagedTargets = project.GetTargets(GetTargetsOptions.ExcludeNonUserManaged);
            IEnumerable<ITargetScopeService> matchingTargets = userManagedTargets
                .Where(scope => string.Equals(scope.GetScopeDisplayName(), targetName, StringComparison.InvariantCultureIgnoreCase));
            if (matchingTargets.HasMoreThan(1))
            {
                throw new CommandLineOperationException("Multiple targets matching the provided name were found. This likely indicates a corrupt project file.");
            }

            return matchingTargets.SingleOrDefault();
        }

        private async Task<string> CreateResolveTargetErrorMessageAsync(Project project, string targetName)
        {
            var errorMessageBuilder = new StringBuilder(
                string.Format(
                    CultureInfo.CurrentCulture,
                    LocalizedStrings.BuildComponentTool_InvalidTargetName,
                    targetName));
            await AppendAvailableTargetsForProvidedComponentNameErrorMessageAsync(project, errorMessageBuilder);
            return errorMessageBuilder.ToString();
        }

        private async Task<Envoy> ResolveComponentOnTargetAsync(string componentName, ITargetScopeService targetScope)
        {
            ITargetScopeService targetbuildBuildSpecScope = targetScope.GetDefaultBuildSpecScope();
            IEnumerable<Envoy> matchingComponents =
                (await targetbuildBuildSpecScope.TargetScope.ResolveAsync(new QualifiedName(componentName)))
                .Where(envoy => envoy.IsComponentDefinitionEnvoy());

            if (matchingComponents.HasMoreThan(1))
            {
                throw new CommandLineOperationException("Multiple components matching the provided name were found. This likely indicates a corrupt project file.");
            }

            Envoy componentEnvoy = matchingComponents.SingleOrDefault();
            if (componentEnvoy == null)
            {
                CommandLineInterfaceApplication.WriteError(await CreateResolveComponentErrorMessageAsync(componentName, targetScope.GetScopeDisplayName(), targetScope));
            }

            return componentEnvoy;
        }

        private async Task<string> CreateResolveComponentErrorMessageAsync(string componentName, string targetName, ITargetScopeService targetScopeService)
        {
            var errorMessageBuilder = new StringBuilder(
                string.Format(
                    CultureInfo.CurrentCulture,
                    LocalizedStrings.BuildComponentTool_InvalidComponentName,
                    componentName,
                    targetName));

            if (!await AppendAvailableTargetsForProvidedComponentNameErrorMessageAsync(targetScopeService.TargetScope.GetProject(), errorMessageBuilder))
            {
                await AppendAvailableComponentsForProvidedTargetNameErrorMessageAsync(errorMessageBuilder, targetScopeService);
            }
            return errorMessageBuilder.ToString();
        }

        private async Task AppendAvailableComponentsForProvidedTargetNameErrorMessageAsync(StringBuilder errorMessageBuilder, ITargetScopeService targetScopeService)
        {
            string validComponentNamesHeader = string.Format(CultureInfo.CurrentCulture,
                LocalizedStrings.BuildComponentTool_ValidComponentNamesHeader,
                ComponentType.GetComponentDisplayName());
            string validComponentNamesMessage = await GetComponentNamesMessageAsync(targetScopeService);
            if (!string.IsNullOrEmpty(validComponentNamesMessage))
            {
                errorMessageBuilder.AppendLine()
                    .AppendLine(validComponentNamesHeader)
                    .AppendLine(validComponentNamesMessage);
            }
        }

        private async Task<string> GetComponentNamesMessageAsync(ITargetScopeService target)
        {
            IEnumerable<Envoy> targetEnvoys = target.GetDefaultBuildSpecScope().Envoy.Envoys;
            IList<string> componentNames = new List<string>();

            foreach (Envoy targetEnvoy in targetEnvoys)
            {
                if (await IsComponentForDisplayAsync(targetEnvoy))
                {
                    componentNames.Add('\t' + targetEnvoy.Name.Last);
                }
            }

            return string.Join(Environment.NewLine, componentNames);
        }

        private async Task<bool> IsComponentForDisplayAsync(Envoy envoy)
        {
            if ((envoy.ModelDefinitionType == ComponentDefinition.ModelDefinitionTypeKeyword
                    || envoy.BindingKeywords.Contains(ComponentDefinition.ModelDefinitionTypeKeyword))
                    && envoy.OverridingModelDefinitionType != ComponentDefinition.ReferencedModelDefinitionTypeKeyword)
            {
                using (await envoy.LoadAsync())
                {
                    IComponentSubtype subtype = envoy.GetOwningComponentConfigurationReference().Configuration.ComponentSubtype;
                    return CanBuildSubtype(subtype);
                }
            }
            return false;
        }

        private bool CanBuildSubtype(IComponentSubtype subtype)
        {
            return subtype is IBuildableComponentSubtype && subtype.ComponentType.Equals(ComponentType);
        }

        private async Task<bool> LoadAndBuildComponentEnvoyAsync(Envoy componentEnvoy)
        {
            CommandLineInterfaceApplication.WriteLineVerbose($"Resolved to {componentEnvoy.Name.Last}");

            ILockedSourceFile componentFileLock;
            try
            {
                componentFileLock = await componentEnvoy.LoadAsync();
            }
            catch (Exception e) when (ExceptionHelper.ShouldExceptionBeCaught(e))
            {
                string loadErrorMessage = DocumentExtensions.GetLoadErrorMessageForException(e, componentEnvoy.FileName());
                throw new CommandLineOperationException(loadErrorMessage, e);
            }
            using (componentFileLock)
            {
                CommandLineInterfaceApplication.WriteLineVerbose($"Loaded {componentEnvoy.Name.Last}");
                var configurationReference = componentEnvoy.GetOwningComponentConfigurationReference();
                if (ShowErrorIfSubTypeNotSupported(configurationReference))
                {
                    return false;
                }
                bool buildSucceeded = await BuildComponentAsync(configurationReference);

                bool saveFailed = Save && !await TrySaveProjectFilesAsync(componentEnvoy.Project);

                if (!buildSucceeded || saveFailed)
                {
                    CommandLineInterfaceApplication.WriteError(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            LocalizedStrings.BuildComponentTool_BuildFailed,
                            componentEnvoy.Name.Last));
                    return false;
                }

                WriteSuccessMessage(componentEnvoy.Name.Last, configurationReference.Configuration.GetOutputDirectory());
                return true;
            }
        }

        private bool ShowErrorIfSubTypeNotSupported(ComponentConfigurationReference configurationReference)
        {
            var componentSubtype = configurationReference.Configuration.ComponentSubtype;
            if (!CanBuildSubtype(componentSubtype))
            {
                string message = string.Format(
                    CultureInfo.CurrentCulture,
                    LocalizedStrings.BuildComponentTool_ComponentSubTypeNotSupported,
                    configurationReference.ComponentName.Last,
                    configurationReference.Configuration.ComponentSubtype.DisplayName);
                CommandLineInterfaceApplication.WriteError(message);
                return true;
            }

            return false;
        }

        private async Task<bool> BuildComponentAsync(ComponentConfigurationReference configurationReference)
        {
            var progressToken = new ProgressToken();
            CancellationTokenSource<CompileCancellationToken> cancellationTokenSource = CompileCancellationToken.CreateNewSource();

            using (var buildMonitor = new CommandLineBuildJobMonitor(configurationReference.Configuration, progressToken, cancellationTokenSource))
            {
                bool result = await StartBuildAsync(configurationReference, progressToken, cancellationTokenSource);
                return result && await buildMonitor.WaitForBuildToFinishAsync();
            }
        }

        private Task<bool> StartBuildAsync(ComponentConfigurationReference configurationReference, ProgressToken progressToken, CancellationTokenSource<CompileCancellationToken> cancellationTokenSource)
        {
            var buildableComponentSubtype = (IBuildableComponentSubtype)configurationReference.Configuration.ComponentSubtype;
            return buildableComponentSubtype.BuildAsync(configurationReference, cancellationTokenSource.Token, progressToken);
        }

        private static async Task<bool> TrySaveProjectFilesAsync(Project project)
        {
            bool success = true;

            foreach (Envoy projectItem in GetDirtyItemsUnderProject(project))
            {
                success &= await TrySaveEnvoyAsync(projectItem);
            }
            success &= await TrySaveProjectAsync(project);
            return success;
        }

        private static IEnumerable<Envoy> GetDirtyItemsUnderProject(Project project)
        {
            Predicate<Element> visitPredicate = element =>
                EnvoyManagerExtensions.RecurseIntoEnvoyManagers(element, EnvoySearchFlags.ForEditEnvoyManagers | EnvoySearchFlags.NullTarget);
            IEnumerable<Envoy> dirtyItemsUnderProject = project.GetDescendantsBreadthFirst(visitPredicate)
                .OfType<Envoy>()
                .Where(envoy =>
                {
                    var fileService = envoy.QueryService<IReferencedFileService>().FirstOrDefault();
                    return fileService != null && fileService.IsDirty && fileService.ReferencedFileLoaded;
                });
            return dirtyItemsUnderProject.DistinctLinkedReferences();
        }

        private static async Task<bool> TrySaveEnvoyAsync(Envoy envoy)
        {
            try
            {
                await envoy.SaveAsync();
                return true;
            }
            catch (Exception e) when (ExceptionHelper.ShouldExceptionBeCaught(e))
            {
                CommandLineInterfaceApplication.WriteError(DocumentExtensions.GetUserVisibleSaveFailureMessage(envoy.FileName(), e));
                return false;
            }
        }

        private static async Task<bool> TrySaveProjectAsync(Project project)
        {
            try
            {
                await project.SaveAsync();
                return true;
            }
            catch (Exception e) when (ExceptionHelper.ShouldExceptionBeCaught(e))
            {
                CommandLineInterfaceApplication.WriteError(DocumentExtensions.GetUserVisibleSaveFailureMessage(project.ProjectFileName, e));
                return false;
            }
        }

        private static void WriteSuccessMessage(string componentName, string outputDirectory)
        {
            string message = string.Format(
                CultureInfo.CurrentCulture,
                LocalizedStrings.BuildComponentTool_RootBuildSuccess,
                componentName,
                outputDirectory,
                CommandLineHelpers.GetFullDateTimeString());
            CommandLineInterfaceApplication.WriteLine(message);
        }
    }
}
