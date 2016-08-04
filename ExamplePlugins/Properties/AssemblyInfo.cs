using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Windows;
using NationalInstruments.Composition;


// This attribute is required to plug-in to the editing system
// Any assembly that has exports which need to be found by our editor
// must have this attribute or it will not be looked at.
[assembly: ParticipatesInComposition]

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("ExamplePlugins")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("National Instruments")]
[assembly: AssemblyProduct("Example Plugins")]
[assembly: AssemblyCopyright("Copyright © National Instruments 2016")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly:ThemeInfo(ResourceDictionaryLocation.None,ResourceDictionaryLocation.SourceAssembly)]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: NeutralResourcesLanguage("en-US")]

