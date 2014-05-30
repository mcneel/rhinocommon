#pragma warning disable 1591
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Rhino.Render;
using Rhino.Runtime.InteropWrappers;

namespace Rhino.DocObjects
{
  //[Serializable]
  public class Layer : Rhino.Runtime.CommonObject
  {
    #region members
    // Represents both a CRhinoLayer and an ON_Layer. When m_ptr is
    // null, the object uses m_doc and m_id to look up the const
    // CRhinoLayer in the layer table.
#if RHINO_SDK
    readonly Rhino.RhinoDoc m_doc;
#endif
    readonly Guid m_id=Guid.Empty;
    readonly Rhino.FileIO.File3dm m_onx_model;
    #endregion

    #region constructors
    public Layer()
    {
      // Creates a new non-document control ON_Layer
      IntPtr pLayer = UnsafeNativeMethods.ON_Layer_New();
      ConstructNonConstObject(pLayer);
    }

#if RHINO_SDK
    internal Layer(int index, Rhino.RhinoDoc doc)
    {
      m_id = UnsafeNativeMethods.CRhinoLayerTable_GetLayerId(doc.m_docId, index);
      m_doc = doc;
      m__parent = m_doc;
    }
#endif

    class LayerHolder
    {
      readonly IntPtr m_pConstLayer;
      public LayerHolder(IntPtr pConstLayer)
      {
        m_pConstLayer = pConstLayer;
      }
      public IntPtr ConstPointer()
      {
        return m_pConstLayer;
      }
    }

    internal Layer(IntPtr pConstLayer)
    {
      LayerHolder holder = new LayerHolder(pConstLayer);
      ConstructConstObject(holder, -1);
    }

    internal Layer(Guid id, Rhino.FileIO.File3dm onxModel)
    {
      m_id = id;
      m_onx_model = onxModel;
      m__parent = onxModel;
    }

    //// serialization constructor
    //protected Layer(SerializationInfo info, StreamingContext context)
    //  : base (info, context)
    //{
    //}

#if RHINO_SDK
    /// <summary>
    /// Constructs a layer with the current default properties.
    /// The default layer properties are:
    /// <para>color = Rhino.ApplicationSettings.AppearanceSettings.DefaultLayerColor</para>
    /// <para>line style = Rhino.ApplicationSettings.AppearanceSettings.DefaultLayerLineStyle</para>
    /// <para>material index = -1</para>
    /// <para>iges level = -1</para>
    /// <para>mode = NormalLayer</para>
    /// <para>name = empty</para>
    /// <para>layer index = 0 (ignored by AddLayer)</para>
    /// </summary>
    /// <returns>A new layer instance.</returns>
    public static Layer GetDefaultLayerProperties()
    {
      Layer layer = new Layer();
      IntPtr ptr = layer.NonConstPointer();
      UnsafeNativeMethods.CRhinoLayerTable_GetDefaultLayerProperties(ptr);
      return layer;
    }
#endif
    #endregion

    /// <example>
    /// <code source='examples\vbnet\ex_locklayer.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_locklayer.cs' lang='cs'/>
    /// <code source='examples\py\ex_locklayer.py' lang='py'/>
    /// </example>
    public bool CommitChanges()
    {
#if RHINO_SDK
#if RDK_CHECKED
      if ((m_set_render_material != Guid.Empty || m_set_render_material_to_null) && null != m_doc)
      {
        var doc_id = m_doc.DocumentId;
        var layer_index = LayerIndex;
        UnsafeNativeMethods.Rdk_RenderContent_SetLayerMaterialInstanceId((uint)doc_id, layer_index, m_set_render_material);
      }
      m_set_render_material = Guid.Empty;
      m_set_render_material_to_null = false;
#endif
      if (m_id == Guid.Empty || IsDocumentControlled)
        return false;
      IntPtr pThis = NonConstPointer();
      var result = UnsafeNativeMethods.CRhinoLayerTable_CommitChanges(m_doc.m_docId, pThis, m_id);
      return result;
#else
      return true;
#endif
    }

    internal override IntPtr _InternalGetConstPointer()
    {
#if RHINO_SDK
      if (m_doc != null)
        return UnsafeNativeMethods.CRhinoLayerTable_GetLayerPointer2(m_doc.m_docId, m_id);
#endif
      if (m_onx_model != null)
      {
        IntPtr pModel = m_onx_model.NonConstPointer();
        return UnsafeNativeMethods.ONX_Model_GetLayerPointer(pModel, m_id);
      }

      if (m__parent is LayerHolder)
      {
        LayerHolder holder = m__parent as LayerHolder;
        return holder.ConstPointer();
      }
      return IntPtr.Zero;
    }
    
    internal override IntPtr _InternalDuplicate(out bool applymempressure)
    {
      applymempressure = false;
      IntPtr pConstPointer = ConstPointer();
      return UnsafeNativeMethods.ON_Object_Duplicate(pConstPointer);
    }

