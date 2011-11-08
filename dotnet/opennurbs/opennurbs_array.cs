using System;
using System.Runtime.InteropServices;
using Rhino.Geometry;

namespace Rhino.Runtime
{
  class INTERNAL_GeometryArray : IDisposable
  {
    IntPtr m_ptr; //ON_SimpleArray<ON_Geometry*>*
    public IntPtr ConstPointer() { return m_ptr; }
    public IntPtr NonConstPointer() { return m_ptr; }

    public INTERNAL_GeometryArray()
    {
      m_ptr = UnsafeNativeMethods.ON_GeometryArray_New(0);
    }

    //public INTERNAL_GeometryArray(GeometryBase[] geom)
    //{
    //  int initial_capacity = 0;
    //  if (geom != null)
    //    initial_capacity = geom.Length;
    //  m_ptr = UnsafeNativeMethods.ON_GeometryArray_New(initial_capacity);
    //  for (int i = 0; i < geom.Length; i++)
    //  {
    //    IntPtr geomPtr = geom[i].ConstPointer();
    //    UnsafeNativeMethods.ON_GeometryArray_Append(m_ptr, geomPtr);
    //  }
    //}

    public INTERNAL_GeometryArray(System.Collections.Generic.IEnumerable<GeometryBase> geom)
    {
      m_ptr = UnsafeNativeMethods.ON_GeometryArray_New(0);

      foreach (GeometryBase gb in geom)
      {
        IntPtr geomPtr = gb.ConstPointer();
        UnsafeNativeMethods.ON_GeometryArray_Append(m_ptr, geomPtr);
      }
    }

    public INTERNAL_GeometryArray(System.Collections.Generic.IEnumerable<Surface> srfs)
    {
      m_ptr = UnsafeNativeMethods.ON_GeometryArray_New(0);

      foreach (Surface s in srfs)
      {
        IntPtr geomPtr = s.ConstPointer();
        UnsafeNativeMethods.ON_GeometryArray_Append(m_ptr, geomPtr);
      }
    }

    public INTERNAL_GeometryArray(System.Collections.Generic.IEnumerable<TextDot> dots)
    {
      m_ptr = UnsafeNativeMethods.ON_GeometryArray_New(0);

      foreach (TextDot td in dots)
      {
        IntPtr geomPtr = td.ConstPointer();
        UnsafeNativeMethods.ON_GeometryArray_Append(m_ptr, geomPtr);
      }
    }
    //public GeometryBase[] ToArray(bool isConst)
    //{
    //  int count = UnsafeNativeMethods.ON_GeometryArray_Count(m_ptr);
    //  GeometryBase[] rc = new GeometryBase[count];
    //  for (int i = 0; i < count; i++)
    //  {
    //    IntPtr geom = UnsafeNativeMethods.ON_GeometryArray_Get(m_ptr, i);
    //    if (IntPtr.Zero == geom)
    //      continue;
    //    rc[i] = GeometryBase.CreateGeometryHelper(geom, isConst, 0);
    //  }
    //  return rc;
    //}

