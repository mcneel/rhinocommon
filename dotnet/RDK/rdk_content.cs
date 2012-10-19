#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

#if RDK_UNCHECKED

namespace Rhino.Render
{
  [Flags]
  public enum RenderContentStyles : int
  {
    /// <summary>
    /// No defined styles
    /// </summary>
    None = 0,
    /// <summary>
    /// Texture UI includes an auto texture summary section. See AddAutoParameters().
    /// </summary>
    TextureSummary = 0x0001,
    /// <summary>
    /// Editor displays an instant preview before preview cycle begins.
    /// </summary>
    QuickPreview = 0x0002,
    /// <summary>
    /// Content's preview imagery can be stored in the preview cache.
    /// </summary>
    PreviewCache = 0x0004,
    /// <summary>
    /// Content's preview imagery can be rendered progressively.
    /// </summary>
    ProgressivePreview = 0x0008,
    /// <summary>
    /// Texture UI includes an auto local mapping section for textures. See AddAutoParameters()
    /// </summary>
    LocalTextureMapping = 0x0010,
    /// <summary>
    /// Texture UI includes a graph section.
    /// </summary>
    GraphDisplay = 0x0020,
    /// <summary>
    /// Content supports UI sharing between contents of the same type id.
    /// </summary>
    SharedUI = 0x0040,
    /// <summary>
    /// Texture UI includes an adjustment section.
    /// </summary>
    Adjustment = 0x0080,
    /// <summary>
    /// Content uses fields to facilitate data storage and undo support. See Fields()
    /// </summary>
    Fields = 0x0100,
    /// <summary>
    /// Content supports editing in a modal editor.
    /// </summary>
    ModalEditing = 0x0200,
  }

  [AttributeUsage(AttributeTargets.Class)]
  public sealed class CustomRenderContentAttribute : System.Attribute
  {
    private RenderContentStyles m_style = RenderContentStyles.None;
    private readonly Guid m_renderengine_id;

    public CustomRenderContentAttribute()
    {
      m_renderengine_id = Guid.Empty;
    }
    public CustomRenderContentAttribute(String renderEngineGuid)
    {
      m_renderengine_id = new Guid(renderEngineGuid);
    }

    public Guid RenderEngineId
    {
      get { return m_renderengine_id; }
    }

    public RenderContentStyles Styles
    {
      get { return m_style; }
      set { m_style = value; }
    }

    public bool ImageBased { get; set; }
  }

  /// <summary>
  /// Defines constant values for all render content kinds, such as material, environment or texture.
  /// </summary>
  [Flags]
  public enum RenderContentKind : int
  {
    None = 0,
    Material = 1,
    Environment = 2,
    Texture = 4,
  }

  public abstract class RenderContent : IDisposable
  {
    #region Kinds

    static RenderContentKind KindFromString(string kind)
    {
      RenderContentKind k = RenderContentKind.None;
      if (kind.Contains("material"))
        k |= RenderContentKind.Material;
      if (kind.Contains("environment"))
        k |= RenderContentKind.Environment;
      if (kind.Contains("texture"))
        k |= RenderContentKind.Texture;
      return k;
    }

    internal static String KindString(RenderContentKind kinds)
    {
      System.Text.StringBuilder sb = new System.Text.StringBuilder();

      if ((kinds & RenderContentKind.Material) == RenderContentKind.Material)
      {
        sb.Append("material");
      }

      if ((kinds & RenderContentKind.Environment) == RenderContentKind.Environment)
      {
        if (sb.Length != 0)
        {
          sb.Append(";");
        }
        sb.Append("environment");
      }

      if ((kinds & RenderContentKind.Texture) == RenderContentKind.Texture)
      {
        if (sb.Length != 0)
        {
          sb.Append(";");
        }
        sb.Append("texture");
      }
      return sb.ToString();
    }

    /// <summary>
    /// Internal string ids to be used in the GetString method.
    /// </summary>
    internal enum StringIds : int
    {
      Kind = 0,
      Name = 1,
      Notes = 2,
      TypeName = 3,
      TypeDescription = 4,
      ChildSlotName = 5,
      Xml = 6,

      //Material specific
      DiffuseChildSlotName = 100,
      TransparencyChildSlotName = 101,
      BumpChildSlotName = 102,
      EnvironmentChildSlotName = 103,
    }
    #endregion

    #region statics
    public enum ShowContentChooserFlags : int
    {
      None = 0x0000,
      HideNewTab = 0x0001,
      HideExistingTab = 0x0002,
    };

    /// <summary>
    /// Constructs a new content of the specified type and add it to the persistent content list.
    /// This function cannot be used to create temporary content that you delete after use.
    /// Content created by this function is owned by RDK and appears in the content editor.
    /// To create a temporary content which is owned by you, call RhRdkContentFactories().NewContentFromType().
    /// </summary>
    /// <param name="type">Is the type of the content to add.</param>
    /// <param name="flags">Options for the tab.</param>
    /// <param name="doc">The current Rhino document.</param>
    /// <returns>A new persistent render content.</returns>
    public static RenderContent Create(Guid type, ShowContentChooserFlags flags, RhinoDoc doc)
    {
      IntPtr pContent = UnsafeNativeMethods.Rdk_Globals_CreateContentByType(type, IntPtr.Zero, String.Empty, (int)flags, doc.m_docId);
      return pContent == IntPtr.Zero ? null : FromPointer(pContent);
    }

    /// <summary>
    /// Constructs a new content of the specified type and add it to the persistent content list.
    /// This function cannot be used to create temporary content that you delete after use.
    /// Content created by this function is owned by RDK and appears in the content editor.
    /// To create a temporary content which is owned by you, call RhRdkContentFactories().NewContentFromType().
    /// </summary>
    /// <param name="type">Is the type of the content to add.</param>
    /// <param name="flags">Options for the tab.</param>
    /// <param name="doc">The current Rhino document.</param>
    /// <returns>A new persistent render content.</returns>
    public static RenderContent Create(Type type, ShowContentChooserFlags flags, RhinoDoc doc)
    {
      return Create(type.GUID, flags, doc);
    }

    /// <summary>
    /// Constructs a new content of the specified type and add it to the persistent content list.
    /// This function cannot be used to create temporary content that you delete after use.
    /// Content created by this function is owned by RDK and appears in the content editor.
    /// To create a temporary content which is owned by you, call RhRdkContentFactories().NewContentFromType().
    /// </summary>
    /// <param name="type">is the type of the content to add.</param>
    /// <param name="parent">Parent is the parent content. If not NULL, this must be an RDK-owned content that is
    /// in the persistent content list (either top-level or child). The new content then becomes its child.
    /// If NULL, the new content is added to the top-level content list instead.</param>
    /// <param name="childSlotName">ChildSlotName is the unique child identifier to use for the new content when creating it as a child of pParent (i.e., when pParent is not NULL)</param>
    /// <param name="flags">Options for the tab.</param>
    /// <param name="doc">The current Rhino document.</param>
    /// <returns>A new persistent render content.</returns>
    public static RenderContent Create(Guid type, RenderContent parent, String childSlotName, ShowContentChooserFlags flags, RhinoDoc doc)
    {
      IntPtr pContent = UnsafeNativeMethods.Rdk_Globals_CreateContentByType(type, parent.ConstPointer(), childSlotName, (int)flags, doc.m_docId);
      return pContent == IntPtr.Zero ? null : FromPointer(pContent);
    }