    internal override IntPtr NonConstPointer()
    {
      if (m__parent is Rhino.FileIO.File3dm)
      {
        return _InternalGetConstPointer();
      }

      return base.NonConstPointer();
    }

    #region properties
    /// <summary>Gets or sets the name of this layer.</summary>
    /// <example>
    /// <code source='examples\vbnet\ex_sellayer.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_sellayer.cs' lang='cs'/>
    /// <code source='examples\py\ex_sellayer.py' lang='py'/>
    /// </example>
    /// <remarks>If you are modifying a layer inside a Rhino document, 
    /// you must call CommitChanges for the modifications to take effect.</remarks>
    /// <example>
    /// <code source='examples\vbnet\ex_renamelayer.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_renamelayer.cs' lang='cs'/>
    /// <code source='examples\py\ex_renamelayer.py' lang='py'/>
    /// </example>
    public string Name
    {
      get
      {
        using (var sh = new StringHolder())
        {
          IntPtr pLayer = ConstPointer();
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.ON_Layer_GetLayerName(pLayer, pString);
          return sh.ToString();
        }
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Layer_SetLayerName(pThis, value);
      }
    }

    /// <summary>
    /// Gets the full path to this layer. The full path includes nesting information.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_locklayer.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_locklayer.cs' lang='cs'/>
    /// <code source='examples\py\ex_locklayer.py' lang='py'/>
    /// </example>
    public string FullPath
    {
      get
      {
#if RHINO_SDK
        if (null == m_doc)
          return Name;
        int index = LayerIndex;
        using (var sh = new StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          if (!UnsafeNativeMethods.CRhinoLayer_GetLayerPathName(m_doc.m_docId, index, pString))
            return Name;
          return sh.ToString();
        }
#else
        return Name;
#endif
      }
    }

    public override string ToString()
    {
      return FullPath;
    }

    /// <summary>
    /// Gets or sets the index of this layer.
    /// </summary>
    public int LayerIndex
    {
      get { return GetInt(idxLayerIndex); }
      set { SetInt(idxLayerIndex, value); }
    }

    /// <summary>
    /// Gets or sets the ID of this layer object. 
    /// You typically do not need to assign a custom ID.
    /// </summary>
    public Guid Id
    {
      get
      {
        IntPtr pLayer = ConstPointer();
        return UnsafeNativeMethods.ON_Layer_GetGuid(pLayer, true);
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.ON_Layer_SetGuid(ptr, true, value);
      }
    }

    /// <summary>
    /// Gets the ID of the parent layer. Layers can be origanized in a hierarchical structure, 
    /// in which case this returns the parent layer ID. If the layer has no parent, 
    /// Guid.Empty will be returned.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_addchildlayer.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addchildlayer.cs' lang='cs'/>
    /// <code source='examples\py\ex_addchildlayer.py' lang='py'/>
    /// </example>
    public Guid ParentLayerId
    {
      get
      {
        IntPtr pConstLayer = ConstPointer();
        return UnsafeNativeMethods.ON_Layer_GetGuid(pConstLayer, false);
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.ON_Layer_SetGuid(ptr, false, value);
      }
    }

    /// <summary>
    /// Gets or sets the IGES level for this layer.
    /// </summary>
    public int IgesLevel
    {
      get { return GetInt(idxIgesLevel); }
      set { SetInt(idxIgesLevel, value); }
    }

    /// <summary>
    /// Gets or sets the display color for this layer.
    /// </summary>
    /// <remarks>If you are modifying a layer inside a Rhino document, 
    /// you must call CommitChanges for the modifications to take effect.</remarks>
    public Rhino.Drawing.Color Color
    {
      get
      {
        IntPtr pConstLayer = ConstPointer();
        int abgr = UnsafeNativeMethods.ON_Layer_GetColor(pConstLayer, true);
        return Rhino.Runtime.Interop.ColorFromWin32(abgr);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        int argb = value.ToArgb();
        UnsafeNativeMethods.ON_Layer_SetColor(pThis, argb, true);
      }
    }

    /// <summary>
    /// Gets or sets the plot color for this layer.
    /// </summary>
    /// <remarks>If you are modifying a layer inside a Rhino document, 
    /// you must call CommitChanges for the modifications to take effect.</remarks>
    public Rhino.Drawing.Color PlotColor
    {
      get
      {
        IntPtr pConstLayer = ConstPointer();
        int abgr = UnsafeNativeMethods.ON_Layer_GetColor(pConstLayer, false);
        return Rhino.Runtime.Interop.ColorFromWin32(abgr);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        int argb = value.ToArgb();
        UnsafeNativeMethods.ON_Layer_SetColor(pThis, argb, false);
      }
    }

