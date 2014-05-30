using System;
using System.Reflection;

namespace Rhino.Runtime
{
  /// <summary>
  /// Contains static methods to marshal objects between RhinoCommon and legacy Rhino_DotNet or C++.
  /// </summary>
  public static class Interop
  {
    //eventually this could go away if we just do all of the abgr->argb conversions in C++
      internal static Rhino.Drawing.Color ColorFromWin32(int abgr)
      {
#if MONO_BUILD
      return Rhino.Drawing.Color.FromArgb(0xFF, (abgr & 0xFF), ((abgr >> 8) & 0xFF), ((abgr >> 16) & 0xFF));
#else
          return Rhino.Drawing.ColorTranslator.FromWin32(abgr);
#endif
      }    

#if RHINO_SDK
    /// <summary>
    /// Gets the C++ CRhinoDoc* for a given RhinoCommon RhinoDoc class.
    /// </summary>
    /// <param name="doc">A document.</param>
    /// <returns>A pointer value.</returns>
    public static IntPtr NativeRhinoDocPointer(RhinoDoc doc)
    {
      if (doc == null)
        return IntPtr.Zero;
      return UnsafeNativeMethods.CRhinoDoc_GetFromId(doc.m_docId);
    }
#endif

    /// <summary>
    /// Returns the underlying const ON_Geometry* for a RhinoCommon class. You should only
    /// be interested in using this function if you are writing C++ code.
    /// </summary>
    /// <param name="geometry">A geometry object. This can be null and in such a case <see cref="IntPtr.Zero"/> is returned.</param>
    /// <returns>A pointer to the const geometry.</returns>
    public static IntPtr NativeGeometryConstPointer(Geometry.GeometryBase geometry)
    {
      IntPtr rc = IntPtr.Zero;
      if (geometry != null)
        rc = geometry.ConstPointer();
      return rc;
    }

    /// <summary>
    /// Returns the underlying non-const ON_Geometry* for a RhinoCommon class. You should
    /// only be interested in using this function if you are writing C++ code.
    /// </summary>
    /// <param name="geometry">A geometry object. This can be null and in such a case <see cref="IntPtr.Zero"/> is returned.</param>
    /// <returns>A pointer to the non-const geometry.</returns>
    public static IntPtr NativeGeometryNonConstPointer(Geometry.GeometryBase geometry)
    {
      IntPtr rc = IntPtr.Zero;
      if (geometry != null)
        rc = geometry.NonConstPointer();
      return rc;
    }

    /// <summary>
    /// Get ON_Viewport* from a ViewportInfo instance
    /// </summary>
    /// <param name="viewport"></param>
    /// <returns></returns>
    public static IntPtr NativeNonConstPointer(DocObjects.ViewportInfo viewport)
    {
      return viewport.NonConstPointer();
    }

#if RHINO_SDK
    /// <summary>
    /// Returns the underlying const CRhinoObject* for a RhinoCommon class. You should only
    /// be interested in using this function if you are writing C++ code.
    /// </summary>
    /// <param name="rhinoObject">A Rhino object.</param>
    /// <returns>A pointer to the Rhino const object.</returns>
    public static IntPtr RhinoObjectConstPointer(Rhino.DocObjects.RhinoObject rhinoObject)
    {
      IntPtr rc = IntPtr.Zero;
      if (rhinoObject != null)
        rc = rhinoObject.ConstPointer();
      return rc;
    }

    /// <summary>
    /// Constructs a RhinoCommon Rhino object from an unmanaged C++ RhinoObject pointer.
    /// </summary>
    /// <param name="pRhinoObject">The original pointer.</param>
    /// <returns>A new Rhino object, or null if the pointer was invalid or <see cref="IntPtr.Zero"/>.</returns>
    public static Rhino.DocObjects.RhinoObject RhinoObjectFromPointer(IntPtr pRhinoObject)
    {
      return Rhino.DocObjects.RhinoObject.CreateRhinoObjectHelper(pRhinoObject);
    }
#endif