    /// <summary>
    /// Constructs a new content of the specified type and add it to the persistent content list.
    /// This function cannot be used to create temporary content that you delete after use.
    /// Content created by this function is owned by RDK and appears in the content editor.
    /// To create a temporary content which is owned by you, call RhRdkContentFactories().NewContentFromType().
    /// </summary>
    /// <param name="type">is the type of the content to add.</param>
    /// <param name="parent">Parent is the parent content. If not NULL, this must be an RDK-owned content that is
    /// in the persistent content list (either top-level or child). The new content then becomes its child.
    /// If NULL, the new content is added to the top-level content list instead.</param>
    /// <param name="childSlotName">ChildSlotName is the unique child identifier to use for the new content when creating it as a child of pParent (i.e., when pParent is not NULL)</param>
    /// <param name="flags">Options for the tab.</param>
    /// <param name="doc">The current Rhino document.</param>
    /// <returns>A new persistent render content.</returns>
    public static RenderContent Create(Type type, RenderContent parent, String childSlotName, ShowContentChooserFlags flags, RhinoDoc doc)
    {
      return Create(type.GUID, parent, childSlotName, flags, doc);
    }

    /// <summary>
    /// Render content is automatically registered for the Assembly that a plug-in is defined in. If
    /// you have content defined in a different assembly (for example a Grasshopper component), then
    /// you need to explicitly call RegisterContent.
    /// </summary>
    /// <param name="assembly">
    /// Assembly where custom content is defined, this may be a plug-in assembly
    /// or another assembly referenced by the plug-in.
    /// </param>
    /// <param name="pluginId">Parent plug-in for this assembly.</param>
    /// <returns>array of render content types registered on success. null on error.</returns>
    public static Type[] RegisterContent(Assembly assembly, Guid pluginId)
    {
      // Check the Rhino plug-in for a RhinoPlugIn with the specified Id
      var plugin = PlugIns.PlugIn.GetLoadedPlugIn(pluginId);
      // RhinoPlugIn not found so bail, all content gets associated with a plug-in!
      if (plugin == null) return null;
      // Get a list of the publicly exported class types from the requested assembly
      var exportedTypes = assembly.GetExportedTypes();
      // Scan the exported class types for RenderContent derived classes
      var contentTypes = new List<Type>();
      var renderContentType = typeof (RenderContent);
      for (int i = 0; i < exportedTypes.Length; i++)
      {
        var exportedType = exportedTypes[i];
        // If abstract class or not derived from RenderContent or does not contain a public constructor then skip it
        if (exportedType.IsAbstract || !exportedType.IsSubclassOf(renderContentType) || exportedType.GetConstructor(new Type[] { }) == null)
          continue;
        // Check the class type for a GUID custom attribute
        var attr = exportedType.GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false);
        // If the class does not have a GUID custom attribute then throw an exception
        if (attr.Length < 1) throw new InvalidDataException("Class \"" + exportedType + "\" must include a GUID attribute");
        // Add the class type to the content list
        contentTypes.Add(exportedType);
      }
      // If this plug-in does not contain any valid RenderContent derived objects then bail
      if (contentTypes.Count == 0) return null;

      // make sure that content types have not already been registered
      foreach (var contentType in contentTypes)
        if (RdkPlugIn.RenderContentTypeIsRegistered(contentType))
          return null; // Bail out because this type was previously registered

      // Get the RdkPlugIn associated with this RhinoPlugIn, if it is not in
      // the RdkPlugIn dictionary it will get added if possible.
      var rdkPlugIn = RdkPlugIn.GetRdkPlugIn(plugin);

      // Plug-in not found or there was a problem adding it to the dictionary
      if (rdkPlugIn == null) return null;

      // Append the RdkPlugIn registered content type list
      rdkPlugIn.AddRegisteredContentTypes(contentTypes);

      // Process the valid class type list and register each class with the
      // appropriate C++ RDK class factory
      var textureType = typeof(RenderTexture);
      var materialType = typeof(RenderMaterial);
      var enviornmentType = typeof(RenderEnvironment);
      foreach (var contentType in contentTypes)
      {
        var id = contentType.GUID;
        if (contentType.IsSubclassOf(textureType))
          UnsafeNativeMethods.Rdk_AddTextureFactory(id);
        if (contentType.IsSubclassOf(materialType))
          UnsafeNativeMethods.Rdk_AddMaterialFactory(id);
        if (contentType.IsSubclassOf(enviornmentType))
          UnsafeNativeMethods.Rdk_AddEnvironmentFactory(id);
      }

      // Return an array of the valid content types
      return contentTypes.ToArray();
    }
    /// <summary>
    /// Search for a C++ pointer of the requested Id
    /// </summary>
    /// <param name="instanceId"></param>
    /// <returns></returns>
    internal static RenderContent FromInstanceId(Guid instanceId)
    {
      IntPtr renderContent = UnsafeNativeMethods.Rdk_FindContentInstance(instanceId);
      if (renderContent == IntPtr.Zero)
        return null;
      return FromPointer(renderContent);
    }
    /// <summary>
    /// Create a .NET object of the appropriate type and attach it to the
    /// requested C++ pointer
    /// </summary>
    /// <param name="renderContent"></param>
    /// <returns></returns>
    internal static RenderContent FromPointer(IntPtr renderContent)
    {
      // If null C++ pointer then bail
      if (renderContent == IntPtr.Zero) return null;
      // Get the runtime memory serial number for the requested item
      var serialNumber = UnsafeNativeMethods.CRhCmnRenderContent_IsRhCmnDefined(renderContent);
      // If the object has been created and not disposed of then return it
      if (serialNumber > 0) return FromSerialNumber(serialNumber);
      // Could not find the object in the runtime list so check to see if the C++
      // pointer is a CRhRdkTexture pointer 
      IntPtr pTexture = UnsafeNativeMethods.Rdk_RenderContent_DynamicCastToTexture(renderContent);
      // Is a RenderTexture so create one and attach it to the requested C++ pointer
      if (pTexture != IntPtr.Zero) return new NativeRenderTexture(pTexture);
      // Check to see if the C++ pointer is a CRhRdkMaterial pointer
      IntPtr pMaterial = UnsafeNativeMethods.Rdk_RenderContent_DynamicCastToMaterial(renderContent);
      // It is a RenderMaterial so create one and attach it to the requested C++ pointer
      if (pMaterial != IntPtr.Zero) return new NativeRenderMaterial(pMaterial);
      // Check to see if the C++ pointer is a CRhRdkEnviornmen pointer
      IntPtr pEnvironment = UnsafeNativeMethods.Rdk_RenderContent_DynamicCastToEnvironment(renderContent);
      // It is a RenderEnviornment so create one and attach it to the requested C++ pointer
      if (pEnvironment != IntPtr.Zero) return new NativeRenderEnvironment(pEnvironment);
      //This should never, ever, happen.
      throw new InvalidCastException("renderContent Pointer is not of a recognized type");
    }
    #endregion

