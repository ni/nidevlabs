using System.Windows.Media;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using NationalInstruments.Core;
using NationalInstruments.SourceModel;
using NationalInstruments.MocCommon.Design;
using NationalInstruments.Shell;

namespace ExamplePlugins.ExampleDiagram.Design
{
    /// <summary>
    /// View model for spline wires.
    /// </summary>
    public class SplineWireViewModel : MocCommonWireViewModel
    {
        /// <summary>
        /// Constructs a new instance
        /// </summary>
        /// <param name="element">The wire model this view model is bound to.</param>
        public SplineWireViewModel(Wire element)
            : base(element)
        {
            // Default is for wires to disable antialiasing so that wires render sharp.
            // This will not work for curved wires.
            AliasingDisabled = true;
        }

        /// <inheritdoc />
        protected override PlatformVisual CreateVisualControl(PlatformVisual parent)
        {
            return new SplineWireControl();
        }

        /// <inheritdoc />
        public override bool ShowStandardSelectionAdorner
        {
            get { return false; }
        }

        /// <summary>
        /// Selects this wire
        /// </summary>
        public override bool Select(DesignerSelectionMode mode, PlatformVisual source, ElementSelection elementSelection, UIPosition position)
        {
            ((SplineWireControl)this.View.AsFrameworkElement).Stroke = Brushes.Orange;
            return base.Select(mode, source, elementSelection, position);
        }

        /// <summary>
        /// Deselects this wire
        /// </summary>
        public override void Deselect(bool wasHardSelected, bool deselectSoftSelectionOnly)
        {
            ((SplineWireControl)this.View.AsFrameworkElement).Stroke = Brushes.Blue;
            base.Deselect(wasHardSelected, deselectSoftSelectionOnly);
        }

        /// <summary>
        /// Spline wire changes color instead of using a selection adorner.
        /// </summary>
        protected override FrameworkElement CreateSelectionVisual()
        {
            return null;
        }

        /// <summary>
        /// Called to rubber band select this visual.
        /// The entire wire is selected since we do not have any segments.  testRect can
        /// be ignored since we do not have segment calculations.
        /// </summary>
        public override bool RubberBandSelect(SMRect testRect)
        {
            ((SplineWireControl)this.View.AsFrameworkElement).Stroke = Brushes.Orange;
            return true;
        }

        /// <summary>
        /// This wire does not use any adorners.
        /// Just return an empty enumerable
        /// </summary>
        public override IEnumerable<Adorner> GetSoftSelectAdorners()
        {
            return Enumerable.Empty<Adorner>();
        }

        /// <summary>
        /// This wire does not use any adorners.
        /// Just return an empty enumerable
        /// </summary>
        public override IEnumerable<Adorner> GetHardSelectAdorners()
        {
            return Enumerable.Empty<Adorner>();
        }

        /// <summary>
        /// Notify that joint positions and the WireControl offset have changed.
        /// </summary>
        /// <inheritdoc />
        protected override void NotifyPropertyChanged(string name)
        {
            base.NotifyPropertyChanged(name);

            if (name == "Joints")
            {
                NotifyPropertyChanged("Offset");
                NotifyPropertyChanged("PathData");
            }
        }

        /// <summary>
        /// Creates the bezier path that is used to draw the spline wire
        /// The visual data binds to this property.
        /// </summary>
        public PathGeometry PathData
        {
            get
            {
                IEnumerable<WireJoint> joints = Joints;

                if (!joints.Any())
                {
                    return null;
                }
                Point offsetDelta = LastOffset;

                // now walk the joints and construct a curved path connecting them
                PathFigureCollection bezierFigures = new PathFigureCollection();
                WireJoint firstJoint = joints.First();
                var pairs = firstJoint.Wire.WalkJoints(firstJoint);
                Point startPoint = new Point();
                var count = pairs.Count();
                bool foundStartJoint = false;
                foreach (var pair in pairs)
                {
                    if (IsStartPoint(pair, count))
                    {
                        startPoint.X = pair.Current.X - offsetDelta.X;
                        startPoint.Y = pair.Current.Y - offsetDelta.Y;
                        foundStartJoint = true;
                        break;
                    }
                }
                if (!foundStartJoint)
                {
                    return null;
                }
                if (count >= 2)
                {
                    foreach (var pair in pairs)
                    {
                        if (IsStartPoint(pair, count))
                        {
                            continue;
                        }
                        double controlPointDistance = Math.Max(Math.Abs(startPoint.X + offsetDelta.X - pair.Current.X), Math.Abs(startPoint.Y + offsetDelta.Y - pair.Current.Y));
                        var bezierSegment = new BezierSegment()
                        {
                            // first control point
                            Point1 = new Point(startPoint.X + controlPointDistance, startPoint.Y),
                            // second control point
                            Point2 = new Point(pair.Current.X - offsetDelta.X - controlPointDistance, pair.Current.Y - offsetDelta.Y),
                            // endpoint (where the wire ends)
                            Point3 = new Point(pair.Current.X - offsetDelta.X, pair.Current.Y - offsetDelta.Y)
                        };
                        bezierFigures.Add(new PathFigure() { StartPoint = startPoint, Segments = new PathSegmentCollection() { bezierSegment } });
                        // move back to the start point now in case there is another segment after this one
                        bezierFigures.Add(new PathFigure() { StartPoint = startPoint });
                    }
                }
                PathGeometry bezierGeometry = new PathGeometry() { Figures = bezierFigures };
                return bezierGeometry;
            }
        }

        /// <summary>
        /// Is the current joint in the pair passed in a good starting joint--one which is connected to the other joints
        /// </summary>
        /// <param name="pair">the current joint pair</param>
        /// <param name="jointCount">the number of total joints in the wire</param>
        /// <returns>true if the jointCount is 2 or less and the pair.IsStart is true OR if jointCount is greater than 2 and the pair's Current joint's AdjacentJoints'
        /// count is greater than 2 (signifying that this is the branch joint from which other joints emanate)</returns>
        private bool IsStartPoint(AdjacentOrderedPair<WireJoint> pair, int jointCount)
        {
            if (((jointCount == 1 || jointCount == 2) && pair.IsStart) || (jointCount > 2 && pair.Current.AdjacentJoints.Count() >= 2))
            {
                return true;
            }
            return false;
        }
    }
}