    /// <summary>
    /// Constructs a RhinoCommon Geometry class from a given ON_Geomety*. The ON_Geometry*
    /// must be declared on the heap and its lifetime becomes controlled by RhinoCommon.
    /// </summary>
    /// <param name="pGeometry">ON_Geometry*</param>
    /// <returns>The appropriate geometry class in RhinoCommon on success.</returns>
    public static Geometry.GeometryBase CreateFromNativePointer(IntPtr pGeometry)
    {
      return Geometry.GeometryBase.CreateGeometryHelper(pGeometry, null);
    }

    /// <summary>
    /// Attempts to copy the contents of a RMA.OpenNURBS.OnArc to a Rhino.Geometry.Arc.
    /// </summary>
    /// <param name="source">A source OnArc.</param>
    /// <param name="destination">A destination arc.</param>
    /// <returns>true if the operation succeeded; false otherwise.</returns>
    public static bool TryCopyFromOnArc(object source, out Rhino.Geometry.Arc destination)
    {
      destination = new Rhino.Geometry.Arc();
      bool rc = false;
      IntPtr ptr = GetInternalPointer(source, "RMA.OpenNURBS.OnArc");
      if (IntPtr.Zero != ptr)
      {
        UnsafeNativeMethods.ON_Arc_Copy(ptr, ref destination, true);
        rc = true;
      }
      return rc;
    }

    /// <summary>
    /// Attempts to copy the contents of a Rhino.Geometry.Arc to a RMA.OpenNURBS.OnArc.
    /// </summary>
    /// <param name="source">A source arc.</param>
    /// <param name="destination">A destination OnArc.</param>
    /// <returns>true if the operation succeeded; false otherwise.</returns>
    public static bool TryCopyToOnArc(Rhino.Geometry.Arc source, object destination)
    {
      bool rc = false;
      IntPtr ptr = GetInternalPointer(destination, "RMA.OpenNURBS.OnArc");
      if (IntPtr.Zero != ptr)
      {
        UnsafeNativeMethods.ON_Arc_Copy(ptr, ref source, false);
        rc = true;
      }
      return rc;
    }

    static IntPtr GetInternalPointer(object rhinoDotNetObject, string name)
    {
      IntPtr rc = IntPtr.Zero;
      if (null != rhinoDotNetObject)
      {
        System.Type t = rhinoDotNetObject.GetType();
        if (t.FullName == name)
        {
          System.Reflection.PropertyInfo pi = t.GetRuntimeProperty("InternalPointer");
          rc = (IntPtr)pi.GetValue(rhinoDotNetObject, null);
        }
      }
      return rc;
    }

    static Rhino.Geometry.GeometryBase CopyHelper(object source, string name)
    {
      IntPtr pGeometry = GetInternalPointer(source, name);
      IntPtr pNewGeometry = UnsafeNativeMethods.ON_Object_Duplicate(pGeometry);
      return Geometry.GeometryBase.CreateGeometryHelper(pNewGeometry, null);
    }

    /// <summary>
    /// Copies a Rhino_DotNet brep to a RhinoCommon brep class.
    /// </summary>
    /// <param name="source">
    /// RMA.OpenNURBS.IOnBrep or RMA.OpenNURBS.OnBrep.
    /// </param>
    /// <returns>
    /// RhinoCommon object on success. This will be an independent copy.
    /// </returns>
    public static Rhino.Geometry.Brep FromOnBrep(object source)
    {
      Rhino.Geometry.GeometryBase g = CopyHelper(source, "RMA.OpenNURBS.OnBrep");
      Rhino.Geometry.Brep rc = g as Rhino.Geometry.Brep;
      return rc;
    }

