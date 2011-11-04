#pragma warning disable 1591
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Rhino.DocObjects
{
  [Serializable]
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
      base.ConstructNonConstObject(pLayer);
    }

#if RHINO_SDK
    internal Layer(int index, Rhino.RhinoDoc doc)
    {
      m_id = UnsafeNativeMethods.CRhinoLayerTable_GetLayerId(doc.m_docId, index);
      m_doc = doc;
      this.m__parent = m_doc;
    }
#endif

    internal Layer(Guid id, Rhino.FileIO.File3dm onxModel)
    {
      m_id = id;
      m_onx_model = onxModel;
      this.m__parent = onxModel;
    }

    // serialization constructor
    protected Layer(SerializationInfo info, StreamingContext context)
      : base (info, context)
    {
    }

    /// <summary>
    /// Creates a Layer with the current default layer properties.
    /// The default layer properties are:
    /// color = Rhino.ApplicationSettings.AppearanceSettings.DefaultLayerColor
    /// line style = Rhino.ApplicationSettings.AppearanceSettings.DefaultLayerLineStyle
    /// material index = -1
    /// iges level = -1
    /// mode = NormalLayer
    /// name = empty
    /// layer index = 0 (ignored by AddLayer)
    /// </summary>
    /// <returns></returns>
    public static Layer GetDefaultLayerProperties()
    {
      Layer layer = new Layer();
      IntPtr ptr = layer.NonConstPointer();
      UnsafeNativeMethods.CRhinoLayerTable_GetDefaultLayerProperties(ptr);
      return layer;
    }
    #endregion

    public bool CommitChanges()
    {
#if RHINO_SDK
      if (m_id == Guid.Empty || IsDocumentControlled)
        return false;
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoLayerTable_CommitChanges(m_doc.m_docId, pThis, m_id);
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
      return IntPtr.Zero;
    }
    
    internal override IntPtr _InternalDuplicate(out bool applymempressure)
    {
      applymempressure = false;
      IntPtr pConstPointer = ConstPointer();
      return UnsafeNativeMethods.ON_Object_Duplicate(pConstPointer);
    }

    const int idxIsVisible = 0;
    const int idxIsLocked = 1;
    const int idxIsExpanded = 2;
    #region properties
    /// <summary>The name of this layer</summary>
    /// <example>
    /// <code source='examples\vbnet\ex_sellayer.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_sellayer.cs' lang='cs'/>
    /// <code source='examples\py\ex_sellayer.py' lang='py'/>
    /// </example>
    public string Name
    {
      get
      {
        using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
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

    public string FullPath
    {
      get
      {
#if RHINO_SDK
        if (null == m_doc)
          return Name;
        int index = LayerIndex;
        using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
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
    /// Gets the index of this layer.
    /// </summary>
    public int LayerIndex
    {
      get { return GetInt(idxLayerIndex); }
      set { SetInt(idxLayerIndex, value); }
    }

    /// <summary>
    /// Gets the ID of this layer object.
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
    /// Gets the IGES level for this layer.
    /// </summary>
    public int IgesLevel
    {
      get { return GetInt(idxIgesLevel); }
      set { SetInt(idxIgesLevel, value); }
    }

    /// <summary>
    /// Gets the display color for this layer.
    /// </summary>
    public System.Drawing.Color Color
    {
      get
      {
        IntPtr pConstLayer = ConstPointer();
        int abgr = UnsafeNativeMethods.ON_Layer_GetColor(pConstLayer, true);
        return System.Drawing.ColorTranslator.FromWin32(abgr);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        int argb = value.ToArgb();
        UnsafeNativeMethods.ON_Layer_SetColor(pThis, argb, true);
      }
    }

    /// <summary>
    /// Gets the plot color for this layer.
    /// </summary>
    public System.Drawing.Color PlotColor
    {
      get
      {
        IntPtr pConstLayer = ConstPointer();
        int abgr = UnsafeNativeMethods.ON_Layer_GetColor(pConstLayer, false);
        return System.Drawing.ColorTranslator.FromWin32(abgr);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        int argb = value.ToArgb();
        UnsafeNativeMethods.ON_Layer_SetColor(pThis, argb, false);
      }
    }

    /// <summary>
    /// Gets the thickness of the plotting pen in millimeters. 
    /// A thickness of 0.0 indicates the "default" pen weight should be used.
    /// </summary>
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
    /// Gets the line-type index for this layer.
    /// </summary>
    public int LinetypeIndex
    {
      get { return GetInt(idxLinetypeIndex); }
      set { SetInt(idxLinetypeIndex, value); }
    }

    /// <summary>
    /// Gets the index of render material for objects on this layer that have
    /// MaterialSource() == MaterialFromLayer. 
    /// A material index of -1 indicates no material has been assigned 
    /// and the material created by the default Material constructor 
    /// should be used.
    /// </summary>
    public int RenderMaterialIndex
    {
      get { return GetInt(idxRenderMaterialIndex); }
      set { SetInt(idxRenderMaterialIndex, value); }
    }

    /// <summary>
    /// Gets the visibility of this layer.
    /// </summary>
    public bool IsVisible
    {
      get { return GetBool(idxIsVisible); }
      set { SetBool(idxIsVisible, value); }
    }

    /// <summary>
    /// Gets a value indicating the locked state of this layer.
    /// </summary>
    public bool IsLocked
    {
      get { return GetBool(idxIsLocked); }
      set { SetBool(idxIsLocked, value); }
    }

    /// <summary>
    /// Gets a value indicating whether this layer is expanded in the Rhino Layer dialog.
    /// </summary>
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

#if RDK_UNCHECKED
    public Guid RenderMaterialInstanceId
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.Rdk_RenderContent_LayerMaterialInstanceId(pConstThis);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.Rdk_RenderContent_SetLayerMaterialInstanceId(pThis, value);
      }
    }
    public Rhino.Render.RenderMaterial RenderMaterial
    {
      get
      {
        return Rhino.Render.RenderContent.FromInstanceId(RenderMaterialInstanceId) as Rhino.Render.RenderMaterial;
      }
      set
      {
        RenderMaterialInstanceId = value.InstanceId;
      }
    }
#endif

    /// <summary>Runtime index used to sort layers in layer dialog</summary>
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
    /// Set layer to default settings
    /// </summary>
    public void Default()
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_Layer_Default(pThis);
    }

    /// <summary>
    /// Determine if a given string is valid for a layer name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addlayer.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addlayer.cs' lang='cs'/>
    /// <code source='examples\py\ex_addlayer.py' lang='py'/>
    /// </example>
    public static bool IsValidName(string name)
    {
      return UnsafeNativeMethods.RHC_IsValidName(name);
    }

    #region methods
#if RHINO_SDK

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
    /// Get immediate children of this layer. Note that child layers may have their own children
    /// </summary>
    /// <returns>Array of child layers. null if this layer does not have any children</returns>
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
  }
}


