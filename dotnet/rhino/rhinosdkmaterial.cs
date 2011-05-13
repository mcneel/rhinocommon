using System;
using System.Runtime.Serialization;

namespace Rhino.DocObjects
{
  [Serializable]
  public class Material : Rhino.Runtime.CommonObject, ISerializable
  {
    #region members
    // Represents both a CRhinoMaterial and an ON_Material. When m_ptr is
    // null, the object uses m_doc and m_id to look up the const
    // CRhinoMaterial in the material table.
    readonly Guid m_id=Guid.Empty;
    readonly RhinoDoc m_doc;
    #endregion

    #region constructors
    public Material()
    {
      // Creates a new non-document control ON_Material
      IntPtr pMaterial = UnsafeNativeMethods.ON_Material_New(IntPtr.Zero);
      base.ConstructNonConstObject(pMaterial);
    }
    internal Material(int index, RhinoDoc doc)
    {
      m_id = UnsafeNativeMethods.CRhinoMaterialTable_GetMaterialId(doc.m_docId, index);
      m_doc = doc;
      this.m__parent = m_doc;
    }

    // Thish is for temporary wrappers. You should always call
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
      base.ConstructNonConstObject(pMaterial);
    }

    // serialization constructor
    protected Material(SerializationInfo info, StreamingContext context)
      : base (info, context)
    {
    }
    #endregion

    internal override IntPtr _InternalGetConstPointer()
    {
      if (m_doc != null)
        return UnsafeNativeMethods.CRhinoMaterialTable_GetMaterialPointer(m_doc.m_docId, m_id);
      return IntPtr.Zero;
    }

    internal override IntPtr _InternalDuplicate(out bool applymempressure)
    {
      applymempressure = false;
      IntPtr pConstPointer = ConstPointer();
      return UnsafeNativeMethods.ON_Object_Duplicate(pConstPointer);
    }

    #region properties
    const int idxIsDeleted = 0;
    const int idxIsReference = 1;
    //const int idxIsModified = 2;
    const int idxIsDefaultMaterial = 3;

    /// <summary>
    /// Deleted materials are kept in the runtime material table so that undo
    /// will work with materials.  Call IsDeleted to determine to determine if
    /// a material is deleted.
    /// </summary>
    public bool IsDeleted
    {
      get
      {
        if (!this.IsDocumentControlled)
          return false;
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.CRhinoMaterial_GetBool(pConstThis, idxIsDeleted);
      }
    }

    /// <summary>
    /// Rhino allows multiple files to be viewed simultaneously. Materials in the
    /// document are "normal" or "reference". Reference materials are not saved.
    /// </summary>
    public bool IsReference
    {
      get
      {
        if (!this.IsDocumentControlled)
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

    /// <summary>
    /// By default Rhino layers and objects are assigned the default rendering material.
    /// </summary>
    public bool IsDefaultMaterial
    {
      get
      {
        if (!this.IsDocumentControlled)
          return false;
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.CRhinoMaterial_GetBool(pConstThis, idxIsDefaultMaterial);
      }
    }

    /// <summary>
    /// Number of objects and layers that use this material.
    /// </summary>
    public int UseCount
    {
      get
      {
        if (!this.IsDocumentControlled)
          return 0;
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.CRhinoMaterial_InUse(pConstThis);
      }
    }

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
    public double Shine
    {
      get { return GetDouble(idxShine); }
      set { SetDouble(idxShine, value); }
    }
    public double Transparency
    {
      get { return GetDouble(idxTransparency); }
      set { SetDouble(idxTransparency, value); }
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
      return System.Drawing.ColorTranslator.FromWin32(abgr);
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
    /// Set material to default settings
    /// </summary>
    public void Default()
    {
      IntPtr pConstThis = NonConstPointer();
      UnsafeNativeMethods.ON_Material_Default(pConstThis);
    }

    const int idxBitmapTexture = 0;
    const int idxBumpTexture = 1;
    const int idxEmapTexture = 2;
    const int idxTransparencyTexture = 3;
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
    /// may be null if no bump texture has been added to this material
    /// </summary>
    /// <returns></returns>
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
      if (m_id == Guid.Empty || IsDocumentControlled)
        return false;
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoMaterialTable_CommitChanges(m_doc.m_docId, pThis, m_id);
    }
  }
}

namespace Rhino.DocObjects.Tables
{
  public sealed class MaterialTable
  {
    private readonly RhinoDoc m_doc;
    private MaterialTable() { }
    internal MaterialTable(RhinoDoc doc)
    {
      m_doc = doc;
    }

    /// <summary>Document that owns this table</summary>
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
    /// either active or deleted and this state is reported by Material.IsDeleted
    /// </summary>
    /// <param name="index">zero based array index</param>
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
    /// Adds a new material to the table based on the default material
    /// </summary>
    /// <returns></returns>
    public int Add()
    {
      return UnsafeNativeMethods.CRhinoMaterialTable_GetInt(m_doc.m_docId, idxAddDefaultMaterial);
    }

    /// <summary>
    /// Finds a meterial with a given name
    /// </summary>
    /// <param name="materialName">Name of the material to search for. The search ignores case.</param>
    /// <param name="ignoreDeletedMaterials">true means don't search deleted materials</param>
    /// <returns>
    /// >=0 index of the material with the given name
    /// -1  no material has the given name
    /// </returns>
    public int Find(string materialName, bool ignoreDeletedMaterials)
    {
      return UnsafeNativeMethods.CRhinoMaterialTable_FindByName(m_doc.m_docId, materialName, ignoreDeletedMaterials);
    }

    /// <summary>Find a material with a matching id</summary>
    /// <param name="materialId"></param>
    /// <param name="ignoreDeletedMaterials">If true, deleted materials are not checked</param>
    /// <returns>
    /// >=0 index of the material with the given name
    /// -1  no material has the given name
    /// </returns>
    public int Find(Guid materialId, bool ignoreDeletedMaterials)
    {
      return UnsafeNativeMethods.CRhinoMaterialTable_FindById(m_doc.m_docId, materialId, ignoreDeletedMaterials);
    }

    /// <summary>Modify material settings</summary>
    /// <param name="newSettings">This information is copied</param>
    /// <param name="materialIndex">
    /// zero based index of material to set.  This must be in the range 0 &lt;= layerIndex &lt; MaterialTable.Count
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
  }
}