    ~INTERNAL_GeometryArray()
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
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.ON_GeometryArray_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }
  }

  //////////////////////////////////////////////////////////////
  //////////////////////////////////////////////////////////////
  class INTERNAL_ComponentIndexArray : IDisposable
  {
    public IntPtr m_ptr; // ON_SimpleArray<ON_COMPONENT_INDEX>
    public IntPtr NonConstPointer() { return m_ptr; }

    public INTERNAL_ComponentIndexArray()
    {
      m_ptr = UnsafeNativeMethods.ON_ComponentIndexArray_New();
    }

    public int Count
    {
      get { return UnsafeNativeMethods.ON_ComponentIndexArray_Count(m_ptr); }
    }

    public ComponentIndex[] ToArray()
    {
      int count = Count;
      if (count < 1)
        return null;
      ComponentIndex[] rc = new ComponentIndex[count];
      UnsafeNativeMethods.ON_ComponentIndexArray_CopyValues(m_ptr, ref rc[0]);
      return rc;
    }


    ~INTERNAL_ComponentIndexArray()
    {
      InternalDispose();
    }

    public void Dispose()
    {
      InternalDispose();
      GC.SuppressFinalize(this);
    }

    private void InternalDispose()
    {
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.ON_ComponentIndexArray_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }
  }

  class StringHolder : IDisposable
  {
    IntPtr m_ptr; // CRhCmnString*
    internal IntPtr ConstPointer() { return m_ptr; }
    internal IntPtr NonConstPointer() { return m_ptr; }

    public StringHolder()
    {
      m_ptr = UnsafeNativeMethods.StringHolder_New();
    }

    ~StringHolder()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose( bool disposing )
    {
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.StringHolder_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }

    public override string ToString()
    {
      IntPtr pString = UnsafeNativeMethods.StringHolder_Get(m_ptr);
      string rc = Marshal.PtrToStringUni(pString);
      return rc ?? String.Empty;
    }
  }

  //////////////////////////////////////////////////////////////
  //////////////////////////////////////////////////////////////
  class INTERNAL_BrepArray : IDisposable
  {
    IntPtr m_ptr; // ON_SimpleArray<ON_Brep*>*
    internal IntPtr ConstPointer() { return m_ptr; }
    internal IntPtr NonConstPointer() { return m_ptr; }

    public INTERNAL_BrepArray()
    {
      m_ptr = UnsafeNativeMethods.ON_BrepArray_New();
    }

    ~INTERNAL_BrepArray()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose( bool disposing )
    {
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.ON_BrepArray_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }

    public int Count
    {
      get
      {
        IntPtr ptr = ConstPointer();
        int count = UnsafeNativeMethods.ON_BrepArray_Count(ptr);
        return count;
      }
    }

    public void AddBrep(Geometry.Brep brep, bool asConst)
    {
      if (null != brep)
      {
        IntPtr pBrep = brep.ConstPointer();
        if (!asConst)
          pBrep = brep.NonConstPointer();
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.ON_BrepArray_Append(ptr, pBrep);
      }
    }

    public Geometry.Brep[] ToNonConstArray()
    {
      int count = Count;
      if (count < 1)
        return new Geometry.Brep[0]; //MSDN guidelines prefer empty arrays
      IntPtr ptr = ConstPointer();
      Brep[] rc = new Brep[count];
      for (int i = 0; i < count; i++)
      {
        IntPtr pBrep = UnsafeNativeMethods.ON_BrepArray_Get(ptr, i);
        if (IntPtr.Zero != pBrep)
          rc[i] = new Brep(pBrep, null);
      }
      return rc;
    }
  }

}

namespace Rhino.Runtime.InteropWrappers
{
  /// <summary>
  /// Wrapper for ON_SimpleArray&lt;int&gt;. If you are not writing C++ code
  /// then this class is not for you
  /// </summary>
  public class SimpleArrayInt : IDisposable
  {
    internal IntPtr m_ptr; // ON_SimpleArray<int>

    /// <summary>
    /// Gets the const (immutable) pointer of this array.
    /// </summary>
    /// <returns>The const pointer.</returns>
    public IntPtr ConstPointer() { return m_ptr; }

    /// <summary>
    /// Gets the non-const pointer (for modification) of this array.
    /// </summary>
    /// <returns>The non-const pointer.</returns>
    public IntPtr NonConstPointer() { return m_ptr; }

    /// <summary>
    /// Initializes a new <see cref="SimpleArrayInt"/> class.
    /// </summary>
    public SimpleArrayInt()
    {
      m_ptr = UnsafeNativeMethods.ON_IntArray_New();
    }

    /// <summary>
    /// Gets the amount of elements in this array.
    /// </summary>
    public int Count
    {
      get { return UnsafeNativeMethods.ON_IntArray_Count(m_ptr); }
    }

    /// <summary>
    /// Returns the managed counterpart of the unmanaged array.
    /// </summary>
    /// <returns>The managed array.</returns>
    public int[] ToArray()
    {
      int count = Count;
      if (count < 1)
        return new int[0];
      int[] rc = new int[count];
      UnsafeNativeMethods.ON_IntArray_CopyValues(m_ptr, rc);
      return rc;
    }

    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~SimpleArrayInt()
    {
      InternalDispose();
    }

