using System;
using System.Collections.Generic;

namespace Rhino.DocObjects.Tables
{
  /// <summary>
  /// All named construction planes in a rhino document
  /// </summary>
  public sealed class NamedConstructionPlaneTable : IEnumerable<ConstructionPlane>, IDocObjectTable<ConstructionPlane>
  {
    private readonly RhinoDoc m_doc;
    private NamedConstructionPlaneTable() { }
    internal NamedConstructionPlaneTable(RhinoDoc doc)
    {
      m_doc = doc;
    }

    /// <summary>Document that owns this table</summary>
    public RhinoDoc Document
    {
      get { return m_doc; }
    }

    /// <summary>Number of construction planes in the table</summary>
    public int Count
    {
      get
      {
        return UnsafeNativeMethods.CRhinoDocProperties_CPlaneCount(m_doc.m_docId);
      }
    }

    /// <summary>
    /// Conceptually, the named construction plane table is an array of ConstructionPlanes
    /// and their associated names. The operator[] can be used to get individual ConstructionPlanes.
    /// </summary>
    /// <param name="index">zero based array index</param>
    /// <returns>
    /// </returns>
    public DocObjects.ConstructionPlane this[int index]
    {
      get
      {
        IntPtr pConstructionPlane = UnsafeNativeMethods.CRhinoDocProperties_GetCPlane(m_doc.m_docId, index);
        return DocObjects.ConstructionPlane.FromIntPtr(pConstructionPlane);
      }
    }

    /// <summary>Find a named construction plane</summary>
    /// <param name="name">
    /// Name of construction plane to search for.
    /// </param>
    /// <returns>
    /// &gt;=0 index of the construction plane with the given name.
    /// -1 no construction plane found with the given name
    /// </returns>
    public int Find(string name)
    {
      return UnsafeNativeMethods.CRhinoDocProperties_FindCPlane(m_doc.m_docId, name);
    }

    /// <summary>
    /// Add named construction plane to document.
    /// </summary>
    /// <param name="name">
    /// If name is empty, a unique name is automatically created.
    /// If there is already a named onstruction plane with the same name, that 
    /// construction plane is replaced.
    /// </param>
    /// <param name="plane"></param>
    /// <returns>
    /// 0 based index of named construction plane.
    /// -1 on failure
    /// </returns>
    public int Add(string name, Rhino.Geometry.Plane plane)
    {
      return UnsafeNativeMethods.CRhinoDocProperties_AddCPlane(m_doc.m_docId, name, ref plane);
    }

    /// <summary>
    /// Remove named construction plane from the document
    /// </summary>
    /// <param name="index">zero based array index</param>
    /// <returns>true if successful</returns>
    public bool Delete(int index)
    {
      return UnsafeNativeMethods.CRhinoDocProperties_RemoveCPlane(m_doc.m_docId, index);
    }

    /// <summary>
    /// Remove named construction plane from the document
    /// </summary>
    /// <param name="name">name of the construction plane</param>
    /// <returns>true if successful</returns>
    public bool Delete(string name)
    {
      int index = Find(name);
      return Delete(index);
    }