    /// <summary>
    /// Gets or sets the thickness of the plotting pen in millimeters. 
    /// A thickness of 0.0 indicates the "default" pen weight should be used.
    /// </summary>
    /// <remarks>If you are modifying a layer inside a Rhino document, 
    /// you must call CommitChanges for the modifications to take effect.</remarks>
    public double PlotWeight
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_Layer_GetPlotWeight(pConstThis);
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.ON_Layer_SetPlotWeight(ptr, value);
      }
    }

    /// <summary>
    /// Gets or sets the line-type index for this layer.
    /// </summary>
    /// <remarks>If you are modifying a layer inside a Rhino document, 
    /// you must call CommitChanges for the modifications to take effect.</remarks>
    public int LinetypeIndex
    {
      get { return GetInt(idxLinetypeIndex); }
      set { SetInt(idxLinetypeIndex, value); }
    }

    /// <summary>
    /// Gets or sets the index of render material for objects on this layer that have
    /// MaterialSource() == MaterialFromLayer. 
    /// A material index of -1 indicates no material has been assigned 
    /// and the material created by the default Material constructor 
    /// should be used.
    /// </summary>
    /// <remarks>If you are modifying a layer inside a Rhino document, 
    /// you must call CommitChanges for the modifications to take effect.</remarks>
    public int RenderMaterialIndex
    {
      get { return GetInt(idxRenderMaterialIndex); }
      set { SetInt(idxRenderMaterialIndex, value); }
    }

    /// <summary>
    /// Gets or sets the visibility of this layer.
    /// </summary>
    /// <remarks>If you are modifying a layer inside a Rhino document, 
    /// you must call CommitChanges for the modifications to take effect.</remarks>
    public bool IsVisible
    {
      get { return GetBool(idxIsVisible); }
      set { SetBool(idxIsVisible, value); }
    }

    /// <summary>
    /// Gets or sets a value indicating the locked state of this layer.
    /// </summary>
    /// <remarks>If you are modifying a layer inside a Rhino document, 
    /// you must call CommitChanges for the modifications to take effect.</remarks>
    /// <example>
    /// <code source='examples\vbnet\ex_locklayer.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_locklayer.cs' lang='cs'/>
    /// <code source='examples\py\ex_locklayer.py' lang='py'/>
    /// </example>
    public bool IsLocked
    {
      get { return GetBool(idxIsLocked); }
      set { SetBool(idxIsLocked, value); }
    }

    /// <summary>
    /// The persistent visbility setting is used for layers whose visibilty can
    /// be changed by a "parent" object. A common case is when a layer is a
    /// child layer (ParentId is not nil). In this case, when a parent layer is
    /// turned off, then child layers are also turned off. The persistent
    /// visibility setting determines what happens when the parent is turned on
    /// again.
    /// </summary>
    /// <remarks>
    /// Returns true if this layer's visibility is controlled by a parent
    /// object and the parent is turned on (after being off), then this
    /// layer will also be turned on.
    /// Returns false if this layer's visibility is controlled by a parent
    /// object and the parent layer is turned on (after being off), then
    /// this layer will continue to be off.
    /// 
    /// When the persistent visbility is not explicitly set, this
    /// property returns the current value of IsVisible
    /// </remarks>
    public bool GetPersistentVisibility()
    {
      return GetBool(idxPersistentVisibility);
    }

    /// <summary>
    /// Set the persistent visibility setting for this layer
    /// </summary>
    /// <param name="persistentVisibility"></param>
    public void SetPersistentVisibility(bool persistentVisibility)
    {
      SetBool(idxPersistentVisibility, persistentVisibility);
    }

    /// <summary>
    /// Remove any explicit persistent visibility setting from this layer
    /// </summary>
    public void UnsetPersistentVisibility()
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_Layer_UnsetPersistentVisibility(pThis);
    }

    /// <summary>
    /// The persistent locking setting is used for layers that can be locked by
    /// a "parent" object. A common case is when a layer is a child layer
    /// (Layer.ParentI is not nil). In this case, when a parent layer is locked,
    /// then child layers are also locked. The persistent locking setting
    /// determines what happens when the parent is unlocked again.
    /// </summary>
    /// <returns></returns>
    public bool GetPersistentLocking()
    {
      return GetBool(idxPersistentLocking);
    }

    /// <summary>
    /// Set the persistent locking setting for this layer
    /// </summary>
    /// <param name="persistentLocking"></param>
    public void SetPersistentLocking(bool persistentLocking)
    {
      SetBool(idxPersistentLocking, persistentLocking);
    }

    /// <summary>
    /// Remove any explicity persistent locking settings from this layer
    /// </summary>
    public void UnsetPersistentLocking()
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_Layer_UnsetPersistentLocking(pThis);
    }

    /// <summary>
    /// Gets or sets a value indicating whether this layer is expanded in the Rhino Layer dialog.
    /// </summary>
    /// <remarks>If you are modifying a layer inside a Rhino document, 
    /// you must call CommitChanges for the modifications to take effect.</remarks>
    public bool IsExpanded
    {
      get { return GetBool(idxIsExpanded); }
      set { SetBool(idxIsExpanded, value); }
    }

    /// <summary>
    /// Gets a value indicating whether this layer has been deleted and is 
    /// currently in the Undo buffer.
    /// </summary>
    public bool IsDeleted
    {
      get
      {
#if RHINO_SDK
        if (null == m_doc)
          return false;
        int index = LayerIndex;
        return UnsafeNativeMethods.CRhinoLayer_IsDeleted(m_doc.m_docId, index);
#else
        return false;
#endif
      }
    }

    /// <summary>
    /// Gets a value indicting whether this layer is a referenced layer. 
    /// Referenced layers are part of referenced documents.
    /// </summary>
    public bool IsReference
    {
      get
      {
#if RHINO_SDK
        if (null == m_doc)
          return false;
        int index = LayerIndex;
        return UnsafeNativeMethods.CRhinoLayer_IsReference(m_doc.m_docId, index);
#else
        return false;
#endif
      }
    }

