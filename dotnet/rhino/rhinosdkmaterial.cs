#pragma warning disable 1591
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Rhino.Runtime.InteropWrappers;

namespace Rhino.DocObjects
{
  class MaterialHolder
  {
    IntPtr m_ptr_const_material;
    readonly bool m_is_opennurbs_material;

    public MaterialHolder(IntPtr pConstMaterial, bool isOpenNurbsMaterial)
    {
      m_ptr_const_material = pConstMaterial;
      m_is_opennurbs_material = isOpenNurbsMaterial;
    }
    public void Done()
    {
      m_ptr_const_material = IntPtr.Zero;
    }
    public IntPtr ConstMaterialPointer()
    {
      return m_ptr_const_material;
    }
    public bool IsOpenNurbsMaterial
    {
      get { return m_is_opennurbs_material; }
    }
    Material m_cached_material;
    public Material GetMaterial()
    {
      return m_cached_material ?? (m_cached_material = new Material(this));
    }
  }

  //[Serializable]
  public class Material : Runtime.CommonObject
  {
    #region members
    // Represents both a CRhinoMaterial and an ON_Material. When m_ptr is
    // null, the object uses m_doc and m_id to look up the const
    // CRhinoMaterial in the material table.
    readonly Guid m_id=Guid.Empty;
#if RHINO_SDK
    readonly RhinoDoc m_doc;
    bool m_is_default;
    static Material g_default_material;
#endif
    #endregion

    #region constructors
    public Material()
    {
      // Creates a new non-document control ON_Material
      IntPtr ptr_material = UnsafeNativeMethods.ON_Material_New(IntPtr.Zero);
      ConstructNonConstObject(ptr_material);
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
        if (g_default_material == null || !g_default_material.IsDocumentControlled)
          g_default_material = new Material(true);
        return g_default_material;
      }
    }

    Material(bool defaultMaterial)
    {
      IntPtr ptr_const_material = UnsafeNativeMethods.CRhinoMaterial_DefaultMaterial();
      m_is_default = true;
      m_id = UnsafeNativeMethods.ON_Material_ModelObjectId(ptr_const_material);
      m_doc = null;
      m__parent = null;
    }
#endif

    // This is for temporary wrappers. You should always call
    // ReleaseNonConstPointer after you are done using this material
    internal static Material NewTemporaryMaterial(IntPtr pOpennurbsMaterial)
    {
      if (IntPtr.Zero == pOpennurbsMaterial)
        return null;
      Material rc = new Material(pOpennurbsMaterial);
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

    internal Material(Guid id, FileIO.File3dm parent)
    {
      m_id = id;
      m__parent = parent;
    }

    //// serialization constructor
    //protected Material(SerializationInfo info, StreamingContext context)
    //  : base (info, context)
    //{
    //}
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
      FileIO.File3dm parent_file = m__parent as FileIO.File3dm;
      if (parent_file != null)
      {
        IntPtr ptr_model = parent_file.ConstPointer();
        return UnsafeNativeMethods.ONX_Model_GetMaterialPointer(ptr_model, m_id);
      }
      return IntPtr.Zero;
    }

    internal override IntPtr NonConstPointer()
    {
      if (m__parent is FileIO.File3dm)
        return _InternalGetConstPointer();

      return base.NonConstPointer();
    }

    internal override IntPtr _InternalDuplicate(out bool applymempressure)
    {
      applymempressure = false;
      IntPtr ptr_const_this = ConstPointer();
      return UnsafeNativeMethods.ON_Object_Duplicate(ptr_const_this);
    }
    protected override void OnSwitchToNonConst()
    {
#if RHINO_SDK
      m_is_default = false;
#endif
      base.OnSwitchToNonConst();
    }
    #region properties
    const int IDX_IS_DELETED = 0;
    const int IDX_IS_REFERENCE = 1;
    //const int idxIsModified = 2;
    const int IDX_IS_DEFAULT_MATERIAL = 3;

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
        IntPtr ptr_const_this = ConstPointer();
        return UnsafeNativeMethods.CRhinoMaterial_GetBool(ptr_const_this, IDX_IS_DELETED);
      }
    }
