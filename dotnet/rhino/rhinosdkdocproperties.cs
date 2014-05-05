#pragma warning disable 1591
using System;
using System.Collections.Generic;

#if RHINO_SDK
namespace Rhino.DocObjects.Tables
{
  /// <summary>
  /// Contains all named construction planes in a rhino document.
  /// <para>This class cannot be inherited.</para>
  /// </summary>
  public sealed class NamedConstructionPlaneTable : IEnumerable<ConstructionPlane>, Collections.IRhinoTable<ConstructionPlane>
  {
    private readonly RhinoDoc m_doc;
    internal NamedConstructionPlaneTable(RhinoDoc doc)
    {
      m_doc = doc;
    }

    /// <summary>Gets the document that owns this table.</summary>
    public RhinoDoc Document
    {
      get { return m_doc; }
    }

    /// <summary>Number of construction planes in the table.</summary>
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
    /// <param name="index">Zero based array index.</param>
    /// <returns>
    /// A construction plane at the index, or null on error.
    /// </returns>
    public ConstructionPlane this[int index]
    {
      get
      {
        IntPtr ptr_construction_plane = UnsafeNativeMethods.CRhinoDocProperties_GetCPlane(m_doc.m_docId, index);
        return ConstructionPlane.FromIntPtr(ptr_construction_plane);
      }
    }

    /// <summary>Finds a named construction plane.</summary>
    /// <param name="name">
    /// Name of construction plane to search for.
    /// </param>
    /// <returns>
    /// &gt;=0 index of the construction plane with the given name.
    /// -1 no construction plane found with the given name.
    /// </returns>
    public int Find(string name)
    {
      return UnsafeNativeMethods.CRhinoDocProperties_FindCPlane(m_doc.m_docId, name);
    }

    /// <summary>
    /// Adds named construction plane to document.
    /// </summary>
    /// <param name="name">
    /// If name is empty, a unique name is automatically created.
    /// If there is already a named onstruction plane with the same name, that 
    /// construction plane is replaced.
    /// </param>
    /// <param name="plane">The plane value.</param>
    /// <returns>
    /// 0 based index of named construction plane.
    /// -1 on failure.
    /// </returns>
    public int Add(string name, Geometry.Plane plane)
    {
      return UnsafeNativeMethods.CRhinoDocProperties_AddCPlane(m_doc.m_docId, name, ref plane);
    }

    /// <summary>
    /// Remove named construction plane from the document.
    /// </summary>
    /// <param name="index">zero based array index.</param>
    /// <returns>true if successful.</returns>
    public bool Delete(int index)
    {
      return UnsafeNativeMethods.CRhinoDocProperties_RemoveCPlane(m_doc.m_docId, index);
    }

    /// <summary>
    /// Remove named construction plane from the document.
    /// </summary>
    /// <param name="name">name of the construction plane.</param>
    /// <returns>true if successful.</returns>
    public bool Delete(string name)
    {
      int index = Find(name);
      return Delete(index);
    }

