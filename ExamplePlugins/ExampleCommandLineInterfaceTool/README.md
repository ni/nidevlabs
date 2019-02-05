# Extending the LabVIEW NXG CLI with custom tools

Introduced in LabVIEW NXG 3.1, the LabVIEW NXG CLI (`labviewnxgcli.exe`) supports custom tools that are exported via MEF. 

## How to write a tool that can be invoked by the CLI
1. Reference NationalInstruments.CommandLineInterface.Core.dll in the csproj.
1. Create a class that inherits from `NationalInstruments.CommandLineInterface.CommandLineTool`. This class will implement a `RunAsync()` method that will be called when the CLI is invoked. 
1. In the constructor for the tool, add the set of command-line options using the `AddOption()` method. See the example code for details.
1. Add the `ExportCommandLineAttribute` to the class. The attribute must also provide the set of options that were added via `AddOption()` in the step above. By adding the options as an attribute to the class, the MEF cache system can provide help for each command without needing to load all the assemblies. This makes the CLI respond faster when providing help. See the example code for details.
1. To include other locations in MEF composition, override `ComponentPaths`. 
1. To validate the values of the command-line options passed-in by the user, override `ThrowIfOptionNotSupported()` and throw a `CommandLineInterfaceException` if the set of options passed-in is not supported. Note that required options can be specified directly in `AddOption()`.

## How to report errors and status
### Return value
The integer returned by `RunAsync()` will be returned by the labviewnxgcli.exe. Return 0 to indicate success, or any other integer to indicate failure.

### Printing to the console
The NationalInstruments.CommandLineInterface.CommandLineInterfaceApplication class provides static methods for writing to the output and error streams.
1. Use `CommandLineInterfaceApplication.WriteLine()` to write to standard out.
1. Use `CommandLineInterfaceApplication.WriteLineVerbose()` to write to standard out when the user passes-in the --verbose flag. This flag is available by default to all tools.
1. Use `CommandLineInterfaceApplication.WriteError()` to write to standard error.
1. Use `CommandLineInterfaceApplication.WriteErrorVerbose()` to write to standard error when the user passes-in the --verbose flag.

### Reporting errors
There are a few ways that you can report errors from a command line tool:
1. **Throw a `CommandLineOperationException`** - This is the recommended way of reporting an expected error to the user.  The CLI application will print the message in the `Message` field to the console and then the CLI process will exit with a value of 1.  If the `ShowToolHelp` property on the exception is set, the application will also print out the tool's help, which is recommended if the error is related to the options the user passed in (such as missing a required option)
1. **Return a non-zero value from the `RunAsync()` method** - This will make the CLI process return a non-zero value.  In this case your tool is responsible for printing out information about the error.
1. **Throw another kind of exception** - This is only recommended if there's an internal error that a user should not be able to cause.  This will print out the exception message along with a stack trace.

If you find yourself calling `CommandLineInterfaceApplication.WriteError()` and then returning 1, it's easier just to throw a `CommandLineOperationException`!

> **Note**: The CLI application will log any `NIMessageBox` text that would have been shown to the console.

> **Note**: the CLI application will actually catch `EnvoyLoadException` as a special case and only print out the exception's message without a stack trace.  We decided to do this because this is a commonly-thrown exception that we don't want every tool to have to handle individually.