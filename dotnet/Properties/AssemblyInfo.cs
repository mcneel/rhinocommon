using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("RhinoCommon")]
[assembly: AssemblyDescription("Cross Platform Rhino.NET SDK")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Robert McNeel & Associates")]
[assembly: AssemblyProduct("Rhino")]
[assembly: AssemblyCopyright("Copyright ©  2012")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
// Brian Gillespie: 9/22/2010
// See http://msdn.microsoft.com/en-us/library/system.security.securityrulesattribute.aspx
// This is needed to get the AssemblyResolver to work... probably a bunch more:
// [assembly: System.Security.SecurityRules(System.Security.SecurityRuleSet.Level1)]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("e1abd154-3fbb-4901-b1b7-76202cafc6bf")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
// Roll the version number when we don't want older Mono built plug-ins to run
// The autobuild used to tweak this number for every build and we stopped doing this
// with a build number in the mid-12000 range. Starting with build-15000, the Build
// number is manually set by Steve
// 10 Feb 2011 - updated to 15006 because exposed python classes have changed which
//               should break python on OSX
// 31 July 2012 (5.0.15006.1) Rolled revision to 1 in order to test plug-in versioning in Rhino 5
// 06 Aug 2012 (5.1.30000.0) Rolled minor to 1 and build number to 30000 since the autobuild
//                           was actually rolling version numbers
// 10 Aug 2012 (5.1.30000.1) Added document properties and object properties custom page support
//                           Added ObjectTable.Replace which takes a point cloud
//                           Added MeshFaceList.AddFaces
[assembly: AssemblyVersion("5.1.30000.1")]

[assembly: AssemblyFileVersion("5.0.20693.0")]

[assembly: System.CLSCompliant(true)]

// 23 April 2007 S. Baer (RR 25439)
// Plug-Ins that are being loaded from a network drive will throw security exceptions
// if they are not marked with the AllowPartiallyTrustedCallersAttribute. This assembly
// also requires that this attribute be set in order for things to work.
[assembly: System.Security.AllowPartiallyTrustedCallers]