#if RDK_CHECKED
    /// <summary>
    /// Gets or sets the <see cref="Render.RenderMaterial"/> for objects on
    /// this layer that have MaterialSource() == MaterialFromLayer.
    /// A null result indicates that no <see cref="Render.RenderMaterial"/> has
    /// been assigned  and the material created by the default Material
    /// constructor or the <see cref="RenderMaterialIndex"/> should be used.
    /// </summary>
    /// <remarks>If you are modifying a layer inside a Rhino document, 
    /// you must call CommitChanges for the modifications to take effect.</remarks>
    public RenderMaterial RenderMaterial
    {
      get
      {
        // If set was called and the render material was changed or set to null
        // then return the cached set to render material.
        if (m_set_render_material_to_null || m_set_render_material != Guid.Empty)
        {
          var result = RenderContent.FromId(m_doc, m_set_render_material) as RenderMaterial;
          return result;
        }
        // Get the document Id
        var doc_id = (null == m_doc ? 0 : m_doc.DocumentId);
        if (doc_id < 1) return null;
        var pointer = ConstPointer();
        // Get the render material associated with the layers render material
        // index into the documents material table.
        var id = UnsafeNativeMethods.Rdk_RenderContent_LayerMaterialInstanceId((uint)doc_id, pointer);
        var content = RenderContent.FromId(m_doc, id) as RenderMaterial;
        return content;
      }
      set
      {
        // Cache the new values Id and set a flag that tells CommitChanges() to
        // remove the render material Id as necessary.
        m_set_render_material_to_null = (value == null);
        m_set_render_material = value == null ? Guid.Empty : value.Id;
      }
    }

    private Guid m_set_render_material = Guid.Empty;
    private bool m_set_render_material_to_null;
#endif

    /// <summary>
    /// Runtime index used to sort layers in layer dialog.
    /// </summary>
    public int SortIndex
    {
      get
      {
#if RHINO_SDK
        if (null == m_doc)
          return -1;
        int index = LayerIndex;
        return UnsafeNativeMethods.CRhinoLayer_SortIndex(m_doc.m_docId, index);
#else
        return -1;
#endif
      }
    }

    const int idxLinetypeIndex = 0;
    const int idxRenderMaterialIndex = 1;
    const int idxLayerIndex = 2;
    const int idxIgesLevel = 3;
    int GetInt(int which)
    {
      IntPtr pConstLayer = ConstPointer();
      return UnsafeNativeMethods.ON_Layer_GetInt(pConstLayer, which);
    }
    void SetInt(int which, int val)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.ON_Layer_SetInt(ptr, which, val);
    }

    const int idxIsVisible = 0;
    const int idxIsLocked = 1;
    const int idxIsExpanded = 2;
    const int idxPersistentVisibility = 3;
    const int idxPersistentLocking = 4;
    bool GetBool(int which)
    {
      IntPtr pConstLayer = ConstPointer();
      return UnsafeNativeMethods.ON_Layer_GetSetBool(pConstLayer, which, false, false);
    }
    void SetBool(int which, bool val)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.ON_Layer_GetSetBool(ptr, which, true, val);
    }
    #endregion

    /// <summary>
    /// Sets layer to default settings.
    /// </summary>
    /// <remarks>If you are modifying a layer inside a Rhino document, 
    /// you must call CommitChanges for the modifications to take effect.</remarks>
    public void Default()
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_Layer_Default(pThis);
    }

    #region methods
