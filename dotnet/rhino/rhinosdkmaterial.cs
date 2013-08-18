#pragma warning disable 1591
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Rhino.DocObjects
{
  class MaterialHolder
  {
    IntPtr m_pConstMaterial;
    public MaterialHolder(IntPtr pConstMaterial)
    {
      m_pConstMaterial = pConstMaterial;
    }
    public void Done()
    {
      m_pConstMaterial = IntPtr.Zero;
    }
    public IntPtr ConstMaterialPointer()
    {
      return m_pConstMaterial;
    }

    Material m_cached_material;
    public Material GetMaterial()
    {
      return m_cached_material ?? (m_cached_material = new Material(this));
    }
  }

  [Serializable]
  public class Material : Rhino.Runtime.CommonObject
  {
    #region members
    // Represents both a CRhinoMaterial and an ON_Material. When m_ptr is
    // null, the object uses m_doc and m_id to look up the const
    // CRhinoMaterial in the material table.
    readonly Guid m_id=Guid.Empty;
#if RHINO_SDK
    readonly RhinoDoc m_doc;
    bool m_is_default;
    static Material m_default_material;
#endif
    #endregion

    #region constructors
    public Material()
    {
      // Creates a new non-document control ON_Material
      IntPtr pMaterial = UnsafeNativeMethods.ON_Material_New(IntPtr.Zero);
      ConstructNonConstObject(pMaterial);
    }
#if RHINO_SDK
    internal Material(int index, RhinoDoc doc)
    {
      m_id = UnsafeNativeMethods.CRhinoMaterialTable_GetMaterialId(doc.m_docId, index);
      m_doc = doc;
      m__parent = m_doc;
    }

    public static Material DefaultMaterial
    {
      get
      {
        if (m_default_material == null || !m_default_material.IsDocumentControlled)
          m_default_material = new Material(true);
        return m_default_material;
      }
    }

    Material(bool defaultMaterial)
    {
      IntPtr pConstThis = UnsafeNativeMethods.CRhinoMaterial_DefaultMaterial();
      m_is_default = true;
      m_id = UnsafeNativeMethods.ON_Material_ModelObjectId(pConstThis);
      m_doc = null;
      m__parent = null;
    }
#endif

    // This is for temporary wrappers. You should always call
    // ReleaseNonConstPointer after you are done using this material
    internal static Material NewTemporaryMaterial(IntPtr pON_Material)
    {
      if (IntPtr.Zero == pON_Material)
        return null;
      Material rc = new Material(pON_Material);
      rc.DoNotDestructOnDispose();
      return rc;
    }
    private Material(IntPtr pMaterial)
    {
      ConstructNonConstObject(pMaterial);
    }

    internal Material(MaterialHolder holder)
    {
      ConstructConstObject(holder, -1);
    }

    internal Material(Guid id, Rhino.FileIO.File3dm parent)
    {
      m_id = id;
      m__parent = parent;
    }

    // serialization constructor
    protected Material(SerializationInfo info, StreamingContext context)
      : base (info, context)
    {
    }
    #endregion

    internal override IntPtr _InternalGetConstPointer()
    {
      MaterialHolder mh = m__parent as MaterialHolder;
      if( mh!=null )
        return mh.ConstMaterialPointer();
#if RHINO_SDK
      if( m_is_default )
        return UnsafeNativeMethods.CRhinoMaterial_DefaultMaterial();
      if (m_doc != null)
        return UnsafeNativeMethods.CRhinoMaterialTable_GetMaterialPointer(m_doc.m_docId, m_id);
#endif
      Rhino.FileIO.File3dm parent_file = m__parent as Rhino.FileIO.File3dm;
      if (parent_file != null)
      {
        IntPtr pModel = parent_file.ConstPointer();
        return UnsafeNativeMethods.ONX_Model_GetMaterialPointer(pModel, m_id);
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
    protected override void OnSwitchToNonConst()
    {
#if RHINO_SDK
      m_is_default = false;
#endif
      base.OnSwitchToNonConst();
    }
    #region properties
    const int idxIsDeleted = 0;
    const int idxIsReference = 1;
    //const int idxIsModified = 2;
    const int idxIsDefaultMaterial = 3;

#if RHINO_SDK
    /// <summary>
    /// Deleted materials are kept in the runtime material table so that undo
    /// will work with materials.  Call IsDeleted to determine to determine if
    /// a material is deleted.
    /// </summary>
    public bool IsDeleted
    {
      get
      {
        if (!IsDocumentControlled)
          return false;
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.CRhinoMaterial_GetBool(pConstThis, idxIsDeleted);
      }
    }
#endif

    /// <summary>Gets or sets the ID of this material.</summary>
    public Guid Id
    {
      get
      {
        IntPtr pMaterial = ConstPointer();
        return UnsafeNativeMethods.ON_Material_ModelObjectId(pMaterial);
      }
    }

    /// <summary>
    /// The Id of the RenderPlugIn that is associated with this material.
    /// </summary>
    public Guid RenderPlugInId
    {
      get
      {
        IntPtr pMaterial = ConstPointer();
        return UnsafeNativeMethods.ON_Material_PlugInId(pMaterial);
      }
      set
      {
        IntPtr pMaterial = NonConstPointer();
        UnsafeNativeMethods.ON_Material_SetPlugInId(pMaterial, value);
      }
    }

#if RHINO_SDK
    /// <summary>
    /// Rhino allows multiple files to be viewed simultaneously. Materials in the
    /// document are "normal" or "reference". Reference materials are not saved.
    /// </summary>
    public bool IsReference
    {
      get
      {
        if (!IsDocumentControlled)
          return false;
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.CRhinoMaterial_GetBool(pConstThis, idxIsReference);
      }
    }

    // IsModified appears to have never been implemented in core Rhino
    //public bool IsModified
    //{
    //  get { return UnsafeNativeMethods.CRhinoMaterial_GetBool(m_doc.m_docId, m_index, idxIsModified); }
    //}
#endif

    /// <summary>
    /// By default Rhino layers and objects are assigned the default rendering material.
    /// </summary>
    public bool IsDefaultMaterial
    {
      get
      {
        if (!IsDocumentControlled)
          return false;
        IntPtr pConstThis = ConstPointer();
#if RHINO_SDK
        return UnsafeNativeMethods.CRhinoMaterial_GetBool(pConstThis, idxIsDefaultMaterial);
#else
        return MaterialIndex == -1;
#endif
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public int MaterialIndex
    {
      get
      {
        IntPtr pConsThis = ConstPointer();
        return UnsafeNativeMethods.ON_Material_Index(pConsThis);
      }
    }

#if RHINO_SDK
    /// <summary>
    /// Number of objects and layers that use this material.
    /// </summary>
    public int UseCount
    {
      get
      {
        if (!IsDocumentControlled)
          return 0;
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.CRhinoMaterial_InUse(pConstThis);
      }
    }
#endif

    public string Name
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        if (IntPtr.Zero == pConstThis)
          return String.Empty;
        using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.ON_Material_GetName(pConstThis, pString);
          return sh.ToString();
        }
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Material_SetName(pThis, value);
      }
    }

    const int idxShine = 0;
    const int idxTransparency = 1;
    const int idxIOR = 2;

    double GetDouble(int which)
    {
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.ON_Material_GetDouble(pConstThis, which);
    }
    void SetDouble(int which, double val)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_Material_SetDouble(pThis, which, val);
    }

    public static double MaxShine
    {
      get { return 255.0; }
    }

    /// <summary>
    /// Gets or sets the shine factor of the material.
    /// </summary>
    public double Shine
    {
      get { return GetDouble(idxShine); }
      set { SetDouble(idxShine, value); }
    }

    /// <summary>
    /// Gets or sets the transparency of the material (0.0 = opaque to 1.0 = transparent)
    /// </summary>
    public double Transparency
    {
      get { return GetDouble(idxTransparency); }
      set { SetDouble(idxTransparency, value); }
    }

    /// <summary>
    /// Gets or sets the index of refraction of the material, generally
    /// >= 1.0 (speed of light in vacuum)/(speed of light in material)
    /// </summary>
    public double IndexOfRefraction
    {
      get { return GetDouble(idxIOR); }
      set { SetDouble(idxIOR, value); }
    }

    const int idxDiffuse = 0;
    const int idxAmbient = 1;
    const int idxEmission = 2;
    const int idxSpecular = 3;
    const int idxReflection = 4;
    const int idxTransparent = 5;
    System.Drawing.Color GetColor(int which)
    {
      IntPtr pConstThis = ConstPointer();
      int abgr = UnsafeNativeMethods.ON_Material_GetColor(pConstThis, which);
      return Rhino.Runtime.Interop.ColorFromWin32(abgr);
    }
    void SetColor(int which, System.Drawing.Color c)
    {
      IntPtr pThis = NonConstPointer();
      int argb = c.ToArgb();
      UnsafeNativeMethods.ON_Material_SetColor(pThis, which, argb);
    }

    public System.Drawing.Color DiffuseColor
    {
      get{ return GetColor(idxDiffuse); }
      set{ SetColor(idxDiffuse, value); }
    }
    public System.Drawing.Color AmbientColor
    {
      get { return GetColor(idxAmbient); }
      set { SetColor(idxAmbient, value); }
    }
    public System.Drawing.Color EmissionColor
    {
      get { return GetColor(idxEmission); }
      set { SetColor(idxEmission, value); }
    }
    public System.Drawing.Color SpecularColor
    {
      get { return GetColor(idxSpecular); }
      set { SetColor(idxSpecular, value); }
    }
    public System.Drawing.Color ReflectionColor
    {
      get { return GetColor(idxReflection); }
      set { SetColor(idxReflection, value); }
    }
    public System.Drawing.Color TransparentColor
    {
      get { return GetColor(idxTransparent); }
      set { SetColor(idxTransparent, value); }
    }
    #endregion

    /// <summary>
    /// Set material to default settings.
    /// </summary>
    public void Default()
    {
      IntPtr pConstThis = NonConstPointer();
      UnsafeNativeMethods.ON_Material_Default(pConstThis);
    }

    internal const int idxBitmapTexture = 0;
    internal const int idxBumpTexture = 1;
    internal const int idxEmapTexture = 2;
    internal const int idxTransparencyTexture = 3;
    bool AddTexture(string filename, int which)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Material_AddTexture(pThis, filename, which);
    }
    bool SetTexture(Texture texture, int which)
    {
      IntPtr pThis = NonConstPointer();
      IntPtr pTexture = texture.ConstPointer();
      return UnsafeNativeMethods.ON_Material_SetTexture(pThis, pTexture, which);
    }
    Texture GetTexture(int which)
    {
      IntPtr pConstThis = ConstPointer();
      int index = UnsafeNativeMethods.ON_Material_GetTexture(pConstThis, which);
      if (index >= 0)
        return new Texture(index, this);
      return null;
    }

#if TODO_RDK_UNCHECKED
    Rhino.Render.RenderMaterial RenderMaterial
    {
        get
        {
            Guid instanceId = UnsafeNativeMethods.Rdk_MaterialFromOnMaterial(ConstPointer());
            return new Rhino.Render.RenderMaterial(instanceId);
        }
        set
        {
            UnsafeNativeMethods.Rdk_SetMaterialToOnMaterial(NonConstPointer(), value.Id);
        }
    }
#endif

    #region Bitmap
    public Texture GetBitmapTexture()
    {
      return GetTexture(idxBitmapTexture);
    }
    public bool SetBitmapTexture(string filename)
    {
      return AddTexture(filename, idxBitmapTexture);
    }
    public bool SetBitmapTexture(Texture texture)
    {
      return SetTexture(texture, idxBitmapTexture);
    }
    #endregion

    #region Bump
    /// <summary>
    /// Gets the bump texture of this material.
    /// </summary>
    /// <returns>A texture; or null if no bump texture has been added to this material.</returns>
    public Texture GetBumpTexture()
    {
      return GetTexture(idxBumpTexture);
    }
    public bool SetBumpTexture(string filename)
    {
      return AddTexture(filename, idxBumpTexture);
    }
    public bool SetBumpTexture(Texture texture)
    {
      return SetTexture(texture, idxBumpTexture);
    }
    #endregion

    #region Environment
    public Texture GetEnvironmentTexture()
    {
      return GetTexture(idxEmapTexture);
    }
    public bool SetEnvironmentTexture(string filename)
    {
      return AddTexture(filename, idxEmapTexture);
    }
    public bool SetEnvironmentTexture(Texture texture)
    {
      return SetTexture(texture, idxEmapTexture);
    }
    #endregion

    #region Transparency
    public Texture GetTransparencyTexture()
    {
      return GetTexture(idxTransparencyTexture);
    }
    public bool SetTransparencyTexture(string filename)
    {
      return AddTexture(filename, idxTransparencyTexture);
    }
    public bool SetTransparencyTexture(Texture texture)
    {
      return SetTexture(texture, idxTransparencyTexture);
    }
    #endregion

    public bool CommitChanges()
    {
#if RHINO_SDK
      if (m_id == Guid.Empty || IsDocumentControlled)
        return false;
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoMaterialTable_CommitChanges(m_doc.m_docId, pThis, m_id);
#else
      return true;
#endif
    }

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
    /// Gets a user string.
    /// </summary>
    /// <param name="key">id used to retrieve the string.</param>
    /// <returns>string associated with the key if successful. null if no key was found.</returns>
    public string GetUserString(string key)
    {
      return _GetUserString(key);
    }

    public int UserStringCount
    {
      get
      {
        return _UserStringCount;
      }
    }

    /// <summary>
    /// Gets an independent copy of the collection of (user text key, user text value) pairs attached to this object.
    /// </summary>
    /// <returns>A collection of key strings and values strings. This </returns>
    public System.Collections.Specialized.NameValueCollection GetUserStrings()
    {
      return _GetUserStrings();
    }
    #endregion

  }
}