    /// <summary>
    /// Actively reclaims unmanaged resources that this instance uses.
    /// </summary>
    public void Dispose()
    {
      InternalDispose();
      GC.SuppressFinalize(this);
    }

    private void InternalDispose()
    {
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.ON_IntArray_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }
  }

  /// <summary>
  /// Wrapper for ON_SimpleArray&lt;ON_Imterval&gt;. If you are not writing C++ code
  /// then this class is not for you.
  /// </summary>
  public class SimpleArrayInterval : IDisposable
  {
    IntPtr m_ptr; // ON_SimpleArray<ON_Interval>

    /// <summary>
    /// Gets the const (immutable) pointer of this array.
    /// </summary>
    /// <returns>The const pointer.</returns>
    public IntPtr ConstPointer() { return m_ptr; }

    /// <summary>
    /// Gets the non-const pointer (for modification) of this array.
    /// </summary>
    /// <returns>The non-const pointer.</returns>
    public IntPtr NonConstPointer() { return m_ptr; }

    /// <summary>
    /// Initializes a new <see cref="SimpleArrayInterval"/> class.
    /// </summary>
    public SimpleArrayInterval()
    {
      m_ptr = UnsafeNativeMethods.ON_IntervalArray_New();
    }

    /// <summary>
    /// Gets the amount of elements in this array.
    /// </summary>
    public int Count
    {
      get { return UnsafeNativeMethods.ON_IntervalArray_Count(m_ptr); }
    }

    /// <summary>
    /// Returns the managed counterpart of the unmanaged array.
    /// </summary>
    /// <returns>The managed array.</returns>
    public Interval[] ToArray()
    {
      int count = Count;
      if (count < 1)
        return new Interval[0];
      Interval[] rc = new Interval[count];
      UnsafeNativeMethods.ON_IntervalArray_CopyValues(m_ptr, rc);
      return rc;
    }

    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~SimpleArrayInterval()
    {
      InternalDispose();
    }

    /// <summary>
    /// Actively reclaims unmanaged resources that this instance uses.
    /// </summary>
    public void Dispose()
    {
      InternalDispose();
      GC.SuppressFinalize(this);
    }

    private void InternalDispose()
    {
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.ON_IntervalArray_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }
  }

  /// <summary>
  /// Wrapper for ON_SimpleArray&lt;double&gt;. If you are not writing C++ code,
  /// then this class is not for you.
  /// </summary>
  public class SimpleArrayDouble : IDisposable
  {
    private IntPtr m_ptr; // ON_SimpleArray<double>

    /// <summary>
    /// Gets the const (immutable) pointer of this array.
    /// </summary>
    /// <returns>The const pointer.</returns>
    public IntPtr ConstPointer() { return m_ptr; }

    /// <summary>
    /// Gets the non-const pointer (for modification) of this array.
    /// </summary>
    /// <returns>The non-const pointer.</returns>
    public IntPtr NonConstPointer() { return m_ptr; }

    /// <summary>
    /// Initializes a new <see cref="SimpleArrayDouble"/> instance.
    /// </summary>
    public SimpleArrayDouble()
    {
      m_ptr = UnsafeNativeMethods.ON_DoubleArray_New();
    }

    /// <summary>
    /// Initializes a new <see cref="SimpleArrayDouble"/> instance, with items.
    /// </summary>
    public SimpleArrayDouble(System.Collections.Generic.IEnumerable<double> items)
    {
      Rhino.Collections.RhinoList<double> list = new Rhino.Collections.RhinoList<double>(items);
      UnsafeNativeMethods.ON_DoubleArray_Append(m_ptr, list.Count, list.m_items);
    }

    /// <summary>
    /// Gets the amount of elements in this array.
    /// </summary>
    public int Count
    {
      get { return UnsafeNativeMethods.ON_DoubleArray_Count(m_ptr); }
    }

