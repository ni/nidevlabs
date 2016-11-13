using System;
using System.Collections.Generic;
using System.Linq;
using NationalInstruments.Core;
using NationalInstruments.SourceModel;

namespace ExamplePlugins.ExampleDiagram.SourceModel
{
    /// <summary>
    /// A spline wiring behavior
    /// This behavior is currently minimally implemented to enable spline wires on a single diagram
    /// </summary>
    public class SplineWiringBehavior : IWiringBehavior
    {
        /// <summary>
        /// Should routing algorithm be used to avoid obstacles
        /// </summary>
        public bool AutoRoutingEnabled
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        /// <summary>
        /// Start of the wiring operation; This will help prepare for internal data structures in case of preview operations
        /// </summary>
        public void BeginWiring()
        {
        }

        /// <summary>
        ///  Auto layout all the branches of the given wire.
        /// </summary>
        /// <param name="wire"></param>
        public void CleanUpWire(Wire wire)
        {
        }

        /// <summary>
        /// Auto layout the branch of the given wire, of which the termianl is a part.
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <param name="wire">The wire.</param>
        public void CleanUpBranch(WireSegment segment, Wire wire)
        {
        }

        /// <summary>
        /// Auto layout the branch of the given wire, of which the termianl is a part.
        /// </summary>
        /// <param name="terminal">The terminal.</param>
        /// <param name="wire">The wire.</param>
        public void CleanUpBranch(WireTerminal terminal, Wire wire)
        {
        }

        /// <summary>
        /// Configure the new tunnels that has been inserted because of a wiring operation across structure boundaries.
        /// </summary>
        /// <param name="tunnels">The tunnels to be configured.</param>
        public void ConfigureTunnels(IEnumerable<BorderNode> tunnels)
        {
        }

        /// <summary>
        /// Configure any new wires created during a wiring operation.
        /// </summary>
        /// <param name="wires">The wires to be configured.</param>
        public void ConfigureWires(IEnumerable<Wire> wires)
        {
        }

        /// <summary>
        /// Create a wire.  Clients can create whatever wire-type they want, so long as it
        /// inherits from Wire.
        /// </summary>
        public Wire CreateWire()
        {
            return new SplineWire();
        }

        /// <summary>
        /// Get the initial user hint from two points specified by the user.
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="previousHint"></param>
        /// <returns></returns>
        public WireRouteHint GetInitialUserHint(DiagramPoint startPoint, DiagramPoint endPoint, WireRouteHint previousHint)
        {
            return new WireRouteHint(WireRouteDirections.East);
        }

        public DiagramPoint GetNearestPointOnSegment(WireSegment segment, DiagramPoint p)
        {
            return new DiagramPoint();
        }

        /// <summary>
        /// Get the intersection of a <see cref="Structure"/> and a <see cref="Wire"/>.
        /// </summary>
        /// <param name="structure">The structure we are trying to get an intersection with.</param>
        /// <param name="start">The start point in the structure's parent diagram or one of its nested diagrams.</param>
        /// <param name="end">The end point in the structure's diagram or one of its nested diagrams.</param>
        /// <returns>The intersection with the structure.</returns>
        public StructureIntersection GetStructureIntersection(Structure structure, DiagramPoint start, DiagramPoint end)
        {
            if (structure == null)
            {
                throw new ArgumentNullException("structure");
            }
            if (structure.Owner == null || !(structure.Owner is Diagram))
            {
                throw new ArgumentException("The structure must be a child of a diagram", "structure");
            }
            // this just does straight line intersection; we should walk the spline and find out where it intersects but I don't know
            // enough math for that.
            return structure.GetIntersection(start, end, this);
        }

        /// <summary>
        /// Get a hint for the direction(s) a wire can take from a joint.
        /// </summary>
        /// <param name="joint">The joint to get the hint from.</param>
        /// <returns>The hint.</returns>
        public WireRouteHint GetWireRouteHintFromJoint(WireJoint joint)
        {
            return new WireRouteHint(WireRouteDirections.West);
        }

        /// <summary>
        /// Get a hint for the direction(s) a wire can take from between two points in space.
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public WireRouteHint GetWireRouteHintFromPoints(DiagramPoint startPoint, DiagramPoint endPoint)
        {
            return new WireRouteHint(WireRouteDirections.West);
        }