#if RHINO_SDK
namespace Rhino.DocObjects.Tables
{
  public enum MaterialTableEventType : int
  {
    Added = 0,
    Deleted = 1,
    Undeleted = 2,
    Modified = 3,
    Sorted = 4,
    Current = 5
  }

  public class MaterialTableEventArgs : EventArgs
  {
    readonly int m_docId;
    readonly MaterialTableEventType m_event_type;
    readonly int m_material_index;
    readonly MaterialHolder m_holder;

    internal MaterialTableEventArgs(int docId, int event_type, int index, IntPtr pOldSettings)
    {
      m_docId = docId;
      m_event_type = (MaterialTableEventType)event_type;
      m_material_index = index;
      if( pOldSettings!=IntPtr.Zero )
        m_holder = new MaterialHolder(pOldSettings);
    }

    internal void Done()
    {
      m_holder.Done();
    }

    RhinoDoc m_doc;
    public RhinoDoc Document
    {
      get { return m_doc ?? (m_doc = RhinoDoc.FromId(m_docId)); }
    }

    public MaterialTableEventType EventType
    {
      get { return m_event_type; }
    }

    public int Index
    {
      get { return m_material_index; }
    }

    public Material OldSettings
    {
      get
      {
        if( m_holder != null )
          return m_holder.GetMaterial();
        return null;
      }
    }
  }

