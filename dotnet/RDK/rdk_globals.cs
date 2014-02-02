#pragma warning disable 1591
using System;
using Rhino.Runtime.InteropWrappers;

#if RDK_CHECKED
namespace Rhino.Render
{
  public static class Utilities
  {
    /// <summary>
    /// Set default render application
    /// </summary>
    /// <param name="pluginId">ID of render plug-in</param>
    /// <returns>
    /// True if plug-in found and loaded successfully.  False if pluginId is
    ///  invalid or was unable to load plug-in
    /// </returns>
    public static bool SetDefaultRenderPlugIn(Guid pluginId)
    {
      return UnsafeNativeMethods.CRhinoApp_SetDefaultRenderApp(pluginId);
    }

    /// <summary>
    /// Get the plug-in Id for the default render plug-in
    /// </summary>
    public static Guid DefaultRenderPlugInId
    {
      get
      {
        return UnsafeNativeMethods.CRhinoApp_GetDefaultRenderApp();
      }
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

    /// <summary>
    /// Queries whether or not the Safe Frame is visible.
    /// </summary>
    public static bool SafeFrameEnabled { get { return 1 == UnsafeNativeMethods.Rdk_Globals_IsSafeFrameVisible(); } }

#if RDK_UNCHECKED
    /*
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
    */

       

    /*
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
    */

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
    /// Prompts the user for a save file name and the width, height and depth of an image to be saved.
    /// </summary>
    /// <param name="filename">The original file path.</param>
    /// <param name="width">A width.</param>
    /// <param name="height">An height.</param>
    /// <param name="colorDepth">A color depth.</param>
    /// <returns>The new file name.</returns>
    public static string PromptForSaveImageFileParameters(string filename, ref int width, ref int height, ref int colorDepth)
    {
      using (var sh = new StringHolder())
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

    /*
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
    */
 
    /// <summary>
    /// Finds a file and also handles network shares.
    /// <remarks>This is a replacement for CRhinoFileUtilities::FindFile().</remarks>
    /// </summary>
    /// <param name="fullPathToFile">The file to be found.</param>
    /// <returns>The found file.</returns>
    public static string FindFile(string fullPathToFile)
    {
      using (var sh = new StringHolder())
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
#endif
  }
}

#endif