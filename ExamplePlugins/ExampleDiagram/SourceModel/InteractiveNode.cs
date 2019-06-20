using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using NationalInstruments.DataTypes;
using NationalInstruments.SourceModel;
using NationalInstruments.SourceModel.Persistence;
using NationalInstruments.DynamicProperties;

namespace ExamplePlugins.ExampleDiagram.SourceModel
{
    public class InteractiveNode : Node
    {
        private Terminal _input1Terminal;
        private Terminal _input2Terminal;
        private Terminal _output1Terminal;
        private Terminal _output2Terminal;

        private bool _isActive;
        private string _sound;

        /// <summary>
        /// This is the specific type identifier for the node
        /// </summary>
        public const string ElementName = "InteractiveNode";

        public static readonly PropertySymbol ConcatenateInputsPropertySymbol =
                ExposeStaticProperty<InteractiveNode>(
                "IsActive",
                obj => obj.IsActive,
                (obj, value) => obj.IsActive = (bool)value,
                PropertySerializers.BooleanSerializer,
                false);

        public static readonly PropertySymbol SoundPropertySymbol =
            ExposeStaticProperty<InteractiveNode>(
                "Sound", obj => obj.Sound,
                (obj, value) => obj.Sound = (string)value,
                PropertySerializers.StringSerializer,
                string.Empty);
        
        /// <summary>
        /// The standard constructor.  To construct a new instance use the static Create method to enable
        /// two pass creation
        /// </summary>
        protected InteractiveNode()
        {
        }

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive != value)
                {
                    var wasIsActive = _isActive;
                    _isActive = value;
                    TransactionRecruiter.EnlistPropertyItem(this, "IsActive", wasIsActive, _isActive, (v, r) => { _isActive = v; }, TransactionHints.None);
                }
            }
        }

        public string Sound
        {
            get { return _sound; }
            set
            {
                if (_sound != value)
                {
                    var oldSound = _sound;
                    _sound = value;
                    TransactionRecruiter.EnlistPropertyItem(this, "Sound", oldSound, _sound, (v, r) => { _sound = v; }, TransactionHints.None);
                }
            }
        }

        /// <summary>
        /// Creates the documentation for this node.
        /// Note: These string should be loaded from a localized resource
        /// </summary>
        /// <returns>the documentation</returns>
        protected override IDocumentation CreateDocumentation()
        {
            return new Documentation()
            {
                Description = "An Interactive Node for the Example Diagram",
                Name = "Interactive Node",
                InstanceName = "Interactive Node"
            };
        }

        /// <summary>
        /// Creates the documentation for one of our terminals
        /// Note: These string should be loaded from a localized resource
        /// </summary>
        /// <param name="terminal">The terminal to create documentation for</param>
        /// <returns>the documentation</returns>
        public override IDocumentation CreateDocumentationForTerminal(Terminal terminal)
        {
            if (terminal == _input1Terminal)
            {
                return new Documentation() { Name = "Input1" };
            }
            return new Documentation() { Name = "Result" };
        }

        /// <summary>
        /// Our create this.  This is used to create a new instance either programmatically, from load, and from the palette
        /// </summary>
        /// <param name="info">creation information.  This tells us why we are being created (new, load, ...)</param>
        /// <returns>The newly created node</returns>
        [XmlParserFactoryMethod(ElementName, ExamplePluginsNamespaceSchema.ParsableNamespaceName)]
        public static InteractiveNode Create(IElementCreateInfo info)
        {
            var node = new InteractiveNode();
            node.Init(info);
            return node;
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
        /// Performs node initialization
        /// </summary>
        /// <param name="info">Information about why the node is being created</param>
        protected override void Init(IElementCreateInfo info)
        {
            base.Init(info);
            Width = 80;
            Height = 50;

            _input1Terminal = new NodeTerminal(Direction.Unknown, NITypes.Void, "Input 1", TerminalHotspots.CreateInputTerminalHotspot(TerminalSize.Small, 0));
            _input2Terminal = new NodeTerminal(Direction.Unknown, NITypes.Void, "Input 1", TerminalHotspots.CreateInputTerminalHotspot(TerminalSize.Small, 3));
            
            // We are using a custom terminal for our outputs so that the wires of these terminals will be spline wires.
            _output1Terminal = new SplineWireTerminal(Direction.Unknown, NITypes.Void, "Output 1", TerminalHotspots.CreateOutputTerminalHotspot(TerminalSize.Small, Width, 0));
            _output2Terminal = new SplineWireTerminal(Direction.Unknown, NITypes.Void, "Output 1", TerminalHotspots.CreateOutputTerminalHotspot(TerminalSize.Small, Width, 3));

            OnComponentInserted(_input1Terminal);
            OnComponentInserted(_input2Terminal);
            OnComponentInserted(_output1Terminal);
            OnComponentInserted(_output2Terminal);
        }

        /// <summary>
        /// Returns all of the "Components" of this node.  In this case our components are just our terminals.
        /// </summary>
        public override IEnumerable<Element> Components
        {
            get
            {
                yield return _input1Terminal;
                yield return _input2Terminal;
                yield return _output1Terminal;
                yield return _output2Terminal;
            }
        }
    }
}