#endif

    /// <summary>Gets or sets the ID of this material.</summary>
    public Guid Id
    {
      get
      {
        IntPtr ptr_const_this = ConstPointer();
        return UnsafeNativeMethods.ON_Material_ModelObjectId(ptr_const_this);
      }
    }

    /// <summary>
    /// The Id of the RenderPlugIn that is associated with this material.
    /// </summary>
    public Guid RenderPlugInId
    {
      get
      {
        IntPtr ptr_const_this = ConstPointer();
        return UnsafeNativeMethods.ON_Material_PlugInId(ptr_const_this);
      }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.ON_Material_SetPlugInId(ptr_this, value);
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
        IntPtr ptr_const_this = ConstPointer();
        return UnsafeNativeMethods.CRhinoMaterial_GetBool(ptr_const_this, IDX_IS_REFERENCE);
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
        IntPtr ptr_const_this = ConstPointer();
#if RHINO_SDK
        return UnsafeNativeMethods.CRhinoMaterial_GetBool(ptr_const_this, IDX_IS_DEFAULT_MATERIAL);
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
        IntPtr ptr_const_this = ConstPointer();
        return UnsafeNativeMethods.ON_Material_Index(ptr_const_this);
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
        IntPtr ptr_const_this = ConstPointer();
        return UnsafeNativeMethods.CRhinoMaterial_InUse(ptr_const_this);
      }
    }

    /// <summary>
    /// If true this object may not be modified. Any properties or functions that attempt
    /// to modify this object when it is set to "IsReadOnly" will throw a NotSupportedException.
    /// </summary>
    public override bool IsDocumentControlled
    {
      get
      {
        MaterialHolder mh = m__parent as MaterialHolder;
        if (mh != null && mh.IsOpenNurbsMaterial)
          return false;
        return base.IsDocumentControlled;
      }
    }