    /// <summary>
    /// Copies a Rhino_DotNet surface to a RhinoCommon Surface class.
    /// </summary>
    /// <param name="source">
    /// Any of the following in the RMA.OpenNURBS namespace are acceptable.
    /// IOnSurface, OnSurface, IOnPlaneSurface, OnPlaneSurface, IOnClippingPlaneSurface,
    /// OnClippingPlaneSurface, IOnNurbsSurface, OnNurbsSurfac, IOnRevSurface, OnRevSurface,
    /// IOnSumSurface, OnSumSurface.
    /// </param>
    /// <returns>
    /// RhinoCommon object on success. This will be an independent copy.
    /// </returns>
    public static Rhino.Geometry.Surface FromOnSurface(object source)
    {
      Rhino.Geometry.GeometryBase g = CopyHelper(source, "RMA.OpenNURBS.OnSurface");
      if (null == g)
        g = CopyHelper(source, "RMA.OpenNURBS.OnPlaneSurface");
      if (null == g)
        g = CopyHelper(source, "RMA.OpenNURBS.OnClippingPlaneSurface");
      if (null == g)
        g = CopyHelper(source, "RMA.OpenNURBS.OnNurbsSurface");
      if (null == g)
        g = CopyHelper(source, "RMA.OpenNURBS.OnRevSurface");
      if (null == g)
        g = CopyHelper(source, "RMA.OpenNURBS.OnSumSurface");
      Rhino.Geometry.Surface rc = g as Rhino.Geometry.Surface;
      return rc;
    }

    /// <summary>
    /// Copies a Rhino_DotNet mesh to a RhinoCommon mesh class.
    /// </summary>
    /// <param name="source">
    /// RMA.OpenNURBS.IOnMesh or RMA.OpenNURBS.OnMesh.
    /// </param>
    /// <returns>
    /// RhinoCommon object on success. This will be an independent copy.
    /// </returns>
    public static Rhino.Geometry.Mesh FromOnMesh(object source)
    {
      Rhino.Geometry.GeometryBase g = CopyHelper(source, "RMA.OpenNURBS.OnMesh");
      Rhino.Geometry.Mesh rc = g as Rhino.Geometry.Mesh;
      return rc;
    }

    /// <summary>
    /// Copies a Rhino_DotNet curve to a RhinoCommon curve class.
    /// </summary>
    /// <param name="source">
    /// RMA.OpenNURBS.IOnCurve or RMA.OpenNURBS.OnCurve.
    /// </param>
    /// <returns>
    /// RhinoCommon object on success. This will be an independent copy.
    /// </returns>
    public static Rhino.Geometry.Curve FromOnCurve(object source)
    {
      Rhino.Geometry.GeometryBase g = CopyHelper(source, "RMA.OpenNURBS.OnCurve");
      if (null == g)
        g = CopyHelper(source, "RMA.OpenNURBS.OnArcCurve");
      if (null == g)
        g = CopyHelper(source, "RMA.OpenNURBS.OnLineCurve");
      if (null == g)
        g = CopyHelper(source, "RMA.OpenNURBS.OnNurbsCurve");
      if (null == g)
        g = CopyHelper(source, "RMA.OpenNURBS.OnPolylineCurve");
      if (null == g)
        g = CopyHelper(source, "RMA.OpenNURBS.OnPolyCurve");
      Rhino.Geometry.Curve rc = g as Rhino.Geometry.Curve;
      return rc;
    }

    //static Type GetRhinoDotNetType(string name)
    //{
    //  System.Reflection.Assembly rhinoDotNet = HostUtils.GetRhinoDotNetAssembly();
    //  if (null == rhinoDotNet)
    //    return null;
    //  return rhinoDotNet.GetType(name);
    //}