        /// <summary>
        /// Get a hint for the direction(s) a wire can take from a segment.
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <returns>The hint.</returns>
        public WireRouteHint GetWireRouteHintFromSegment(WireSegment segment)
        {
            return new WireRouteHint(WireRouteDirections.West);
        }

        /// <summary>
        /// Get a hint for the direction(s) a wire can take from a terminal.
        /// </summary>
        /// <param name="terminal">The terminal.</param>
        /// <returns>The hint.</returns>
        public WireRouteHint GetWireRouteHintFromTerminal(WireableTerminal terminal)
        {
            return new WireRouteHint(WireRouteDirections.West);
        }

        public bool IsKeyDiagramPointInBetween(DiagramPoint keyDiagramPoint, DiagramPoint diagramPoint1, DiagramPoint diagramPoint2)
        {
            return true;
        }

        public bool PassesThroughDiagram(Diagram diagram, DiagramPoint sibling1, DiagramPoint sibling2)
        {
            return true;
        }

        /// <summary>
        /// Route the given wire in the current diagram.
        /// </summary>
        /// <param name="wire">The wire to be routed.</param>
        /// <param name="startDiagramPoint">The start Diagram Point, the point where the route starts.</param>
        /// <param name="endDiagramPoint">The end DiagramPoint, the point where the route ends.</param>
        /// <param name="startDirections">The hint direction from the start point.</param>
        /// <param name="endDirections">The hint direction to the end point.</param>
        /// <param name="startTerminal">The start terminal.</param>
        /// <param name="endTerminal">The end terminal.</param>
        /// <param name="purpose">Is the layout being called for cleanup wire operation or for preview operation (the later might give a very quick but rough intermediate results sometimes)</param>
        /// <param name="userHint">The user hint direction to the end point.</param>
        /// <param name="endingWireSegment">If the layout is ending on a wire then the segment on which it is ending (for merging operations)</param>
        /// <param name="isBranchingOperation">If the layout is branching off from an already existing wire</param>
        /// <returns>
        /// A list of diagram points that form the route.
        /// </returns>
        /// <remarks>
        /// Provides the routed path using the designated algorithm(if auto-routing is enabled),
        /// else it returns the Manhattan route"L".
        /// </remarks>
        public IList<DiagramPoint> PreviewRouteWire(Wire wire, DiagramPoint startDiagramPoint, DiagramPoint endDiagramPoint, WireRouteDirections startDirections, WireRouteDirections endDirections, Terminal startTerminal, Terminal endTerminal, WireRoutingPurpose purpose, WireRouteDirections userHint, WireSegment endingWireSegment, bool isBranchingOperation)
        {
            return new List<DiagramPoint>();
        }

        /// <summary>
        /// Generate a preview or a wire route from a joint to an end joint on an existing wire.
        /// </summary>
        /// <param name="joint">The joint to start the preview.</param>
        /// <param name="userHint">The user hint for the direction they want the wire to route.</param>
        /// <param name="existingJoint">The existing joint.</param>
        /// <param name="isBranchingOperation">Is the start joint a branch of already routed wire</param>
        /// <param name="retainPreviousRoute">Indicates if the userHint's direction must be the enddirection.</param>
        /// <returns></returns>
        public IList<DiagramPoint> PreviewRouteWireToJoint(WireJoint joint, WireRouteHint userHint, WireJoint existingJoint, bool isBranchingOperation, bool retainPreviousRoute)
        {
            return new List<DiagramPoint>();
        }

        /// <summary>
        /// Generate a preview or a wire route from a joint to an end point.
        /// </summary>
        /// <param name="joint">The joint to start the preview.</param>
        /// <param name="userHint">The user hint for the direction they want the wire to route.</param>
        /// <param name="point">The end point of the wire.</param>
        /// <param name="isBranchingOperation"> Is the start joint a branch of already routed wire</param>
        /// <param name="retainPreviousRoute"> Indicates if the userHint's direction must be the enddirection.</param>
        public IList<DiagramPoint> PreviewRouteWireToPoint(WireJoint joint, WireRouteHint userHint, WireRouteDirections endDirections, DiagramPoint point, bool isBranchingOperation, bool retainPreviousRoute)
        {
            return PreviewRouteWireToPoint(joint, userHint, endDirections, point, isBranchingOperation, retainPreviousRoute, false);
        }

