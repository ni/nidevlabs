using System;
using NationalInstruments.DataTypes;
using NationalInstruments.SourceModel;
using NationalInstruments.Core;

namespace ExamplePlugins.ExampleDiagram.SourceModel
{
    /// <summary>
    /// This terminal type specifies that it should always use a spline wiring behavior.
    /// Having terminals return custom wiring behaviors lets a diagram have multiple wiring
    /// behaviors.
    /// </summary>
    public class SplineWireTerminal : NodeTerminal
    {
        private SplineWiringBehavior _wiringBehavior;

        /// <summary>
        /// Constructs a new instance
        /// </summary>
        /// <param name="direction">Direction of the terminal</param>
        /// <param name="dataType">Initial data type of the terminal</param>
        /// <param name="identifier">Node specific terminal identifier</param>
        public SplineWireTerminal(Direction direction, NIType dataType, string identifier)
            : base(direction, dataType, identifier)
        {
            _wiringBehavior = new SplineWiringBehavior();
        }

        /// <summary>
        /// Constructs a new instance
        /// </summary>
        /// <param name="direction">Direction of the terminal</param>
        /// <param name="dataType">Initial data type of the terminal</param>
        /// <param name="identifier">Node specific terminal identifier</param>
        /// <param name="hotPoint">Location, relative to the node, of the hotspot of this terminal</param>
        public SplineWireTerminal(Direction direction, NIType dataType, string identifier, SMPoint hotPoint)
            : base(direction, dataType, identifier, hotPoint)
        {
            _wiringBehavior = new SplineWiringBehavior();
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        SplineWireTerminal()
        {
            _wiringBehavior = new SplineWiringBehavior();
        }

        /// <summary>
        /// Get the wiring behavior for this wiring start; used by the wiring tool to know what kind of wire to create and how to route it
        /// </summary>
        /// <returns>an implementation of IWiringBehavior</returns>
        /// <remarks>base class implementation returns the IModel's DefaultWiringBehavior</remarks>
        public override IWiringBehavior WiringBehavior
        {
            get
            {
                return _wiringBehavior;
            }
        }
    }
}
