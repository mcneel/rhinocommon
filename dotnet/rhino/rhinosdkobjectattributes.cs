using System;
using System.Runtime.InteropServices;

namespace Rhino.DocObjects
{
  public class ObjectAttributes : Runtime.CommonObject
  {
    internal override IntPtr _InternalDuplicate(out bool applymempressure)
    {
      applymempressure = false;
      IntPtr pConstPointer = ConstPointer();
      return UnsafeNativeMethods.ON_Object_Duplicate(pConstPointer);
    }

    internal override IntPtr _InternalGetConstPointer()
    {
      uint serial_number = 0;
      Rhino.DocObjects.RhinoObject parent_object = ParentRhinoObject();
      if (null != parent_object)
        serial_number = parent_object.m_rhinoobject_serial_number;
      return UnsafeNativeMethods.CRhinoObject_Attributes(serial_number);
    }

    internal ObjectAttributes(IntPtr pNonConstAttributes)
    {
      ConstructNonConstObject(pNonConstAttributes);
    }

    internal ObjectAttributes(RhinoObject parentObject)
    {
      ConstructConstObject(parentObject, -1);
    }

    public ObjectAttributes()
    {
      IntPtr ptr = UnsafeNativeMethods.CRhinoObjectAttributes_New(IntPtr.Zero);
      ConstructNonConstObject(ptr);
    }

    /// <summary>
    /// Create a copy of this ObjectAttributes
    /// </summary>
    /// <returns></returns>
    public ObjectAttributes Duplicate()
    {
      IntPtr pThis = ConstPointer();
      IntPtr pNew = UnsafeNativeMethods.CRhinoObjectAttributes_New(pThis);
      if (IntPtr.Zero == pNew)
        return null;
      return new ObjectAttributes(pNew);
    }

    int GetInt(int which)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_3dmObjectAttributes_GetSetInt(ptr, which, false, 0);
    }
    void SetInt(int which, int set_value)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.ON_3dmObjectAttributes_GetSetInt(ptr, which, true, set_value);
    }
    const int idxMode = 0;
    const int idxLineTypeSource = 1;
    const int idxColorSource = 2;
    const int idxPlotColorSource = 3;
    const int idxPlotWeightSource = 4;
    const int idxDisplayMode = 5;
    const int idxLayerIndex = 6;
    const int idxLinetypeIndex = 7;
    const int idxMaterialIndex = 8;
    const int idxMaterialSource = 9;
    const int idxObjectDecoration = 10;
    const int idxWireDensity = 11;
    const int idxSpace = 12;
    const int idxGroupCount = 13;


    /// <summary>
    /// An object must be in one of three modes: normal, locked or hidden.
    /// If an object is in normal mode, then the object's layer controls visibility
    /// and selectability. If an object is locked, then the object's layer controls
    /// visibility by the object cannot be selected. If the object is hidden, it is
    /// not visible and it cannot be selected.
    /// </summary>
    public ObjectMode Mode
    {
      get
      {
        int rc = GetInt(idxMode);
        return (ObjectMode)rc;
      }
      set { SetInt(idxMode, (int)value); }
    }

    bool GetBool(int which)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_3dmObjectAttributes_GetSetBool(ptr, which, false, false);
    }
    void SetBool(int which, bool setValue)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.ON_3dmObjectAttributes_GetSetBool(ptr, which, true, setValue);
    }
    const int idxIsInstanceDefinitionObject = 0;
    const int idxIsVisible = 1;

    /// <summary>
    /// Use this query to determine if an object is part of an instance definition.
    /// </summary>
    public bool IsInstanceDefinitionObject
    {
      get { return GetBool(idxIsInstanceDefinitionObject); }
    }

    /// <summary>object visibility</summary>
    public bool Visible
    {
      get { return GetBool(idxIsVisible); }
      set { SetBool(idxIsVisible, value); }
    }

    /// <summary>
    /// The Linetype used to display an object is specified in one of two ways.
    /// If LinetypeSource is ON::linetype_from_layer, then the object's layer ON_Layer::Linetype() is used.
    /// If LinetypeSource is ON::linetype_from_object, then value of m_linetype is used.
    /// </summary>
    public ObjectLinetypeSource LinetypeSource
    {
      get
      {
        int rc = GetInt(idxLineTypeSource);
        return (ObjectLinetypeSource)rc;
      }
      set { SetInt(idxLineTypeSource, (int)value); }
    }

    /// <summary>
    /// The color used to display an object is specified in one of three ways.
    /// If ColorSource is ON::color_from_layer, then the object's layer ON_Layer::Color() is used.
    /// If ColorSource is ON::color_from_object, then value of m_color is used.
    /// If ColorSource is ON::color_from_material, then the diffuse color of the object's
    /// render material is used.  See ON_3dmObjectAttributes::MaterialSource() to
    /// determine where to get the definition of the object's render material.
    /// </summary>
    public ObjectColorSource ColorSource
    {
      get
      {
        int rc = GetInt(idxColorSource);
        return (ObjectColorSource)rc;
      }
      set { SetInt(idxColorSource, (int)value); }
    }

    /// <summary>
    /// The color used to plot an object on paper is specified in one of three ways.
    /// If PlotColorSource is ON::plot_color_from_layer, then the object's layer ON_Layer::PlotColor() is used.
    /// If PlotColorSource is ON::plot_color_from_object, then value of PlotColor() is used.
    /// </summary>
    public ObjectPlotColorSource PlotColorSource
    {
      get
      {
        int rc = GetInt(idxPlotColorSource);
        return (ObjectPlotColorSource)rc;
      }
      set { SetInt(idxPlotColorSource, (int)value); }
    }

    public ObjectPlotWeightSource PlotWeightSource
    {
      get
      {
        int rc = GetInt(idxPlotWeightSource);
        return (ObjectPlotWeightSource)rc;
      }
      set { SetInt(idxPlotWeightSource, (int)value); }
    }

    /// <summary>
    /// objects can be displayed in one of three ways: wireframe, shaded, or render preview.
    /// If the display mode is ON::default_display, then the display mode of the viewport
    /// detrmines how the object is displayed. If the display mode is ON::wireframe_display,
    /// ON::shaded_display, or ON::renderpreview_display, then the object is forced to
    /// display in that mode.
    /// </summary>
    public DisplayMode DisplayMode
    {
      get
      {
        int rc = GetInt(idxDisplayMode);
        return (DisplayMode)rc;
      }
      set { SetInt(idxDisplayMode, (int)value); }
    }
