using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.DataTypes;
using NationalInstruments.SourceModel;

namespace FanDemo
{
    /// <summary>
    /// Diagram event model for a position click event
    /// </summary>
    public class FanClickDiagramEvent : DiagramEvent, IXamlGenerableEvent
    {
        /// <summary>
        /// The unique identifier for the click event
        /// </summary>
        public static readonly string ClickEventId = "{19DF459F-773E-4B02-B6E4-89FBF3D35888}";

        /// <summary>
        /// These are the names of the cluster members of this events data
        /// These names are visible to the user and will be the names of the elements in
        /// the cluster of the event data.
        /// </summary>
        public static readonly string ClickCountFieldName = "Click Count";
        public static readonly string XPositionFieldName = "X Position";
        public static readonly string YPositionFieldName = "Y Position";

        /// <summary>
        /// This is a numeric ID that is unique for this event.  This event id must be
        /// unique for the type of control that is registering this event.
        /// It does not need to be unique for all controls
        /// </summary>
        public static readonly uint ClickEventIndexId = 1;

        /// <summary>
        /// This defines the event type of this event, i.e. the cluster data type of
        /// this event.
        /// </summary>
        private static Lazy<NIType> _clickEventType = new Lazy<NIType>(() =>
        {
            var eventData = PFTypes.Factory.DefineCluster();
            
            // The first field must be this control reference field
            eventData.DefineField(PFTypes.UInt32, CtrlRefFieldName);
            
            // The rest of the fields are event specific
            eventData.DefineField(PFTypes.Int32, ClickCountFieldName);
            eventData.DefineField(PFTypes.Double, XPositionFieldName);
            eventData.DefineField(PFTypes.Double, YPositionFieldName);
            var eventDataType = eventData.CreateType();

            return EventDataTypes.MakeEvent(eventDataType, "Fan Click");
        });

        /// <summary>
        /// Gets the type for the data of the click event
        /// </summary>
        public static NIType ClickEventType
        {
            get { return _clickEventType.Value; }
        }

        /// <summary>
        /// This defines the event data type we will marshal at runtime when we post the
        /// event
        /// It must be the same as _clickEventType without the control reference field
        /// </summary>
        private static Lazy<NIType> _clickEventMarshalType = new Lazy<NIType>(() =>
        {
            var eventData = PFTypes.Factory.DefineCluster();
            eventData.DefineField(PFTypes.Int32, ClickCountFieldName);
            eventData.DefineField(PFTypes.Double, XPositionFieldName);
            eventData.DefineField(PFTypes.Double, YPositionFieldName);
            return eventData.CreateType();
        });

        /// <summary>
        /// Gets the type for the data of the click event that is marshaled during execution
        /// This includes all of the event specific data.
        /// Source, type, time, and ref are part of the common data of events
        /// </summary>
        public static NIType ClickEventMarshalType
        {
            get { return _clickEventMarshalType.Value; }
        }

        /// <summary>
        /// Constructs a new instance
        /// </summary>
        /// <param name="eventSource">The owning content object</param>
        public FanClickDiagramEvent(Content eventSource) :
            base(
                EventSourceType.Content,          // This is the type of event source.  Content means an element in the VI
                eventSource,                      // The model object which is defining the event
                ClickEventId,                     // The unique ID of this event
                ClickEventIndexId,                // The integer id of this event
                "Fan Click",                      // The user visible name of this event
                ClickEventType,                   // The full cluster type of this event
                ClickEventMarshalType)            // The type of data we will marshal at runtime
        {
        }

        /// <summary>
        /// At runtime we use an "Attached Behavior" on the control to manage the event.
        /// This is the clr namespace where the attached property is implemented
        /// </summary>
        public const string FanRuntimeNamespace = "clr-namespace:FanControl;assembly=FanControl";

        /// <summary>
        /// This is called when we are generating the XAML for the panel when there are event structures registerd
        /// for this event
        /// </summary>
        /// <param name="xaml">The XAML that is being generated</param>
        /// <param name="context">The current generation context</param>
        public void AddToXaml(TypelessXamlElement xaml, XamlGenerationContext context)
        {
            // Calling this helper is all you usually need to do
            // See FanClickEventBehavior.cs in the Fan project for the implementation
            // of the attached behavior.
            XamlGenerableEventHelpers.AddToXaml(
                this,                    // Pass this along
                xaml,                    // Pass this along
                context,                 // Pass this along
                "FanClickEventBehavior", // The .net typename of the attached behavior 
                FanRuntimeNamespace);    // The clr namespace where the attached behavior is defined
        }

        /// <summary>
        /// This override lets you hide some specific fields from the user
        /// Currently we always hide the control reference since we do not have
        /// control references yet
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>false to hide the field from the user, true to let the user use the field on the diagram.</returns>
        public override bool ShouldExposeProperty(string fieldName)
        {
            if (fieldName == CtrlRefFieldName)
            {
                return false;
            }
            return base.ShouldExposeProperty(fieldName);
        }

        /// <summary>
        /// This should return the fields that we show the user when they first select this event.
        /// </summary>
        public override IEnumerable<string> DefaultEventDataNodeProperties
        {
            get { return new[] { ClickCountFieldName, XPositionFieldName, YPositionFieldName }; }
        }
    }

}
