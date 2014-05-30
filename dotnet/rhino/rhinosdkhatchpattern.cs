#pragma warning disable 1591
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Rhino.Runtime.InteropWrappers;

namespace Rhino.DocObjects
{
  public enum HatchPatternFillType
  {
    Solid = 0,
    Lines = 1,
    Gradient = 2
  }

  //[Serializable]
  public class HatchPattern : Rhino.Runtime.CommonObject
  {
    #region members
    // Represents both a CRhinoHatchPattern and an ON_HatchPattern. When m_ptr is
    // null, the object uses m_doc and m_id to look up the const
    // CRhinoHatchPattern in the hatch pattern table.
    readonly Guid m_id = Guid.Empty;
#if RHINO_SDK
    readonly RhinoDoc m_doc;
#endif
    #endregion

    #region constructors
    public HatchPattern()
    {
      // Creates a new non-document control ON_HatchPattern
      IntPtr pHP = UnsafeNativeMethods.ON_HatchPattern_New();
      ConstructNonConstObject(pHP);
    }
#if RHINO_SDK
    internal HatchPattern(int index, RhinoDoc doc)
    {
      m_id = UnsafeNativeMethods.CRhinoHatchPatternTable_GetHatchPatternId(doc.m_docId, index);
      m_doc = doc;
      m__parent = m_doc;
    }
#endif
    internal HatchPattern(IntPtr pHatchPattern)
    {
      ConstructNonConstObject(pHatchPattern);
    }

    internal HatchPattern(Guid id, Rhino.FileIO.File3dm parent)
    {
      m_id = id;
      m__parent = parent;
    }

    //// serialization constructor
    //protected HatchPattern(SerializationInfo info, StreamingContext context)
    //  : base (info, context)
    //{
    //}
    #endregion

#if RHINO_SDK
    /// <summary>
    /// Reads hatch pattern definitions from a file.
    /// </summary>
    /// <param name="filename">
    /// Name of an existing file. If filename is null or empty, default hatch pattern filename is used.
    /// </param>
    /// <param name="quiet">
    /// If file doesn't exist, and quiet is false, an error meesage box is shown.
    /// </param>
    /// <returns>An array of hatch patterns. This can be null, but not empty.</returns>
    public static HatchPattern[] ReadFromFile(string filename, bool quiet)
    {
      if (string.IsNullOrEmpty(filename))
        filename = null;
      int count = UnsafeNativeMethods.RHC_RhinoReadHatchPatterns(filename, quiet);
      if (count < 1)
        return null;
      HatchPattern[] rc = new HatchPattern[count];
      for (int i = 0; i < count; i++)
      {
        IntPtr pHatchPattern = UnsafeNativeMethods.RHC_RhinoReadHatchPatterns2(i);
        if (pHatchPattern != IntPtr.Zero)
          rc[i] = new HatchPattern(pHatchPattern);
      }
      return rc;
    }
#endif

    internal override IntPtr _InternalGetConstPointer()
    {
#if RHINO_SDK
      if (m_doc != null)
        return UnsafeNativeMethods.CRhinoHatchPatternTable_GetHatchPatternPointer(m_doc.m_docId, m_id);
#endif
      Rhino.FileIO.File3dm parent_file = m__parent as Rhino.FileIO.File3dm;
      if (parent_file!=null)
      {
        IntPtr pModel = parent_file.NonConstPointer();
        return UnsafeNativeMethods.ONX_Model_GetHatchPatternPointer(pModel, m_id);
      }
      return IntPtr.Zero;
    }

    internal override IntPtr NonConstPointer()
    {
      if (m__parent is Rhino.FileIO.File3dm)
        return _InternalGetConstPointer();

      return base.NonConstPointer();
    }

    internal override IntPtr _InternalDuplicate(out bool applymempressure)
    {
      applymempressure = false;
      IntPtr pConstPointer = ConstPointer();
      return UnsafeNativeMethods.ON_Object_Duplicate(pConstPointer);
    }

    #region properties
#if RHINO_SDK
    const int idxIsDeleted = 0;
    const int idxIsReference = 1;

    /// <summary>
    /// Deleted hatch patterns are kept in the runtime hatch pattern table so that undo
    /// will work with hatch patterns.  Call IsDeleted to determine to determine if
    /// a hatch pattern is deleted.
    /// </summary>
    public bool IsDeleted
    {
      get
      {
        if (!IsDocumentControlled)
          return false;
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.CRhinoHatchPattern_GetBool(pConstThis, idxIsDeleted);
      }
    }

    /// <summary>
    /// Rhino allows multiple files to be viewed simultaneously. Hatch patterns in the
    /// document are "normal" or "reference". Reference hatch patterns are not saved.
    /// </summary>
    public bool IsReference
    {
      get
      {
        if (!IsDocumentControlled)
          return false;
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.CRhinoHatchPattern_GetBool(pConstThis, idxIsReference);
      }
    }
#endif

    /// <summary>
    /// Index in the hatch pattern table for this pattern. -1 if not in the table.
    /// </summary>
    public int Index
    {
      get
      {
        if (!IsDocumentControlled)
          return -1;
        IntPtr pConstThis = ConstPointer();
#if RHINO_SDK
        return UnsafeNativeMethods.CRhinoHatchPattern_GetIndex(pConstThis);
#else
        return UnsafeNativeMethods.ON_HatchPattern_Index(pConstThis);
#endif
      }
    }

