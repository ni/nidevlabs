# Extending the LabVIEW NXG CLI with custom tools

The LabVIEW NXG CLI (`labviewnxgcli.exe`) supports custom tools that are exported via MEF. 

## How to write a tool that can be invoked by the CLI

1. Reference NationalInstruments.CommandLineInterface.Core.dll in the csproj.
1. Create a class that inherits from `NationalInstruments.CommandLineInterface.CommandLineTool`. This class will implement a `RunAsync()` method that will be called when the CLI is invoked. 
1. In the constructor for the tool, add the set of command-line options using the `AddOption()` method. See the example code for details.
1. Add the `ExportCommandLineAttribute` to the class. The attribute must also provide the set of options that were added via `AddOption()` in the step above. By adding the options as an attribute to the class, the MEF cache system can provide help for each command without needing to load all the assemblies. This makes the CLI respond faster when providing help. See the example code for details.
1. To include other locations in MEF composition, override `ComponentPaths`. 
1. To validate the values of the command-line options passed-in by the user, override `ThrowIfOptionNotSupported()` and throw a `CommandLineInterfaceException` if the set of options passed-in is not supported. Note that required options can be specified directly in `AddOption()`.

## How to report errors to the CLI