    // -1 == Disposed content
    /// <summary>
    /// Serial number of the created object, valid values:
    ///   -1  == OnDeleteRhCmnRenderContent() was called with this serial number
    ///   >0  == Value set by the constructor
    /// </summary>
    internal int m_runtime_serial_number;// = 0; initialized by runtime
    /// <summary>
    /// The next allocation serial number
    /// </summary>
    static int m_current_serial_number = 1;
    /// <summary>
    /// I think this is the index to start the search from if we have an
    /// idea as to where to start looking.
    /// </summary>
    private int m_search_hint = -1;
    /// <summary>
    /// Contains a list of objects with a m_runtime_serial_number > 0,
    /// OnDeleteRhCmnRenderContent() will remove objects from the dictionary
    /// by m_runtime_serial_number.
    /// </summary>
    static readonly Dictionary<int, RenderContent> m_CustomContentDictionary = new Dictionary<int, RenderContent>();
    /// <summary>
    /// Rhino.Render.Fields FieldDictionary which provides access to setting
    /// and retrieving field values.
    /// </summary>
    private Fields.FieldDictionary m_FieldDictionary;
    /// <summary>
    /// Rhino.Render.Fields FieldDictionary which provides access to setting
    /// and retrieving field values.
    /// </summary>
    public Fields.FieldDictionary Fields
    {
      get { return m_FieldDictionary ?? (m_FieldDictionary = new Fields.FieldDictionary(this)); }
    }
    /// <summary>
    /// Check to see if the class is defined by RhinoCommon or some other
    /// assembly.
    /// </summary>
    /// <returns>
    /// If the class assembly type is equal to the RhinoCommon assembly then
    /// return true  indicating native content otherwise return false
    /// indicating custom content.
    /// </returns>
    protected bool ClassDefinedInRhinoCommon()
    {
      var renderContent = typeof (RenderContent);
      var classType = GetType();
      return renderContent.Assembly.Equals(classType.Assembly);
    }
    /// <summary>
    /// Check to see if the class type assembly is something other than
    /// RhinoCommon.
    /// </summary>
    /// <returns>
    /// Return true if the class definition resides in an assembly other than
    /// RhinoCommon otherwise return false because it is native content.
    /// </returns>
    protected bool IsCustomClassDefintion()
    {
      return !ClassDefinedInRhinoCommon();
    }
    /// <summary>
    /// Constructor
    /// </summary>
    internal RenderContent()
    {
      // This constructor is being called because we have a custom .NET subclass
      if (IsCustomClassDefintion())
      {
        m_runtime_serial_number = m_current_serial_number++;
        m_CustomContentDictionary.Add(m_runtime_serial_number, this);
      }
      // Find the plug-in that registered this class type
      var type = GetType();
      var typeId = type.GUID;
      Guid pluginId;
      RdkPlugIn.GetRenderContentType(typeId, out pluginId);
      if (Guid.Empty == pluginId) return;
      // Get information from custom class attributes
      var renderEngine = Guid.Empty;
      var imageBased = false;
      var attr = type.GetCustomAttributes(typeof(CustomRenderContentAttribute), false);
      if (attr.Length > 0)
      {
        var custom = attr[0] as CustomRenderContentAttribute;
        if (custom != null)
        {
          imageBased = custom.ImageBased;
          renderEngine = custom.RenderEngineId;
        }
      }
      // Crete C++ pointer of the appropriate type
      const int category = 0;
      if (this is RenderTexture)
        UnsafeNativeMethods.CRhCmnTexture_New(m_runtime_serial_number, imageBased, renderEngine, pluginId, typeId, category);
      else if (this is RenderMaterial)
        UnsafeNativeMethods.CRhCmnMaterial_New(m_runtime_serial_number, imageBased, renderEngine, pluginId, typeId, category);
      else if (this is RenderEnvironment)
        UnsafeNativeMethods.CRhCmnEnvironment_New(m_runtime_serial_number, imageBased, renderEngine, pluginId, typeId, category);
      else
        throw new InvalidCastException("Content is of unknown type");
    }
    /// <summary>
    /// Internal method used to get string values from the C++ SDK
    /// </summary>
    /// <param name="which">Id of string value to get</param>
    /// <returns>Returns the requested string value.</returns>
    internal string GetString(StringIds which)
    {
      IntPtr pConstThis = ConstPointer();
      using (Runtime.StringHolder sh = new Runtime.StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        UnsafeNativeMethods.Rdk_RenderContent_GetString(pConstThis, pString, (int)which);
        return sh.ToString();
      }
    }
    /// <summary>
    /// Override this method to provide a name for your content type.  ie. "My .net Texture"
    /// </summary>
    public abstract String TypeName { get; }
    /// <summary>
    /// Override this method to provide a description for your content type.  ie.  "Procedural checker pattern"
    /// </summary>
    public abstract String TypeDescription { get; }
    // <summary>
    // Returns either KindMaterial, KindTexture or KindEnvironment.
    // </summary>
    //public RenderContentKind Kind
    //{
    //  get { return KindFromString(GetString(StringIds.Kind)); }
    //}

    /// <summary>
    /// Instance name for this content.
    /// </summary>
    public String Name
    {
      get { return GetString(StringIds.Name); }
      set
      {
        UnsafeNativeMethods.Rdk_RenderContent_SetInstanceName(ConstPointer(), value);
      }
    }

    /// <summary>
    /// Notes for this content.
    /// </summary>
    public String Notes
    {
      get { return GetString(StringIds.Notes); }
      set
      {
        UnsafeNativeMethods.Rdk_RenderContent_SetNotes(ConstPointer(), value);
      }
    }

    /// <summary>
    /// Instance identifier for this content.
    /// </summary>
    public Guid Id
    {
      get
      {
        return UnsafeNativeMethods.Rdk_RenderContent_InstanceId(ConstPointer());
      }
      set
      {
        UnsafeNativeMethods.Rdk_RenderContent_SetInstanceId(ConstPointer(), value);
      }
    }