  public sealed class MaterialTable : IEnumerable<Material>, Rhino.Collections.IRhinoTable<Material>
  {
    private readonly RhinoDoc m_doc;
    internal MaterialTable(RhinoDoc doc)
    {
      m_doc = doc;
    }

    /// <summary>Document that owns this table.</summary>
    public RhinoDoc Document
    {
      get { return m_doc; }
    }

    const int idxMaterialCount = 0;
    const int idxCurrentMaterialIndex = 1;
    const int idxCurrentMaterialSource = 2;
    const int idxAddDefaultMaterial = 3;

    /// <summary>
    /// Returns number of materials in the material table, including deleted materials.
    /// </summary>
    public int Count
    {
      get
      {
        return UnsafeNativeMethods.CRhinoMaterialTable_GetInt(m_doc.m_docId, idxMaterialCount);
      }
    }

    /// <summary>
    /// Conceptually, the material table is an array of materials.
    /// The operator[] can be used to get individual materials. A material is
    /// either active or deleted and this state is reported by Material.IsDeleted.
    /// </summary>
    /// <param name="index">zero based array index.</param>
    /// <returns>
    /// If index is out of range, the current material is returned.
    /// </returns>
    public DocObjects.Material this[int index]
    {
      get
      {
        if (index < 0 || index >= Count)
          index = CurrentMaterialIndex;
        return new Rhino.DocObjects.Material(index, m_doc);
      }
    }