    ///// <summary>
    ///// Constructs a Rhino_DotNet OnBrep that is a copy of a given brep.
    ///// </summary>
    ///// <param name="source">A source brep.</param>
    ///// <returns>
    ///// Rhino_DotNet object on success. This will be an independent copy.
    ///// </returns>
    //public static object ToOnBrep(Rhino.Geometry.Brep source)
    //{
    //  object rc = null;
    //  IntPtr pSource = source.ConstPointer();
    //  Type onBrepType = GetRhinoDotNetType("RMA.OpenNURBS.OnBrep");
    //  if (IntPtr.Zero != pSource && null != onBrepType)
    //  {
    //    System.Reflection.MethodInfo mi = onBrepType.GetRuntimeMethod("WrapNativePointer", new Type[] { typeof(IntPtr), typeof(bool), typeof(bool) });
    //    IntPtr pNewBrep = UnsafeNativeMethods.ON_Brep_New(pSource);
    //    rc = mi.Invoke(null, new object[] { pNewBrep, false, true });
    //  }
    //  return rc;
    //}

    ///// <summary>
    ///// Constructs a Rhino_DotNet OnSurface that is a copy of a given curve.
    ///// </summary>
    ///// <param name="source">A source brep.</param>
    ///// <returns>
    ///// Rhino_DotNet object on success. This will be an independent copy.
    ///// </returns>
    //public static object ToOnSurface(Rhino.Geometry.Surface source)
    //{
    //  object rc = null;
    //  IntPtr pSource = source.ConstPointer();
    //  Type onType = GetRhinoDotNetType("RMA.OpenNURBS.OnSurface");
    //  if (IntPtr.Zero != pSource && null != onType)
    //  {
    //    System.Reflection.MethodInfo mi = onType.GetRuntimeMethod("WrapNativePointer", new Type[] { typeof(IntPtr), typeof(bool), typeof(bool) });
    //    IntPtr pNewSurface = UnsafeNativeMethods.ON_Surface_DuplicateSurface(pSource);
    //    rc = mi.Invoke(null, new object[] { pNewSurface, false, true });
    //  }
    //  return rc;
    //}

    ///// <summary>
    ///// Constructs a Rhino_DotNet OnMesh that is a copy of a given mesh.
    ///// </summary>
    ///// <param name="source">A source brep.</param>
    ///// <returns>
    ///// Rhino_DotNet object on success. This will be an independent copy.
    ///// </returns>
    //public static object ToOnMesh(Rhino.Geometry.Mesh source)
    //{
    //  object rc = null;
    //  IntPtr pSource = source.ConstPointer();
    //  Type onType = GetRhinoDotNetType("RMA.OpenNURBS.OnMesh");
    //  if (IntPtr.Zero != pSource && null != onType)
    //  {
    //    System.Reflection.MethodInfo mi = onType.GetRuntimeMethod("WrapNativePointer", new Type[] { typeof(IntPtr), typeof(bool), typeof(bool) });
    //    IntPtr pNewMesh = UnsafeNativeMethods.ON_Mesh_New(pSource);
    //    rc = mi.Invoke(null, new object[] { pNewMesh, false, true });
    //  }
    //  return rc;
    //}

    ///// <summary>
    ///// Constructs a Rhino_DotNet OnCurve that is a copy of a given curve.
    ///// </summary>
    ///// <param name="source">A RhinoCommon source curve.</param>
    ///// <returns>
    ///// Rhino_DotNet object on success. This will be an independent copy.
    ///// </returns>
    //public static object ToOnCurve(Rhino.Geometry.Curve source)
    //{
    //  object rc = null;
    //  IntPtr pSource = source.ConstPointer();
    //  Type onType = GetRhinoDotNetType("RMA.OpenNURBS.OnCurve");
    //  if (IntPtr.Zero != pSource && null != onType)
    //  {
    //    System.Reflection.MethodInfo mi = onType.GetRuntimeMethod("WrapNativePointer", new Type[] { typeof(IntPtr), typeof(bool), typeof(bool) });
    //    IntPtr pNewCurve = UnsafeNativeMethods.ON_Curve_DuplicateCurve(pSource);
    //    rc = mi.Invoke(null, new object[] { pNewCurve, false, true });
    //  }
    //  return rc;
    //}

