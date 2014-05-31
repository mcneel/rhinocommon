#pragma warning disable 1591
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Rhino.Runtime.InteropWrappers;

namespace Rhino.DocObjects
{
  public enum DimensionStyleArrowType : int
  {
    /// <summary>2:1</summary>
    SolidTriangle = 0,
    /// <summary></summary>
    Dot = 1,
    /// <summary></summary>
    Tick = 2,
    /// <summary>1:1</summary>
    ShortTriangle = 3,
    /// <summary></summary>
    Arrow = 4,
    /// <summary></summary>
    Rectangle = 5,
    /// <summary>4:1</summary>
    LongTriangle = 6,
    /// <summary>6:1</summary>
    LongerTriangle = 7
  }

  //[Serializable]
  public class DimensionStyle : Rhino.Runtime.CommonObject
  {
    // Represents both a CRhinoDimStyle and an ON_DimStyle. When m_ptr
    // is null, the object uses m_doc and m_id to look up the const
    // CRhinoDimStyle in the dimstyle table.
#if RHINO_SDK
    readonly Rhino.RhinoDoc m_doc;
#endif
    readonly Guid m_id=Guid.Empty;

    public DimensionStyle()
    {
      // Creates a new non-document control ON_DimStyle
      IntPtr pOnDimStyle = UnsafeNativeMethods.ON_DimStyle_New();
      ConstructNonConstObject(pOnDimStyle);
    }

#if RHINO_SDK
    internal DimensionStyle(int index, Rhino.RhinoDoc doc)
    {
      m_id = UnsafeNativeMethods.CRhinoDimStyleTable_GetGuid(doc.m_docId, index);
      m_doc = doc;
      m__parent = m_doc;
    }
#endif

    internal DimensionStyle(Guid id, Rhino.FileIO.File3dm parent)
    {
      m_id = id;
      m__parent = parent;
    }

    //// serialization constructor
    //protected DimensionStyle(SerializationInfo info, StreamingContext context)
    //  : base (info, context)
    //{
    //}


    public bool CommitChanges()
    {
#if RHINO_SDK
      if (m_id == Guid.Empty || IsDocumentControlled)
        return false;
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoDimStyleTable_CommitChanges(m_doc.m_docId, pThis, m_id);
#else
      return true;
#endif
    }

    internal override IntPtr _InternalGetConstPointer()
    {
#if RHINO_SDK
      if (m_doc != null)
        return UnsafeNativeMethods.CRhinoDimStyleTable_GetDimStylePointer(m_doc.m_docId, m_id);
#endif

      FileIO.File3dm file_parent = m__parent as FileIO.File3dm;
      if( file_parent!=null )
      {
        IntPtr pConstParent = file_parent.ConstPointer();
        return UnsafeNativeMethods.ONX_Model_GetDimStylePointer(pConstParent, m_id);
      }
      return IntPtr.Zero;
    }

    internal override IntPtr NonConstPointer()
    {
      FileIO.File3dm file_parent = m__parent as FileIO.File3dm;
      if (file_parent != null)
      {
        IntPtr pConstParent = file_parent.ConstPointer();
        return UnsafeNativeMethods.ONX_Model_GetDimStylePointer(pConstParent, m_id);
      }

      return base.NonConstPointer();
    }

    internal override IntPtr _InternalDuplicate(out bool applymempressure)
    {
      applymempressure = false;
      IntPtr pConstPointer = ConstPointer();
      return UnsafeNativeMethods.ON_Object_Duplicate(pConstPointer);
    }

    public Guid Id
    {
      get
      {
        if (m_id != Guid.Empty)
          return m_id;
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_DimStyle_ModelObjectId(pConstThis);
      }
    }

    public string Name
    {
      get
      {
        using (var sh = new StringHolder())
        {
          IntPtr pStringHolder = sh.NonConstPointer();
          IntPtr pConstThis = ConstPointer();
          UnsafeNativeMethods.ON_DimStyle_Name(pConstThis, pStringHolder);
          return sh.ToString();
        }
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_DimStyle_SetName(pThis, value);
      }
    }
    
    public int Index
    {
      get
      {
        IntPtr pThis = ConstPointer();
        return UnsafeNativeMethods.ON_DimStyle_GetIndex(pThis);
      }
    }

    #region double properties
    const int idxExtensionLineExtension = 0;
    const int idxExtensionLineOffset = 1;
    const int idxArrowSize = 2;
    const int idxLeaderArrowSize = 3;
    const int idxCenterMark = 4;
    const int idxTextGap = 5;
    const int idxTextHeight = 6;
    const int idxLengthFactor = 7;
    const int idxAlternateLengthFactor = 8;

