#pragma warning disable 1591
using System;

#if RDK_UNCHECKED
namespace Rhino.Render
{
  public class Utilities
  {
    /// <summary>
    /// RDK may fail to load if something has gone wrong during installation. You can check this properties if you want to be certain that RDK is actually available before using it.
    /// </summary>
    public static bool RdkIsAvailable
    {
      get
      {
        return 1==UnsafeNativeMethods.Rdk_Globals_RdkIsAvailable();
      }
    }


    /// <summary>
    /// Returns the RDK_SDK_VERSION this RDK was built with.
    /// </summary>
    public static String RdkVersion
    {
      get
      {
        using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.Rdk_Globals_RdkVersion(pString);
          return sh.ToString();
        }
        
      }
    }

    /// <summary>
    /// Returns the build date of this RDK - implemented as return __DATE__;
    /// </summary>
    public static String RdkBuildDate
    {
      get
      {
        using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.Rdk_Globals_RdkBuildDate(pString);
          return sh.ToString();
        }
        
      }
    }

    /// <summary>
    /// Returns the RDK_MAJOR_VERSION this RDK was built with.
    /// </summary>
    /// <returns>The Rdk major version.</returns>
    public static int RdkMajorVersion()    {   return UnsafeNativeMethods.Rdk_Globals_RdkMajorVersion();    }
    
    /// <summary>
    /// Returns the RDK_MINOR_VERSION this RDK was built with.
    /// </summary>
    /// <returns>The Rdk minor version.</returns>
    public static int RdkMinorVersion()   {    return UnsafeNativeMethods.Rdk_Globals_RdkMinorVersion();    }

    /// <summary>
    /// Returns the RDK_BETA_RELEASE this RDK was built with.
    /// </summary>
    /// <returns>The Rdk beta release.</returns>
    public static int RdkBetaRelease()   {    return UnsafeNativeMethods.Rdk_Globals_RdkBetaRelease();    }

    /// <summary>
    /// Displays the standard modal color picker dialog.
    /// </summary>
    /// <param name="colorInOut">The initial color to set the picker to and also accepts the user's choice.</param>
    /// <param name="bUseAlpha">Specifies if the color picker should allow changes to the alpha channel or not.</param>
    /// <returns>true if a color was picked, false if the user canceled the picker dialog.</returns>
    public static bool ShowColorPicker(ref Rhino.Display.Color4f colorInOut, bool bUseAlpha)
    {
      Rhino.Display.Color4f c = new Rhino.Display.Color4f();

      bool b = 1==UnsafeNativeMethods.Rdk_Globals_ShowColorPicker(colorInOut, bUseAlpha, ref c);

      if (b) { colorInOut = c; }
      
      return b;
    }

    internal static bool ShowIncompatibleContent(RenderContentKind kind) { return 1 == UnsafeNativeMethods.Rdk_Globals_ShowIncompatibleContent(RenderContent.KindString(kind)); }
    internal static void SetShowIncompatibleContent(RenderContentKind kind, bool bShow) { UnsafeNativeMethods.Rdk_Globals_SetShowIncompatbileContent(RenderContent.KindString(kind), bShow); }

    /// <summary>
    /// Specifies whether incompatible content should be shown in the corresponding editor.
    /// </summary>
    public static bool ShowIncompatibleMaterials
    {
      get { return ShowIncompatibleContent(RenderContentKind.Material); }
      set { SetShowIncompatibleContent(RenderContentKind.Material, value); }
    }

    /// <summary>
    /// Specifies whether incompatible content should be shown in the corresponding editor.
    /// </summary>
    public static bool ShowIncompatibleEnvironments
    {
      get { return ShowIncompatibleContent(RenderContentKind.Environment); }
      set { SetShowIncompatibleContent(RenderContentKind.Environment, value); }
    }

    /// <summary>
    /// Specifies whether incompatible content should be shown in the corresponding editor.
    /// </summary>
    public static bool ShowIncompatibleTextures
    {
      get { return ShowIncompatibleContent(RenderContentKind.Texture); }
      set { SetShowIncompatibleContent(RenderContentKind.Texture, value); }
    }

    public enum ChooseContentFlags : int
    {
      /// <summary>
      /// Dialog will have [New...] button.
      /// </summary>
	    NewButton  = 1,
      /// <summary>
      /// Dialog will have [Edit...] button.
      /// </summary>
	    EditButton = 2,
    };

    /// <summary>
    /// Allows the user to choose a content by displaying the Content Chooser dialog.
	  /// The dialog will have OK, Cancel and Help buttons, and optional New and Edit buttons.
    /// </summary>
    /// <param name="instanceId">Sets the initially selected content and receives the instance id of the chosen content.</param>
    /// <param name="kinds">Specifies the kind(s) of content that should be displayed in the chooser.</param>
    /// <param name="flags">Specifies flags controlling the browser.</param>
    /// <param name="doc">A Rhino document.</param>
    /// <returns>true if the operation succeeded.</returns>
    public static bool ChooseContent(ref Guid instanceId, RenderContentKind kinds, ChooseContentFlags flags, RhinoDoc doc)
    {
      return 1 == UnsafeNativeMethods.Rdk_Globals_ChooseContentEx(ref instanceId, RenderContent.KindString(kinds), (int)flags, doc.m_docId);
    }

    internal static bool IsKindEditorVisible(RenderContentKind kind)
    {
      return 1==UnsafeNativeMethods.Rdk_Globals_IsContentEditorVisible(RenderContent.KindString(kind));
    }

    /// <summary>
    /// Queries whether or not the material editor is visible.
    /// </summary>
    public static bool IsMaterialEditorVisible { get { return IsKindEditorVisible(RenderContentKind.Material); } }

    /// <summary>
    /// Queries whether or not the environment editor is visible.
    /// </summary>
    public static bool IsEnvironmentEditorVisible { get { return IsKindEditorVisible(RenderContentKind.Environment); } }

    /// <summary>
    /// Queries whether or not the texture palette is visible.
    /// </summary>
    public static bool IsTexturePaletteVisible { get { return IsKindEditorVisible(RenderContentKind.Texture); } }

    /// <summary>
    /// Queries whether or not the sun dock bar is visible.
    /// </summary>
    public static bool IsSunDockBarVisible   { get { return 1==UnsafeNativeMethods.Rdk_Globals_IsSunDockBarVisible(); } }

    /// <summary>
    /// Queries whether or not the view dock bar is visible.
    /// </summary>
    public static bool IsViewDockBarVisible   { get { return 1==UnsafeNativeMethods.Rdk_Globals_IsViewDockBarVisible(); } }

    /// <summary>
    /// Queries whether or not the Safe Frame is visible.
    /// </summary>
    public static bool IsSafeFrameEnabled    { get { return 1==UnsafeNativeMethods.Rdk_Globals_IsSafeFrameVisible(); } }

    /// <summary>
    /// Queries whether or not the Ground Plane is visible.
    /// </summary>
    public static bool IsGroundPlaneVisible  { get { return 1==UnsafeNativeMethods.Rdk_Globals_IsGroundPlaneVisible(); } }
        
    /// <summary>
    /// Constructs a new basic material from a <see cref="Rhino.DocObjects.Material">Material</see>.
    /// </summary>
    /// <param name="material">The material to create the basic material from.</param>
    /// <returns>A new basic material.</returns>
    public static RenderMaterial NewBasicMaterial(Rhino.DocObjects.Material material)
    {
      NativeRenderMaterial newMaterial = RenderContent.FromPointer(UnsafeNativeMethods.Rdk_Globals_NewBasicMaterial(material == null ? IntPtr.Zero : material.ConstPointer())) as NativeRenderMaterial;
      newMaterial.AutoDelete = true;
      return newMaterial;
    }

    /// <summary>
    /// Constructs a new <see cref="RenderEnvironment"/> from a <see cref="SimulatedEnvironment"/>.
    /// </summary>
    /// <param name="environment">The environment to create the basic environment from.</param>
    /// <returns>A new basic environment.</returns>
    public static RenderEnvironment NewBasicEnvironment(SimulatedEnvironment environment)
    {
      NativeRenderEnvironment newEnvironment = RenderContent.FromPointer(UnsafeNativeMethods.Rdk_Globals_NewBasicEnvironment(environment == null ? IntPtr.Zero : environment.ConstPointer())) as NativeRenderEnvironment;
      newEnvironment.AutoDelete = true;
      return newEnvironment;
    }

    /// <summary>
    /// Constructs a new basic texture from a SimulatedTexture.
    /// </summary>
    /// <param name="texture">The texture to create the basic texture from.</param>
    /// <returns>A new render texture.</returns>
    public static RenderTexture NewBitmapTexture(SimulatedTexture texture)
    {
      NativeRenderTexture newTexture = RenderContent.FromPointer(UnsafeNativeMethods.Rdk_Globals_NewBasicTexture(texture == null ? IntPtr.Zero : texture.ConstPointer())) as NativeRenderTexture;
      newTexture.AutoDelete = true;
      return newTexture;
    }

    public enum ShowContentChooserFlags : int
    {
      None            = 0x0000,       
	    HideNewTab      = 0x0001,
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
    public static RenderContent CreateContentByType(Guid type, ShowContentChooserFlags flags, Rhino.RhinoDoc doc)
    {
      IntPtr pContent = UnsafeNativeMethods.Rdk_Globals_CreateContentByType(type, IntPtr.Zero, String.Empty, (int)flags, doc.m_docId);
      return pContent == IntPtr.Zero ? null : RenderContent.FromPointer(pContent);
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
    public static RenderContent CreateContentByType(Guid type, RenderContent parent, String childSlotName, ShowContentChooserFlags flags, Rhino.RhinoDoc doc)
    {
      IntPtr pContent = UnsafeNativeMethods.Rdk_Globals_CreateContentByType(type, parent.ConstPointer(), childSlotName, (int)flags, doc.m_docId);
      return pContent == IntPtr.Zero ? null : RenderContent.FromPointer(pContent);
    }

    /// <summary>
    /// Constructs a new content chosen by the user and add it to the persistent content list.
	  /// This function cannot be used to create temporary content that you delete after use.
	  /// Content created by this function is owned by RDK and appears in the content editor.
    /// </summary>
    /// <param name="defaultType">The default content type.</param>
    /// <param name="defaultInstance">The default selected content instance.</param>
    /// <param name="kinds">Determines which content kinds are allowed to be chosen from the dialog.</param>
    /// <param name="flags">Options for the tab.</param>
    /// <param name="doc">The current Rhino document.</param>
    /// <returns>A new persistent render content.</returns>
    public static RenderContent CreateContentByUser(Guid defaultType, Guid defaultInstance, RenderContentKind kinds, ChooseContentFlags flags, RhinoDoc doc)
    {
      IntPtr pContent = UnsafeNativeMethods.Rdk_Globals_CreateContentByUser(defaultType, defaultInstance, RenderContent.KindString(kinds), (int)flags, doc.m_docId);
      return pContent == IntPtr.Zero ? null : RenderContent.FromPointer(pContent);
    }
    
    /// <summary>
    /// Changes the type of a content. This deletes the content and creates a replacement
	  /// of the specified type allowing the caller to decide about harvesting.
    /// </summary>
    /// <param name="oldContent">oldContent is the old content which is deleted.</param>
    /// <param name="newType">The type of content to replace pOldContent with.</param>
    /// <param name="harvestParameters">Determines whether or not parameter harvesting will be performed.</param>
    /// <returns>A new persistent render content.</returns>
    public static RenderContent ChangeContentType(RenderContent oldContent, Guid newType, bool harvestParameters)
    {
      IntPtr pContent = UnsafeNativeMethods.Rdk_Globals_ChangeContentType(oldContent.NonConstPointer(), newType, harvestParameters);
      return IntPtr.Zero==pContent ? null : RenderContent.FromPointer(pContent);
    }

    /// <summary>
    /// Accesses the material table.
    /// </summary>
    /// <param name="doc">A Rhino document.</param>
    /// <returns>The materials list.</returns>
    public static ContentList MaterialList(RhinoDoc doc)
    {
      return new ContentList(RenderContentKind.Material, doc);
    }

    /// <summary>
    /// Accesses the environment table.
    /// </summary>
    /// <param name="doc">A Rhino document.</param>
    /// <returns>The environments list.</returns>
    public static ContentList EnvironmentList(RhinoDoc doc)
    {
      return new ContentList(RenderContentKind.Environment, doc);
    }

    /// <summary>
    /// Accesses the texture table.
    /// </summary>
    /// <param name="doc">A Rhino document.</param>
    /// <returns>The textures list.</returns>
    public static ContentList TextureList(RhinoDoc doc)
    {
      return new ContentList(RenderContentKind.Texture, doc);
    }

    /// <summary>
    /// Accesses any content table given a (single) kind.
    /// </summary>
    /// <param name="kind">A single kind.</param>
    /// <param name="doc">A Rhino document.</param>
    /// <returns>The (render content kind) list.</returns>
    public static ContentList ContentList(RenderContentKind kind, RhinoDoc doc)
    {
      return new ContentList(kind, doc);
    }

    /// <summary>
    /// Prompts the user for a save file name and the width, height and depth of an image to be saved.
    /// </summary>
    /// <param name="filename">The original file path.</param>
    /// <param name="width">A width.</param>
    /// <param name="height">An height.</param>
    /// <param name="colorDepth">A color depth.</param>
    /// <returns>The new file name.</returns>
    public static string PromptForSaveImageFileParameters(string filename, ref int width, ref int height, ref int colorDepth)
    {
      using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        bool bRet = 1==UnsafeNativeMethods.Rdk_Globals_PromptForSaveImageFileParams(filename, ref width, ref height, ref colorDepth, pString);
        
        if (bRet)
          return sh.ToString();
      }
      return null;      
    }

    /// <summary>
    /// Loads a content from a library file.  Adds the content to the persistent content list.
    /// </summary>
    /// <param name="filename">is the full path to the file to be loaded.</param>
    /// <returns>The loaded content or null if an error occurred.</returns>
    public static RenderContent LoadPersistantRenderContentFromFile(String filename)
    {
      IntPtr pContent = UnsafeNativeMethods.Rdk_RenderContent_LoadPersistantContentFromFile(filename);
      return IntPtr.Zero == pContent ? RenderContent.FromPointer(pContent) : null;
    }

    /// <summary>
    /// Loads a content from a library file.  Does not add the content to the persistent content list.  Use AddPersistantContent to add it to the list.
    /// </summary>
    /// <param name="filename">is the full path to the file to be loaded.</param>
    /// <returns>The loaded content or null if an error occurred.</returns>
    public static RenderContent LoadRenderContentFromFile(String filename)
    {
      IntPtr pContent = UnsafeNativeMethods.Rdk_RenderContent_LoadContentFromFile(filename);
      if (pContent == IntPtr.Zero)
        return null;

      RenderContent newContent = RenderContent.FromPointer(pContent);
      newContent.AutoDelete = true;
      return newContent;
    }

    /// <summary>
    /// Use this function to add a material, environment or texture to the internal
    /// RDK document lists as a top level content.  The content must have been returned from
    /// RenderContent::MakeCopy, NewContentFromType or a similar function that returns a non-document
    /// content.
    /// </summary>
    /// <param name="renderContent">The render content.</param>
    /// <returns>true on success.</returns>
    public static bool AddPersistentRenderContent(RenderContent renderContent)
    {
      renderContent.AutoDelete = false;
      return 1 == UnsafeNativeMethods.Rdk_Globals_AddPersistentContent(renderContent.ConstPointer());
    }

    public enum ShowContentChooserResults
    {
      /// <summary>
      /// No choice (user cancelled).
      /// </summary>
	    None,
      /// <summary>
      /// User chose from 'New' tab. uuidOut is the type id.
      /// </summary>
	    New,
      /// <summary>
      /// User chose from 'Existing' tab with 'copy' radio button checked. uuidOut is the instance id.
      /// </summary>
	    Copy,
      /// <summary>
      /// User chose from 'Existing' tab with 'instance' radio button checked. uuidOut is the instance id.
      /// </summary>
	    Instance, 
    };

    /// <summary>
    /// Shows the content chooser to allow the user to select a new or existing content.
    /// </summary>
    /// <param name="defaultType">The content type that will be initially selected in the 'New' tab.</param>
    /// <param name="defaultInstanceId">The content instance that will be initially selected in the 'Existing' tab.</param>
    /// <param name="kinds">Which content kinds will be displayed.</param>
    /// <param name="instanceIdOut">The UUID of the chosen item. Depending on eRhRdkSccResult, this can be the type id of a content type or the instance id of an existing content.</param>
    /// <param name="flags">Tabs specifications.</param>
    /// <param name="doc">A Rhino document.</param>
    /// <returns>The result.</returns>
    public static ShowContentChooserResults ShowContentChooser(Guid defaultType, Guid defaultInstanceId, RenderContentKind kinds, ref Guid instanceIdOut, ShowContentChooserFlags flags, RhinoDoc doc)
    {
      return (ShowContentChooserResults)UnsafeNativeMethods.Rdk_Globals_ShowContentChooser(defaultType, defaultInstanceId, RenderContent.KindString(kinds), ref instanceIdOut, (int)flags, doc.m_docId);
    }

    /// <summary>
    /// Finds a file and also handles network shares.
    /// <remarks>This is a replacement for CRhinoFileUtilities::FindFile().</remarks>
    /// </summary>
    /// <param name="fullPathToFile">The file to be found.</param>
    /// <returns>The found file.</returns>
    public static string FindFile(string fullPathToFile)
    {
      using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        bool found = (1 == UnsafeNativeMethods.Rdk_Globals_FindFile(fullPathToFile, pString));

        return found ? sh.ToString() : null;
      }
    }

    /// <summary>
    /// Determines if any texture in any persistent content list is using the specified file name for caching.
    /// </summary>
    /// <param name="textureFileName">The file name to check for. The extension is ignored.</param>
    /// <returns>true if the texture is present.</returns>
    public static bool IsCachedTextureFileInUse(string textureFileName)
    {
      return 1 == UnsafeNativeMethods.Rdk_Globals_IsCachedTextureFileInUse(textureFileName);
    }

    //TODO
    /* \return A reference to RDK's collection of registered content I/O plug-ins. */
    //RHRDK_SDK const IRhRdkContentIOPlugIns& RhRdkContentIOPlugIns(void);

    //TODO