    /// <summary>
    /// Returns true if this content has no parent, false if it is the child of another content.
    /// </summary>
    public bool TopLevel
    {
      get { return 1 == UnsafeNativeMethods.Rdk_RenderContent_IsTopLevel(ConstPointer()); }
    }

    // Hiding for the time being. It may be better to just have a Document property
    /// <summary>
    /// Returns true if this content is a resident of one of the persistent lists.
    /// </summary>
    /*public*/ bool InDocument
    {
      get { return 1 == UnsafeNativeMethods.Rdk_RenderContent_IsInDocument(ConstPointer()); }
    }
    /*
    /// <summary>
    /// Helper function to check which content kind this content is.
    /// </summary>
    /// <param name="kind">Either KindMaterial, KindEnvironment or KindTexture.</param>
    /// <returns>true if the content is the specified kind, otherwise false.</returns>
    public bool IsKind(RenderContentKind kind)
    {
      return 1 == UnsafeNativeMethods.Rdk_RenderContent_IsKind(ConstPointer(), KindString(kind));
    }
    */
    /// <summary>
    /// Determines if the content has the hidden flag set.
    /// </summary>
    public bool Hidden
    {
      get { return 1 == UnsafeNativeMethods.Rdk_RenderContent_IsHidden(ConstPointer()); }
      set { UnsafeNativeMethods.Rdk_RenderContent_SetIsHidden(NonConstPointer(), value); }
    }

    // hiding for the time being. If this is only for environments, it may make more sense
    // to place the property there
    /// <summary>
    /// Determines if the content is considered the "Current" content - currently only used for Environments.
    /// </summary>
    /*public*/ bool Current
    {
      get { return 1 == UnsafeNativeMethods.Rdk_RenderContent_IsCurrent(ConstPointer()); }
    }

    /// <summary>
    /// Returns the top content in this parent/child chain.
    /// </summary>
    public RenderContent TopLevelParent
    {
      get
      {
        IntPtr pContent = UnsafeNativeMethods.Rdk_RenderContent_TopLevelParent(ConstPointer());
        return FromPointer(pContent);
      }
    }

    #region Serialization

    // See if we can fit this into the standard .NET serialization method (ISerializable)
    /*
    public bool ReadFromXml(String inputXml)
    {
      return 1 == UnsafeNativeMethods.Rdk_RenderContent_ReadFromXml(NonConstPointer(), inputXml);
    }
    public String GetXml()
    {
      return GetString(StringIds.Xml);
    }
    */

    #endregion

    /// <summary>
    /// Override this function to provide UI sections to display in the editor.
    /// </summary>
    protected virtual void OnAddUserInterfaceSections()
    {
      if (IsNativeWrapper())
      {
        UnsafeNativeMethods.Rdk_AddUISections(NonConstPointer());
      }
      else
      {
        UnsafeNativeMethods.Rdk_CallAddUISectionsBase(NonConstPointer());
      }
    }

    public bool AddUserInterfaceSection(string caption, int id)
    {
      return UnsafeNativeMethods.Rdk_CoreContent_AddAutomaticUISection(NonConstPointer(), caption, id);
    }


    // hiding until I understand what this does
    /*public virtual*/ bool IsContentTypeAcceptableAsChild(Guid type, String childSlotName)
    {
      if (IsNativeWrapper())
        return 1 == UnsafeNativeMethods.Rdk_RenderContent_IsContentTypeAcceptableAsChild(ConstPointer(), type, childSlotName);

      return 1 == UnsafeNativeMethods.Rdk_RenderContent_CallIsContentTypeAcceptableAsChildBase(ConstPointer(), type, childSlotName);
    }

    /*public*/ enum HarvestedResult : int // Return values for HarvestData()
    {
      None = 0,
      Some = 1,
      All = 2,
    };

    // hiding until I understand what this does
    /// <summary>
    /// Implement this to transfer data from another content to this content during creation.
    /// </summary>
    /// <param name="oldContent">An old content object from which the implementation may harvest data.</param>
    /// <returns>The harvested result.</returns>
    /*public virtual*/ HarvestedResult HarvestData(RenderContent oldContent)
    {
      if (IsNativeWrapper())
        return (HarvestedResult)UnsafeNativeMethods.Rdk_RenderContent_HarvestData(ConstPointer(), oldContent.ConstPointer());

      return (HarvestedResult)UnsafeNativeMethods.Rdk_RenderContent_CallHarvestDataBase(ConstPointer(), oldContent.ConstPointer());
    }
    
    #region Operations

    //TODO
    /** Delete a child content.
	\param parentContent is the content whose child is to be deleted. This must be an
	RDK-owned content that is in the persistent content list (either top-level or child).
	\param wszChildSlotName is the child-slot name of the child to be deleted.
	\return \e true if successful, else \e false. */
//RHRDK_SDK bool RhRdkDeleteChildContent(CRhRdkContent& parentContent, const wchar_t* wszChildSlotName);

    enum ChangeChildContentFlags : int
    {
      /// <summary>
      /// Allow (none) item to be displayed in dialog.
      /// </summary>
      AllowNone = 0x0001,
      /// <summary>
      /// Automatically open new content in thumbnail editor.
      /// </summary>
      AutoEdit = 0x0002,

      /// <summary>
      /// Mask to use to isolate harvesting flags.
      /// </summary>
      HarvestMask = 0xF000,
      /// <summary>
      /// Use Renderer Support option to decide about harvesting.
      /// </summary>
      HarvestUseOpt = 0x0000,
      /// <summary>
      /// Always copy similar parameters from old child.
      /// </summary>
      HarvestAlways = 0x1000,
      /// <summary>
      /// Never copy similar parameters from old child.
      /// </summary>
      HarvestNever = 0x2000,
    };
    //TODO
    /** Change a content's child by allowing the user to choose the new content type from a
      content browser dialog. The child is created if it does not exist, otherwise the old
      child is deleted and replaced by the new child.
      \param parentContent is the content whose child is to be manipulated. This must be an
      RDK-owned content that is in the persistent content list (either top-level or child).
      \param wszChildSlotName is the child-slot name of the child to be manipulated.
      \param allowedKinds determines which content kinds are allowed to be chosen from the content browser dialog.
      \param uFlags is a set of flags for controlling the content browser dialog.
      \return \e true if successful, \e false if it fails or if the user cancels. */

    //RHRDK_SDK bool RhRdkChangeChildContent(CRhRdkContent& parentContent, const wchar_t* wszChildSlotName,
    //                                      const CRhRdkContentKindList& allowedKinds,
    //                                     UINT uFlags = rdkccc_AllowNone | rdkccc_AutoEdit);
    #endregion

    internal enum ParameterTypes : int
    {
      Null = 0,
      Boolean = 1,
      Integer = 2,
      Float = 3,
      Double = 4,
      Color = 5,
      Vector2d = 6,
      Vector3d = 7,
      String = 8,
      Pointer = 9,
      Uuid = 10,
      Matrix = 11,
      Time = 12,
      Buffer = 13,
      Point4d = 14,
    }