        /// <summary>
        /// Previews the route wire to point.
        /// </summary>
        /// <param name="joint">The joint.</param>
        /// <param name="userHint">The user hint.</param>
        /// <param name="endDirections">The end directions.</param>
        /// <param name="point">The point.</param>
        /// <param name="isBranchingOperation">if set to <c>true</c> [is branching operation].</param>
        /// <param name="retainPreviousRoute">if set to <c>true</c> [retain previous route].</param>
        /// <param name="forceLayout">if set to <c>true</c> [force layout].</param>
        /// <returns>List of Diagram Points</returns>
        public IList<DiagramPoint> PreviewRouteWireToPoint(WireJoint joint, WireRouteHint userHint, WireRouteDirections endDirections, DiagramPoint point, bool isBranchingOperation, bool retainPreviousRoute,
            bool forceLayout)
        {
            SMPoint p = point.TransformTo(joint.Wire.Diagram).Point;
            double dx = p.X - joint.X;
            double dy = p.Y - joint.Y;
            // if the points are on top of each other do nothing
            if (WireJoint.JointEqual((float)dx, 0) && WireJoint.JointEqual((float)dy, 0))
            {
                return new List<DiagramPoint>();
            }
            return new List<DiagramPoint>(new[] { point });
        }

        /// <summary>
        /// Generate a preview or a wire route from a joint to an end segment on an existing wire.
        /// </summary>
        /// <param name="joint">The joint to start the preview.</param>
        /// <param name="userHint">The user hint for the direction they want the wire to route.</param>
        /// <param name="segment">The end segment of the wire.</param>
        /// <param name="point">The point.</param>
        /// <param name="isBranchingOperation">Is the start joint a branch of already routed wire</param>
        /// <param name="retainPreviousRoute">Indicates if the userHint's direction must be the enddirection.</param>
        /// <returns></returns>
        public IList<DiagramPoint> PreviewRouteWireToSegment(WireJoint joint, WireRouteHint userHint, WireSegment segment, DiagramPoint point, bool isBranchingOperation, bool retainPreviousRoute)
        {
            return new List<DiagramPoint>();
        }

        /// <summary>
        /// Generate a preview or a wire route from a joint to an end terminal.
        /// </summary>
        /// <param name="joint">The joint to start the preview.</param>
        /// <param name="userHint">The user hint for the direction they want the wire to route.</param>
        /// <param name="terminal">The end terminal of the wire.</param>
        /// <param name="isBranchingOperation"> Is the start joint a branch of already routed wire</param>
        /// <param name="retainPreviousRoute"> Indicates if the userHint's direction must be the enddirection.</param>
        public IList<DiagramPoint> PreviewRouteWireToTerminal(WireJoint joint, WireRouteHint userHint, WireableTerminal terminal, bool isBranchingOperation, bool retainPreviousRoute)
        {
            var point = terminal.HotspotDiagramPoint;
            SMPoint p = point.TransformTo(joint.Wire.Diagram).Point;
            double dx = p.X - joint.X;
            double dy = p.Y - joint.Y;

            // if the points are on top of each other do nothing
            if (WireJoint.JointEqual((float)dx, 0) && WireJoint.JointEqual((float)dy, 0))
            {
                return new List<DiagramPoint>();
            }
            return new List<DiagramPoint>(new[] { point });
        }

        /// <summary>
        /// Where there are segments intersecting/overlapping, short the intersections/overlaps; and then remove the cycles in the wire. 
        /// </summary>
        /// <param name="wire">The wire in which to remove cycles.</param>
        /// <param name="danglingEndHandling">how to handle dangling ends to the wire</param>
        public void RemoveCycles(IWireAlgorithmInfo wire, WireDanglingEndHandling danglingEndHandling)
        {
        }

        /// <summary>
        /// Where there are new segments intersecting with existing segments, short the intersections; and then remove the resulting cycles in the wire. 
        /// </summary>
        /// <param name="wire">The wire in which to remove cycles.</param>
        /// <param name="newJoints">The new adjacent joints. </param>
        /// <param name="danglingEndHandling">how to handle dangling ends to the wire</param>
        public void RemoveCyclesForNewJoints(Wire wire, Stack<WireJoint> newJoints, WireDanglingEndHandling danglingEndHandling)
        {
        }

