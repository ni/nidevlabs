using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using NationalInstruments.Core;
using NationalInstruments.ExecutionFramework;
using NationalInstruments.PanelRuntime;

namespace FanControl
{
    /// <summary>
    /// Attached behavior which is used to generate EventStructure events for a click event
    /// This is just an example and not a proposal
    /// </summary>
    public class FanClickEventBehavior : EventBehaviorManager
    {
        /// <summary>
        /// Event data which is marshaled to the runtime
        /// This must match the NIType that was defined in the source model definition of the event
        /// otherwise the marshaller will not be happy.
        /// See FanClickDiagramEvent.cs _clickEventMarshalType for this definition
        /// </summary>
        private struct ClickEventNativeData
        {
            public int ClickCount;

            public double XPosition;

            public double YPosition;
        }

        /// <summary>
        /// Attach property which sets the template to use for interactive operations
        /// </summary>
        public static readonly DependencyProperty FanClickEventBehaviorProperty = DependencyProperty.RegisterAttached(
            "Behavior", typeof(FanClickEventBehavior), typeof(FanClickEventBehavior), new FrameworkPropertyMetadata(null, HandleClickEventBehaviorChanged));

        /// <summary>
        /// Sets the click event attached behavior on a visual
        /// </summary>
        /// <param name="visual">the visual to set the behavior on</param>
        /// <param name="behavior">the click event behavior to set</param>
        public static void SetBehavior(DependencyObject visual, FanClickEventBehavior behavior)
        {
            visual.SetValue(FanClickEventBehaviorProperty, behavior);
        }

        /// <summary>
        /// Gets the click event attached behavior on a visual
        /// </summary>
        /// <param name="visual">the visual to get the behavior for</param>
        /// <returns>the currently attached click event behavior</returns>
        public static FanClickEventBehavior GetBehavior(DependencyObject visual)
        {
            return visual.GetValue(FanClickEventBehaviorProperty) as FanClickEventBehavior;
        }

        /// <summary>
        /// Constructs a new instance
        /// </summary>
        public FanClickEventBehavior()
        {
        }

        private static void HandleClickEventBehaviorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DependencyObject target = sender as DependencyObject;
            var clickEventBehavior = e.NewValue as FanClickEventBehavior;
            if (clickEventBehavior != null)
            {
                // You must call BindToTarget when the behavior is attached to the visual
                clickEventBehavior.BindToTarget(target);
            }
        }

        /// <inheritdoc />
        protected override void BindToTarget(DependencyObject target)
        {
            base.BindToTarget(target);
            
            // Here you can do whatever is necessary to setup your specific event
            // In this case we are just hooking the mouse events
            var uiElement = target as UIElement;
            uiElement.PreviewMouseLeftButtonDown += HandleMouseLeftButtonDown;
            uiElement.PreviewMouseLeftButtonUp += HandleMouseLeftButtonUp;
        }

        private bool _gotMouseDown;
        private Point _buttonDownPanelPosition;

        private void HandleMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _gotMouseDown = true;
            _buttonDownPanelPosition = e.GetPosition(FrontPanelRuntimeOwner.GetRootPanel(Target));
        }

        private void HandleMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var panelPosition = e.GetPosition(FrontPanelRuntimeOwner.GetRootPanel(Target));

            // Some simple logic to see if the up is a click or the end of a drag
            if (_gotMouseDown && (Math.Abs(panelPosition.X - _buttonDownPanelPosition.X) < 4) && (Math.Abs(panelPosition.Y - _buttonDownPanelPosition.Y) < 4))
            {
                // It's time to send the event
                _gotMouseDown = false;
                var position = e.GetPosition(Target.AsFrameworkElement);
                
                // Fill in the event data
                var nativeData = new ClickEventNativeData()
                {
                    ClickCount = e.ClickCount,
                    XPosition = position.X,
                    YPosition = position.Y,
                };

                // Send the event to the diagram
                // If you wait to be notified when the event processing is complete
                // you can await the call to OccurEventAsync(...)
                OccurEventAsync(
                    1,           // same numeric event id that we defined in the model object for the event
                    nativeData); // The data of the event
            }
        }
    }
}
