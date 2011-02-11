using System;

namespace Rhino.Runtime
{
  [Serializable]
  public class DocumentCollectedException : Exception
  {
  }

  /// <summary>
  /// Base class for .NET classes that wrap C++ unmanaged Rhino classes
  /// </summary>
  public abstract class CommonObject : IDisposable
  {
    long m_unmanaged_memory;  // amount of "memory" pressure reported to the .NET runtime

    IntPtr m_ptr=IntPtr.Zero; // C++ pointer. This is only set when the wrapped pointer is of an
                              // object that has been created in .NET and is not part of the document
                              // m_ptr must have been created outside of the document and must be deleted in Dispose

    internal object m__parent;  // May be a Rhino.DocObject.RhinoObject, Rhino.DocObjects.ObjRef,
                                // Rhino.Render.RenderMesh, PolyCurve 
    internal int m_subobject_index = -1;

    internal Rhino.DocObjects.RhinoObject ParentRhinoObject()
    {
      return m__parent as Rhino.DocObjects.RhinoObject;
    }
    internal void SetParentRhinoObject(Rhino.DocObjects.RhinoObject parent)
    {
      m__parent = parent;
    }

    internal IntPtr ConstPointer()
    {
      if (IntPtr.Zero != m_ptr)
        return m_ptr;

      IntPtr pConstObject = _InternalGetConstPointer();
      if (IntPtr.Zero == pConstObject)
        throw new DocumentCollectedException();

      return pConstObject;
    }

    // returns null if this is NOT a const object
    // returns "parent" object if this IS a const object
    internal virtual object _GetConstObjectParent()
    {
      if (IntPtr.Zero != m_ptr)
        return null;

      return m__parent;
    }

    internal IntPtr NonConstPointer()
    {
      if (IntPtr.Zero == m_ptr && m_subobject_index >= 0 && m__parent!=null)
      {
        Rhino.Geometry.PolyCurve pc = m__parent as Rhino.Geometry.PolyCurve;
        if (pc != null)
        {
          IntPtr pPolyCurve = pc.NonConstPointer();
          IntPtr pThis = UnsafeNativeMethods.ON_PolyCurve_SegmentCurve(pPolyCurve, m_subobject_index);
          return pThis;
        }
      }
      NonConstOperation(); // allows cached data to clean up
      return m_ptr;
    }

    internal abstract IntPtr _InternalGetConstPointer();
    internal abstract IntPtr _InternalDuplicate(out bool applymempressure);

    protected virtual void NonConstOperation()
    {
      if ( IntPtr.Zero==m_ptr )
      {
        bool applymempressure;
        IntPtr pNewPointer = _InternalDuplicate(out applymempressure);
        m_ptr = pNewPointer;
        if (applymempressure)
          ApplyMemoryPressure();

        Rhino.DocObjects.RhinoObject parent_object = m__parent as Rhino.DocObjects.RhinoObject;
        if (null != parent_object)
        {
          if (parent_object.m_original_geometry == this)
          {
            parent_object.m_original_geometry = null;
            parent_object.m_edited_geometry = this as Geometry.GeometryBase;
          }
          if (parent_object.m_original_attributes == this)
          {
            parent_object.m_original_attributes = null;
            parent_object.m_edited_attributes = this as DocObjects.ObjectAttributes;
          }
        }
        OnSwitchToNonConst();
      }
    }

    protected virtual void OnSwitchToNonConst(){}

    /// <summary>
    /// If you want to keep a copy of this class around by holding onto it in a variable after a command
    /// completes, call EnsurePrivateCopy to make sure that this class is not tied to the document. You can
    /// call this function as many times as you want.
    /// </summary>
    public void EnsurePrivateCopy()
    {
      NonConstOperation();
    }

    /// <summary>
    /// If true this object may not be modified. Any properties or functions that attempt
    /// to modify this object when it is set to "IsReadOnly" will throw a NotSupportedException
    /// </summary>
    public virtual bool IsDocumentControlled
    {
      get { return (IntPtr.Zero==m_ptr); }
    }

    /// <summary>
    /// Used for "temporary" wrapping of objects that we don't want .NET to destruct
    /// on disposal
    /// </summary>
    internal void ReleaseNonConstPointer()
    {
      m_ptr = IntPtr.Zero;
    }
    bool m_bDestructOnDispose = true;
    internal void DoNotDestructOnDispose()
    {
      m_bDestructOnDispose = false;
    }

    internal void ConstructNonConstObject(IntPtr nativePointer)
    {
      m_ptr = nativePointer;
    }

    protected void ConstructConstObject(object parentObject, int subobject_index)
    {
      m__parent = parentObject;
      m_subobject_index = subobject_index;
      // We may want to call GC.SuppressFinalize in this situation. This does mean
      // that we would have to tell the GC that the object needs finalization if
      // we in-place copy
    }

    public virtual bool IsValid
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_Object_IsValid(ptr);
      }
    }

    internal void ApplyMemoryPressure()
    {
      if (IntPtr.Zero != m_ptr)
      {
        uint current_size = UnsafeNativeMethods.ON_Object_SizeOf(m_ptr);
        long difference = current_size - m_unmanaged_memory;
        if (difference > 0)
        {
          GC.AddMemoryPressure(difference);
        }
        else if (difference < 0)
        {
          difference = -difference;
          GC.RemoveMemoryPressure(difference);
        }
        m_unmanaged_memory = current_size;
      }
    }

    #region IDisposable implementation
    ~CommonObject()
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
      if (IntPtr.Zero == m_ptr)
        return;

      if (m_bDestructOnDispose)
      {
        UnsafeNativeMethods.ON_Object_Delete(m_ptr);
        if (m_unmanaged_memory > 0)
          GC.RemoveMemoryPressure(m_unmanaged_memory);
      }
      m_ptr = IntPtr.Zero;
    }
    #endregion
  }
}
