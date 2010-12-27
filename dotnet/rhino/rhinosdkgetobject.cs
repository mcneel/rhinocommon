using System;
using System.Runtime.InteropServices;
using Rhino.Geometry;
using Rhino.DocObjects;

namespace Rhino.Input.Custom
{
  /// <summary>
  /// The GetObject class is the tool commands use to interactively select objects.
  /// </summary>
  /// <example>
  /// GetObject go = new GetObject();
  /// go.GetObjects(1,0);
  /// if( go.CommandResult() != Command.Result.Success )
  ///    ... use canceled or some other type of input was provided
  /// int object_count = go.ObjectCount();
  /// for( int i=0; i&lt;object_count; i++ )
  /// {
  ///   ObjectReference objref = go.Object(i);
  ///   ON_Geometry geo = objref.Geometry();
  ///   ...
  /// }
  /// </example>
  public class GetObject : GetBaseClass
  {
    /// <example>
    /// <code source='examples\vbnet\ex_addobjectstogroup.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addobjectstogroup.cs' lang='cs'/>
    /// <code source='examples\py\ex_addobjectstogroup.py' lang='py'/>
    /// </example>
    public GetObject()
    {
      IntPtr ptr = UnsafeNativeMethods.CRhinoGetObject_New();
      Construct(ptr);
    }

    /// <summary>
    /// The geometry type filter controls which types of geometry
    /// (points, curves, surfaces, meshes, etc.) can be selected.
    /// The default geometry type filter permits selection of all
    /// types of geometry.
    /// NOTE: the filter can be a bitwise combination of multiple ObjectTypes
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_orientonsrf.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_orientonsrf.cs' lang='cs'/>
    /// <code source='examples\py\ex_orientonsrf.py' lang='py'/>
    /// </example>
    [CLSCompliant(false)]
    public ObjectType GeometryFilter
    {
      get
      {
        IntPtr ptr = NonConstPointer();
        uint rc = UnsafeNativeMethods.CRhinoGetObject_GetSetGeometryFilter(ptr, false, 0);
        return (ObjectType)rc;
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.CRhinoGetObject_GetSetGeometryFilter(ptr, true, (uint)value);
      }
    }

    /// <summary>
    /// The geometry attribute filter provides a secondary filter that
    /// can be used to restrict which objects can be selected. Control
    /// of the type of geometry (points, curves, surfaces, meshes, etc.)
    /// is provided by GetObject.SetGeometryFilter. The geometry attribute
    /// filter is used to require the selected geometry to have certain
    /// attributes (open, closed, etc.). The default attribute filter
    /// permits selection of all types of geometry.
    /// </summary>
    [CLSCompliant(false)]
    public GeometryAttributeFilter GeometryAttributeFilter
    {
      get
      {
        IntPtr ptr = NonConstPointer();
        uint filter = UnsafeNativeMethods.CRhinoGetObject_GetSetGeometryAttrFilter(ptr, false, 0);
        return (GeometryAttributeFilter)filter;
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.CRhinoGetObject_GetSetGeometryAttrFilter(ptr, true, (uint)value);
      }
    }

    /// <summary>
    /// Checks geometry to see if it can be selected.
    /// Override to provide fancy filtering.
    /// </summary>
    /// <param name="rhObject">parent object being considered</param>
    /// <param name="geometry">geometry being considered</param>
    /// <param name="componentIndex">
    /// if >= 0, geometry is a proper sub-part of object->Geometry() with componentIndex
    /// </param>
    /// <returns>The default always returns true</returns>
    public virtual bool CustomGeometryFilter( DocObjects.RhinoObject rhObject, GeometryBase geometry, ComponentIndex componentIndex )
    {
      return true;
    }