/*
    /// <summary>
    /// If "this" has attributes (color, plot weight, ...) with "by parent" sources,
    /// then the values of those attributes on parentAttributes are copied.
    /// </summary>
    /// <param name="parentAttributes"></param>
    /// <param name="controlLimits">
    /// The bits in controlLimits determine which attributes may be copied.
    /// 1: visibility
    /// 2: color
    /// 4: render material
    /// 8: plot color
    /// 0x10: plot weight
    /// 0x20: linetype
    /// </param>
    /// <returns>
    /// The bits in the returned integer indicate which attributes were actually modified.
    /// 1: visibility
    /// 2: color
    /// 4: render material
    /// 8: plot color
    /// 0x10: plot weight
    /// 0x20: linetype
    /// </returns>
    public int ApplyParentalControl(ObjectAttributes parentAttributes, int controlLimits)
    {
      IntPtr ptr = NonConstPointer();
      IntPtr parentPtr = parentAttributes.ConstPointer();
      return (int)UnsafeNativeMethods.ON_3dmObjectAttributes_ApplyParentalControl(ptr, parentPtr, (uint)controlLimits);
    }
    /// <summary>
    /// If "this" has attributes (color, plot weight, ...) with "by parent" sources,
    /// then the values of those attributes on parentAttributes are copied.
    /// </summary>
    /// <param name="parentAttributes"></param>
    /// <returns>
    /// The bits in the returned integer indicate which attributes were actually modified.
    /// 1: visibility
    /// 2: color
    /// 4: render material
    /// 8: plot color
    /// 0x10: plot weight
    /// 0x20: linetype
    /// </returns>
    public int ApplyParentalControl(ObjectAttributes parentAttributes)
    {
      IntPtr ptr = NonConstPointer();
      IntPtr parentPtr = parentAttributes.ConstPointer();
      return (int)UnsafeNativeMethods.ON_3dmObjectAttributes_ApplyParentalControl(ptr, parentPtr, 0xFFFFFFFF);
    }
*/
    /// <summary>
    /// Every object has a UUID (universally unique identifier). The default value is Guid.Empty.
    /// When an object is added to a model, the value is checked.  If the value is NULL, a
    /// new UUID is created. If the value is not NULL but it is already used by another object
    /// in the model, a new UUID is created. If the value is not NULL and it is not used by
    /// another object in the model, then that value persists. When an object is updated, by
    /// a move for example, the value of ObjectId persists.
    /// </summary>
    public Guid ObjectId
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_3dmObjectAttributes_m_uuid(ptr); 
      }
      // I don't think I've ever seen a "set" case so leave out
    }


    const int idxName = 0;
    //const int idxUrl = 1;
    string GetString(int which)
    {
      IntPtr ptr = ConstPointer();
      IntPtr rc = UnsafeNativeMethods.ON_3dmObjectAttributes_GetSetString(ptr, which, false, null);
      // 24 April 2010 S. Baer
      // I think returning the empty string is going to be better
      // in more cases than returning null
      if (IntPtr.Zero == rc)
        return String.Empty;
      return Marshal.PtrToStringUni(rc);
    }
    void SetString(int which, string str)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.ON_3dmObjectAttributes_GetSetString(ptr, which, true, str);
    }

    /// <summary>
    /// objects have optional text names.  More than one object in
    /// a model can have the same name and some objects may have no name.
    /// </summary>
    public string Name
    {
      get { return GetString(idxName); }
      set { SetString(idxName, value); }
    }

    // Skipping for now. I'm not sure if we want to use this string for something else
    /*
    /// <summary>
    /// objects may have an URL. There are no restrictions on what value this
    /// URL may have. As an example, if the object came from a commercial part
    /// library, the URL might point to the definition of that part.
    /// </summary>
    public System.Uri Url
    {
      get{ return GetString(idxUrl); }
      set { SetString(idxUrl, value); }
    }
    */

    /// <summary>
    /// Layer definitions in an OpenNURBS model are stored in a layer table.
    /// The layer table is conceptually an array of ON_Layer classes.  Every
    /// OpenNURBS object in a model is on some layer.  The object's layer
    /// is specified by zero based indicies into the ON_Layer array.
    /// </summary>
    public int LayerIndex
    {
      get { return GetInt(idxLayerIndex); }
      set { SetInt(idxLayerIndex, value); }
    }
    /// <summary>
    /// Linetype definitions in an OpenNURBS model are stored in a linetype table.
    /// The linetype table is conceptually an array of ON_Linetype classes. Every
    /// OpenNURBS object in a model references some linetype.  The object's linetype
    /// is specified by zero based indicies into the ON_Linetype array.
    /// index 0 is reserved for continuous linetype (no pattern)
    /// </summary>
    public int LinetypeIndex
    {
      get { return GetInt(idxLinetypeIndex); }
      set { SetInt(idxLinetypeIndex, value); }
    }
    /// <summary>
    /// If you want something simple and fast, set m_material_index to the index of
    /// the rendering material and ignore m_rendering_attributes. If you are developing
    /// a high quality plug-in renderer, and a user is assigning one of your fabulous
    /// rendering materials to this object, then add rendering material information to
    /// the m_rendering_attributes.m_materials[] array. 
    /// </summary>
    public int MaterialIndex
    {
      get { return GetInt(idxMaterialIndex); }
      set { SetInt(idxMaterialIndex, value); }
    }

    // [skipping]
    // ON_ObjectRenderingAttributes m_rendering_attributes;

    /// <summary>
    /// Determine if the simple material should come from the object or from it's layer.
    /// High quality rendering plug-ins should use m_rendering_attributes.
    /// </summary>
    public ObjectMaterialSource MaterialSource
    {
      get { return (ObjectMaterialSource)GetInt(idxMaterialSource); }
      set { SetInt(idxMaterialSource, (int)value); }
    }

    const int idxColor = 0;
    const int idxPlotColor = 1;
    System.Drawing.Color GetColor(int which)
    {
      IntPtr ptr = ConstPointer();
      int argb = UnsafeNativeMethods.ON_3dmObjectAttributes_GetSetColor(ptr, which, false, 0);
      return System.Drawing.Color.FromArgb(argb);
    }
    void SetColor(int which, System.Drawing.Color c)
    {
      IntPtr ptr = NonConstPointer();
      int argb = c.ToArgb();
      UnsafeNativeMethods.ON_3dmObjectAttributes_GetSetColor(ptr, which, true, argb);
    }

    /// <summary>
    /// If ON::color_from_object == ColorSource, then color is the object's display color.
    /// </summary>
    public System.Drawing.Color ObjectColor
    {
      get { return GetColor(idxColor); }
      set { SetColor(idxColor, value); }
    }
    /// <summary>
    /// If plot_color_from_object == PlotColorSource, then PlotColor is the object's plotting color.
    /// </summary>
    public System.Drawing.Color PlotColor
    {
      get { return GetColor(idxPlotColor); }
      set { SetColor(idxPlotColor, value); }
    }

    public System.Drawing.Color DrawColor(RhinoDoc document)
    {
      return DrawColor(document, Guid.Empty);
    }
    public System.Drawing.Color DrawColor(RhinoDoc document, Guid viewportId)
    {
      IntPtr pConstThis = ConstPointer();
      int argb = UnsafeNativeMethods.CRhinoObjectAttributes_DrawColor(pConstThis, document.m_docId, viewportId);
      return System.Drawing.Color.FromArgb(argb);
    }

    /// <summary>
    /// Plot weight in millimeters.
    /// =0.0 means use the default width
    /// &lt;0.0 means don't plot (visible for screen display, but does not show on plot)
    /// </summary>
    public double PlotWeight
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_3dmObjectAttributes_PlotWeight(ptr, false, 0.0);
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.ON_3dmObjectAttributes_PlotWeight(ptr, true, value);
      }
    }

    /// <summary>
    /// Used to indicate an object has a decoration (like an arrowhead on a curve)
    /// </summary>
    public ObjectDecoration ObjectDecoration
    {
      get { return (ObjectDecoration)GetInt(idxObjectDecoration); }
      set { SetInt(idxObjectDecoration, (int)value); }
    }

    /// <summary>
    /// When a surface object is displayed in wireframe, m_wire_density controls
    /// how many isoparametric wires are used.
    /// value    number of isoparametric wires
    /// 0        boundary and knot wires 
    /// 1        boundary and knot wires and, if there are no interior knots, a single interior wire.
    /// N>=2     boundary and knot wires and (N+1) interior wires
    /// </summary>
    public int WireDensity
    {
      get { return GetInt(idxWireDensity); }
      set { SetInt(idxWireDensity, value); }
    }


    /// <summary>
    /// If ViewportId is nil, the object is active in all viewports. If ViewportId is not nil, then 
    /// this object is only active in a specific view. This field is primarily used to assign page
    /// space objects to a specific page, but it can also be used to restrict model space to a
    /// specific view.
    /// </summary>
    public Guid ViewportId
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_3dmObjectAttributes_ViewportId(ptr, false, Guid.Empty);
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.ON_3dmObjectAttributes_ViewportId(ptr, true, value);
      }
    }

    /// <summary>
    /// Starting with V4, objects can be in either model space or page space.
    /// If an object is in page space, then ViewportId is not nil and
    /// identifies the page it is on.
    /// </summary>
    public ActiveSpace Space
    {
      get { return (ActiveSpace)GetInt(idxSpace); }
      set { SetInt(idxSpace, (int)value); }
    }

    /// <summary>number of groups object belongs to</summary>
    public int GroupCount
    {
      get { return GetInt(idxGroupCount); }
    }


    /// <summary>
    /// Returns an array of GroupCount group indices.  If GroupCount is zero, then GetGroupList() returns null.
    /// </summary>
    /// <returns></returns>
    public int[] GetGroupList()
    {
      int count = GroupCount;
      if( count < 1 )
        return null;
      int[] rc = new int[count];
      IntPtr ptr = ConstPointer();
      UnsafeNativeMethods.ON_3dmObjectAttributes_GroupList(ptr, ref rc[0]);
      return rc;
    }

    // [skipping]
    // int TopGroup() const; I'm not sure how this is used
    // BOOL IsInGroup()
    // BOOL IsInGroups()

    const int idxAddToGroup = 0;
    const int idxRemoveFromGroup = 1;
    //const int idxRemoveFromTopGroup = 2;
    const int idxRemoveFromAllGroups = 3;

    /// <summary>
    /// Adds object to the group with specified index by appending index to
    /// group list (If the object is already in group, nothing is changed.)
    /// </summary>
    /// <param name="groupIndex"></param>
    public void AddToGroup(int groupIndex)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.ON_3dmObjectAttributes_GroupOp(ptr, idxAddToGroup, groupIndex);
    }
    /// <summary>
    /// removes object from the group with specified index. If the 
    /// object is not in the group, nothing is changed.
    /// </summary>
    /// <param name="groupIndex"></param>
    public void RemoveFromGroup(int groupIndex)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.ON_3dmObjectAttributes_GroupOp(ptr, idxRemoveFromGroup, groupIndex);
    }

    // [skipping]
    // void RemoveFromTopGroup(); don't understand how this is used

    /// <summary>Removes object from all groups</summary>
    public void RemoveFromAllGroups()
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.ON_3dmObjectAttributes_GroupOp(ptr, idxRemoveFromAllGroups, -1);
    }

    // [skipping]
    //  bool FindDisplayMaterialRef(
    //  bool FindDisplayMaterialId( 
    //  bool AddDisplayMaterialRef(
    //  bool RemoveDisplayMaterialRef(
    //  void RemoveAllDisplayMaterialRefs();
    //  int DisplayMaterialRefCount() const;
    //  ON_SimpleArray<ON_DisplayMaterialRef> m_dmref;

    #region user strings
    /// <summary>
    /// Attach a user string (key,value combination) to this geometry
    /// </summary>
    /// <param name="key">id used to retrieve this string</param>
    /// <param name="value">string associated with key</param>
    /// <returns>true on success</returns>
    public bool SetUserString(string key, string value)
    {
      //const lie
      IntPtr pThis = ConstPointer();
      bool rc = UnsafeNativeMethods.ON_Object_SetUserString(pThis, key, value);
      return rc;
    }
    /// <summary>
    /// Get user string
    /// </summary>
    /// <param name="key">id used to retrieve the string</param>
    /// <returns>string associated with the key if successful. null if no key was found</returns>
    public string GetUserString(string key)
    {
      IntPtr pThis = ConstPointer();
      IntPtr pValue = UnsafeNativeMethods.ON_Object_GetUserString(pThis, key);
      if (IntPtr.Zero == pValue)
        return null;
      return Marshal.PtrToStringUni(pValue);
    }

    public int UserStringCount
    {
      get
      {
        IntPtr pThis = ConstPointer();
        int rc = UnsafeNativeMethods.ON_Object_UserStringCount(pThis);
        return rc;
      }
    }

    /// <summary>
    /// Get all (key, value) user strings attached
    /// </summary>
    /// <returns></returns>
    public System.Collections.Specialized.NameValueCollection GetUserStrings()
    {
      System.Collections.Specialized.NameValueCollection rc = new System.Collections.Specialized.NameValueCollection();
      IntPtr pThis = ConstPointer();
      int count = 0;
      IntPtr pUserStrings = UnsafeNativeMethods.ON_Object_GetUserStrings(pThis, ref count);

      for (int i = 0; i < count; i++)
      {
        IntPtr pKey = UnsafeNativeMethods.ON_UserStringList_KeyValue(pUserStrings, i, true);
        IntPtr pValue = UnsafeNativeMethods.ON_UserStringList_KeyValue(pUserStrings, i, false);
        if (IntPtr.Zero != pKey && IntPtr.Zero != pValue)
        {
          string key = Marshal.PtrToStringUni(pKey);
          string value = Marshal.PtrToStringUni(pValue);
          if(key!=null && value!=null)
            rc.Add(key, value);
        }
      }

      if (IntPtr.Zero != pUserStrings)
        UnsafeNativeMethods.ON_UserStringList_Delete(pUserStrings);

      return rc;
    }
    #endregion
  }
}
