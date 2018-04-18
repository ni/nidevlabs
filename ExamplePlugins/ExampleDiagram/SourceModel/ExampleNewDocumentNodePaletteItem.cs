using System;
using System.ComponentModel.Composition;
using ExamplePlugins.ExampleDiagram.Design;
using NationalInstruments;
using NationalInstruments.Composition;
using NationalInstruments.Core;
using NationalInstruments.SourceModel;
using NationalInstruments.SourceModel.Envoys;

namespace ExamplePlugins.ExampleDiagram.SourceModel
{
    [ExportPaletteLoader(ExampleDiagramEditControl.PaletteIdentifier)]
    [BindsToKeyword(ExampleDiagramDefinition.ElementName, ExamplePluginsNamespaceSchema.ParsableNamespaceName)]
    [PartMetadata(ExportIdentifier.ExportIdentifierKey, ProductLevel.Elemental)]
    public class ExampleNewDocumentNodePaletteItem : NewDocumentPaletteItem<Node>
    {
        protected override string Icon
        {
            get
            {
                return "/ExamplePlugins.plugin;component/Resources/Cow_40x40.xml";
            }
        }

        protected override BindingKeyword ModelDefinitionType
        {
            get
            {
                return ExampleDiagramDefinition.DefinitionType;
            }
        }

        protected override string Name
        {
            get
            {
                return "New Example Diagram";
            }
        }

        protected override BindingKeyword OverridingModelDefinitionType
        {
            get
            {
                return null;
            }
        }

        protected override string UniqueId
        {
            get
            {
                return "That Is Some Id";
            }
        }

        protected override Func<ElementCreateInfo, Node> ElementCreator()
        {
            return BasicNode.Create;
        }

        protected override PaletteElementCategory CreateParentCategory()
        {
            return null;
        }

        protected override void SetTargetPlaceholderText(string targetPlaceholderText, Node newNode)
        {
            newNode.Height = 50;
            newNode.Width = 50;
        }

        protected override Action<Envoy> ModifyNewDocument
        {
            get
            {
                return (envoy) =>
                {
                    var diagramDefinition = envoy.ReferenceDefinition as ExampleDiagramDefinition;
                    if (diagramDefinition != null)
                    {
                        using (var transaction = diagramDefinition.TransactionManager.BeginTransaction("Drop Some Nodes", TransactionPurpose.NonUser))
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                var node = GrowableNode.Create(new ElementCreateInfo());
                                node.Bounds = new SMRect(i * 75 + 100, i * 75 + 100, 50, 50);
                                diagramDefinition.RootDiagram.AddNode(node);
                            }
                            transaction.Commit();
                        }
                    }
                };
            }
        }
    }
}