    /// <summary>
    /// Checks geometry to see if it passes the basic GeometryAttributeFilter.
    /// </summary>
    /// <param name="rhObject">parent object being considered</param>
    /// <param name="geometry">geometry being considered</param>
    /// <param name="componentIndex">if >= 0, geometry is a proper sub-part of object->Geometry() with componentIndex.</param>
    /// <returns>
    /// true if the geometry passes the filter returned by GeometryAttributeFilter().
    /// </returns>
    public bool PassesGeometryAttributeFilter(DocObjects.RhinoObject rhObject, GeometryBase geometry, ComponentIndex componentIndex)
    {
      IntPtr pRhinoObject = IntPtr.Zero;
      if (rhObject != null)
        pRhinoObject = rhObject.ConstPointer();
      IntPtr pGeometry = IntPtr.Zero;
      if (geometry != null)
        pGeometry = geometry.ConstPointer();
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.CRhinoGetObject_PassesGeometryAttributeFilter(ptr, pRhinoObject, pGeometry, componentIndex);
    }

    /// <summary>
    /// Control the pre selection behavior GetObjects.
    /// </summary>
    /// <param name="enable">if true, pre-selection is enabled</param>
    /// <param name="ignoreUnacceptablePreselectedObjects">
    /// If true and some acceptable objects are pre-selected, then any unacceptable
    /// pre-selected objects are ignored. If false and any unacceptable are pre-selected,
    /// then the user is forced to post-select.
    /// </param>
    /// <remarks>
    /// By default, if valid input is pre-selected when GetObjects() is called, then that input
    /// is returned and the user is not given the opportunity to post-select. If you want
    /// to force the user to post-select, then call EnablePreSelect(false).
    /// </remarks>
    public void EnablePreSelect(bool enable, bool ignoreUnacceptablePreselectedObjects)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.CRhinoGetObject_EnablePreSelect(ptr, enable, ignoreUnacceptablePreselectedObjects);
    }
    public void DisablePreSelect()
    {
      EnablePreSelect(false, true);
    }

