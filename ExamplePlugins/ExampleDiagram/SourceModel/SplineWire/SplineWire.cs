using System;
using System.Collections.Generic;
using System.Xml.Linq;
using NationalInstruments.Core;
using NationalInstruments.SourceModel;
using NationalInstruments.SourceModel.Persistence;

namespace ExamplePlugins.ExampleDiagram.SourceModel
{
    /// <summary>
    /// A spline wire (derives from Wire base class), used by the SplineWiringBehavior
    /// </summary>
    public class SplineWire : Wire
    {
        /// <summary>
        /// 
        /// </summary>
        private const string ElementName = "SplineWire";

        /// <summary>
        /// Gets the name of the XML element.
        /// </summary>
        /// <value>
        /// The name of the XML element.
        /// </value>
        public override XName XmlElementName
        {
            get
            {
                return XName.Get(ElementName, ExamplePluginsNamespaceSchema.ParsableNamespaceName);
            }
        }

        /// <summary>
        /// Creates the specified element create info.
        /// </summary>
        /// <param name="elementCreateInfo">The element create info.</param>
        /// <returns></returns>
        [XmlParserFactoryMethod(ElementName, ExamplePluginsNamespaceSchema.ParsableNamespaceName)]
        public static SplineWire Create(IElementCreateInfo elementCreateInfo)
        {
            var wire = new SplineWire();
            wire.Init(elementCreateInfo);
            return wire;
        }

        /// <summary>
        /// Create a new joint and add it to the wire's joint collection.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>
        /// The new joint.
        /// </returns>
        public override WireJoint MakeJoint(SMPoint point)
        {
            var joint = new SplineWireJoint(point);
            AddJoint(joint);
            return joint;
        }

        /// <summary>
        /// Creates a new joint and adds it to the wire's joint collection.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="existingJoint">The existing joint it should be connected to.</param>
        /// <returns>
        /// The new joint.
        /// </returns>
        public override WireJoint MakeJoint(SMPoint point, WireJoint existingJoint)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new joint and add it to the wire's joint collection and inserts it between between two joints.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="joint1">the joint connected to joint2</param>
        /// <param name="joint2">the joint connected to joint1</param>
        /// <returns>
        /// the newly created and inserted joint or null if the joint could not be inserted
        /// </returns>
        public override WireJoint MakeJoint(SMPoint point, WireJoint joint1, WireJoint joint2)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the near wire children.
        /// </summary>
        /// <inheritdoc />
        public override IEnumerable<NearWireChild> GetNearWireChildren(DiagramPoint point, float threshold, bool considerOrthogonalSegmentsOnly)
        {
            if (Owner == null || !(Owner is Diagram))
            {
                throw new ArgumentException("Wire must be a child of a Diagram", "wire");
            }
            SMPoint position = point.TransformTo(Diagram).Point;
            foreach (var joint in Joints)
            {
                float distance = joint.GetDistance(position);
                if (distance <= Math.Max(threshold, WireJoint.JointFloatEpsilon))
                {
                    yield return new NearWireChild(joint, point, distance);
                }
            }
        }

        /// <inheritdoc />
        protected override SMRect CalculateBoundsForJointChange(SMRect originalBounds, SMPoint modifiedJointPosition, bool jointAdded, out bool boundsModified, out bool needsBoundsCompute)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Spline Wire Joint
    /// </summary>
    public class SplineWireJoint : WireJoint
    {
        private IList<SplineWireJoint> _adjacentJoints;

        /// <summary>1
        /// Empty constructor
        /// </summary>
        internal SplineWireJoint()
            : this(new SMPoint(0, 0))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SplineWireJoint"/> class.
        /// </summary>
        /// <param name="p">The p.</param>
        internal SplineWireJoint(SMPoint p) : base(p)
        {
            _adjacentJoints = new List<SplineWireJoint>();
        }

        /// <summary>
        /// Construct a wire joint connected to a previous joint.
        /// </summary>
        public override IEnumerable<WireJoint> AdjacentJoints
        {
            get { return _adjacentJoints; }
        }

        /// <summary>
        /// The transacted update of our Adjacent Joint List.
        /// </summary>
        /// <inheritdoc />
        public override WireJoint AddAdjacentJoint(WireJoint sibling, IList<Mapping<WireJoint>> collapsedJoints)
        {
            var splineJoint = sibling as SplineWireJoint;
            if (splineJoint != null)
            {
                TransactionRecruiter.EnlistCollectionItem(this, "_adjacentJoints", _ => _adjacentJoints, CollectionChangeTypes.Add, splineJoint, TransactionHints.Visual);
                _adjacentJoints.Add(splineJoint);

                var siblingAdjacentJoints = splineJoint.AdjacentJoints as List<SplineWireJoint>;
                if (siblingAdjacentJoints != null)
                {
                    TransactionRecruiter.EnlistCollectionItem(splineJoint, "_adjacentJoints", _ => siblingAdjacentJoints, CollectionChangeTypes.Add, this, TransactionHints.Visual);
                    siblingAdjacentJoints.Add(this);
                }
            }
            return splineJoint;
        }

        /// <summary>
        /// The transacted update of our Adjacent Joint List.
        /// </summary>
        /// <inheritdoc />
        public override bool RemoveAdjacentJoint(WireJoint sibling)
        {

            var splineJoint = sibling as SplineWireJoint;
            if (splineJoint != null)
            {
                TransactionRecruiter.EnlistCollectionItem(this, "_adjacentJoints", _ => _adjacentJoints, CollectionChangeTypes.Remove, splineJoint, TransactionHints.Visual);
                _adjacentJoints.Remove(splineJoint);

                var siblingAdjacentJoints = splineJoint.AdjacentJoints as List<SplineWireJoint>;
                if (siblingAdjacentJoints != null)
                {
                    TransactionRecruiter.EnlistCollectionItem(splineJoint, "_adjacentJoints", _ => siblingAdjacentJoints, CollectionChangeTypes.Add, this, TransactionHints.Visual);
                    siblingAdjacentJoints.Remove(this);
                }
                return true;
            }
            return false;
        }
    }
}
