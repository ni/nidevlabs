# NI Dev Labs
## Extend NI's development environment with C# 

This repository contains examples for how to extend the editor and also how to create a custom control and expose it in the editor.
There are 3 projects in the solution:
ExamplePlugins - Contains the source for the Example tool window, plug-in node, ribbon extension, and custom document type.
Fan - Custom WPF Fan control
FanEditorExtensions - Editor extensions to host and edit a Fan control on the front panel of a VI

This solution is configured to copy everything to the standard install location of LabVIEW Communications System Design Suite
"C:\Program Files (x86)\National Instruments\LabVIEW Communications System Design Suite"
Since this location can only be written to by an administrator you should run Visual Studio as an administrator so that the build can copy the output
files to the LabVIEW Comms directory.

If you installed LabVIEW Comms somewhere else you will need to change the location of the executable project that we use to launch and change the
post build step of the ExamplePlugins, Fan, and FanEditorExtensions project to refer to your install location.
You can edit each of the .csproj files and update the <InstallLocation></InstallLocation> tag to specify where you have LabVIEW Installed


To remove this example from the installed application, delete the following files from the install directory:
ExamplePlugins.dll
ExamplePlugins.pdb
Fan.dll
Fan.pdb
FanDemo.dll
FanDemo.pdb
disable_cache.txt
