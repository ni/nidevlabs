using System.Windows.Media;
using NationalInstruments.Design;
using System.Windows;
using System.Collections.Generic;
using System.Linq;

namespace ExamplePlugins.ExampleDiagram.Design
{
    /// <summary>
    /// The visual of a wire when the wire is represented as a spline.
    /// </summary>
    public class SplineWireControl : WireControl
    {
        /// <summary>
        /// Constructs a new instance
        /// </summary>
        public SplineWireControl()
        {
            DefaultStyleKey = typeof(SplineWireControl);
        }

        /// <summary>
        /// Called to update the stroke thickness of this wire.  In this case it is always a constant value.
        /// </summary>
        /// <param name="state"></param>
        protected override void UpdateStrokeThickness(WirePathPresentationState state)
        {
            InnerWireStrokeThickness = 3;
            OuterWireStrokeThickness = 5;
        }

        /// <summary>
        /// Called to update the brush used to stroke this wire
        /// </summary>
        /// <param name="state">The current wire state</param>
        protected override void UpdateStrokeBrush(WirePathPresentationState state)
        {
            if (ViewModel.IsSelected)
            {
                Stroke = Brushes.Orange;
            }
            else
            {
                Stroke = Brushes.Blue;
            }
        }

        /// <summary>
        /// The base wire control attempts to do manhattan hit testing.  This is a bug and the default should
        /// rely on WPF.  Once that is corrected this can be removed.
        /// </summary>
        protected override GeometryHitTestResult HitTestCore(GeometryHitTestParameters hitTestParameters)
        {
            List<DependencyObject> results = new List<DependencyObject>();
            VisualTreeHelper.HitTest(InnerWire, null, 
                r =>
                {
                    results.Add(r.VisualHit);
                    return HitTestResultBehavior.Stop;
                }, new GeometryHitTestParameters(hitTestParameters.HitGeometry));
            return new GeometryHitTestResult(results.FirstOrDefault() as Visual, IntersectionDetail.NotCalculated);
        }

        /// <summary>
        /// The base wire control attempts to do manhattan hit testing.  This is a bug and the default should
        /// rely on WPF.  Once that is corrected this can be removed.
        /// </summary>
        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return VisualTreeHelper.HitTest(InnerWire, hitTestParameters.HitPoint);
        }
    }
}