    /// <summary>
    /// Returns the managed counterpart of the unmanaged array.
    /// </summary>
    /// <returns>The managed array.</returns>
    public double[] ToArray()
    {
      int count = Count;
      if (count < 1)
        return new double[0];
      double[] rc = new double[count];
      UnsafeNativeMethods.ON_DoubleArray_CopyValues(m_ptr, ref rc[0]);
      return rc;
    }

    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~SimpleArrayDouble()
    {
      InternalDispose();
    }

    /// <summary>
    /// Actively reclaims unmanaged resources that this instance uses.
    /// </summary>
    public void Dispose()
    {
      InternalDispose();
      GC.SuppressFinalize(this);
    }

    private void InternalDispose()
    {
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.ON_DoubleArray_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }
  }

  /// <summary>
  /// ON_SimpleArray&lt;ON_3dPoint&gt;, ON_3dPointArray, ON_PolyLine all have the same size
  /// This class wraps all of these C++ versions.  If you are not writing C++ code then this
  /// class is not for you.
  /// </summary>
  public class SimpleArrayPoint3d : IDisposable
  {
    private IntPtr m_ptr;

    /// <summary>
    /// Gets the const (immutable) pointer of this array.
    /// </summary>
    /// <returns>The const pointer.</returns>
    public IntPtr ConstPointer() { return m_ptr; }

    /// <summary>
    /// Gets the non-const pointer (for modification) of this array.
    /// </summary>
    /// <returns>The non-const pointer.</returns>
    public IntPtr NonConstPointer() { return m_ptr; }

    /// <summary>
    /// Initializes a new empty <see cref="SimpleArrayPoint3d"/> instance.
    /// </summary>
    public SimpleArrayPoint3d()
    {
      m_ptr = UnsafeNativeMethods.ON_3dPointArray_New(0);
    }

    // not used and internal class, so comment out
    //public SimpleArrayPoint3d(int initialCapacity)
    //{
    //  m_ptr = UnsafeNativeMethods.ON_3dPointArray_New(initialCapacity);
    //}

    /// <summary>
    /// Gets the amount of points in this array.
    /// </summary>
    public int Count
    {
      get
      {
        IntPtr ptr = ConstPointer();
        int count = UnsafeNativeMethods.ON_3dPointArray_Count(ptr);
        return count;
      }
    }

    /// <summary>
    /// Copies the unmanaged array to a managed counterpart.
    /// </summary>
    /// <returns>The managed array.</returns>
    public Point3d[] ToArray()
    {
      int count = Count;
      if (count < 1)
        return new Point3d[0];

      Point3d[] rc = new Point3d[count];
      UnsafeNativeMethods.ON_3dPointArray_CopyValues(m_ptr, rc);
      return rc;
    }

    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~SimpleArrayPoint3d()
    {
      Dispose(false);
    }

    /// <summary>
    /// Actively reclaims unmanaged resources that this instance uses.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// For derived class implementers.
    /// <para>This method is called with argument true when class user calls Dispose(), while with argument false when
    /// the Garbage Collector invokes the finalizer, or Finalize() method.</para>
    /// <para>You must reclaim all used unmanaged resources in both cases, and can use this chance to call Dispose on disposable fields if the argument is true.</para>
    /// <para>Also, you must call the base virtual method within your overriding method.</para>
    /// </summary>
    /// <param name="disposing">true if the call comes from the Dispose() method; false if it comes from the Garbage Collector finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.ON_3dPointArray_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }
  }

  /// <summary>
  /// Wrapper for ON_SimpleArray&lt;ON_Line&gt;. If you are not writing C++ code
  /// then this class is not for you
  /// </summary>
  public class SimpleArrayLine : IDisposable
  {
    IntPtr m_ptr; // ON_SimpleArray<ON_Line>

    /// <summary>
    /// Gets the const (immutable) pointer of this array.
    /// </summary>
    /// <returns>The const pointer.</returns>
    public IntPtr ConstPointer()
    {
      return m_ptr;
    }

    /// <summary>
    /// Gets the non-const pointer (for modification) of this array.
    /// </summary>
    /// <returns>The non-const pointer.</returns>
    public IntPtr NonConstPointer()
    {
      return m_ptr;
    }

    /// <summary>
    /// Initializes a new <see cref="SimpleArrayLine"/> instance.
    /// </summary>
    public SimpleArrayLine()
    {
      m_ptr = UnsafeNativeMethods.ON_LineArray_New();
    }

