using System;
using NationalInstruments.Compiler;
using NationalInstruments.Composition;
using NationalInstruments.Controls.Shell;
using NationalInstruments.Core;
using NationalInstruments.MocCommon.SourceModel;
using NationalInstruments.Shell;
using NationalInstruments.SourceModel;
using NationalInstruments.SourceModel.Envoys;
using NationalInstruments.VI.SourceModel;

namespace ExamplePlugins.ExampleCommandPaneContent
{
    /// <summary>
    /// Exports a set of commands that are added to the Edit menu which perform a set of scripting operations
    /// </summary>
    [ExportPushCommandContent]
    public class ScriptingCommands : PushCommandContent
    {
        /// <summary>
        /// This is the root used for the commands in this class.  This will be placed in the edit menu
        /// and the rest of the commands will in the pull right menu of this menu item
        /// </summary>
        public static readonly ICommandEx ScriptingMenuRoot = new RelayCommandEx(RelayCommandEx.HandleNoOp)
        {
            LabelTitle = "(Plug-in) Scripting",
            MenuParent = MenuPathCommands.EditMenu,
            Weight = 0.3
        };

        /// <summary>
        /// Adds and edits a new VI to the project
        /// </summary>
        public readonly ICommandEx AddNewVICommand = new ShellRelayCommand(OnAddNewVI)
        {
            LabelTitle = "(Plug-in) Add New VI",
            MenuParent = ScriptingMenuRoot
        };

        /// <summary>
        /// Adds an edits a new member VI to the active gtype / class in the editor
        /// </summary>
        public readonly ICommandEx AddNewMemberVICommand = new ShellRelayCommand(OnAddNewMemberVI)
        {
            LabelTitle = "(Plug-in) Add New Member VI",
            MenuParent = ScriptingMenuRoot
        };

        /// <summary>
        /// Adds a new gType / class to the project
        /// </summary>
        public readonly ICommandEx AddNewTypeCommand = new ShellRelayCommand(OnAddNewType)
        {
            LabelTitle = "(Plug-in) Add New Type",
            MenuParent = ScriptingMenuRoot
        };

        /// <summary>
        /// Adds a new gType / class to the project which is derived from the currently active gtype in the
        /// editor
        /// </summary>
        public readonly ICommandEx AddNewDerivedTypeCommand = new ShellRelayCommand(OnAddNewDerviedType)
        {
            LabelTitle = "(Plug-in) Add New DerivedType",
            MenuParent = ScriptingMenuRoot
        };

        /// <summary>
        /// Overriden to add our menu items to the application menu
        /// </summary>
        /// <param name="context">The active command presentation context</param>
        public override void CreateApplicationContent(ICommandPresentationContext context)
        {
            base.CreateApplicationContent(context);
            context.Add(CommandHelpers.CreateAdjacentSeparator(ScriptingMenuRoot));
            context.Add(ScriptingMenuRoot);
            context.Add(AddNewVICommand);
            context.Add(AddNewMemberVICommand);
            context.Add(AddNewTypeCommand);
            context.Add(AddNewDerivedTypeCommand);
        }

        public static void OnAddNewVI(ICommandParameter parameter, ICompositionHost host, DocumentEditSite site)
        {
            var project = host.GetSharedExportedValue<IDocumentManager>().ActiveProject;
            var createInfo = EnvoyCreateInfo.CreateForNew(VirtualInstrument.VIModelDefinitionType, new QualifiedName("My New VI"));
            Envoy createdItem = null;
            using (var transaction = project.TransactionManager.BeginTransaction("Add A VI", TransactionPurpose.User))
            {
                var createdFile = project.CreateNewFile(null, createInfo);
                createdItem = createdFile?.Envoy;
                transaction.Commit();
            }
            createdItem?.Edit();
        }

        public static void OnAddNewMemberVI(ICommandParameter parameter, ICompositionHost host, DocumentEditSite site)
        {
            var gType = (GTypeDefinition)site?.ActiveDocument?.Envoy?.ReferenceDefinition;
            if (gType == null)
            {
                return;
            }
            var project = host.GetSharedExportedValue<IDocumentManager>().ActiveProject;
            var createInfo = EnvoyCreateInfo.CreateForNew(VirtualInstrument.VIModelDefinitionType, new QualifiedName("My New VI"));
            Envoy createdItem = null;
            using (var transaction = gType.TransactionManager.BeginTransaction("Add A VI", TransactionPurpose.User))
            {
                var createdFile = project.CreateNewFile(gType.Scope, createInfo);
                createdItem = createdFile.Envoy;
                transaction.Commit();
            }
            createdItem?.Edit();
        }

        public static void OnAddNewType(ICommandParameter parameter, ICompositionHost host, DocumentEditSite site)
        {
            var project = host.GetSharedExportedValue<IDocumentManager>().ActiveProject;
            var createInfo = EnvoyCreateInfo.CreateForNew(GTypeDefinition.ModelDefinitionTypeString, new QualifiedName("My New Type"));
            ILockedSourceFile lockSourceFile;
            using (var transaction = project.TransactionManager.BeginTransaction("Add A Type", TransactionPurpose.User))
            {
                lockSourceFile = project.CreateNewFile(null, createInfo);
                transaction.Commit();
            }
            using (var transaction = lockSourceFile.Envoy.ReferenceDefinition.TransactionManager.BeginTransaction("Make Type A Class", TransactionPurpose.NonUser))
            {
                ((GTypeDefinition)lockSourceFile.Envoy.ReferenceDefinition).BaseTypeQualifiedName = TargetCommonTypes.GObjectQualifiedName;
                transaction.Commit();
            }
        }

        public static void OnAddNewDerviedType(ICommandParameter parameter, ICompositionHost host, DocumentEditSite site)
        {
            var gType = (GTypeDefinition)site?.ActiveDocument?.Envoy?.ReferenceDefinition;
            if (gType == null)
            {
                return;
            }

            var project = host.GetSharedExportedValue<IDocumentManager>().ActiveProject;
            var createInfo = EnvoyCreateInfo.CreateForNew(GTypeDefinition.ModelDefinitionTypeString, new QualifiedName("My New Derived Type.gtype"));
            ILockedSourceFile lockSourceFile;
            using (var transaction = project.TransactionManager.BeginTransaction("Add A Type", TransactionPurpose.User))
            {
                lockSourceFile = project.CreateNewFile(null, createInfo);
                transaction.Commit();
            }
            using (var transaction = lockSourceFile.Envoy.ReferenceDefinition.TransactionManager.BeginTransaction("Make Type A Class", TransactionPurpose.NonUser))
            {
                ((GTypeDefinition)lockSourceFile.Envoy.ReferenceDefinition).BaseTypeQualifiedName = gType.Name;
                transaction.Commit();
            }
        }
    }
}