#if RHINO_SDK
    /// <summary>
    /// Determines if a given string is valid for a layer name.
    /// </summary>
    /// <param name="name">A name to be validated.</param>
    /// <returns>true if the name is valid for a layer name; otherwise, false.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addlayer.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addlayer.cs' lang='cs'/>
    /// <code source='examples\py\ex_addlayer.py' lang='py'/>
    /// </example>
    public static bool IsValidName(string name)
    {
      return UnsafeNativeMethods.RHC_IsValidName(name);
    }

    public bool IsChildOf(int layerIndex)
    {
      int index = LayerIndex;
      int rc = UnsafeNativeMethods.CRhinoLayerNode_IsChildOrParent(m_doc.m_docId, index, layerIndex, true);
      return 1 == rc;
    }
    public bool IsChildOf(Layer otherLayer)
    {
      return IsChildOf(otherLayer.LayerIndex);
    }

    public bool IsParentOf(int layerIndex)
    {
      int index = LayerIndex;
      int rc = UnsafeNativeMethods.CRhinoLayerNode_IsChildOrParent(m_doc.m_docId, index, layerIndex, false);
      return 1 == rc;
    }
    public bool IsParentOf(Layer otherLayer)
    {
      return IsParentOf(otherLayer.LayerIndex);
    }

    /// <summary>
    /// Gets immediate children of this layer. Note that child layers may have their own children.
    /// </summary>
    /// <returns>Array of child layers. null if this layer does not have any children.</returns>
    public Layer[] GetChildren()
    {
      Runtime.InteropWrappers.SimpleArrayInt childIndices = new Rhino.Runtime.InteropWrappers.SimpleArrayInt();
      int index = LayerIndex;
      int count = UnsafeNativeMethods.CRhinoLayerNode_GetChildren(m_doc.m_docId, index, childIndices.m_ptr);
      Layer[] rc = null;
      if (count > 0)
      {
        int[] indices = childIndices.ToArray();
        count = indices.Length;
        rc = new Layer[count];
        for (int i = 0; i < count; i++)
        {
          rc[i] = new Layer(indices[i], m_doc);
        }
      }
      childIndices.Dispose();
      return rc;
    }
#endif
    #endregion

    #region user strings
    /// <summary>
    /// Attach a user string (key,value combination) to this geometry.
    /// </summary>
    /// <param name="key">id used to retrieve this string.</param>
    /// <param name="value">string associated with key.</param>
    /// <returns>true on success.</returns>
    public bool SetUserString(string key, string value)
    {
      return _SetUserString(key, value);
    }
    /// <summary>
    /// Gets user string from this geometry.
    /// </summary>
    /// <param name="key">id used to retrieve the string.</param>
    /// <returns>string associated with the key if successful. null if no key was found.</returns>
    public string GetUserString(string key)
    {
      return _GetUserString(key);
    }

    /// <summary>
    /// Gets the amount of user strings.
    /// </summary>
    public int UserStringCount
    {
      get
      {
        return _UserStringCount;
      }
    }

    ///// <summary>
    ///// Gets a copy of all (user key string, user value string) pairs attached to this geometry.
    ///// </summary>
    ///// <returns>A new collection.</returns>
    //public System.Collections.Specialized.NameValueCollection GetUserStrings()
    //{
    //  return _GetUserStrings();
    //}
    #endregion
  }
}


#if RHINO_SDK
namespace Rhino.DocObjects.Tables
{
  public enum LayerTableEventType : int
  {
    Added = 0,
    Deleted = 1,
    Undeleted = 2,
    Modified = 3,
    /// <summary>LayerTable.Sort() potentially changed sort order.</summary>
    Sorted = 4,
    /// <summary>Current layer change.</summary>
    Current = 5
  }

  public class LayerTableEventArgs : EventArgs
  {
    readonly int m_docId;
    readonly LayerTableEventType m_event_type;
    readonly int m_layer_index;
    readonly IntPtr m_pOldLayer;

    internal LayerTableEventArgs(int docId, int event_type, int index, IntPtr pConstOldLayer)
    {
      m_docId = docId;
      m_event_type = (LayerTableEventType)event_type;
      m_layer_index = index;
      m_pOldLayer = pConstOldLayer;
    }

    RhinoDoc m_doc;
    public RhinoDoc Document
    {
      get { return m_doc ?? (m_doc = RhinoDoc.FromId(m_docId)); }
    }

    public LayerTableEventType EventType
    {
      get { return m_event_type; }
    }

    public int LayerIndex
    {
      get { return m_layer_index; }
    }

    Layer m_new_layer;
    public Layer NewState
    {
      get { return m_new_layer ?? (m_new_layer = new Layer(LayerIndex, Document)); }
    }

    Layer m_old_layer;
    public Layer OldState
    {
      get
      {
        if (m_old_layer == null && m_pOldLayer!=IntPtr.Zero)
        {
          m_old_layer = new Layer(m_pOldLayer);
        }
        return m_old_layer;
      }
    }
  }


  public sealed class LayerTable : IEnumerable<Layer>, Rhino.Collections.IRhinoTable<Layer>
  {
    private readonly RhinoDoc m_doc;
    internal LayerTable(RhinoDoc doc)
    {
      m_doc = doc;
    }

