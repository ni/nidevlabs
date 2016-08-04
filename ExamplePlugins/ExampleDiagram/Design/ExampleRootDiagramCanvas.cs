using System;
using System.Windows.Media;
using NationalInstruments.Design;

namespace ExamplePlugins.ExampleDiagram.Design
{
    /// <summary>
    /// This class is the visual control that is used for the root diagram of the diagram editor
    /// The base class handles the majority of the work but you can override various behaviors
    /// </summary>
    public class ExampleRootDiagramCanvas : RootDiagramCanvas
    {
        /// <summary>
        /// The default constructor
        /// </summary>
        public ExampleRootDiagramCanvas()
        {
            // Set the background color of the diagram to something interesting
            // This can be any color but it should not be null or transparent.
            Background = Brushes.Azure;
        }
    }
}