    /// <summary>
    /// At all times, there is a "current" material.  Unless otherwise
    /// specified, new objects are assigned to the current material.
    /// The current material is never locked, hidden, or deleted.
    /// </summary>
    public int CurrentMaterialIndex
    {
      get
      {
        return UnsafeNativeMethods.CRhinoMaterialTable_GetInt(m_doc.m_docId, idxCurrentMaterialIndex);
      }
      set
      {
        UnsafeNativeMethods.CRhinoMaterialTable_SetCurrentMaterialIndex(m_doc.m_docId, value, false);
      }
    }

    /// <summary>
    /// Gets or sets the current material source.
    /// </summary>
    public ObjectMaterialSource CurrentMaterialSource
    {
      get
      {
        int rc = UnsafeNativeMethods.CRhinoMaterialTable_GetInt(m_doc.m_docId, idxCurrentMaterialSource);
        return (ObjectMaterialSource)rc;
      }
      set
      {
        UnsafeNativeMethods.CRhinoMaterialTable_SetCurrentMaterialSource(m_doc.m_docId, (int)value);
      }
    }

    /// <summary>
    /// Adds a new material to the table based on the default material.
    /// </summary>
    /// <returns>The position of the new material in the table.</returns>
    public int Add()
    {
      return UnsafeNativeMethods.CRhinoMaterialTable_GetInt(m_doc.m_docId, idxAddDefaultMaterial);
    }