    /// <example>
    /// <code source='examples\vbnet\ex_hatchcurve.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_hatchcurve.cs' lang='cs'/>
    /// <code source='examples\py\ex_hatchcurve.py' lang='py'/>
    /// </example>
    public string Name
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        if (IntPtr.Zero == pConstThis)
          return String.Empty;
        using (var sh = new StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.ON_HatchPattern_GetString(pConstThis, pString, true);
          return sh.ToString();
        }
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_HatchPattern_SetString(pThis, value, true);
      }
    }

    public string Description
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        if (IntPtr.Zero == pConstThis)
          return String.Empty;
        using (var sh = new StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.ON_HatchPattern_GetString(pConstThis, pString, false);
          return sh.ToString();
        }
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_HatchPattern_SetString(pThis, value, false);
      }
    }

    public HatchPatternFillType FillType
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return (HatchPatternFillType)UnsafeNativeMethods.ON_HatchPattern_GetFillType(pConstThis);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_HatchPattern_SetFillType(pThis, (int)value);
      }
    }

    #endregion
  }
}

#if RHINO_SDK
namespace Rhino.DocObjects.Tables
{
  /// <summary>
  /// All of the hatch pattern definitions contained in a rhino document.
  /// </summary>
  public sealed class HatchPatternTable : IEnumerable<HatchPattern>, Rhino.Collections.IRhinoTable<HatchPattern>
  {
    private readonly RhinoDoc m_doc;
    internal HatchPatternTable(RhinoDoc doc)
    {
      m_doc = doc;
    }

    /// <summary>Document that owns this table.</summary>
    public RhinoDoc Document
    {
      get { return m_doc; }
    }

    /// <summary>Number of patterns in the table.</summary>
    public int Count
    {
      get
      {
        return UnsafeNativeMethods.CRhinoHatchPatternTable_HatchPatternCount(m_doc.m_docId);
      }
    }

    /// <summary>
    /// Conceptually, the hatch pattern table is an array of hatch patterns.
    /// The operator[] can be used to get individual hatch patterns. A hatch pattern is
    /// either active or deleted and this state is reported by HatchPattern.IsDeleted.
    /// </summary>
    /// <param name="index">zero based array index.</param>
    /// <returns>
    /// If index is out of range, the current hatch pattern is returned.
    /// </returns>
    public DocObjects.HatchPattern this[int index]
    {
      get
      {
        if (index < 0 || index >= Count)
          index = CurrentHatchPatternIndex;
        return new Rhino.DocObjects.HatchPattern(index, m_doc);
      }
    }

    /// <summary>
    /// At all times, there is a "current" hatch pattern.  Unless otherwise
    /// specified, new objects are assigned to the current hatch pattern.
    /// The current hatch pattern is never locked, hidden, or deleted.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_hatchcurve.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_hatchcurve.cs' lang='cs'/>
    /// <code source='examples\py\ex_hatchcurve.py' lang='py'/>
    /// </example>
    public int CurrentHatchPatternIndex
    {
      get
      {
        return UnsafeNativeMethods.CRhinoHatchPatternTable_GetCurrentIndex(m_doc.m_docId);
      }
      set
      {
        UnsafeNativeMethods.CRhinoHatchPatternTable_SetCurrentIndex(m_doc.m_docId, value);
      }
    }

    /// <summary>
    /// Finds the hatch pattern with a given name. Search ignores case.
    /// </summary>
    /// <param name="name">The name of the hatch patter to be found.</param>
    /// <param name="ignoreDeleted">true means don't search deleted hatch patterns.</param>
    /// <returns>Index of the hatch pattern with the given name. -1 if no hatch pattern found.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_hatchcurve.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_hatchcurve.cs' lang='cs'/>
    /// <code source='examples\py\ex_hatchcurve.py' lang='py'/>
    /// </example>
    public int Find(string name, bool ignoreDeleted)
    {
      return UnsafeNativeMethods.CRhinoHatchPatternTable_Find(m_doc.m_docId, name, ignoreDeleted);
    }

    /// <summary>
    /// Adds a new hatch pattern with specified definition to the table.
    /// </summary>
    /// <param name="pattern">
    /// definition of new hatch pattern. The information in pattern is copied.
    /// If patern.Name is empty the a unique name of the form "HatchPattern 01"
    /// will be automatically created.
    /// </param>
    /// <returns>
    /// >=0 index of new hatch pattern
    /// -1  not added because a hatch pattern with that name already exists or
    /// some other problem occured.
    /// </returns>
    public int Add(Rhino.DocObjects.HatchPattern pattern)
    {
      if (null == pattern)
        return -1;
      IntPtr pPattern = pattern.ConstPointer();
      return UnsafeNativeMethods.CRhinoHatchPatternTable_AddPattern(m_doc.m_docId, pPattern, false);
    }

    #region enumerator

    // for IEnumerable<Layer>
    public IEnumerator<HatchPattern> GetEnumerator()
    {
      return new Rhino.Collections.TableEnumerator<HatchPatternTable, HatchPattern>(this);
    }

    // for IEnumerable
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new Rhino.Collections.TableEnumerator<HatchPatternTable, HatchPattern>(this);
    }

    #endregion
  }
}
#endif