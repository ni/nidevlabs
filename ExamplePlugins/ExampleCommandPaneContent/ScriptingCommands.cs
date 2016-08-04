using System;
using System.Linq;
using NationalInstruments.Compiler;
using NationalInstruments.Composition;
using NationalInstruments.Controls.Shell;
using NationalInstruments.Core;
using NationalInstruments.MocCommon.SourceModel;
using NationalInstruments.Shell;
using NationalInstruments.SourceModel;
using NationalInstruments.SourceModel.Envoys;
using NationalInstruments.VI.Design;
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
            LabelTitle = "Add New VI",
            MenuParent = ScriptingMenuRoot
        };

        /// <summary>
        /// Adds an edits a new member VI to the active gtype / class in the editor
        /// </summary>
        public readonly ICommandEx AddNewMemberVICommand = new ShellRelayCommand(OnAddNewMemberVI)
        {
            LabelTitle = "Add New Member VI",
            MenuParent = ScriptingMenuRoot
        };

        /// <summary>
        /// Adds a new gType / class to the project
        /// </summary>
        public readonly ICommandEx AddNewTypeCommand = new ShellRelayCommand(OnAddNewType)
        {
            LabelTitle = "Add New Type",
            MenuParent = ScriptingMenuRoot
        };

        /// <summary>
        /// Adds a new gType / class to the project which is derived from the currently active gtype in the
        /// editor
        /// </summary>
        public readonly ICommandEx AddNewDerivedTypeCommand = new ShellRelayCommand(OnAddNewDerviedType)
        {
            LabelTitle = "Add New DerivedType",
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

        /// <summary>
        /// Command handler which adds a new VI to the project (under the default target)
        /// </summary>
        public static void OnAddNewVI(ICommandParameter parameter, ICompositionHost host, DocumentEditSite site)
        {
            var project = host.GetSharedExportedValue<IDocumentManager>().ActiveProject;
            var createInfo = EnvoyCreateInfo.CreateForNew(VirtualInstrument.VIModelDefinitionType, new QualifiedName("New VI.gvi"));
            Envoy createdItem = null;
            ILockedSourceFile createdFile = null;
            // Always perform modifications from within a transaction
            // Here we are creating a user transaction which means it is undoable
            using (var transaction = project.TransactionManager.BeginTransaction("Add A VI", TransactionPurpose.User))
            {
                createdFile = project.CreateNewFile(null, createInfo);
                createdItem = createdFile?.Envoy;
                transaction.Commit();
            }
            // edit the newly created VI
            createdItem?.Edit();

            // After editing dispose our lock in the file
            createdFile?.Dispose();
        }

        /// <summary>
        /// Command handler which adds a new member VI to the gtype that is currently being edited
        /// </summary>
        public static void OnAddNewMemberVI(ICommandParameter parameter, ICompositionHost host, DocumentEditSite site)
        {
            // Check to see that the user is currently editing a type and if not do nothing.
            var gTypeEnvoy = site?.ActiveDocument?.Envoy;
            var gType = (GTypeDefinition)gTypeEnvoy?.ReferenceDefinition;
            if (gType == null)
            {
                return;
            }

            var project = host.GetSharedExportedValue<IDocumentManager>().ActiveProject;
            var createInfo = EnvoyCreateInfo.CreateForNew(VirtualInstrument.VIModelDefinitionType, new QualifiedName("New Member VI.gvi"));
            Envoy createdItem = null;
            ILockedSourceFile createdFile = null;
            // Always perform modifications from within a transaction
            // Here we are creating a user transaction which means it is undoable
            using (var transaction = gType.TransactionManager.BeginTransaction("Add A VI", TransactionPurpose.User))
            {
                createdFile = project.CreateNewFile(gType.Scope, createInfo);
                createdItem = createdFile.Envoy;
                transaction.Commit();
            }

            // Lets add a terminal to the member data
            // First we query for the merge script provider from our gtype.  Merge scripts are snippets of code that can be
            // merged into diagrams / panels / ...
            // The gtype will provide a merge script that can be used to add data item (control / terminal) to a VI and
            // many other things
            string mergeScriptText = string.Empty;
            var dataProviders = gTypeEnvoy.QueryService<IProvideMergeScriptData>();
            foreach (var dataProvider in dataProviders)
            {
                foreach (var script in dataProvider.MergeScriptData)
                {
                    if (script.ClipboardDataFormat == VIDiagramControl.ClipboardDataFormat)
                    {
                        // Found the merge script for a VI diagram, we are done
                        mergeScriptText = script.MergeText;
                        break;
                    }
                }
            }
            // Now merge the script onto the diagram of the VI we just created
            if (!string.IsNullOrEmpty(mergeScriptText))
            {
                var vi = (VirtualInstrument)createdItem.ReferenceDefinition;
                // Always perform modifications from within a transaction
                // We are making this transaction a non user so that it cannot be undone it is just the initial state of the VI
                using (var transaction = vi.TransactionManager.BeginTransaction("Add A VI", TransactionPurpose.NonUser))
                {
                    var mergeScript = MergeScript.FromString(mergeScriptText, host);
                    var resolver = new MergeScriptResolver(mergeScript, host);
                    resolver.Merge(vi.BlockDiagram);
                    // Don't forget to commit the transaction or it will cancel.
                    transaction.Commit();
                }
            }
            // Now edit the memeber VI that was just created, and this time let's edit starting on the diagram
            EditDocumentInfo editInfo = new EditDocumentInfo();
            editInfo.EditorType = typeof(VIDiagramControl);
            createdItem?.Edit(editInfo);

            // After editing dispose our lock in the file
            createdFile?.Dispose();
        }

        /// <summary>
        /// Command handler which adds a new gtype to the project
        /// </summary>
        public static void OnAddNewType(ICommandParameter parameter, ICompositionHost host, DocumentEditSite site)
        {
            var project = host.GetSharedExportedValue<IDocumentManager>().ActiveProject;
            var createInfo = EnvoyCreateInfo.CreateForNew(GTypeDefinition.ModelDefinitionTypeString, new QualifiedName("New Type.gtype"));
            ILockedSourceFile lockSourceFile;
            using (var transaction = project.TransactionManager.BeginTransaction("Add A Type", TransactionPurpose.User))
            {
                lockSourceFile = project.CreateNewFile(null, createInfo);
                transaction.Commit();
            }
            // To create a class we need to set the base type to whatever we want.  The default base type for a class is GObject
            // We don't point to the base directly and instead we set the QualifiedName of the base class we want.  The linker will then
            // find the base type by name and hook everything up based on the project configuration.
            using (var transaction = lockSourceFile.Envoy.ReferenceDefinition.TransactionManager.BeginTransaction("Make Type A Class", TransactionPurpose.NonUser))
            {
                ((GTypeDefinition)lockSourceFile.Envoy.ReferenceDefinition).BaseTypeQualifiedName = TargetCommonTypes.GObjectQualifiedName;
                transaction.Commit();
            }
            lockSourceFile?.Envoy?.Edit();

            // After editing dispose our lock in the file
            lockSourceFile?.Dispose();
        }

        /// <summary>
        /// Command handler which adds a new gtype to the project that is derived from the gtype currently being edited
        /// </summary>
        public static void OnAddNewDerviedType(ICommandParameter parameter, ICompositionHost host, DocumentEditSite site)
        {
            // Check to see that the user is currently editing a type and if not do nothing.
            // The edit site provides access to the state of the editor.  Here we are checking to see of the active document
            // is a gtype by looking at the definition that is being edited.  The definition is the model of the object being
            // edited
            var gType = (GTypeDefinition)site?.ActiveDocument?.Envoy?.ReferenceDefinition;
            if (gType == null)
            {
                return;
            }
            // Get the project being edited
            var project = host.GetSharedExportedValue<IDocumentManager>().ActiveProject;
            // Setup the create info for a gtype.
            var createInfo = EnvoyCreateInfo.CreateForNew(GTypeDefinition.ModelDefinitionTypeString, new QualifiedName("New Derived Type.gtype"));
            // The LockedSourceFile is an object which holds a document in memory
            ILockedSourceFile lockSourceFile;
            // Always perform modifications from within a transaction
            // Here we are creating a user transaction which means it is undoable
            using (var transaction = project.TransactionManager.BeginTransaction("Add A Type", TransactionPurpose.User))
            {
                lockSourceFile = project.CreateNewFile(null, createInfo);
                transaction.Commit();
            }
            // Here we are setting the base type based on the type that is currently being edited.
            using (var transaction = lockSourceFile.Envoy.ReferenceDefinition.TransactionManager.BeginTransaction("Make Type A Class", TransactionPurpose.NonUser))
            {
                ((GTypeDefinition)lockSourceFile.Envoy.ReferenceDefinition).BaseTypeQualifiedName = gType.Name;
                transaction.Commit();
            }
        }
    }
}
