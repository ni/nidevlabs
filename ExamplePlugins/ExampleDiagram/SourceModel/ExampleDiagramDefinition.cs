using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NationalInstruments.SourceModel;
using NationalInstruments.SourceModel.Envoys;
using NationalInstruments.SourceModel.Persistence;
using NationalInstruments.Core;
using NationalInstruments.VI.SourceModel;
using NationalInstruments;

namespace ExamplePlugins.ExampleDiagram.SourceModel
{
    /// <summary>
    /// A batch rule ensuring the consistency of our loops.
    /// </summary>
    public class MyLoopBatchRule : BatchRule
    {
        /// <inheritdoc/>
        public override ModelBatchRuleExecuteLevels InitializeForTransaction(IRuleInitializeContext context)
        {
            return ModelBatchRuleExecuteLevels.Intermediate;
        }

        /// <inheritdoc/>
        protected override void OnBeginExecuteTransactionItems(IRuleExecuteContext context)
        {
            if (context.ShouldRunIntermediateRules)
            {
                HandleBorderNodeBoundsChanges(context.MergeItems<BoundsChangeMerger<BorderNode>>());

                var boundsChanges = context.MergeItems<BoundsChangeMerger<Loop>>();
                HandleLoopBoundsChanges(boundsChanges);
            }
            ProcessTransactionItems = false;
        }

        private void HandleBorderNodeBoundsChanges(BoundsChangeMerger<BorderNode> boundsChanges)
        {
            foreach (var change in boundsChanges)
            {
                var borderNode = change.TargetElement;
                if (borderNode.Structure is Loop)
                {
                    ViewElementOverlapHelper.PreventBorderNodeOverlap(borderNode.Structure, g => ViewElementOverlapHelper.PreventBorderNodeOverlap(g));
                }
            }

            CoreBatchRule.HandleBorderNodeChanges<Loop>(boundsChanges);
        }

        private void HandleLoopBoundsChanges(BoundsChangeMerger<Loop> boundsChanges)
        {
            foreach (var change in boundsChanges)
            {
                SMRect oldBounds = change.OldBounds;
                SMRect newBounds = change.NewBounds;
                // Don't move things on structure move.
                if (!change.IsResize)
                {
                    continue;
                }

                var element = change.TargetElement;
                float leftDiff = newBounds.Left - oldBounds.Left;
                float topDiff = newBounds.Top - oldBounds.Top;
                foreach (BorderNode node in element.BorderNodes)
                {
                    if (BorderNode.GetBorderNodeDockingAxis(node.Docking) == BorderNodeDockingAxis.Horizontal)
                    {
                        node.Left -= leftDiff;
                    }
                    else
                    {
                        node.Top -= topDiff;
                    }
                }
                ViewElementOverlapHelper.PreventBorderNodeOverlap(element, g => ViewElementOverlapHelper.PreventBorderNodeOverlap(g));
                element.BorderNodes.ForEach(bn => bn.EnsureDocking());
            }
        }
    }

    public class ExampleDiagramDefinition : DiagramDefinition
    {
        /// <summary>
        /// This is the identifier used to tie this definition to the Document type which enables the
        /// editing of the definition.
        /// </summary>
        public static readonly BindingKeyword DefinitionType = new BindingKeyword(ElementName, ExamplePluginsNamespaceSchema.ParsableNamespaceName);

        /// <summary>
        /// This is the specific type identifier for the definition
        /// </summary>
        public const string ElementName = "ExampleDiagramDefinition";

        /// <summary>
        /// The constructor for the definition.  It is protected to avoid usage.
        /// New definitions should be created by calling the static Create method.
        /// </summary>
        protected ExampleDiagramDefinition()
        {
        }

        protected override void CreateBatchRules(ICollection<ModelBatchRule> rules)
        {
            base.CreateBatchRules(rules);

            rules.Add(new CoreBatchRule());
            rules.Add(new VerticalGrowNodeBoundsRule());
            rules.Add(new MyLoopBatchRule());
            rules.Add(new WiringBatchRule());
            rules.Add(new WireCommentBatchRule());
            rules.Add(new TerminalDirectionBatchRule());
        }

        protected override void Init(IElementCreateInfo info)
        {
            if (!info.ForParse)
            {
                RootDiagram = new ExampleRootDiagram();
            }
            base.Init(info);
        }

        /// <summary>
        /// Gets or sets the root diagram of the function. Child classes will likely want to expose a public property
        /// that wraps this property but is of a more specific class as needed for that model of computation.
        /// </summary>
        public ExampleRootDiagram RootDiagram
        {
            get
            {
                return Components.OfType<ExampleRootDiagram>().FirstOrDefault();
            }
            protected set
            {
                var components = ComponentsForModify;
                ExampleRootDiagram current = components.OfType<ExampleRootDiagram>().FirstOrDefault();
                if (current != null)
                {
                    components.Remove(current);
                }
                components.Add(value);
            }
        }
        
        /// <summary>
        /// Returns the persistence name of this node
        /// </summary>
        public override XName XmlElementName
        {
            get
            {
                return XName.Get(ElementName, ExamplePluginsNamespaceSchema.ParsableNamespaceName);
            }
        }

        /// <summary>
        ///  String used to identify the document type in things such as SourceFileReference.
        /// </summary>
        /// <remarks>Overriding this is a good idea for child classes. By overriding and returning a constant,
        /// you will improve performance by avoiding the expensive reflection.</remarks>
        public override BindingKeyword ModelDefinitionType
        {
            get
            {
                return DefinitionType;
            }
        }

        // This example can use the default wiring behavior which creates Manhatten wires.  This is what is used for a VI
        // If desired the SplineWiringBehavior can be used instead.  This will create spline wires instead.
        //private IWiringBehavior _wiringBehavior = new SplineWiringBehavior();
        private IWiringBehavior _wiringBehavior = new ManhattanWiringBehavior();

        protected override Lazy<IWiringBehavior> DefaultWiringBehavior
        {
            get
            {
                return new Lazy<IWiringBehavior>(() => _wiringBehavior);
            }
        }

        /// <summary>
        /// Our create this.  This is used to create a new instance either programmatically, from load, and from the palette
        /// </summary>
        /// <param name="elementCreateInfo">creation information.  This tells us why we are being created (new, load, ...)</param>
        /// <returns>The newly created definition</returns>
        [XmlParserFactoryMethod(ElementName, ExamplePluginsNamespaceSchema.ParsableNamespaceName)]
        [ExportDefinitionFactory(ElementName, ExamplePluginsNamespaceSchema.ParsableNamespaceName)]
        public static ExampleDiagramDefinition Create(IElementCreateInfo elementCreateInfo)
        {
            var definition = new ExampleDiagramDefinition();
            definition.Init(elementCreateInfo);
            return definition;
        }
    }
}