#endif

    public string Name
    {
      get
      {
        IntPtr ptr_const_this = ConstPointer();
        if (IntPtr.Zero == ptr_const_this)
          return String.Empty;
        using (var sh = new StringHolder())
        {
          IntPtr ptr_string = sh.NonConstPointer();
          UnsafeNativeMethods.ON_Material_GetName(ptr_const_this, ptr_string);
          return sh.ToString();
        }
      }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.ON_Material_SetName(ptr_this, value);
      }
    }

    const int IDX_SHINE = 0;
    const int IDX_TRANSPARENCY = 1;
    const int IDX_IOR = 2;
    const int idxReflectivity = 3;

    double GetDouble(int which)
    {
      IntPtr ptr_const_this = ConstPointer();
      return UnsafeNativeMethods.ON_Material_GetDouble(ptr_const_this, which);
    }
    void SetDouble(int which, double val)
    {
      IntPtr ptr_this = NonConstPointer();
      UnsafeNativeMethods.ON_Material_SetDouble(ptr_this, which, val);
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
      get { return GetDouble(IDX_SHINE); }
      set { SetDouble(IDX_SHINE, value); }
    }

    /// <summary>
    /// Gets or sets the transparency of the material (0.0 = opaque to 1.0 = transparent)
    /// </summary>
    public double Transparency
    {
      get { return GetDouble(IDX_TRANSPARENCY); }
      set { SetDouble(IDX_TRANSPARENCY, value); }
    }

    /// <summary>
    /// Gets or sets the index of refraction of the material, generally
    /// >= 1.0 (speed of light in vacuum)/(speed of light in material)
    /// </summary>
    public double IndexOfRefraction
    {
      get { return GetDouble(IDX_IOR); }
      set { SetDouble(IDX_IOR, value); }
    }

    /// <summary>
    /// Gets or sets how reflective a material is, 0f is no reflection
    /// 1f is 100% reflective.
    /// </summary>
    public double Reflectivity
    {
      get { return GetDouble(idxReflectivity); }
      set { SetDouble(idxReflectivity, value); }
    }

    const int IDX_DIFFUSE = 0;
    const int IDX_AMBIENT = 1;
    const int IDX_EMISSION = 2;
    const int IDX_SPECULAR = 3;
    const int IDX_REFLECTION = 4;
    const int IDX_TRANSPARENT = 5;
    Rhino.Drawing.Color GetColor(int which)
    {
      IntPtr ptr_const_this = ConstPointer();
      int abgr = UnsafeNativeMethods.ON_Material_GetColor(ptr_const_this, which);
      return Runtime.Interop.ColorFromWin32(abgr);
    }
    void SetColor(int which, Rhino.Drawing.Color c)
    {
      IntPtr ptr_this = NonConstPointer();
      int argb = c.ToArgb();
      UnsafeNativeMethods.ON_Material_SetColor(ptr_this, which, argb);
    }

    public Rhino.Drawing.Color DiffuseColor
    {
      get{ return GetColor(IDX_DIFFUSE); }
      set{ SetColor(IDX_DIFFUSE, value); }
    }
    public Rhino.Drawing.Color AmbientColor
    {
      get { return GetColor(IDX_AMBIENT); }
      set { SetColor(IDX_AMBIENT, value); }
    }
    public Rhino.Drawing.Color EmissionColor
    {
      get { return GetColor(IDX_EMISSION); }
      set { SetColor(IDX_EMISSION, value); }
    }
    public Rhino.Drawing.Color SpecularColor
    {
      get { return GetColor(IDX_SPECULAR); }
      set { SetColor(IDX_SPECULAR, value); }
    }
    public Rhino.Drawing.Color ReflectionColor
    {
      get { return GetColor(IDX_REFLECTION); }
      set { SetColor(IDX_REFLECTION, value); }
    }
    public Rhino.Drawing.Color TransparentColor
    {
      get { return GetColor(IDX_TRANSPARENT); }
      set { SetColor(IDX_TRANSPARENT, value); }
    }
    #endregion

    /// <summary>
    /// Set material to default settings.
    /// </summary>
    public void Default()
    {
      IntPtr ptr_const_this = NonConstPointer();
      UnsafeNativeMethods.ON_Material_Default(ptr_const_this);
    }

    internal const int idxBitmapTexture = 0;
    internal const int idxBumpTexture = 1;
    internal const int idxEmapTexture = 2;
    internal const int idxTransparencyTexture = 3;
    bool AddTexture(string filename, int which)
    {
      IntPtr ptr_this = NonConstPointer();
      return UnsafeNativeMethods.ON_Material_AddTexture(ptr_this, filename, which);
    }
    bool SetTexture(Texture texture, int which)
    {
      IntPtr ptr_this = NonConstPointer();
      IntPtr ptr_const_texture = texture.ConstPointer();
      return UnsafeNativeMethods.ON_Material_SetTexture(ptr_this, ptr_const_texture, which);
    }
    Texture GetTexture(int which)
    {
      IntPtr ptr_const_this = ConstPointer();
      int index = UnsafeNativeMethods.ON_Material_GetTexture(ptr_const_this, which);
      if (index >= 0)
        return new Texture(index, this);
      return null;
    }