    /// <summary>
    /// Adds a new material to the table based on a given material.
    /// </summary>
    /// <param name="material">A model of the material to be added.</param>
    /// <returns>The position of the new material in the table.</returns>
    public int Add(Material material)
    {
      return Add(material, false);
    }

    /// <summary>
    /// Adds a new material to the table based on a given material.
    /// </summary>
    /// <param name="material">A model of the material to be added.</param>
    /// <param name="reference">
    /// true if this material is supposed to be a reference material.
    /// Reference materials are not saved in the file.
    /// </param>
    /// <returns>The position of the new material in the table.</returns>
    public int Add(Material material, bool reference)
    {
      IntPtr pConstMaterial = material.ConstPointer();
      return UnsafeNativeMethods.CRhinoMaterialTable_Add(m_doc.m_docId, pConstMaterial, reference);
    }

    /// <summary>
    /// Finds a meterial with a given name.
    /// </summary>
    /// <param name="materialName">Name of the material to search for. The search ignores case.</param>
    /// <param name="ignoreDeletedMaterials">true means don't search deleted materials.</param>
    /// <returns>
    /// >=0 index of the material with the given name
    /// -1  no material has the given name.
    /// </returns>
    public int Find(string materialName, bool ignoreDeletedMaterials)
    {
      return UnsafeNativeMethods.CRhinoMaterialTable_FindByName(m_doc.m_docId, materialName, ignoreDeletedMaterials);
    }

    /// <summary>Finds a material with a matching id.</summary>
    /// <param name="materialId">A material ID to be found.</param>
    /// <param name="ignoreDeletedMaterials">If true, deleted materials are not checked.</param>
    /// <returns>
    /// >=0 index of the material with the given name
    /// -1  no material has the given name.
    /// </returns>
    public int Find(Guid materialId, bool ignoreDeletedMaterials)
    {
      return UnsafeNativeMethods.CRhinoMaterialTable_FindById(m_doc.m_docId, materialId, ignoreDeletedMaterials);
    }

    /// <summary>Modify material settings.</summary>
    /// <param name="newSettings">This information is copied.</param>
    /// <param name="materialIndex">
    /// zero based index of material to set.  This must be in the range 0 &lt;= layerIndex &lt; MaterialTable.Count.
    /// </param>
    /// <param name="quiet">if true, information message boxes pop up when illegal changes are attempted.</param>
    /// <returns>
    /// true if successful. false if materialIndex is out of range or the settings attempt
    /// to lock or hide the current material.
    /// </returns>
    public bool Modify(Rhino.DocObjects.Material newSettings, int materialIndex, bool quiet)
    {
      IntPtr pConstMaterial = newSettings.ConstPointer();
      return UnsafeNativeMethods.CRhinoMaterialTable_ModifyMaterial(m_doc.m_docId, pConstMaterial, materialIndex, quiet);
    }

    public bool ResetMaterial(int materialIndex)
    {
      return UnsafeNativeMethods.CRhinoMaterialTable_ResetMaterial(m_doc.m_docId, materialIndex);
    }

    /// <summary>
    /// Removes a material at a specific position from this material table.
    /// </summary>
    /// <param name="materialIndex">The position to be removed.</param>
    /// <returns>
    /// true if successful. false if materialIndex is out of range or the
    /// material cannot be deleted because it is the current material or because
    /// it material contains active geometry.
    /// </returns>
    public bool DeleteAt(int materialIndex)
    {
      return UnsafeNativeMethods.CRhinoMaterialTable_DeleteMaterial(m_doc.m_docId, materialIndex);
    }

    #region enumerator
    public IEnumerator<Material> GetEnumerator()
    {
      return new Rhino.Collections.TableEnumerator<MaterialTable, Material>(this);
    }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new Rhino.Collections.TableEnumerator<MaterialTable, Material>(this);
    }
    #endregion
  }
}
#endif