    /// <summary>
    /// Gets the amount of lines in this array.
    /// </summary>
    public int Count
    {
      get { return UnsafeNativeMethods.ON_LineArray_Count(m_ptr); }
    }

    /// <summary>
    /// Copies the unmanaged array to a managed counterpart.
    /// </summary>
    /// <returns>The managed array.</returns>
    public Line[] ToArray()
    {
      int count = Count;
      if (count < 1)
        return new Line[0];
      Line[] rc = new Line[count];
      UnsafeNativeMethods.ON_LineArray_CopyValues(m_ptr, ref rc[0]);
      return rc;
    }

    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~SimpleArrayLine()
    {
      InternalDispose();
    }

    /// <summary>
    /// Actively reclaims unmanaged resources that this instance uses.
    /// </summary>
    public void Dispose()
    {
      InternalDispose();
      GC.SuppressFinalize(this);
    }

    private void InternalDispose()
    {
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.ON_LineArray_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }
  }

  /// <summary>
  /// Wrapper for a C++ ON_SimpleArray of ON_Surface* or const ON_Surface*.  If
  /// you are not writing C++ code then this class is not for you.
  /// </summary>
  public class SimpleArraySurfacePointer : IDisposable
  {
    IntPtr m_ptr; //ON_SimpleArray<ON_Surface*>*

    /// <summary>
    /// Gets the const (immutable) pointer of this array.
    /// </summary>
    /// <returns>The const pointer.</returns>
    public IntPtr ConstPointer() { return m_ptr; }

    /// <summary>
    /// Gets the non-const pointer (for modification) of this array.
    /// </summary>
    /// <returns>The non-const pointer.</returns>
    public IntPtr NonConstPointer() { return m_ptr; }

    /// <summary>
    /// Initializes a new <see cref="SimpleArraySurfacePointer"/> instance.
    /// </summary>
    public SimpleArraySurfacePointer()
    {
      m_ptr = UnsafeNativeMethods.ON_SurfaceArray_New();
    }

    /// <summary>
    /// Copies the unmanaged array to a managed counterpart.
    /// Elements are made non-const.
    /// </summary>
    /// <returns>The managed array.</returns>
    public Surface[] ToNonConstArray()
    {
      int count = UnsafeNativeMethods.ON_SurfaceArray_Count(m_ptr);
      if (count < 1)
        return new Surface[0];
      Surface[] rc = new Surface[count];
      for (int i = 0; i < count; i++)
      {
        IntPtr surface = UnsafeNativeMethods.ON_SurfaceArray_Get(m_ptr, i);
        if (IntPtr.Zero == surface)
          continue;
        rc[i] = GeometryBase.CreateGeometryHelper(surface, null) as Surface;
      }
      return rc;
    }

    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~SimpleArraySurfacePointer()
    {
      Dispose(false);
    }

    /// <summary>
    /// Actively reclaims unmanaged resources that this instance uses.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// For derived class implementers.
    /// <para>This method is called with argument true when class user calls Dispose(), while with argument false when
    /// the Garbage Collector invokes the finalizer, or Finalize() method.</para>
    /// <para>You must reclaim all used unmanaged resources in both cases, and can use this chance to call Dispose on disposable fields if the argument is true.</para>
    /// <para>Also, you must call the base virtual method within your overriding method.</para>
    /// </summary>
    /// <param name="disposing">true if the call comes from the Dispose() method; false if it comes from the Garbage Collector finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.ON_SurfaceArray_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }
  }

  /// <summary>
  /// Wrapper for a C++ ON_SimpleArray of ON_Curve* or const ON_Curve*.  If you are not
  /// writing C++ code, then you can ignore this class
  /// </summary>
  public class SimpleArrayCurvePointer : IDisposable
  {
    IntPtr m_ptr; //ON_SimpleArray<ON_Curve*>*

    /// <summary>
    /// Gets the const (immutable) pointer of this array.
    /// </summary>
    /// <returns>The const pointer.</returns>
    public IntPtr ConstPointer() { return m_ptr; }