        /// <summary>
        /// Shape the wire specified by the joint changes.
        /// </summary>
        /// <param name="wire">The wire to shape.</param>
        /// <param name="changedShapingElements">The wire shaping elements that effect the shape of the wire.</param>
        /// <param name="removeCycles">if set to <c>true</c> [remove cycles].</param>
        /// <returns></returns>
        public List<SimplifyJointChange> ShapeWire(Wire wire, IEnumerable<IWireShapingElement> changedShapingElements, bool removeCycles)
        {
            var spline = wire as SplineWire;
            var nodeShapers = changedShapingElements.OfType<Node>().Select(s => s);
            foreach (var shapingElement in nodeShapers)
            {
                foreach (var affectedJoint in shapingElement.EffectedJoints)
                {
                    if (wire.Diagram != affectedJoint.DiagramPoint.Diagram)
                    {
                        throw new InvalidOperationException("Need to reparent joint into parent wire's diagram");
                    }
                    var terminalPoint = ((WireableTerminal)affectedJoint.Terminal.ConnectedTerminal).HotspotDiagramPoint.Point;
                    if (affectedJoint.Point != terminalPoint)
                    {
                        affectedJoint.Point = terminalPoint;
                    }
                }
            }
            return new List<SimplifyJointChange>();
        }

        /// <summary>
        /// Simplify a wire by removing extra joints and possibly dangling ends.
        /// </summary>
        /// <param name="wire">The wire to simplify.</param>
        /// <param name="jointsToPreserve">A wire joints to preserve if at all possible.</param>
        /// <param name="overlapHandling">The overlap handling.</param>
        /// <param name="danglingEndHandling">The dangling end handling.</param>
        /// <returns>
        /// The list of simplify changes to a wire.
        /// </returns>
        /// <remarks>
        /// This function should be called after moving a wire segment (done automatically if
        /// MoveWireSegment is called) or if new joints are created, connected, and added to the wire's
        /// Joints
        /// </remarks>
        public List<SimplifyJointChange> SimplifyWire(Wire wire, HashSet<WireJoint> jointsToPreserve, WireOverlapHandling overlapHandling, WireDanglingEndHandling danglingEndHandling)
        {
            return new List<SimplifyJointChange>();
        }

        /// <summary>
        /// Simplify a joint.
        /// </summary>
        /// <param name="joint">The joint.</param>
        /// <param name="jointsToPreserve">The joints to preserve.</param>
        /// <param name="addedJoints">The added joints.</param>
        /// <param name="overlapProcessing">The overlap processing.</param>
        /// <param name="originalConnectivity">The original connectivity.</param>
        /// <param name="danglingEndClipping">The dangling end clipping.</param>
        /// <param name="changes">The changes.</param>
        /// <returns>The effected joints</returns>
        public IEnumerable<WireJoint> SimplifyJoint(WireJoint joint, HashSet<WireJoint> jointsToPreserve, List<WireJoint> addedJoints, WireOverlapHandling overlapProcessing, WireConnectivity originalConnectivity, WireDanglingEndHandling danglingEndClipping, List<SimplifyJointChange> changes)
        {
            return new List<WireJoint>();
        }

        /// <summary>
        /// The property indicates if the SnapToGrid property is enabled for the blockDiagram.
        /// Used as a hint to the routing algorithm.
        /// </summary>
        public bool SnapToGridLine
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        /// <summary>
        /// Gets whether this wiring behavior supports tacking down a wire and continuing to wire from the tack point
        /// </summary>
        public bool SupportsTacking
        {
            get
            {
                // spline wiring could support tacking if desired, probably just need to watch for it in the JointsToCurvedPathDataConverter
                // and create other bezier segments and connect them together smoothly
                return false;
            }
            set
            {
                // not supported
            }
        }

        /// <summary>
        /// Gets whether this wiring behavior supports branching a wire from a segment
        /// </summary>
        public bool SupportsSegmentBranching
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets any additional wiring transaction tags specific to the behavior
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<WiringTransaction> AdditionalWiringTransactionTagsForBehavior
        {
            get
            {
                return Enumerable.Empty<WiringTransaction>();
            }
        }

        /// <summary>
        /// The grid size for the canvas. Used as a hint to the routing algorithm.
        /// </summary>
        public SMSize SnapToGridSize
        {
            get
            {
                return new SMSize(StockDiagramGeometries.GridSize, StockDiagramGeometries.GridSize);
            }
            set
            {
            }
        }

