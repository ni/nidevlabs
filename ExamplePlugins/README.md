# NI Dev Labs
## Example Editor Extension Plugins 

This project contains several examples which extend the editor:

1. ExampleCommandPaneContent - Implements several commands which extend right click menus and the right rail of several nodes
2. ExampleCustomButtonsToolWindow - Implements a tool window accessible from the Tools Pane. The tool window imports an arbitrary number of buttons via MEF. One example button is provided, which generates DFIR from the open document hierarchy.
3. ExampleDiagram - A simple Diagram editor without any semantics
4. ExampleDocument - A simple text editor which uses the SourceModel file format for persistence
5. ExampleNode - Example nodes which plug into a VI and generate custom DFIR (execution code).
6. ExampleToolWindow - Implements a tool window pane that plugs into the editor.  This tool window tracks the current selection.