    /// <summary>Document that owns this table.</summary>
    public RhinoDoc Document
    {
      get { return m_doc; }
    }

    /// <summary>
    /// Returns number of layers in the layer table, including deleted layers.
    /// </summary>
    public int Count
    {
      get
      {
        return UnsafeNativeMethods.CRhinoLayerTable_LayerCount(m_doc.m_docId, false);
      }
    }

    /// <summary>
    /// Returns number of layers in the layer table, excluding deleted layers.
    /// </summary>
    public int ActiveCount
    {
      get
      {
        return UnsafeNativeMethods.CRhinoLayerTable_LayerCount(m_doc.m_docId, true);
      }
    }

    /// <summary>
    /// Conceptually, the layer table is an array of layers.
    /// The operator[] can be used to get individual layers. A layer is
    /// either active or deleted and this state is reported by Layer.IsDeleted.
    /// </summary>
    /// <param name="index">zero based array index.</param>
    /// <returns>
    /// Refererence to the layer.  If layer_index is out of range, the current
    /// layer is returned. Note that this reference may become invalid after
    /// AddLayer() is called.
    /// </returns>
    public DocObjects.Layer this[int index]
    {
      get
      {
        if (index < 0 || index >= Count)
          index = CurrentLayerIndex;
        return new Rhino.DocObjects.Layer(index, m_doc);
      }
    }

    /// <summary>
    /// At all times, there is a "current" layer.  Unless otherwise specified, new objects
    /// are assigned to the current layer. The current layer is never locked, hidden, or deleted.
    /// Resturns: Zero based layer table index of the current layer.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_moveobjectstocurrentlayer.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_moveobjectstocurrentlayer.cs' lang='cs'/>
    /// <code source='examples\py\ex_moveobjectstocurrentlayer.py' lang='py'/>
    /// </example>
    public int CurrentLayerIndex
    {
      get
      {
        return UnsafeNativeMethods.CRhinoLayerTable_CurrentLayerIndex(m_doc.m_docId);
      }
    }

    /// <summary>
    /// At all times, there is a "current" layer. Unless otherwise specified, new objects
    /// are assigned to the current layer. The current layer is never locked, hidden, or deleted.
    /// </summary>
    /// <param name="layerIndex">
    /// Value for new current layer. 0 &lt;= layerIndex &lt; LayerTable.Count.
    /// The layer's mode is automatically set to NormalMode.
    /// </param>
    /// <param name="quiet">
    /// if true, then no warning message box pops up if the current layer request can't be satisfied.
    /// </param>
    /// <returns>true if current layer index successfully set.</returns>
    public bool SetCurrentLayerIndex(int layerIndex, bool quiet)
    {
      return UnsafeNativeMethods.CRhinoLayerTable_SetCurrentLayerIndex(m_doc.m_docId, layerIndex, quiet);
    }

    /// <summary>
    /// At all times, there is a "current" layer. Unless otherwise specified,
    /// new objects are assigned to the current layer. The current layer is
    /// never locked, hidden, or deleted.
    /// 
    /// Returns reference to the current layer. Note that this reference may
    /// become invalid after a call to AddLayer().
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_sellayer.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_sellayer.cs' lang='cs'/>
    /// <code source='examples\py\ex_sellayer.py' lang='py'/>
    /// </example>
    public DocObjects.Layer CurrentLayer
    {
      get
      {
        return new Rhino.DocObjects.Layer(CurrentLayerIndex, m_doc);
      }
    }

    /// <summary>
    /// Finds the layer with a given name. If multiple layers exist that have the same name, the
    /// first match layer index will be returned.
    /// </summary>
    /// <param name="layerName">name of layer to search for. The search ignores case.</param>
    /// <param name="ignoreDeletedLayers">true means don't search deleted layers.</param>
    /// <returns>
    /// >=0 index of the layer with the given name
    /// -1  no layer has the given name.
    /// </returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addlayer.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addlayer.cs' lang='cs'/>
    /// <code source='examples\py\ex_addlayer.py' lang='py'/>
    /// </example>
    public int Find(string layerName, bool ignoreDeletedLayers)
    {
      if (string.IsNullOrEmpty(layerName))
        return -1;
      return UnsafeNativeMethods.CRhinoLayerTable_FindLayer(m_doc.m_docId, layerName, ignoreDeletedLayers, -1);
    }

    public int FindNext(int index, string layerName, bool ignoreDeletedLayers)
    {
      if (string.IsNullOrEmpty(layerName))
        return -1;
      return UnsafeNativeMethods.CRhinoLayerTable_FindLayer(m_doc.m_docId, layerName, ignoreDeletedLayers, index);
    }

    public int FindByFullPath(string layerPath, bool ignoreDeletedLayers)
    {
      if (string.IsNullOrEmpty(layerPath))
        return -1;
      return UnsafeNativeMethods.CRhinoLayerTable_FindExact(m_doc.m_docId, layerPath, ignoreDeletedLayers);
    }