    double GetDouble(int which)
    {
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.ON_DimStyle_GetDouble(pConstThis, which);
    }
    void SetDouble(int which, double val)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_DimStyle_SetDouble(pThis, which, val);
    }

    public double ExtensionLineExtension
    {
      get { return GetDouble(idxExtensionLineExtension); }
      set { SetDouble(idxExtensionLineExtension, value); }
    }

    public double ExtensionLineOffset
    {
      get { return GetDouble(idxExtensionLineOffset); }
      set { SetDouble(idxExtensionLineOffset, value); }
    }

    public double ArrowLength
    {
      get { return GetDouble(idxArrowSize); }
      set { SetDouble(idxArrowSize, value); }
    }

    public double LeaderArrowLength
    {
      get { return GetDouble(idxLeaderArrowSize); }
      set { SetDouble(idxLeaderArrowSize, value); }
    }

    public double CenterMarkSize
    {
      get { return GetDouble(idxCenterMark); }
      set { SetDouble(idxCenterMark, value); }
    }

    public double TextGap
    {
      get { return GetDouble(idxTextGap); }
      set { SetDouble(idxTextGap, value); }
    }

    public double TextHeight
    {
      get { return GetDouble(idxTextHeight); }
      set { SetDouble(idxTextHeight, value); }
    }

    public double LengthFactor
    {
      get { return GetDouble(idxLengthFactor); }
      set { SetDouble(idxLengthFactor, value); }
    }

    public double AlternateLengthFactor
    {
      get { return GetDouble(idxAlternateLengthFactor); }
      set { SetDouble(idxAlternateLengthFactor, value); }
    }
    #endregion

    #region int properties
    const int idxAngleResolution = 0;
    const int idxFontIndex = 1;
    const int idxLengthResolution = 2;
    const int idxLengthFormat = 3;
    const int idxTextAlignment = 4;
    const int idxArrowType = 5;
    const int idxLeaderArrowType = 6;

    int GetInt(int which)
    {
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.ON_DimStyle_GetInt(pConstThis, which);
    }
    void SetInt(int which, int val)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_DimStyle_SetInt(pThis, which, val);
    }

    public TextDisplayAlignment TextAlignment
    {
      get
      {
        int rc = GetInt(idxTextAlignment);
        return (TextDisplayAlignment)Enum.ToObject(typeof(TextDisplayAlignment), rc);
      }
      set { SetInt(idxTextAlignment, (int)value); }
    }

    public int AngleResolution
    {
      get { return GetInt(idxAngleResolution); }
      set { SetInt(idxAngleResolution, value); }
    }

    /// <summary>Linear display precision.</summary>
    public int LengthResolution
    {
      get { return GetInt(idxLengthResolution); }
      set { SetInt(idxLengthResolution, value); }
    }

    public int FontIndex
    {
      get { return GetInt(idxFontIndex); }
      set { SetInt(idxFontIndex, value); }
    }

    public DistanceDisplayMode LengthFormat
    {
      get
      {
        int rc = GetInt(idxLengthFormat);
        return (DistanceDisplayMode)Enum.ToObject(typeof(DistanceDisplayMode), rc);
      }
      set { SetInt(idxLengthFormat, (int)value); }
    }

    public DimensionStyleArrowType ArrowType
    {
      get
      {
        int rc = GetInt(idxArrowType);
        return (DimensionStyleArrowType)Enum.ToObject(typeof(DimensionStyleArrowType), rc);
      }
      set { SetInt(idxArrowType, (int)value); }
    }

    public DimensionStyleArrowType LeaderArrowType
    {
      get
      {
        int rc = GetInt(idxLeaderArrowType);
        return (DimensionStyleArrowType)Enum.ToObject(typeof(DimensionStyleArrowType), rc);
      }
      set { SetInt(idxLeaderArrowType, (int)value); }
    }

    #endregion

    public string Prefix
    {
      get
      {
        using (var sh = new StringHolder())
        {
          IntPtr pConstThis = ConstPointer();
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.ON_DimStyle_GetString(pConstThis, pString, true);
          return sh.ToString();
        }
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_DimStyle_SetString(pThis, value, true);
      }
    }

    public string Suffix
    {
      get
      {
        using (var sh = new StringHolder())
        {
          IntPtr pConstThis = ConstPointer();
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.ON_DimStyle_GetString(pConstThis, pString, false);
          return  sh.ToString();
        }
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_DimStyle_SetString(pThis, value, false);
      }
    }

    public bool IsReference
    {
      get
      {
#if RHINO_SDK
        if (m_doc == null || m_id == Guid.Empty)
          return false;
        int index = Index;
        return UnsafeNativeMethods.CRhinoDimStyle_IsReference(m_doc.m_docId, index);
#else
        return false;
#endif
      }
    }

  }
}