    ///// <summary>
    ///// Constructs a Rhino_DotNet OnXform from a given RhinoCommon Transform.
    ///// </summary>
    ///// <param name="source">A RhinoCommon source transform.</param>
    ///// <returns>
    ///// Rhino_DotNet object on success. This will be an independent copy.
    ///// </returns>
    //public static object ToOnXform(Rhino.Geometry.Transform source)
    //{
    //  object rc = null;
    //  Type onType = GetRhinoDotNetType("RMA.OpenNURBS.OnXform");
    //  if (null != onType)
    //  {
    //    double[] vals = new double[16];
    //    for( int row=0; row<4; row++ )
    //    {
    //      for (int column = 0; column < 4; column++)
    //      {
    //        vals[4 * row + column] = source[row, column];
    //      }
    //    }
    //    rc = System.Activator.CreateInstance(onType, new object[] { vals });
    //  }
    //  return rc;
    //}

#if RHINO_SDK
    /// <summary>
    /// Convert a Rhino.Display.Viewport to an RMA.Rhino.IRhinoViewport.
    /// </summary>
    /// <param name="source">A RhinoCommon viewport.</param>
    /// <returns>
    /// Rhino_DotNet IRhinoViewport object on success. This will be an independent copy.
    /// </returns>
    public static object ToIRhinoViewport(Rhino.Display.RhinoViewport source)
    {
      object rc = null;
      IntPtr pSource = source.ConstPointer();
      Type rhType = GetRhinoDotNetType("RMA.Rhino.MRhinoViewport");
      if (IntPtr.Zero != pSource && null != rhType)
      {
        System.Reflection.MethodInfo mi = rhType.GetRuntimeMethod("WrapNativePointer", new Type[] { typeof(IntPtr), typeof(bool), typeof(bool) });
        const bool isConst = true;
        const bool doDelete = false;
        rc = mi.Invoke(null, new object[] { pSource, isConst, doDelete });
      }
      return rc;
    }
#endif
    /*
        public static Rhino.Geometry.Curve TryCopyFromOnCurve(object source)
        {
          if (source != null)
          {
            try
            {
              Type base_type = Type.GetType("RMA.OpenNURBS.OnCurve");
              System.Type t = source.GetType();
              if (t.IsAssignableFrom(base_type))
              {
                System.Reflection.PropertyInfo pi = t.GetProperty("InternalPointer");
                IntPtr ptr = (IntPtr)pi.GetValue(source, null);
                Rhino.Geometry.Curve crv = Rhino.Geometry.Curve.CreateCurveHelper(ptr, null);
                crv.NonConstPointer();
                return crv;
              }
            }
            catch (Exception)
            {
            }
          }
          return null;
        }

        /// <summary>
        /// Do not hold on to the returned class outside the scope of your current function.
        /// </summary>
        /// <param name="source">-</param>
        /// <returns>-</returns>
        public static Rhino.Display.DisplayPipeline ConvertFromMRhinoDisplayPipeline(object source)
        {
          if (source != null)
          {
            try
            {
              Type base_type = Type.GetType("RMA.Rhino.MRhinoDisplayPipeline");
              System.Type t = source.GetType();
              if (t.IsAssignableFrom(base_type))
              {
                System.Reflection.PropertyInfo pi = t.GetProperty("InternalPointer");
                IntPtr ptr = (IntPtr)pi.GetValue(source, null);
                return new Rhino.Display.DisplayPipeline(ptr);
              }
            }
            catch (Exception)
            {
            }
          }
          return null;
        }
        */
#if RHINO_SDK
    /// <summary>
    /// Gets a C++ plug-in pointer for a given RhinoCommon plug-in.
    /// <para>This is a Rhino SDK function.</para>
    /// </summary>
    /// <param name="plugin">A plug-in.</param>
    /// <returns>A pointer.</returns>
    public static IntPtr PlugInPointer(Rhino.PlugIns.PlugIn plugin)
    {
      return null == plugin ? IntPtr.Zero : plugin.NonConstPointer();
    }
#endif
  }
}