    #region enumerator
    public IEnumerator<ConstructionPlane> GetEnumerator()
    {
      return new TableEnumerator<NamedConstructionPlaneTable, ConstructionPlane>(this);
    }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new TableEnumerator<NamedConstructionPlaneTable, ConstructionPlane>(this);
    }
    #endregion
  }

  /// <summary>
  /// All named views in a rhino document
  /// </summary>
  public sealed class NamedViewTable : IEnumerable<ViewInfo>, IDocObjectTable<ViewInfo>
  {
    private readonly RhinoDoc m_doc;
    private NamedViewTable() { }
    internal NamedViewTable(RhinoDoc doc)
    {
      m_doc = doc;
    }

    /// <summary>Document that owns this table</summary>
    public RhinoDoc Document
    {
      get { return m_doc; }
    }

    /// <summary>Number of named views in the table</summary>
    public int Count
    {
      get
      {
        return UnsafeNativeMethods.CRhinoDocProperties_NamedViewCount(m_doc.m_docId);
      }
    }

    /// <summary>
    /// Conceptually, the named view table is an array of ViewInfo and their associated names.
    /// The operator[] can be used to get individual ViewInfo items.
    /// </summary>
    /// <param name="index">zero based array index</param>
    /// <returns></returns>
    public DocObjects.ViewInfo this[int index]
    {
      get
      {
        IntPtr pViewInfo = UnsafeNativeMethods.CRhinoDocProperties_GetNamedView(m_doc.m_docId, index);
        if (IntPtr.Zero == pViewInfo)
          return null;
        return new Rhino.DocObjects.ViewInfo(m_doc, index);
      }
    }

    [Obsolete("Use FindByName instead - this will be removed in a future WIP")]
    public int Find(string name)
    {
      return FindByName(name);
    }

    /// <summary>Find a named view</summary>
    /// <param name="name">name to search for</param>
    /// <returns>
    /// &gt;=0 index of the found named view
    /// -1 no named view found
    /// </returns>
    public int FindByName(string name)
    {
      return UnsafeNativeMethods.CRhinoDocProperties_FindNamedView(m_doc.m_docId, name);
    }

    /// <summary>
    /// Add named view to document which is based on an existing viewport
    /// </summary>
    /// <param name="name">
    /// If name is empty, a unique name is automatically created.
    /// If there is already a named view with the same name, that view is replaced.
    /// </param>
    /// <param name="viewportId">
    /// Id of an existing viewport in the document. View information is copied from this viewport</param>
    /// <returns>
    /// 0 based index of named view.
    /// -1 on failure
    /// </returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addnamedview.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addnamedview.cs' lang='cs'/>
    /// <code source='examples\py\ex_addnamedview.py' lang='py'/>
    /// </example>
    public int Add(string name, Guid viewportId)
    {
      return UnsafeNativeMethods.CRhinoDocProperties_AddNamedView(m_doc.m_docId, name, viewportId);
    }

    public int Add(ViewInfo view)
    {
      IntPtr pConstView = view.ConstPointer();
      return UnsafeNativeMethods.CRhinoDocProperties_AddNamedView2(m_doc.m_docId, pConstView);
    }

    /// <summary>Remove named view from the document</summary>
    /// <param name="index">index of the named view in the named view table</param>
    /// <returns>true if successful</returns>
    public bool Delete(int index)
    {
      return UnsafeNativeMethods.CRhinoDocProperties_RemoveNamedView(m_doc.m_docId, index);
    }

    /// <summary>Remove named view from the document</summary>
    /// <param name="name">name of the view</param>
    /// <returns>true if successful</returns>
    public bool Delete(string name)
    {
      int index = FindByName(name);
      return Delete(index);
    }

    public bool Restore(int index, Rhino.Display.RhinoView viewport, bool backgroundBitmap)
    {
      IntPtr pConstViewport = viewport.NonConstPointer();
      return UnsafeNativeMethods.RHC_RhinoRestoreNamedView(m_doc.m_docId, index, pConstViewport, backgroundBitmap, 0, 0);
    }

    public bool RestoreAnimated(int index, Rhino.Display.RhinoView viewport, bool backgroundBitmap)
    {
      return RestoreAnimated(index, viewport, backgroundBitmap, 100, 10);
    }

    public bool RestoreAnimated(int index, Rhino.Display.RhinoView viewport, bool backgroundBitmap, int frames, int frameRate)
    {
      IntPtr pConstViewport = viewport.NonConstPointer();
      return UnsafeNativeMethods.RHC_RhinoRestoreNamedView(m_doc.m_docId, index, pConstViewport, backgroundBitmap, frames, frameRate);
    }

    #region enumerator
    public IEnumerator<ViewInfo> GetEnumerator()
    {
      return new TableEnumerator<NamedViewTable, ViewInfo>(this);
    }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new TableEnumerator<NamedViewTable, ViewInfo>(this);
    }
    #endregion

  }
}