    /// <summary>
    /// Gets the non-const pointer (for modification) of this array.
    /// </summary>
    /// <returns>The non-const pointer.</returns>
    public IntPtr NonConstPointer() { return m_ptr; }

    /// <summary>
    /// Initializes a new <see cref="SimpleArrayCurvePointer"/> instance.
    /// </summary>
    public SimpleArrayCurvePointer()
    {
      m_ptr = UnsafeNativeMethods.ON_CurveArray_New(0);
    }

    /// <summary>
    /// Initializes a new <see cref="SimpleArrayCurvePointer"/> instance, from a set of input curves.
    /// </summary>
    /// <param name="curves">A list, an array or any collection of curves that implements the enumerable interface.</param>
    public SimpleArrayCurvePointer(System.Collections.Generic.IEnumerable<Curve> curves)
    {
      int initial_capacity = 0;
      foreach (Curve c in curves)
      {
        if (null != c)
          initial_capacity++;
      }

      m_ptr = UnsafeNativeMethods.ON_CurveArray_New(initial_capacity);
      foreach (Curve c in curves)
      {
        if (null != c)
        {
          IntPtr curvePtr = c.ConstPointer();
          UnsafeNativeMethods.ON_CurveArray_Append(m_ptr, curvePtr);
        }
      }
    }

    /// <summary>
    /// Copies the unmanaged array to a managed counterpart.
    /// </summary>
    /// <returns>The managed array.</returns>
    public Curve[] ToNonConstArray()
    {
      int count = UnsafeNativeMethods.ON_CurveArray_Count(m_ptr);
      if (count < 1)
        return new Curve[0];
      Curve[] rc = new Curve[count];
      for (int i = 0; i < count; i++)
      {
        IntPtr curve = UnsafeNativeMethods.ON_CurveArray_Get(m_ptr, i);
        if (IntPtr.Zero == curve)
          continue;
        rc[i] = GeometryBase.CreateGeometryHelper(curve, null) as Curve;
      }
      return rc;
    }

    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~SimpleArrayCurvePointer()
    {
      Dispose(false);
    }

    /// <summary>
    /// Actively reclaims unmanaged resources that this instance uses.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// For derived class implementers.
    /// <para>This method is called with argument true when class user calls Dispose(), while with argument false when
    /// the Garbage Collector invokes the finalizer, or Finalize() method.</para>
    /// <para>You must reclaim all used unmanaged resources in both cases, and can use this chance to call Dispose on disposable fields if the argument is true.</para>
    /// <para>Also, you must call the base virtual method within your overriding method.</para>
    /// </summary>
    /// <param name="disposing">true if the call comes from the Dispose() method; false if it comes from the Garbage Collector finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.ON_CurveArray_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }
  }

  /// <summary>
  /// Wrapper for a C++ ON_SimpleArray of ON_Geometry* or const ON_Geometry*.  If you are not
  /// writing C++ code, then this class is not for you
  /// </summary>
  public class SimpleArrayGeometryPointer : IDisposable
  {
    IntPtr m_ptr; //ON_SimpleArray<ON_Geometry*>*

    /// <summary>
    /// Gets the const (immutable) pointer of this array.
    /// </summary>
    /// <returns>The const pointer.</returns>
    public IntPtr ConstPointer() { return m_ptr; }

    /// <summary>
    /// Gets the non-const pointer (for modification) of this array.
    /// </summary>
    /// <returns>The non-const pointer.</returns>
    public IntPtr NonConstPointer() { return m_ptr; }

    /// <summary>
    /// Initializes a new <see cref="SimpleArrayGeometryPointer"/> instance.
    /// </summary>
    public SimpleArrayGeometryPointer()
    {
      m_ptr = UnsafeNativeMethods.ON_GeometryArray_New(0);
    }