//RHRDK_SDK IRhRdkRegisteredPropertyManager& RhRdkRegisteredPropertiesManager(void);

    //TODO
//RHRDK_SDK CRhRdkRenderPlugIn* FindCurrentRenderPlugIn(void);

    //TODO
//RHRDK_SDK void RhRdkCopySun(IRhRdkSun& dest, const IRhRdkSun& srce);

    //TODO
//RHRDK_SDK ON_BoundingBox RhRdkGetCRMBoundingBox(const IRhRdkCustomRenderMeshes& meshes);

    /* \return A reference to RDK's custom render mesh manager. */
    //RHRDK_SDK IRhRdkCustomRenderMeshManager& RhRdkCustomRenderMeshManager(void);

    /* Create a new texture from a HBITMAP (which should be a DIBSECTION).
	\param hBitmap is the bitmap to create the texture from.
	\param bAllowSimulation determines whether simulation of the texture into a temporary bitmap is allowed.
	\param bShared determines whether ownership is passed to RDK. If bShared is \e false, you must call
	       ::DeleteObject() on hBitmap at some convenient time. If bShared is \e true, RDK will delete it
	       when the texture is deleted. You can use this parameter to share bitmaps between textures.
	\return A pointer to the texture. Never NULL. */
    //RHRDK_SDK CRhRdkTexture* RhRdkNewDibTexture(HBITMAP hBitmap, bool bShared=false, bool bAllowSimulation=true);
    //RHRDK_SDK CRhRdkTexture* RhRdkNewDibTexture(CRhinoUiDib* pDib, bool bShared=false, bool bAllowSimulation=true);

    /* Get an interface to an automatic UI. The caller shall delete the interface
      when it is no longer required.
      \param pParent is the parent window which <b>must not be NULL</b>.
      \param style specifies the visual style of the UI.
      \return Interface to automatic UI. This will be NULL only if RDK or plug-in is
      not correctly initialized. */
    //RHRDK_SDK IRhRdkAutomaticUI* RhRdkNewAutomaticUI(CWnd* pParent, IRhRdkAutomaticUI::eStyle style);


  }
}




#endif