#if RHINO_SDK
namespace Rhino.DocObjects.Tables
{
  public sealed class DimStyleTable : IEnumerable<DimensionStyle>, Rhino.Collections.IRhinoTable<DimensionStyle>
  {
    private readonly RhinoDoc m_doc;
    internal DimStyleTable(RhinoDoc doc)
    {
      m_doc = doc;
    }

    /// <summary>Document that owns this dimstyle table.</summary>
    public RhinoDoc Document
    {
      get { return m_doc; }
    }

    /// <summary>Number of dimstyles in the table.</summary>
    public int Count
    {
      get
      {
        return UnsafeNativeMethods.CRhinoDimStyleTable_DimStyleCount(m_doc.m_docId);
      }
    }

    public Rhino.DocObjects.DimensionStyle this[int index]
    {
      get
      {
        if (index < 0 || index >= Count)
          return null;
        return new Rhino.DocObjects.DimensionStyle(index, m_doc);
      }
    }

    /// <summary>
    /// Adds a new dimension style to the document. The new dimension style will be initialized
    /// with the current default dimension style properties.
    /// </summary>
    /// <param name="name">
    /// Name of the new dimension style. If null or empty, Rhino automatically generates the name.
    /// </param>
    /// <returns>index of new dimension style.</returns>
    public int Add(string name)
    {
      return Add(name, false);
    }

    /// <summary>
    /// Adds a new dimension style to the document. The new dimension style will be initialized
    /// with the current default dimension style properties.
    /// </summary>
    /// <param name="name">
    /// Name of the new dimension style. If null or empty, Rhino automatically generates the name.
    /// </param>
    /// <param name="reference">if true the dimstyle will not be saved in files.</param>
    /// <returns>index of new dimension style.</returns>
    public int Add(string name, bool reference)
    {
      return UnsafeNativeMethods.CRhinoDimStyleTable_Add(m_doc.m_docId, name, reference);
    }

    /// <example>
    /// <code source='examples\vbnet\ex_dimstyle.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_dimstyle.cs' lang='cs'/>
    /// <code source='examples\py\ex_dimstyle.py' lang='py'/>
    /// </example>
    public int CurrentDimensionStyleIndex
    {
      get
      {
        return UnsafeNativeMethods.CRhinoDimStyleTable_CurrentDimStyleIndex(m_doc.m_docId);
      }
    }

    public Rhino.DocObjects.DimensionStyle CurrentDimensionStyle
    {
      get
      {
        return new DimensionStyle(CurrentDimensionStyleIndex, m_doc);
      }
    }

    public bool SetCurrentDimensionStyleIndex(int index, bool quiet)
    {
      return UnsafeNativeMethods.CRhinoDimStyleTable_SetCurrentDimStyleIndex(m_doc.m_docId, index, quiet);
    }

    public Rhino.DocObjects.DimensionStyle Find(string name, bool ignoreDeletedDimensionStyles)
    {
      int rc = UnsafeNativeMethods.CRhinoDimStyleTable_FindDimStyle(m_doc.m_docId, name, ignoreDeletedDimensionStyles);
      if (rc < 0)
        return null;
      return new DimensionStyle(rc, m_doc);
    }

    public string GetUnusedDimensionStyleName()
    {
      using (var sh = new StringHolder())
      {
        IntPtr pStringHolder = sh.NonConstPointer();
        UnsafeNativeMethods.CRhinoDimStyleTable_GetUnusedDimensionStyleName(m_doc.m_docId, pStringHolder);
        return sh.ToString();
      }
    }

    public bool DeleteDimensionStyle(int index, bool quiet)
    {
      return UnsafeNativeMethods.CRhinoDimStyleTable_DeleteDimStyle(m_doc.m_docId, index, quiet);
    }

    #region enumerator

    // for IEnumerable<Layer>
    public IEnumerator<DimensionStyle> GetEnumerator()
    {
      return new Rhino.Collections.TableEnumerator<DimStyleTable, DimensionStyle>(this);
    }

    // for IEnumerable
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new Rhino.Collections.TableEnumerator<DimStyleTable, DimensionStyle>(this);
    }

    #endregion

  }
}
#endif