    /// <summary>Finds a layer with a matching ID.</summary>
    /// <param name="layerId">A valid layer ID.</param>
    /// <param name="ignoreDeletedLayers">If true, deleted layers are not checked.</param>
    /// <returns>
    /// >=0 index of the layer with the given name
    /// -1  no layer has the given name.
    /// </returns>
    public int Find(Guid layerId, bool ignoreDeletedLayers)
    {
      return UnsafeNativeMethods.CRhinoLayerTable_FindLayer2(m_doc.m_docId, layerId, ignoreDeletedLayers);
    }

    /// <summary>
    /// Adds a new layer with specified definition to the layer table.
    /// </summary>
    /// <param name="layer">
    /// definition of new layer. The information in layer is copied. If layer.Name is empty
    /// the a unique name of the form "Layer 01" will be automatically created.
    /// </param>
    /// <returns>
    /// >=0 index of new layer
    /// -1  layer not added because a layer with that name already exists.
    /// </returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addchildlayer.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addchildlayer.cs' lang='cs'/>
    /// <code source='examples\py\ex_addchildlayer.py' lang='py'/>
    /// </example>
    public int Add(Rhino.DocObjects.Layer layer)
    {
      if (null == layer)
        return -1;
      IntPtr pLayer = layer.ConstPointer();
      return UnsafeNativeMethods.CRhinoLayerTable_AddLayer(m_doc.m_docId, pLayer, false);
    }
    /// <summary>
    /// Adds a new layer with specified definition to the layer table.
    /// </summary>
    /// <param name="layerName">Name for new layer. Cannot be a null or zero-length string.</param>
    /// <param name="layerColor">Color of new layer. Alpha components will be ignored.</param>
    /// <returns>
    /// >=0 index of new layer
    /// -1  layer not added because a layer with that name already exists.
    /// </returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addlayer.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addlayer.cs' lang='cs'/>
    /// <code source='examples\py\ex_addlayer.py' lang='py'/>
    /// </example>
    public int Add(string layerName, Rhino.Drawing.Color layerColor)
    {
      if (string.IsNullOrEmpty(layerName)) { return -1; }

      Layer layer = new Layer();
      layer.Name = layerName;
      layer.Color = layerColor;

      IntPtr pLayer = layer.ConstPointer();
      int rc = UnsafeNativeMethods.CRhinoLayerTable_AddLayer(m_doc.m_docId, pLayer, false);
      layer.Dispose();
      return rc;
    }

    /// <summary>
    /// Adds a new reference layer with specified definition to the layer table
    /// Reference layers are not saved in files.
    /// </summary>
    /// <param name="layer">
    /// definition of new layer. The information in layer is copied. If layer.Name is empty
    /// the a unique name of the form "Layer 01" will be automatically created.
    /// </param>
    /// <returns>
    /// >=0 index of new layer
    /// -1  layer not added because a layer with that name already exists.
    /// </returns>
    public int AddReferenceLayer(Rhino.DocObjects.Layer layer)
    {
      if (null == layer)
        return -1;
      IntPtr pLayer = layer.ConstPointer();
      return UnsafeNativeMethods.CRhinoLayerTable_AddLayer(m_doc.m_docId, pLayer, true);
    }

    /// <summary>
    /// Adds a new layer with default definition to the layer table.
    /// </summary>
    /// <returns>index of new layer.</returns>
    public int Add()
    {
      return UnsafeNativeMethods.CRhinoLayerTable_AddLayer(m_doc.m_docId, IntPtr.Zero, false);
    }
    /// <summary>
    /// Adds a new reference layer with default definition to the layer table.
    /// Reference layers are not saved in files.
    /// </summary>
    /// <returns>index of new layer.</returns>
    public int AddReferenceLayer()
    {
      return UnsafeNativeMethods.CRhinoLayerTable_AddLayer(m_doc.m_docId, IntPtr.Zero, true);
    }

    /// <summary>Modifies layer settings.</summary>
    /// <param name="newSettings">This information is copied.</param>
    /// <param name="layerIndex">
    /// zero based index of layer to set.  This must be in the range 0 &lt;= layerIndex &lt; LayerTable.Count.
    /// </param>
    /// <param name="quiet">if true, information message boxes pop up when illegal changes are attempted.</param>
    /// <returns>
    /// true if successful. false if layerIndex is out of range or the settings attempt
    /// to lock or hide the current layer.
    /// </returns>
    public bool Modify(Rhino.DocObjects.Layer newSettings, int layerIndex, bool quiet)
    {
      if (null == newSettings)
        return false;
      IntPtr pLayer = newSettings.ConstPointer();
      return UnsafeNativeMethods.CRhinoLayerTable_ModifyLayer(m_doc.m_docId, pLayer, layerIndex, quiet);
    }