        /// <summary>
        /// Toggle the user's routing hint.
        /// </summary>
        /// <param name="currentUserHint">The current user <see cref="WireRouteHint"/></param>
        /// <returns>The toggled user hint.</returns>
        public WireRouteHint ToggleUserRouteHint(WireRouteHint currentUserHint)
        {
            return new WireRouteHint(WireRouteDirections.East);
        }

        public bool TryGetWireSegmentIntersection(WireSegment segment, DiagramPoint p1, DiagramPoint p2, out DiagramPoint intersection)
        {
            intersection = new DiagramPoint();
            return false;
        }

        public bool ValidateSegmentPoints(DiagramPoint point1, DiagramPoint point2)
        {
            return true;
        }

        /// <summary>
        /// Gets whether this wiring behavior supports inter diagram wiring
        /// </summary>
        public bool AllowsInterDiagramWiring
        {
            get { return true; }
        }

        /// <summary>
        /// Get the modified position of the tunnel. Could be overriden to force a tunnel on one of the faces of structure.
        /// </summary>
        /// <param name="structure">Structure to create tunnel on.</param>
        /// <param name="initialPoint">Original position of the tunnel.</param>
        /// <returns></returns>
        public DiagramPoint GetAdjustedTunnelPosition(WireJoint startJoint, Structure structure, DiagramPoint originalPoint)
        {
            return originalPoint;
        }

        /// <summary>
        /// Gets the modified end target of the wire.
        /// </summary>
        /// <returns></returns>
        public IWiringEnd GetModifiedWireEndTarget(WireJoint startJoint, Diagram endDiagram, ref DiagramPoint endPoint, IWiringEnd originalWireEndTarget)
        {
            return originalWireEndTarget;
        }

        /// <summary>
        /// Gets whether this wiring behavior supports multiple tunnels across the same structure for single wire.
        /// </summary>
        public bool AllowsMultipleTunnelsForWire
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Reparents the joints and get joints to connect.
        /// </summary>
        /// <param name="jointsInNewParent">The joints in new parent.</param>
        /// <param name="existingWire">The existing wire.</param>
        /// <param name="diagramToReparentTo">The diagram to reparent to.</param>
        /// <param name="commonJointInOldWire">The common joint in old wire.</param>
        /// <param name="commonJointInNewWire">The common joint in new wire.</param>
        public void ReparentJointsAndGetJointsToConnect(IEnumerable<WireJoint> jointsInNewParent, Wire existingWire, Diagram diagramToReparentTo, IWireShapingContext shapingContext, out WireJoint commonJointInOldWire, out WireJoint commonJointInNewWire)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the wire shaping context.
        /// </summary>
        /// <returns></returns>
        public IWireShapingContext CreateWireShapingContext()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Cleans wire for stray segments and small whiskers
        /// </summary>
        /// <param name="wire">Wire to be cleaned</param>
        /// <param name="cleanupDistance">Maximum length of the branch that can be deleted while cleanup</param>
        /// <returns>True if joints are deleted</returns>
        public bool CleanUpDanglingBranches(Wire wire, float cleanupDistance)
        {
            return false;
        }

        /// <summary>
        /// Perform a clean up of unnecessary tunnel when reparenting
        /// </summary>
        public bool DeleteUnnecessaryTunnelsWhenReparenting
        {
            get { return true; }
        }

        /// <summary>
        /// Shapes the offline wire.
        /// </summary>
        /// <param name="shapingWire">The shaping wire.</param>
        /// <param name="removeCycles">if set to <c>true</c> [remove cycles].</param>
        /// <returns>The list of joints that have been simplified along with the reason</returns>
        public List<SimplifyShapingJointChange> ShapeOfflineWire(ShapingWire shapingWire, bool removeCycles)
        {
            return new List<SimplifyShapingJointChange>();
        }

        /// <summary>
        /// Gets a value indicating whether to use offline shaping or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use offline shaping]; otherwise, <c>false</c>.
        /// </value>
        public bool UseOfflineShaping
        {
            get { return false; }
        }

        public bool IsPointNearSegment(WireSegment segment, DiagramPoint p, float threshold)
        {
            return false;
        }
    }
}