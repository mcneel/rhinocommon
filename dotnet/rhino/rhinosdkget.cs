#pragma warning disable 1591
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Rhino.Geometry;
using Rhino.Display;
using System.Collections.Generic;
using Rhino.Runtime.InteropWrappers;

#if RHINO_SDK
namespace Rhino.Input
{
  /// <summary>
  /// Enumerates all Box getter modes.
  /// </summary>
  public enum GetBoxMode : int
  {
    /// <summary>
    /// All modes are allowed.
    /// </summary>
    All = 0,

    /// <summary>
    /// The base rectangle is created by picking the two corner points.
    /// </summary>
    Corner = 1,

    /// <summary>
    /// The base rectangle is created by picking three points.
    /// </summary>
    ThreePoint = 2,

    /// <summary>
    /// The base vertical rectangle is created by picking three points.
    /// </summary>
    Vertical = 3,

    /// <summary>
    /// The base rectangle is created by picking a center point and a corner point.
    /// </summary>
    Center = 4
  }

  /// <summary>
  /// Base class for GetObject, GetPoint, GetSphere, etc.
  /// 
  /// You will never directly create a RhinoGet but you will use its member
  /// functions after calling GetObject::GetObjects(), GetPoint::GetPoint(), and so on.
  /// 
  /// Provides tools to set command prompt, set command options, and specify
  /// if the "get" can optionally accept numbers, nothing (pressing enter),
  /// and undo.
  /// </summary>
  public static class RhinoGet
  {
    #region easy to use static get functions
    /// <summary>
    /// Returns true if the document is current in a "Get" operation.
    /// </summary>
    /// <returns>true if a getter is currently active.</returns>
    public static bool InGet(RhinoDoc doc)
    {
      return doc.InGet;
    }

    /*
    /// <summary>Returns true if currently in a GetPoint.Get()</summary>
    public static bool InGetPoint(RhinoDoc doc)
    {
      return doc.InGetPoint;
    }
    /// <summary>Returns true if currently in a GetObject.GetObjects()</summary>
    public static bool InGetObject(RhinoDoc doc)
    {
      return doc.InGetObject;
    }
    /// <summary>Returns true if currently in a GetString.Get()</summary>
    public static bool InGetString(RhinoDoc doc)
    {
      return doc.InGetString;
    }
    /// <summary>Returns true if currently in a GetNumber.Get()</summary>
    public static bool InGetNumber(RhinoDoc doc)
    {
      return doc.InGetNumber;
    }
    /// <summary>Returns true if currently in a GetOption.Get()</summary>
    public static bool InGetOption(RhinoDoc doc)
    {
      return doc.InGetOption;
    }
    /// <summary>Returns true if currently in a GetColor.Get()</summary>
    public static bool InGetColor(RhinoDoc doc)
    {
      return doc.InGetColor;
    }
    /// <summary>Returns true if currently in a GetMeshes.Get()</summary>
    public static bool InGetMeshes(RhinoDoc doc)
    {
      return doc.InGetMeshes;
    }
    */

    /// <summary>
    /// Gets a point coordinate from the document.
    /// </summary>
    /// <param name="prompt">Prompt to display in command line during the operation.</param>
    /// <param name="acceptNothing">if true, the user can press enter.</param>
    /// <param name="point">point value returned here.</param>
    /// <returns>
    /// Commands.Result.Success - got point
    /// Commands.Result.Nothing - user pressed enter
    /// Commands.Result.Cancel - user cancel point getting.
    /// </returns>
    /// <remarks>
    /// If you need options or more advanced user interface, then use GetPoint class.
    /// </remarks>
    /// <example>
    /// <code source='examples\vbnet\ex_addnamedview.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addnamedview.cs' lang='cs'/>
    /// <code source='examples\py\ex_addnamedview.py' lang='py'/>
    /// </example>
    public static Commands.Result GetPoint(string prompt, bool acceptNothing, out Point3d point)
    {
      point = new Point3d();
      uint rc = UnsafeNativeMethods.RhinoSdkGet_RhinoGetPoint(prompt, acceptNothing, ref point);
      return (Commands.Result)rc;
    }

    /// <summary>
    /// Gets a point constrained to an existing mesh in the document.
    /// </summary>
    /// <param name="meshObjectId">An ID of a mesh in the document.</param>
    /// <param name="prompt">Text prompt.</param>
    /// <param name="acceptNothing">true if nothing else should be accepted.</param>
    /// <param name="point">A point value will be assigned to this out parameter during this call.</param>
    /// <returns>A command result based on user choice.</returns>
    public static Commands.Result GetPointOnMesh(Guid meshObjectId, string prompt, bool acceptNothing, out Point3d point)
    {
      point = new Point3d();
      int gprc = UnsafeNativeMethods.RHC_RhinoGetPointOnMesh(meshObjectId, prompt, acceptNothing, ref point);
      Commands.Result rc = Commands.Result.Failure;
      if (0 == gprc)
        rc = Commands.Result.Success;
      else if (1 == gprc)
        rc = Commands.Result.Nothing;
      else if (2 == gprc)
        rc = Commands.Result.Cancel;
      return rc;
    }

    /// <summary>
    /// Gets a point constrained to an existing mesh in the document.
    /// </summary>
    /// <param name="meshObject">An mesh object in the document.</param>
    /// <param name="prompt">Text prompt.</param>
    /// <param name="acceptNothing">true if nothing else should be accepted.</param>
    /// <param name="point">A point value will be assigned to this out parameter during this call.</param>
    /// <returns>The command result based on user choice.</returns>
    public static Commands.Result GetPointOnMesh(DocObjects.MeshObject meshObject, string prompt, bool acceptNothing, out Point3d point)
    {
      return GetPointOnMesh(meshObject.Id, prompt, acceptNothing, out point);
    }

    /// <summary>Easy to use color getter.</summary>
    /// <param name="prompt">Command prompt.</param>
    /// <param name="acceptNothing">If true, the user can press enter.</param>
    /// <param name="color">Color value returned here. also used as default color.</param>
    /// <returns>
    /// <para>Commands.Result.Success - got color.</para>
    /// <para>Commands.Result.Nothing - user pressed enter.</para>
    /// <para>Commands.Result.Cancel - user cancel color getting.</para>
    /// </returns>
    /// <remarks>
    /// If you need options or more advanced user interface, then use GetColor class.
    /// </remarks>
    public static Commands.Result GetColor(string prompt, bool acceptNothing, ref Rhino.Drawing.Color color)
    {
      int abgr = Rhino.Drawing.ColorTranslator.ToWin32(color);
      uint rc = UnsafeNativeMethods.RhinoSdkGet_RhinoGetColor(prompt, acceptNothing, ref abgr, true);
      color = Runtime.Interop.ColorFromWin32(abgr);
      return (Commands.Result)rc;
    }

    /// <summary>Easy to use object getter.</summary>
    /// <param name="prompt">command prompt.</param>
    /// <param name="acceptNothing">if true, the user can press enter.</param>
    /// <param name="filter">geometry filter to use when getting objects.</param>
    /// <param name="rhObject">result of the get. may be null.</param>
    /// <returns>
    /// Commands.Result.Success - got object
    /// Commands.Result.Nothing - user pressed enter
    /// Commands.Result.Cancel - user cancel object getting.
    /// </returns>
    /// <remarks>
    /// If you need options or more advanced user interface, then use GetObject class.
    /// </remarks>
    /// <example>
    /// <code source='examples\vbnet\ex_dividebylength.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_dividebylength.cs' lang='cs'/>
    /// <code source='examples\py\ex_dividebylength.py' lang='py'/>
    /// </example>
    [CLSCompliant(false)]
    public static Commands.Result GetOneObject(string prompt, bool acceptNothing, DocObjects.ObjectType filter, out DocObjects.ObjRef rhObject)
    {
      rhObject = null;
      Custom.GetObject go = new Custom.GetObject();
      go.SetCommandPrompt(prompt);
      go.AcceptNothing(acceptNothing);
      go.GeometryFilter = filter;
      go.Get();
      Commands.Result rc = go.CommandResult();
      if (rc == Commands.Result.Success && go.ObjectCount > 0)
      {
        rhObject = go.Object(0);
      }
      go.Dispose();
      return rc;
    }


    /// <summary>Easy to use object getter.</summary>
    /// <param name="prompt">command prompt.</param>
    /// <param name="acceptNothing">if true, the user can press enter.</param>
    /// <param name="filter">geometry filter to use when getting objects.</param>
    /// <param name="objref">result of the get. may be null.</param>
    /// <returns>
    /// Commands.Result.Success - got object
    /// Commands.Result.Nothing - user pressed enter
    /// Commands.Result.Cancel - user cancel object getting.
    /// </returns>
    /// <remarks>
    /// If you need options or more advanced user interface, then use GetObject class.
    /// </remarks>
    public static Commands.Result GetOneObject(string prompt, bool acceptNothing, Custom.GetObjectGeometryFilter filter, out DocObjects.ObjRef objref)
    {
      objref = null;
      Custom.GetObject go = new Custom.GetObject();
      go.SetCommandPrompt(prompt);
      go.AcceptNothing(acceptNothing);
      go.SetCustomGeometryFilter(filter);
      go.Get();
      Commands.Result rc = go.CommandResult();
      if (rc == Commands.Result.Success && go.ObjectCount > 0)
      {
        objref = go.Object(0);
      }
      go.Dispose();
      return rc;
    }

    /// <summary>Easy to use object getter for getting multiple objects.</summary>
    /// <param name="prompt">command prompt.</param>
    /// <param name="acceptNothing">if true, the user can press enter.</param>
    /// <param name="filter">geometry filter to use when getting objects.</param>
    /// <param name="rhObjects">result of the get. may be null.</param>
    /// <returns>
    /// Commands.Result.Success - got object
    /// Commands.Result.Nothing - user pressed enter
    /// Commands.Result.Cancel - user cancel object getting.
    /// </returns>
    /// <remarks>
    /// If you need options or more advanced user interface, then use GetObject class.
    /// </remarks>
    /// <example>
    /// <code source='examples\vbnet\ex_booleandifference.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_booleandifference.cs' lang='cs'/>
    /// <code source='examples\py\ex_booleandifference.py' lang='py'/>
    /// </example>
    [CLSCompliant(false)]
    public static Commands.Result GetMultipleObjects(string prompt, bool acceptNothing, DocObjects.ObjectType filter, out DocObjects.ObjRef[] rhObjects)
    {
      rhObjects = null;
      Custom.GetObject go = new Custom.GetObject();
      go.SetCommandPrompt(prompt);
      go.AcceptNothing(acceptNothing);
      go.GeometryFilter = filter;
      go.GetMultiple(1, 0); //David: changed this from GetMultiple(1, -1), which is a much rarer case (imo).
      Commands.Result rc = go.CommandResult();
      if (rc == Commands.Result.Success && go.ObjectCount > 0)
      {
        rhObjects = go.Objects();
      }
      go.Dispose();
      return rc;
    }

    /// <summary>Easy to use object getter for getting multiple objects.</summary>
    /// <param name="prompt">command prompt.</param>
    /// <param name="acceptNothing">if true, the user can press enter.</param>
    /// <param name="filter">geometry filter to use when getting objects.</param>
    /// <param name="rhObjects">result of the get. may be null.</param>
    /// <returns>
    /// Commands.Result.Success - got object
    /// Commands.Result.Nothing - user pressed enter
    /// Commands.Result.Cancel - user cancel object getting.
    /// </returns>
    /// <remarks>
    /// If you need options or more advanced user interface, then use GetObject class.
    /// </remarks>
    public static Commands.Result GetMultipleObjects(string prompt, bool acceptNothing, Custom.GetObjectGeometryFilter filter, out DocObjects.ObjRef[] rhObjects)
    {
      rhObjects = null;
      Custom.GetObject go = new Custom.GetObject();
      go.SetCommandPrompt(prompt);
      go.AcceptNothing(acceptNothing);
      go.SetCustomGeometryFilter(filter);
      go.GetMultiple(1, 0); //David: changed this from GetMultiple(1, -1), which is a much rarer case (imo).
      Commands.Result rc = go.CommandResult();
      if (rc == Commands.Result.Success && go.ObjectCount > 0)
      {
        rhObjects = go.Objects();
      }
      go.Dispose();
      return rc;
    }

