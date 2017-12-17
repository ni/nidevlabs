using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Linq;
using NationalInstruments.Core;
using NationalInstruments.DataTypes;
using NationalInstruments.Linking;
using NationalInstruments.SourceModel;
using NationalInstruments.SourceModel.Envoys;
using NationalInstruments.SourceModel.Persistence;
using NationalInstruments;
using NationalInstruments.DynamicProperties;

namespace ExamplePlugins.ExampleDiagram.SourceModel
{
    /// <summary>
    /// Define our dependency type.  A dependency is used to establish a connection between
    /// two a model object and an item in the project.
    /// </summary>
    public class VIReferenceDependency : Dependency, IDependencyTargetExportChanged
    {
        /// <summary>
        /// Constructs a new instance
        /// </summary>
        /// <param name="owningElement">the model element which owns this dependency</param>
        public VIReferenceDependency(IQualifiedSource owningElement)
            : this(owningElement, QualifiedName.Empty)
        {
        }

        /// <summary>
        /// Constructs a new instance
        /// </summary>
        /// <param name="owningElement">the model element which owns this dependency</param>
        /// <param name="name">The name of the project item to link to</param>
        public VIReferenceDependency(IQualifiedSource owningElement, QualifiedName name) :
            base(owningElement, name)
        {
        }

        /// <inheritdoc/>
        Task IDependencyTargetExportChanged.OnExportsChangedAsync(Envoy envoy, ExportsChangedData data)
        {
            return AsyncHelpers.CompletedTask;
        }
    }

    /// <summary>
    /// Model node for an example diagram node which links to a VI in the project.
    /// You can get this nodes by dragging a VI and dropping onto an example diagram
    /// </summary>
    public class VIReferenceNode : Node, IQualifiedSource, IDependencyTargetExportChanged
    {
        private DependencyCollection _dependencies;
        private OwnerComponentCollection _components;
        private static readonly Action<IQualifiedSource, DependencyCollection> _setDependencyCollection = (s, d) => ((VIReferenceNode)s)._dependencies = d;

        /// <summary>
        /// xml element name
        /// </summary>
        public const string ElementName = "VIReference";

        /// <inheritdoc />
        public override XName XmlElementName => XName.Get(ElementName, ExamplePluginsNamespaceSchema.ParsableNamespaceName);

        /// <inheritdoc />
        public override IEnumerable<Element> Components => _components ?? OwnerComponentCollection.EmptyComponents;

        /// <summary>
        /// Persisted property which stores the name of the project item to link to
        /// </summary>
        public static readonly PropertySymbol TargetPropertySymbol = ExposeStaticProperty<VIReferenceNode>(
            "Target",
            obj => obj.Target,
            (obj, value) => { obj.Target = (QualifiedName)value; },
            PropertySerializers.QualifiedNameSerializer,
            QualifiedName.Empty);

        /// <inheritdoc/>
        public DependencyCollection Dependencies
        {
            get
            {
                if (_dependencies == null)
                {
                    _dependencies = DependencyCollection.Create(this, _setDependencyCollection, new VIReferenceDependency(this).ToEnumerable());
                }
                return _dependencies;
            }
        }

        public QualifiedName Target
        {
            get
            {
                var dependency = Dependencies.FirstOrDefault(this) as VIReferenceDependency;
                return dependency == null ? QualifiedName.Empty : dependency.Target;
            }
            set
            {
                Dependency methodCallDependency = Dependencies.FirstOrDefault(this) as VIReferenceDependency;
                if (methodCallDependency != null)
                {
                    methodCallDependency.Target = value;
                }
            }
        }

        /// <summary>
        /// Parser factory method
        /// </summary>
        /// <param name="elementCreateInfo">creation information</param>
        /// <returns>The newly created object</returns>
        [XmlParserFactoryMethod(ElementName, ExamplePluginsNamespaceSchema.ParsableNamespaceName)]
        public static VIReferenceNode Create(IElementCreateInfo elementCreateInfo)
        {
            var element = new VIReferenceNode();
            element.Init(elementCreateInfo);
            return element;
        }

        /// <inheritdoc />
        protected override IOwnerComponentCollection ComponentsForModify
        {
            get
            {
                if (_components == null)
                {
                    _components = new OwnerComponentCollection(this);
                }
                return _components;
            }
        }

        public PlatformImage Icon
        {
            get
            {
                var dependency = Dependencies.FirstOrDefault(this) as VIReferenceDependency;
                var target = dependency?.SelectedTargetEnvoy?.QueryService<IMethodCallTarget>().FirstOrDefault();
                if (target != null)
                {
                    return target.GetIcon(ViewElementTemplate.Icon);
                }
                return PlatformImage.NullImage;
            }
        }

        /// <inheritdoc />
        public Task OnExportsChangedAsync(Envoy envoy, ExportsChangedData data)
        {
            // This is called when something about the "exported" attributes of the item we are linked to changes.
            if (!(Rooted && envoy.Rooted))
            {
                return AsyncHelpers.CompletedTask;
            }
            this.TransactUpdateFromDependency(envoy, data, "Update From Dependency", () =>
            {
                if (!(Rooted && envoy.Rooted))
                {
                    return;
                }
                IMethodCallTarget target = envoy?.QueryService<IMethodCallTarget>().FirstOrDefault();
                if (target != null)
                {
                    var name = target.Name;
                    var icon = target.GetIcon(ViewElementTemplate.Icon);
                    bool envoyChanged = data.Reason == ExportsChangeReason.Resolve || data.Reason == ExportsChangeReason.Unresolve;
                    bool propertyChanged = data.Reason == ExportsChangeReason.PropertyChange;
                    if (envoyChanged || (propertyChanged && data.ChangedProperties.Any(n => n == "Icon")))
                    {
                        // Notify that the icon changed
                        TransactionRecruiter.EnlistPropertyChanged(this, "Icon");
                    }
                    if (envoyChanged || (propertyChanged && data.ChangedProperties.Any(n => n == "CacheSignature")))
                    {
                        TransactionRecruiter.EnlistPropertyChanged(this, "Signature");
                        // You can look at the signature to gather connector pane information
                        var signature = target.Signature;
                        var allParameters = signature.GetParameters();
                        foreach (var parameter in allParameters)
                        {
                            var terminalName = parameter.GetAttributeValue("NI.UserDefinedName").Value ?? "Unknown";
                            var index = parameter.GetName();
                            var usage = parameter.GetParameterTerminalUsage();
                            var input = parameter.GetInputParameterPassingRule() != NIParameterPassingRule.NotAllowed;
                            var output = parameter.GetOutputParameterPassingRule() != NIParameterPassingRule.NotAllowed;
                            Log.WriteLine($"Parameter: {terminalName}, Type: {parameter.GetDataType()}");
                        }
                    }
                }

            });
            return AsyncHelpers.CompletedTask;
        }
    }
}