// This is Private and never called so do we really need it?
//#if RDK_UNCHECKED
//    Render.RenderMaterial RenderMaterial
//    {
//      get
//      {
//        var pointer = ConstPointer();
//        var instance_id = UnsafeNativeMethods.Rdk_MaterialFromOnMaterial(pointer);
//        return Render.RenderContent.FromId(m_doc, instance_id) as Render.RenderMaterial;
//      }
//      set
//      {
//        var pointer = NonConstPointer();
//        var id = (value == null ? Guid.Empty : value.Id);
//        UnsafeNativeMethods.Rdk_SetMaterialToOnMaterial(pointer, id);
//      }
//    }
//#endif

    /// <summary>
    /// Get array of textures that this material uses
    /// </summary>
    /// <returns></returns>
    public Texture[] GetTextures()
    {
      IntPtr ptr_const_this = ConstPointer();
      int count = UnsafeNativeMethods.ON_Material_GetTextureCount(ptr_const_this);
      Texture[] rc = new Texture[count];
      for (int i = 0; i < count; i++)
        rc[i] = new Texture(i, this);
      return rc;
    }

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
      IntPtr ptr_this = NonConstPointer();
      return UnsafeNativeMethods.CRhinoMaterialTable_CommitChanges(m_doc.m_docId, ptr_this, m_id);
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

    ///// <summary>
    ///// Gets an independent copy of the collection of (user text key, user text value) pairs attached to this object.
    ///// </summary>
    ///// <returns>A collection of key strings and values strings. This </returns>
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
    readonly int m_document_id;
    readonly MaterialTableEventType m_event_type;
    readonly int m_material_index;
    readonly MaterialHolder m_holder;

    internal MaterialTableEventArgs(int docId, int eventType, int index, IntPtr pOldSettings)
    {
      m_document_id = docId;
      m_event_type = (MaterialTableEventType)eventType;
      m_material_index = index;
      if( pOldSettings!=IntPtr.Zero )
        m_holder = new MaterialHolder(pOldSettings, true);
    }

    internal void Done()
    {
      m_holder.Done();
    }

    RhinoDoc m_doc;
    public RhinoDoc Document
    {
      get { return m_doc ?? (m_doc = RhinoDoc.FromId(m_document_id)); }
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

  public sealed class MaterialTable : IEnumerable<Material>, Collections.IRhinoTable<Material>
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

    const int IDX_MATERIAL_COUNT = 0;
    const int IDX_CURRENT_MATERIAL_INDEX = 1;
    const int IDX_CURRENT_MATERIAL_SOURCE = 2;
    const int IDX_ADD_DEFAULT_MATERIAL = 3;

    /// <summary>
    /// Returns number of materials in the material table, including deleted materials.
    /// </summary>
    public int Count
    {
      get
      {
        return UnsafeNativeMethods.CRhinoMaterialTable_GetInt(m_doc.m_docId, IDX_MATERIAL_COUNT);
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
    public Material this[int index]
    {
      get
      {
        if (index < 0 || index >= Count)
          index = CurrentMaterialIndex;
        return new Material(index, m_doc);
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
        return UnsafeNativeMethods.CRhinoMaterialTable_GetInt(m_doc.m_docId, IDX_CURRENT_MATERIAL_INDEX);
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
        int rc = UnsafeNativeMethods.CRhinoMaterialTable_GetInt(m_doc.m_docId, IDX_CURRENT_MATERIAL_SOURCE);
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
      return UnsafeNativeMethods.CRhinoMaterialTable_GetInt(m_doc.m_docId, IDX_ADD_DEFAULT_MATERIAL);
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
      IntPtr ptr_const_material = material.ConstPointer();
      return UnsafeNativeMethods.CRhinoMaterialTable_Add(m_doc.m_docId, ptr_const_material, reference);
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
    public bool Modify(Material newSettings, int materialIndex, bool quiet)
    {
      IntPtr ptr_const_material = newSettings.ConstPointer();
      return UnsafeNativeMethods.CRhinoMaterialTable_ModifyMaterial(m_doc.m_docId, ptr_const_material, materialIndex, quiet);
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
      return new Collections.TableEnumerator<MaterialTable, Material>(this);
    }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new Collections.TableEnumerator<MaterialTable, Material>(this);
    }
    #endregion
  }
}
#endif