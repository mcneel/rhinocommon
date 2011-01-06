using System;
using System.Runtime.InteropServices;

namespace Rhino.DocObjects
{
  public class Linetype : Rhino.Runtime.CommonObject
  {
    #region members
    // Represents both a CRhinoLinetype and an ON_Linetype. When m_ptr is
    // null, the object uses m_doc and m_id to look up the const
    // CRhinoLinetype in the linetype table.
    Rhino.RhinoDoc m_doc;
    Guid m_id=Guid.Empty;
    #endregion

    #region constructors
    public Linetype()
    {
      // Creates a new non-document control ON_Linetype
      IntPtr pLinetype = UnsafeNativeMethods.ON_Linetype_New();
      base.ConstructNonConstObject(pLinetype);
    }

    internal Linetype(int index, Rhino.RhinoDoc doc)
    {
      m_id = UnsafeNativeMethods.CRhinoLinetypeTable_GetLinetypeId(doc.m_docId, index);
      m_doc = doc;
      this.m__parent = m_doc;
    }
    #endregion

    public bool CommitChanges()
    {
      if (m_id == Guid.Empty || IsDocumentControlled)
        return false;
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoLinetypeTable_CommitChanges(m_doc.m_docId, pThis, m_id);
    }

    internal override IntPtr _InternalGetConstPointer()
    {
      if (m_doc != null)
        return UnsafeNativeMethods.CRhinoLinetypeTable_GetLinetypePointer2(m_doc.m_docId, m_id);
      return IntPtr.Zero;
    }

    internal override IntPtr _InternalDuplicate(out bool applymempressure)
    {
      applymempressure = false;
      IntPtr pConstPointer = ConstPointer();
      return UnsafeNativeMethods.ON_Object_Duplicate(pConstPointer);
    }

    #region properties
    /// <summary>The name of this linetype</summary>
    public string Name
    {
      get
      {
        using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
        {
          IntPtr pLinetype = ConstPointer();
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.ON_Linetype_GetLinetypeName(pLinetype, pString);
          return sh.ToString();
        }
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Linetype_SetLinetypeName(pThis, value);
      }
    }

    /// <summary>The index of this linetype.</summary>
    public int LinetypeIndex
    {
      get { return GetInt(idxLinetypeIndex); }
      set { SetInt(idxLinetypeIndex, value); }
    }

    /// <summary>Total length of one repeat of the pattern</summary>
    public double PatternLength
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_Linetype_PatternLength(pConstThis);
      }
    }

    /// <summary>Number of segments in the pattern</summary>
    public int SegmentCount
    {
      get { return GetInt(idxSegmentCount); }
    }

    /// <summary>
    /// Gets the ID of this linetype object.
    /// </summary>
    public Guid Id
    {
      get
      {
        IntPtr pLinetype = ConstPointer();
        return UnsafeNativeMethods.ON_Linetype_GetGuid(pLinetype);
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.ON_Linetype_SetGuid(ptr, value);
      }
    }

    const int idxLinetypeIndex = 0;
    const int idxSegmentCount = 1;
    int GetInt(int which)
    {
      IntPtr pConstLinetype = ConstPointer();
      return UnsafeNativeMethods.ON_Linetype_GetInt(pConstLinetype, which);
    }
    void SetInt(int which, int val)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.ON_Linetype_SetInt(ptr, which, val);
    }
    #endregion
  }
}

namespace Rhino.DocObjects.Tables
{
//  public class LineTypeTable { }
}
