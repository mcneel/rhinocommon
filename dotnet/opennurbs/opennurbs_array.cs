using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Rhino.Geometry;

namespace Rhino.Runtime.InteropWrappers
{
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
    [System.Security.SecuritySafeCritical]
  /// <summary>
  /// This class is used to pass strings back and forth between managed
  /// and unmanaged code.  This should not be be needed by plug-ins.
  /// </summary>
  public class StringHolder : IDisposable
  {
    IntPtr m_ptr; // CRhCmnString*
    /// <summary>
    /// C++ pointer used to access the ON_wString, managed plug-ins should
    /// never need this.
    /// </summary>
    /// <returns></returns>
    public IntPtr ConstPointer() { return m_ptr; }
    /// <summary>
    /// C++ pointer used to access the ON_wString, managed plug-ins should
    /// never need this.
    /// </summary>
    /// <returns></returns>
    public IntPtr NonConstPointer() { return m_ptr; }

        [System.Security.SecuritySafeCritical]
    /// <summary>
    /// Constructor
    /// </summary>
    public StringHolder()
    {
      m_ptr = UnsafeNativeMethods.StringHolder_New();
    }
    /// <summary>
    /// Destructor
    /// </summary>
    ~StringHolder()
    {
      Dispose(false);
    }
    /// <summary>
    /// IDispose implementation
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }
    /// <summary>
    /// Called by Dispose and finalizer
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose( bool disposing )
    {
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.StringHolder_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }
    /// <summary>
    /// Marshal unmanaged ON_wString to a managed .NET string
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return GetString(m_ptr);
    }
    /// <summary>
    /// Get managed string from unmanaged ON_wString pointer.
    /// </summary>
    /// <param name="pStringHolder"></param>
    /// <returns></returns>
    public static string GetString(IntPtr pStringHolder)
    {
      IntPtr pString = UnsafeNativeMethods.StringHolder_Get(pStringHolder);
      string rc = Marshal.PtrToStringUni(pString);
      return rc ?? String.Empty;
    }
  }
  /// <summary>
  /// Wrapper for ON_SimpleArray&lt;int&gt;. If you are not writing C++ code
  /// then this class is not for you.
  /// </summary>
  public class SimpleArrayInt : IDisposable
  {
    //This should be private eventually and have everything call either ConstPointer or NonConstPointer
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
      m_ptr = UnsafeNativeMethods.ON_IntArray_New(null,0);
    }

    /// <summary>
    /// Initializes a new <see cref="SimpleArrayInt"/> class
    /// </summary>
    /// <param name="values">initial set of integers to add to the array</param>
    public SimpleArrayInt(IEnumerable<int> values)
    {
      if (values == null)
      {
        m_ptr = UnsafeNativeMethods.ON_IntArray_New(null,0);
      }
      else
      {
        List<int> list_values = new List<int>(values);
        int[] array_values = list_values.ToArray();
        m_ptr = UnsafeNativeMethods.ON_IntArray_New(array_values, list_values.Count);
      }
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
  /// Wrapper for ON_SimpleArray&lt;ON_UUID&gt;. If you are not writing C++ code
  /// then this class is not for you.
  /// </summary>
  public class SimpleArrayGuid : IDisposable
  {
    internal IntPtr m_ptr; // ON_SimpleArray<ON_UUID>

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
    /// Initializes a new <see cref="SimpleArrayGuid"/> class.
    /// </summary>
    public SimpleArrayGuid()
    {
      m_ptr = UnsafeNativeMethods.ON_UUIDArray_New();
    }

    /// <summary>
    /// Gets the amount of elements in this array.
    /// </summary>
    public int Count
    {
      get { return UnsafeNativeMethods.ON_UUIDArray_Count(m_ptr); }
    }

    /// <summary>
    /// Returns the managed counterpart of the unmanaged array.
    /// </summary>
    /// <returns>The managed array.</returns>
    public Guid[] ToArray()
    {
      int count = Count;
      if (count < 1)
        return new Guid[0];
      Guid[] rc = new Guid[count];
      UnsafeNativeMethods.ON_UUIDArray_CopyValues(m_ptr, rc);
      return rc;
    }

    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~SimpleArrayGuid()
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
        UnsafeNativeMethods.ON_UUIDArray_Delete(m_ptr);
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
  /// ON_SimpleArray&lt;ON_2dPoint&gt; class wrapper.  If you are not writing
  /// C++ code then this class is not for you.
  /// </summary>
  public class SimpleArrayPoint2d : IDisposable
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
    public SimpleArrayPoint2d()
    {
      m_ptr = UnsafeNativeMethods.ON_2dPointArray_New(0);
    }

    // not used and internal class, so comment out
    //public SimpleArrayPoint3d(int initialCapacity)
    //{
    //  m_ptr = UnsafeNativeMethods.ON_2dPointArray_New(initialCapacity);
    //}

    /// <summary>
    /// Gets the amount of points in this array.
    /// </summary>
    public int Count
    {
      get
      {
        IntPtr ptr = ConstPointer();
        int count = UnsafeNativeMethods.ON_2dPointArray_Count(ptr);
        return count;
      }
    }

    /// <summary>
    /// Copies the unmanaged array to a managed counterpart.
    /// </summary>
    /// <returns>The managed array.</returns>
    public Point2d[] ToArray()
    {
      int count = Count;
      if (count < 1)
        return new Point2d[0];

      Point2d[] rc = new Point2d[count];
      UnsafeNativeMethods.ON_2dPointArray_CopyValues(m_ptr, rc);
      return rc;
    }

    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~SimpleArrayPoint2d()
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
        UnsafeNativeMethods.ON_2dPointArray_Delete(m_ptr);
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
  /// then this class is not for you.
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
  /// writing C++ code, then you can ignore this class.
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
  /// Wrapper for a C++ ON_SimpleArray&lt;ON_Geometry*&gt;* or ON_SimpleArray&lt;const ON_Geometry*&gt;.
  /// If you are not writing C++ code, then this class is not for you.
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
    /// Create an ON_SimpleArray&lt;ON_Geometry*&gt; filled with items in geometry
    /// </summary>
    /// <param name="geometry"></param>
    public SimpleArrayGeometryPointer(System.Collections.Generic.IEnumerable<GeometryBase> geometry)
    {
      m_ptr = UnsafeNativeMethods.ON_GeometryArray_New(0);

      foreach (GeometryBase gb in geometry)
      {
        IntPtr geomPtr = gb.ConstPointer();
        UnsafeNativeMethods.ON_GeometryArray_Append(m_ptr, geomPtr);
      }
    }

    /// <summary>
    /// Expects all of the items in the IEnumerable to be GeometryBase types
    /// </summary>
    /// <param name="geometry"></param>
    public SimpleArrayGeometryPointer(System.Collections.IEnumerable geometry)
    {
      m_ptr = UnsafeNativeMethods.ON_GeometryArray_New(0);

      foreach (object o in geometry)
      {
        GeometryBase gb = o as GeometryBase;
        if (gb != null)
        {
          IntPtr geomPtr = gb.ConstPointer();
          UnsafeNativeMethods.ON_GeometryArray_Append(m_ptr, geomPtr);
        }
      }
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
  /// Represents a wrapper to an unmanaged array of mesh pointers.
  /// <para>Wrapper for a C++ ON_SimpleArray of ON_Mesh* or const ON_Mesh*. If you are not
  /// writing C++ code then this class is not for you.</para>
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
    /// Adds a mesh to the list.
    /// </summary>
    /// <param name="mesh">A mesh to add.</param>
    /// <param name="asConst">Whether this mesh should be treated as non-modifiable.</param>
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
    /// 
    /// </summary>
    /// <param name="disposing"></param>
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

#if RHINO_SDK
    internal Geometry.Mesh[] ToConstArray(Rhino.DocObjects.RhinoObject parent)
    {
      int count = Count;
      if (count < 1)
        return new Geometry.Mesh[0];
      IntPtr ptr = ConstPointer();

      Mesh[] rc = new Mesh[count];
      for (int i = 0; i < rc.Length; i++)
      {
        IntPtr pMesh = UnsafeNativeMethods.ON_MeshArray_Get(ptr, i);
        Rhino.DocObjects.ObjRef objref = new DocObjects.ObjRef(parent, pMesh);
        rc[i] = objref.Mesh();
      }
      return rc;
    }
#endif
  }

  /// <summary>
  /// Wrapper for a C++ ON_SimpleArray&lt;ON_Brep*&gt; or ON_SimpleArray&lt;const ON_Brep*&gt;
  /// If you are not writing C++ code then this class is not for you.
  /// </summary>
  public class SimpleArrayBrepPointer : IDisposable
  {
    IntPtr m_ptr; // ON_SimpleArray<ON_Brep*>*

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
    /// Initializes a new <see cref="SimpleArrayBrepPointer"/> instance.
    /// </summary>
    public SimpleArrayBrepPointer()
    {
      m_ptr = UnsafeNativeMethods.ON_BrepArray_New();
    }

    /// <summary>
    /// Gets the amount of breps in this array.
    /// </summary>
    public int Count
    {
      get
      {
        IntPtr ptr = ConstPointer();
        int count = UnsafeNativeMethods.ON_BrepArray_Count(ptr);
        return count;
      }
    }

    /// <summary>
    /// Adds a brep to the list.
    /// </summary>
    /// <param name="brep">A brep to add.</param>
    /// <param name="asConst">Whether this brep should be treated as non-modifiable.</param>
    public void Add(Geometry.Brep brep, bool asConst)
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

    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~SimpleArrayBrepPointer()
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
    /// 
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.ON_BrepArray_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }

    /// <summary>
    /// Copies the unmanaged array to a managed counterpart.
    /// </summary>
    /// <returns>The managed array.</returns>
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


#if RHINO_SDK
  /// <summary>
  /// Represents a wrapper to an unmanaged "array" (list) of CRhinoObjRef instances.
  /// <para>Wrapper for a C++ ON_ClassArray of CRhinoObjRef</para>
  /// </summary>
  public sealed class ClassArrayObjRef : IDisposable
  {
    IntPtr m_ptr; // ON_ClassArray<CRhinoObjRef>*

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
    /// Initializes a new <see cref="ClassArrayObjRef"/> instance.
    /// </summary>
    public ClassArrayObjRef()
    {
      m_ptr = UnsafeNativeMethods.ON_ClassArrayCRhinoObjRef_New();
    }

    /// <summary>
    /// Initializes a new instances from a set of ObjRefs
    /// </summary>
    /// <param name="objrefs">An array, a list or any enumerable set of Rhino object references.</param>
    public ClassArrayObjRef(System.Collections.Generic.IEnumerable<Rhino.DocObjects.ObjRef> objrefs)
    {
      m_ptr = UnsafeNativeMethods.ON_ClassArrayCRhinoObjRef_New();
      foreach (var objref in objrefs)
      {
        Add(objref);
      }
    }

    /// <summary>
    /// Gets the number of CRhinoObjRef instances in this array.
    /// </summary>
    public int Count
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_ClassArrayCRhinoObjRef_Count(ptr);
      }
    }

    /// <summary>
    /// Adds an ObjRef to the list.
    /// </summary>
    /// <param name="objref">An ObjRef to add.</param>
    public void Add(Rhino.DocObjects.ObjRef objref)
    {
      if (null != objref)
      {
        IntPtr pConstObjRef = objref.ConstPointer();
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_ClassArrayCRhinoObjRef_Append(pThis, pConstObjRef);
      }
    }
    
    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~ClassArrayObjRef()
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

    void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.ON_ClassArrayCRhinoObjRef_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }

    /// <summary>
    /// Copies the unmanaged array to a managed counterpart.
    /// </summary>
    /// <returns>The managed array.</returns>
    public Rhino.DocObjects.ObjRef[] ToNonConstArray()
    {
      int count = Count;
      if (count < 1)
        return new DocObjects.ObjRef[0];
      IntPtr ptr = ConstPointer();

      Rhino.DocObjects.ObjRef[] rc = new DocObjects.ObjRef[count];
      for (int i = 0; i < count; i++)
      {
        IntPtr pObjRef = UnsafeNativeMethods.ON_ClassArrayCRhinoObjRef_Get(ptr, i);
        if (IntPtr.Zero != pObjRef)
          rc[i] = new DocObjects.ObjRef(pObjRef);
      }
      return rc;
    }
  }


  /// <summary>
  /// Represents a wrapper to an unmanaged "array" (list) of ON_ObjRef instances.
  /// <para>Wrapper for a C++ ON_ClassArray of ON_ObjRef</para>
  /// </summary>
  public sealed class ClassArrayOnObjRef : IDisposable
  {
    IntPtr m_ptr; // ON_ClassArray<ON_ObjRef>*

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
    /// Initializes a new <see cref="ClassArrayOnObjRef"/> instance.
    /// </summary>
    public ClassArrayOnObjRef()
    {
      m_ptr = UnsafeNativeMethods.ON_ClassArrayON_ObjRef_New();
    }

    /// <summary>
    /// Initializes a new instances from a set of ObjRefs
    /// </summary>
    /// <param name="objrefs">An array, a list or any enumerable set of Rhino object references.</param>
    public ClassArrayOnObjRef(System.Collections.Generic.IEnumerable<Rhino.DocObjects.ObjRef> objrefs)
    {
      m_ptr = UnsafeNativeMethods.ON_ClassArrayON_ObjRef_New();
      foreach (var objref in objrefs)
      {
        Add(objref);
      }
    }

    /// <summary>
    /// Gets the number of ObjRef instances in this array.
    /// </summary>
    public int Count
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_ClassArrayON_ObjRef_Count(ptr);
      }
    }

    /// <summary>
    /// Adds an ObjRef to the list.
    /// </summary>
    /// <param name="objref">An ObjRef to add.</param>
    public void Add(DocObjects.ObjRef objref)
    {
      if (null != objref)
      {
        IntPtr ptr_const_objref = objref.ConstPointer();
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.ON_ClassArrayON_ObjRef_Append(ptr_this, ptr_const_objref);
      }
    }

    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~ClassArrayOnObjRef()
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

    void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.ON_ClassArrayON_ObjRef_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }

    /// <summary>
    /// Copies the unmanaged array to a managed counterpart.
    /// </summary>
    /// <returns>The managed array.</returns>
    public DocObjects.ObjRef[] ToNonConstArray()
    {
      int count = Count;
      if (count < 1)
        return new DocObjects.ObjRef[0];
      IntPtr ptr_const_this = ConstPointer();

      DocObjects.ObjRef[] rc = new DocObjects.ObjRef[count];
      for (int i = 0; i < count; i++)
      {
        IntPtr ptr_const_objref = UnsafeNativeMethods.ON_ClassArrayON_ObjRef_Get(ptr_const_this, i);
        if (IntPtr.Zero != ptr_const_objref)
          rc[i] = new DocObjects.ObjRef(ptr_const_objref, false);
      }
      return rc;
    }
  }
#endif
}