    /// <summary>
    /// Context of a change to content parameters.
    /// </summary>
    public enum ChangeContexts
    {
      /// <summary>
      /// Change occurred as a result of user activity in the content's UI.
      /// </summary>
      UI = 0,
      /// <summary>
      /// Change occurred as a result of drag and drop.
      /// </summary>
      Drop = 1,
      /// <summary>
      /// Change occurred as a result of internal program activity.
      /// </summary>
      Program = 2,
      /// <summary>
      /// Change can be disregarded.
      /// </summary>
      Ignore = 3,
      /// <summary>
      /// Change occurred within the content tree (e.g., nodes reordered).
      /// </summary>
      Tree = 4,
      /// <summary>
      /// Change occurred as a result of an undo.
      /// </summary>
      Undo = 5,
      /// <summary>
      /// Change occurred as a result of a field initialization.
      /// </summary>
      FieldInit = 6,
      /// <summary>
      /// Change occurred during serialization (loading).
      /// </summary>
      Serialize = 7,
    }

    /// <summary>
    /// See C++ RDK documentation - this is a pass through function that gives access to your own
    /// native shader.  .NET clients will more likely simply check the type of their content and call their own
    /// shader access functions
    /// If you overide this function, you must ensure that you call "IsCompatible" and return IntPtr.Zero is that returns false.
    /// </summary>
    /// <param name="renderEngineId">The render engine requesting the shader.</param>
    /// <param name="privateData">A pointer to the render engine's own context object.</param>
    /// <returns>A pointer to the unmanaged shader.</returns>
    /*public virtual*/ IntPtr GetShader(Guid renderEngineId, IntPtr privateData)
    {
      if (IsNativeWrapper())
      {
        return UnsafeNativeMethods.Rdk_RenderContent_GetShader(ConstPointer(), renderEngineId, privateData);
      }
      return IntPtr.Zero;
    }

    /*public*/ bool IsCompatible(Guid renderEngineId)
    {
      return 1 == UnsafeNativeMethods.Rdk_RenderContent_IsCompatible(ConstPointer(), renderEngineId);
    }

    #region Child content support
    /// <summary>
    /// A "child slot" is the specific "slot" that a child (usually a texture) occupies.
    /// This is generally the "use" of the child - in other words, the thing the child
    /// operates on.  Some examples are "color", "transparency".
    /// </summary>
    /// <param name="paramName">The name of a parameter field. Since child textures will usually correspond with some
    ///parameter (they generally either replace or modify a parameter over UV space) these functions are used to
    ///specify which parameter corresponded with child slot.  If there is no correspondence, return the empty
    ///string.</param>
    /// <returns>
    /// The default behavior for these functions is to return the input string.
    /// Sub-classes may (in the future) override these functions to provide different mappings.
    /// </returns>
    public string ChildSlotNameFromParamName(String paramName)
    {
      using (Runtime.StringHolder sh = new Runtime.StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        IntPtr pConstThis = ConstPointer();
        UnsafeNativeMethods.Rdk_RenderContent_ChildSlotNameFromParamName(pConstThis, paramName, pString);
        return sh.ToString();
      }
    }

