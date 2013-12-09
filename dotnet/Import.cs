using System;
using Rhino.Geometry;
using System.Runtime.InteropServices;
//
// good marshalling webpage
// http://msdn.microsoft.com/en-us/library/aa288468(VS.71).aspx
//
//

class Import
{
#if MONO_BUILD

#if __ANDROID__
	public const string lib = "opennurbs";
	public const string librdk = "opennurbs";
#else
  public const string lib = "__Internal";
  public const string librdk = "__Internal";
#endif
#else
#if OPENNURBS_SDK
  public const string lib = "rhino3dmio_native";
  public const string librdk = "rhino3dmio_native";
#else
  // DO NOT add the ".dll, .dynlib, .so, ..." extension.
  // Each platform should be smart enough to figure out how
  // to append an extension to find the dynamic library
  public const string lib = "rhcommon_c";
  public const string librdk = "rhcommonrdk_c";
#endif
#endif
  private Import() { }
}
