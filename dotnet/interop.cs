using System;

namespace Rhino.Runtime
{
  /// <summary>
  /// Use for moving object types between RhinoCommon and legacy Rhino_DotNet or C++
  /// </summary>
  public static class Interop
  {
    /// <summary>
    /// Return the underlying const ON_Geometry* for a RhinoCommon class
    /// </summary>
    /// <param name="geometry"></param>
    /// <returns></returns>
    public static IntPtr NativeGeometryConstPointer(Geometry.GeometryBase geometry)
    {
      IntPtr rc = IntPtr.Zero;
      if (geometry != null)
        rc = geometry.ConstPointer();
      return rc;
    }

    public static IntPtr NativeGeometryNonConstPointer(Geometry.GeometryBase geometry)
    {
      IntPtr rc = IntPtr.Zero;
      if (geometry != null)
        rc = geometry.NonConstPointer();
      return rc;
    }

    public static IntPtr RhinoObjectConstPointer(Rhino.DocObjects.RhinoObject rhinoObject)
    {
      IntPtr rc = IntPtr.Zero;
      if (rhinoObject != null)
        rc = rhinoObject.ConstPointer();
      return rc;
    }


    /// <summary>
    /// Create a RhinoCommon Geometry class from a given ON_Geomety*. The ON_Geometry*
    /// must be declared on the heap and it's lifetime becomes controlled by RhinoCommon
    /// </summary>
    /// <param name="pGeometry">ON_Geometry*</param>
    /// <returns>The appropriate geometry class in RhinoCommon on success</returns>
    public static Geometry.GeometryBase CreateFromNativePointer(IntPtr pGeometry)
    {
      return Geometry.GeometryBase.CreateGeometryHelper(pGeometry, null);
    }

    /// <summary>
    /// Attempts to copy the contents of a RMA.OpenNURBS.OnArc to a Rhino.Geometry.Arc
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    /// <returns>true on success</returns>
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
    /// Attempts to copy the contents of a Rhino.Geometry.Arc to a RMA.OpenNURBS.OnArc
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    /// <returns></returns>
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
          System.Reflection.PropertyInfo pi = t.GetProperty("InternalPointer");
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
    /// Copy a Rhino_DotNet brep to a RhinoCommon Brep class
    /// </summary>
    /// <param name="source">
    /// RMA.OpenNURBS.IOnBrep or RMA.OpenNURBS.OnBrep
    /// </param>
    /// <returns>
    /// RhinoCommon Brep on success. This will be an independent copy
    /// </returns>
    public static Rhino.Geometry.Brep FromOnBrep(object source)
    {
      Rhino.Geometry.GeometryBase g = CopyHelper(source, "RMA.OpenNURBS.OnBrep");
      Rhino.Geometry.Brep rc = g as Rhino.Geometry.Brep;
      return rc;
    }

    /// <summary>
    /// Copy a Rhino_DotNet surface to a RhinoCommon Surface class.
    /// </summary>
    /// <param name="source">
    /// Any of the following in the RMA.OpenNURBS namespace are acceptable.
    /// IOnSurface, OnSurface, IOnPlaneSurface, OnPlaneSurface, IOnClippingPlaneSurface,
    /// OnClippingPlaneSurface, IOnNurbsSurface, OnNurbsSurfac, IOnRevSurface, OnRevSurface,
    /// IOnSumSurface, OnSumSurface
    /// </param>
    /// <returns>
    /// RhinoCommon Surface on success. This will be an independent copy
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
    /// Copy a Rhino_DotNet Mesh to a RhinoCommon Mesh class.
    /// </summary>
    /// <param name="source">
    /// RMA.OpenNURBS.IOnMesh or RMA.OpenNURBS.OnMesh
    /// </param>
    /// <returns>
    /// RhinoCommon Mesh on success. This will be an independent copy
    /// </returns>
    public static Rhino.Geometry.Mesh FromOnMesh(object source)
    {
      Rhino.Geometry.GeometryBase g = CopyHelper(source, "RMA.OpenNURBS.OnMesh");
      Rhino.Geometry.Mesh rc = g as Rhino.Geometry.Mesh;
      return rc;
    }

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

    static Type GetRhinoDotNetType(string name)
    {
      System.Reflection.Assembly rhinoDotNet = HostUtils.GetRhinoDotNetAssembly();
      if (null == rhinoDotNet)
        return null;
      return rhinoDotNet.GetType(name);
    }