    /// <summary>Easy to use string getter.</summary>
    /// <param name="prompt">command prompt.</param>
    /// <param name="acceptNothing">if true, the user can press enter.</param>
    /// <param name="outputString">default string set to this value and string value returned here.</param>
    /// <returns>
    /// Commands.Result.Success - got string
    /// Commands.Result.Nothing - user pressed enter
    /// Commands.Result.Cancel - user cancel string getting.
    /// </returns>
    /// <remarks>
    /// If you need options or more advanced user interface, then use GetString class.
    /// </remarks>
    /// <example>
    /// <code source='examples\vbnet\ex_addnamedview.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addnamedview.cs' lang='cs'/>
    /// <code source='examples\py\ex_addnamedview.py' lang='py'/>
    /// </example>
    public static Commands.Result GetString(string prompt, bool acceptNothing, ref string outputString)
    {
      uint rc = 0;
      IntPtr resultstring = UnsafeNativeMethods.RhinoSdkGet_RhinoGetString(prompt, acceptNothing, outputString, ref rc);
      if (resultstring != IntPtr.Zero)
        outputString = Marshal.PtrToStringUni(resultstring);
      return (Commands.Result)rc;
    }

    /// <summary>Easy to use bool getter.</summary>
    /// <param name="prompt">Command prompt.</param>
    /// <param name="acceptNothing">If true, the user can press enter.</param>
    /// <param name="offPrompt">The 'false/off' message.</param>
    /// <param name="onPrompt">The 'true/on' message.</param>
    /// <param name="boolValue">Default bool value set to this and returned here.</param>
    /// <returns>The getter result based on user choice.
    /// <para>Commands.Result.Success - got value.</para>
    /// <para>Commands.Result.Nothing - user pressed enter.</para>
    /// <para>Commands.Result.Cancel - user cancelled value getting.</para>
    /// </returns>
    public static Commands.Result GetBool(string prompt, bool acceptNothing, string offPrompt, string onPrompt, ref bool boolValue)
    {
      using (Custom.GetOption get = new Custom.GetOption())
      {
        get.SetCommandPrompt(prompt);
        get.AcceptNothing(acceptNothing);
        if (acceptNothing)
          get.SetDefaultString(boolValue ? onPrompt : offPrompt);
        int on_value = get.AddOption(onPrompt);
        int off_value = get.AddOption(offPrompt);
        get.Get();
        Commands.Result result = get.CommandResult();
        if (result == Commands.Result.Success && get.Result() == GetResult.Option)
        {
          Custom.CommandLineOption option = get.Option();
          if (null != option)
          {
            if (option.Index == on_value)
              boolValue = true;
            else if (option.Index == off_value)
              boolValue = false;
          }
        }
        return result;
      }
    }


    /// <summary>
    /// Easy to use number getter.
    /// </summary>
    /// <param name="prompt">The command prompt.</param>
    /// <param name="acceptNothing">If true, the user can press Enter.</param>
    /// <param name="outputNumber">
    /// Default number is set to this value and the return number value is assigned to this variable during the call.
    /// </param>
    /// <param name="lowerLimit">The minimum allowed value.</param>
    /// <param name="upperLimit">The maximum allowed value.</param>
    /// <returns>
    /// <para>Commands.Result.Success - got number.</para>
    /// <para>Commands.Result.Nothing - user pressed enter.</para>
    /// <para>Commands.Result.Cancel - user cancel number getting.</para>
    /// </returns>
    /// <example>
    /// <code source='examples\vbnet\ex_dividebylength.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_dividebylength.cs' lang='cs'/>
    /// <code source='examples\py\ex_dividebylength.py' lang='py'/>
    /// </example>
    public static Commands.Result GetNumber(string prompt, bool acceptNothing, ref double outputNumber, double lowerLimit, double upperLimit)
    {
      uint rc = UnsafeNativeMethods.RhinoSdkGet_RhinoGetNumber(prompt, acceptNothing, false, ref outputNumber, lowerLimit, upperLimit);
      return (Commands.Result)rc;
    }
    /// <summary>
    /// Easy to use number getter.
    /// </summary>
    /// <param name="prompt">command prompt.</param>
    /// <param name="acceptNothing">if true, the user can press enter.</param>
    /// <param name="outputNumber">
    /// default number is set to this value and number value returned here.
    /// </param>
    /// <returns>
    /// Commands.Result.Success - got number
    /// Commands.Result.Nothing - user pressed enter
    /// Commands.Result.Cancel - user cancel number getting.
    /// </returns>
    /// <example>
    /// <code source='examples\vbnet\ex_dividebylength.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_dividebylength.cs' lang='cs'/>
    /// <code source='examples\py\ex_dividebylength.py' lang='py'/>
    /// </example>
    public static Commands.Result GetNumber(string prompt, bool acceptNothing, ref double outputNumber)
    {
      return GetNumber(prompt, acceptNothing, ref outputNumber, RhinoMath.UnsetValue, RhinoMath.UnsetValue);
    }

    /// <summary>
    /// Easy to use number getter.
    /// </summary>
    /// <param name="prompt">command prompt.</param>
    /// <param name="acceptNothing">if true, the user can press enter.</param>
    /// <param name="outputNumber">
    /// default number is set to this value and number value returned here.
    /// </param>
    /// <returns>
    /// Commands.Result.Success - got number
    /// Commands.Result.Nothing - user pressed enter
    /// Commands.Result.Cancel - user cancel number getting.
    /// </returns>
    public static Commands.Result GetInteger(string prompt, bool acceptNothing, ref int outputNumber)
    {
      return GetInteger(prompt, acceptNothing, ref outputNumber, int.MinValue, int.MaxValue);
    }
    /// <summary>
    /// Easy to use number getter.
    /// </summary>
    /// <param name="prompt">The command prompt.</param>
    /// <param name="acceptNothing">If true, the user can press enter.</param>
    /// <param name="outputNumber">
    /// default number is set to this value and number value returned here.
    /// </param>
    /// <param name="lowerLimit">The minimum allowed value.</param>
    /// <param name="upperLimit">The maximum allowed value.</param>
    /// <returns>
    /// Commands.Result.Success - got number
    /// Commands.Result.Nothing - user pressed enter
    /// Commands.Result.Cancel - user cancel number getting.
    /// </returns>
    public static Commands.Result GetInteger(string prompt, bool acceptNothing, ref int outputNumber, int lowerLimit, int upperLimit)
    {
      double output = outputNumber;
      uint rc = UnsafeNativeMethods.RhinoSdkGet_RhinoGetNumber(prompt, acceptNothing, true, ref output, (double)lowerLimit, (double)upperLimit);
      if (output >= int.MaxValue)
        outputNumber = int.MaxValue;
      else if (output <= int.MinValue)
        outputNumber = int.MinValue;
      else
        outputNumber = (int)output;
      return (Commands.Result)rc;
    }

    /// <summary>
    /// Gets an oriented infinite plane.
    /// </summary>
    /// <param name="plane">The plane result.</param>
    /// <returns>
    /// <para>Commands.Result.Success - got plane.</para>
    /// <para>Commands.Result.Nothing - user pressed enter.</para>
    /// <para>Commands.Result.Cancel - user cancel number getting.</para>
    /// </returns>
    /// <example>
    /// <code source='examples\vbnet\ex_splitbrepwithplane.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_splitbrepwithplane.cs' lang='cs'/>
    /// <code source='examples\py\ex_splitbrepwithplane.py' lang='py'/>
    /// </example>
    public static Commands.Result GetPlane(out Plane plane)
    {
      plane = Plane.Unset;
      Commands.Result rc = (Commands.Result)UnsafeNativeMethods.RHC_RhinoGetPlane(ref plane);
      return rc;
    }

    /// <summary>
    /// Gets a 3d rectangle.
    /// </summary>
    /// <param name="corners">corners of the rectangle in counter-clockwise order.</param>
    /// <returns>Commands.Result.Success if successful.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addclippingplane.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addclippingplane.cs' lang='cs'/>
    /// <code source='examples\py\ex_addclippingplane.py' lang='py'/>
    /// </example>
    public static Commands.Result GetRectangle(out Point3d[] corners)
    {
      corners = new Point3d[4];
      Commands.Result rc = (Commands.Result)UnsafeNativeMethods.RHC_RhinoGetRectangle(corners, IntPtr.Zero);
      if (rc != Commands.Result.Success)
        corners = null;
      return rc;
    }

    /// <summary>
    /// Gets a 3d rectangle made up of four points.
    /// </summary>
    /// <param name="mode">A get box mode.</param>
    /// <param name="firstPoint">The first corner used. Pass Point3d.Unset if you do not want to set this.</param>
    /// <param name="prompts">Optional prompts to display while getting points. May be null.</param>
    /// <param name="corners">Corners of the rectangle in counter-clockwise order will be assigned to this out parameter during this call.</param>
    /// <returns>Commands.Result.Success if successful.</returns>
    public static Commands.Result GetRectangle(GetBoxMode mode, Point3d firstPoint, IEnumerable<string> prompts, out Point3d[] corners)
    {
      corners = new Point3d[4];
      IntPtr ptr = UnsafeNativeMethods.CArgsRhinoGetPlane_New();
      UnsafeNativeMethods.CArgsRhinoGetPlane_SetMode(ptr, (int)mode);
      if (firstPoint.IsValid) UnsafeNativeMethods.CArgsRhinoGetPlane_SetFirstPoint(ptr, firstPoint);
      if (prompts != null)
      {
        int i = 0;
        foreach (string s in prompts)
        {
          if (!string.IsNullOrEmpty(s))
            UnsafeNativeMethods.CArgsRhinoGetPlane_SetPrompt(ptr, s, i++);
        }
      }

      Commands.Result rc = (Commands.Result)UnsafeNativeMethods.RHC_RhinoGetRectangle(corners, ptr);
      if (rc != Commands.Result.Success)
        corners = null;
      UnsafeNativeMethods.CArgsRhinoGetPlane_Delete(ptr);
      return rc;
    }

    /// <summary>
    /// Gets a rectangle in view window coordinates.
    /// </summary>
    /// <param name="solidPen">
    /// If true, a solid pen is used for drawing while the user selects a rectangle.
    /// If false, a dotted pen is used for drawing while the user selects a rectangle.
    /// </param>
    /// <param name="rectangle">
    /// user selected rectangle in window coordinates.
    /// </param>
    /// <param name="rectView">
    /// view that the user selected the window in.
    /// </param>
    /// <returns>Success or Cancel.</returns>
    public static Commands.Result Get2dRectangle(bool solidPen, out Rhino.Drawing.Rectangle rectangle, out RhinoView rectView)
    {
      rectangle = Rhino.Drawing.Rectangle.Empty;
      rectView = null;
      const int PS_SOLID = 0;
      const int PS_DOT = 2;
      int left = 0, top = 0, right = 0, bottom = 0;
      IntPtr ptr_view = UnsafeNativeMethods.RHC_RhinoGet2dRectangle(ref left, ref top, ref right, ref bottom, solidPen ? PS_SOLID : PS_DOT);
      if (IntPtr.Zero == ptr_view)
        return Commands.Result.Cancel;
      rectView = RhinoView.FromIntPtr(ptr_view);
      rectangle = Rhino.Drawing.Rectangle.FromLTRB(left, top, right, bottom);
      return Commands.Result.Success;
    }

