using System.Windows.Media;
using NationalInstruments;
using NationalInstruments.Core;
using NationalInstruments.ProjectExplorer;
using NationalInstruments.SourceModel.Envoys;
using ExamplePlugins.ExampleDiagram.SourceModel;

namespace ExamplePlugins.ExampleDiagram.Shell
{
    /// <summary>
    /// Project service factory
    /// </summary>
    [ExportProjectServiceFactory(typeof(IProjectItemInfo))]
    [BindsToModelDefinitionType(ExampleDiagramDefinition.ElementName, ExamplePluginsNamespaceSchema.ParsableNamespaceName)]
    public class ExampleDiagramProjectExplorerServiceFactory : ProjectServiceFactory
    {
        /// <inheritdoc />
        protected override IProjectService CreateService()
        {
            return Host.CreateInstance<ExampleDiagramProjectExplorerService>();
        }
    }

    /// <summary>
    /// This is a "ProjectSerice" which provides information about our definition used to display it in
    /// the "Project Explorer Window" which is the project tree view.
    /// If a file does not have a ProjectExplorerService it will not be shown in the project tree.
    /// </summary>
    public class ExampleDiagramProjectExplorerService : ProjectItemInfoSourceFileReferenceDefaultService
    {
        public static ImageSource DefaultIcon = ResourceHelpers.LoadBitmapImage(typeof(ExampleDiagramProjectExplorerService), "Resources/Paper.png");

        /// <summary>
        /// Returns the icon to use in the project tree
        /// </summary>
        public override PlatformImage Icon
        {
            get
            {
                return DefaultIcon;
            }
        }
    }
}