    public static object ToOnBrep(Rhino.Geometry.Brep source)
    {
      object rc = null;
      IntPtr pSource = source.ConstPointer();
      Type onBrepType = GetRhinoDotNetType("RMA.OpenNURBS.OnBrep");
      if (IntPtr.Zero != pSource && null != onBrepType)
      {
        System.Reflection.MethodInfo mi = onBrepType.GetMethod("WrapNativePointer", new Type[] { typeof(IntPtr), typeof(bool), typeof(bool) });
        IntPtr pNewBrep = UnsafeNativeMethods.ON_Brep_New(pSource);
        rc = mi.Invoke(null, new object[] { pNewBrep, false, true });
      }
      return rc;
    }

    public static object ToOnSurface(Rhino.Geometry.Surface source)
    {
      object rc = null;
      IntPtr pSource = source.ConstPointer();
      Type onType = GetRhinoDotNetType("RMA.OpenNURBS.OnSurface");
      if (IntPtr.Zero != pSource && null != onType)
      {
        System.Reflection.MethodInfo mi = onType.GetMethod("WrapNativePointer", new Type[] { typeof(IntPtr), typeof(bool), typeof(bool) });
        IntPtr pNewSurface = UnsafeNativeMethods.ON_Surface_DuplicateSurface(pSource);
        rc = mi.Invoke(null, new object[] { pNewSurface, false, true });
      }
      return rc;
    }

    public static object ToOnMesh(Rhino.Geometry.Mesh source)
    {
      object rc = null;
      IntPtr pSource = source.ConstPointer();
      Type onType = GetRhinoDotNetType("RMA.OpenNURBS.OnMesh");
      if (IntPtr.Zero != pSource && null != onType)
      {
        System.Reflection.MethodInfo mi = onType.GetMethod("WrapNativePointer", new Type[] { typeof(IntPtr), typeof(bool), typeof(bool) });
        IntPtr pNewMesh = UnsafeNativeMethods.ON_Mesh_New(pSource);
        rc = mi.Invoke(null, new object[] { pNewMesh, false, true });
      }
      return rc;
    }

    public static object ToOnCurve(Rhino.Geometry.Curve source)
    {
      object rc = null;
      IntPtr pSource = source.ConstPointer();
      Type onType = GetRhinoDotNetType("RMA.OpenNURBS.OnCurve");
      if (IntPtr.Zero != pSource && null != onType)
      {
        System.Reflection.MethodInfo mi = onType.GetMethod("WrapNativePointer", new Type[] { typeof(IntPtr), typeof(bool), typeof(bool) });
        IntPtr pNewCurve = UnsafeNativeMethods.ON_Curve_DuplicateCurve(pSource);
        rc = mi.Invoke(null, new object[] { pNewCurve, false, true });
      }
      return rc;
    }

    public static object ToOnXform(Rhino.Geometry.Transform source)
    {
      object rc = null;
      Type onType = GetRhinoDotNetType("RMA.OpenNURBS.OnXform");
      if (null != onType)
      {
        double[] vals = new double[16];
        for( int row=0; row<4; row++ )
        {
          for (int column = 0; column < 4; column++)
          {
            vals[4 * row + column] = source[row, column];
          }
        }
        rc = System.Activator.CreateInstance(onType, new object[] { vals });
      }
      return rc;
    }

    /// <summary>
    /// Convert a Rhino.Display.Viewport to an RMA.Rhino.IRhinoViewport
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static object ToIRhinoViewport(Rhino.Display.RhinoViewport source)
    {
      object rc = null;
      IntPtr pSource = source.ConstPointer();
      Type rhType = GetRhinoDotNetType("RMA.Rhino.MRhinoViewport");
      if (IntPtr.Zero != pSource && null != rhType)
      {
        System.Reflection.MethodInfo mi = rhType.GetMethod("WrapNativePointer", new Type[] { typeof(IntPtr), typeof(bool), typeof(bool) });
        const bool isConst = true;
        const bool doDelete = false;
        rc = mi.Invoke(null, new object[] { pSource, isConst, doDelete });
      }
      return rc;
    }
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
        /// <param name="source"></param>
        /// <returns></returns>
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
    public static IntPtr PlugInPointer(Rhino.PlugIns.PlugIn plugin)
    {
      return null == plugin ? IntPtr.Zero : plugin.NonConstPointer();
    }
  }
}