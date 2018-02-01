using System;
using System.Xml.Linq;
using FanControl;
using NationalInstruments.Controls.SourceModel;
using NationalInstruments.DynamicProperties;
using NationalInstruments.VI.SourceModel;
using NationalInstruments.SourceModel.Persistence;
using NationalInstruments.SourceModel;
using NationalInstruments.PanelCommon.SourceModel;
using System.Linq;
using System.Collections.Generic;

namespace FanPlugin.SourceModel
{
    /// <summary>
    // VisualModel is the common model class for control models to inherit from
    // The IConfigurationPaneControl simply tags our model as a control to inherit a variety of base functionality for configuration (label, connector pane, etc...)
    /// </summary>
    public class FanModel : VisualModel, IConfigurationPaneControl, IProvideDiagramEvents
    {
        private const string FanName = "Fan";
        public const string ValueName = "Value";
        public const string FanSpeedName = "FanSpeed";

        // These are the properties of the Fan control that we are choosing to expose to the NextGen IDE. The 'Vale' property
        // is of particular importance as this is the one we will be representing on the diagram.
        public static readonly PropertySymbol ValueSymbol = ExposePropertySymbol<FanModel>(ValueName, false);
        public static readonly PropertySymbol FanSpeedSymbol = ExposePropertySymbol<FanModel>(FanSpeedName, FanSpeed.Low);

        protected FanModel()
        {
        }

        // This provides our parser with the name we are using to represent our control in its persisted state (in the xml)
        public override XName XmlElementName
        {
            get { return XName.Get(FanName, DemoPanelNamespaceSchema.ParsableNamespaceName); }
        }

        /// <summary>
        /// This is required to supply the parser the mechanism to actually instantiate an instance of the model for a particular
        /// name it comes across in the persisted xml.
        /// <summary>
        [XmlParserFactoryMethod(FanName, DemoPanelNamespaceSchema.ParsableNamespaceName)]
        public static FanModel Create(IElementCreateInfo info)
        {
            FanModel model = new FanModel();
            model.Init(info);
            return model;
        }

        #region IProvideDiagramEvents

        /// <summary>
        /// This is the user visible name the event structure will present to the user for this event source
        /// </summary>
        public string EventSourceName
        {
            get { return "Fan"; }
        }

        /// <summary>
        /// Determines if descendant Elements can provide diagram events.  This can be used for UI elements that are composed of
        /// UI Elements that should not be made visible to the user.
        /// </summary>
        public bool CanDescendantsProvideDiagramEvents
        {
            get { return false; }
        }

        /// <summary>
        /// In this override you need to provide all of the possible events that the control can post at runtime.  Event
        /// structures will discover these events and present them to the user
        /// </summary>
        protected override IList<IDiagramEvent> SetupDiagramEvents()
        {
            var events = base.SetupDiagramEvents();
            events.Add(new FanClickDiagramEvent(this));
            return events;
        }

        #endregion

        /// <summary>
        /// Required by the parser. In general for any property you would like to expose to the NextGen IDE, you should
        /// provide a case for it below
        /// </summary>
        public override Type GetPropertyType(PropertySymbol identifier)
        {
            switch (identifier.Name)
            {
                case ValueName:
                    return typeof(bool);
                case FanSpeedName:
                    return typeof (FanSpeed);
                default:
                    return base.GetPropertyType(identifier);
            }
        }

        /// <summary>
        /// Models the 'Value' property of the Fan control
        /// </summary>
        public bool Value
        {
            get { return ImmediateValueOrDefault<bool>(ValueSymbol); }
            set { SetOrReplaceImmediateValue(ValueSymbol, value); }
        }

        /// <summary>
        /// Models the 'FanSpeed' property of the Fan control
        /// </summary>
        public FanSpeed FanSpeed
        {
            get { return ImmediateValueOrDefault<FanSpeed>(FanSpeedSymbol); }
            set { SetOrReplaceImmediateValue(FanSpeedSymbol, value); }
        }

        /// <summary>
        /// Required to create the necessary run-time configuration of the control
        /// </summary>
        public override XamlGenerationHelper XamlGenerationHelper
        {
            get { return new FanXamlHelper(); }
        }
    }

    /// <summary>
    /// This class provides the IDE knowledge for which model property to use to bind to its default
    /// diagram terminal. In this case we select the 'Value' property. As a result, when we drop the fan
    /// control on the panel, we will get an accessor bound to it's 'Value' property available on the diagram
    /// </summary>
    [ExportModelPropertyInfo(typeof(FanModel))]
    public class FanPropertyInfo : IModelPropertyInfo
    {
        /// <inheritdoc/>
        public PropertySymbol DefaultBindableProperty => FanModel.ValueSymbol;

        /// <inheritdoc/>
        public System.Collections.Generic.IEnumerable<PropertySymbol> NonBindableProperties => Enumerable.Empty<PropertySymbol>();
    }

    /// <summary>
    /// This class is responsible for generating the appropriate run-time configuration of your control that
    /// is ideally based on the current state of your model. This is Windows-specific.
    /// </summary>
    public class FanXamlHelper : XamlGenerationHelper
    {
        public override Type ControlType => typeof(Fan);

        public override NameWithNamespace AttributeName(UIModel model, PropertySymbol propertyName)
        {
            switch (propertyName.Name)
            {
                case FanModel.FanSpeedName:
                case FanModel.ValueName:
                    return new NameWithNamespace(propertyName.Name, String.Empty);
                default:
                    return base.AttributeName(model, propertyName);
            }
        }
    }
}

