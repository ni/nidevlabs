﻿# NI Dev Labs
## Extend NI's development environment with C# 

This repository contains examples for how to extend the editor and also how to create a custom control and expose it in the editor.
There are 4 projects in the solution:

1. ExamplePlugins - Contains the source for several editor extensions.  [See ExamplePlugins.md](ExamplePlugins/README.md).
2. Fan - Custom WPF Fan control
3. FanEditorExtensions - Editor extensions to host and edit a Fan control on the front panel of a VI
4. Programmatic Control - Shows how to programmatically open a project, compile and run a VI, and then retrieve the results of the VI execution.

This solution is configured to copy everything to the standard install location of the LabVIEW NXG Technology Preview
"C:\Program Files\National Instruments\LabVIEW NXG <version#>"

Since this location can only be written to by an administrator you should run Visual Studio as an administrator so that the build can copy the output files to the LabVIEW NXG Technology Preview directory.

If you installed LabVIEW NXG Technology Preview somewhere else you will need to change the location of the executable project that is used to launch.
You will also need to change the [InstallLocation.targets](InstallLocation.targets) file to refer to the install location.  
The install location affects where the projects look for the dependent assemblies and where the built assemblies are copied for use.

The APIs and features used by these examples are expected to change and may be removed with new releases.  The plan is to keep these examples up to date with each new release.
 
To remove this example from the installed application, delete the following files from the install directory:

* ExamplePlugins.dll
* ExamplePlugins.pdb
* Fan.dll
* Fan.pdb
* FanDemo.dll
* FanDemo.pdb
* ProgrammaticControl.exe
* ProgrammaticControl.pdb
* disable_cache.txt
