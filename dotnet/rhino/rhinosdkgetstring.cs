using System;
using System.Runtime.InteropServices;

namespace Rhino.Input.Custom
{
  public class GetString : GetBaseClass
  {
    /// <summary>
    /// Create a new GetString
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_addlayer.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addlayer.cs' lang='cs'/>
    /// <code source='examples\py\ex_addlayer.py' lang='py'/>
    /// </example>
    public GetString()
    {
      IntPtr ptr = UnsafeNativeMethods.CRhinoGetString_New();
      Construct(ptr);
    }

    /// <summary>call to get a string</summary>
    /// <returns></returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addlayer.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addlayer.cs' lang='cs'/>
    /// <code source='examples\py\ex_addlayer.py' lang='py'/>
    /// </example>
    [CLSCompliant(false)]
    public GetResult Get()
    {
      IntPtr ptr = NonConstPointer();
      int rc = UnsafeNativeMethods.CRhinoGetString_Get(ptr, false);
      return (GetResult)rc;
    }

    [CLSCompliant(false)]
    public GetResult GetLiteralString()
    {
#if USING_V5_SDK
      IntPtr ptr = NonConstPointer();
      int rc = UnsafeNativeMethods.CRhinoGetString_Get(ptr,true);
      return (GetResult)rc;
#else
      return Get();
#endif
    }
  }

  /// <summary>
  /// If you want to explicitly get string input, then use GetString class with
  /// options. If you only want to get options, then use this class (GetOption)
  /// </summary>
  public class GetOption : GetBaseClass
  {
    public GetOption()
    {
      IntPtr ptr = UnsafeNativeMethods.CRhinoGetOption_New();
      Construct(ptr);
    }

    /// <summary>
    /// Call to get an option. A return value of "option" means the user selected
    /// a valid option. Use Option() the determine which option
    /// </summary>
    /// <returns></returns>
    [CLSCompliant(false)]
    public GetResult Get()
    {
      IntPtr ptr = NonConstPointer();
      int rc = UnsafeNativeMethods.CRhinoGetOption_Get(ptr);
      return (GetResult)rc;
    }
  }

  /// <summary>used to get double precision numbers</summary>
  public class GetNumber : GetBaseClass
  {
    /// <summary>Create a new GetNumber</summary>
    /// <example>
    /// <code source='examples\vbnet\ex_addbackgroundbitmap.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addbackgroundbitmap.cs' lang='cs'/>
    /// <code source='examples\py\ex_addbackgroundbitmap.py' lang='py'/>
    /// </example>
    public GetNumber()
    {
      IntPtr ptr = UnsafeNativeMethods.CRhinoGetNumber_New();
      Construct(ptr);
    }

    /// <summary>Call to get a number</summary>
    /// <returns></returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addbackgroundbitmap.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addbackgroundbitmap.cs' lang='cs'/>
    /// <code source='examples\py\ex_addbackgroundbitmap.py' lang='py'/>
    /// </example>
    [CLSCompliant(false)]
    public GetResult Get()
    {
      IntPtr ptr = NonConstPointer();
      int rc = UnsafeNativeMethods.CRhinoGetNumber_Get(ptr);
      return (GetResult)rc;
    }

    /// <summary>
    /// Set a lower limit on the number that can be returned.
    /// By default there is no lower limit.
    /// </summary>
    /// <param name="lowerLimit">smallest acceptable number</param>
    /// <param name="strictlyGreaterThan">
    /// If true, then the returned number will be > lower_limit
    /// </param>
    /// <example>
    /// <code source='examples\vbnet\ex_addbackgroundbitmap.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addbackgroundbitmap.cs' lang='cs'/>
    /// <code source='examples\py\ex_addbackgroundbitmap.py' lang='py'/>
    /// </example>
    public void SetLowerLimit(double lowerLimit, bool strictlyGreaterThan)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.CRhinoGetNumber_SetLimit(ptr, lowerLimit, strictlyGreaterThan, true);
    }

    /// <summary>
    /// Set an upper limit on the number that can be returned.
    /// By default there is no upper limit.
    /// </summary>
    /// <param name="upperLimit">largest acceptable number</param>
    /// <param name="strictlyLessThan">If true, then the returned number will be &lt; upper_limit</param>
    public void SetUpperLimit( double upperLimit, bool strictlyLessThan )
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.CRhinoGetNumber_SetLimit(ptr, upperLimit, strictlyLessThan, false);
    }

  }

  /// <summary>used to get integer numbers</summary>
  public class GetInteger : GetBaseClass
  {
    public GetInteger()
    {
      IntPtr ptr = UnsafeNativeMethods.CRhinoGetInteger_New();
      Construct(ptr);
    }

    /// <summary>
    /// Call to get an integer
    /// </summary>
    /// <returns></returns>
    [CLSCompliant(false)]
    public GetResult Get()
    {
      IntPtr ptr = NonConstPointer();
      int rc = UnsafeNativeMethods.CRhinoGetInteger_Get(ptr);
      return (GetResult)rc;
    }

    public new int Number()
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.CRhinoGetInteger_Number(ptr);
    }

    /// <summary>
    /// Set a lower limit on the number that can be returned.
    /// By default there is no lower limit.
    /// </summary>
    /// <param name="lowerLimit">smallest acceptable number</param>
    /// <param name="strictlyGreaterThan">
    /// If true, then the returned number will be > lower_limit
    /// </param>
    public void SetLowerLimit( int lowerLimit, bool strictlyGreaterThan )
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.CRhinoGetInteger_SetLimit(ptr, lowerLimit, strictlyGreaterThan, true);
    }

    /// <summary>
    /// Set an upper limit on the number that can be returned.
    /// By default there is no upper limit.
    /// </summary>
    /// <param name="upperLimit">largest acceptable number</param>
    /// <param name="strictlyLessThan">If true, then the returned number will be &lt; upper_limit</param>
    public void SetUpperLimit( int upperLimit, bool strictlyLessThan )
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.CRhinoGetInteger_SetLimit(ptr, upperLimit, strictlyLessThan, false);
    }
  }

  // skipping CRhinoGetColor - somewhat unusual
}