    /// <summary>
    /// Asks the user to select a Box in the viewport.
    /// </summary>
    /// <param name="box">If the result is Success, this parameter will be filled out.</param>
    /// <returns>Commands.Result.Success if successful.</returns>
    public static Commands.Result GetBox(out Box box)
    {
      return GetBox(out box, GetBoxMode.All, Point3d.Unset, null, null, null);
    }

    /// <summary>
    /// Asks the user to select a Box in the viewport.
    /// </summary>
    /// <param name="box">If the result is Success, this parameter will be filled out.</param>
    /// <param name="mode">A particular "get box" mode, or <see cref="GetBoxMode.All"/>.</param>
    /// <param name="basePoint">Optional base point. Supply Point3d.Unset if you don't want to use this.</param>
    /// <param name="prompt1">Optional first prompt. Supply null to use the default prompt.</param>
    /// <param name="prompt2">Optional second prompt. Supply null to use the default prompt.</param>
    /// <param name="prompt3">Optional third prompt. Supply null to use the default prompt.</param>
    /// <returns>Commands.Result.Success if successful.</returns>
    public static Commands.Result GetBox(out Box box, GetBoxMode mode, Point3d basePoint, string prompt1, string prompt2, string prompt3)
    {
      Point3d[] corners = new Point3d[8];
      // 19 Feb 2010 S. Baer
      // On Win x64 builds the .NET framework appears to have problems if you don't initialize the array
      // before passing it off to unmanaged code.
      for (int i = 0; i < corners.Length; i++)
        corners[i] = new Point3d();
      Commands.Result rc = (Commands.Result)UnsafeNativeMethods.RHC_RhinoGetBox(corners, (int)mode, basePoint, prompt1, prompt2, prompt3);

      // David: This code is untested.
      box = new Box();

      if (rc == Commands.Result.Success)
      {
        Vector3d x = corners[1] - corners[0];
        Vector3d y = corners[3] - corners[0];
        Vector3d z = corners[4] - corners[0];

        // Create a singular box.
        if (x.IsZero && y.IsZero && z.IsZero)
        {
          box = new Box(new Plane(corners[0], new Vector3d(0, 0, 1)), new Interval(), new Interval(), new Interval());
          return rc;
        }

        // Create a linear box.
        if (x.IsZero && y.IsZero)
        {
          box = new Box(new Plane(corners[0], z), new Interval(), new Interval(), new Interval(0, z.Length));
          return rc;
        }

        // Boxes were getting inverted if the "height" pick was on the negative side of the base plane.
        Plane base_plane = new Plane(corners[0], x, y);
        Point3d c0, c1;
        base_plane.RemapToPlaneSpace(corners[0], out c0);
        base_plane.RemapToPlaneSpace(corners[6], out c1);

        Interval ix = new Interval(c0.X, c1.X); ix.MakeIncreasing();
        Interval iy = new Interval(c0.Y, c1.Y); iy.MakeIncreasing();
        Interval iz = new Interval(c0.Z, c1.Z); iz.MakeIncreasing();

        box = new Box(base_plane, ix, iy, iz);
      }

      return rc;
    }

    static Commands.Result GetGripsHelper(out DocObjects.GripObject[] grips, string prompt, bool singleGrip)
    {
      grips = null;
      using (Custom.GetObject go = new Custom.GetObject())
      {
        if (!string.IsNullOrEmpty(prompt))
          go.SetCommandPrompt(prompt);
        go.SubObjectSelect = false;
        go.GeometryFilter = DocObjects.ObjectType.Grip;
        go.GroupSelect = false;
        go.AcceptNothing(true);
        if (singleGrip)
          go.Get();
        else
          go.GetMultiple(1, 0);
        Commands.Result rc = go.CommandResult();
        if (Commands.Result.Success == rc)
        {
          DocObjects.ObjRef[] objrefs = go.Objects();
          if (null != objrefs && objrefs.Length > 0)
          {
            List<DocObjects.GripObject> griplist = new List<DocObjects.GripObject>();
            for (int i = 0; i < objrefs.Length; i++)
            {
              DocObjects.ObjRef ob = objrefs[i];
              if (null == ob)
                continue;
              DocObjects.GripObject grip = ob.Object() as DocObjects.GripObject;
              if (grip != null)
                griplist.Add(grip);
              ob.Dispose();
            }
            if (griplist.Count > 0)
              grips = griplist.ToArray();
          }
        }
        return rc;
      }
    }

    public static Commands.Result GetGrips(out DocObjects.GripObject[] grips, string prompt)
    {
      return GetGripsHelper(out grips, prompt, false);
    }

    public static Commands.Result GetGrip(out DocObjects.GripObject grip, string prompt)
    {
      grip = null;
      DocObjects.GripObject[] grips;
      Commands.Result rc = GetGripsHelper(out grips, prompt, true);
      if (grips != null && grips.Length > 0)
        grip = grips[0];
      return rc;
    }

    public static Commands.Result GetSpiral(out NurbsCurve spiral)
    {
      spiral = null;
      NurbsCurve nc = new NurbsCurve();
      IntPtr ptr_curve = nc.NonConstPointer();
      uint rc = UnsafeNativeMethods.RHC_RhinoGetSpiralHelix(ptr_curve, true);
      Commands.Result command_rc = (Commands.Result)rc;
      if (command_rc == Commands.Result.Success)
        spiral = nc;
      return command_rc;
    }

    public static Commands.Result GetHelix(out NurbsCurve helix)
    {
      helix = null;
      NurbsCurve nc = new NurbsCurve();
      IntPtr ptr_curve = nc.NonConstPointer();
      uint rc = UnsafeNativeMethods.RHC_RhinoGetSpiralHelix(ptr_curve, false);
      Commands.Result command_rc = (Commands.Result)rc;
      if (command_rc == Commands.Result.Success)
        helix = nc;
      return command_rc;
    }

    public static Commands.Result GetLine(out Line line)
    {
      line = new Line();
      uint rc = UnsafeNativeMethods.RHC_RhinoGetLine(ref line);
      Commands.Result command_rc = (Commands.Result)rc;
      if (command_rc != Commands.Result.Success)
        line = Line.Unset;
      return command_rc;
    }

    public static Commands.Result GetPolyline(out Polyline polyline)
    {
      using (var points = new SimpleArrayPoint3d())
      {
        IntPtr ptr_points = points.NonConstPointer();
        uint rc = UnsafeNativeMethods.RHC_RhinoGetPolyline(IntPtr.Zero, ptr_points);
        polyline = new Polyline(points.ToArray());
        return (Commands.Result)rc;
      }
    }

    public static Commands.Result GetArc(out Arc arc)
    {
      arc = new Arc();
      uint rc = UnsafeNativeMethods.RHC_RhinoGetArc(ref arc);
      Commands.Result command_rc = (Commands.Result)rc;
      if (command_rc != Commands.Result.Success)
        arc = new Arc();
      return command_rc;
    }

    public static Commands.Result GetCircle(out Circle circle)
    {
      circle = new Circle();
      uint rc = UnsafeNativeMethods.RHC_RhinoGetCircle(ref circle);
      Commands.Result command_rc = (Commands.Result)rc;
      if (command_rc != Commands.Result.Success)
        circle = new Circle();
      return command_rc;
    }

    /// <example>
    /// <code source='examples\vbnet\ex_addlineardimension.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addlineardimension.cs' lang='cs'/>
    /// <code source='examples\py\ex_addlineardimension.py' lang='py'/>
    /// </example>
    public static Commands.Result GetLinearDimension(out LinearDimension dimension)
    {
      uint command_rc = 0;
      IntPtr ptr_dimension = UnsafeNativeMethods.RHC_RhinoGetDimLinear(ref command_rc);
      dimension = GeometryBase.CreateGeometryHelper(ptr_dimension, null) as LinearDimension;
      return (Commands.Result)command_rc;
    }

    /// <summary>
    /// Allows the user to interactively pick a viewport.
    /// </summary>
    /// <param name="commandPrompt">The command prompt during the request.</param>
    /// <param name="view">The view that the user picked.
    /// <para>If the operation is successful, then this out parameter is assigned the correct view during this call.</para>
    /// </param>
    /// <returns>The result based on user choice.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addnamedview.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addnamedview.cs' lang='cs'/>
    /// <code source='examples\py\ex_addnamedview.py' lang='py'/>
    /// </example>
    public static Commands.Result GetView(string commandPrompt, out RhinoView view)
    {
      uint command_rc = 0;
      IntPtr ptr_view = UnsafeNativeMethods.RHC_RhinoGetView(commandPrompt, ref command_rc);
      view = RhinoView.FromIntPtr(ptr_view);
      return (Commands.Result)command_rc;
    }

    /// <summary>
    /// Allows user to interactively pick an angle
    /// </summary>
    /// <param name="commandPrompt">if null, a default prompt will be displayed</param>
    /// <param name="basePoint"></param>
    /// <param name="referencePoint"></param>
    /// <param name="defaultAngleRadians"></param>
    /// <param name="angleRadians"></param>
    /// <returns></returns>
    public static Commands.Result GetAngle(string commandPrompt, Point3d basePoint, Point3d referencePoint,
                                           double defaultAngleRadians, out double angleRadians)
    {
      angleRadians = 0;
      int rc = UnsafeNativeMethods.RHC_RhinoGetAngle(commandPrompt, basePoint, referencePoint, defaultAngleRadians,
                                            ref angleRadians);
      GetResult get_rc = (GetResult)rc;
      if (get_rc == GetResult.Angle)
        return Commands.Result.Success;
      if (get_rc == GetResult.ExitRhino)
        return Commands.Result.ExitRhino;
      if (get_rc == GetResult.Cancel)
        return Commands.Result.Cancel;
      if (get_rc == GetResult.Nothing)
        return Commands.Result.Nothing;
      return Commands.Result.Failure;
    }

    /// <example>
    /// <code source='examples\vbnet\ex_extractthumbnail.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_extractthumbnail.cs' lang='cs'/>
    /// <code source='examples\py\ex_extractthumbnail.py' lang='py'/>
    /// </example>
    public static string GetFileName(Custom.GetFileNameMode mode, string defaultName, string title, System.Windows.Forms.IWin32Window parent)
    {
      using (var sh = new StringHolder())
      {
        IntPtr ptr_string = sh.NonConstPointer();
        IntPtr pParent = parent != null ? parent.Handle : IntPtr.Zero;
        UnsafeNativeMethods.CRhinoGetFileDialog_Get((int)mode, defaultName, title, pParent, ptr_string);
        return sh.ToString();
      }
    }

    public static string GetFileNameScripted(Custom.GetFileNameMode mode, string defaultName)
    {
      using (var sh = new StringHolder())
      {
        IntPtr ptr_string = sh.NonConstPointer();
        UnsafeNativeMethods.CRhinoGetFileDialog_Get2((int)mode, defaultName, ptr_string);
        return sh.ToString();
      }
    }
    #endregion
  }