    /// <summary>
    /// Copies the unmanaged array to a managed counterpart.
    /// </summary>
    /// <returns>The managed array.</returns>
    public GeometryBase[] ToNonConstArray()
    {
      int count = UnsafeNativeMethods.ON_GeometryArray_Count(m_ptr);
      GeometryBase[] rc = new GeometryBase[count];
      for (int i = 0; i < count; i++)
      {
        IntPtr pGeometry = UnsafeNativeMethods.ON_GeometryArray_Get(m_ptr, i);
        rc[i] = GeometryBase.CreateGeometryHelper(pGeometry, null);
      }
      return rc;
    }

    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~SimpleArrayGeometryPointer()
    {
      Dispose(false);
    }

    /// <summary>
    /// Actively reclaims unmanaged resources that this instance uses.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// For derived class implementers.
    /// <para>This method is called with argument true when class user calls Dispose(), while with argument false when
    /// the Garbage Collector invokes the finalizer, or Finalize() method.</para>
    /// <para>You must reclaim all used unmanaged resources in both cases, and can use this chance to call Dispose on disposable fields if the argument is true.</para>
    /// <para>Also, you must call the base virtual method within your overriding method.</para>
    /// </summary>
    /// <param name="disposing">true if the call comes from the Dispose() method; false if it comes from the Garbage Collector finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.ON_GeometryArray_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }
  }

  /// <summary>
  /// Wrapper for a C++ ON_SimpleArray of ON_Mesh* or const ON_Mesh*. If you are not
  /// writing C++ code then this class is not for you
  /// </summary>
  public class SimpleArrayMeshPointer : IDisposable
  {
    IntPtr m_ptr; // ON_SimpleArray<ON_Mesh*>*

    /// <summary>
    /// Gets the const (immutable) pointer of this array.
    /// </summary>
    /// <returns>The const pointer.</returns>
    public IntPtr ConstPointer() { return m_ptr; }

    /// <summary>
    /// Gets the non-const pointer (for modification) of this array.
    /// </summary>
    /// <returns>The non-const pointer.</returns>
    public IntPtr NonConstPointer() { return m_ptr; }

    /// <summary>
    /// Initializes a new <see cref="SimpleArrayMeshPointer"/> instance.
    /// </summary>
    public SimpleArrayMeshPointer()
    {
      m_ptr = UnsafeNativeMethods.ON_MeshArray_New();
    }

    /// <summary>
    /// Gets the amount of meshes in this array.
    /// </summary>
    public int Count
    {
      get
      {
        IntPtr ptr = ConstPointer();
        int count = UnsafeNativeMethods.ON_MeshArray_Count(ptr);
        return count;
      }
    }

    /// <summary>
    /// Adds a mesh to 
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="asConst"></param>
    public void Add(Geometry.Mesh mesh, bool asConst)
    {
      if (null != mesh)
      {
        IntPtr pMesh = mesh.ConstPointer();
        if (!asConst)
          pMesh = mesh.NonConstPointer();
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.ON_MeshArray_Append(ptr, pMesh);
      }
    }
    
    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~SimpleArrayMeshPointer()
    {
      Dispose(false);
    }

    /// <summary>
    /// Actively reclaims unmanaged resources that this instance uses.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// For derived class implementers.
    /// <para>This method is called with argument true when class user calls Dispose(), while with argument false when
    /// the Garbage Collector invokes the finalizer, or Finalize() method.</para>
    /// <para>You must reclaim all used unmanaged resources in both cases, and can use this chance to call Dispose on disposable fields if the argument is true.</para>
    /// <para>Also, you must call the base virtual method within your overriding method.</para>
    /// </summary>
    /// <param name="disposing">true if the call comes from the Dispose() method; false if it comes from the Garbage Collector finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.ON_MeshArray_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }

    /// <summary>
    /// Copies the unmanaged array to a managed counterpart.
    /// </summary>
    /// <returns>The managed array.</returns>
    public Geometry.Mesh[] ToNonConstArray()
    {
      int count = Count;
      if (count < 1)
        return new Geometry.Mesh[0];
      IntPtr ptr = ConstPointer();
      Mesh[] rc = new Mesh[count];
      for (int i = 0; i < count; i++)
      {
        IntPtr pMesh = UnsafeNativeMethods.ON_MeshArray_Get(ptr, i);
        if (IntPtr.Zero != pMesh)
          rc[i] = new Mesh(pMesh, null);
      }
      return rc;
    }
  }

}