#if RHINO_SDK
namespace Rhino.DocObjects.Tables
{
  public sealed class LayerTable : IEnumerable<Layer>, Rhino.Collections.IRhinoTable<Layer>
  {
    private readonly RhinoDoc m_doc;
    private LayerTable() { }
    internal LayerTable(RhinoDoc doc)
    {
      m_doc = doc;
    }

    /// <summary>Document that owns this table</summary>
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
    /// Returns number of layers in the layer table, excluding deleted layers
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
    /// either active or deleted and this state is reported by Layer.IsDeleted
    /// </summary>
    /// <param name="index">zero based array index</param>
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
    /// Resturns: Zero based layer table index of the current layer
    /// </summary>
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
    /// -1  no layer has the given name
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

#if USING_V5_SDK
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
#endif

    /// <summary>Find a layer with a matching id</summary>
    /// <param name="layerId"></param>
    /// <param name="ignoreDeletedLayers">If true, deleted layers are not checked</param>
    /// <returns>
    /// >=0 index of the layer with the given name
    /// -1  no layer has the given name
    /// </returns>
    public int Find(Guid layerId, bool ignoreDeletedLayers)
    {
      return UnsafeNativeMethods.CRhinoLayerTable_FindLayer2(m_doc.m_docId, layerId, ignoreDeletedLayers);
    }