    /// <summary>
    /// A "child slot" is the specific "slot" that a child (usually a texture) occupies.
    /// This is generally the "use" of the child - in other words, the thing the child
    /// operates on.  Some examples are "color", "transparency".
    /// </summary>
    /// <param name="childSlotName">The named of the child slot to receive the parameter name for.</param>
    /// <returns>The default behaviour for these functions is to return the input string.  Sub-classes may (in the future) override these functions to provide different mappings.</returns>
    public string ParamNameFromChildSlotName(String childSlotName)
    {
      using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        IntPtr pConstThis = ConstPointer();
        UnsafeNativeMethods.Rdk_RenderContent_ParamNameFromChildSlotName(pConstThis, childSlotName, pString);
        return sh.ToString();
      }
    }

    public RenderContent FindChild(String childSlotName)
    {
      IntPtr pConstThis = ConstPointer();
      IntPtr pChild = UnsafeNativeMethods.Rdk_RenderContent_FindChild(pConstThis, childSlotName);
      return RenderContent.FromPointer(pChild);
    }

    public String ChildSlotName
    {
      get { return GetString(StringIds.ChildSlotName); }
      set
      {
        UnsafeNativeMethods.Rdk_RenderContent_SetChildSlotName(ConstPointer(), value);
      }
    }

    #endregion

    #region C++->C# Callbacks

    /// <summary>
    /// Function pointer to pass to the C++ wrapper, there is one of these for 
    /// each of RenderMaterail, RenderTexture and RenderEnvironment.
    /// </summary>
    /// <param name="typeId">Class type GUID custom attribute</param>
    /// <returns>Returns a new C++ pointer of the requested content type.</returns>
    internal delegate IntPtr NewRenderContentCallbackEvent(Guid typeId);
    /// <summary>
    /// Create content from type Id, called by the NewRenderContentCallbackEvent
    /// methods to create a .NET object pointer of a specific type from a class
    /// type Guid.
    /// </summary>
    /// <param name="typeId">The class GUID property to look up</param>
    /// <param name="isSubclassOf">The created content must be this type</param>
    /// <returns>Valid content object if the typeId is found otherwise null.</returns>
    static protected RenderContent NewRenderContent(Guid typeId, Type isSubclassOf)
    {
      Type renderContentType = typeof(RenderContent);
      // If the requested type is not derived from RenderContent
      if (!isSubclassOf.IsSubclassOf(renderContentType)) throw new InvalidCastException();
      // The class is at least derived from RenderContent so continue to try 
      // an create and instance of it.
      try
      {
        // Ask the RDK plug-in manager for the class Type to create and the
        // plug-in that registered the class type.
        Guid pluginId;
        var type = RdkPlugIn.GetRenderContentType(typeId, out pluginId);
        // If the requested typeId was found and is derived from the requested
        // type then create an instance of the class.
        if (null != type && !type.IsAbstract && type.IsSubclassOf(isSubclassOf))
          return Activator.CreateInstance(type) as RenderContent;
      }
      catch (Exception exception)
      {
        Runtime.HostUtils.ExceptionReport(exception);
      }
      return null;
    }

    internal delegate bool IsContentTypeAcceptableAsChildCallback(int serialNumber, Guid type, IntPtr childSlotName);
    internal static IsContentTypeAcceptableAsChildCallback m_IsContentTypeAcceptableAsChild = OnIsContentTypeAcceptableAsChild;
    static bool OnIsContentTypeAcceptableAsChild(int serialNumber, Guid type, IntPtr childSlotName)
    {
      try
      {
        RenderContent content = RenderContent.FromSerialNumber(serialNumber);
        if (content != null && childSlotName != IntPtr.Zero)
          return content.IsContentTypeAcceptableAsChild(type, System.Runtime.InteropServices.Marshal.PtrToStringUni(childSlotName));
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }
      return false;
    }

    internal delegate int HarvestDataCallback(int serialNumber, IntPtr oldContent);
    internal static HarvestDataCallback m_HarvestData = OnHarvestData;
    static int OnHarvestData(int serialNumber, IntPtr oldContent)
    {
      try
      {
        RenderContent content = RenderContent.FromSerialNumber(serialNumber);
        RenderContent old = RenderContent.FromPointer(oldContent);
        if (content != null && old != null)
          return (int)content.HarvestData(old);
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }
      return (int)HarvestedResult.None;
    }

    internal delegate void AddUISectionsCallback(int serialNumber);
    internal static AddUISectionsCallback m_AddUISections = _OnAddUISections;
    static void _OnAddUISections(int serialNumber)
    {
      try
      {
        RenderContent content = RenderContent.FromSerialNumber(serialNumber);
        if (content != null)
          content.OnAddUserInterfaceSections();
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }
    }

    internal delegate void RenderContentDeleteThisCallback(int serialNumber);
    internal static RenderContentDeleteThisCallback m_DeleteThis = OnDeleteRhCmnRenderContent;
    static void OnDeleteRhCmnRenderContent(int serialNumber)
    {
      try
      {
        RenderContent content = RenderContent.FromSerialNumber(serialNumber);
        if (content != null)
        {
          content.m_runtime_serial_number = -1;
          m_CustomContentDictionary.Remove(serialNumber);
        }
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }
    }


    internal delegate void GetRenderContentStringCallback(int serialNumber, bool isName, IntPtr pON_wString);
    internal static GetRenderContentStringCallback m_GetRenderContentString = OnGetRenderContentString;
    static void OnGetRenderContentString(int serialNnumber, bool isName, IntPtr pON_wString)
    {
      try
      {
        RenderContent content = RenderContent.FromSerialNumber(serialNnumber);
        if (content != null)
        {
          string str = isName ? content.TypeName : content.TypeDescription;
          if (!string.IsNullOrEmpty(str))
            UnsafeNativeMethods.ON_wString_Set(pON_wString, str);
        }
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }
    }

    internal delegate IntPtr GetShaderCallback(int serialNumber, Guid renderEngineId, IntPtr privateData);
    internal static GetShaderCallback m_GetShader = OnGetShader;
    static IntPtr OnGetShader(int serialNumber, Guid renderEngineId, IntPtr privateData)
    {
      try
      {
        RenderContent content = RenderContent.FromSerialNumber(serialNumber);
        if (content != null)
          return content.GetShader(renderEngineId, privateData);
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }
      return IntPtr.Zero;
    }

    #endregion

    #region events

    /*public*/ class ContentChangedEventArgs : RenderContentEventArgs
    {
      internal ContentChangedEventArgs(RenderContent content, RenderContent.ChangeContexts cc)
        : base(content)
      { m_cc = cc; }

      readonly RenderContent.ChangeContexts m_cc;
      public RenderContent.ChangeContexts ChangeContext { get { return m_cc; } }
    }

    /*public*/ class ContentTypeEventArgs : EventArgs
    {
      readonly Guid m_content_type;
      internal ContentTypeEventArgs(Guid type) { m_content_type = type; }
      public Guid Content { get { return m_content_type; } }
    }

    /*public*/ class ContentKindEventArgs : EventArgs
    {
      readonly RenderContentKind m_kind;
      internal ContentKindEventArgs(RenderContentKind kind) { m_kind = kind; }
      //public RenderContentKind Content { get { return m_kind; } }
    }

    /*public*/ class CurrentContentChangedEventArgs : RenderContentEventArgs
    {
      internal CurrentContentChangedEventArgs(RenderContent content, RenderContentKind kind)
        : base(content)
      { m_kind = kind; }

      readonly RenderContentKind m_kind;
      //public RenderContentKind Kind { get { return m_kind; } }
    }

    internal delegate void ContentAddedCallback(IntPtr pContent);
    internal delegate void ContentRenamedCallback(IntPtr pContent);
    internal delegate void ContentDeletingCallback(IntPtr pContent);
    internal delegate void ContentReplacingCallback(IntPtr pContent);
    internal delegate void ContentReplacedCallback(IntPtr pContent);
    internal delegate void ContentChangedCallback(IntPtr pContent, int changeContext);
    internal delegate void ContentUpdatePreviewCallback(IntPtr pContent);

    internal delegate void ContentTypeAddedCallback(Guid typeId);
    internal delegate void ContentTypeDeletingCallback(Guid typeId);
    internal delegate void ContentTypeDeletedCallback(int kind);

    internal delegate void CurrentContentChangedCallback(int kind, IntPtr pContent);

    private static CurrentContentChangedCallback m_OnCurrentContentChanged;
    private static void OnCurrentContentChanged(int kind, IntPtr pContent)
    {
      if (m_current_content_changed_event != null)
      {
        try { m_current_content_changed_event(null, new CurrentContentChangedEventArgs(Rhino.Render.RenderContent.FromPointer(pContent), (RenderContentKind)kind)); }
        catch (Exception ex) { Runtime.HostUtils.ExceptionReport(ex); }
      }
    }
    static EventHandler<CurrentContentChangedEventArgs> m_current_content_changed_event;


    private static ContentTypeAddedCallback m_OnContentTypeAdded;
    private static void OnContentTypeAdded(Guid type)
    {
      if (m_content_type_added_event != null)
      {
        try { m_content_type_added_event(null, new ContentTypeEventArgs(type));  }
        catch (Exception ex) { Runtime.HostUtils.ExceptionReport(ex); }
      }
    }
    static EventHandler<ContentTypeEventArgs> m_content_type_added_event;

    private static ContentTypeDeletingCallback m_OnContentTypeDeleting;
    private static void OnContentTypeDeleting(Guid type)
    {
      if (m_content_type_deleting_event != null)
      {
        try { m_content_type_deleting_event(null, new ContentTypeEventArgs(type)); }
        catch (Exception ex) { Runtime.HostUtils.ExceptionReport(ex); }
      }
    }
    static EventHandler<ContentTypeEventArgs> m_content_type_deleting_event;

    private static ContentTypeDeletedCallback m_OnContentTypeDeleted;
    private static void OnContentTypeDeleted(int kind)
    {
      if (m_content_type_deleted_event != null)
      {
        try { m_content_type_deleted_event(null, new ContentKindEventArgs((RenderContentKind)kind)); }
        catch (Exception ex) { Runtime.HostUtils.ExceptionReport(ex); }
      }
    }
    static EventHandler<ContentKindEventArgs> m_content_type_deleted_event;
    


    private static ContentAddedCallback m_OnContentAdded;
    private static void OnContentAdded(IntPtr pContent)
    {
      if (m_content_added_event != null)
      {
        try
        {
          m_content_added_event(null, new RenderContentEventArgs(Rhino.Render.RenderContent.FromPointer(pContent)));
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    internal static EventHandler<RenderContentEventArgs> m_content_added_event;

    private static ContentRenamedCallback m_OnContentRenamed;
    private static void OnContentRenamed(IntPtr pContent)
    {
      if (m_content_renamed_event != null)
      {
        try { m_content_renamed_event(null, new RenderContentEventArgs(Rhino.Render.RenderContent.FromPointer(pContent))); }
        catch (Exception ex) { Runtime.HostUtils.ExceptionReport(ex); }
      }
    }
    static EventHandler<RenderContentEventArgs> m_content_renamed_event;

    private static ContentDeletingCallback m_OnContentDeleting;
    private static void OnContentDeleting(IntPtr pContent)
    {
      if (m_content_deleting_event != null)
      {
        try { m_content_deleting_event(null, new RenderContentEventArgs(Rhino.Render.RenderContent.FromPointer(pContent))); }
        catch (Exception ex) { Runtime.HostUtils.ExceptionReport(ex); }
      }
    }
    static EventHandler<RenderContentEventArgs> m_content_deleting_event;

    private static ContentReplacingCallback m_OnContentReplacing;
    private static void OnContentReplacing(IntPtr pContent)
    {
      if (m_content_replacing_event != null)
      {
        try { m_content_replacing_event(null, new RenderContentEventArgs(Rhino.Render.RenderContent.FromPointer(pContent))); }
        catch (Exception ex) { Runtime.HostUtils.ExceptionReport(ex); }
      }
    }
    static EventHandler<RenderContentEventArgs> m_content_replacing_event;

    private static ContentReplacedCallback m_OnContentReplaced;
    private static void OnContentReplaced(IntPtr pContent)
    {
      if (m_content_replaced_event != null)
      {
        try { m_content_replaced_event(null, new RenderContentEventArgs(Rhino.Render.RenderContent.FromPointer(pContent))); }
        catch (Exception ex) { Runtime.HostUtils.ExceptionReport(ex); }
      }
    }
    static EventHandler<RenderContentEventArgs> m_content_replaced_event;

    private static ContentChangedCallback m_OnContentChanged;
    private static void OnContentChanged(IntPtr pContent, int cc)
    {
      if (m_content_changed_event != null)
      {
        try { m_content_changed_event(null, new ContentChangedEventArgs(Rhino.Render.RenderContent.FromPointer(pContent), (RenderContent.ChangeContexts)cc)); }
        catch (Exception ex) { Runtime.HostUtils.ExceptionReport(ex); }
      }
    }
    static EventHandler<ContentChangedEventArgs> m_content_changed_event;

    private static ContentUpdatePreviewCallback m_OnContentUpdatePreview;
    private static void OnContentUpdatePreview(IntPtr pContent)
    {
      if (m_content_update_preview_event != null)
      {
        try { m_content_update_preview_event(null, new RenderContentEventArgs(Rhino.Render.RenderContent.FromPointer(pContent))); }
        catch (Exception ex) { Runtime.HostUtils.ExceptionReport(ex); }
      }
    }
    static EventHandler<RenderContentEventArgs> m_content_update_preview_event;
    
    /// <summary>
    /// Used to monitor render content addition to the document.
    /// </summary>
    public static event EventHandler<RenderContentEventArgs> RenderContentAdded
    {
      add
      {
        if (m_content_added_event == null)
        {
          m_OnContentAdded = OnContentAdded;
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentAddedEventCallback(m_OnContentAdded, Rhino.Runtime.HostUtils.m_rdk_ew_report);
        }
        m_content_added_event += value;
      }
      remove
      {
        m_content_added_event -= value;
        if (m_content_added_event == null)
        {
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentAddedEventCallback(null, Rhino.Runtime.HostUtils.m_rdk_ew_report);
          m_OnContentAdded = null;
        }
      }
    }

    /// <summary>
    /// Used to monitor render content renaming in the document.
    /// </summary>
    /*public*/ static event EventHandler<RenderContentEventArgs> RenderContentRenamed
    {
      add
      {
        if (m_content_renamed_event == null)
        {
          m_OnContentRenamed = OnContentRenamed;
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentRenamedEventCallback(m_OnContentRenamed, Rhino.Runtime.HostUtils.m_rdk_ew_report);
        }
        m_content_renamed_event += value;
      }
      remove
      {
        m_content_renamed_event -= value;
        if (m_content_renamed_event == null)
        {
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentRenamedEventCallback(null, Rhino.Runtime.HostUtils.m_rdk_ew_report);
          m_OnContentRenamed = null;
        }
      }
    }

    /// <summary>
    /// Used to monitor render content deletion from the document.
    /// </summary>
    /*public*/ static event EventHandler<RenderContentEventArgs> RenderContentDeleting
    {
      add
      {
        if (m_content_deleting_event == null)
        {
          m_OnContentDeleting = OnContentDeleting;
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentDeletingEventCallback(m_OnContentDeleting, Rhino.Runtime.HostUtils.m_rdk_ew_report);
        }
        m_content_deleting_event += value;
      }
      remove
      {
        m_content_deleting_event -= value;
        if (m_content_deleting_event == null)
        {
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentDeletingEventCallback(null, Rhino.Runtime.HostUtils.m_rdk_ew_report);
          m_OnContentDeleting = null;
        }
      }
    }

    /// <summary>
    /// Used to monitor render content replacing in the document.
    /// </summary>
    /*public*/ static event EventHandler<RenderContentEventArgs> RenderContentReplacing
    {
      add
      {
        if (m_content_replacing_event == null)
        {
          m_OnContentReplacing = OnContentReplacing;
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentReplacingEventCallback(m_OnContentReplacing, Rhino.Runtime.HostUtils.m_rdk_ew_report);
        }
        m_content_replacing_event += value;
      }
      remove
      {
        m_content_replacing_event -= value;
        if (m_content_replacing_event == null)
        {
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentReplacingEventCallback(null, Rhino.Runtime.HostUtils.m_rdk_ew_report);
          m_OnContentReplacing = null;
        }
      }
    }

    /// <summary>
    /// Used to monitor render content replacing in the document.
    /// </summary>
    /*public*/ static event EventHandler<RenderContentEventArgs> RenderContentReplaced
    {
      add
      {
        if (m_content_replaced_event == null)
        {
          m_OnContentReplaced = OnContentReplaced;
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentReplacedEventCallback(m_OnContentReplaced, Rhino.Runtime.HostUtils.m_rdk_ew_report);
        }
        m_content_replaced_event += value;
      }
      remove
      {
        m_content_replaced_event -= value;
        if (m_content_replaced_event == null)
        {
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentReplacedEventCallback(null, Rhino.Runtime.HostUtils.m_rdk_ew_report);
          m_OnContentReplaced = null;
        }
      }
    }

    /// <summary>
    /// Used to monitor render content modifications.
    /// </summary>
    /*public*/ static event EventHandler<ContentChangedEventArgs> RenderContentChanged
    {
      add
      {
        if (m_content_changed_event == null)
        {
          m_OnContentChanged = OnContentChanged;
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentChangedEventCallback(m_OnContentChanged, Rhino.Runtime.HostUtils.m_rdk_ew_report);
        }
        m_content_changed_event += value;
      }
      remove
      {
        m_content_changed_event -= value;
        if (m_content_changed_event == null)
        {
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentChangedEventCallback(null, Rhino.Runtime.HostUtils.m_rdk_ew_report);
          m_OnContentChanged = null;
        }
      }
    }

    /// <summary>
    /// Used to monitor render content preview updates.
    /// </summary>
    /*public*/ static event EventHandler<RenderContentEventArgs> RenderContentUpdatePreview
    {
      add
      {
        if (m_content_update_preview_event == null)
        {
          m_OnContentUpdatePreview = OnContentUpdatePreview;
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentUpdatePreviewEventCallback(m_OnContentUpdatePreview, Rhino.Runtime.HostUtils.m_rdk_ew_report);
        }
        m_content_update_preview_event += value;
      }
      remove
      {
        m_content_update_preview_event -= value;
        if (m_content_update_preview_event == null)
        {
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentUpdatePreviewEventCallback(null, Rhino.Runtime.HostUtils.m_rdk_ew_report);
          m_OnContentUpdatePreview = null;
        }
      }
    }

    /// <summary>
    /// Used to monitor render content preview updates.
    /// </summary>
    /*public*/ static event EventHandler<CurrentContentChangedEventArgs> CurrentRenderContentChanged
    {
      add
      {
        if (m_current_content_changed_event == null)
        {
          m_OnCurrentContentChanged = OnCurrentContentChanged;
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentCurrencyChangedEventCallback(m_OnCurrentContentChanged, Rhino.Runtime.HostUtils.m_rdk_ew_report);
        }
        m_current_content_changed_event += value;
      }
      remove
      {
        m_current_content_changed_event -= value;
        if (m_current_content_changed_event == null)
        {
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentCurrencyChangedEventCallback(null, Rhino.Runtime.HostUtils.m_rdk_ew_report);
          m_OnCurrentContentChanged = null;
        }
      }
    }



    /// <summary>
    /// Used to monitor render content types being registered.
    /// </summary>
    /*public*/ static event EventHandler<ContentTypeEventArgs> RenderContentTypeAdded
    {
      add
      {
        if (m_content_type_added_event == null)
        {
          m_OnContentTypeAdded = OnContentTypeAdded;
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetFactoryAddedEventCallback(m_OnContentTypeAdded, Rhino.Runtime.HostUtils.m_rdk_ew_report);
        }
        m_content_type_added_event += value;
      }
      remove
      {
        m_content_type_added_event -= value;
        if (m_content_type_added_event == null)
        {
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetFactoryAddedEventCallback(null, Rhino.Runtime.HostUtils.m_rdk_ew_report);
          m_OnContentTypeAdded = null;
        }
      }
    }


    /// <summary>
    /// Used to monitor render content types being registered.
    /// </summary>
    /*public*/ static event EventHandler<ContentTypeEventArgs> RenderContentTypeDeleting
    {
      add
      {
        if (m_content_type_deleting_event == null)
        {
          m_OnContentTypeDeleting = OnContentTypeDeleting;
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetFactoryDeletingEventCallback(m_OnContentTypeDeleting, Rhino.Runtime.HostUtils.m_rdk_ew_report);
        }
        m_content_type_deleting_event += value;
      }
      remove
      {
        m_content_type_deleting_event -= value;
        if (m_content_type_deleting_event == null)
        {
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetFactoryDeletingEventCallback(null, Rhino.Runtime.HostUtils.m_rdk_ew_report);
          m_OnContentTypeDeleting = null;
        }
      }
    }


    /// <summary>
    /// Used to monitor render content types being registered.
    /// </summary>
    /*public*/ static event EventHandler<ContentKindEventArgs> RenderContentTypeDeleted
    {
      add
      {
        if (m_content_type_deleted_event == null)
        {
          m_OnContentTypeDeleted = OnContentTypeDeleted;
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetFactoryDeletedEventCallback(m_OnContentTypeDeleted, Rhino.Runtime.HostUtils.m_rdk_ew_report);
        }
        m_content_type_deleted_event += value;
      }
      remove
      {
        m_content_type_deleted_event -= value;
        if (m_content_type_deleted_event == null)
        {
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetFactoryDeletedEventCallback(null, Rhino.Runtime.HostUtils.m_rdk_ew_report);
          m_OnContentTypeDeleted = null;
        }
      }
    }


    #endregion

    #region pointer tracking

    private bool m_bAutoDelete;
    internal bool AutoDelete
    {
      get { return m_bAutoDelete; }
      set { m_bAutoDelete = value; }
    }

    internal static RenderContent FromSerialNumber(int serial_number)
    {
      RenderContent rc;
      m_CustomContentDictionary.TryGetValue(serial_number, out rc);
      return rc;
    }

    internal virtual IntPtr ConstPointer()
    {
      IntPtr pContent = UnsafeNativeMethods.Rdk_FindRhCmnContentPointer(m_runtime_serial_number, ref m_search_hint);
      return pContent;
    }
    internal virtual IntPtr NonConstPointer()
    {
      IntPtr pContent = UnsafeNativeMethods.Rdk_FindRhCmnContentPointer(m_runtime_serial_number, ref m_search_hint);
      return pContent;
    }

    internal bool IsNativeWrapper()
    {
      return ClassDefinedInRhinoCommon();
    }
    #endregion

    #region disposable implementation
    ~RenderContent()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (null != m_FieldDictionary)
      {
        m_FieldDictionary.InternalDispose();
        m_FieldDictionary = null;
      }
      if (m_bAutoDelete)
      {
        UnsafeNativeMethods.Rdk_RenderContent_DeleteThis(NonConstPointer());
      }
      // for now we, don't need to perform any disposal
      //if (IntPtr.Zero != m_pRenderContent)
      //{
      //  UnsafeNativeMethods.Rdk_RenderContent_DeleteThis(m_pRenderContent);
      //  m_pRenderContent = IntPtr.Zero;
      //}
    }
    #endregion
  }

  public class RenderContentEventArgs : EventArgs
  {
    readonly RenderContent m_content;
    internal RenderContentEventArgs(RenderContent content) { m_content = content; }
    public RenderContent Content { get { return m_content; } }
  }
}

#endif