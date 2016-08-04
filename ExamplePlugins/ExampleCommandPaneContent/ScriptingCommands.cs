using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamplePlugins.SourceModel;
using NationalInstruments.Compiler;
using NationalInstruments.Composition;
using NationalInstruments.Controls.Shell;
using NationalInstruments.Core;
using NationalInstruments.Design;
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
            LabelTitle = "Add New Class",
            MenuParent = ScriptingMenuRoot
        };

        /// <summary>
        /// Adds a new gType / class to the project which is derived from the currently active gtype in the
        /// editor
        /// </summary>
        public readonly ICommandEx AddNewDerivedTypeCommand = new ShellRelayCommand(OnAddNewDerviedType)
        {
            LabelTitle = "Add New Derived Class",
            MenuParent = ScriptingMenuRoot
        };

        /// <summary>
        /// This command create a merge script string from the selection of the active editor.
        /// </summary>
        public readonly ICommandEx CreateMergeScriptFromSelectionCommand = new ShellRelayCommand(OnCreateMergeScriptFromSelection)
        {
            LabelTitle = "Capture Merge Script from Selection",
            MenuParent = ScriptingMenuRoot
        };

        /// <summary>
        /// This command merges the last created merge script onto the active editor.  It better match the formats since we are
        /// not checking.
        /// </summary>
        public readonly ICommandEx MergeFromLastMergeScriptCommand = new ShellRelayCommand(OnMergeCapturedMergeScript)
        {
            LabelTitle = "Merge From Last Capture",
            MenuParent = ScriptingMenuRoot
        };

        /// <summary>
        /// This command adds our example "tag" attached property on all of the selected elements
        /// </summary>
        public readonly ICommandEx TagSelectionCommand = new ShellRelayCommand(OnTagSelection)
        {
            LabelTitle = "Tag Current Selection",
            MenuParent = ScriptingMenuRoot
        };

        /// <summary>
        /// This command finds all current model elements that have the example "tag" set and highlights the first element
        /// </summary>
        public readonly ICommandEx FindTaggedElementsCommand = new ShellRelayCommand(OnFindTaggedElements)
        {
            LabelTitle = "Find Tagged Elements",
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
            context.Add(CreateMergeScriptFromSelectionCommand);
            context.Add(MergeFromLastMergeScriptCommand);
            context.Add(TagSelectionCommand);
            context.Add(FindTaggedElementsCommand);
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

        private static string _lastMergeScript;

        /// <summary>
        /// This command create a merge script string from the selection of the active editor.  Merge scripts are what are used
        /// for the clipboard, palette entries, and drag drop.  Merge scripts are basically mini versions of our persisted source
        /// file format.  Merge scripts are also useful for scripting use cases where merge scripts can be pased onto a diagram,
        /// wired together and modified.  Using merge scripts as templates is easier than writing a bunch on scripting code by hand.
        /// </summary>
        public static void OnCreateMergeScriptFromSelection(ICommandParameter parameter, ICompositionHost host, DocumentEditSite site)
        {
            var selection = site.ActiveSelection.OfType<IElementViewModel>();
            if (!selection.Any())
            {
                return;
            }
            // Create an Element selection is which a selection model for the selected view models
            var elementSelection = SelectionToolViewModel.CreateElementSelection(selection, true);
            // Get the text from the merge script
            _lastMergeScript = elementSelection.CopyMergeScript(host);
        }

        /// <summary>
        /// This command merges the last created merge script onto the active editor.  It better match the formats since we are
        /// not checking.
        /// </summary>
        public static void OnMergeCapturedMergeScript(ICommandParameter parameter, ICompositionHost host, DocumentEditSite site)
        {
            if (string.IsNullOrEmpty(_lastMergeScript))
            {
                return;
            }
            // The root element is the root of what ever the active editor is editing.  This would be things like the BlockDiagram
            // or FrontPanel of a VI.
            var rootElement = site.ActiveDocumentEditor?.EditorInfo?.RootElement;
            if (rootElement != null)
            {
                // Always perform modifications from within a transaction
                // Here we are creating a user transaction which means it is undoable
                using (var transaction = rootElement.TransactionManager.BeginTransaction("Merge", TransactionPurpose.User))
                {
                    var mergeScript = MergeScript.FromString(_lastMergeScript, host);
                    var resolver = new MergeScriptResolver(mergeScript, host);
                    resolver.Merge(rootElement);
                    // Don't forget to commit the transaction
                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// This command adds our example "tag" attached property on all of the selected elements
        /// </summary>
        public static void OnTagSelection(ICommandParameter parameter, ICompositionHost host, DocumentEditSite site)
        {
            var selectedModels = site.ActiveSelection.Select(vm => vm.Model).OfType<Element>();
            if (!selectedModels.Any())
            {
                return;
            }
            // When setting a tag we are modifying the model so we must set the tags in a transaction.  Since all of the models are in
            // the same file we can set all of the tags in a single transaction.
            using (var transaction = selectedModels.First().TransactionManager.BeginTransaction("Tag Selection", TransactionPurpose.User))
            {
                foreach (var element in selectedModels)
                {
                    ExampleAttachedProperties.SetTag(element, "Tagged");
                }
                transaction.Commit();
            }
        }

        /// <summary>
        /// This command finds all current model elements that have the example "tag" set and highlights the first element
        /// </summary>
        public static void OnFindTaggedElements(ICommandParameter parameter, ICompositionHost host, DocumentEditSite site)
        {
            var rootElement = site.ActiveDocumentEditor?.EditorInfo?.RootElement;
            if (rootElement != null)
            {
                List<Element> taggedElements = new List<Element>();
                foreach (var element in rootElement.GetSelfAndDescendantsBreadthFirst((e) => true))
                {
                    if (ExampleAttachedProperties.GetTag(element) == "Tagged")
                    {
                        taggedElements.Add(element);
                    }
                }
                if (taggedElements.Any())
                {
                    // If we found tagged elements highlight all of them with a slight delay for a fun effect.
                    // Note the usage of .IgnoreAwait().  This is a convention we use to ensure that any unhandled exceptions
                    // are dealt with.
                    SlowlyHighlightElementsAsync(site, taggedElements).IgnoreAwait();
                }
            }
        }

        /// <summary>
        /// Highlights a collection of elements with a slight delay between the start of the highlight animation.
        /// </summary>
        /// <param name="site">The current document edit site</param>
        /// <param name="elements">The elements to highlight</param>
        /// <returns>The task to await on</returns>
        private static async Task SlowlyHighlightElementsAsync(DocumentEditSite site, IEnumerable<Element> elements)
        {
            foreach (var element in elements)
            {
                var findOptions = new FindViewModelOptions();
                findOptions.Highlight = true;
                await site.FindViewModelForModelElementAsync(element, findOptions);
                await Task.Delay(100);
            }
        }
    }
}