    /// <summary>
    /// Makes a layer and all of its parent layers visible.
    /// </summary>
    /// <param name="layerId">The layer ID to be made visible.</param>
    /// <returns>true if the operation succeeded.</returns>
    public bool ForceLayerVisible(Guid layerId)
    {
      return UnsafeNativeMethods.CRhinoLayerTable_ForceVisible(m_doc.m_docId, layerId);
    }

    /// <summary>
    /// Makes a layer and all of its parent layers visible.
    /// </summary>
    /// <param name="layerIndex">The layer index to be made visible.</param>
    /// <returns>true if the operation succeeded.</returns>
    public bool ForceLayerVisible(int layerIndex)
    {
      return ForceLayerVisible(this[layerIndex].Id);
    }

    /// <summary>
    /// Restores the layer to its previous state,
    /// if the layer has been modified and the modification can be undone.
    /// </summary>
    /// <param name="layerIndex">The layer index to be used.</param>
    /// <param name="undoRecordSerialNumber">The undo record serial number. Pass 0 not to specify one.</param>
    /// <returns>true if this layer had been modified and the modifications were undone.</returns>
    [CLSCompliant(false)]
    public bool UndoModify(int layerIndex, uint undoRecordSerialNumber)
    {
      return UnsafeNativeMethods.CRhinoLayerTable_UndoModifyLayer(m_doc.m_docId, layerIndex, undoRecordSerialNumber);
    }

    public bool UndoModify(int layerIndex)
    {
      return UnsafeNativeMethods.CRhinoLayerTable_UndoModifyLayer(m_doc.m_docId, layerIndex, 0);
    }

    /// <summary>Deletes layer.</summary>
    /// <param name="layerIndex">
    /// zero based index of layer to delete. This must be in the range 0 &lt;= layerIndex &lt; LayerTable.Count.
    /// </param>
    /// <param name="quiet">
    /// If true, no warning message box appears if a layer the layer cannot be
    /// deleted because it is the current layer or it contains active geometry.
    /// </param>
    /// <returns>
    /// true if successful. false if layerIndex is out of range or the the layer cannot be
    /// deleted because it is the current layer or because it layer contains active geometry.
    /// </returns>
    public bool Delete(int layerIndex, bool quiet)
    {
      return UnsafeNativeMethods.CRhinoLayerTable_DeleteLayer(m_doc.m_docId, layerIndex, quiet, false);
    }

    /// <summary>
    /// Delete layer and all geometry objects on a layer
    /// </summary>
    /// <param name="layerIndex">
    /// zero based index of layer to delete. This must be in the range 0 &lt;= layerIndex &lt; LayerTable.Count.
    /// </param>
    /// <param name="quiet">
    /// If true, no warning message box appears if a layer the layer cannot be
    /// deleted because it is the current layer.
    /// </param>
    /// <returns>
    /// true if successful. false if layerIndex is out of range or the the layer cannot be
    /// deleted because it is the current layer.
    /// </returns>
    public bool Purge(int layerIndex, bool quiet)
    {
      return UnsafeNativeMethods.CRhinoLayerTable_DeleteLayer(m_doc.m_docId, layerIndex, quiet, true);
    }
    //[skipping]
    // int DeleteLayers( int layer_index_count, const int* layer_index_list, bool  bQuiet );

    /// <summary>
    /// Undeletes a layer that has been deleted by DeleteLayer().
    /// </summary>
    /// <param name="layerIndex">
    /// zero based index of layer to undelete.
    /// This must be in the range 0 &lt;= layerIndex &lt; LayerTable.Count.
    /// </param>
    /// <returns>true if successful.</returns>
    public bool Undelete(int layerIndex)
    {
      return UnsafeNativeMethods.CRhinoLayerTable_UndeleteLayer(m_doc.m_docId, layerIndex);
    }

    /// <summary>
    /// Gets the next unused layer name used as default when creating new layers.
    /// </summary>
    /// <param name="ignoreDeleted">
    /// If this is true then Rhino may use a name used by a deleted layer.
    /// </param>
    /// <returns>An unused layer name string.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addlayer.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addlayer.cs' lang='cs'/>
    /// <code source='examples\py\ex_addlayer.py' lang='py'/>
    /// </example>
    public string GetUnusedLayerName(bool ignoreDeleted)
    {
      using (var sh = new StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        UnsafeNativeMethods.CRhinoLayerTable_GetUnusedLayerName(m_doc.m_docId, ignoreDeleted, pString);
        return sh.ToString();
      }
    }

    #region enumerator
    // for IEnumerable<Layer>
    public IEnumerator<Layer> GetEnumerator()
    {
      return new Rhino.Collections.TableEnumerator<LayerTable, Layer>(this);
    }

    // for IEnumerable
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new Rhino.Collections.TableEnumerator<LayerTable, Layer>(this);
    }
    #endregion
  }
}
#endif