  /// <summary>
  /// Possible results from GetObject.Get(), GetPoint.Get(), etc...
  /// </summary>
  [CLSCompliant(false)]
  public enum GetResult : uint
  {
    NoResult = 0,
    ///<summary>User wants to cancel current command.</summary>
    Cancel = 1,
    ///<summary>User pressed enter - typically used to accept defaults.</summary>
    Nothing = 2,
    ///<summary>User specified an option - call Option() to get option index.</summary>
    Option = 3,
    ///<summary>User entered a real number - call Number() to get value.</summary>
    Number = 4,
    ///<summary>User entered a color - call Color() to get value.</summary>
    Color = 5,
    ///<summary>User pressed undo.</summary>
    Undo = 6,
    ///<summary>User clicked and missed.</summary>
    Miss = 7,
    ///<summary>User picked 3d point - call Point() to get 3d point.</summary>
    Point = 8,
    ///<summary>
    ///User picked 2d window point in CRhinoGetPoint::Get2dPoint()
    ///call ON_2dPoint() to get the point and View() to get the view.
    ///</summary>
    Point2d = 9,
    ///<summary>
    ///User picked a 2d line in CRhinoGetPoint::Get2dLine() call Line2d()
    ///to get the line and View() to get the view.
    ///</summary>
    Line2d = 10,
    ///<summary>
    ///User picked a 2d rectangle in CRhinoGetPoint::Get2dRectangle() call
    ///Rectangle2d() to get the rectangle and View() to get the view.
    ///</summary>
    Rectangle2d = 11,
    ///<summary>User selected an object - call Object() to get object.</summary>
    Object = 12,
    ///<summary>User typed a string - call String() to get the string.</summary>
    String = 13,
    ///<summary>
    ///A custom message was posted to the RhinoGet
    ///</summary>
    CustomMessage = 14,
    ///<summary>
    ///The getter waited for the amount of time specifed in RhinoGet::SetWaitDuration()
    ///and then gave up.
    ///</summary>
    Timeout = 15,
    ///<summary>call CRhinoGetCircle::GetCircle() to get the circle.</summary>
    Circle = 16,
    ///<summary>call CRhinoGetPlane::GetPlane() to get the plane.</summary>
    Plane = 17,
    ///<summary>call CRhinoGetCylinder::GetCylinder() to get the cylinder.</summary>
    Cylinder = 18,
    ///<summary>call CRhinoGetSphere::GetSphere() to get the sphere.</summary>
    Sphere = 19,
    ///<summary>call CRhinoGetAngle::Angle() to get the angle in radians (CRhinoGetAngle() returns this for typed number, too).</summary>
    Angle = 20,
    ///<summary>call CRhinoGetDistance::Distance() to get the distance value.</summary>
    Distance = 21,
    ///<summary>call CRhinoGetDirection::Direction() to get the direction vector.</summary>
    Direction = 22,
    ///<summary>call CRhinoGetFrame::Frame() to get the frame that was picked.</summary>
    Frame = 23,

    User1 = 0xFFFFFFFF,
    User2 = 0xFFFFFFFE,
    User3 = 0xFFFFFFFD,
    User4 = 0xFFFFFFFC,
    User5 = 0xFFFFFFFB,

    /// <summary>Stop now, do not cleaup, just return ASAP.</summary>
    ExitRhino = 0x0FFFFFFF
  }
}

// dumping other get classes in here for the moment
// I'm not convinced these need to get wrapped. Most
// can probably just be static functions on RhinoGet
/*
namespace Rhino.Input
{
  public class GetAngle { }
  public class GetAnnotationLeader { }
  public class GetArc { }
  public class GetCircle { }
  public class GetCone { }
  public class GetCurve { }
  public class GetCylinder { }
  public class GetDimAngular { }
  public class GetDimLinear { }
  public class GetDimOrdinate { }
  public class GetDimRadial { }
  public class GetDirection { }
  public class GetDistance { }
  public class GetEllipse { }
  public class GetFrame { }
  public class GetLine { }
  public class GetParabola { }
  public class GetPlane { }
  public class GetPolygon { }
  public class GetRectangle { }
  public class GetRevolve { }
  public class GetSphere { }
  public class GetSpiral { }
  public class GetSubcurve { }
  public class GetText { }
  public class GetTorus { }
  public class GetView { }
  public class GetXform { }
}
*/
namespace Rhino.Input.Custom
{
  /// <summary>
  /// Base class for GetObject, GetPoint, GetSphere, etc.
  /// 
  /// You will never directly create a GetBaseClass but you will use its member
  /// functions after calling GetObject.Gets(), GetPoint.Get(), and so on.
  /// 
  /// Provides tools to set command prompt, set command options, and specify
  /// if the "get" can optionally accept numbers, nothing (pressing enter),
  /// and undo.
  /// </summary>
  public abstract class GetBaseClass : IDisposable
  {
    #region constructor
    private IntPtr m_ptr; // CRhinoGet*
    bool m_is_const;
    internal IntPtr NonConstPointer()
    {
      if (m_is_const)
        return IntPtr.Zero;
      return m_ptr;
    }
    internal IntPtr ConstPointer() { return m_ptr; }

    internal void Construct(IntPtr ptr)
    {
      m_is_const = false;
      m_ptr = ptr;
    }

    internal void ConstConstruct(IntPtr ptr)
    {
      m_is_const = true;
      m_ptr = ptr;
      GC.SuppressFinalize(this);
    }