    /// <summary>
    /// Adds a new layer with specified definition to the layer table
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
    /// Adds a new layer with specified definition to the layer table
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
    public int Add(string layerName, System.Drawing.Color layerColor)
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
    /// Reference layers are not saved in files
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
    /// <returns>index of new layer</returns>
    public int Add()
    {
      return UnsafeNativeMethods.CRhinoLayerTable_AddLayer(m_doc.m_docId, IntPtr.Zero, false);
    }
    /// <summary>
    /// Adds a new reference layer with default definition to the layer table.
    /// Reference layers are not saved in files
    /// </summary>
    /// <returns>index of new layer</returns>
    public int AddReferenceLayer()
    {
      return UnsafeNativeMethods.CRhinoLayerTable_AddLayer(m_doc.m_docId, IntPtr.Zero, true);
    }

    /// <summary>Modify layer settings</summary>
    /// <param name="newSettings">This information is copied</param>
    /// <param name="layerIndex">
    /// zero based index of layer to set.  This must be in the range 0 &lt;= layerIndex &lt; LayerTable.Count
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
    /// Make a layer and all of it's parent layers visible
    /// </summary>
    /// <param name="layerId"></param>
    /// <returns></returns>
    public bool ForceLayerVisible(Guid layerId)
    {
      return UnsafeNativeMethods.CRhinoLayerTable_ForceVisible(m_doc.m_docId, layerId);
    }

    /// <summary>
    /// Make a layer and all of it's parent layers visible
    /// </summary>
    /// <param name="layerIndex"></param>
    /// <returns></returns>
    public bool ForceLayerVisible(int layerIndex)
    {
      return ForceLayerVisible(this[layerIndex].Id);
    }

    /// <summary>
    /// If the layer has been modified and the modifcation can be undone,
    /// then UndoModifyLayer() will restore the layer to its previous state.
    /// </summary>
    /// <param name="layerIndex"></param>
    /// <param name="undoRecordSerialNumber"></param>
    /// <returns>true if this layer had been modified and the modifications were undone</returns>
    [CLSCompliant(false)]
    public bool UndoModify(int layerIndex, uint undoRecordSerialNumber)
    {
      return UnsafeNativeMethods.CRhinoLayerTable_UndoModifyLayer(m_doc.m_docId, layerIndex, undoRecordSerialNumber);
    }

    public bool UndoModify(int layerIndex)
    {
      return UnsafeNativeMethods.CRhinoLayerTable_UndoModifyLayer(m_doc.m_docId, layerIndex, 0);
    }

    /// <summary>Deletes layer</summary>
    /// <param name="layerIndex">
    /// zero based index of layer to delete. This must be in the range 0 &lt;= layerIndex &lt; LayerTable.Count
    /// </param>
    /// <param name="quiet">
    /// If true, no warning message box appears if a layer the layer cannot be
    /// deleted because it is the current layer or it contains active geometry.
    /// </param>
    /// <returns>
    /// true if successful. false if layer_index is out of range or the the layer cannot be
    /// deleted because it is the current layer or because it layer contains active geometry.
    /// </returns>
    public bool Delete(int layerIndex, bool quiet)
    {
      return UnsafeNativeMethods.CRhinoLayerTable_DeleteLayer(m_doc.m_docId, layerIndex, quiet);
    }

    //[skipping]
    // int DeleteLayers( int layer_index_count, const int* layer_index_list, bool  bQuiet );

    /// <summary>
    /// Undeletes a layer that has been deleted by DeleteLayer().
    /// </summary>
    /// <param name="layerIndex">
    /// zero based index of layer to undelete.
    /// This must be in the range 0 &lt;= layerIndex &lt; LayerTable.Count
    /// </param>
    /// <returns>true if successful</returns>
    public bool Undelete(int layerIndex)
    {
      return UnsafeNativeMethods.CRhinoLayerTable_UndeleteLayer(m_doc.m_docId, layerIndex);
    }

    /// <summary>
    /// Gets unused layer name used as default when creating new layers.
    /// </summary>
    /// <param name="ignoreDeleted">
    /// if this is true then may use a name used by a deleted layer.
    /// </param>
    /// <returns></returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addlayer.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addlayer.cs' lang='cs'/>
    /// <code source='examples\py\ex_addlayer.py' lang='py'/>
    /// </example>
    public string GetUnusedLayerName(bool ignoreDeleted)
    {
      using (Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
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