using System;
using System.Windows.Media;
using NationalInstruments.Design;

namespace ExamplePlugins.ExampleDiagram.Design
{
    public class ExampleRootDiagramCanvas : RootDiagramCanvas
    {
        public ExampleRootDiagramCanvas()
        {
            Background = Brushes.Azure;
        }
    }
}