    const int idxEnablePostSelect            = 0;
    const int idxDeselectAllBeforePostSelect = 1;
    const int idxOneByOnePostSelect          = 2;
    const int idxSubObjectSelect             = 3;
    const int idxChooseOneQuestion           = 4;
    const int idxBottomObjectPreference      = 5;
    const int idxGroupSelect                 = 6;
    const int idxSelPrev                     = 7;
    const int idxHighlight                   = 8;
    const int idxReferenceObjectSelect       = 9;
    const int idxIgnoreGrips                 = 10;
    const int idxPressEnterWhenDonePrompt    = 11;
    const int idxAlreadySelectedObjectSelect = 12;
    const int idxObjectsWerePreselected      = 13;
    const int idxClearObjectsOnEntry         = 14;
    const int idxUnselectObjectsOnExit       = 15;
    bool GetBool(int which)
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.CRhinoGetObject_GetSetBool(ptr, which, false, false);
    }
    void SetBool(int which, bool set_val)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.CRhinoGetObject_GetSetBool(ptr, which, true, set_val);
    }

    /// <summary>
    /// Control the availability of post selection in GetObjects.
    /// </summary>
    /// <remarks>
    /// By default, if no valid input is pre-selected when GetObjects is called, then
    /// the user is given the chance to post select. If you want to force the user to pre-select,
    /// then call EnablePostSelect(false).
    /// </remarks>
    public void EnablePostSelect( bool enable )
    {
      SetBool(idxEnablePostSelect, enable);
    }

    /// <summary>
    /// True if pre-selected input will be deselected before
    /// post-selection begins when no pre-selected input is valid.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_orientonsrf.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_orientonsrf.cs' lang='cs'/>
    /// <code source='examples\py\ex_orientonsrf.py' lang='py'/>
    /// </example>
    public bool DeselectAllBeforePostSelect
    {
      get { return GetBool(idxDeselectAllBeforePostSelect); }
      set { SetBool(idxDeselectAllBeforePostSelect, value); }
    }

    /// <summary>
    /// In one-by-one post selection, the user is forced
    /// to select objects by post picking them one at a time.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_orientonsrf.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_orientonsrf.cs' lang='cs'/>
    /// <code source='examples\py\ex_orientonsrf.py' lang='py'/>
    /// </example>
    public bool OneByOnePostSelect
    {
      get { return GetBool(idxOneByOnePostSelect); }
      set { SetBool(idxOneByOnePostSelect, value); }
    }

    /// <summary>
    /// By default, GetObject.Input will permit a user to select
    /// sub-objects (like a curve in a b-rep or a curve in a group).
    /// If you only want the user to select "top" level objects,
    /// then call EnableSubObjectSelect = false.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_orientonsrf.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_orientonsrf.cs' lang='cs'/>
    /// <code source='examples\py\ex_orientonsrf.py' lang='py'/>
    /// </example>
    public bool SubObjectSelect
    {
      get { return GetBool(idxSubObjectSelect); }
      set { SetBool(idxSubObjectSelect, value); }
    }

    /// <summary>
    /// By default, if a call to Input is permitted to select different parts
    /// of the same object, like a polysurface and an edge of that polysurface,
    /// then the top-most object is automatically selected. If you want the
    /// choose-one-object mechanism to include pop up in these cases, then call
    /// EnableChooseOneQuestion = true before calling GetObjects().
    /// </summary>
    public bool ChooseOneQuestion
    {
      get { return GetBool(idxChooseOneQuestion); }
      set { SetBool(idxChooseOneQuestion, value); }
    }

    /// <summary>
    /// By default, if a call to Input is permitted to select different parts of
    /// the same object, like a polysurface, a surface and an edge, then the
    /// top-most object is prefered. (polysurface beats face beats edge). If
    /// you want the bottom most object to be prefered, then call 
    /// EnableBottomObjectPreference = true before calling GetObjects().
    /// </summary>
    public bool BottomObjectPreference
    {
      get { return GetBool(idxBottomObjectPreference); }
      set { SetBool(idxBottomObjectPreference, value); }
    }

    /// <summary>
    /// By default, groups are ignored in GetObject. If you want your call to
    /// GetObjects() to select every object in a group that has any objects
    /// selected, then enable group selection.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_orientonsrf.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_orientonsrf.cs' lang='cs'/>
    /// <code source='examples\py\ex_orientonsrf.py' lang='py'/>
    /// </example>
    public bool GroupSelect
    {
      get { return GetBool(idxGroupSelect); }
      set { SetBool(idxGroupSelect, value); }
    }

    /// <summary>
    /// By default, any object selected during a command becomes part of the
    /// "previous selection set" and can be reselected by the SelPrev command.
    /// If you need to select objects but do not want them to be selected by
    /// a subsquent call to SelPrev, then call EnableSelPrev = false
    /// </summary>
    public void EnableSelPrevious(bool enable)
    {
      SetBool(idxSelPrev, enable);
    }

    /// <summary>
    /// By default, any object post-pick selected by GetObjects() is highlighted.
    /// If you want to post-pick objects and not have them automatically highlight,
    /// then call EnableHighlight = false
    /// </summary>
    public void EnableHighlight( bool enable )
    {
      SetBool(idxHighlight, enable);
    }

    /// <summary>
    /// By default, reference objects can be selected. If you do not want to be
    /// able to select reference objects, then call EnableReferenceObjectSelect=false
    /// </summary>
    public bool ReferenceObjectSelect
    {
      get { return GetBool(idxReferenceObjectSelect); }
      set { SetBool(idxReferenceObjectSelect, value); }
    }

    /// <summary>
    /// By default, post selection will select objects with grips on. If you do
    /// not want to be able to post select objects with grips on, then call
    /// EnableIgnoreGrips = false. The ability to preselect an object with grips
    /// on is determined by the value returned by the virtual
    /// RhinoObject.IsSelectableWithGripsOn.
    /// </summary>
    public void EnableIgnoreGrips( bool enable )
    {
      SetBool(idxIgnoreGrips, enable);
    }

    /// <summary>
    /// By default, when GetObject.GetObjects is called with minimumNumber > 0
    /// and maximumNumber = 0, the command prompt automatically includes "Press Enter
    /// when done" after the user has selected at least minimumNumber of objects. If
    /// you want to prohibit the addition of the "Press Enter when done", then call
    /// EnablePressEnterWhenDonePrompt = false;
    /// </summary>
    public void EnablePressEnterWhenDonePrompt( bool enable )
    {
      SetBool(idxPressEnterWhenDonePrompt, enable);
    }

    /// <summary>
    /// The default prompt when EnablePressEnterWhenDonePrompt is enabled is "Press Enter
    /// when done". Use this function to specify a different string to be appended.
    /// </summary>
    /// <param name="prompt"></param>
    public void SetPressEnterWhenDonePrompt(string prompt)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.CRhinoGetObject_SetPressEnterWhenDonePrompt(ptr, prompt);
    }

    /// <summary>
    /// Allow selecting objects that are already selected. By default, GetObjects() disallows
    /// selection of objects that are already selected to avoid putting the same object
    /// in the selection set more than once. Calling EnableAlreadySelectedObjectSelect = true
    /// overrides that restriction and allows selected objects to be selected and
    /// returned by GetObjects. This is useful because, coupled with the return immediately
    /// mode of GetObjects( 1, -1), it is possible to select a selected object to deselect
    /// when the selected objects are being managed outside GetObjects() as in the case of
    /// CRhinoPolyEdge::GetEdge().
    /// </summary>
    public bool AlreadySelectedObjectSelect
    {
      get { return GetBool(idxAlreadySelectedObjectSelect); }
      set { SetBool(idxAlreadySelectedObjectSelect, value); }
    }

    internal delegate bool GeometryFilterCallback(IntPtr rhObject, IntPtr geometry, ComponentIndex componentIndex);
    private static GetObject m_active_go; // = null; [runtime default]
    private static bool CustomGeometryFilter(IntPtr rhObject, IntPtr geometry, ComponentIndex componentIndex)
    {
      bool rc = true;
      if (m_active_go != null)
      {
        try
        {
          DocObjects.RhinoObject _rhObj = DocObjects.RhinoObject.CreateRhinoObjectHelper(rhObject);
          GeometryBase _geom = GeometryBase.CreateGeometryHelper(geometry, _rhObj);
          rc = m_active_go.CustomGeometryFilter(_rhObj, _geom, componentIndex);
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
      return rc;
    }

    /// <summary>
    /// Call to select a single object
    /// </summary>
    /// <returns>
    /// Success - objects selected.
    /// Cancel - user pressed ESCAPE to cancel the get.
    /// See GetResults for other possible values that may be returned when options, numbers,
    /// etc., are acceptable responses.
    /// </returns>
    [CLSCompliant(false)]
    public GetResult Get()
    {
      return GetMultiple(1, 1);
    }

    /// <summary>Call to select objects.</summary>
    /// <param name="minimumNumber">minimum number of objects to select.</param>
    /// <param name="maximumNumber">
    /// maximum number of objects to select.
    /// If 0, then the user must press enter to finish object selection.
    /// If -1, then object selection stops as soon as there are at least minimumNumber of object selected.
    /// If >0, then the picking stops when there are maximumNumber objects.  If a window pick, crossing
    /// pick, or Sel* command attempts to add more than maximumNumber, then the attempt is ignored.
    /// </param>
    /// <returns>
    /// Success - objects selected.
    /// Cancel - user pressed ESCAPE to cancel the get.
    /// See GetResults for other possible values that may be returned when options, numbers,
    /// etc., are acceptable responses.
    /// </returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addobjectstogroup.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addobjectstogroup.cs' lang='cs'/>
    /// <code source='examples\py\ex_addobjectstogroup.py' lang='py'/>
    /// </example>
    [CLSCompliant(false)]
    public GetResult GetMultiple(int minimumNumber, int maximumNumber)
    {
      GetObject old = m_active_go;
      m_active_go = this;
      GeometryFilterCallback cb = null;
      Type t = this.GetType();
      // Hook up CustomGeometryFilter virtual function is this is a subclass. This way we
      // don't have to pin anything and this class will get collected on the next appropriate GC
      if (t.IsSubclassOf(typeof(GetObject)))
        cb = CustomGeometryFilter;

      IntPtr ptr = NonConstPointer();
      uint rc = UnsafeNativeMethods.CRhinoGetObject_GetObjects(ptr, minimumNumber, maximumNumber, cb);

      m_active_go = old;
      return (GetResult)rc;
    }

    /// <summary>
    /// Gets the number of objects that were selected.
    /// </summary>
    public int ObjectCount
    {
      get
      {
        IntPtr ptr = NonConstPointer();
        return UnsafeNativeMethods.CRhinoGetObject_ObjectCount(ptr);
      }
    }

    /// <example>
    /// <code source='examples\vbnet\ex_orientonsrf.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_orientonsrf.cs' lang='cs'/>
    /// <code source='examples\py\ex_orientonsrf.py' lang='py'/>
    /// </example>
    public DocObjects.ObjRef Object(int index)
    {
      DocObjects.ObjRef rc = new Rhino.DocObjects.ObjRef();
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.CRhinoGetObject_Object(ptr, index, rc.NonConstPointer());
      return rc;
    }

    public DocObjects.ObjRef[] Objects()
    {
      int count = ObjectCount;
      if (count < 1)
        return null;
      Rhino.Collections.RhinoList<DocObjects.ObjRef> objrefs = new Rhino.Collections.RhinoList<Rhino.DocObjects.ObjRef>(count);
      for(int i=0; i<count; i++)
      {
        DocObjects.ObjRef objref = Object(i);
        objrefs.Add(objref);
      }
      return objrefs.ToArray();
    }

    public bool ObjectsWerePreselected
    {
      get { return GetBool(idxObjectsWerePreselected); }
    }

    /// <summary>
    /// Each instance of GetObject has a unique runtime serial number that
    /// is used to identify object selection events associated with that instance.
    /// </summary>
    [CLSCompliant(false)]
    public uint SerialNumber
    {
      get
      {
        IntPtr ptr = NonConstPointer();
        return UnsafeNativeMethods.CRhinoGetObject_SerialNumber(ptr);
      }
    }

    /// <summary>
    /// By default the picked object list is cleared when GetObject.GetObjects() is called.
    /// If you are reusing a GetObject class and do not want the existing object list
    /// cleared when you call Input, then call EnableClearObjectsOnEntry(false) before
    /// calling GetObjects().
    /// </summary>
    /// <param name="enable"></param>
    public void EnableClearObjectsOnEntry(bool enable)
    {
      SetBool(idxClearObjectsOnEntry, enable);
    }

    /// <summary>
    /// By default any objects in the object list are unselected when GetObject.GetObjects()
    /// exits with any return code besides Object. If you want to leave the objects
    /// selected when non-object input is returned, then call EnableClearObjectsOnExit(false)
    /// before calling GetObjects().
    /// </summary>
    /// <param name="enable"></param>
    public void EnableUnselectObjectsOnExit(bool enable)
    {
      SetBool(idxUnselectObjectsOnExit, enable);
    }
  }

  /// <summary>
  /// If an object passes the geometry TYPE filter, then the geometry ATTRIBUTE
  /// filter is applied.
  /// </summary>
  [Flags(), CLSCompliant(false)]
  public enum GeometryAttributeFilter : uint
  {
    /// <summary>
    /// 3d wire curve
    /// If you want to accept only wire or edge curves, then
    /// specify wire_curve or edge_curve, otherwise both wire
    /// and edge curves will pass the attribute filter.
    /// </summary>
    WireCurve = 1<<0,
    /// <summary>
    /// 3d curve of a surface edge
    /// If you want to accept only wire or edge curves, then
    /// specify wire_curve or edge_curve, otherwise both wire
    /// and edge curves will pass the attribute filter.
    /// </summary>
    EdgeCurve = 1 << 1,
    /// <summary>
    /// Closed Curves and Edges are acceptable
    /// If you want to accept only closed or open curves, then
    /// specify either closed_curve or open_curve.  Otherwise both
    /// closed and open curves will pass the attribute filter.
    /// </summary>
    ClosedCurve = 1<<2,
    /// <summary>
    /// Open Curves and Edges are acceptable
    /// If you want to accept only closed or open curves, then
    /// specify either closed_curve or open_curve.  Otherwise both
    /// closed and open curves will pass the attribute filter.
    /// </summary>
    OpenCurve = 1 << 3,
    /// <summary>
    /// seam edges are acceptable
    /// attributes of acceptable trimming edge objects (associated with an ON_BrepTrim).
    ///
    /// If none of these attributes are explicitly specified, then
    /// any kind of trimming edge will pass the attribute filter.
    /// </summary>
    SeamEdge = 1<<4,
    /// <summary>
    /// edges with 2 different surfaces pass
    /// attributes of acceptable trimming edge objects (associated with an ON_BrepTrim).
    ///
    /// If none of these attributes are explicitly specified, then
    /// any kind of trimming edge will pass the attribute filter.
    /// </summary>
    ManifoldEdge = 1 << 5,
    /// <summary>
    /// edges with 3 or more surfaces pass
    /// attributes of acceptable trimming edge objects (associated with an ON_BrepTrim).
    ///
    /// If none of these attributes are explicitly specified, then
    /// any kind of trimming edge will pass the attribute filter.
    /// </summary>
    NonmanifoldEdge = 1 << 6,
    /// <summary>
    /// any mated edge passes
    /// attributes of acceptable trimming edge objects (associated with an ON_BrepTrim).
    ///
    /// If none of these attributes are explicitly specified, then
    /// any kind of trimming edge will pass the attribute filter.
    /// </summary>
    MatedEdge = (1 << 4) | (1 << 5) | (1 << 6),
    /// <summary>
    /// boundary edges on surface sides pass
    /// attributes of acceptable trimming edge objects (associated with an ON_BrepTrim).
    ///
    /// If none of these attributes are explicitly specified, then
    /// any kind of trimming edge will pass the attribute filter.
    /// </summary>
    SurfaceBoundaryEdge = 1 << 7,
    /// <summary>
    /// boundary edges that trim a surface pass
    /// attributes of acceptable trimming edge objects (associated with an ON_BrepTrim).
    ///
    /// If none of these attributes are explicitly specified, then
    /// any kind of trimming edge will pass the attribute filter.
    /// </summary>
    TrimmingBoundaryEdge = 1 << 8,
    /// <summary>
    /// ant boundary edge passes
    /// attributes of acceptable trimming edge objects (associated with an ON_BrepTrim).
    ///
    /// If none of these attributes are explicitly specified, then
    /// any kind of trimming edge will pass the attribute filter.
    /// </summary>
    BoundaryEdge = (1 << 7) | (1 << 8),
    ///<summary>
    /// If you want to accept only closed or open surfaces, then
    /// specify either closed_surface or open_surface.  Otherwise both
    /// closed and open surfaces will pass the attribute filter.
    ///</summary>
    ClosedSurface = 1 << 9,
    ///<summary>
    /// If you want to accept only closed or open surfaces, then
    /// specify either closed_surface or open_surface.  Otherwise both
    /// closed and open surfaces will pass the attribute filter.
    ///</summary>
    OpenSurface = 1 << 10,
    ///<summary>
    /// If you want to accept only trimmed or untrimmed surfaces, then
    /// specify either trimmed_surface or untrimmed_surface.  Otherwise
    /// both trimmed and untrimmed surfaces will pass the attribute filter.
    ///</summary>
    TrimmedSurface = 1 << 11,
    ///<summary>
    /// If you want to accept only trimmed or untrimmed surfaces, then
    /// specify either trimmed_surface or untrimmed_surface.  Otherwise
    /// both trimmed and untrimmed surfaces will pass the attribute filter.
    ///</summary>
    UntrimmedSurface = 1 << 12,
    ///<summary>
    /// If you want to accept only sub-surfaces of (multi-surface)
    /// polysrf, then specify sub_surface.  If you do not want to
    /// accept sub-surfaces, then specify top_surface.  Otherwise
    /// sub-surfaces and top surfaces will pass the attribute filter.
    ///</summary>
    SubSurface = 1 << 13,
    ///<summary>
    /// If you want to accept only sub-surfaces of (multi-surface)
    /// polysrf, then specify sub_surface.  If you do not want to
    /// accept sub-surfaces, then specify top_surface.  Otherwise
    /// sub-surfaces and top surfaces will pass the attribute filter.
    ///</summary>
    TopSurface = 1 << 14,
    ///<summary>
    /// If you want to accept only manifold or nonmanifold polysrfs,
    /// then specify manifold_polysrf or nonmanifold_polysrf. Otherwise
    /// both manifold and nonmanifold polysrfs will pass the attribute
    /// filter.
    ///</summary>
    ManifoldPolysrf = 1 << 15,
    ///<summary>
    /// If you want to accept only manifold or nonmanifold polysrfs,
    /// then specify manifold_polysrf or nonmanifold_polysrf. Otherwise
    /// both manifold and nonmanifold polysrfs will pass the attribute
    /// filter.
    ///</summary>
    NonmanifoldPolysrf = 1 << 16,
    ///<summary>
    /// If you want to accept only closed or open polysrfs, then
    /// specify either closed_polysrf or open_polysrf.  Otherwise both
    /// closed and open polysrfs will pass the attribute filter.
    ///</summary>
    ClosedPolysrf = 1 << 17,
    ///<summary>
    /// If you want to accept only closed or open polysrfs, then
    /// specify either closed_polysrf or open_polysrf.  Otherwise both
    /// closed and open polysrfs will pass the attribute filter.
    ///</summary>
    OpenPolysrf = 1 << 18,
    ///<summary>
    /// If you want to accept only closed or open meshs, then
    /// specify either closed_mesh or open_mesh.  Otherwise both
    /// closed and open meshs will pass the attribute filter.
    ///</summary>
    ClosedMesh = 1 << 19,
    ///<summary>
    /// If you want to accept only closed or open meshs, then
    /// specify either closed_mesh or open_mesh.  Otherwise both
    /// closed and open meshs will pass the attribute filter.
    ///</summary>
    OpenMesh = 1 << 20,
    ///<summary>all trimming edges are boundary edges</summary>
    BoundaryInnerLoop = 1 << 21,
    ///<summary>all trimming edges are mated</summary>
    MatedInnerLoop = 1 << 22,
    ///<summary>any inner loop is acceptable</summary>
    InnerLoop = (1 << 21) | (1 << 22),
    ///<summary>all trimming edges are boundary edges</summary>
    BoundaryOuterLoop = 1 << 23,
    ///<summary>all trimming edges are mated</summary>
    MatedOuterLoop = 1 << 24,
    ///<summary>any outer loop is acceptable</summary>
    OuterLoop = (1 << 23) | (1 << 24),
    ///<summary>slit, crvonsrf, ptonsrf, etc.</summary>
    SpecialLoop = (1 << 25),
    AcceptAllAttributes = 0xffffffff
  }

  // skipping CRhinoMeshRef, CRhinoGetMeshes
}