    #region enumerator
    public IEnumerator<ConstructionPlane> GetEnumerator()
    {
      return new Collections.TableEnumerator<NamedConstructionPlaneTable, ConstructionPlane>(this);
    }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new Collections.TableEnumerator<NamedConstructionPlaneTable, ConstructionPlane>(this);
    }
    #endregion
  }

  /// <summary>
  /// All named views in a rhino document.
  /// </summary>
  public sealed class NamedViewTable : IEnumerable<ViewInfo>, Collections.IRhinoTable<ViewInfo>
  {
    private readonly RhinoDoc m_doc;
    internal NamedViewTable(RhinoDoc doc)
    {
      m_doc = doc;
    }

    /// <summary>Document that owns this table.</summary>
    public RhinoDoc Document
    {
      get { return m_doc; }
    }

    /// <summary>Number of named views in the table.</summary>
    public int Count
    {
      get
      {
        return UnsafeNativeMethods.CRhinoDocProperties_NamedViewCount(m_doc.m_docId);
      }
    }

    /// <summary>
    /// Conceptually, the named view table is an array of ViewInfo and their associated names.
    /// The indexing operator ([] in C#) can be used to get individual ViewInfo items.
    /// </summary>
    /// <param name="index">Zero based array index.</param>
    /// <returns>The view that was found.</returns>
    public ViewInfo this[int index]
    {
      get
      {
        IntPtr ptr_viewinfo = UnsafeNativeMethods.CRhinoDocProperties_GetNamedView(m_doc.m_docId, index);
        if (IntPtr.Zero == ptr_viewinfo)
          return null;
        return new ViewInfo(m_doc, index);
      }
    }

    /// <summary>Finds a named view.</summary>
    /// <param name="name">name to search for.</param>
    /// <returns>
    /// &gt;=0 index of the found named view
    /// -1 no named view found.
    /// </returns>
    public int FindByName(string name)
    {
      return UnsafeNativeMethods.CRhinoDocProperties_FindNamedView(m_doc.m_docId, name);
    }

    /// <summary>
    /// Adds named view to document which is based on an existing viewport.
    /// </summary>
    /// <param name="name">
    /// If name is empty, a unique name is automatically created.
    /// If there is already a named view with the same name, that view is replaced.
    /// </param>
    /// <param name="viewportId">
    /// Id of an existing viewport in the document. View information is copied from this viewport.</param>
    /// <returns>
    /// 0 based index of named view.
    /// -1 on failure.
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
      IntPtr ptr_const_view = view.ConstPointer();
      return UnsafeNativeMethods.CRhinoDocProperties_AddNamedView2(m_doc.m_docId, ptr_const_view);
    }

    /// <summary>Remove named view from the document.</summary>
    /// <param name="index">index of the named view in the named view table.</param>
    /// <returns>true if successful.</returns>
    public bool Delete(int index)
    {
      return UnsafeNativeMethods.CRhinoDocProperties_RemoveNamedView(m_doc.m_docId, index);
    }

    /// <summary>Remove named view from the document.</summary>
    /// <param name="name">name of the view.</param>
    /// <returns>true if successful.</returns>
    public bool Delete(string name)
    {
      int index = FindByName(name);
      return Delete(index);
    }

    /// <summary>
    /// Sets the MainViewport of a standard RhinoView to a named views settings
    /// </summary>
    /// <param name="index"></param>
    /// <param name="view"></param>
    /// <param name="backgroundBitmap"></param>
    /// <returns></returns>
    public bool Restore(int index, Display.RhinoView view, bool backgroundBitmap)
    {
      if (view is Display.RhinoPageView)
        throw new Exception("Use form of Restore that takes a RhinoViewport for layout views");
      return Restore(index, view.MainViewport, backgroundBitmap);
    }

    public bool Restore(int index, Display.RhinoViewport viewport, bool backgroundBitmap)
    {
      IntPtr ptr_const_viewport = viewport.NonConstPointer();
      return UnsafeNativeMethods.RHC_RhinoRestoreNamedView(m_doc.m_docId, index, ptr_const_viewport, backgroundBitmap, 0, 0);
    }

    public bool RestoreAnimated(int index, Display.RhinoView view, bool backgroundBitmap)
    {
      return RestoreAnimated(index, view, backgroundBitmap, 100, 10);
    }

    public bool RestoreAnimated(int index, Display.RhinoView view, bool backgroundBitmap, int frames, int frameRate)
    {
      if (view is Display.RhinoPageView)
        throw new Exception("Use form of RestoreAnimated that takes a RhinoViewport for layout views");
      return RestoreAnimated(index, view.MainViewport, backgroundBitmap, frames, frameRate);
    }

    public bool RestoreAnimated(int index, Display.RhinoViewport viewport, bool backgroundBitmap)
    {
      return RestoreAnimated(index, viewport, backgroundBitmap, 100, 10);
    }

    public bool RestoreAnimated(int index, Display.RhinoViewport viewport, bool backgroundBitmap, int frames, int frameRate)
    {
      IntPtr ptr_const_viewport = viewport.NonConstPointer();
      return UnsafeNativeMethods.RHC_RhinoRestoreNamedView(m_doc.m_docId, index, ptr_const_viewport, backgroundBitmap, frames, frameRate);
    }


    #region enumerator
    public IEnumerator<ViewInfo> GetEnumerator()
    {
      return new Collections.TableEnumerator<NamedViewTable, ViewInfo>(this);
    }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new Collections.TableEnumerator<NamedViewTable, ViewInfo>(this);
    }
    #endregion

  }
}

#endif