    ~GetBaseClass()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_ptr && !m_is_const)
      {
        UnsafeNativeMethods.CRhinoGet_Delete(m_ptr);
      }
      m_ptr = IntPtr.Zero;
      if (m_option != null)
      {
        m_option.m_ptr = IntPtr.Zero;
      }
    }
    #endregion

    const int idxCommandPrompt = 0;
    const int idxCommandPromptDefault = 1;
    const int idxDefaultString = 2;
    /// <summary>
    /// Sets prompt message that appears in the command prompt window.
    /// </summary>
    /// <param name="prompt">command prompt message.</param>
    /// <example>
    /// <code source='examples\vbnet\ex_addline.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addline.cs' lang='cs'/>
    /// <code source='examples\py\ex_addline.py' lang='py'/>
    /// </example>
    public void SetCommandPrompt(string prompt)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.CRhinoGet_SetString(ptr, prompt, idxCommandPrompt);
    }

    /// <summary>
    /// Sets message that describes what default value will be used if the user presses enter.
    /// This description appears in angle brackets &lt;> in the command prompt window. You do
    /// not need to provide a default value description unless you explicity enable AcceptNothing.
    /// </summary>
    /// <param name="defaultValue">description of default value.</param>
    /// <example>
    /// ON_3dPoint default_center = new ON_3dPoint(2,3,4);
    /// GetPoint gp = new GetPoint();
    /// gp.SetCommandPrompt( "Center point" );
    /// gp.SetCommandPromptDefault( "(2,3,4)" );
    /// gp.AcceptNothing(true);
    /// gp.GetPoint();
    /// if ( gp.Result() == GetResult.Nothing )
    ///   point = default_center;
    /// </example>
    /// <remarks>
    /// If you have a simple default point, number, or string, it is easier to use SetDefaultPoint,
    /// SetDefaultNumber, or SetDefaultString. SetCommandPromptDefault and AcceptNothing can be used
    /// for providing more advanced UI behavior.
    /// </remarks>
    public void SetCommandPromptDefault(string defaultValue)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.CRhinoGet_SetString(ptr, defaultValue, idxCommandPromptDefault);
    }

    /// <summary>
    /// Sets a point as default value that will be returned if the user presses the ENTER key during the get.
    /// </summary>
    /// <param name="point">value for default point.</param>
    /// <remarks>
    /// Calling SetDefaultPoint will automatically handle setting the command prompt default and reacting to
    /// the user pressing ENTER.  If the user presses enter to accept the default point, GetResult.Point is
    /// returned and RhinoGet.GotDefault() will return true. Calling SetDefaultPoint will clear any previous
    /// calls to SetDefaultString or SetDefaultNumber.
    /// </remarks>
    public void SetDefaultPoint(Point3d point)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.CRhinoGet_SetDefaultPoint(ptr, point);
    }

    /// <summary>
    /// Sets a number as default value that will be returned if the user presses ENTER key during the get.
    /// </summary>
    /// <param name="defaultNumber">value for default number.</param>
    /// <remarks>
    /// Calling SetDefaultNumber will automatically handle setting the command prompt default and
    /// reacting to the user pressing ENTER. If the user presses ENTER to accept the default number,
    /// GetResult.Number is returned and RhinoGet.GotDefault() will return true. Calling
    /// SetDefaultNumber will clear any previous calls to SetDefaultString or SetDefaultPoint.
    /// </remarks>
    public void SetDefaultNumber(double defaultNumber)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.CRhinoGet_SetDefaultNumber(ptr, defaultNumber);
    }

    /// <summary>
    /// Sets a number as default value that will be returned if the user presses ENTER key during the get.
    /// </summary>
    /// <param name="defaultValue">value for default number.</param>
    /// <remarks>
    /// Calling SetDefaultInteger will automatically handle setting the command prompt default and
    /// reacting to the user pressing ENTER. If the user presses ENTER to accept the default integer,
    /// GetResult.Number is returned and CRhinoGet.GotDefault() will return true. Calling
    /// SetDefaultNumber will clear any previous calls to SetDefaultString or SetDefaultPoint.
    /// </remarks>
    public void SetDefaultInteger(int defaultValue)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.CRhinoGet_SetDefaultInteger(ptr, defaultValue);
    }

    /// <summary>
    /// Sets a string as default value that will be returned
    /// if the user presses ENTER key during the get.
    /// </summary>
    /// <param name="defaultValue">value for default string.</param>
    /// <remarks>
    /// Calling SetDefaultString will automatically handle setting the command prompt
    /// default and reacting to the user pressing ENTER. If the user presses ENTER to
    /// accept the default string, GetResult.String is returned and RhinoGet.GotDefault()
    /// will return true. Calling SetDefaultString will clear any previous calls to
    /// SetDefaultNumber or SetDefaultPoint.
    /// </remarks>
    /// <example>
    /// <code source='examples\vbnet\ex_addlayer.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addlayer.cs' lang='cs'/>
    /// <code source='examples\py\ex_addlayer.py' lang='py'/>
    /// </example>
    public void SetDefaultString(string defaultValue)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.CRhinoGet_SetString(ptr, defaultValue, idxDefaultString);
    }

    /// <summary>
    /// Sets a color as default value that will be returned if the user presses ENTER key during the get.
    /// </summary>
    /// <param name="defaultColor">value for default color.</param>
    /// <remarks>
    /// Calling SetDefaultColor will automatically handle setting the command prompt default and
    /// reacting to the user pressing ENTER.  If the user presses ENTER to accept the default color,
    /// GetResult.Color is returned and RhinoGet.GotDefault() will return true. Calling
    /// SetDefaultColor will clear any previous calls to SetDefaultString or SetDefaultPoint.
    /// </remarks>
    public void SetDefaultColor(Rhino.Drawing.Color defaultColor)
    {
      IntPtr ptr = NonConstPointer();
      int argb = defaultColor.ToArgb();
      UnsafeNativeMethods.CRhinoGet_SetDefaultColor(ptr, argb);
    }

    /// <summary>
    /// Sets the wait duration (in milliseconds) of the getter. If the duration passes without 
    /// the user making a decision, the GetResult.Timeout code is returned.
    /// </summary>
    /// <param name="milliseconds">Number of milliseconds to wait.</param>
    public void SetWaitDuration(int milliseconds)
    {
      if (milliseconds <= 0) { return; }

      IntPtr ptr = NonConstPointer();
      double seconds = milliseconds * 0.001;
      UnsafeNativeMethods.CRhinoGet_SetWaitDuration(ptr, seconds);
    }

    /// <summary>
    /// Clears any defaults set using SetDefaultPoint, SetDefaultNumber, SetDefaultString, or SetCommandPromptDefault.
    /// </summary>
    public void ClearDefault()
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.CRhinoGet_ClearDefault(ptr);
    }

    /// <summary>
    /// Returns true if user pressed Enter to accept a default point, number,
    /// or string set using SetDefaultPoint, SetDefaultNumber, or SetDefaultString.
    /// </summary>
    /// <returns>true if the result if the default point, number or string set. Otherwise, false.</returns>
    public bool GotDefault()
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CRhinoGet_GotDefault(ptr);
    }


    #region commandlineoptions
    /// <summary>
    /// Adds a command line option.
    /// </summary>
    /// <param name="englishOption">
    /// Must only consist of letters and numbers (no characters list periods, spaces, or dashes)
    /// </param>
    /// <returns>
    /// option index value (&gt;0) or 0 if option cannot be added.
    /// </returns>
    /// <example>
    /// <code source='examples\vbnet\ex_advanceddisplay.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_advanceddisplay.cs' lang='cs'/>
    /// <code source='examples\py\ex_advanceddisplay.py' lang='py'/>
    /// </example>
    public int AddOption(string englishOption) //, bool hidden)
    {
      return AddOption(englishOption, null);
    }

    /// <summary>
    /// Adds a command line option.
    /// </summary>
    /// <param name="englishOption">
    /// Must only consist of letters and numbers (no characters list periods, spaces, or dashes)
    /// </param>
    /// <param name="englishOptionValue">The option value in English, visualized after an equality sign.</param>
    /// <returns>
    /// Option index value (&gt;0) or 0 if option cannot be added.
    /// </returns>
    public int AddOption(string englishOption, string englishOptionValue) //, bool hidden)
    {
      const bool hidden = false;
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.CRhinoGet_AddCommandOption(ptr, englishOption, englishOptionValue, hidden);
    }

    /// <summary>
    /// Adds a command line option.
    /// </summary>
    /// <param name="optionName">
    /// Must only consist of letters and numbers (no characters list periods, spaces, or dashes)
    /// </param>
    /// <returns>
    /// option index value (&gt;0) or 0 if option cannot be added.
    /// </returns>
    public int AddOption(Rhino.UI.LocalizeStringPair optionName)
    {
      return AddOption(optionName, null);
    }

    /// <summary>
    /// Adds a command line option.
    /// </summary>
    /// <param name="optionName">
    /// Must only consist of letters and numbers (no characters list periods, spaces, or dashes)
    /// </param>
    /// <param name="optionValue">The localized value visualized after an equality sign.</param>
    /// <returns>
    /// option index value (&gt;0) or 0 if option cannot be added.
    /// </returns>
    public int AddOption(Rhino.UI.LocalizeStringPair optionName, Rhino.UI.LocalizeStringPair optionValue)
    {
      string val_english = null;
      string val_local = null;
      if (optionValue != null)
      {
        val_english = optionValue.English;
        val_local = optionValue.Local;
      }
      const bool hidden = false;
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.CRhinoGet_AddCommandOptionLoc(ptr, optionName.English, optionName.Local, val_english, val_local, hidden);
    }

    // 9 Feb 2010 S. Baer
    // Commenting out until we find a need for this version of the function
    ///// <summary>
    ///// Adds a simple command line option with a number as a value
    ///// </summary>
    ///// <param name="englishOption">The option value in English, visualized after an equality sign.</param>
    ///// <param name="numberValue">current value.</param>
    ///// <returns>
    ///// option index value (&gt;0) or 0 if option cannot be added
    ///// </returns>
    //public int AddOptionDouble(string englishOption, double numberValue)
    //{
    //  IntPtr ptr = NonConstPointer();
    //  return UnsafeNativeMethods.CRhinoGet_AddCommandOption2(ptr, englishOption, numberValue);
    //}

    /// <summary>
    /// Adds a command line option to get numbers and automatically save the value.
    /// </summary>
    /// <param name="englishName">
    /// Must only consist of letters and numbers (no characters list periods, spaces, or dashes)
    /// </param>
    /// <param name="numberValue">Current value.</param>
    /// <param name="prompt">
    /// option prompt shown if the user selects this option.  If null or empty, then the
    /// option name is used as the get number prompt.
    /// </param>
    /// <returns>
    /// option index value (&gt;0) or 0 if option cannot be added.
    /// </returns>
    public int AddOptionDouble(string englishName, ref Rhino.Input.Custom.OptionDouble numberValue, string prompt)
    {
      return AddOptionDouble(new Rhino.UI.LocalizeStringPair(englishName, englishName), ref numberValue, prompt);
    }

    /// <summary>
    /// Adds a command line option to get numbers and automatically saves the value.
    /// </summary>
    /// <param name="optionName">
    /// Must only consist of letters and numbers (no characters list periods, spaces, or dashes)
    /// </param>
    /// <param name="numberValue">The current number value.</param>
    /// <param name="prompt">
    /// option prompt shown if the user selects this option.  If null or empty, then the
    /// option name is used as the get number prompt.
    /// </param>
    /// <returns>
    /// option index value (&gt;0) or 0 if option cannot be added.
    /// </returns>
    public int AddOptionDouble(Rhino.UI.LocalizeStringPair optionName, ref Rhino.Input.Custom.OptionDouble numberValue, string prompt)
    {
      IntPtr ptr = NonConstPointer();
      IntPtr pHolder = numberValue.OptionHolderPointer;
      int rc = UnsafeNativeMethods.CRhinoGet_AddCommandOption3Loc(ptr, optionName.English, optionName.Local, pHolder, numberValue.m_lowerLimit, numberValue.m_upperLimit, prompt);
      return rc;
    }
    /// <summary>
    /// Adds a command line option to get numbers and automatically save the value.
    /// </summary>
    /// <param name="englishName">
    /// Must only consist of letters and numbers (no characters list periods, spaces, or dashes)
    /// </param>
    /// <param name="numberValue">The current number value.</param>
    /// <returns>
    /// Option index value (&gt;0) or 0 if option cannot be added.
    /// </returns>
    /// <example>
    /// <code source='examples\vbnet\ex_commandlineoptions.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_commandlineoptions.cs' lang='cs'/>
    /// <code source='examples\py\ex_commandlineoptions.py' lang='py'/>
    /// </example>
    public int AddOptionDouble(string englishName, ref Rhino.Input.Custom.OptionDouble numberValue)
    {
      return AddOptionDouble(englishName, ref numberValue, null);
    }

    /// <summary>
    /// Adds a command line option to get numbers and automatically save the value.
    /// </summary>
    /// <param name="optionName">
    /// Must only consist of letters and numbers (no characters list periods, spaces, or dashes)
    /// </param>
    /// <param name="numberValue">The current number value.</param>
    /// <returns>
    /// option index value (&gt;0) or 0 if option cannot be added.
    /// </returns>
    /// <example>
    /// <code source='examples\vbnet\ex_commandlineoptions.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_commandlineoptions.cs' lang='cs'/>
    /// <code source='examples\py\ex_commandlineoptions.py' lang='py'/>
    /// </example>
    public int AddOptionDouble(Rhino.UI.LocalizeStringPair optionName, ref Rhino.Input.Custom.OptionDouble numberValue)
    {
      return AddOptionDouble(optionName, ref numberValue, null);
    }

    /// <summary>
    /// Adds a command line option to get integers and automatically save the value.
    /// </summary>
    /// <param name="englishName">
    /// Must only consist of letters and numbers (no characters list periods, spaces, or dashes)
    /// </param>
    /// <param name="intValue">The current integer value.</param>
    /// <param name="prompt">
    /// option prompt shown if the user selects this option.  If null or empty, then the
    /// option name is used as the get number prompt.
    /// </param>
    /// <returns>
    /// option index value (&gt;0) or 0 if option cannot be added.
    /// </returns>
    public int AddOptionInteger(string englishName, ref Rhino.Input.Custom.OptionInteger intValue, string prompt)
    {
      return AddOptionInteger(new Rhino.UI.LocalizeStringPair(englishName, englishName), ref intValue, prompt);
    }

    /// <summary>
    /// Adds a command line option to get integers and automatically save the value.
    /// </summary>
    /// <param name="optionName">
    /// Must only consist of letters and numbers (no characters list periods, spaces, or dashes)
    /// </param>
    /// <param name="intValue">The current integer value.</param>
    /// <param name="prompt">
    /// option prompt shown if the user selects this option.  If null or empty, then the
    /// option name is used as the get number prompt.
    /// </param>
    /// <returns>
    /// option index value (&gt;0) or 0 if option cannot be added.
    /// </returns>
    public int AddOptionInteger(Rhino.UI.LocalizeStringPair optionName, ref Rhino.Input.Custom.OptionInteger intValue, string prompt)
    {
      IntPtr ptr = NonConstPointer();
      IntPtr pOption = intValue.OptionHolderPointer;
      int rc = UnsafeNativeMethods.CRhinoGet_AddCommandOption4Loc(ptr, optionName.English, optionName.Local, pOption, intValue.m_lowerLimit, intValue.m_upperLimit, prompt);
      return rc;
    }

    /// <summary>
    /// Adds a command line option to get integers and automatically save the value.
    /// </summary>
    /// <param name="englishName">
    /// Must only consist of letters and numbers (no characters list periods, spaces, or dashes)
    /// </param>
    /// <param name="intValue">The current integer value.</param>
    /// <returns>
    /// option index value (&gt;0) or 0 if option cannot be added.
    /// </returns>
    /// <example>
    /// <code source='examples\vbnet\ex_commandlineoptions.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_commandlineoptions.cs' lang='cs'/>
    /// <code source='examples\py\ex_commandlineoptions.py' lang='py'/>
    /// </example>
    public int AddOptionInteger(string englishName, ref Rhino.Input.Custom.OptionInteger intValue)
    {
      return AddOptionInteger(englishName, ref intValue, null);
    }

    /// <summary>
    /// Adds a command line option to get integers and automatically save the value.
    /// </summary>
    /// <param name="optionName">
    /// Must only consist of letters and numbers (no characters list periods, spaces, or dashes)
    /// </param>
    /// <param name="intValue">The current integer value.</param>
    /// <returns>
    /// option index value (&gt;0) or 0 if option cannot be added.
    /// </returns>
    /// <example>
    /// <code source='examples\vbnet\ex_commandlineoptions.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_commandlineoptions.cs' lang='cs'/>
    /// <code source='examples\py\ex_commandlineoptions.py' lang='py'/>
    /// </example>
    public int AddOptionInteger(Rhino.UI.LocalizeStringPair optionName, ref Rhino.Input.Custom.OptionInteger intValue)
    {
      return AddOptionInteger(optionName, ref intValue, null);
    }

    /// <summary>
    /// Add a command line option to get colors and automatically save the value.
    /// </summary>
    /// <param name="optionName">option description.</param>
    /// <param name="colorValue">The current color value.</param>
    /// <param name="prompt">option prompt shown if the user selects this option</param>
    /// <returns>option index value (&gt;0) or 0 if option cannot be added.</returns>
    public int AddOptionColor(Rhino.UI.LocalizeStringPair optionName, ref Rhino.Input.Custom.OptionColor colorValue, string prompt)
    {
      IntPtr ptr = NonConstPointer();
      IntPtr pOption = colorValue.OptionHolderPointer;
      int rc = UnsafeNativeMethods.CRhinoGet_AddCommandOption5Loc(ptr, optionName.English, optionName.Local, pOption, prompt);
      return rc;
    }

    /// <summary>
    /// Add a command line option to get colors and automatically save the value.
    /// </summary>
    /// <param name="optionName">option description</param>
    /// <param name="colorValue">The current color value.</param>
    /// <returns>option index value (&gt;0) or 0 if option cannot be added.</returns>
    public int AddOptionColor(Rhino.UI.LocalizeStringPair optionName, ref Rhino.Input.Custom.OptionColor colorValue)
    {
      return AddOptionColor(optionName, ref colorValue, null);
    }

    /// <summary>
    /// Add a command line option to get colors and automatically save the value.
    /// </summary>
    /// <param name="englishName">option description</param>
    /// <param name="colorValue">The current color value.</param>
    /// <param name="prompt">The command prompt will show this during picking.</param>
    /// <returns>option index value (&gt;0) or 0 if option cannot be added.</returns>
    public int AddOptionColor(string englishName, ref Rhino.Input.Custom.OptionColor colorValue, string prompt)
    {
      return AddOptionColor(new UI.LocalizeStringPair(englishName, englishName), ref colorValue, prompt);
    }

    /// <summary>
    /// Add a command line option to get colors and automatically save the value.
    /// </summary>
    /// <param name="englishName">option description</param>
    /// <param name="colorValue">The current color value.</param>
    /// <returns>option index value (&gt;0) or 0 if option cannot be added.</returns>
    public int AddOptionColor(string englishName, ref Rhino.Input.Custom.OptionColor colorValue)
    {
      return AddOptionColor(new UI.LocalizeStringPair(englishName, englishName), ref colorValue, null);
    }

    /// <summary>
    /// Adds a command line option to toggle a setting.
    /// </summary>
    /// <param name="englishName">
    /// Must only consist of letters and numbers (no characters list periods, spaces, or dashes)
    /// </param>
    /// <param name="toggleValue">The current toggle value.</param>
    /// <returns>option index value (&gt;0) or 0 if option cannot be added.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_commandlineoptions.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_commandlineoptions.cs' lang='cs'/>
    /// <code source='examples\py\ex_commandlineoptions.py' lang='py'/>
    /// </example>
    public int AddOptionToggle(string englishName, ref Rhino.Input.Custom.OptionToggle toggleValue)
    {
      return AddOptionToggle(new UI.LocalizeStringPair(englishName, englishName), ref toggleValue);
    }

    /// <summary>
    /// Adds a command line option to toggle a setting.
    /// </summary>
    /// <param name="optionName">
    /// Must only consist of letters and numbers (no characters list periods, spaces, or dashes)
    /// </param>
    /// <param name="toggleValue">The current toggle value.</param>
    /// <returns>option index value (&gt;0) or 0 if option cannot be added.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_commandlineoptions.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_commandlineoptions.cs' lang='cs'/>
    /// <code source='examples\py\ex_commandlineoptions.py' lang='py'/>
    /// </example>
    public int AddOptionToggle(Rhino.UI.LocalizeStringPair optionName, ref Rhino.Input.Custom.OptionToggle toggleValue)
    {
      IntPtr ptr = NonConstPointer();
      IntPtr pToggle = toggleValue.OptionHolderPointer;
      int rc = UnsafeNativeMethods.CRhinoGet_AddCommandOptionToggleLoc(ptr, pToggle, optionName.English, optionName.Local,
        toggleValue.m_offValue.English, toggleValue.m_offValue.Local, toggleValue.m_onValue.English, toggleValue.m_onValue.Local);
      return rc;
    }

    /// <summary>
    /// Adds a command line list option.
    /// </summary>
    /// <param name="englishOptionName">
    /// Must only consist of letters and numbers (no characters list periods, spaces, or dashes)
    /// </param>
    /// <param name="listValues">The string values.</param>
    /// <param name="listCurrentIndex">Zero based index of current option.</param>
    /// <returns>option index value (&gt;0) or 0 if option cannot be added.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_objectdisplaymode.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_objectdisplaymode.cs' lang='cs'/>
    /// <code source='examples\py\ex_objectdisplaymode.py' lang='py'/>
    /// </example>
    public int AddOptionList(string englishOptionName, System.Collections.Generic.IEnumerable<string> listValues, int listCurrentIndex)
    {
      IntPtr pStrings = UnsafeNativeMethods.ON_StringArray_New();
      foreach( string s in listValues)
        UnsafeNativeMethods.ON_StringArray_Append(pStrings, s);

      IntPtr pThis = NonConstPointer();
      int rc = UnsafeNativeMethods.CRhinoGet_AddCommandOptionListLoc(pThis, englishOptionName, englishOptionName, pStrings, pStrings, listCurrentIndex);
      UnsafeNativeMethods.ON_StringArray_Delete(pStrings);
      return rc;
    }

    /// <summary>
    /// Adds a command line list option.
    /// </summary>
    /// <param name="optionName">
    /// Must only consist of letters and numbers (no characters list periods, spaces, or dashes)
    /// </param>
    /// <param name="listValues">The string values.</param>
    /// <param name="listCurrentIndex">Zero based index of current option.</param>
    /// <returns>option index value (&gt;0) or 0 if option cannot be added.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_objectdisplaymode.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_objectdisplaymode.cs' lang='cs'/>
    /// <code source='examples\py\ex_objectdisplaymode.py' lang='py'/>
    /// </example>
    public int AddOptionList(Rhino.UI.LocalizeStringPair optionName, System.Collections.Generic.IEnumerable<Rhino.UI.LocalizeStringPair> listValues, int listCurrentIndex)
    {
      IntPtr pStringsEnglish = UnsafeNativeMethods.ON_StringArray_New();
      IntPtr pStringsLocal = UnsafeNativeMethods.ON_StringArray_New();
      foreach (Rhino.UI.LocalizeStringPair s in listValues)
      {
        UnsafeNativeMethods.ON_StringArray_Append(pStringsEnglish, s.English);
        UnsafeNativeMethods.ON_StringArray_Append(pStringsLocal, s.Local);
      }

      IntPtr pThis = NonConstPointer();
      int rc = UnsafeNativeMethods.CRhinoGet_AddCommandOptionListLoc(pThis, optionName.English, optionName.Local, pStringsEnglish, pStringsLocal, listCurrentIndex);
      UnsafeNativeMethods.ON_StringArray_Delete(pStringsEnglish);
      UnsafeNativeMethods.ON_StringArray_Delete(pStringsLocal);
      return rc;
    }

    /// <summary>
    /// Add a choice of enum values as list option 
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <param name="englishOptionName">The name of the option</param>
    /// <param name="defaultValue">The default value</param>
    /// <exception cref="ArgumentException">Gets thrown if defaultValue provided is not an enum type.</exception>
    /// <returns>Option index</returns>
    [CLSCompliant(false)]
    public int AddOptionEnumList<T>(string englishOptionName, T defaultValue) 
        where T : struct, IConvertible
    {
        Type enumType = typeof(T);
        if (!enumType.GetTypeInfo().IsEnum) throw new ArgumentException("!typeof(T).GetTypeInfo().IsEnum");
        
        string[] names = Enum.GetNames(enumType);
        int index = Array.IndexOf(names, defaultValue.ToString(CultureInfo.InvariantCulture));
        return AddOptionList(englishOptionName, names, index);
    }

    /// <summary>
    /// Add a list of enum values as option list. Use enumSelection[go.Option.CurrentListOptionIndex] to retrieve selection.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="englishOptionName"></param>
    /// <param name="enumSelection"></param>
    /// <param name="listCurrentIndex"></param>
    /// <returns></returns>
    [CLSCompliant(false)]
    public int AddOptionEnumSelectionList<T>(string englishOptionName, IEnumerable<T> enumSelection, int listCurrentIndex)
        where T : struct, IConvertible
    {
        if (!typeof(T).GetTypeInfo().IsEnum) throw new ArgumentException("!typeof(T).GetTypeInfo().IsEnum");
        if (null == enumSelection) throw new ArgumentNullException("enumSelection");

        List<String> names = new List<String>();
        foreach(T e in enumSelection)
            names.Add(e.ToString(CultureInfo.InvariantCulture));

        return AddOptionList(englishOptionName, names, listCurrentIndex);
    }

    /// <summary>
    /// Returns the selected enum value. Use this in combination with <see cref="AddOptionEnumList{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="ArgumentException">Gets thrown if type T is not an enum type.</exception>
    /// <exception cref="IndexOutOfRangeException">If 0 &gt;= CurrentListOptionIndex or CurrentListOptionIndex &gt; N where N is the number of enum values.</exception>
    /// <returns></returns>
    [CLSCompliant(false)]
    public T GetSelectedEnumValue<T>()
        where T : struct, IConvertible
    {
        Type enumType = typeof(T);
        if (!enumType.GetTypeInfo().IsEnum) throw new ArgumentException("!enumType.GetTypeInfo().IsEnum");

        Array values = Enum.GetValues(enumType);
        T[] tValues = new T[values.Length];
        for (int i = 0; i < values.Length; ++i)
            tValues[i] = (T)values.GetValue(i);

        return GetSelectedEnumValueFromSelectionList<T>(tValues);
    }


    /// <summary>
    /// Returns the selected enum value by looking at the list of values from which to select.
    /// Use this in combination with <see cref="AddOptionEnumSelectionList{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="selectionList"> </param>
    /// <exception cref="ArgumentException">Gets thrown if type T is not an enum type.</exception>
    /// <exception cref="IndexOutOfRangeException">If 0 &gt;= CurrentListOptionIndex or CurrentListOptionIndex &gt; N where N is the number of enum values.</exception>
    /// <returns></returns>
    [CLSCompliant(false)]
    public T GetSelectedEnumValueFromSelectionList<T>(IEnumerable<T> selectionList)
        where T : struct, IConvertible
    {
        Type enumType = typeof(T);
        if (!enumType.GetTypeInfo().IsEnum) throw new ArgumentException("!enumType.GetTypeInfo().IsEnum");

        List<T> values = new List<T>(selectionList);

        int index = Option().CurrentListOptionIndex;
        if (index >= values.Count || index < 0)
        {
            String errMsg = String.Format("GetSelectedEnumValue received incorrect index i [{0}]: i should be 0 <= i < N, where N is number of enum values in {1}. N = {2}",
                index, enumType.Name, values.Count);
            throw new IndexOutOfRangeException(errMsg);
        }
        return (T)values[index];
    }
    /// <summary>Clear all command options.</summary>
    /// <example>
    /// <code source='examples\vbnet\ex_arraybydistance.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_arraybydistance.cs' lang='cs'/>
    /// <code source='examples\py\ex_arraybydistance.py' lang='py'/>
    /// </example>
    public void ClearCommandOptions()
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.CRhinoGet_ClearCommandOptions(ptr);
    }
    #endregion


    const int idxEnableTransparentCommands = 0;
    const int idxAcceptNothing = 1;
    const int idxAcceptUndo = 2;
    const int idxAcceptPoint = 3;
    const int idxAcceptColor = 4;
    const int idxAcceptString = 5;
    void SetBool(int which, bool b)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.CRhinoGet_SetBool(ptr, which, b);
    }

    /// <summary>
    /// Control the availability of transparent commands during the get.
    /// </summary>
    /// <param name="enable">
    /// If true, then transparent commands can be run during the get.
    /// If false, then transparent commands cannot be run during the get.
    /// </param>
    /// <remarks>
    /// Some Rhino commands are "transparent" and can be run inside of other
    /// commands.  Examples of transparent commands include the view
    /// manipulation commands like ZoomExtents, Top, etc., and the selection
    /// commands like SelAll, SelPoint, etc.
    /// By default transparent commands can be run during any get. If you
    /// want to disable this feature, then call EnableTransparentCommands(false)
    /// before calling GetString, GetPoint, GetObject, etc.
    /// </remarks>
    public void EnableTransparentCommands(bool enable)
    {
      SetBool(idxEnableTransparentCommands, enable);
    }


    /// <summary>
    /// If you want to allow the user to be able to press enter in order to
    /// skip selecting a something in GetPoint.Get(), GetObject::GetObjects(),
    /// etc., then call AcceptNothing( true ) beforehand.
    /// </summary>
    /// <param name="enable">true if user is able to press enter in order to skip selecting.</param>
    /// <example>
    /// <code source='examples\vbnet\ex_addlayer.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addlayer.cs' lang='cs'/>
    /// <code source='examples\py\ex_addlayer.py' lang='py'/>
    /// </example>
    public void AcceptNothing(bool enable)
    {
      SetBool(idxAcceptNothing, enable);
    }

    /// <summary>
    /// If you want to allow the user to have an 'undo' option in GetPoint.Get(),
    /// GetObject.GetObjects(), etc., then call AcceptUndo(true) beforehand.
    /// </summary>
    /// <param name="enable">true if user is able to choose the 'Undo' option.</param>
    public void AcceptUndo(bool enable)
    {
      SetBool(idxAcceptUndo, enable);
    }

    /// <summary>
    /// If you want to allow the user to be able to type in a number during GetPoint.Get(),
    /// GetObject::GetObjects(), etc., then call AcceptNumber() beforehand.
    /// If the user chooses to type in a number, then the result code GetResult.Number is
    /// returned and you can use RhinoGet.Number() to get the value of the number. If you
    /// are using GetPoint and you want "0" to return (0,0,0) instead of the number zero, 
    /// then set acceptZero = false.
    /// </summary>
    /// <param name="enable">true if user is able to type a number.</param>
    /// <param name="acceptZero">
    /// If you are using GetPoint and you want "0" to return (0,0,0) instead of the number zero, 
    /// then set acceptZero = false.
    /// </param>
    public void AcceptNumber(bool enable, bool acceptZero)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.CRhinoGet_AcceptNumber(ptr, enable, acceptZero);
    }

    /// <summary>
    /// If you want to allow the user to be able to type in a point then call AcceptPoint(true)
    /// before calling GetPoint()/GetObject(). If the user chooses to type in a number, then
    /// the result code GetResult.Point is returned and you can use RhinoGet.Point()
    /// to get the value of the point.
    /// </summary>
    /// <param name="enable">true if user is able to type in a point.</param>
    public void AcceptPoint(bool enable)
    {
      SetBool(idxAcceptPoint, enable);
    }

    /// <summary>
    /// If you want to allow the user to be able to type in a color r,g,b or name
    /// during GetPoint.Get(), GetObject::GetObjects(), etc., then call AcceptColor(true)
    /// before calling GetPoint()/GetObject(). If the user chooses to type in a color,
    /// then the result code GetResult.Color is returned and you can use RhinoGet.Color()
    /// to get the value of the color.  If the get accepts points, then the user will not
    /// be able to type in r,g,b colors but will be able to type color names.
    /// </summary>
    /// <param name="enable">true if user is able to type a color.</param>
    public void AcceptColor(bool enable)
    {
      SetBool(idxAcceptColor, enable);
    }

    /// <summary>
    /// If you want to allow the user to be able to type in a string during GetPoint.Get(),
    /// GetObject::GetObjects(), etc., then call AcceptString(true) before calling
    /// GetPoint()/GetObject(). If the user chooses to type in a string, then the result code
    /// GetResult.String is returned and you can use RhinoGet.String() to get the value of the string.
    /// </summary>
    /// <param name="enable">true if user is able to type a string.</param>
    public void AcceptString(bool enable)
    {
      SetBool(idxAcceptString, enable);
    }

    const uint custom_message_id = 0xC001;
    public void AcceptCustomMessage(bool enable)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoGet_AcceptCustomMessage(pThis, custom_message_id, enable);
    }

    public static void PostCustomMessage( object messageData )
    {
      m_message_data = messageData;
      UnsafeNativeMethods.CRhinoGet_PostCustomMessage(custom_message_id);
    }
    static object m_message_data;
    object m_local_message_data;

    public object CustomMessage()
    {
      IntPtr pConstThis = ConstPointer();
      uint id = UnsafeNativeMethods.CRhinoGet_WindowsMessage(pConstThis);
      if( id!=custom_message_id )
        return null;

      if( m_message_data!=null )
      {
        m_local_message_data = m_message_data;
        m_message_data = null;
      }
      return m_local_message_data;
    }

    /// <summary>Returns result of the Get*() call.</summary>
    /// <returns>The result of the last Get*() call.</returns>
    [CLSCompliant(false)]
    public GetResult Result()
    {
      IntPtr ptr = ConstPointer();
      uint rc = UnsafeNativeMethods.CRhinoGet_Result(ptr);
      return (GetResult)rc;
    }

    /// <summary>
    /// Helper method for getting command result value from getter results.
    /// </summary>
    /// <returns>The converted command result.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addbackgroundbitmap.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addbackgroundbitmap.cs' lang='cs'/>
    /// <code source='examples\py\ex_addbackgroundbitmap.py' lang='py'/>
    /// </example>
    public Commands.Result CommandResult()
    {
      IntPtr ptr = ConstPointer();
      uint rc = UnsafeNativeMethods.CRhinoGet_CommandResult(ptr);
      return (Rhino.Commands.Result)rc;
    }

    // clear the native pointer on m_option when this class
    // is collected. Devs should never hang on to an option
    // class, but just in case they do we can track down the
    // exception instead of calling into unmanaged memory that
    // is no longer valid
    internal Rhino.Input.Custom.CommandLineOption m_option;// = null; initialized to null by runtime
    public Rhino.Input.Custom.CommandLineOption Option()
    {
      IntPtr pConstThis = ConstPointer();
      IntPtr pOption = UnsafeNativeMethods.CRhinoGet_Option(pConstThis);
      if (pOption == IntPtr.Zero)
        return null;

      if (m_option == null)
        m_option = new Rhino.Input.Custom.CommandLineOption();
      m_option.m_ptr = pOption;
      return m_option;
    }

    public int OptionIndex()
    {
      Rhino.Input.Custom.CommandLineOption op = Option();
      if (op == null)
        return -1;
      return op.Index;
    }

    /// <summary>
    /// Gets a number if GetPoint.Get(), GetObject.GetObjects(), etc., returns GetResult.Number.
    /// </summary>
    /// <returns>The number chosen by the user.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addbackgroundbitmap.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addbackgroundbitmap.cs' lang='cs'/>
    /// <code source='examples\py\ex_addbackgroundbitmap.py' lang='py'/>
    /// </example>
    public double Number()
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CRhinoGet_Number(ptr);
    }

    /// <summary>
    /// Gets a string if GetPoint.Get(), GetObject.GetObjects(), etc., returns GetResult.String.
    /// </summary>
    /// <returns>The string chosen by the user.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addlayer.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addlayer.cs' lang='cs'/>
    /// <code source='examples\py\ex_addlayer.py' lang='py'/>
    /// </example>
    public string StringResult()
    {
      IntPtr ptr = ConstPointer();
      IntPtr rc = UnsafeNativeMethods.CRhinoGet_String(ptr);
      if (IntPtr.Zero == rc)
        return String.Empty;
      return Marshal.PtrToStringUni(rc);
    }

    /// <summary>
    /// Gets a point if Get*() returns GetResult.Point.
    /// </summary>
    /// <returns>The point chosen by the user.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addline.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addline.cs' lang='cs'/>
    /// <code source='examples\py\ex_addline.py' lang='py'/>
    /// </example>
    public Point3d Point()
    {
      Point3d rc = new Point3d();
      IntPtr ptr = ConstPointer();
      UnsafeNativeMethods.CRhinoGet_Point(ptr, ref rc);
      return rc;
    }

    /// <summary>
    /// Gets a direction if Get*() returns GetResult.Point (Set by some digitizers, but in general it's (0,0,0).
    /// </summary>
    /// <returns>The vector chosen by the user.</returns>
    public Vector3d Vector()
    {
      IntPtr ptr = ConstPointer();
      Vector3d rc = new Vector3d();
      UnsafeNativeMethods.CRhinoGet_Vector(ptr, ref rc);
      return rc;
    }

    /// <summary>Gets a color if Get*() returns GetResult.Color.</summary>
    /// <returns>The color chosen by the user.</returns>
    public Rhino.Drawing.Color Color()
    {
      IntPtr ptr = ConstPointer();
      uint abgr = UnsafeNativeMethods.CRhinoGet_Color(ptr);
      return Rhino.Runtime.Interop.ColorFromWin32((int)abgr);
    }

    /// <summary>
    /// Gets a view the user clicked in during GetPoint.Get(), GetObject.GetObjects(), etc.
    /// </summary>
    /// <returns>The view chosen by the user.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addbackgroundbitmap.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addbackgroundbitmap.cs' lang='cs'/>
    /// <code source='examples\py\ex_addbackgroundbitmap.py' lang='py'/>
    /// </example>
    public RhinoView View()
    {
      IntPtr ptr = ConstPointer();
      IntPtr pView = UnsafeNativeMethods.CRhinoGet_View(ptr);
      return RhinoView.FromIntPtr(pView);
    }

    const int idxPickRectangle = 0;
    const int idxRectangle2d = 1;
    /// <summary>
    /// If the get was a GetObjects() and the mouse was used to select the objects,
    /// then the returned rect has left &lt; right and top &lt; bottom. This rect
    /// is the Windows GDI screen coordinates of the picking rectangle.
    /// RhinoViewport.GetPickXform( pick_rect, pick_xform )
    /// will calculate the picking transformation that was used.
    /// In all other cases, left=right=top=bottom=0;
    /// </summary>
    /// <returns>The picking rectangle; or 0 in the specified cases.</returns>
    public Rhino.Drawing.Rectangle PickRectangle()
    {
      int[] lrtb = new int[4];
      IntPtr ptr = ConstPointer();
      UnsafeNativeMethods.CRhinoGet_GetRectangle(ptr, ref lrtb[0], idxPickRectangle);
      return Rhino.Drawing.Rectangle.FromLTRB(lrtb[0], lrtb[1], lrtb[2], lrtb[3]);
    }

    /// <summary>
    /// Returns location in view of point in selected in GetPoint::Get() or GetPoint::Get2dPoint().
    /// (0,0) = upper left corner of window.
    /// </summary>
    /// <returns>The location.</returns>
    public Rhino.Drawing.Point Point2d()
    {
      IntPtr ptr = ConstPointer();
      int x = 0;
      int y = 0;
      UnsafeNativeMethods.CRhinoGet_Point2d(ptr, ref x, ref y);
      return new Rhino.Drawing.Point(x, y);
    }

    //[skipping]
    //ON_3dPoint  WorldPoint1() const;
    //ON_3dPoint  WorldPoint2() const;

    /// <summary>
    /// Returns the location in the view of the 2d rectangle selected in GetPoint::Get2dRectangle().
    /// rect.left &lt; rect.right and rect.top &lt; rect.bottom
    /// (0,0) = upper left corner of window.
    /// </summary>
    /// <returns>The rectangle.</returns>
    public Rhino.Drawing.Rectangle Rectangle2d()
    {
      int[] lrtb = new int[4];
      IntPtr ptr = ConstPointer();
      UnsafeNativeMethods.CRhinoGet_GetRectangle(ptr, ref lrtb[0], idxRectangle2d);
      return Rhino.Drawing.Rectangle.FromLTRB(lrtb[0], lrtb[1], lrtb[2], lrtb[3]);
    }

    /// <summary>
    /// Returns two points defining the location in the view window of the 2d line selected
    /// in GetPoint::Get2dLine().
    /// <para>(0,0) = upper left corner of window.</para>
    /// </summary>
    /// <returns>An array with two 2D points.</returns>
    public Rhino.Drawing.Point[] Line2d()
    {
      int x0 = 0, y0 = 0, x1 = 0, y1 = 0;
      IntPtr ptr = ConstPointer();
      UnsafeNativeMethods.CRhinoGet_Line2d(ptr, ref x0, ref y0, ref x1, ref y1);
      Rhino.Drawing.Point[] rc = new Rhino.Drawing.Point[2];
      rc[0] = new Rhino.Drawing.Point(x0, y0);
      rc[1] = new Rhino.Drawing.Point(x1, y1);
      return rc;
    }
  }

  // 29 Jan 2010 - S. Baer
  // Had to change Option class to CommandLineOption. Option is a reserved
  // keyword in some languages (http://msdn.microsoft.com/en-us/library/ms182248.aspx)
  public sealed class CommandLineOption
  {
    #region statics
    /// <summary>
    /// Test a string to see if it can be used as an option name in any of the RhinoGet::AddCommandOption...() functions.
    /// </summary>
    /// <param name="optionName">The string to be tested.</param>
    /// <returns>true if string can be used as an option name.</returns>
    public static bool IsValidOptionName(string optionName)
    {
      if (String.IsNullOrEmpty(optionName))
        return false;
      return UnsafeNativeMethods.CRhinoGet_IsValidName(optionName, true);
    }

    /// <summary>
    /// Test a string to see if it can be used as an option value in RhinoGet::AddCommandOption,
    /// RhinoGet::AddCommandOptionToggle, or RhinoGet::AddCommandOptionList.
    /// </summary>
    /// <param name="optionValue">The string to be tested.</param>
    /// <returns>true if string can be used as an option value.</returns>
    public static bool IsValidOptionValueName(string optionValue)
    {
      if (String.IsNullOrEmpty(optionValue))
        return false;
      return UnsafeNativeMethods.CRhinoGet_IsValidName(optionValue, false);
    }
    #endregion

    internal IntPtr m_ptr; // const CRhinoCommandOption*
    internal CommandLineOption() { }

    public int Index
    {
      get
      {
        return UnsafeNativeMethods.CRhinoCommandOption_OptionIndex(m_ptr, true);
      }
    }

    /// <example>
    /// <code source='examples\vbnet\ex_commandlineoptions.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_commandlineoptions.cs' lang='cs'/>
    /// <code source='examples\py\ex_commandlineoptions.py' lang='py'/>
    /// </example>
    public int CurrentListOptionIndex
    {
      get
      {
        return UnsafeNativeMethods.CRhinoCommandOption_OptionIndex(m_ptr, false);
      }
    }

    public string EnglishName
    {
      get
      {
        IntPtr pName = UnsafeNativeMethods.CRhinoCommandOption_EnglishName(m_ptr);
        if (IntPtr.Zero == pName)
          return String.Empty;
        return System.Runtime.InteropServices.Marshal.PtrToStringUni(pName);
      }
    }
  }

  public class OptionToggle : IDisposable
  {
    internal IntPtr m_pOptionHolder = IntPtr.Zero;
    readonly bool m_initialValue;
    internal readonly Rhino.UI.LocalizeStringPair m_offValue;
    internal readonly Rhino.UI.LocalizeStringPair m_onValue;

    /// <example>
    /// <code source='examples\vbnet\ex_commandlineoptions.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_commandlineoptions.cs' lang='cs'/>
    /// <code source='examples\py\ex_commandlineoptions.py' lang='py'/>
    /// </example>
    public OptionToggle(bool initialValue, string offValue, string onValue)
    {
      m_initialValue = initialValue;
      m_offValue = new UI.LocalizeStringPair(offValue, offValue);
      m_onValue = new UI.LocalizeStringPair(onValue, onValue);
    }

    public OptionToggle(bool initialValue, Rhino.UI.LocalizeStringPair offValue, Rhino.UI.LocalizeStringPair onValue)
    {
      m_initialValue = initialValue;
      m_offValue = offValue;
      m_onValue = onValue;
    }

    ~OptionToggle()
    {
      Dispose(false);
    }
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
      if (m_pOptionHolder != IntPtr.Zero)
      {
        UnsafeNativeMethods.CRhCommonOptionHolder_Delete(m_pOptionHolder);
        m_pOptionHolder = IntPtr.Zero;
      }
    }

    /// <example>
    /// <code source='examples\vbnet\ex_commandlineoptions.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_commandlineoptions.cs' lang='cs'/>
    /// <code source='examples\py\ex_commandlineoptions.py' lang='py'/>
    /// </example>
    public bool CurrentValue
    {
      get
      {
        bool rc = m_initialValue;
        if (IntPtr.Zero != m_pOptionHolder)
        {
          rc = UnsafeNativeMethods.CRhCommonOptionHolder_Bool(m_pOptionHolder);
        }
        return rc;
      }
      set
      {
        IntPtr pThis = OptionHolderPointer;
        UnsafeNativeMethods.CRhCommonOptionHolder_SetBool(pThis, value);
      }
    }

    public bool InitialValue
    {
      get { return m_initialValue; }
    }

    internal IntPtr OptionHolderPointer
    {
      get
      {
        if (IntPtr.Zero == m_pOptionHolder)
          m_pOptionHolder = UnsafeNativeMethods.CRhCommonOptionHolder_New3(m_initialValue);
        return m_pOptionHolder;
      }
    }
  }

  public class OptionDouble : IDisposable
  {
    internal IntPtr m_pOptionHolder = IntPtr.Zero;
    readonly double m_initialValue;
    internal readonly double m_lowerLimit;
    internal readonly double m_upperLimit;

    public OptionDouble(double initialValue)
    {
      m_initialValue = initialValue;
      m_lowerLimit = Rhino.RhinoMath.UnsetValue;
      m_upperLimit = Rhino.RhinoMath.UnsetValue;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="OptionDouble"/> class with lower and upper limits.
    /// </summary>
    /// <param name="initialValue">The initial number .</param>
    /// <param name="lowerLimit">The minimum value.</param>
    /// <param name="upperLimit">The maximum value.</param>
    /// <example>
    /// <code source='examples\vbnet\ex_commandlineoptions.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_commandlineoptions.cs' lang='cs'/>
    /// <code source='examples\py\ex_commandlineoptions.py' lang='py'/>
    /// </example>
    public OptionDouble(double initialValue, double lowerLimit, double upperLimit)
    {
      m_initialValue = initialValue;
      m_lowerLimit = lowerLimit;
      m_upperLimit = upperLimit;
    }
    /// <summary>
    /// Initializes a new instance of the double option class.
    /// </summary>
    /// <param name="initialValue">The initial number .</param>
    /// <param name="setLowerLimit">
    /// If true, limit sets the lower limit and upper limit is undefined.
    /// If false, limit sets the upper limit and lower limit is undefined.
    /// </param>
    /// <param name="limit">The lower limit if setLowerLimit is true; otherwise, the upper limit.</param>
    public OptionDouble(double initialValue, bool setLowerLimit, double limit)
    {
      m_initialValue = initialValue;
      if (setLowerLimit)
      {
        m_lowerLimit = limit;
        m_upperLimit = Rhino.RhinoMath.UnsetValue;
      }
      else
      {
        m_lowerLimit = Rhino.RhinoMath.UnsetValue;
        m_upperLimit = limit;
      }
    }

    ~OptionDouble()
    {
      Dispose(false);
    }
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
      if (m_pOptionHolder != IntPtr.Zero)
      {
        UnsafeNativeMethods.CRhCommonOptionHolder_Delete(m_pOptionHolder);
        m_pOptionHolder = IntPtr.Zero;
      }
    }


    /// <example>
    /// <code source='examples\vbnet\ex_commandlineoptions.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_commandlineoptions.cs' lang='cs'/>
    /// <code source='examples\py\ex_commandlineoptions.py' lang='py'/>
    /// </example>
    public double CurrentValue
    {
      get
      {
        double rc = m_initialValue;
        if (IntPtr.Zero != m_pOptionHolder)
        {
          rc = UnsafeNativeMethods.CRhCommonOptionHolder_Double(m_pOptionHolder);
        }
        return rc;
      }
      set
      {
        IntPtr pThis = OptionHolderPointer;
        UnsafeNativeMethods.CRhCommonOptionHolder_SetDouble(pThis, value);
      }
    }

    public double InitialValue
    {
      get { return m_initialValue; }
    }

    internal IntPtr OptionHolderPointer
    {
      get
      {
        if (IntPtr.Zero == m_pOptionHolder)
          m_pOptionHolder = UnsafeNativeMethods.CRhCommonOptionHolder_New(m_initialValue);
        return m_pOptionHolder;
      }
    }
  }

  public class OptionInteger : IDisposable
  {
    internal IntPtr m_pOptionHolder = IntPtr.Zero;
    readonly int m_initialValue;
    internal readonly double m_lowerLimit;
    internal readonly double m_upperLimit;

    public OptionInteger(int initialValue)
    {
      m_initialValue = initialValue;
      m_lowerLimit = Rhino.RhinoMath.UnsetValue;
      m_upperLimit = Rhino.RhinoMath.UnsetValue;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="OptionInteger"/> class wiht both lower and upper limits.
    /// </summary>
    /// <param name="initialValue">The initial value.</param>
    /// <param name="lowerLimit">The minimum value.</param>
    /// <param name="upperLimit">The maximum value.</param>
    /// <example>
    /// <code source='examples\vbnet\ex_commandlineoptions.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_commandlineoptions.cs' lang='cs'/>
    /// <code source='examples\py\ex_commandlineoptions.py' lang='py'/>
    /// </example>
    public OptionInteger(int initialValue, int lowerLimit, int upperLimit)
    {
      m_initialValue = initialValue;
      m_lowerLimit = lowerLimit;
      m_upperLimit = upperLimit;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="OptionInteger"/> class.
    /// </summary>
    /// <param name="initialValue">The initial value.</param>
    /// <param name="setLowerLimit">
    /// If true, limit sets the lower limit and upper limit is undefined
    /// If false, limit sets the upper limit and lower limit is undefined.
    /// </param>
    /// <param name="limit">If <c>setLowerLimit</c> is true, then <c>limit</c> is the minimum value.
    /// Otherwise, it is the maximum.</param>
    public OptionInteger(int initialValue, bool setLowerLimit, int limit)
    {
      m_initialValue = initialValue;
      if (setLowerLimit)
      {
        m_lowerLimit = limit;
        m_upperLimit = Rhino.RhinoMath.UnsetValue;
      }
      else
      {
        m_lowerLimit = Rhino.RhinoMath.UnsetValue;
        m_upperLimit = limit;
      }
    }

    ~OptionInteger()
    {
      Dispose(false);
    }
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
      if (m_pOptionHolder != IntPtr.Zero)
      {
        UnsafeNativeMethods.CRhCommonOptionHolder_Delete(m_pOptionHolder);
        m_pOptionHolder = IntPtr.Zero;
      }
    }

    /// <example>
    /// <code source='examples\vbnet\ex_commandlineoptions.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_commandlineoptions.cs' lang='cs'/>
    /// <code source='examples\py\ex_commandlineoptions.py' lang='py'/>
    /// </example>
    public int CurrentValue
    {
      get
      {
        int rc = m_initialValue;
        if (IntPtr.Zero != m_pOptionHolder)
        {
          rc = UnsafeNativeMethods.CRhCommonOptionHolder_Integer(m_pOptionHolder);
        }
        return rc;
      }
      set
      {
        IntPtr pThis = OptionHolderPointer;
        UnsafeNativeMethods.CRhCommonOptionHolder_SetInt(pThis, value);
      }
    }

    public int InitialValue
    {
      get { return m_initialValue; }
    }

    internal IntPtr OptionHolderPointer
    {
      get
      {
        if (IntPtr.Zero == m_pOptionHolder)
          m_pOptionHolder = UnsafeNativeMethods.CRhCommonOptionHolder_New2(m_initialValue);
        return m_pOptionHolder;
      }
    }
  }

  public class OptionColor : IDisposable
  {
    internal IntPtr m_pOptionHolder = IntPtr.Zero;
    Rhino.Drawing.Color m_initialValue;

    public OptionColor(Rhino.Drawing.Color initialValue)
    {
      m_initialValue = initialValue;
    }

    ~OptionColor()
    {
      Dispose(false);
    }
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
      if (m_pOptionHolder != IntPtr.Zero)
      {
        UnsafeNativeMethods.CRhCommonOptionHolder_Delete(m_pOptionHolder);
        m_pOptionHolder = IntPtr.Zero;
      }
    }

    public Rhino.Drawing.Color CurrentValue
    {
      get
      {
        var rc = m_initialValue;
        if (IntPtr.Zero != m_pOptionHolder)
        {
          int abgr = UnsafeNativeMethods.CRhCommonOptionHolder_Color(m_pOptionHolder);
          rc = Rhino.Runtime.Interop.ColorFromWin32(abgr);
        }
        return rc;
      }
      set
      {
        IntPtr pThis = OptionHolderPointer;
        int argb = value.ToArgb();
        UnsafeNativeMethods.CRhCommonOptionHolder_SetColor(pThis, argb);
      }
    }

    public Rhino.Drawing.Color InitialValue
    {
      get { return m_initialValue; }
    }

    internal IntPtr OptionHolderPointer
    {
      get
      {
        if (IntPtr.Zero == m_pOptionHolder)
        {
          int argb = m_initialValue.ToArgb();
          m_pOptionHolder = UnsafeNativeMethods.CRhCommonOptionHolder_New4(argb);
        }
        return m_pOptionHolder;
      }
    }
  }
}
#endif