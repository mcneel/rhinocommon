#pragma warning disable 1591

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Rhino.Geometry;
using Rhino.Display;
using Rhino.Collections;
using System.Collections.Generic;
using Rhino.Render;
using Rhino.Runtime.InteropWrappers;

#if RHINO_SDK
namespace Rhino.Commands
{
  /// <summary>
  /// Argument package that is passed to a custom undo delegate
  /// </summary>
  public class CustomUndoEventArgs : EventArgs
  {
    readonly Guid m_command_id;
    readonly String m_action_description;
    readonly bool m_created_by_redo;
    readonly uint m_undo_event_sn;
    readonly object m_tag;
    readonly RhinoDoc m_doc;

    internal CustomUndoEventArgs(Guid commandId, string description, bool createdByRedo, uint eventSn, object tag, RhinoDoc doc)
    {
      m_command_id = commandId;
      m_action_description = description;
      m_created_by_redo = createdByRedo;
      m_undo_event_sn = eventSn;
      m_tag = tag;
      m_doc = doc;
    }

    public Guid CommandId
    {
      get { return m_command_id; }
    }

    [CLSCompliant(false)]
    public uint UndoSerialNumber
    {
      get { return m_undo_event_sn; }
    }

    public string ActionDescription
    {
      get { return m_action_description; }
    }

    public bool CreatedByRedo
    {
      get { return m_created_by_redo; }
    }

    public object Tag
    {
      get { return m_tag; }
    }

    public RhinoDoc Document
    {
      get { return m_doc; }
    }
  }
}

namespace Rhino
{
  class CustomUndoCallback
  {
    readonly EventHandler<Commands.CustomUndoEventArgs> m_handler;
    public CustomUndoCallback(uint serialNumber, EventHandler<Commands.CustomUndoEventArgs> handler, object tag, string description, RhinoDoc document)
    {
      m_handler = handler;
      SerialNumber = serialNumber;
      Tag = tag;
      Description = description;
      Document = document;
    }
    public uint SerialNumber { get; private set; }
    public EventHandler<Commands.CustomUndoEventArgs> Handler { get { return m_handler; } }
    public object Tag { get; private set; }
    public RhinoDoc Document { get; private set; }
    public string Description { get; private set; }
  }

  /// <summary>
  /// Represents an active model.
  /// </summary>
  public sealed class RhinoDoc
  {
#region statics
    public static bool OpenFile(string path)
    {
      return UnsafeNativeMethods.CRhinoFileMenu_Open(path);
    }
    public static bool ReadFile(string path, FileIO.FileReadOptions options)
    {
      IntPtr const_ptr_options = options.ConstPointer();
      return UnsafeNativeMethods.RHC_RhinoReadFile(path, const_ptr_options);
    }
#endregion
    public bool WriteFile(string path, FileIO.FileWriteOptions options)
    {
      IntPtr const_ptr_options = options.ConstPointer();
      return UnsafeNativeMethods.RHC_RhinoWriteFile(m_docId, path, const_ptr_options);
    }

    /// <summary>
    /// Search for a file using Rhino's search path.  Rhino will look in the
    /// following places:
    /// 1. Current model folder
    /// 2. Path specified in options dialog/File tab
    /// 3. Rhino system folders
    /// 4. Rhino executable folder
    /// </summary>
    /// <param name="filename"></param>
    /// <returns>
    /// Path to existing file if found, an empty string if no file was found
    /// </returns>
    public string FindFile(string filename)
    {
      using (var sh = new StringHolder())
      {
        IntPtr ptr_string = sh.NonConstPointer();
        UnsafeNativeMethods.CRhinoFileUtilities_FindFile(filename, ptr_string);
        return sh.ToString();
      }
    }
 
    internal int m_docId;
    private RhinoDoc(int id)
    {
      m_docId = id;
    }

    private static RhinoDoc g_doc;
    /// <summary>
    /// WARNING!! Do not use the ActiveDoc if you don't have to. Under Mac Rhino the ActiveDoc
    /// can change while a command is running. Use the doc that is passed to you in your RunCommand
    /// function or continue to use the same doc after the first call to ActiveDoc.
    /// </summary>
    public static RhinoDoc ActiveDoc
    {
      get
      {
        int id = UnsafeNativeMethods.CRhinoDoc_ActiveDocId();
        if (g_doc == null || g_doc.m_docId != id)
          g_doc = new RhinoDoc(id);

        return g_doc;
      }
    }

    public static RhinoDoc FromId(int docId)
    {
      if (docId == 0)
        return null;
      if (null != g_doc && g_doc.m_docId == docId)
        return g_doc;
      g_doc = new RhinoDoc(docId);
      return g_doc;
    }

    internal static RhinoDoc FromIntPtr(IntPtr pDoc)
    {
      int id = UnsafeNativeMethods.CRhinoDoc_GetId(pDoc);
      return FromId(id);
    }
    //void GetDefaultObjectAttributes(ON_3dmObjectAttributes&); // attributes to use for new object

    //[skipping] - BoundingBox includes page geometry and can be a little bit misleading
    //ON_BoundingBox BoundingBox() const;
    //void InvalidateBoundingBox();
    //bool Write3DM( ON_BinaryArchive& archive, const wchar_t* sFilename,
    //bool Write3DM( ON_BinaryArchive& archive, const wchar_t* sFilename,
    //bool Read3DM( ON_BinaryArchive& archive, const wchar_t* sFileName,
    //bool Read3DM( ON_BinaryArchive& archive, class CRhinoRead3dmOptions& read_opts );
    //void Destroy();

#region docproperties
    string GetString(int which)
    {
      using (var sh = new StringHolder())
      {
        IntPtr ptr_string = sh.NonConstPointer();
        UnsafeNativeMethods.CRhinoDoc_GetSetString(m_docId, which, false, null, ptr_string);
        return sh.ToString();
      }
    }
    //const int idxName = 0;
    const int IDX_PATH = 1;
    //const int idxUrl = 2;
    const int IDX_NOTES = 3;
    const int IDX_TEMPLATE_FILE_USED = 4;

    ///<summary>Returns the name of the currently loaded Rhino document (3DM file).</summary>
    public string Name
    {
      get
      {
        string path = Path;
        if (!string.IsNullOrEmpty(path))
        {
          path = System.IO.Path.GetFileName(path);
        }
        return path;
      }
    }

    ///<summary>Returns the path of the currently loaded Rhino document (3DM file).</summary>
    public string Path
    {
      get
      {
        return GetString(IDX_PATH);
      }
    }
    /*
        ///<summary>
        ///Returns or sets the uniform resource locator (URL) of the currently
        ///loaded Rhino document (3DM file).
        ///</summary>
        public string URL
        {
          get { return GetString(idxUrl); }
          set { CRhinoDoc_GetSetString(m_doc.m_docId, idxUrl, true, value); }
        }
    */
    ///<summary>Returns or sets the document&apos;s notes.</summary>
    public string Notes
    {
      get { return GetString(IDX_NOTES); }
      set { UnsafeNativeMethods.CRhinoDoc_GetSetString(m_docId, IDX_NOTES, true, value, IntPtr.Zero); }
    }

    public DateTime DateCreated
    {
      get
      {
        int year = 0;
        int month = 0;
        int day = 0;
        int hour = 0;
        int minute = 0;
        UnsafeNativeMethods.CRhinoDoc_GetRevisionDate(m_docId, ref year, ref month, ref day, ref hour, ref minute, true);
        if (year < 1980)
          return DateTime.MinValue;
        return new DateTime(year, month, day, hour, minute, 0);
      }
    }
    public DateTime DateLastEdited
    {
      get
      {
        int year = 0;
        int month = 0;
        int day = 0;
        int hour = 0;
        int minute = 0;
        UnsafeNativeMethods.CRhinoDoc_GetRevisionDate(m_docId, ref year, ref month, ref day, ref hour, ref minute, false);
        if (year < 1980)
          return DateTime.MinValue;
        return new DateTime(year, month, day, hour, minute, 0);
      }
    }

    double GetDouble(UnsafeNativeMethods.CRhDocPropertiesDoubleConsts which)
    {
      return UnsafeNativeMethods.CRhinoDocProperties_GetSetDouble(m_docId, which, false, 0.0);
    }
    void SetDouble(UnsafeNativeMethods.CRhDocPropertiesDoubleConsts which, double val)
    {
      UnsafeNativeMethods.CRhinoDocProperties_GetSetDouble(m_docId, which, true, val);
    }

    /// <summary>Model space absolute tolerance.</summary>
    public double ModelAbsoluteTolerance
    {
      get { return GetDouble(UnsafeNativeMethods.CRhDocPropertiesDoubleConsts.ModelAbsTol); }
      set { SetDouble(UnsafeNativeMethods.CRhDocPropertiesDoubleConsts.ModelAbsTol, value); }
    }
    /// <summary>Model space angle tolerance.</summary>
    public double ModelAngleToleranceRadians
    {
      get { return GetDouble(UnsafeNativeMethods.CRhDocPropertiesDoubleConsts.ModelAngleTol); }
      set { SetDouble(UnsafeNativeMethods.CRhDocPropertiesDoubleConsts.ModelAngleTol, value); }
    }
    /// <summary>Model space angle tolerance.</summary>
    public double ModelAngleToleranceDegrees
    {
      get
      {
        double rc = ModelAngleToleranceRadians;
        rc = RhinoMath.ToDegrees(rc);
        return rc;
      }
      set
      {
        double radians = RhinoMath.ToRadians(value);
        ModelAngleToleranceRadians = radians;
      }
    }
    /// <summary>Model space relative tolerance.</summary>
    public double ModelRelativeTolerance
    {
      get { return GetDouble(UnsafeNativeMethods.CRhDocPropertiesDoubleConsts.ModelRelTol); }
      set { SetDouble(UnsafeNativeMethods.CRhDocPropertiesDoubleConsts.ModelRelTol, value); }
    }

    /// <example>
    /// <code source='examples\vbnet\ex_displayprecision.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_displayprecision.cs' lang='cs'/>
    /// <code source='examples\py\ex_displayprecision.py' lang='py'/>
    /// </example>
    public int ModelDistanceDisplayPrecision
    {
      get { return UnsafeNativeMethods.CRhinoDocProperties_DistanceDisplayPrecision(m_docId, true, 0, false); }
      set { UnsafeNativeMethods.CRhinoDocProperties_DistanceDisplayPrecision(m_docId, true, value, true); }
    }

    public int PageDistanceDisplayPrecision
    {
      get { return UnsafeNativeMethods.CRhinoDocProperties_DistanceDisplayPrecision(m_docId, false, 0, false); }
      set { UnsafeNativeMethods.CRhinoDocProperties_DistanceDisplayPrecision(m_docId, false, value, true); }
    }

    /// <summary>Page space absolute tolerance.</summary>
    public double PageAbsoluteTolerance
    {
      get { return GetDouble(UnsafeNativeMethods.CRhDocPropertiesDoubleConsts.PageAbsTol); }
      set { SetDouble(UnsafeNativeMethods.CRhDocPropertiesDoubleConsts.PageAbsTol, value); }
    }
    /// <summary>Page space angle tolerance.</summary>
    public double PageAngleToleranceRadians
    {
      get { return GetDouble(UnsafeNativeMethods.CRhDocPropertiesDoubleConsts.PageAngleTol); }
      set { SetDouble(UnsafeNativeMethods.CRhDocPropertiesDoubleConsts.PageAngleTol, value); }
    }
    /// <summary>Page space angle tolerance.</summary>
    public double PageAngleToleranceDegrees
    {
      get
      {
        double rc = PageAngleToleranceRadians;
        rc = RhinoMath.ToDegrees(rc);
        return rc;
      }
      set
      {
        double radians = RhinoMath.ToRadians(value);
        PageAngleToleranceRadians = radians;
      }
    }
    /// <summary>Page space relative tolerance.</summary>
    public double PageRelativeTolerance
    {
      get { return GetDouble(UnsafeNativeMethods.CRhDocPropertiesDoubleConsts.PageRelTol); }
      set { SetDouble(UnsafeNativeMethods.CRhDocPropertiesDoubleConsts.PageRelTol, value); }
    }


    const int idxModified = 0;
    internal const int idxRedrawEnabled = 1;
    const int idxIsDocumentReadOnly = 2;
    const int idxIsDocumentLocked = 3;
    const int idxInGet = 4;
    //const int idxInGetPoint = 5;
    //const int idxInGetObject = 6;
    //const int idxInGetString = 7;
    //const int idxInGetNumber = 8;
    //const int idxInGetOption = 9;
    //const int idxInGetColor = 10;
    //const int idxInGetMeshes = 11;
    const int idxIsSendingMail = 12;
    const int idxUndoRecordingEnable = 13;
    const int idxUndoRecordingIsActive = 14;

    internal bool GetBool(int which)
    {
      return UnsafeNativeMethods.CRhinoDoc_GetSetBool(m_docId, which, false, false);
    }

    ///<summary>Returns or sets the document's modified flag.</summary>
    public bool Modified
    {
      get { return GetBool(idxModified); }
      set { UnsafeNativeMethods.CRhinoDoc_GetSetBool(m_docId, idxModified, true, value); }
    }

    ///<summary>
    ///Returns the file version of the current document.  
    ///Use this function to determine which version of Rhino last saved the document.
    ///</summary>
    ///<returns>
    ///The file version (e.g. 1, 2, 3, 4, etc.) or -1 if the document has not been read from disk.
    ///</returns>
    public int ReadFileVersion()
    {
      return UnsafeNativeMethods.CRhinoDocProperties_ReadFileVersion(m_docId);
    }

    public Rhino.UnitSystem ModelUnitSystem
    {
      get
      {
        int rc = UnsafeNativeMethods.CRhinoDocProperties_GetSetUnitSystem(m_docId, true, false, 0);
        return (Rhino.UnitSystem)rc;
      }
      set
      {
        int set_val = (int)value;
        UnsafeNativeMethods.CRhinoDocProperties_GetSetUnitSystem(m_docId, true, true, set_val);
      }
    }

    public string GetUnitSystemName(bool modelUnits, bool capitalize, bool singular, bool abbreviate)
    {
      using (var sh = new StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        UnsafeNativeMethods.CRhinoDocProperties_GetUnitSystemName(m_docId, modelUnits, capitalize, singular, abbreviate, pString);
        return sh.ToString();
      }
    }

    public void AdjustModelUnitSystem(Rhino.UnitSystem newUnitSystem, bool scale)
    {
      UnsafeNativeMethods.CRhinoDocProperties_AdjustUnitSystem(m_docId, true, (int)newUnitSystem, scale);
    }

    /// <example>
    /// <code source='examples\vbnet\ex_addlayout.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addlayout.cs' lang='cs'/>
    /// <code source='examples\py\ex_addlayout.py' lang='py'/>
    /// </example>
    public Rhino.UnitSystem PageUnitSystem
    {
      get
      {
        int rc = UnsafeNativeMethods.CRhinoDocProperties_GetSetUnitSystem(m_docId, false, false, 0);
        return (Rhino.UnitSystem)rc;
      }
      set
      {
        int set_val = (int)value;
        UnsafeNativeMethods.CRhinoDocProperties_GetSetUnitSystem(m_docId, false, true, set_val);
      }
    }
    public void AdjustPageUnitSystem(Rhino.UnitSystem newUnitSystem, bool scale)
    {
      UnsafeNativeMethods.CRhinoDocProperties_AdjustUnitSystem(m_docId, false, (int)newUnitSystem, scale);
    }

    public int DistanceDisplayPrecision
    {
      get
      {
        return ModelDistanceDisplayPrecision;
      }
    }

    #endregion

    //[skipping]
    //const ON_UnitSystem& ModelUnits() const;
    //const ON_UnitSystem& PageUnits() const;

    //[skipping]
    //bool Audit( ON_TextLog* pTestLog, bool bAttemptRepair );
    //void SetModelName(const char*);

    //[skipping]
    //void ViewModified( CRhinoView* ); <- this may belong on the CRhinoView wrapper instead

    //[skipping]
    //CRhinoView* CreateDerivedRhinoView( CRuntimeClass* pMyRhinoViewClass, const wchar_t* lpsViewTitle, const UUID& plug_in_id);

    /// <summary>
    /// Current read-only mode for this document.
    /// true if the document is can be viewed but NOT saved.
    /// false if document can be viewed and saved.
    /// </summary>
    public bool IsReadOnly
    {
      get { return GetBool(idxIsDocumentReadOnly); }
    }

    /// <summary>
    /// Check to see if the file associated with this document is locked.  If it is
    /// locked then this is the only document that will be able to write the file.  Other
    /// instances of Rhino will fail to write this document.
    /// </summary>
    public bool IsLocked
    {
      get { return GetBool(idxIsDocumentLocked); }
    }

    /// <summary>
    /// Gets the Document Id.
    /// </summary>
    public int DocumentId
    {
      get { return m_docId; }
    }

    public Rhino.DocObjects.EarthAnchorPoint EarthAnchorPoint
    {
      get { return new Rhino.DocObjects.EarthAnchorPoint(this); }
      set
      {
        IntPtr pConstAnchor = value.ConstPointer();
        UnsafeNativeMethods.CRhinoDocProperties_SetEarthAnchorPoint(m_docId, pConstAnchor);
      }
    }

    public Rhino.Render.RenderSettings RenderSettings
    {
      get { return new Rhino.Render.RenderSettings(this); }
      set
      {
        IntPtr pConstRenderSettings = value.ConstPointer();
        UnsafeNativeMethods.CRhinoDocProperties_SetRenderSettings(m_docId, pConstRenderSettings);
      }
    }

    /// <summary>
    /// Type of MeshingParameters currently used by the document to mesh objects
    /// </summary>
    public Rhino.Geometry.MeshingParameterStyle MeshingParameterStyle
    {
      get
      {
        int rc = UnsafeNativeMethods.CRhinoDocProperties_GetRenderMeshStyle(m_docId);
        return (Geometry.MeshingParameterStyle)rc;
      }
      set
      {
        UnsafeNativeMethods.CRhinoDocProperties_SetRenderMeshStyle(m_docId, (int)value);
      }
    }

    /// <summary>
    /// Get MeshingParameters currently used by the document
    /// </summary>
    /// <param name="style"></param>
    /// <returns></returns>
    public Rhino.Geometry.MeshingParameters GetMeshingParameters(MeshingParameterStyle style)
    {
      IntPtr pMeshingParameters = UnsafeNativeMethods.CRhinoDocProperties_GetRenderMeshParameters(m_docId, (int)style);
      if (IntPtr.Zero == pMeshingParameters)
        return null;
      return new MeshingParameters(pMeshingParameters);
    }

    /// <summary>
    /// Set the custom meshing parameters that this document will use. You must also modify the
    /// MeshingParameterStyle property on the document to Custom if you want these meshing
    /// parameters to be used
    /// </summary>
    /// <param name="mp"></param>
    public void SetCustomMeshingParameters(MeshingParameters mp)
    {
      IntPtr pConstMeshingParameters = mp.ConstPointer();
      UnsafeNativeMethods.CRhinoDocProperties_SetCustomRenderMeshParameters(m_docId, pConstMeshingParameters);
    }

#region tables
    private Rhino.DocObjects.Tables.ViewTable m_view_table;
    public Rhino.DocObjects.Tables.ViewTable Views
    {
      get { return m_view_table ?? (m_view_table = new Rhino.DocObjects.Tables.ViewTable(this)); }
    }

    private Rhino.DocObjects.Tables.ObjectTable m_object_table;
    public Rhino.DocObjects.Tables.ObjectTable Objects
    {
      get { return m_object_table ?? (m_object_table = new Rhino.DocObjects.Tables.ObjectTable(this)); }
    }

    /// <summary>
    /// Gets the default object attributes for this document. 
    /// The attributes will be linked to the currently active layer 
    /// and they will inherit the Document WireDensity setting.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_objectdecoration.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_objectdecoration.cs' lang='cs'/>
    /// <code source='examples\py\ex_objectdecoration.py' lang='py'/>
    /// </example>
    public DocObjects.ObjectAttributes CreateDefaultAttributes()
    {
      DocObjects.ObjectAttributes rc = new Rhino.DocObjects.ObjectAttributes();
      IntPtr pAttr = rc.NonConstPointer();
      UnsafeNativeMethods.CRhinoDoc_GetDefaultObjectAttributes(m_docId, pAttr);
      return rc;
    }

    private Rhino.DocObjects.Tables.BitmapTable m_bitmap_table;
    /// <summary>
    /// bitmaps used in textures, backgrounds, wallpapers, ...
    /// </summary>
    public Rhino.DocObjects.Tables.BitmapTable Bitmaps
    {
      get { return m_bitmap_table ?? (m_bitmap_table = new Rhino.DocObjects.Tables.BitmapTable(this)); }
    }
    //[skipping]
    //  CRhinoTextureMappingTable m_texture_mapping_table;

    private Rhino.DocObjects.Tables.MaterialTable m_material_table;

    /// <summary>Materials in the document.</summary>
    public Rhino.DocObjects.Tables.MaterialTable Materials
    {
      get { return m_material_table ?? (m_material_table = new Rhino.DocObjects.Tables.MaterialTable(this)); }
    }

    private Rhino.DocObjects.Tables.LinetypeTable m_linetype_table;
    /// <summary>
    /// Linetypes in the document.
    /// </summary>
    public Rhino.DocObjects.Tables.LinetypeTable Linetypes
    {
      get { return m_linetype_table ?? (m_linetype_table = new Rhino.DocObjects.Tables.LinetypeTable(this)); }
    }

    private Rhino.DocObjects.Tables.LayerTable m_layer_table;
    /// <summary>
    /// Layers in the document.
    /// </summary>
    public Rhino.DocObjects.Tables.LayerTable Layers
    {
      get { return m_layer_table ?? (m_layer_table = new Rhino.DocObjects.Tables.LayerTable(this)); }
    }

    private Rhino.DocObjects.Tables.GroupTable m_group_table;
    /// <example>
    /// <code source='examples\vbnet\ex_addobjectstogroup.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addobjectstogroup.cs' lang='cs'/>
    /// <code source='examples\py\ex_addobjectstogroup.py' lang='py'/>
    /// </example>
    public Rhino.DocObjects.Tables.GroupTable Groups
    {
      get { return m_group_table ?? (m_group_table = new Rhino.DocObjects.Tables.GroupTable(this)); }
    }

    private Rhino.DocObjects.Tables.FontTable m_font_table;
    public Rhino.DocObjects.Tables.FontTable Fonts
    {
      get { return m_font_table ?? (m_font_table = new Rhino.DocObjects.Tables.FontTable(this)); }
    }

    private Rhino.DocObjects.Tables.DimStyleTable m_dimstyle_table;
    /// <example>
    /// <code source='examples\vbnet\ex_dimstyle.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_dimstyle.cs' lang='cs'/>
    /// <code source='examples\py\ex_dimstyle.py' lang='py'/>
    /// </example>
    public Rhino.DocObjects.Tables.DimStyleTable DimStyles
    {
      get { return m_dimstyle_table ?? (m_dimstyle_table = new Rhino.DocObjects.Tables.DimStyleTable(this)); }
    }

    private Rhino.DocObjects.Tables.LightTable m_light_table;
    public Rhino.DocObjects.Tables.LightTable Lights
    {
      get { return m_light_table ?? (m_light_table = new Rhino.DocObjects.Tables.LightTable(this)); }
    }

    private Rhino.DocObjects.Tables.HatchPatternTable m_hatchpattern_table;
    /// <example>
    /// <code source='examples\vbnet\ex_hatchcurve.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_hatchcurve.cs' lang='cs'/>
    /// <code source='examples\py\ex_hatchcurve.py' lang='py'/>
    /// </example>
    public Rhino.DocObjects.Tables.HatchPatternTable HatchPatterns
    {
      get { return m_hatchpattern_table ?? (m_hatchpattern_table = new Rhino.DocObjects.Tables.HatchPatternTable(this)); }
    }

    private Rhino.DocObjects.Tables.InstanceDefinitionTable m_instance_definition_table;

    /// <example>
    /// <code source='examples\vbnet\ex_printinstancedefinitiontree.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_printinstancedefinitiontree.cs' lang='cs'/>
    /// <code source='examples\py\ex_printinstancedefinitiontree.py' lang='py'/>
    /// </example>
    public Rhino.DocObjects.Tables.InstanceDefinitionTable InstanceDefinitions
    {
      get
      {
        return m_instance_definition_table ??
               (m_instance_definition_table = new Rhino.DocObjects.Tables.InstanceDefinitionTable(this));
      }
    }
    //[skipping]
    //  CRhinoHistoryRecordTable m_history_record_table;

    private Rhino.DocObjects.Tables.NamedConstructionPlaneTable m_named_cplane_table;
    public Rhino.DocObjects.Tables.NamedConstructionPlaneTable NamedConstructionPlanes
    {
      get
      {
        return m_named_cplane_table ??
               (m_named_cplane_table = new Rhino.DocObjects.Tables.NamedConstructionPlaneTable(this));
      }
    }

    private Rhino.DocObjects.Tables.NamedViewTable m_named_view_table;

    /// <example>
    /// <code source='examples\vbnet\ex_addnamedview.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addnamedview.cs' lang='cs'/>
    /// <code source='examples\py\ex_addnamedview.py' lang='py'/>
    /// </example>
    public Rhino.DocObjects.Tables.NamedViewTable NamedViews
    {
      get { return m_named_view_table ?? (m_named_view_table = new Rhino.DocObjects.Tables.NamedViewTable(this)); }
    }

    private Rhino.DocObjects.Tables.StringTable m_strings;
    public Rhino.DocObjects.Tables.StringTable Strings
    {
      get { return m_strings ?? (m_strings = new Rhino.DocObjects.Tables.StringTable(this)); }
    }

#endregion

#if RDK_CHECKED
    private RenderMaterialTable m_render_materials;
    public RenderMaterialTable RenderMaterials
    {
      get { return (m_render_materials ?? (m_render_materials = new RenderMaterialTable(this))); }
    }
    private RenderEnvironmentTable m_render_environments;
    public RenderEnvironmentTable RenderEnvironments
    {
      get { return (m_render_environments ?? (m_render_environments = new RenderEnvironmentTable(this))); }
    }
    private RenderTextureTable m_render_textures;
    public RenderTextureTable RenderTextures
    {
      get { return (m_render_textures ?? (m_render_textures = new RenderTextureTable(this))); }
    }

    public IEnumerable<RenderPrimitive> GetRenderPrimitives(bool forceTriangleMeshes)
    {
      return new RenderPrimitiveEnumerable(Guid.Empty, null, forceTriangleMeshes);
    }
    public IEnumerable<RenderPrimitive> GetRenderPrimitives(DocObjects.ViewportInfo viewport, bool forceTriangleMeshes)
    {
      return new RenderPrimitiveEnumerable(Guid.Empty, viewport, forceTriangleMeshes);
    }
    public IEnumerable<RenderPrimitive> GetRenderPrimitives(Guid plugInId, DocObjects.ViewportInfo viewport, bool forceTriangleMeshes)
    {
      return new RenderPrimitiveEnumerable(plugInId, viewport, forceTriangleMeshes);
    }

    private Rhino.Render.GroundPlane m_ground_plane;
    /// <summary>Gets the ground plane of this document.</summary>
    /// <exception cref="Rhino.Runtime.RdkNotLoadedException">If the RDK is not loaded.</exception>
    public Rhino.Render.GroundPlane GroundPlane
    {
      get
      {
        if (m_ground_plane == null)
        {
          Rhino.Runtime.HostUtils.CheckForRdk(true, true);
          m_ground_plane = new Rhino.Render.GroundPlane(this);
        }
        return m_ground_plane;
      }
    }
#endif



    //[skipping]
    //  int LookupObject( const CRhinoLayer&, ON_SimpleArray<CRhinoObject*>& ) const;
    //  int LookupObject( const CRhinoLinetype&, ON_SimpleArray<CRhinoObject*>& ) const;
    //  int LookupObject( const CRhinoMaterial&, ON_SimpleArray<CRhinoObject*>& ) const;

#region Getter context utility methods
    /// <summary>
    /// Returns true if currently in a GetPoint.Get(), GetObject.GetObjects(), or GetString.Get()
    /// </summary>
    internal bool InGet
    {
      get { return GetBool(idxInGet); }
    }

    #endregion

    /// <summary>
    /// true if Rhino is in the process of sending this document as an email attachment.
    /// </summary>
    public bool IsSendingMail
    {
      get
      {
        if (Runtime.HostUtils.RunningOnOSX)
          throw new NotSupportedException();
        return GetBool(idxIsSendingMail);
      }
    }

    /// <summary>
    /// name of the template file used to create this document. This is a runtime value
    /// only present if the document was newly created.
    /// </summary>
    public string TemplateFileUsed
    {
      get { return GetString(IDX_TEMPLATE_FILE_USED); }
    }

    public void ClearUndoRecords(bool purgeDeletedObjects)
    {
      UnsafeNativeMethods.CRhinoDoc_ClearUndoRecords(m_docId, purgeDeletedObjects);
    }

    public void ClearRedoRecords()
    {
      UnsafeNativeMethods.CRhinoDoc_ClearRedoRecords(m_docId);
    }

    public bool UndoRecordingEnabled
    {
      get { return GetBool(idxUndoRecordingEnable); }
      set { UnsafeNativeMethods.CRhinoDoc_GetSetBool(m_docId, idxUndoRecordingEnable, true, value); }
    }

    /// <summary>
    /// true if undo recording is actually happening now.
    /// </summary>
    public bool UndoRecordingIsActive
    {
      get { return GetBool(idxUndoRecordingIsActive); }
    }

    /// <summary>
    /// Instructs Rhino to begin recording undo information when the document
    /// is changed outside of a command. We use this, e.g., to save changes
    /// caused by the modeless layer or object properties dialogs
    /// when commands are not running.
    /// </summary>
    /// <param name="description">A text describing the record.</param>
    /// <returns>
    /// Serial number of record.  Returns 0 if record is not started
    /// because undo information is already being recorded or
    /// undo is disabled.
    /// </returns>
    [CLSCompliant(false)]
    public uint BeginUndoRecord(string description)
    {
      return UnsafeNativeMethods.CRhinoDoc_BeginUndoRecordEx(m_docId, description);
    }

    static List<CustomUndoCallback> m_custom_undo_callbacks;
    internal delegate void RhinoUndoEventHandlerCallback(Guid command_id, IntPtr action_description, int created_by_redo, uint sn);
    internal delegate void RhinoDeleteUndoEventHandlerCallback(uint sn);

    static RhinoUndoEventHandlerCallback m_undo_event_handler;
    static RhinoDeleteUndoEventHandlerCallback m_delete_undo_event_handler;

    static void OnUndoEventHandler(Guid command_id, IntPtr action_description, int created_by_redo, uint sn)
    {
      if (m_custom_undo_callbacks != null)
      {
        for (int i = 0; i < m_custom_undo_callbacks.Count; i++)
        {
          if (m_custom_undo_callbacks[i].SerialNumber == sn)
          {
            var handler = m_custom_undo_callbacks[i].Handler;
            if( handler!=null )
            {
              try
              {
                object tag = m_custom_undo_callbacks[i].Tag;
                string description = m_custom_undo_callbacks[i].Description;
                RhinoDoc doc = m_custom_undo_callbacks[i].Document;
                handler(null, new Commands.CustomUndoEventArgs(command_id, description, created_by_redo == 1, sn, tag, doc));
              }
              catch (Exception ex)
              {
                Rhino.Runtime.HostUtils.ExceptionReport("OnUndoEventHandler", ex);
              }
            }
            break;
          }
        }
      }
    }

    static void OnDeleteUndoEventHandler(uint sn)
    {
      if (m_custom_undo_callbacks != null)
      {
        for (int i = 0; i < m_custom_undo_callbacks.Count; i++)
        {
          if (m_custom_undo_callbacks[i].SerialNumber == sn)
          {
            m_custom_undo_callbacks.RemoveAt(i);
            return;
          }
        }
      }
    }

    public bool AddCustomUndoEvent(string description, EventHandler<Rhino.Commands.CustomUndoEventArgs> handler)
    {
      return AddCustomUndoEvent(description, handler, null);
    }

    /// <summary>
    /// Add a custom undo event so you can undo private plug-in data
    /// when the user performs an undo or redo
    /// </summary>
    /// <param name="description"></param>
    /// <param name="handler"></param>
    /// <param name="tag"></param>
    /// <returns></returns>
    /// <example>
    /// <code source='examples\vbnet\ex_customundo.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_customundo.cs' lang='cs'/>
    /// <code source='examples\py\ex_customundo.py' lang='py'/>
    /// </example>
    public bool AddCustomUndoEvent(string description, EventHandler<Rhino.Commands.CustomUndoEventArgs> handler, object tag)
    {
      if (string.IsNullOrEmpty(description) || handler == null)
        return false;

      m_undo_event_handler = OnUndoEventHandler;
      m_delete_undo_event_handler = OnDeleteUndoEventHandler;

      uint rc = UnsafeNativeMethods.CRhinoDoc_AddCustomUndoEvent(m_docId, description, m_undo_event_handler, m_delete_undo_event_handler);
      if (rc == 0)
        return false;

      if (m_custom_undo_callbacks == null)
        m_custom_undo_callbacks = new List<CustomUndoCallback>();
      m_custom_undo_callbacks.Add(new CustomUndoCallback(rc, handler, tag, description, this));
      return true;
    }


    [CLSCompliant(false)]
    public bool EndUndoRecord(uint undoRecordSerialNumber)
    {
      return UnsafeNativeMethods.CRhinoDoc_EndUndoRecord(m_docId, undoRecordSerialNumber);
    }

    //  bool Undo( CRhUndoRecord* = NULL );
    //  bool Redo();

    //  Returns:
    //    True if Undo() is currently active.  
    //  bool UndoActive() const;

    //  Returns:
    //    True if Redo() is currently active.  
    //  bool RedoActive() const;

    //  Returns:
    //    Record being currently recorded.  If undo recording is disabled
    //    or nothing is currently being recorded, then NULL is returned.
    //  */
    //  CRhUndoRecord* CurrentUndoRecord() const;
    //  /*
    //  Returns:
    //    >0: undo recording is active and being saved on
    //        the undo record with the specified serial number.
    //     0: undo recording is not active. (Disabled or nothing is
    //        being recorded.)
    //  */
    //  unsigned int CurrentUndoRecordSerialNumber() const;

    //  /*
    //  Returns:
    //    Number of undo records.
    //  */
    //  int GetUndoRecords( ON_SimpleArray<CRhUndoRecord* >& ) const;

    //  /*
    //  Returns: 
    //    Number of undo records.
    //  */
    //  int UndoRecordCount() const;

    //  /*
    //  Returns: 
    //    Number bytes in used by undo records
    //  */
    //  size_t UndoRecordMemorySize() const;

    //  /*
    //  Description:
    //    Culls the undo list to release memory.
    //  Parameters:
    //    min_step_count - [in] 
    //      minimum number of undo steps to keep.
    //    max_memory_size_bytes - [in] 
    //      maximum amount of memory, in bytes, for undo list to use.
    //  Returns:
    //    Number of culled records.    
    //  Remarks:
    //    In the version with no arguments, the settings in
    //    RhinoApp().AppSettings().GeneralSettings() are used.
    //  */
    //  int CullUndoRecords();

    //  int CullUndoRecords( 
    //        int min_step_count, 
    //        size_t max_memory_size_bytes
    //        );


    //  /*
    //  Returns true if document contains undo records.
    //  */
    //  bool HasUndoRecords() const;

    //  /*
    //  Returns:
    //    Number of undo records.
    //  */
    //  int GetRedoRecords( ON_SimpleArray<CRhUndoRecord* >& ) const;

    //  class CRhSelSetManager* m_selset_manager;

    //  void ChangeTitleToUnNamed();

    //  /*
    //  Universal construction plane stack operators
    //  */
    //  void PushConstructionPlane( const ON_Plane& plane );
    //  bool ActiveConstructionPlane( ON_Plane& plane );
    //  bool NextConstructionPlane( ON_Plane& plane );
    //  bool PrevConstructionPlane( ON_Plane& plane );
    //  int ConstructionPlaneCount() const;


    internal bool CreatePreviewImage(string imagePath, Guid viewportId, Rhino.Drawing.Size size, int settings, bool wireframe)
    {
      int width = size.Width;
      int height = size.Height;
      bool rc = UnsafeNativeMethods.CRhinoDoc_CreatePreviewImage(m_docId, imagePath, viewportId, width, height, settings, wireframe);
      return rc;
    }

    ///<summary>Extracts the bitmap preview image from the specified model (3DM).</summary>
    ///<param name='path'>
    ///The model (3DM) from which to extract the preview image.
    ///If null, the currently loaded model is used.
    ///</param>
    ///<returns>true on success.</returns>
    static public Rhino.Drawing.Bitmap ExtractPreviewImage(string path)
    {
      if (string.IsNullOrEmpty(path))
      {
        path = RhinoDoc.ActiveDoc.Path;
      }
      IntPtr pRhinoDib = UnsafeNativeMethods.CRhinoDoc_ExtractPreviewImage(path);
      if (IntPtr.Zero == pRhinoDib)
        return null;

      Rhino.Drawing.Bitmap rc = null;
      IntPtr hBmp = UnsafeNativeMethods.CRhinoDib_Bitmap(pRhinoDib);
      if (IntPtr.Zero != hBmp)
      {
        rc = Rhino.Drawing.Image.FromHbitmap(hBmp);
      }
      UnsafeNativeMethods.CRhinoDib_Delete(pRhinoDib);
      return rc;
    }


#region events
    internal delegate void DocumentCallback(int docId);
    private static DocumentCallback m_OnCloseDocumentCallback;
    private static DocumentCallback m_OnNewDocumentCallback;
    private static DocumentCallback m_OnDocumentPropertiesChanged;
    private static void OnCloseDocument(int docId)
    {
      if (m_close_document != null)
      {
        try
        {
          m_close_document(null, new DocumentEventArgs(docId));
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    private static void OnNewDocument(int docId)
    {
      if (m_new_document != null)
      {
        try
        {
          m_new_document(null, new DocumentEventArgs(docId));
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    private static void OnDocumentPropertiesChanged(int docId)
    {
      if (m_document_properties_changed != null)
      {
        try
        {
          m_document_properties_changed(null, new DocumentEventArgs(docId));
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }

    internal delegate void DocumentIoCallback(int docId, IntPtr filename, int b1, int b2);
    private static void OnBeginOpenDocument(int docId, IntPtr filename, int bMerge, int bReference)
    {
      if (m_begin_open_document != null)
      {
        try
        {
          m_begin_open_document(null, new DocumentOpenEventArgs(docId, filename, bMerge != 0, bReference != 0));
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    private static void OnEndOpenDocument(int docId, IntPtr filename, int bMerge, int bReference)
    {
      if (m_end_open_document != null)
      {
        try
        {
          m_end_open_document(null, new DocumentOpenEventArgs(docId, filename, bMerge != 0, bReference != 0));
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    private static void OnBeginSaveDocument(int docId, IntPtr filename, int bExportSelected, int bUnused)
    {
      if (m_begin_save_document != null)
      {
        try
        {
          m_begin_save_document(null, new DocumentSaveEventArgs(docId, filename, bExportSelected != 0));
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    private static void OnEndSaveDocument(int docId, IntPtr filename, int bExportSelected, int bUnused)
    {
      if (m_end_save_document != null)
      {
        try
        {
          m_end_save_document(null, new DocumentSaveEventArgs(docId, filename, bExportSelected != 0));
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }

    private static readonly object m_event_lock = new object();
    internal static EventHandler<DocumentEventArgs> m_close_document;
    public static event EventHandler<DocumentEventArgs> CloseDocument
    {
      add
      {
        lock (m_event_lock)
        {
          if (m_close_document == null)
          {
            m_OnCloseDocumentCallback = OnCloseDocument;
            UnsafeNativeMethods.CRhinoEventWatcher_SetCloseDocumentCallback(m_OnCloseDocumentCallback, Rhino.Runtime.HostUtils.m_ew_report);
          }
          m_close_document -= value;
          m_close_document += value;
        }
      }
      remove
      {
        lock (m_event_lock)
        {
          m_close_document -= value;
          if (m_close_document == null)
          {
            UnsafeNativeMethods.CRhinoEventWatcher_SetCloseDocumentCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
            m_OnCloseDocumentCallback = null;
          }
        }
      }
    }

    internal static EventHandler<DocumentEventArgs> m_new_document;
    public static event EventHandler<DocumentEventArgs> NewDocument
    {
      add
      {
        lock (m_event_lock)
        {
          if (m_new_document == null)
          {
            m_OnNewDocumentCallback = OnNewDocument;
            UnsafeNativeMethods.CRhinoEventWatcher_SetNewDocumentCallback(m_OnNewDocumentCallback, Rhino.Runtime.HostUtils.m_ew_report);
          }
          m_new_document -= value;
          m_new_document += value;
        }
      }
      remove
      {
        lock (m_event_lock)
        {
          m_new_document -= value;
          if (m_new_document == null)
          {
            UnsafeNativeMethods.CRhinoEventWatcher_SetNewDocumentCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
            m_OnNewDocumentCallback = null;
          }
        }
      }
    }

    internal static EventHandler<DocumentEventArgs> m_document_properties_changed;
    public static event EventHandler<DocumentEventArgs> DocumentPropertiesChanged
    {
      add
      {
        lock (m_event_lock)
        {
          if (m_document_properties_changed == null)
          {
            m_OnDocumentPropertiesChanged = OnDocumentPropertiesChanged;
            UnsafeNativeMethods.CRhinoEventWatcher_SetDocPropChangeCallback(m_OnDocumentPropertiesChanged, Rhino.Runtime.HostUtils.m_ew_report);
          }
          m_document_properties_changed -= value;
          m_document_properties_changed += value;
        }
      }
      remove
      {
        lock (m_event_lock)
        {
          m_document_properties_changed -= value;
          if (m_document_properties_changed == null)
          {
            UnsafeNativeMethods.CRhinoEventWatcher_SetDocPropChangeCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
            m_OnDocumentPropertiesChanged = null;
          }
        }
      }
    }

    private static DocumentIoCallback m_OnBeginOpenDocument;
    private static DocumentIoCallback m_OnEndOpenDocument;
    private static DocumentIoCallback m_OnBeginSaveDocument;
    private static DocumentIoCallback m_OnEndSaveDocument;
    internal static EventHandler<DocumentOpenEventArgs> m_begin_open_document;
    public static event EventHandler<DocumentOpenEventArgs> BeginOpenDocument
    {
      add
      {
        lock (m_event_lock)
        {
          if (m_begin_open_document == null)
          {
            m_OnBeginOpenDocument = OnBeginOpenDocument;
            UnsafeNativeMethods.CRhinoEventWatcher_SetBeginOpenDocumentCallback(m_OnBeginOpenDocument, Rhino.Runtime.HostUtils.m_ew_report);
          }
          m_begin_open_document -= value;
          m_begin_open_document += value;
        }
      }
      remove
      {
        lock (m_event_lock)
        {
          m_begin_open_document -= value;
          if (m_begin_open_document == null)
          {
            UnsafeNativeMethods.CRhinoEventWatcher_SetBeginOpenDocumentCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
            m_OnBeginOpenDocument = null;
          }
        }
      }
    }

    internal static EventHandler<DocumentOpenEventArgs> m_end_open_document;
    public static event EventHandler<DocumentOpenEventArgs> EndOpenDocument
    {
      add
      {
        lock (m_event_lock)
        {
          if (m_end_open_document == null)
          {
            m_OnEndOpenDocument = OnEndOpenDocument;
            UnsafeNativeMethods.CRhinoEventWatcher_SetEndOpenDocumentCallback(m_OnEndOpenDocument, Rhino.Runtime.HostUtils.m_ew_report);
          }
          m_end_open_document -= value;
          m_end_open_document += value;
        }
      }
      remove
      {
        lock (m_event_lock)
        {
          m_end_open_document -= value;
          if (m_end_open_document == null)
          {
            UnsafeNativeMethods.CRhinoEventWatcher_SetEndOpenDocumentCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
            m_OnEndOpenDocument = null;
          }
        }
      }
    }

    internal static EventHandler<DocumentSaveEventArgs> m_begin_save_document;
    public static event EventHandler<DocumentSaveEventArgs> BeginSaveDocument
    {
      add
      {
        lock (m_event_lock)
        {
          if (m_begin_save_document == null)
          {
            m_OnBeginSaveDocument = OnBeginSaveDocument;
            UnsafeNativeMethods.CRhinoEventWatcher_SetBeginSaveDocumentCallback(m_OnBeginSaveDocument, Rhino.Runtime.HostUtils.m_ew_report);
          }
          m_begin_save_document -= value;
          m_begin_save_document += value;
        }
      }
      remove
      {
        lock (m_event_lock)
        {
          m_begin_save_document -= value;
          if (m_begin_save_document == null)
          {
            UnsafeNativeMethods.CRhinoEventWatcher_SetBeginSaveDocumentCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
            m_OnBeginSaveDocument = null;
          }
        }
      }
    }

    internal static EventHandler<DocumentSaveEventArgs> m_end_save_document;
    public static event EventHandler<DocumentSaveEventArgs> EndSaveDocument
    {
      add
      {
        lock (m_event_lock)
        {
          if (m_end_save_document == null)
          {
            m_OnEndSaveDocument = OnEndSaveDocument;
            UnsafeNativeMethods.CRhinoEventWatcher_SetEndSaveDocumentCallback(m_OnEndSaveDocument, Rhino.Runtime.HostUtils.m_ew_report);
          }
          m_end_save_document -= value;
          m_end_save_document += value;
        }
      }
      remove
      {
        lock (m_event_lock)
        {
          m_end_save_document -= value;
          if (m_end_save_document == null)
          {
            UnsafeNativeMethods.CRhinoEventWatcher_SetEndSaveDocumentCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
            m_OnEndSaveDocument = null;
          }
        }
      }
    }

    internal delegate void RhinoObjectCallback(IntPtr pDoc, IntPtr pObject, IntPtr pObject2);

    private static RhinoObjectCallback m_OnAddRhinoObject;
    private static RhinoObjectCallback m_OnDeleteObjectCallback;
    private static RhinoObjectCallback m_OnReplaceObject;
    private static RhinoObjectCallback m_OnUndeleteObject;
    private static RhinoObjectCallback m_OnPurgeObject;
    private static void OnAddObject(IntPtr pDoc, IntPtr pObject, IntPtr pObject2)
    {
      if (m_add_object != null)
      {
        try
        {
          m_add_object(null, new DocObjects.RhinoObjectEventArgs(pDoc, pObject));
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }

    internal static EventHandler<DocObjects.RhinoObjectEventArgs> m_add_object;
    /// <summary>Called if a new object is added to the document.</summary>
    public static event EventHandler<DocObjects.RhinoObjectEventArgs> AddRhinoObject
    {
      add
      {
        lock (m_event_lock)
        {
          if (m_add_object == null)
          {
            m_OnAddRhinoObject = OnAddObject;
            UnsafeNativeMethods.CRhinoEventWatcher_SetAddObjectCallback(m_OnAddRhinoObject, Rhino.Runtime.HostUtils.m_ew_report);
          }
          m_add_object -= value;
          m_add_object += value;
        }
      }
      remove
      {
        lock (m_event_lock)
        {
          m_add_object -= value;
          if (m_add_object == null)
          {
            UnsafeNativeMethods.CRhinoEventWatcher_SetAddObjectCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
            m_OnAddRhinoObject = null;
          }
        }
      }
    }

    private static void OnDeleteObject(IntPtr pDoc, IntPtr pObject, IntPtr pObject2)
    {
      if (m_delete_object != null)
      {
        bool old_state = RhinoApp.InEventWatcher;
        RhinoApp.InEventWatcher = true;
        try
        {
          m_delete_object(null, new DocObjects.RhinoObjectEventArgs(pDoc, pObject));
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
        RhinoApp.InEventWatcher = old_state;
      }
    }
    internal static EventHandler<DocObjects.RhinoObjectEventArgs> m_delete_object;
    /// <summary>
    /// Called if an object is deleted. At some later point the object can be un-deleted.
    /// </summary>
    public static event EventHandler<DocObjects.RhinoObjectEventArgs> DeleteRhinoObject
    {
      add
      {
        lock (m_event_lock)
        {
          if (m_delete_object == null)
          {
            m_OnDeleteObjectCallback = OnDeleteObject;
            UnsafeNativeMethods.CRhinoEventWatcher_SetDeleteObjectCallback(m_OnDeleteObjectCallback, Rhino.Runtime.HostUtils.m_ew_report);
          }
          m_delete_object -= value;
          m_delete_object += value;
        }
      }
      remove
      {
        lock (m_event_lock)
        {
          m_delete_object -= value;
          if (m_delete_object == null)
          {
            UnsafeNativeMethods.CRhinoEventWatcher_SetDeleteObjectCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
            m_OnDeleteObjectCallback = null;
          }
        }
      }
    }

    private static void OnReplaceObject(IntPtr pDoc, IntPtr pOldObject, IntPtr pNewObject)
    {
      if (m_replace_object != null)
      {
        try
        {
          m_replace_object(null, new DocObjects.RhinoReplaceObjectEventArgs(pDoc, pOldObject, pNewObject));
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    internal static EventHandler<DocObjects.RhinoReplaceObjectEventArgs> m_replace_object;
    /// <summary>
    /// Called if an object is about to be replaced.
    /// If either RhinoDoc::UndoActive() or RhinoDoc::RedoActive() is true,
    /// then immediatedly after ReplaceObject is called there will be a call
    /// to DeleteObject and then a call to AddObject.
    ///
    /// If both RhinoDoc::UndoActive() and RhinoDoc::RedoActive() are false,
    /// then immediatedly after ReplaceObject is called there will be a call
    /// to DeleteObject and then a call to UndeleteObject.
    /// </summary>
    public static event EventHandler<DocObjects.RhinoReplaceObjectEventArgs> ReplaceRhinoObject
    {
      add
      {
        lock (m_event_lock)
        {
          if (m_replace_object == null)
          {
            m_OnReplaceObject = OnReplaceObject;
            UnsafeNativeMethods.CRhinoEventWatcher_SetReplaceObjectCallback(m_OnReplaceObject, Rhino.Runtime.HostUtils.m_ew_report);
          }
          m_replace_object -= value;
          m_replace_object += value;
        }
      }
      remove
      {
        lock (m_event_lock)
        {
          m_replace_object -= value;
          if (m_replace_object == null)
          {
            UnsafeNativeMethods.CRhinoEventWatcher_SetReplaceObjectCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
            m_OnReplaceObject = null;
          }
        }
      }
    }

    private static void OnUndeleteObject(IntPtr pDoc, IntPtr pObject, IntPtr pObject2)
    {
      if (m_undelete_object != null)
      {
        try
        {
          m_undelete_object(null, new DocObjects.RhinoObjectEventArgs(pDoc, pObject));
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    internal static EventHandler<DocObjects.RhinoObjectEventArgs> m_undelete_object;
    /// <summary>Called if an object is un-deleted.</summary>
    public static event EventHandler<DocObjects.RhinoObjectEventArgs> UndeleteRhinoObject
    {
      add
      {
        lock (m_event_lock)
        {
          if (m_undelete_object == null)
          {
            m_OnUndeleteObject = OnUndeleteObject;
            UnsafeNativeMethods.CRhinoEventWatcher_SetUnDeleteObjectCallback(m_OnUndeleteObject, Rhino.Runtime.HostUtils.m_ew_report);
          }
          m_undelete_object -= value;
          m_undelete_object += value;
        }
      }
      remove
      {
        lock (m_event_lock)
        {
          m_undelete_object -= value;
          if (m_undelete_object == null)
          {
            UnsafeNativeMethods.CRhinoEventWatcher_SetUnDeleteObjectCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
            m_OnUndeleteObject = null;
          }
        }
      }
    }

    private static void OnPurgeObject(IntPtr pDoc, IntPtr pObject, IntPtr pObject2)
    {
      if (m_purge_object != null)
      {
        try
        {
          m_purge_object(null, new DocObjects.RhinoObjectEventArgs(pDoc, pObject));
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    internal static EventHandler<DocObjects.RhinoObjectEventArgs> m_purge_object;
    /// <summary>
    /// Called if an object is being purged from a document. The object will cease to exist forever.
    /// </summary>
    public static event EventHandler<DocObjects.RhinoObjectEventArgs> PurgeRhinoObject
    {
      add
      {
        lock (m_event_lock)
        {
          if (m_purge_object == null)
          {
            m_OnPurgeObject = OnPurgeObject;
            UnsafeNativeMethods.CRhinoEventWatcher_SetPurgeObjectCallback(m_OnPurgeObject, Rhino.Runtime.HostUtils.m_ew_report);
          }
          m_purge_object -= value;
          m_purge_object += value;
        }
      }
      remove
      {
        lock (m_event_lock)
        {
          m_purge_object -= value;
          if (m_purge_object == null)
          {
            UnsafeNativeMethods.CRhinoEventWatcher_SetPurgeObjectCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
            m_OnPurgeObject = null;
          }
        }
      }
    }

    internal delegate void RhinoObjectSelectionCallback(int docId, int select, IntPtr pObject, IntPtr pObjects);

    private static RhinoObjectSelectionCallback m_OnSelectRhinoObjectCallback;

    private static void OnSelectObject(int docId, int bSelect, IntPtr pObject, IntPtr pObjects)
    {
      if (m_select_objects != null && bSelect == 1)
      {
        try
        {
          DocObjects.RhinoObjectSelectionEventArgs args = new Rhino.DocObjects.RhinoObjectSelectionEventArgs(true, docId, pObject, pObjects);
          m_select_objects(null, args);
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
      else if (m_deselect_objects != null && bSelect == 0)
      {
        try
        {
          DocObjects.RhinoObjectSelectionEventArgs args = new Rhino.DocObjects.RhinoObjectSelectionEventArgs(false, docId, pObject, pObjects);
          m_deselect_objects(null, args);
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    internal static EventHandler<DocObjects.RhinoObjectSelectionEventArgs> m_select_objects;
    internal static EventHandler<DocObjects.RhinoObjectSelectionEventArgs> m_deselect_objects;

    /// <summary>
    /// Called when object(s) are selected.
    /// </summary>
    public static event EventHandler<DocObjects.RhinoObjectSelectionEventArgs> SelectObjects
    {
      add
      {
        lock (m_event_lock)
        {
          if (m_select_objects == null)
          {
            m_OnSelectRhinoObjectCallback = OnSelectObject;
            UnsafeNativeMethods.CRhinoEventWatcher_SetSelectObjectCallback(m_OnSelectRhinoObjectCallback, Rhino.Runtime.HostUtils.m_ew_report);
          }
          m_select_objects -= value;
          m_select_objects += value;
        }
      }
      remove
      {
        lock (m_event_lock)
        {
          m_select_objects -= value;
          if (m_select_objects == null && m_deselect_objects == null)
          {
            UnsafeNativeMethods.CRhinoEventWatcher_SetSelectObjectCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
            m_OnSelectRhinoObjectCallback = null;
          }
        }
      }
    }

    /// <summary>
    /// Called when object(s) are deselected.
    /// </summary>
    public static event EventHandler<DocObjects.RhinoObjectSelectionEventArgs> DeselectObjects
    {
      add
      {
        lock (m_event_lock)
        {
          if (m_deselect_objects == null)
          {
            m_OnSelectRhinoObjectCallback = OnSelectObject;
            UnsafeNativeMethods.CRhinoEventWatcher_SetSelectObjectCallback(m_OnSelectRhinoObjectCallback, Rhino.Runtime.HostUtils.m_ew_report);
          }
          m_deselect_objects -= value;
          m_deselect_objects += value;
        }
      }
      remove
      {
        lock (m_event_lock)
        {
          m_deselect_objects -= value;
          if (m_select_objects == null && m_deselect_objects == null)
          {
            UnsafeNativeMethods.CRhinoEventWatcher_SetSelectObjectCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
            m_OnSelectRhinoObjectCallback = null;
          }
        }
      }
    }

    internal delegate void RhinoDeselectAllObjectsCallback(int docId, int objectCount);
    private static RhinoDeselectAllObjectsCallback m_OnDeselectAllRhinoObjectsCallback;

    private static void OnDeselectAllObjects(int docId, int count)
    {
      if (m_deselect_allobjects != null)
      {
        try
        {
          DocObjects.RhinoDeselectAllObjectsEventArgs args = new DocObjects.RhinoDeselectAllObjectsEventArgs(docId, count);
          m_deselect_allobjects(null, args);
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    internal static EventHandler<DocObjects.RhinoDeselectAllObjectsEventArgs> m_deselect_allobjects;

    /// <summary>
    /// Called when all objects are deselected.
    /// </summary>
    public static event EventHandler<DocObjects.RhinoDeselectAllObjectsEventArgs> DeselectAllObjects
    {
      add
      {
        lock (m_event_lock)
        {
          if (m_deselect_allobjects == null)
          {
            m_OnDeselectAllRhinoObjectsCallback = OnDeselectAllObjects;
            UnsafeNativeMethods.CRhinoEventWatcher_SetDeselectAllObjectsCallback(m_OnDeselectAllRhinoObjectsCallback, Rhino.Runtime.HostUtils.m_ew_report);
          }
          m_deselect_allobjects -= value;
          m_deselect_allobjects += value;
        }
      }
      remove
      {
        lock (m_event_lock)
        {
          m_deselect_allobjects -= value;
          if (m_deselect_allobjects == null)
          {
            UnsafeNativeMethods.CRhinoEventWatcher_SetDeselectAllObjectsCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
            m_OnDeselectAllRhinoObjectsCallback = null;
          }
        }
      }
    }


    internal delegate void RhinoModifyObjectAttributesCallback(int docId, IntPtr pRhinoObject, IntPtr pConstRhinoObjectAttributes);
    private static RhinoModifyObjectAttributesCallback m_OnModifyObjectAttributesCallback;

    private static void OnModifyObjectAttributes(int docId, IntPtr pRhinoObject, IntPtr pConstRhinoObjectAttributes)
    {
      if (m_modify_object_attributes != null)
      {
        try
        {
          DocObjects.RhinoModifyObjectAttributesEventArgs args = new DocObjects.RhinoModifyObjectAttributesEventArgs(docId, pRhinoObject, pConstRhinoObjectAttributes);
          m_modify_object_attributes(null, args);
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    internal static EventHandler<DocObjects.RhinoModifyObjectAttributesEventArgs> m_modify_object_attributes;

    /// <summary>
    /// Called when all object attributes are changed.
    /// </summary>
    public static event EventHandler<DocObjects.RhinoModifyObjectAttributesEventArgs> ModifyObjectAttributes
    {
      add
      {
        lock (m_event_lock)
        {
          if (m_modify_object_attributes == null)
          {
            m_OnModifyObjectAttributesCallback = OnModifyObjectAttributes;
            UnsafeNativeMethods.CRhinoEventWatcher_SetModifyObjectAttributesCallback(m_OnModifyObjectAttributesCallback, Rhino.Runtime.HostUtils.m_ew_report);
          }
          m_modify_object_attributes -= value;
          m_modify_object_attributes += value;
        }
      }
      remove
      {
        lock (m_event_lock)
        {
          m_modify_object_attributes -= value;
          if (m_modify_object_attributes == null)
          {
            UnsafeNativeMethods.CRhinoEventWatcher_SetModifyObjectAttributesCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
            m_OnModifyObjectAttributesCallback = null;
          }
        }
      }
    }


    internal delegate void RhinoTableCallback(int docId, int eventType, int index, IntPtr pConstOldSettings);
    private static RhinoTableCallback m_OnLayerTableEventCallback;
    private static void OnLayerTableEvent(int docId, int eventType, int index, IntPtr pConstOldSettings)
    {
      if (m_layer_table_event != null)
      {
        try
        {
          DocObjects.Tables.LayerTableEventArgs args = new DocObjects.Tables.LayerTableEventArgs(docId, eventType, index, pConstOldSettings);
          m_layer_table_event(null, args);
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    internal static EventHandler<DocObjects.Tables.LayerTableEventArgs> m_layer_table_event;

    /// <summary>
    /// Called when any modification happens to a document's layer table.
    /// </summary>
    public static event EventHandler<Rhino.DocObjects.Tables.LayerTableEventArgs> LayerTableEvent
    {
      add
      {
        lock (m_event_lock)
        {
          if (m_layer_table_event == null)
          {
            m_OnLayerTableEventCallback = OnLayerTableEvent;
            UnsafeNativeMethods.CRhinoEventWatcher_SetLayerTableEventCallback(m_OnLayerTableEventCallback, Rhino.Runtime.HostUtils.m_ew_report);
          }
          m_layer_table_event -= value;
          m_layer_table_event += value;
        }
      }
      remove
      {
        lock (m_event_lock)
        {
          m_layer_table_event -= value;
          if (m_layer_table_event == null)
          {
            UnsafeNativeMethods.CRhinoEventWatcher_SetLayerTableEventCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
            m_OnLayerTableEventCallback = null;
          }
        }
      }
    }

    private static RhinoTableCallback m_OnIdefTableEventCallback;
    private static void OnIdefTableEvent(int docId, int eventType, int index, IntPtr pConstOldSettings)
    {
      if (m_idef_table_event != null)
      {
        try
        {
          var args = new DocObjects.Tables.InstanceDefinitionTableEventArgs(docId, eventType, index, pConstOldSettings);
          m_idef_table_event(null, args);
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    internal static EventHandler<DocObjects.Tables.InstanceDefinitionTableEventArgs> m_idef_table_event;

    /// <summary>
    /// Called when any modification happens to a document's light table.
    /// </summary>
    public static event EventHandler<Rhino.DocObjects.Tables.InstanceDefinitionTableEventArgs> InstanceDefinitionTableEvent
    {
      add
      {
        lock (m_event_lock)
        {
          if (m_idef_table_event == null)
          {
            m_OnIdefTableEventCallback = OnIdefTableEvent;
            UnsafeNativeMethods.CRhinoEventWatcher_SetIdefTableEventCallback(m_OnIdefTableEventCallback, Rhino.Runtime.HostUtils.m_ew_report);
          }
          m_idef_table_event -= value;
          m_idef_table_event += value;
        }
      }
      remove
      {
        lock (m_event_lock)
        {
          m_idef_table_event -= value;
          if (m_idef_table_event == null)
          {
            UnsafeNativeMethods.CRhinoEventWatcher_SetIdefTableEventCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
            m_OnIdefTableEventCallback = null;
          }
        }
      }
    }

    private static RhinoTableCallback m_OnLightTableEventCallback;
    private static void OnLightTableEvent(int docId, int eventType, int index, IntPtr pConstOldSettings)
    {
      if (m_light_table_event != null)
      {
        try
        {
          DocObjects.Tables.LightTableEventArgs args = new DocObjects.Tables.LightTableEventArgs(docId, eventType, index, pConstOldSettings);
          m_light_table_event(null, args);
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    internal static EventHandler<DocObjects.Tables.LightTableEventArgs> m_light_table_event;

    /// <summary>
    /// Called when any modification happens to a document's light table.
    /// </summary>
    public static event EventHandler<Rhino.DocObjects.Tables.LightTableEventArgs> LightTableEvent
    {
      add
      {
        lock (m_event_lock)
        {
          if (m_light_table_event == null)
          {
            m_OnLightTableEventCallback = OnLightTableEvent;
            UnsafeNativeMethods.CRhinoEventWatcher_SetLightTableEventCallback(m_OnLightTableEventCallback, Rhino.Runtime.HostUtils.m_ew_report);
          }
          m_light_table_event -= value;
          m_light_table_event += value;
        }
      }
      remove
      {
        lock (m_event_lock)
        {
          m_light_table_event -= value;
          if (m_light_table_event == null)
          {
            UnsafeNativeMethods.CRhinoEventWatcher_SetLightTableEventCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
            m_OnLightTableEventCallback = null;
          }
        }
      }
    }


    private static RhinoTableCallback m_OnMaterialTableEventCallback;
    private static void OnMaterialTableEvent(int docId, int event_type, int index, IntPtr pConstOldSettings)
    {
      if (m_material_table_event != null)
      {
        try
        {
          DocObjects.Tables.MaterialTableEventArgs args = new DocObjects.Tables.MaterialTableEventArgs(docId, event_type, index, pConstOldSettings);
          m_material_table_event(null, args);
          args.Done();
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    internal static EventHandler<DocObjects.Tables.MaterialTableEventArgs> m_material_table_event;

    /// <summary>
    /// Called when any modification happens to a document's material table.
    /// </summary>
    public static event EventHandler<Rhino.DocObjects.Tables.MaterialTableEventArgs> MaterialTableEvent
    {
      add
      {
        lock (m_event_lock)
        {
          if (m_material_table_event == null)
          {
            m_OnMaterialTableEventCallback = OnMaterialTableEvent;
            UnsafeNativeMethods.CRhinoEventWatcher_SetMaterialTableEventCallback(m_OnMaterialTableEventCallback, Rhino.Runtime.HostUtils.m_ew_report);
          }
          m_material_table_event -= value;
          m_material_table_event += value;
        }
      }
      remove
      {
        lock (m_event_lock)
        {
          m_material_table_event -= value;
          if (m_material_table_event == null)
          {
            UnsafeNativeMethods.CRhinoEventWatcher_SetMaterialTableEventCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
            m_OnMaterialTableEventCallback = null;
          }
        }
      }
    }


    private static RhinoTableCallback m_OnGroupTableEventCallback;
    private static void OnGroupTableEvent(int docId, int event_type, int index, IntPtr pConstOldSettings)
    {
      if (m_group_table_event != null)
      {
        try
        {
          DocObjects.Tables.GroupTableEventArgs args = new DocObjects.Tables.GroupTableEventArgs(docId, event_type);
          m_group_table_event(null, args);
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    internal static EventHandler<DocObjects.Tables.GroupTableEventArgs> m_group_table_event;

    /// <summary>
    /// Called when any modification happens to a document's group table.
    /// </summary>
    public static event EventHandler<Rhino.DocObjects.Tables.GroupTableEventArgs> GroupTableEvent
    {
      add
      {
        lock (m_event_lock)
        {
          if (m_group_table_event == null)
          {
            m_OnGroupTableEventCallback = OnGroupTableEvent;
            UnsafeNativeMethods.CRhinoEventWatcher_SetGroupTableEventCallback(m_OnGroupTableEventCallback, Rhino.Runtime.HostUtils.m_ew_report);
          }
          m_group_table_event -= value;
          m_group_table_event += value;
        }
      }
      remove
      {
        lock (m_event_lock)
        {
          m_group_table_event -= value;
          if (m_group_table_event == null)
          {
            UnsafeNativeMethods.CRhinoEventWatcher_SetGroupTableEventCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
            m_OnGroupTableEventCallback = null;
          }
        }
      }
    }

    #endregion

#if RDK_CHECKED
    #region RenderContentTable events
    /// <summary>
    /// Type of content table event
    /// </summary>
    public enum RenderContentTableEventType
    {
      /// <summary>
      /// The document has been read and the table has been loaded
      /// </summary>
      Loaded,
      /// <summary>
      /// The table is about to be cleared
      /// </summary>
      Clearing,
      /// <summary>
      /// The table has been cleared
      /// </summary>
      Cleared
    }
    /// <summary>
    /// Passed to the <see cref="RenderMaterialsTableEvent"/>, <see cref="RenderEnvironmentTableEvent"/> and the
    /// <see cref="RenderTextureTableEvent"/> events.
    /// </summary>
    public class RenderContentTableEventArgs : EventArgs
    {
      internal RenderContentTableEventArgs(RhinoDoc document, RenderContentTableEventType eventType)
      {
        m_rhino_doc = document;
        m_event_type = eventType;
      }

      /// <summary>
      /// Document the table belongs to
      /// </summary>
      public RhinoDoc Document { get { return m_rhino_doc; } }
      /// <summary>
      /// Event type
      /// </summary>
      public RenderContentTableEventType EventType { get { return m_event_type; } }

      private readonly RhinoDoc m_rhino_doc;
      private readonly RenderContentTableEventType m_event_type;
    }
    private static RenderContentTable.ContentListLoadedCallback g_on_render_content_loaded_event_callback;
    private static void OnRenderContentdLoadedEvent(int kind, int docId)
    {
      var document = FromId(docId);
      switch ((RenderContentKind)kind)
      {
        case RenderContentKind.Material:
          OnRenderMaterialTabledEvent(document, RenderContentTableEventType.Loaded);
          break;
        case RenderContentKind.Environment:
          OnRenderEnvironmentTabledEvent(document, RenderContentTableEventType.Loaded);
          break;
        case RenderContentKind.Texture:
          OnRenderTextureTabledEvent(document, RenderContentTableEventType.Loaded);
          break;
      }
    }

    private static RenderContentTable.ContentListClearingCallback g_on_render_content_clearing_event_callback;
    private static void OnRenderContentdClearingEvent(int kind, int docId)
    {
      var document = FromId(docId);
      switch ((RenderContentKind)kind)
      {
        case RenderContentKind.Material:
          OnRenderMaterialTabledEvent(document, RenderContentTableEventType.Clearing);
          break;
        case RenderContentKind.Environment:
          OnRenderEnvironmentTabledEvent(document, RenderContentTableEventType.Clearing);
          break;
        case RenderContentKind.Texture:
          OnRenderTextureTabledEvent(document, RenderContentTableEventType.Clearing);
          break;
      }
    }

    private static RenderContentTable.ContentListClearedCallback g_on_render_content_cleared_event_callback;
    private static void OnRenderContentdClearedEvent(int kind, int docId)
    {
      var document = FromId(docId);
      switch ((RenderContentKind)kind)
      {
        case RenderContentKind.Material:
          OnRenderMaterialTabledEvent(document, RenderContentTableEventType.Cleared);
          break;
        case RenderContentKind.Environment:
          OnRenderEnvironmentTabledEvent(document, RenderContentTableEventType.Cleared);
          break;
        case RenderContentKind.Texture:
          OnRenderTextureTabledEvent(document, RenderContentTableEventType.Cleared);
          break;
      }
    }
    #endregion RenderContentTable events

    #region RenderMaterialsTable events
    private static void OnRenderMaterialTabledEvent(RhinoDoc document, RenderContentTableEventType eventType)
    {
      if (g_render_materials_table_event == null)
        return;
      try
      {
        var args = new RenderContentTableEventArgs(document, eventType);
        g_render_materials_table_event(null == document ? null : document.RenderMaterials, args);
      }
      catch (Exception ex)
      {
        Runtime.HostUtils.ExceptionReport(ex);
      }
    }

    private static event EventHandler<RenderContentTableEventArgs> g_render_materials_table_event;
    /// Called when the <see cref="RenderMaterialTable"/> has been loaded, is
    /// about to be cleared or has been cleared.  See <see cref="RenderContentTableEventType"/> for more
    /// information.
    public static event EventHandler<RenderContentTableEventArgs> RenderMaterialsTableEvent
    {
      add
      {
        lock (m_event_lock)
        {
          if (g_on_render_content_loaded_event_callback == null)
          {
            g_on_render_content_loaded_event_callback = OnRenderContentdLoadedEvent;
            UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListLoadedEventCallback(g_on_render_content_loaded_event_callback, Runtime.HostUtils.m_rdk_ew_report);
          }
          if (g_on_render_content_clearing_event_callback == null)
          {
            g_on_render_content_clearing_event_callback = OnRenderContentdClearingEvent;
            UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListClearingEventCallback(g_on_render_content_clearing_event_callback, Runtime.HostUtils.m_rdk_ew_report);
          }
          if (g_on_render_content_cleared_event_callback == null)
          {
            g_on_render_content_cleared_event_callback = OnRenderContentdClearedEvent;
            UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListClearedEventCallback(g_on_render_content_cleared_event_callback, Runtime.HostUtils.m_rdk_ew_report);
          }
          g_render_materials_table_event -= value;
          g_render_materials_table_event += value;
        }
      }
      remove
      {
        lock (m_event_lock)
        {
          g_render_materials_table_event -= value;
          if (g_render_materials_table_event == null && g_render_environment_table_event == null && g_render_texture_table_event == null)
          {
            UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListLoadedEventCallback(null, Runtime.HostUtils.m_rdk_ew_report);
            UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListClearingEventCallback(null, Runtime.HostUtils.m_rdk_ew_report);
            UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListClearedEventCallback(null, Runtime.HostUtils.m_rdk_ew_report);
            g_on_render_content_loaded_event_callback = null;
            g_on_render_content_clearing_event_callback = null;
            g_on_render_content_cleared_event_callback = null;
          }
        }
      }
    }
    #endregion RenderMaterialsTable events

    #region RenderEnvironmentsTable events

    private static void OnRenderEnvironmentTabledEvent(RhinoDoc document, RenderContentTableEventType eventType)
    {
      if (g_render_environment_table_event == null)
        return;
      try
      {
        var args = new RenderContentTableEventArgs(document, eventType);
        g_render_environment_table_event(null == document ? null : document.RenderEnvironments, args);
      }
      catch (Exception ex)
      {
        Runtime.HostUtils.ExceptionReport(ex);
      }
    }

    private static event EventHandler<RenderContentTableEventArgs> g_render_environment_table_event;
    /// Called when the <see cref="RenderEnvironmentTable"/> has been loaded, is
    /// about to be cleared or has been cleared.  See <see cref="RenderContentTableEventType"/> for more
    /// information.
    public static event EventHandler<RenderContentTableEventArgs> RenderEnvironmentTableEvent
    {
      add
      {
        lock (m_event_lock)
        {
          if (g_on_render_content_loaded_event_callback == null)
          {
            g_on_render_content_loaded_event_callback = OnRenderContentdLoadedEvent;
            UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListLoadedEventCallback(g_on_render_content_loaded_event_callback, Runtime.HostUtils.m_rdk_ew_report);
          }
          if (g_on_render_content_clearing_event_callback == null)
          {
            g_on_render_content_clearing_event_callback = OnRenderContentdClearingEvent;
            UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListClearingEventCallback(g_on_render_content_clearing_event_callback, Runtime.HostUtils.m_rdk_ew_report);
          }
          if (g_on_render_content_cleared_event_callback == null)
          {
            g_on_render_content_cleared_event_callback = OnRenderContentdClearedEvent;
            UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListClearedEventCallback(g_on_render_content_cleared_event_callback, Runtime.HostUtils.m_rdk_ew_report);
          }
          g_render_environment_table_event -= value;
          g_render_environment_table_event += value;
        }
      }
      remove
      {
        lock (m_event_lock)
        {
          g_render_environment_table_event -= value;
          if (g_render_materials_table_event == null && g_render_environment_table_event == null && g_render_texture_table_event == null)
          {
            UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListLoadedEventCallback(null, Runtime.HostUtils.m_rdk_ew_report);
            UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListClearingEventCallback(null, Runtime.HostUtils.m_rdk_ew_report);
            UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListClearedEventCallback(null, Runtime.HostUtils.m_rdk_ew_report);
            g_on_render_content_loaded_event_callback = null;
            g_on_render_content_clearing_event_callback = null;
            g_on_render_content_cleared_event_callback = null;
          }
        }
      }
    }

    #endregion RenderEnvironmentsTable events

    #region RenderTexturesTable events

    private static void OnRenderTextureTabledEvent(RhinoDoc document, RenderContentTableEventType eventType)
    {
      if (g_render_texture_table_event == null)
        return;
      try
      {
        var args = new RenderContentTableEventArgs(document, eventType);
        g_render_texture_table_event(null == document ? null : document.RenderTextures, args);
      }
      catch (Exception ex)
      {
        Runtime.HostUtils.ExceptionReport(ex);
      }
    }

    private static event EventHandler<RenderContentTableEventArgs> g_render_texture_table_event;
    /// <summary>
    /// Called when the <see cref="RenderTextureTable"/> has been loaded, is
    /// about to be cleared or has been cleared.  See <see cref="RenderContentTableEventType"/> for more
    /// information.
    /// </summary>
    public static event EventHandler<RenderContentTableEventArgs> RenderTextureTableEvent
    {
      add
      {
        lock (m_event_lock)
        {
          if (g_on_render_content_loaded_event_callback == null)
          {
            g_on_render_content_loaded_event_callback = OnRenderContentdLoadedEvent;
            UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListLoadedEventCallback(g_on_render_content_loaded_event_callback, Runtime.HostUtils.m_rdk_ew_report);
          }
          if (g_on_render_content_clearing_event_callback == null)
          {
            g_on_render_content_clearing_event_callback = OnRenderContentdClearingEvent;
            UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListClearingEventCallback(g_on_render_content_clearing_event_callback, Runtime.HostUtils.m_rdk_ew_report);
          }
          if (g_on_render_content_cleared_event_callback == null)
          {
            g_on_render_content_cleared_event_callback = OnRenderContentdClearedEvent;
            UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListClearedEventCallback(g_on_render_content_cleared_event_callback, Runtime.HostUtils.m_rdk_ew_report);
          }
          g_render_texture_table_event -= value;
          g_render_texture_table_event += value;
        }
      }
      remove
      {
        lock (m_event_lock)
        {
          g_render_texture_table_event -= value;
          if (g_render_materials_table_event == null && g_render_environment_table_event == null && g_render_texture_table_event == null)
          {
            UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListLoadedEventCallback(null, Runtime.HostUtils.m_rdk_ew_report);
            UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListClearingEventCallback(null, Runtime.HostUtils.m_rdk_ew_report);
            UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListClearedEventCallback(null, Runtime.HostUtils.m_rdk_ew_report);
            g_on_render_content_loaded_event_callback = null;
            g_on_render_content_clearing_event_callback = null;
            g_on_render_content_cleared_event_callback = null;
          }
        }
      }
    }


    #endregion RenderTexturesTable events
#endif
    public enum TextureMappingEventType : int
    {
      /// <summary>
      /// Adding texture mapping to a document
      /// </summary>
      Added = UnsafeNativeMethods.RhinoEventWatcherTextureMappingEventConsts.Added,
      /// <summary>
      /// A texture mapping was deleted from a document
      /// </summary>
      Deleted = UnsafeNativeMethods.RhinoEventWatcherTextureMappingEventConsts.Deleted,
      /// <summary>
      /// A texture mapping was undeleted in a document
      /// </summary>
      Undeleted = UnsafeNativeMethods.RhinoEventWatcherTextureMappingEventConsts.Undeleted,
      /// <summary>
      /// A texture mapping was modified in a document
      /// </summary>
      Modified = UnsafeNativeMethods.RhinoEventWatcherTextureMappingEventConsts.Modified,
      //Sorted = UnsafeNativeMethods.RhinoEventWatcherTextureMappingEventConsts.Sorted,
      //Current = UnsafeNativeMethods.RhinoEventWatcherTextureMappingEventConsts.Current,
    }
    /// <summary>
    /// Event arguments passed to the RhinoDoc.TextureMappingEvent.
    /// </summary>
    public class TextureMappingEventArgs : EventArgs
    {
      readonly int m_doc_id;
      readonly TextureMappingEventType m_event_type = TextureMappingEventType.Modified;
      readonly IntPtr m_const_pointer_new_mapping;
      readonly IntPtr m_const_pointer_old_mapping;

      internal TextureMappingEventArgs(int docId, UnsafeNativeMethods.RhinoEventWatcherTextureMappingEventConsts eventType, IntPtr constPointerConstNewMapping, IntPtr pConstOldMapping)
      {
        m_doc_id = docId;
        switch (eventType)
        {
          case UnsafeNativeMethods.RhinoEventWatcherTextureMappingEventConsts.Added:
          case UnsafeNativeMethods.RhinoEventWatcherTextureMappingEventConsts.Deleted:
          case UnsafeNativeMethods.RhinoEventWatcherTextureMappingEventConsts.Modified:
          case UnsafeNativeMethods.RhinoEventWatcherTextureMappingEventConsts.Undeleted:
            m_event_type = (TextureMappingEventType)eventType;
            break;
          case UnsafeNativeMethods.RhinoEventWatcherTextureMappingEventConsts.Current:
          case UnsafeNativeMethods.RhinoEventWatcherTextureMappingEventConsts.Sorted:
            break;
        }
        m_const_pointer_new_mapping = constPointerConstNewMapping;
        m_const_pointer_old_mapping = m_const_pointer_new_mapping;
      }

      RhinoDoc m_doc;
      public RhinoDoc Document
      {
        get { return m_doc ?? (m_doc = RhinoDoc.FromId(m_doc_id)); }
      }

      public TextureMappingEventType EventType
      {
        get { return m_event_type; }
      }

      public TextureMapping OldMapping
      {
        get { return (m_old_mapping ?? (m_old_mapping = new TextureMapping(m_const_pointer_old_mapping))); }
      }
      private TextureMapping m_old_mapping;

      public TextureMapping NewMapping
      {
        get { return (m_new_mapping ?? (m_new_mapping = new TextureMapping(m_const_pointer_new_mapping))); }
      }
      TextureMapping m_new_mapping;
    }
    /// <summary>
    /// Called when any modification happens to a document objects texture mapping.
    /// </summary>
    public static event EventHandler<TextureMappingEventArgs> TextureMappingEvent
    {
      add
      {
        lock (m_event_lock)
        {
          if (g_texture_mapping_event == null)
          {
            m_OnTextureMappingEventCallback = OnTextureMappingEvent;
            UnsafeNativeMethods.CRhinoEventWatcher_SetTextureMappingEventCallback(m_OnTextureMappingEventCallback, Rhino.Runtime.HostUtils.m_ew_report);
          }
          g_texture_mapping_event -= value;
          g_texture_mapping_event += value;
        }
      }
      remove
      {
        lock (m_event_lock)
        {
          g_texture_mapping_event -= value;
          if (g_texture_mapping_event == null)
          {
            UnsafeNativeMethods.CRhinoEventWatcher_SetTextureMappingEventCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
            g_texture_mapping_event = null;
          }
        }
      }
    }
    internal delegate void TextureMappingEventCallback(int docId, UnsafeNativeMethods.RhinoEventWatcherTextureMappingEventConsts eventType, IntPtr pConstNewSettings, IntPtr pConstOldSettings);
    private static TextureMappingEventCallback m_OnTextureMappingEventCallback;
    private static void OnTextureMappingEvent(int docId, UnsafeNativeMethods.RhinoEventWatcherTextureMappingEventConsts eventType, IntPtr pConstNewSettings, IntPtr pConstOldSettings)
    {
      if (g_texture_mapping_event != null)
      {
        try
        {
          switch (eventType)
          {
            case UnsafeNativeMethods.RhinoEventWatcherTextureMappingEventConsts.Added:
            case UnsafeNativeMethods.RhinoEventWatcherTextureMappingEventConsts.Deleted:
            case UnsafeNativeMethods.RhinoEventWatcherTextureMappingEventConsts.Modified:
            case UnsafeNativeMethods.RhinoEventWatcherTextureMappingEventConsts.Undeleted:
              break;
            case UnsafeNativeMethods.RhinoEventWatcherTextureMappingEventConsts.Current:
            case UnsafeNativeMethods.RhinoEventWatcherTextureMappingEventConsts.Sorted:
              return; // Ignore these for now
          }
          var args = new TextureMappingEventArgs(docId, eventType, pConstNewSettings, pConstOldSettings);
          g_texture_mapping_event(null, args);
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    private static EventHandler<TextureMappingEventArgs> g_texture_mapping_event;


    #region rdk events
#if RDK_UNCHECKED
    // Notes from John and Steve: 27 August 2013
    // 1) Never use the work RDK, what should these be called?
    // 2) Do we need these or should there be separate events for each case
    // 3) Don't write nested enum or classes that are public
    // 4) What should the public event(s) be?
    // 5) Where is the associated document
    // 6) Event args should derive from common DocumentEventArgs class
    /// <summary>
    /// Bit flags for RdkDocumentSettingsChangedArgs flags parameter.
    /// </summary>
    [Flags]
	  public enum DocSettingsChangedFlags
	  {
		  Rendering         = 0x0001, // Rendering settings changed (see enum 2 below).
		  SafeFrame         = 0x0002, // Safe frame settings changed.
		  DocumentSun       = 0x0004, // Document sun settings changed.
		  PostEffects       = 0x0008, // Post effects settings changed.
		  GroundPlane       = 0x0010, // Ground plane settings changed.
		  ContentFilter     = 0x0020, // Content filter (excluded render engines) changed.
		  CustomRenderMesh  = 0x0040, // Custom render mesh settings changed.
		  Unspecified       = 0x8000, // Unspecified settings changed. For future use.
		  All               = 0xFFFF, // All RDK document settings changed.
	  };

	  /// <summary>
	  /// Values for RdkDocumentSettingsChangedArgs RenderingInfo parameter when Flags is 'Rendering'
	  /// </summary>
	  public enum DocSettingsChangedRenderingInfo
	  {
		  SaveSupportFiles    = 1, // Save support files in 3dm file checkbox changed.
		  Dithering           = 2, // Dithering method changed.
		  Gamma               = 3, // Gamma value changed.
		  UseLinearWorkflow   = 4, // Use linear workflow checkbox changed.
		  ToneMapping         = 5, // Tone mapping method changed.
		  ToneMapperParams    = 6, // Tone mapper parameter(s) changed.
	  };

    public class RdkDocumentSettingsChangedArgs : EventArgs
    {
      readonly DocSettingsChangedFlags m_flags;
      readonly DocSettingsChangedRenderingInfo m_context;
      internal RdkDocumentSettingsChangedArgs(DocSettingsChangedFlags flags, DocSettingsChangedRenderingInfo context) 
      {
        m_flags = flags;
        m_context = context; 
      }
      public DocSettingsChangedFlags         Flags { get { return m_flags; } }
      public DocSettingsChangedRenderingInfo RenderingInfo { get { return m_context; } }
    }

    internal delegate void RdkDocumentSettingsChangedCallback(int flags, int context);

    private static RdkDocumentSettingsChangedCallback m_OnRdkDocumentSettingsChanged;
    private static void OnRdkDocumentSettingsChanged(int flags, int context)
    {
      if (m_rdk_doc_settings_changed_event != null)
      {
        try { m_rdk_doc_settings_changed_event(null, new RdkDocumentSettingsChangedArgs((DocSettingsChangedFlags)flags, (DocSettingsChangedRenderingInfo)context)); }
        catch (Exception ex) { Runtime.HostUtils.ExceptionReport(ex); }
      }
    }
    internal static EventHandler<RdkDocumentSettingsChangedArgs> m_rdk_doc_settings_changed_event;


    /// <summary>
    /// Called when RDK document settings are changed.
    /// </summary>
    public static event EventHandler<RdkDocumentSettingsChangedArgs> RdkSettingsChanged
    {
      add
      {
        if (m_rdk_doc_settings_changed_event == null)
        {
          m_OnRdkDocumentSettingsChanged = OnRdkDocumentSettingsChanged;
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetDocumentSettingsChangedEventCallback(m_OnRdkDocumentSettingsChanged, Rhino.Runtime.HostUtils.m_rdk_ew_report);
        }
        m_rdk_doc_settings_changed_event += value;
      }
      remove
      {
        m_rdk_doc_settings_changed_event -= value;
        if (m_rdk_doc_settings_changed_event == null)
        {
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetDocumentSettingsChangedEventCallback(null, Rhino.Runtime.HostUtils.m_rdk_ew_report);
          m_OnRdkDocumentSettingsChanged = null;
        }
      }
    }

#endif
    #endregion
  }

  /// <summary>
  /// Provides document information for RhinoDoc events.
  /// </summary>
  public class DocumentEventArgs : EventArgs
  {
    private readonly int m_docId;
    private RhinoDoc m_doc;
    internal DocumentEventArgs(int docId)
    {
      m_docId = docId;
    }

    /// <summary>
    /// Gets the document Id of the document for this event.
    /// </summary>
    public int DocumentId
    {
      get { return m_docId;}
    }

    /// <summary>
    /// Gets the document for this event. This field might be null.
    /// </summary>
    public RhinoDoc Document
    {
      get { return m_doc ?? (m_doc = RhinoDoc.FromId(m_docId)); }
    }
  }

  /// <summary>
  /// Provides document information for RhinoDoc events.
  /// </summary>
  public class DocumentOpenEventArgs : DocumentEventArgs
  {
    readonly IntPtr m_filename;
    string m_cachedFileName;
    readonly bool m_bMerge;
    readonly bool m_bReference;
    internal DocumentOpenEventArgs(int docId, IntPtr filename, bool merge, bool reference)
      : base(docId)
    {
      m_filename = filename;
      m_bMerge = merge;
      m_bReference = reference;
    }

    /// <summary>
    /// Name of file being opened.
    /// </summary>
    public string FileName
    {
      get
      {
        if (null == m_cachedFileName)
        {
          if (IntPtr.Zero != m_filename)
            m_cachedFileName = Marshal.PtrToStringUni(m_filename);
        }
        return m_cachedFileName;
      }
    }

    /// <summary>
    /// true if file is being merged into the current document. This
    /// occurs during the "Import" command.
    /// </summary>
    public bool Merge
    {
      get { return m_bMerge; }
    }

    /// <summary>
    /// true is file is openend as a reference file.
    /// </summary>
    public bool Reference
    {
      get { return m_bReference; }
    }
  }

  /// <summary>
  /// Provides document information for RhinoDoc events.
  /// </summary>
  public class DocumentSaveEventArgs : DocumentEventArgs
  {
    readonly IntPtr m_filename;
    string m_cachedFileName;
    readonly bool m_bExportSelected;
    internal DocumentSaveEventArgs(int docId, IntPtr filename, bool exportSelected)
      : base(docId)
    {
      m_filename = filename;
      m_bExportSelected = exportSelected;
    }

    /// <summary>
    /// Name of file being written.
    /// </summary>
    public string FileName
    {
      get
      {
        if (null == m_cachedFileName)
        {
          if (IntPtr.Zero != m_filename)
            m_cachedFileName = Marshal.PtrToStringUni(m_filename);
        }
        return m_cachedFileName;
      }
    }

    /// <summary>
    /// true if only selected objects are being written to a file.
    /// </summary>
    public bool ExportSelected
    {
      get { return m_bExportSelected; }
    }
  }

  namespace DocObjects
  {
    public class RhinoObjectEventArgs : EventArgs
    {
      private readonly IntPtr m_pRhinoObject;
      private RhinoObject m_rhino_object;
      private Guid m_ObjectID = Guid.Empty;

      internal RhinoObjectEventArgs(IntPtr pDoc, IntPtr pRhinoObject)
      {
        m_pRhinoObject = pRhinoObject;
      }

      public Guid ObjectId
      {
        get
        {
          if (m_ObjectID == Guid.Empty)
          {
            m_ObjectID = UnsafeNativeMethods.CRhinoObject_Id(m_pRhinoObject);
          }
          return m_ObjectID;
        }
      }

      public RhinoObject TheObject
      {
        get
        {
          if (null == m_rhino_object || m_rhino_object.ConstPointer() != m_pRhinoObject)
          {
            m_rhino_object = RhinoObject.CreateRhinoObjectHelper(m_pRhinoObject);
          }
          return m_rhino_object;
        }
      }
    }

    public class RhinoObjectSelectionEventArgs : EventArgs
    {
      private readonly bool m_select;
      private readonly int m_docId;
      private readonly IntPtr m_pRhinoObject;
      private readonly IntPtr m_pRhinoObjectList;

      internal RhinoObjectSelectionEventArgs(bool select, int docId, IntPtr pRhinoObject, IntPtr pRhinoObjects)
      {
        m_select = select;
        m_docId = docId;
        m_pRhinoObject = pRhinoObject;
        m_pRhinoObjectList = pRhinoObjects;
      }

      /// <summary>
      /// Returns true if objects are being selected.
      /// Returns false if objects are being deseleced.
      /// </summary>
      public bool Selected
      {
        get { return m_select; }
      }

      RhinoDoc m_doc;
      public RhinoDoc Document
      {
        get { return m_doc ?? (m_doc = RhinoDoc.FromId(m_docId)); }
      }

      List<RhinoObject> m_objects;
      public RhinoObject[] RhinoObjects
      {
        get
        {
          if (m_objects == null)
          {
            m_objects = new List<RhinoObject>();
            if (m_pRhinoObject != IntPtr.Zero)
            {
              RhinoObject rhobj = RhinoObject.CreateRhinoObjectHelper(m_pRhinoObject);
              if (rhobj != null)
                m_objects.Add(rhobj);
            }
            if (m_pRhinoObjectList != IntPtr.Zero)
            {
              RhinoObject[] rhobjs = Rhino.Runtime.INTERNAL_RhinoObjectArray.ToArrayFromPointer(m_pRhinoObjectList);
              m_objects.AddRange(rhobjs);
            }
          }
          return m_objects.ToArray();
        }
      }
    }

    public class RhinoReplaceObjectEventArgs : EventArgs
    {
      private readonly IntPtr m_pOldRhinoObject;
      private RhinoObject m_old_rhino_object;

      private readonly IntPtr m_pNewRhinoObject;
      private RhinoObject m_new_rhino_object;
      private Guid m_ObjectId = Guid.Empty;
      private readonly IntPtr m_pDoc;

      internal RhinoReplaceObjectEventArgs(IntPtr pDoc, IntPtr pOldRhinoObject, IntPtr pNewRhinoObject)
      {
        m_pDoc = pDoc;
        m_pOldRhinoObject = pOldRhinoObject;
        m_pNewRhinoObject = pNewRhinoObject;
      }

      public Guid ObjectId
      {
        get
        {
          if (m_ObjectId == Guid.Empty)
          {
            m_ObjectId = UnsafeNativeMethods.CRhinoObject_Id(m_pOldRhinoObject);
          }
          return m_ObjectId;
        }
      }

      public RhinoObject OldRhinoObject
      {
        get
        {
          if (null == m_old_rhino_object || m_old_rhino_object.ConstPointer() != m_pOldRhinoObject)
          {
            m_old_rhino_object = RhinoObject.CreateRhinoObjectHelper(m_pOldRhinoObject);
            if( m_old_rhino_object!=null )
              m_old_rhino_object.m_pRhinoObject = m_pOldRhinoObject;
          }
          return m_old_rhino_object;
        }
      }

      public RhinoObject NewRhinoObject
      {
        get
        {
          if (null == m_new_rhino_object || m_new_rhino_object.ConstPointer() != m_pNewRhinoObject)
          {
            m_new_rhino_object = RhinoObject.CreateRhinoObjectHelper(m_pNewRhinoObject);
            // Have to explicitly set the pointer since the object is not "officially"
            // in the document yet (can't find it by runtime serial number)
            if (m_new_rhino_object != null)
              m_new_rhino_object.m_pRhinoObject = m_pNewRhinoObject;
          }
          return m_new_rhino_object;
        }
      }

      public RhinoDoc Document
      {
        get
        {
          return RhinoDoc.FromIntPtr(m_pDoc);
        }
      }
    }

    public class RhinoDeselectAllObjectsEventArgs : EventArgs
    {
      private readonly int m_docId;
      private readonly int m_object_count;

      internal RhinoDeselectAllObjectsEventArgs(int docId, int count)
      {
        m_docId = docId;
        m_object_count = count;
      }

      public int ObjectCount
      {
        get { return m_object_count; }
      }

      RhinoDoc m_doc;
      public RhinoDoc Document
      {
        get { return m_doc ?? (m_doc = RhinoDoc.FromId(m_docId)); }
      }
    }


    public class RhinoModifyObjectAttributesEventArgs : EventArgs
    {
      private readonly int m_docId;
      private readonly IntPtr m_pRhinoObject;
      private readonly IntPtr m_pOldObjectAttributes;

      internal RhinoModifyObjectAttributesEventArgs(int docId, IntPtr pRhinoObject, IntPtr pOldObjectAttributes)
      {
        m_docId = docId;
        m_pRhinoObject = pRhinoObject;
        m_pOldObjectAttributes = pOldObjectAttributes;
      }

      RhinoDoc m_doc;
      public RhinoDoc Document
      {
        get { return m_doc ?? (m_doc = RhinoDoc.FromId(m_docId)); }
      }

      RhinoObject m_object;
      public RhinoObject RhinoObject
      {
        get { return m_object ?? (m_object = RhinoObject.CreateRhinoObjectHelper(m_pRhinoObject)); }
      }

      ObjectAttributes m_old_attributes;
      public ObjectAttributes OldAttributes
      {
        get
        {
          if( m_old_attributes==null )
          {
            m_old_attributes = new ObjectAttributes(m_pOldObjectAttributes);
            m_old_attributes.DoNotDestructOnDispose();
          }
          return m_old_attributes;
        }
      }

      public ObjectAttributes NewAttributes
      {
        get
        {
          return RhinoObject.Attributes;
        }
      }
    }
  }
}


namespace Rhino.DocObjects.Tables
{
  public sealed class ViewTable : IEnumerable<RhinoView>
  {
    private readonly RhinoDoc m_doc;
    internal ViewTable(RhinoDoc doc)
    {
      m_doc = doc;
    }

    /// <summary>Document that owns this object table.</summary>
    public RhinoDoc Document
    {
      get { return m_doc; }
    }

    /// <summary>
    /// Gets or Sets the active view.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_addbackgroundbitmap.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addbackgroundbitmap.cs' lang='cs'/>
    /// <code source='examples\py\ex_addbackgroundbitmap.py' lang='py'/>
    /// </example>
    public Rhino.Display.RhinoView ActiveView
    {
      get
      {
        IntPtr ptr = UnsafeNativeMethods.CRhinoDoc_ActiveView();
        if (ptr == IntPtr.Zero)
          return null;
        return RhinoView.FromIntPtr(ptr);
      }
      set
      {
        IntPtr ptr = value.NonConstPointer();
        UnsafeNativeMethods.CRhinoDoc_SetActiveView(ptr);
      }
    }

    /// <summary>
    /// Cause objects selection state to change momentarily so the object
    /// appears to flash on the screen.
    /// </summary>
    /// <param name="list">An array, a list or any enumerable set of Rhino objects.</param>
    /// <param name="useSelectionColor">
    /// If true, flash between object color and selection color. If false,
    /// flash between visible and invisible.
    /// </param>
    public void FlashObjects(IEnumerable<RhinoObject> list, bool useSelectionColor)
    {
      Rhino.Runtime.INTERNAL_RhinoObjectArray rharray = new Rhino.Runtime.INTERNAL_RhinoObjectArray(list);
      IntPtr pArray = rharray.NonConstPointer();
      UnsafeNativeMethods.CRhinoDoc_FlashObjectList(m_doc.m_docId, pArray, useSelectionColor);
    }

    /// <summary>Redraws all views.</summary>
    /// <remarks>
    /// If you change something in the active document -- like adding
    /// objects, deleting objects, modifying layer or object display 
    /// attributes, etc., then you need to call CRhinoDoc::Redraw to 
    /// redraw all the views.
    ///
    /// If you change something in a particular view like the projection,
    /// construction plane, background bitmap, etc., then you need to
    /// call CRhinoView::Redraw to redraw that particular view.
    ///</remarks>
    /// <example>
    /// <code source='examples\vbnet\ex_addcircle.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addcircle.cs' lang='cs'/>
    /// <code source='examples\py\ex_addcircle.py' lang='py'/>
    /// </example>
    public void Redraw()
    {
      UnsafeNativeMethods.CRhinoDoc_Redraw(m_doc.m_docId);
    }

    /// <summary>Gets an array of all the views.</summary>
    /// <param name="includeStandardViews">true if "Right", "Perspective", etc., view should be included; false otherwise.</param>
    /// <param name="includePageViews">true if page-related views should be included; false otherwise.</param>
    /// <returns>A array of Rhino views. This array can be emptry, but not null.</returns>
    public Rhino.Display.RhinoView[] GetViewList(bool includeStandardViews, bool includePageViews)
    {
      if (!includeStandardViews && !includePageViews)
        return new RhinoView[0];

      int count = UnsafeNativeMethods.CRhinoDoc_ViewListBuild(m_doc.m_docId, includeStandardViews, includePageViews);
      if (count < 1)
        return new RhinoView[0];
      List<RhinoView> views = new List<RhinoView>(count);
      for (int i = 0; i < count; i++)
      {
        IntPtr pView = UnsafeNativeMethods.CRhinoDoc_ViewListGet(m_doc.m_docId, i);
        RhinoView view = RhinoView.FromIntPtr(pView);
        if (view != null)
          views.Add(view);
      }
      UnsafeNativeMethods.CRhinoDoc_ViewListBuild(m_doc.m_docId, false, false); // calling with false empties the static list used by ViewListGet
      return views.ToArray();
    }

    public Rhino.Display.RhinoView[] GetStandardRhinoViews()
    {
      return GetViewList(true, false);
    }
    
    /// <summary>
    /// Gets all page views in the document.
    /// </summary>
    /// <returns>An array with all page views. The return value can be an empty array but not null.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addlayout.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addlayout.cs' lang='cs'/>
    /// <code source='examples\py\ex_addlayout.py' lang='py'/>
    /// </example>
    public Rhino.Display.RhinoPageView[] GetPageViews()
    {
      RhinoView[] views = GetViewList(false, true);
      if (null == views || views.Length < 1)
        return new RhinoPageView[0];
      RhinoPageView[] pages = new RhinoPageView[views.Length];
      for (int i = 0; i < views.Length; i++)
      {
        pages[i] = views[i] as RhinoPageView;
      }
      return pages;
    }

    /// <summary>
    /// Finds a view in this document with a given main viewport Id.
    /// </summary>
    /// <param name="mainViewportId">The ID of the main viewport looked for.</param>
    /// <returns>View on success. null if the view could not be found in this document.</returns>
    public Rhino.Display.RhinoView Find(Guid mainViewportId)
    {
      IntPtr pView = UnsafeNativeMethods.CRhinoDoc_FindView(m_doc.m_docId, mainViewportId);
      return RhinoView.FromIntPtr(pView);
    }

    /// <summary>
    /// Finds a view in this document with a main viewport that has a given name. Note that there
    /// may be multiple views in this document that have the same name. This function only returns
    /// the first view found. If you want to find all the views with a given name, use the GetViewList
    /// function and iterate through the views.
    /// </summary>
    /// <param name="mainViewportName">The name of the main viewport.</param>
    /// <param name="compareCase">true if capitalization influences comparison; otherwise, false.</param>
    /// <returns>A Rhino view on success; null on error.</returns>
    public Rhino.Display.RhinoView Find(string mainViewportName, bool compareCase)
    {
      IntPtr pView = UnsafeNativeMethods.CRhinoDoc_FindView2(m_doc.m_docId, mainViewportName, compareCase);
      return RhinoView.FromIntPtr(pView);
    }

    const int idxDefaultViewLayout = 0;
    const int idxFourViewLayout = 1;
    const int idxThreeViewLayout = 2;

    public void DefaultViewLayout()
    {
      UnsafeNativeMethods.CRhinoDoc_ViewLayout(m_doc.m_docId, idxDefaultViewLayout, false);
    }
    public void FourViewLayout(bool useMatchingViews)
    {
      UnsafeNativeMethods.CRhinoDoc_ViewLayout(m_doc.m_docId, idxFourViewLayout, useMatchingViews);
    }
    public void ThreeViewLayout(bool useMatchingViews)
    {
      UnsafeNativeMethods.CRhinoDoc_ViewLayout(m_doc.m_docId, idxThreeViewLayout, useMatchingViews);
    }

    ///<summary>Returns or sets (enable or disables) screen redrawing.</summary>
    public bool RedrawEnabled
    {
      get { return m_doc.GetBool(RhinoDoc.idxRedrawEnabled); }
      set { UnsafeNativeMethods.CRhinoDoc_GetSetBool(m_doc.m_docId, RhinoDoc.idxRedrawEnabled, true, value); }
    }

    /// <summary>
    /// Constructs a new Rhino view and, at the same time, adds it to the list.
    /// </summary>
    /// <param name="title">The title of the new Rhino view.</param>
    /// <param name="projection">A basic projection type.</param>
    /// <param name="position">A position.</param>
    /// <param name="floating">true if the view floats; false if it is docked.</param>
    /// <returns>The newly constructed Rhino view; or null on error.</returns>
    public Rhino.Display.RhinoView Add(string title, DefinedViewportProjection projection, Rhino.Drawing.Rectangle position, bool floating)
    {
      IntPtr pView = UnsafeNativeMethods.CRhinoView_Create(m_doc.m_docId, position.Left, position.Top, position.Right, position.Bottom, floating);
      Rhino.Display.RhinoView rc = RhinoView.FromIntPtr(pView);
      if (rc != null)
      {
        rc.MainViewport.SetCameraLocations(Point3d.Origin, rc.MainViewport.CameraLocation);
        rc.MainViewport.SetProjection(projection, title, true);
        rc.MainViewport.ZoomExtents();
      }
      return rc;
    }

    /// <summary>
    /// Constructs a new page view with a given title and, at the same time, adds it to the list.
    /// </summary>
    /// <param name="title">
    /// If null or empty, a name will be generated as "Page #" where # is the largest page number.
    /// </param>
    /// <returns>The newly created page view on success; or null on error.</returns>
    public Rhino.Display.RhinoPageView AddPageView(string title)
    {
      IntPtr pPageView = UnsafeNativeMethods.CRhinoPageView_CreateView(title, 0, 0);
      bool isPageView = false;
      Guid id = UnsafeNativeMethods.CRhinoView_Details(pPageView, ref isPageView);
      if (isPageView && id != Guid.Empty)
        return new RhinoPageView(pPageView, id);
      return null;
    }

    /// <summary>
    /// Constructs a new page view with a given title and size and, at the same time, adds it to the list.
    /// </summary>
    /// <param name="title">
    /// If null or empty, a name will be generated as "Page #" where # is the largest page number.
    /// </param>
    /// <param name="pageWidth">The page total width.</param>
    /// <param name="pageHeight">The page total height.</param>
    /// <returns>The newly created page view on success; or null on error.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addlayout.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addlayout.cs' lang='cs'/>
    /// <code source='examples\py\ex_addlayout.py' lang='py'/>
    /// </example>
    public Rhino.Display.RhinoPageView AddPageView(string title, double pageWidth, double pageHeight)
    {
      IntPtr pPageView = UnsafeNativeMethods.CRhinoPageView_CreateView(title, pageWidth, pageHeight);
      bool isPageView = false;
      Guid id = UnsafeNativeMethods.CRhinoView_Details(pPageView, ref isPageView);
      if (isPageView && id != Guid.Empty)
        return new RhinoPageView(pPageView, id);
      return null;
    }

    public bool ModelSpaceIsActive
    {
      get
      {
        return !(ActiveView is RhinoPageView);
      }
    }
#region IEnumerable<RhinoView> Members

    public IEnumerator<RhinoView> GetEnumerator()
    {
      RhinoView[] views = GetViewList(true, true);
      List<RhinoView> _views = new List<RhinoView>(views);
      return _views.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      RhinoView[] views = GetViewList(true, true);
      List<RhinoView> _views = new List<RhinoView>(views);
      return _views.GetEnumerator();
    }
    #endregion
  }

  public sealed class ObjectTable : IEnumerable<Rhino.DocObjects.RhinoObject>
  {
    private readonly RhinoDoc m_doc;
    internal ObjectTable(RhinoDoc doc)
    {
      m_doc = doc;
    }

    /// <summary>
    /// Gets the document that owns this object table.
    /// </summary>
    public RhinoDoc Document
    {
      get { return m_doc; }
    }

    /// <summary>
    /// Uses the object guid to find a rhino object. Deleted objects cannot be found by id.
    /// The guid is the value that is stored on RhinoObject.Id
    /// In a single document, no two active objects have the same guid. If an object is
    /// replaced with a new object, then the guid  persists. For example, if the _Move command
    /// moves an object, then the moved object inherits it's guid from the starting object.
    /// If the Copy command copies an object, then the copy gets a new guid. This guid persists
    /// through file saving/openning operations. This function will not find grip objects.
    /// </summary>
    /// <param name="objectId">ID of object to search for.</param>
    /// <returns>Reference to the rhino object with the objectId or null if no such object could be found.</returns>
    public DocObjects.RhinoObject Find(Guid objectId)
    {
      IntPtr ptr = UnsafeNativeMethods.CRhinoDoc_LookupObject(m_doc.m_docId, objectId);
      return DocObjects.RhinoObject.CreateRhinoObjectHelper(ptr);
    }

    //[skipping]
    //  const ON_Object* LookupDocumentObject( ON_UUID id, bool bIgnoreDeleted ) const;

    /// <summary>
    /// Use the object runtime serial number to find a rhino object in the document. This is the value stored on
    /// RhinoObject.RuntimeObjectSerialNumber. The RhinoObject constructor sets the runtime serial number and every
    /// instance of a RhinoObject class will have a unique serial number for the duration of the Rhino application.
    /// If an object is replaced with a new object, then the new object will have a different runtime serial number.
    /// Deleted objects stored in the undo list maintain their runtime serial numbers and this funtion will return
    /// pointers to these objects. Call RhinoObject.IsDeleted if you need to determine if the returned object is
    /// active or deleted.  The runtime serial number is not saved in files.
    /// </summary>
    /// <param name="runtimeSerialNumber">Runtime serial number to search for.</param>
    /// <returns>Reference to the rhino object with the objectId or null if no such object could be found.</returns>
    [CLSCompliant(false)]
    public Rhino.DocObjects.RhinoObject Find(uint runtimeSerialNumber)
    {
      IntPtr ptr = UnsafeNativeMethods.CRhinoDoc_LookupObjectByRuntimeSerialNumber(m_doc.m_docId, runtimeSerialNumber);
      return Rhino.DocObjects.RhinoObject.CreateRhinoObjectHelper(ptr);
    }

    /// <summary>
    /// Finds all RhinoObjects that are in a given group.
    /// </summary>
    /// <param name="groupIndex">Index of group to search for.</param>
    /// <returns>An array of objects that belong to the specified group or null if no objects could be found.</returns>
    public Rhino.DocObjects.RhinoObject[] FindByGroup(int groupIndex)
    {
      Rhino.Runtime.INTERNAL_RhinoObjectArray rhobjs = new Rhino.Runtime.INTERNAL_RhinoObjectArray();
      IntPtr pArray = rhobjs.NonConstPointer();
      UnsafeNativeMethods.CRhinoDoc_LookupObjectsByGroup(m_doc.m_docId, groupIndex, pArray);
      Rhino.DocObjects.RhinoObject[] rc = rhobjs.ToArray();
      rhobjs.Dispose();
      return rc;
    }

    /// <summary>
    /// Finds all RhinoObjects that are in a given layer.
    /// </summary>
    /// <param name="layer">Layer to search.</param>
    /// <returns>
    /// Array of objects that belong to the specified group or null if no objects could be found.
    /// </returns>
    public Rhino.DocObjects.RhinoObject[] FindByLayer(Rhino.DocObjects.Layer layer)
    {
      int layer_index = layer.LayerIndex;
      Rhino.Runtime.INTERNAL_RhinoObjectArray rhobjs = new Rhino.Runtime.INTERNAL_RhinoObjectArray();
      IntPtr pArray = rhobjs.NonConstPointer();
      UnsafeNativeMethods.CRhinoDoc_LookupObjectsByLayer(m_doc.m_docId, layer_index, pArray);
      Rhino.DocObjects.RhinoObject[] rc = rhobjs.ToArray();
      rhobjs.Dispose();
      return rc;
    }

    /// <summary>
    /// Finds all RhinoObjects that are in a given layer.
    /// </summary>
    /// <param name="layerName">Name of layer to search.</param>
    /// <returns>
    /// Array of objects that belong to the specified group or null if no objects could be found.
    /// </returns>
    /// <example>
    /// <code source='examples\vbnet\ex_sellayer.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_sellayer.cs' lang='cs'/>
    /// <code source='examples\py\ex_sellayer.py' lang='py'/>
    /// </example>
    public Rhino.DocObjects.RhinoObject[] FindByLayer(string layerName)
    {
      int index = Document.Layers.Find(layerName, true);
      if (index < 0)
        return null;
      Layer l = Document.Layers[index];
      return FindByLayer(l);
    }

    /// <summary>
    /// Same as GetObjectList but converts the result to an array.
    /// </summary>
    /// <param name="filter">The object enumerator filter to customize inclusion requirements.</param>
    /// <returns>A Rhino object array. This array can be emptry but not null.</returns>
    public RhinoObject[] FindByFilter(ObjectEnumeratorSettings filter)
    {
      List<RhinoObject> list = new List<RhinoObject>(GetObjectList(filter));
      return list.ToArray();
    }

    [CLSCompliant(false)]
    public RhinoObject[] FindByObjectType(ObjectType typeFilter)
    {
      List<RhinoObject> list = new List<RhinoObject>(GetObjectList(typeFilter));
      return list.ToArray();
    }

    /// <summary>
    /// Finds all objects whose UserString matches the search patterns.
    /// </summary>
    /// <param name="key">Search pattern for UserString keys (supported wildcards are: ? = any single character, * = any sequence of characters).</param>
    /// <param name="value">Search pattern for UserString values (supported wildcards are: ? = any single character, * = any sequence of characters).</param>
    /// <param name="caseSensitive">If true, string comparison will be case sensitive.</param>
    /// <returns>An array of all objects whose UserString matches with the search patterns or null when no such objects could be found.</returns>
    public RhinoObject[] FindByUserString(string key, string value, bool caseSensitive)
    {
      return FindByUserString(key, value, caseSensitive, true, true, ObjectType.AnyObject);
    }
    /// <summary>
    /// Finds all objects whose UserString matches the search patterns.
    /// </summary>
    /// <param name="key">Search pattern for UserString keys (supported wildcards are: ? = any single character, * = any sequence of characters).</param>
    /// <param name="value">Search pattern for UserString values (supported wildcards are: ? = any single character, * = any sequence of characters).</param>
    /// <param name="caseSensitive">If true, string comparison will be case sensitive.</param>
    /// <param name="searchGeometry">If true, UserStrings attached to the geometry of an object will be searched.</param>
    /// <param name="searchAttributes">If true, UserStrings attached to the attributes of an object will be searched.</param>
    /// <param name="filter">Object type filter.</param>
    /// <returns>An array of all objects whose UserString matches with the search patterns or null when no such objects could be found.</returns>
    [CLSCompliant(false)]
    public RhinoObject[] FindByUserString(string key, string value, bool caseSensitive, bool searchGeometry, bool searchAttributes, Rhino.DocObjects.ObjectType filter)
    {
      ObjectEnumeratorSettings oes = new ObjectEnumeratorSettings();
      oes.ActiveObjects = true;
      oes.HiddenObjects = true;
      oes.LockedObjects = true;
      oes.NormalObjects = true;
      oes.IncludeLights = true;
      oes.ReferenceObjects = true;

      oes.IdefObjects = false;
      oes.IncludeGrips = false;
      oes.DeletedObjects = false;
      oes.IncludePhantoms = false;
      oes.SelectedObjectsFilter = false;

      oes.ObjectTypeFilter = filter;

      return FindByUserString(key, value, caseSensitive, searchGeometry, searchAttributes, oes);
    }
    /// <summary>
    /// Finds all objects whose UserString matches the search patterns.
    /// </summary>
    /// <param name="key">Search pattern for UserString keys (supported wildcards are: ? = any single character, * = any sequence of characters).</param>
    /// <param name="value">Search pattern for UserString values (supported wildcards are: ? = any single character, * = any sequence of characters).</param>
    /// <param name="caseSensitive">If true, string comparison will be case sensitive.</param>
    /// <param name="searchGeometry">If true, UserStrings attached to the geometry of an object will be searched.</param>
    /// <param name="searchAttributes">If true, UserStrings attached to the attributes of an object will be searched.</param>
    /// <param name="filter">Filter used to restrict the number of objects searched.</param>
    /// <returns>An array of all objects whose UserString matches with the search patterns or null when no such objects could be found.</returns>
    public RhinoObject[] FindByUserString(string key, string value, bool caseSensitive, bool searchGeometry, bool searchAttributes, Rhino.DocObjects.ObjectEnumeratorSettings filter)
    {
      Rhino.Runtime.INTERNAL_RhinoObjectArray rhobjs = new Rhino.Runtime.INTERNAL_RhinoObjectArray();
      IntPtr pArray = rhobjs.NonConstPointer();

      DocObjects.ObjectIterator it = new ObjectIterator(m_doc, filter);
      IntPtr pIterator = it.NonConstPointer();

      UnsafeNativeMethods.CRhinoDoc_LookupObjectsByUserText(key, value, caseSensitive, searchGeometry, searchAttributes, pIterator, pArray);

      Rhino.DocObjects.RhinoObject[] objs = rhobjs.ToArray();
      rhobjs.Dispose();
      return objs;
    }

    /// <summary>
    /// Finds all objects whose draw color matches a given color.
    /// </summary>
    /// <param name="drawColor">The alpha value of this color is ignored.</param>
    /// <param name="includeLights">true if lights should be included.</param>
    /// <returns>An array of Rhino document objects. This array can be empty.</returns>
    public RhinoObject[] FindByDrawColor(Rhino.Drawing.Color drawColor, bool includeLights)
    {
      ObjectEnumeratorSettings it = new ObjectEnumeratorSettings();
      it.IncludeLights = includeLights;
      it.IncludeGrips = false;
      it.IncludePhantoms = true;
      List<RhinoObject> rc = new List<RhinoObject>();
      foreach( RhinoObject obj in GetObjectList(it))
      {
        Rhino.Drawing.Color object_color = obj.Attributes.DrawColor(m_doc);
        if (object_color.R == drawColor.R && object_color.G == drawColor.G && object_color.B == drawColor.B)
          rc.Add(obj);
      }
      return rc.ToArray();
    }

    RhinoObject[] FindByRegion(RhinoViewport viewport, IEnumerable<Point3d> region, int mode, ObjectType filter)
    {
      IntPtr ptr_const_viewport = viewport.ConstPointer();
      List<Point3d> list_region = new List<Point3d>(region);
      Point3d[] array_points = list_region.ToArray();
      using (var objrefs = new Runtime.InteropWrappers.ClassArrayObjRef())
      {
        IntPtr ptr_objref_array = objrefs.NonConstPointer();
        UnsafeNativeMethods.RHC_RhinoRegionSelect(ptr_const_viewport, array_points.Length, array_points, mode, (uint)filter, ptr_objref_array);
        ObjRef[] objref_array = objrefs.ToNonConstArray();
        List<RhinoObject> rc = new List<RhinoObject>();
        for (int i = 0; i < objref_array.Length; i++)
        {
          RhinoObject rhobj = objref_array[i].Object();
          if (rhobj != null)
            rc.Add(rhobj);
        }
        return rc.ToArray();
      }
    }

    RhinoObject[] FindByRegion(RhinoViewport viewport, Point2d screen1, Point2d screen2, int mode, ObjectType filter)
    {
      double min_x = screen1.X < screen2.X ? screen1.X : screen2.X;
      double max_x = screen1.X > screen2.X ? screen1.X : screen2.X;
      double min_y = screen1.Y < screen2.Y ? screen1.Y : screen2.Y;
      double max_y = screen1.Y > screen2.Y ? screen1.Y : screen2.Y;
      var screen_to_world = viewport.GetTransform(CoordinateSystem.Screen, CoordinateSystem.World);
      Point3d[] pts = new Point3d[]{new Point3d(min_x, min_y, 0),
        new Point3d(max_x, min_y, 0),
        new Point3d(max_x, max_y, 0),
        new Point3d(min_x, max_y, 0)};
      for (int i = 0; i < pts.Length; i++)
      {
        pts[i].Transform(screen_to_world);
      }
      return FindByRegion(viewport, pts, mode, filter);
    }

    /// <summary>
    /// Finds objects bounded by a polyline region
    /// </summary>
    /// <param name="viewport">viewport to use for selection</param>
    /// <param name="region">list of points that define the </param>
    /// <param name="inside">should objects returned be the ones inside of this region (or outside)</param>
    /// <param name="filter">filter down list by object type</param>
    /// <returns>An array of RhinoObjects that are inside of this region</returns>
    [CLSCompliant(false)]
    public RhinoObject[] FindByWindowRegion(RhinoViewport viewport, IEnumerable<Point3d> region, bool inside, ObjectType filter)
    {
      // 0=window, 1=crossing, 2=outside window, 3=outside crossing window
      return FindByRegion(viewport, region, inside ? 0 : 2, filter);
    }

    /// <summary>
    /// Finds objects bounded by a polyline region
    /// </summary>
    /// <param name="viewport">viewport to use for selection</param>
    /// <param name="screen1">first screen corner</param>
    /// <param name="screen2">second screen corner</param>
    /// <param name="inside">should objects returned be the ones inside of this region (or outside)</param>
    /// <param name="filter">filter down list by object type</param>
    /// <returns>An array of RhinoObjects that are inside of this region</returns>
    [CLSCompliant(false)]
    public RhinoObject[] FindByWindowRegion(RhinoViewport viewport, Point2d screen1, Point2d screen2, bool inside, ObjectType filter)
    {
      // 0=window, 1=crossing, 2=outside window, 3=outside crossing window
      return FindByRegion(viewport, screen1, screen2, inside ? 0 : 2, filter);
    }

    /// <summary>
    /// Finds objects bounded by a polyline region
    /// </summary>
    /// <param name="viewport">viewport to use for selection</param>
    /// <param name="region">list of points that define the </param>
    /// <param name="inside">should objects returned be the ones inside of this region (or outside)</param>
    /// <param name="filter">filter down list by object type</param>
    /// <returns>An array of RhinoObjects that are inside of this region</returns>
    [CLSCompliant(false)]
    public RhinoObject[] FindByCrossingWindowRegion(RhinoViewport viewport, IEnumerable<Point3d> region, bool inside, ObjectType filter)
    {
      // 0=window, 1=crossing, 2=outside window, 3=outside crossing window
      return FindByRegion(viewport, region, inside ? 1 : 3, filter);
    }

    /// <summary>
    /// Finds objects bounded by a region
    /// </summary>
    /// <param name="viewport">viewport to use for selection</param>
    /// <param name="screen1">first screen corner</param>
    /// <param name="screen2">second screen corner</param>
    /// <param name="inside">should objects returned be the ones inside of this region (or outside)</param>
    /// <param name="filter">filter down list by object type</param>
    /// <returns>An array of RhinoObjects that are inside of this region</returns>
    [CLSCompliant(false)]
    public RhinoObject[] FindByCrossingWindowRegion(RhinoViewport viewport, Point2d screen1, Point2d screen2, bool inside, ObjectType filter)
    {
      // 0=window, 1=crossing, 2=outside window, 3=outside crossing window
      return FindByRegion(viewport, screen1, screen2, inside ? 1 : 3, filter);
    }

    /// <summary>
    /// Finds all of the clipping plane objects that actively clip a viewport.
    /// </summary>
    /// <param name="viewport">The viewport in which clipping planes are searched.</param>
    /// <returns>An array of clipping plane objects. The array can be emptry but not null.</returns>
    public Rhino.DocObjects.ClippingPlaneObject[] FindClippingPlanesForViewport(Rhino.Display.RhinoViewport viewport)
    {
      Guid id = viewport.Id;

      Rhino.DocObjects.RhinoObject[] clipping_planes = FindByObjectType(ObjectType.ClipPlane);
      if (clipping_planes.Length == 0)
        return new Rhino.DocObjects.ClippingPlaneObject[0];

      List<Rhino.DocObjects.ClippingPlaneObject> rc = new List<ClippingPlaneObject>();
      for (int i = 0; i < clipping_planes.Length; i++)
      {
        Rhino.DocObjects.ClippingPlaneObject cp = clipping_planes[i] as Rhino.DocObjects.ClippingPlaneObject;
        if (cp != null)
        {
          Guid[] ids = cp.ClippingPlaneGeometry.ViewportIds();
          for (int j = 0; j < ids.Length; j++)
          {
            if (ids[j] == id)
            {
              rc.Add(cp);
              break;
            }
          }
        }
      }
      return rc.ToArray();
    }

#region Object addition
    public void AddRhinoObject(Rhino.DocObjects.Custom.CustomMeshObject meshObject)
    {
      AddRhinoObjectHelper(meshObject, null);
    }

    public void AddRhinoObject(Rhino.DocObjects.MeshObject meshObject, Rhino.Geometry.Mesh mesh)
    {
      AddRhinoObjectHelper(meshObject, mesh);
    }

    public void AddRhinoObject(Rhino.DocObjects.Custom.CustomBrepObject brepObject)
    {
      //helper will use geometry already hung off of the custom object
      AddRhinoObjectHelper(brepObject, null);
    }

    public void AddRhinoObject(Rhino.DocObjects.BrepObject brepObject, Rhino.Geometry.Brep brep)
    {
      AddRhinoObjectHelper(brepObject, brep);
    }
    /*
    public void AddRhinoObject(Rhino.DocObjects.PointCloudObject pointCloudObject, Rhino.Geometry.PointCloud pointCloud)
    {
      AddRhinoObjectHelper(pointCloudObject, pointCloud);
    }
    */
    public void AddRhinoObject(Rhino.DocObjects.Custom.CustomPointObject pointObject)
    {
      AddRhinoObjectHelper(pointObject, null);
    }
    public void AddRhinoObject(Rhino.DocObjects.PointObject pointObject, Rhino.Geometry.Point point)
    {
      AddRhinoObjectHelper(pointObject, point);
    }
    
    public void AddRhinoObject(Rhino.DocObjects.CurveObject curveObject, Rhino.Geometry.Curve curve)
    {
      AddRhinoObjectHelper(curveObject, curve);
    }

    void AddRhinoObjectHelper(RhinoObject rhinoObject, GeometryBase geometry)
    {
      bool is_proper_subclass = rhinoObject is Rhino.DocObjects.BrepObject ||
                                rhinoObject is Rhino.DocObjects.Custom.CustomCurveObject ||
                                rhinoObject is Rhino.DocObjects.MeshObject ||
                                rhinoObject is Rhino.DocObjects.PointObject;

      // Once the deprecated functions are removed, we should switch to checking for custom subclasses
      //bool is_proper_subclass = rhinoObject is Rhino.DocObjects.Custom.CustomBrepObject ||
      //                          rhinoObject is Rhino.DocObjects.Custom.CustomCurveObject ||
      //                          rhinoObject is Rhino.DocObjects.Custom.CustomMeshObject;
      if( !is_proper_subclass )
        throw new NotImplementedException();

      if( rhinoObject.Document != null )
        throw new NotImplementedException();

      Type t = rhinoObject.GetType();
      if (t.GetConstructor(Type.EmptyTypes) == null)
        throw new NotImplementedException("class must have a public parameterless constructor");

      IntPtr pRhinoObject = rhinoObject.m_pRhinoObject;
      if (geometry != null)
      {
        if ((rhinoObject is Rhino.DocObjects.BrepObject && !(geometry is Rhino.Geometry.Brep)) ||
            (rhinoObject is Rhino.DocObjects.CurveObject && !(geometry is Rhino.Geometry.Curve)) ||
            (rhinoObject is Rhino.DocObjects.MeshObject && !(geometry is Rhino.Geometry.Mesh)) ||
            (rhinoObject is Rhino.DocObjects.PointObject && !(geometry is Rhino.Geometry.Point)))
        {
          throw new NotImplementedException("geometry type does not match rhino object class");
        }
        IntPtr pConstGeometry = geometry.ConstPointer();
        pRhinoObject = UnsafeNativeMethods.CRhinoCustomObject_New(pRhinoObject, pConstGeometry);
      }
      else
      {
        Rhino.Geometry.GeometryBase g = rhinoObject.Geometry;
        if (g == null)
          throw new NotImplementedException("no geometry associated with this RhinoObject");
      }

      uint serial_number = UnsafeNativeMethods.CRhinoObject_RuntimeSN(pRhinoObject);
      if (serial_number > 0)
      {
        rhinoObject.m_rhinoobject_serial_number = serial_number;
        rhinoObject.m_pRhinoObject = IntPtr.Zero;
        GC.SuppressFinalize(rhinoObject);
        AddCustomObjectForTracking(serial_number, rhinoObject, pRhinoObject);
        UnsafeNativeMethods.CRhinoDoc_AddRhinoObject(m_doc.m_docId, pRhinoObject);
      }
    }

    System.Collections.Generic.SortedList<uint, RhinoObject> m_custom_objects;
    internal void AddCustomObjectForTracking(uint serialNumber, RhinoObject rhobj, IntPtr pRhinoObject)
    {
      if (m_custom_objects == null)
        m_custom_objects = new SortedList<uint, RhinoObject>();
      m_custom_objects.Add(serialNumber, rhobj);

      // 17 Sept 2012 S. Baer
      // This seems like the best spot to get everything in sync.
      // Update the description strings when replacing the object
      Type base_type = typeof(RhinoObject);
      Type t = rhobj.GetType();
      const BindingFlags flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
      System.Reflection.MethodInfo mi = t.GetRuntimeMethod("ShortDescription", flags);
      // Don't set description strings if the function has not been overloaded
      if (mi.DeclaringType != base_type)
      {
        string description = rhobj.ShortDescription(false);
        string description_plural = rhobj.ShortDescription(true);
        UnsafeNativeMethods.CRhinoCustomObject_SetDescriptionStrings(pRhinoObject, description, description_plural);
      }
    }
    internal RhinoObject FindCustomObject(uint serialNumber)
    {
      RhinoObject rc = null;
      if (m_custom_objects != null)
        m_custom_objects.TryGetValue(serialNumber, out rc);
      return rc;
    }


    /// <summary>
    /// Adds geometry that is not further specified.
    /// <para>This is meant, for example, to handle addition of sets of different geometrical entities.</para>
    /// </summary>
    /// <param name="geometry">The base geometry. This cannot be null.</param>
    /// <returns>The new object ID on success.</returns>
    /// <exception cref="ArgumentNullException">If geometry is null.</exception>
    public Guid Add(GeometryBase geometry)
    {
      return Add(geometry, null);
    }

    /// <summary>
    /// Adds geometry that is not further specified.
    /// <para>This is meant, for example, to handle addition of sets of different geometrical entities.</para>
    /// </summary>
    /// <param name="geometry">The base geometry. This cannot be null.</param>
    /// <param name="attributes">The object attributes. This can be null.</param>
    /// <returns>The new object ID on success.</returns>
    /// <exception cref="ArgumentNullException">If geometry is null.</exception>
    public Guid Add(GeometryBase geometry, ObjectAttributes attributes)
    {
      if (geometry == null)
      {
        throw new ArgumentNullException("geometry");
      }

      Guid objId;
      switch (geometry.ObjectType)
      {
        case ObjectType.Point:
          objId = AddPoint(((Point)geometry).Location, attributes);
          break;
        case ObjectType.PointSet:
          objId = AddPointCloud((PointCloud)geometry, attributes);
          break;
        case ObjectType.Curve:
          objId = AddCurve((Curve)geometry, attributes);
          break;
        case ObjectType.Surface:
          objId = AddSurface((Surface)geometry, attributes);
          break;
        case ObjectType.Brep:
          objId = AddBrep((Brep)geometry, attributes);
          break;
        case ObjectType.Mesh:
          objId = AddMesh((Mesh)geometry, attributes);
          break;
        case ObjectType.Light:
          Light light = (Light)geometry;
          var index = Document.Lights.Add(light, attributes);
          objId = Document.Lights[index].Id;
          break;
        case ObjectType.Annotation:
          LinearDimension ld = geometry as LinearDimension;
          if (ld != null)
          {
            objId = AddLinearDimension(ld, attributes);
            break;
          }
          RadialDimension rd = geometry as RadialDimension;
          if (rd != null)
          {
            objId = AddRadialDimension(rd, attributes);
            break;
          }
          AngularDimension ad = geometry as AngularDimension;
          if (ad != null)
          {
            objId = AddAngularDimension(ad, attributes);
            break;
          }
          throw new NotImplementedException("Add currently does not support this annotation type.");
        case ObjectType.InstanceDefinition:
          throw new NotImplementedException("Add currently does not support instance definition types.");
        case ObjectType.InstanceReference:
          throw new NotImplementedException("Add currently does not support instance reference types.");
        case ObjectType.TextDot:
          objId = AddTextDot((TextDot)geometry, attributes);
          break;
        case ObjectType.Grip:
          throw new NotImplementedException("Add currently does not support grip types.");
        case ObjectType.Detail:
          throw new NotImplementedException("Add currently does not support detail types.");
        case ObjectType.Hatch:
          objId = AddHatch((Hatch)geometry, attributes);
          break;
        case ObjectType.MorphControl:
          objId = AddMorphControl((MorphControl)geometry, attributes);
          break;
        case ObjectType.Cage:
          throw new NotImplementedException("Add currently does not support cage types.");
        case ObjectType.Phantom:
          throw new NotImplementedException("Add currently does not support phantom types.");
        case ObjectType.ClipPlane:
          throw new NotSupportedException("Add currently does not support clipping planes.");
        case ObjectType.Extrusion:
          objId = AddExtrusion((Extrusion)geometry, attributes);
          break;
        default:
          throw new NotSupportedException("Only native types can be added to the document.");
      }
      return objId;
    }


    /// <summary>
    /// Adds a point object to the document.
    /// </summary>
    /// <param name="x">X component of point coordinate.</param>
    /// <param name="y">Y component of point coordinate.</param>
    /// <param name="z">Z component of point coordinate.</param>
    /// <returns>A unique identifier for the object..</returns>
    public Guid AddPoint(double x, double y, double z)
    {
      return AddPoint(new Point3d(x, y, z));
    }
    /// <summary>Adds a point object to the document.</summary>
    /// <param name="point">location of point.</param>
    /// <returns>A unique identifier for the object.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_dividebylength.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_dividebylength.cs' lang='cs'/>
    /// <code source='examples\py\ex_dividebylength.py' lang='py'/>
    /// </example>
    public Guid AddPoint(Point3d point)
    {
      return AddPoint(point, null);
    }
    /// <summary>Adds a point object to the document.</summary>
    /// <param name="point">location of point.</param>
    /// <param name="attributes">attributes to apply to point. null is acceptible</param>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddPoint(Point3d point, DocObjects.ObjectAttributes attributes)
    {
      return AddPoint(point, attributes, null, false);
    }

    /// <summary>Adds a point object to the document</summary>
    /// <param name="point">location of point</param>
    /// <param name="attributes">attributes to apply to point. null is acceptible</param>
    /// <param name="history">history associated with this point. null is acceptable</param>
    /// <param name="reference">
    /// true if the object is from a reference file.  Reference objects do
    /// not persist in archives
    /// </param>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddPoint(Point3d point, DocObjects.ObjectAttributes attributes, HistoryRecord history, bool reference)
    {
      IntPtr pAttributes = (attributes == null) ? IntPtr.Zero : attributes.ConstPointer();
      IntPtr pHistory = (history == null) ? IntPtr.Zero : history.Handle;
      return UnsafeNativeMethods.CRhinoDoc_AddPoint(m_doc.m_docId, point, pAttributes, pHistory, reference);
    }

    /// <summary>Adds a point object to the document.</summary>
    /// <param name="point">location of point.</param>
    /// <returns>A unique identifier for the object.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_dividebylength.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_dividebylength.cs' lang='cs'/>
    /// <code source='examples\py\ex_dividebylength.py' lang='py'/>
    /// </example>
    public Guid AddPoint(Point3f point)
    {
      Point3d p3d = new Point3d(point);
      return AddPoint(p3d);
    }
    /// <summary>Adds a point object to the document.</summary>
    /// <param name="point">location of point.</param>
    /// <param name="attributes">attributes to apply to point.</param>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddPoint(Point3f point, DocObjects.ObjectAttributes attributes)
    {
      Point3d p3d = new Point3d(point);
      return AddPoint(p3d, attributes);
    }


    /// <summary>
    /// Adds multiple points to the document.
    /// </summary>
    /// <param name="points">Points to add.</param>
    /// <returns>List of object ids.</returns>
    public RhinoList<Guid> AddPoints(IEnumerable<Point3d> points)
    {
      if (points == null)
        throw new ArgumentNullException("points");

      RhinoList<Guid> ids = new RhinoList<Guid>();
      foreach (Point3d pt in points)
      {
        ids.Add(AddPoint(pt));
      }

      return ids;
    }
    /// <summary>
    /// Adds multiple points to the document.
    /// </summary>
    /// <param name="points">Points to add.</param>
    /// <param name="attributes">Attributes to apply to point objects.</param>
    /// <returns>List of object ids.</returns>
    public RhinoList<Guid> AddPoints(IEnumerable<Point3d> points, DocObjects.ObjectAttributes attributes)
    {
      if (points == null) { throw new ArgumentNullException("points"); }

      RhinoList<Guid> ids = new RhinoList<Guid>();
      foreach (Point3d pt in points)
      {
        ids.Add(AddPoint(pt, attributes));
      }

      return ids;
    }

    /// <summary>
    /// Adds multiple points to the document.
    /// </summary>
    /// <param name="points">Points to add.</param>
    /// <returns>List of object ids.</returns>
    public RhinoList<Guid> AddPoints(IEnumerable<Point3f> points)
    {
      if (points == null) { throw new ArgumentNullException("points"); }

      RhinoList<Guid> ids = new RhinoList<Guid>();
      foreach (Point3f pt in points)
      {
        ids.Add(AddPoint(pt));
      }

      return ids;
    }
    /// <summary>
    /// Adds multiple points to the document.
    /// </summary>
    /// <param name="points">Points to add.</param>
    /// <param name="attributes">Attributes to apply to point objects.</param>
    /// <returns>List of object ids.</returns>
    public RhinoList<Guid> AddPoints(IEnumerable<Point3f> points, DocObjects.ObjectAttributes attributes)
    {
      if (points == null) { throw new ArgumentNullException("points"); }

      RhinoList<Guid> ids = new RhinoList<Guid>();
      foreach (Point3f pt in points)
      {
        ids.Add(AddPoint(pt, attributes));
      }

      return ids;
    }

    /// <summary>Adds a point cloud object to the document.</summary>
    /// <param name="cloud">PointCloud to add.</param>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddPointCloud(PointCloud cloud)
    {
      return AddPointCloud(cloud, null);
    }
    /// <summary>Adds a point cloud object to the document.</summary>
    /// <param name="cloud">PointCloud to add.</param>
    /// <param name="attributes">Attributes to apply to point cloud.</param>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddPointCloud(PointCloud cloud, DocObjects.ObjectAttributes attributes)
    {
      return AddPointCloud(cloud, attributes, null, false);
    }

    /// <summary>Adds a point cloud object to the document.</summary>
    /// <param name="cloud">PointCloud to add.</param>
    /// <param name="attributes">Attributes to apply to point cloud. null is acceptable</param>
    /// <param name="history">history associated with this pointcloud. null is acceptable</param>
    /// <param name="reference">
    /// true if the object is from a reference file.  Reference objects do
    /// not persist in archives
    /// </param>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddPointCloud(PointCloud cloud, DocObjects.ObjectAttributes attributes, DocObjects.HistoryRecord history, bool reference)
    {
      if (cloud == null)
        throw new ArgumentNullException("cloud");

      IntPtr pCloud = cloud.ConstPointer();
      IntPtr pAttrs = (attributes==null) ? IntPtr.Zero : attributes.ConstPointer();
      IntPtr pHistory = (history == null) ? IntPtr.Zero : history.Handle;

      return UnsafeNativeMethods.CRhinoDoc_AddPointCloud2(m_doc.m_docId, pCloud, pAttrs, pHistory, reference);
    }

    /// <summary>Adds a point cloud object to the document.</summary>
    /// <param name="points">A list, an array or any enumerable set of points.</param>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddPointCloud(IEnumerable<Point3d> points)
    {
      return AddPointCloud(points, null);
    }
    /// <summary>Adds a point cloud object to the document.</summary>
    /// <param name="points">A list, an array or any enumerable set of points.</param>
    /// <param name="attributes">attributes to apply to point cloud.</param>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddPointCloud(IEnumerable<Point3d> points, DocObjects.ObjectAttributes attributes)
    {
      int count;
      Point3d[] ptArray = Collections.Point3dList.GetConstPointArray(points, out count);
      if (null == ptArray || count < 1)
        return Guid.Empty;

      IntPtr pAttrs = IntPtr.Zero;
      if (null != attributes)
        pAttrs = attributes.ConstPointer();

      return UnsafeNativeMethods.CRhinoDoc_AddPointCloud(m_doc.m_docId, count, ptArray, pAttrs, IntPtr.Zero, false);
    }

    /// <summary>Adds a point cloud object to the document.</summary>
    /// <param name="points">A list, an array or any enumerable set of points</param>
    /// <param name="attributes">Attributes to apply to point cloud. null is acceptable</param>
    /// <param name="history">history associated with this pointcloud. null is acceptable</param>
    /// <param name="reference">
    /// true if the object is from a reference file.  Reference objects do
    /// not persist in archives
    /// </param>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddPointCloud(IEnumerable<Point3d> points, DocObjects.ObjectAttributes attributes, DocObjects.HistoryRecord history, bool reference)
    {
      int count;
      Point3d[] ptArray = Collections.Point3dList.GetConstPointArray(points, out count);
      if (null == ptArray || count < 1)
        return Guid.Empty;

      IntPtr pAttrs = (attributes == null) ? IntPtr.Zero : attributes.ConstPointer();
      IntPtr pHistory = (history == null) ? IntPtr.Zero : history.Handle;

      return UnsafeNativeMethods.CRhinoDoc_AddPointCloud(m_doc.m_docId, count, ptArray, pAttrs, pHistory, reference);
    }

    /// <summary>
    /// Adds a clipping plane object to Rhino.
    /// </summary>
    /// <param name="plane">The plane value.</param>
    /// <param name="uMagnitude">The size in the U direction.</param>
    /// <param name="vMagnitude">The size in the V direction.</param>
    /// <param name="clippedViewportId">Viewport ID that the new clipping plane will clip.</param>
    /// <returns>A unique identifier for the object.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addclippingplane.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addclippingplane.cs' lang='cs'/>
    /// <code source='examples\py\ex_addclippingplane.py' lang='py'/>
    /// </example>
    public Guid AddClippingPlane(Plane plane, double uMagnitude, double vMagnitude, Guid clippedViewportId)
    {
      return AddClippingPlane(plane, uMagnitude, vMagnitude, new Guid[] { clippedViewportId });
    }

    /// <summary>
    /// Adds a clipping plane object to Rhino.
    /// </summary>
    /// <param name="plane">The plane value.</param>
    /// <param name="uMagnitude">The size in the U direction.</param>
    /// <param name="vMagnitude">The size in the V direction.</param>
    /// <param name="clippedViewportIds">A list, an array or any enumerable set of viewport IDs
    /// that the new clipping plane will clip.</param>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddClippingPlane(Plane plane, double uMagnitude, double vMagnitude, IEnumerable<Guid> clippedViewportIds)
    {
      if (null == clippedViewportIds)
        return Guid.Empty;
      List<Guid> ids = new List<Guid>();
      foreach (Guid item in clippedViewportIds)
        ids.Add(item);
      Guid[] clippedIds = ids.ToArray();
      int count = clippedIds.Length;
      if (count < 1)
        return Guid.Empty;
      Guid rc = UnsafeNativeMethods.CRhinoDoc_AddClippingPlane(m_doc.m_docId, ref plane, uMagnitude, vMagnitude, count, clippedIds, IntPtr.Zero, IntPtr.Zero, false);
      return rc;
    }
    /// <summary>
    /// Adds a clipping plane object to Rhino.
    /// </summary>
    /// <param name="plane">The plane value.</param>
    /// <param name="uMagnitude">The size in the U direction.</param>
    /// <param name="vMagnitude">The size in the V direction.</param>
    /// <param name="clippedViewportIds">A list, an array or any enumerable set of viewport IDs
    /// that the new clipping plane will clip.</param>
    /// <param name="attributes">Document attributes for the plane.</param>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddClippingPlane(Plane plane, double uMagnitude, double vMagnitude, IEnumerable<Guid> clippedViewportIds, DocObjects.ObjectAttributes attributes)
    {
      if (null == attributes)
        return AddClippingPlane(plane, uMagnitude, vMagnitude, clippedViewportIds);
      if (null == clippedViewportIds)
        return Guid.Empty;
      List<Guid> ids = new List<Guid>();
      foreach (Guid item in clippedViewportIds)
        ids.Add(item);
      Guid[] clippedIds = ids.ToArray();
      int count = clippedIds.Length;
      if (count < 1)
        return Guid.Empty;
      IntPtr pAttrs = attributes.ConstPointer();
      Guid rc = UnsafeNativeMethods.CRhinoDoc_AddClippingPlane(m_doc.m_docId, ref plane, uMagnitude, vMagnitude, count, clippedIds, pAttrs, IntPtr.Zero, false);
      return rc;
    }

    public Guid AddClippingPlane(Plane plane, double uMagnitude, double vMagnitude, Guid clippedViewportId, DocObjects.ObjectAttributes attributes, DocObjects.HistoryRecord history, bool reference)
    {
      return AddClippingPlane(plane, uMagnitude, vMagnitude, new Guid[] { clippedViewportId }, attributes, history, reference);
    }
    public Guid AddClippingPlane(Plane plane, double uMagnitude, double vMagnitude, IEnumerable<Guid> clippedViewportIds, DocObjects.ObjectAttributes attributes, DocObjects.HistoryRecord history, bool reference)
    {
      List<Guid> ids = new List<Guid>(clippedViewportIds);
      Guid[] clippedIds = ids.ToArray();
      int count = clippedIds.Length;
      if (count < 1)
        return Guid.Empty;

      IntPtr pConstAttributes = (attributes == null) ? IntPtr.Zero : attributes.ConstPointer();
      IntPtr pHistory = (history == null) ? IntPtr.Zero : history.Handle;
      Guid rc = UnsafeNativeMethods.CRhinoDoc_AddClippingPlane(m_doc.m_docId, ref plane, uMagnitude, vMagnitude, count, clippedIds, pConstAttributes, pHistory, reference);
      return rc;
    }


    /// <example>
    /// <code source='examples\vbnet\ex_addlineardimension.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addlineardimension.cs' lang='cs'/>
    /// <code source='examples\py\ex_addlineardimension.py' lang='py'/>
    /// </example>
    public Guid AddLinearDimension(LinearDimension dimension)
    {
      return AddLinearDimension(dimension, null);
    }

    public Guid AddLinearDimension(LinearDimension dimension, DocObjects.ObjectAttributes attributes)
    {
      return AddLinearDimension(dimension, attributes, null, false);
    }

    public Guid AddLinearDimension(LinearDimension dimension, DocObjects.ObjectAttributes attributes, DocObjects.HistoryRecord history, bool reference)
    {
      IntPtr pConstDimension = dimension.ConstPointer();
      IntPtr pAttributes = (attributes == null) ? IntPtr.Zero : attributes.ConstPointer();
      IntPtr pHistory = (history == null) ? IntPtr.Zero : history.Handle;
      return UnsafeNativeMethods.CRhinoDoc_AddLinearDimension(m_doc.m_docId, pConstDimension, pAttributes, pHistory, reference);
    }

    /// <example>
    /// <code source='examples\vbnet\ex_addradialdimension.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addradialdimension.cs' lang='cs'/>
    /// <code source='examples\py\ex_addradialdimension.py' lang='py'/>
    /// </example>
    public Guid AddRadialDimension(RadialDimension dimension)
    {
      return AddRadialDimension(dimension, null);
    }

    public Guid AddRadialDimension(RadialDimension dimension, DocObjects.ObjectAttributes attributes)
    {
      return AddRadialDimension(dimension, attributes, null, false);
    }

    public Guid AddRadialDimension(RadialDimension dimension, DocObjects.ObjectAttributes attributes, DocObjects.HistoryRecord history, bool reference)
    {
      IntPtr pConstDimension = dimension.ConstPointer();
      IntPtr pAttributes = (attributes==null) ? IntPtr.Zero : attributes.ConstPointer();
      IntPtr pHistory = (history == null) ? IntPtr.Zero : history.Handle;
      return UnsafeNativeMethods.CRhinoDoc_AddRadialDimension(m_doc.m_docId, pConstDimension, pAttributes, pHistory, reference);
    }

    public Guid AddAngularDimension(AngularDimension dimension)
    {
      return AddAngularDimension(dimension, null);
    }

    public Guid AddAngularDimension(AngularDimension dimension, DocObjects.ObjectAttributes attributes)
    {
      return AddAngularDimension(dimension, attributes, null, false);
    }

    public Guid AddAngularDimension(AngularDimension dimension, DocObjects.ObjectAttributes attributes, DocObjects.HistoryRecord history, bool reference)
    {
      IntPtr pConstDimension = dimension.ConstPointer();
      IntPtr pAttributes = (attributes == null) ? IntPtr.Zero : attributes.ConstPointer();
      IntPtr pHistory = (history == null) ? IntPtr.Zero : history.Handle;
      return UnsafeNativeMethods.CRhinoDoc_AddAngularDimension(m_doc.m_docId, pConstDimension, pAttributes, pHistory, reference);
    }

    /// <summary>Adds a line object to Rhino.</summary>
    /// <param name="from">The line origin.</param>
    /// <param name="to">The line end.</param>
    /// <returns>A unique identifier for the object.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addline.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addline.cs' lang='cs'/>
    /// <code source='examples\py\ex_addline.py' lang='py'/>
    /// </example>
    public Guid AddLine(Point3d from, Point3d to)
    {
      return AddLine(from, to, null);
    }
    /// <summary>Adds a line object to Rhino.</summary>
    /// <param name="from">The line origin.</param>
    /// <param name="to">The line end.</param>
    /// <param name="attributes">Attributes to apply to line.</param>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddLine(Point3d from, Point3d to, DocObjects.ObjectAttributes attributes)
    {
      return AddLine(from, to, attributes, null, false);
    }

    public Guid AddLine(Point3d from, Point3d to, DocObjects.ObjectAttributes attributes, DocObjects.HistoryRecord history, bool reference)
    {
      IntPtr pAttributes = (attributes == null) ? IntPtr.Zero : attributes.ConstPointer();
      IntPtr pHistory = (history == null) ? IntPtr.Zero : history.Handle;
      return UnsafeNativeMethods.CRhinoDoc_AddLine(m_doc.m_docId, from, to, pAttributes, pHistory, reference);
    }

    /// <summary>Adds a line object to Rhino.</summary>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddLine(Line line)
    {
      return AddLine(line.From, line.To);
    }
    /// <summary>Adds a line object to Rhino.</summary>
    /// <param name="line">The line value.</param>
    /// <param name="attributes">Attributes to apply to line.</param>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddLine(Line line, DocObjects.ObjectAttributes attributes)
    {
      return AddLine(line.From, line.To, attributes);
    }


    /// <summary>Adds a polyline object to Rhino.</summary>
    /// <param name="points">A <see cref="Polyline"/>; a list, an array, or any enumerable set of <see cref="Point3d"/>.</param>
    /// <returns>A unique identifier for the object.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_tightboundingbox.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_tightboundingbox.cs' lang='cs'/>
    /// <code source='examples\py\ex_tightboundingbox.py' lang='py'/>
    /// </example>
    public Guid AddPolyline(IEnumerable<Point3d> points)
    {
      return AddPolyline(points, null);
    }
    /// <summary>Adds a polyline object to Rhino.</summary>
    /// <param name="points">A <see cref="Polyline"/>; a list, an array, or any enumerable set of <see cref="Point3d"/>.</param>
    /// <param name="attributes">attributes to apply to line.</param>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddPolyline(IEnumerable<Point3d> points, DocObjects.ObjectAttributes attributes)
    {
      return AddPolyline(points, attributes, null, false);
    }

    public Guid AddPolyline(IEnumerable<Point3d> points, DocObjects.ObjectAttributes attributes, DocObjects.HistoryRecord history, bool reference)
    {
      int count;
      Point3d[] ptArray = Collections.Point3dList.GetConstPointArray(points, out count);
      if (null == ptArray || count < 1)
        return Guid.Empty;

      IntPtr pAttributes = (attributes == null) ? IntPtr.Zero : attributes.ConstPointer();
      IntPtr pHistory = (history == null) ? IntPtr.Zero : history.Handle;
      return UnsafeNativeMethods.CRhinoDoc_AddPolyLine(m_doc.m_docId, count, ptArray, pAttributes, pHistory, reference);
    }

    /// <summary>Adds a curve object to the document representing an arc.</summary>
    /// <param name="arc">An arc value.</param>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddArc(Arc arc)
    {
      return AddArc(arc, null, null, false);
    }
    /// <summary>Adds a curve object to the document representing an arc.</summary>
    /// <param name="arc">An arc value.</param>
    /// <param name="attributes">Attributes to apply to arc.</param>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddArc(Arc arc, DocObjects.ObjectAttributes attributes)
    {
      return AddArc(arc, attributes, null, false);
    }

    public Guid AddArc(Arc arc, DocObjects.ObjectAttributes attributes, DocObjects.HistoryRecord history, bool reference)
    {
      IntPtr pAttributes = (attributes == null) ? IntPtr.Zero : attributes.ConstPointer();
      IntPtr pHistory = (history == null) ? IntPtr.Zero : history.Handle;
      return UnsafeNativeMethods.CRhinoDoc_AddArc(m_doc.m_docId, ref arc, pAttributes, pHistory, reference);
    }

    /// <summary>Adds a curve object to the document representing a circle.</summary>
    /// <param name="circle">A circle value.</param>
    /// <returns>A unique identifier for the object.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addcircle.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addcircle.cs' lang='cs'/>
    /// <code source='examples\py\ex_addcircle.py' lang='py'/>
    /// </example>
    public Guid AddCircle(Circle circle)
    {
      return AddCircle(circle, null, null, false);
    }
    /// <summary>Adds a curve object to the document representing a circle.</summary>
    /// <param name="circle">A circle value.</param>
    /// <param name="attributes">Attributes to apply to circle.</param>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddCircle(Circle circle, DocObjects.ObjectAttributes attributes)
    {
      return AddCircle(circle, attributes, null, false);
    }

    public Guid AddCircle(Circle circle, DocObjects.ObjectAttributes attributes, DocObjects.HistoryRecord history, bool reference)
    {
      IntPtr pAttributes = (attributes == null) ? IntPtr.Zero : attributes.ConstPointer();
      IntPtr pHistory = (history == null) ? IntPtr.Zero : history.Handle;
      return UnsafeNativeMethods.CRhinoDoc_AddCircle(m_doc.m_docId, ref circle, pAttributes, pHistory, reference);
    }


    /// <summary>Adds a curve object to the document representing an ellipse.</summary>
    /// <param name="ellipse">An ellipse value.</param>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddEllipse(Ellipse ellipse)
    {
      return AddEllipse(ellipse, null, null, false);
    }
    /// <summary>Adds a curve object to the document representing an ellipse.</summary>
    /// <param name="ellipse">An ellipse value.</param>
    /// <param name="attributes">Attributes to apply to ellipse.</param>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddEllipse(Ellipse ellipse, DocObjects.ObjectAttributes attributes)
    {
      return AddEllipse(ellipse, attributes, null, false);
    }

    public Guid AddEllipse(Ellipse ellipse, DocObjects.ObjectAttributes attributes, DocObjects.HistoryRecord history, bool reference)
    {
      IntPtr pAttributes = (attributes == null) ? IntPtr.Zero : attributes.ConstPointer();
      IntPtr pHistory = (history == null) ? IntPtr.Zero : history.Handle;
      return UnsafeNativeMethods.CRhinoDoc_AddEllipse(m_doc.m_docId, ref ellipse, pAttributes, pHistory, reference);
    }

    /// <example>
    /// <code source='examples\vbnet\ex_addsphere.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addsphere.cs' lang='cs'/>
    /// <code source='examples\py\ex_addsphere.py' lang='py'/>
    /// </example>
    public Guid AddSphere(Sphere sphere)
    {
      return AddSphere(sphere, null, null, false);
    }
    public Guid AddSphere(Sphere sphere, DocObjects.ObjectAttributes attributes)
    {
      return AddSphere(sphere, attributes, null, false);
    }

    public Guid AddSphere(Sphere sphere, DocObjects.ObjectAttributes attributes, DocObjects.HistoryRecord history, bool reference)
    {
      IntPtr pAttributes = (attributes == null) ? IntPtr.Zero : attributes.ConstPointer();
      IntPtr pHistory = (history == null) ? IntPtr.Zero : history.Handle;
      return UnsafeNativeMethods.CRhinoDoc_AddSphere(m_doc.m_docId, ref sphere, pAttributes, pHistory, reference);
    }

    //[skipping]
    //  CRhinoCurveObject* AddCurveObject( const ON_BezierCurve& bezier_curve, const ON_3dmObjectAttributes* pAttributes = NULL, CRhinoHistory* pHistory = NULL,  BOOL bReference = NULL );


    /// <summary>Adds a curve object to Rhino.</summary>
    /// <param name="curve">A curve. A duplicate of this curve is added to Rhino.</param>
    /// <returns>A unique identifier for the object.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addnurbscircle.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addnurbscircle.cs' lang='cs'/>
    /// <code source='examples\py\ex_addnurbscircle.py' lang='py'/>
    /// </example>
    public Guid AddCurve(Geometry.Curve curve)
    {
      return AddCurve(curve, null);
    }
    /// <summary>Adds a curve object to Rhino.</summary>
    /// <param name="curve">A curve. A duplicate of this curve is added to Rhino.</param>
    /// <param name="attributes">Attributes to apply to curve.</param>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddCurve(Geometry.Curve curve, DocObjects.ObjectAttributes attributes)
    {
      return AddCurve(curve, attributes, null, false);
    }

    public Guid AddCurve(Geometry.Curve curve, DocObjects.ObjectAttributes attributes, DocObjects.HistoryRecord history, bool reference)
    {
      if (null == curve)
        return Guid.Empty;
      IntPtr pAttributes = (attributes == null) ? IntPtr.Zero : attributes.ConstPointer();
      IntPtr pHistory = (history == null) ? IntPtr.Zero : history.Handle;
      IntPtr curvePtr = curve.ConstPointer();
      return UnsafeNativeMethods.CRhinoDoc_AddCurve(m_doc.m_docId, curvePtr, pAttributes, pHistory, reference);
    }

    /// <summary>Adds a text dot object to Rhino.</summary>
    /// <param name="text">A text string.</param>
    /// <param name="location">A point position.</param>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddTextDot(string text, Point3d location)
    {
      return AddTextDot(text, location, null);
    }
    /// <summary>Adds a text dot object to Rhino.</summary>
    /// <param name="text">A text string.</param>
    /// <param name="location">A point position.</param>
    /// <param name="attributes">Attributes to apply to curve.</param>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddTextDot(string text, Point3d location, DocObjects.ObjectAttributes attributes)
    {
      using (Geometry.TextDot dot = new Rhino.Geometry.TextDot(text, location))
      {
        Guid rc = AddTextDot(dot, attributes);
        dot.Dispose();
        return rc;
      }
    }
    /// <summary>Adds a text dot object to Rhino.</summary>
    /// <param name="dot">A text dot that will be copied.</param>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddTextDot(Geometry.TextDot dot)
    {
      return AddTextDot(dot, null);
    }
    /// <summary>Adds a text dot object to Rhino.</summary>
    /// <param name="dot">A text dot that will be copied.</param>
    /// <param name="attributes">Attributes to apply to text dot.</param>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddTextDot(Geometry.TextDot dot, DocObjects.ObjectAttributes attributes)
    {
      return AddTextDot(dot, attributes, null, false);
    }

    public Guid AddTextDot(Geometry.TextDot dot, DocObjects.ObjectAttributes attributes, DocObjects.HistoryRecord history, bool reference)
    {
      if (null == dot)
        return Guid.Empty;
      IntPtr pConstDot = dot.ConstPointer();
      IntPtr pAttributes = (attributes == null) ? IntPtr.Zero : attributes.ConstPointer();
      IntPtr pHistory = (history == null) ? IntPtr.Zero : history.Handle;
      return UnsafeNativeMethods.CRhinoDoc_AddTextDot(m_doc.m_docId, pConstDot, pAttributes, pHistory, reference);
    }

    /// <summary>
    /// Adds an annotation text object to the document.
    /// </summary>
    /// <param name="text3d">The text object to add.</param>
    /// <returns>The Guid of the newly added object or Guid.Empty on failure.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_textjustify.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_textjustify.cs' lang='cs'/>
    /// <code source='examples\py\ex_textjustify.py' lang='py'/>
    /// </example>
    public Guid AddText(Rhino.Display.Text3d text3d)
    {
      return AddText(text3d.Text, text3d.TextPlane, text3d.Height, text3d.FontFace, text3d.Bold, text3d.Italic);
    }
    /// <summary>
    /// Adds an annotation text object to the document.
    /// </summary>
    /// <param name="text3d">The text object to add.</param>
    /// <param name="attributes">Object Attributes.</param>
    /// <returns>The Guid of the newly added object or Guid.Empty on failure.</returns>
    public Guid AddText(Rhino.Display.Text3d text3d, DocObjects.ObjectAttributes attributes)
    {
      return AddText(text3d.Text, text3d.TextPlane, text3d.Height, text3d.FontFace, text3d.Bold, text3d.Italic, attributes);
    }
    /// <summary>
    /// Adds an annotation text object to the document.
    /// </summary>
    /// <param name="text">Text string.</param>
    /// <param name="plane">Plane of text.</param>
    /// <param name="height">Height of text.</param>
    /// <param name="fontName">Name of FontFace.</param>
    /// <param name="bold">Bold flag.</param>
    /// <param name="italic">Italic flag.</param>
    /// <returns>The Guid of the newly added object or Guid.Empty on failure.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addtext.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addtext.cs' lang='cs'/>
    /// <code source='examples\py\ex_addtext.py' lang='py'/>
    /// </example>
    public Guid AddText(string text, Plane plane, double height, string fontName, bool bold, bool italic)
    {
      return AddText(text, plane, height, fontName, bold, italic, null);
    }

    public Guid AddText(string text, Plane plane, double height, string fontName, bool bold, bool italic, TextJustification justification)
    {
      return AddText(text, plane, height, fontName, bold, italic, justification, null);
    }

    public Guid AddText(string text, Plane plane, double height, string fontName, bool bold, bool italic, TextJustification justification, DocObjects.ObjectAttributes attributes)
    {
      return AddText(text, plane, height, fontName, bold, italic, justification, attributes, null, false);
    }

    public Guid AddText(string text, Plane plane, double height, string fontName, bool bold, bool italic, TextJustification justification, DocObjects.ObjectAttributes attributes, DocObjects.HistoryRecord history, bool reference)
    {
      if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(fontName))
        return Guid.Empty;
      int fontStyle = 0;
      if (bold)
        fontStyle |= 1;
      if (italic)
        fontStyle |= 2;
      IntPtr pConstAttributes = (attributes == null) ? IntPtr.Zero : attributes.ConstPointer();
      IntPtr pHistory = (history == null) ? IntPtr.Zero : history.Handle;
      return UnsafeNativeMethods.CRhinoDoc_AddText(m_doc.m_docId, text, ref plane, height, fontName, fontStyle, (int)justification, pConstAttributes, pHistory, reference);
    }

    /// <summary>
    /// Adds an annotation text object to the document.
    /// </summary>
    /// <param name="text">Text string.</param>
    /// <param name="plane">Plane of text.</param>
    /// <param name="height">Height of text.</param>
    /// <param name="fontName">Name of FontFace.</param>
    /// <param name="bold">Bold flag.</param>
    /// <param name="italic">Italic flag.</param>
    /// <param name="attributes">Attributes that will be linked with the object.</param>
    /// <returns>The Guid of the newly added object or Guid.Empty on failure.</returns>
    public Guid AddText(string text, Plane plane, double height, string fontName, bool bold, bool italic, DocObjects.ObjectAttributes attributes)
    {
      return AddText(text, plane, height, fontName, bold, italic, attributes, null, false);
    }

    public Guid AddText(string text, Plane plane, double height, string fontName, bool bold, bool italic, DocObjects.ObjectAttributes attributes, DocObjects.HistoryRecord history, bool reference)
    {
      if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(fontName))
        return Guid.Empty;
      int fontStyle = 0;
      if (bold)
        fontStyle |= 1;
      if (italic)
        fontStyle |= 2;
      IntPtr pConstAttributes = (attributes == null) ? IntPtr.Zero : attributes.ConstPointer();
      IntPtr pHistory = (history == null) ? IntPtr.Zero : history.Handle;
      return UnsafeNativeMethods.CRhinoDoc_AddText(m_doc.m_docId, text, ref plane, height, fontName, fontStyle, -1, pConstAttributes, pHistory, reference);
    }

    public Guid AddText(Rhino.Geometry.TextEntity text)
    {
      return AddText(text, null);
    }
    public Guid AddText(Rhino.Geometry.TextEntity text, DocObjects.ObjectAttributes attributes)
    {
      return AddText(text, attributes, null, false);
    }

    public Guid AddText(Rhino.Geometry.TextEntity text, DocObjects.ObjectAttributes attributes, DocObjects.HistoryRecord history, bool reference)
    {
      IntPtr pConstTextEntity = text.ConstPointer();
      IntPtr pConstAttributes = (attributes == null) ? IntPtr.Zero : attributes.ConstPointer();
      IntPtr pHistory = (history == null) ? IntPtr.Zero : history.Handle;
      return UnsafeNativeMethods.CRhinoDoc_AddText2(m_doc.m_docId, pConstTextEntity, pConstAttributes, pHistory, reference);
    }

    /// <summary>Adds a surface object to Rhino.</summary>
    /// <param name="surface">A duplicate of this surface is added to Rhino.</param>
    /// <returns>A unique identifier for the object.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addtorus.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addtorus.cs' lang='cs'/>
    /// <code source='examples\py\ex_addtorus.py' lang='py'/>
    /// </example>
    public Guid AddSurface(Geometry.Surface surface)
    {
      return AddSurface(surface, null);
    }
    /// <summary>Adds a surface object to Rhino.</summary>
    /// <param name="surface">A duplicate of this surface is added to Rhino.</param>
    /// <param name="attributes">Attributes that will be linked with the surface object.</param>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddSurface(Geometry.Surface surface, DocObjects.ObjectAttributes attributes)
    {
      return AddSurface(surface, attributes, null, false);
    }

    public Guid AddSurface(Geometry.Surface surface, DocObjects.ObjectAttributes attributes, DocObjects.HistoryRecord history, bool reference)
    {
      if (null == surface)
        return Guid.Empty;
      IntPtr pConstAttributes = (attributes == null) ? IntPtr.Zero : attributes.ConstPointer();
      IntPtr pHistory = (history == null) ? IntPtr.Zero : history.Handle;
      IntPtr surfacePtr = surface.ConstPointer();
      return UnsafeNativeMethods.CRhinoDoc_AddSurface(m_doc.m_docId, surfacePtr, pConstAttributes, pHistory, reference);
    }

    /// <summary>Adds an extrusion object to Rhino.</summary>
    /// <param name="extrusion">A duplicate of this extrusion is added to Rhino.</param>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddExtrusion(Geometry.Extrusion extrusion)
    {
      return AddExtrusion(extrusion, null);
    }
    /// <summary>Adds an extrusion object to Rhino.</summary>
    /// <param name="extrusion">A duplicate of this extrusion is added to Rhino.</param>
    /// <param name="attributes">Attributes that will be linked with the extrusion object.</param>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddExtrusion(Geometry.Extrusion extrusion, DocObjects.ObjectAttributes attributes)
    {
      return AddExtrusion(extrusion, attributes, null, false);
    }

    public Guid AddExtrusion(Geometry.Extrusion extrusion, DocObjects.ObjectAttributes attributes, DocObjects.HistoryRecord history, bool reference)
    {
      if (null == extrusion)
        return Guid.Empty;
      IntPtr pConstExtrusion = extrusion.ConstPointer();
      IntPtr pConstAttributes = (attributes == null) ? IntPtr.Zero : attributes.ConstPointer();
      IntPtr pHistory = (history == null) ? IntPtr.Zero : history.Handle;
      return UnsafeNativeMethods.CRhinoDoc_AddExtrusion(m_doc.m_docId, pConstExtrusion, pConstAttributes, pHistory, reference);
    }

    /// <summary>Adds a mesh object to Rhino.</summary>
    /// <param name="mesh">A duplicate of this mesh is added to Rhino.</param>
    /// <returns>A unique identifier for the object.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addmesh.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addmesh.cs' lang='cs'/>
    /// <code source='examples\py\ex_addmesh.py' lang='py'/>
    /// </example>
    public Guid AddMesh(Geometry.Mesh mesh)
    {
      return AddMesh(mesh, null);
    }
    /// <summary>Adds a mesh object to Rhino.</summary>
    /// <param name="mesh">A duplicate of this mesh is added to Rhino.</param>
    /// <param name="attributes">Attributes that will be linked with the mesh object.</param>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddMesh(Geometry.Mesh mesh, DocObjects.ObjectAttributes attributes)
    {
      return AddMesh(mesh, attributes, null, false);
    }

    public Guid AddMesh(Geometry.Mesh mesh, DocObjects.ObjectAttributes attributes, HistoryRecord history, bool reference)
    {
      if (null == mesh)
        return Guid.Empty;
      IntPtr pConstAttributes = (attributes == null) ? IntPtr.Zero : attributes.ConstPointer();
      IntPtr pHistory = (history == null) ? IntPtr.Zero : history.Handle;
      IntPtr meshPtr = mesh.ConstPointer();
      return UnsafeNativeMethods.CRhinoDoc_AddMesh(m_doc.m_docId, meshPtr, pConstAttributes, pHistory, reference);
    }

    /// <summary>Adds a brep object to Rhino.</summary>
    /// <param name="brep">A duplicate of this brep is added to Rhino.</param>
    /// <returns>A unique identifier for the object.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addbrepbox.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addbrepbox.cs' lang='cs'/>
    /// <code source='examples\py\ex_addbrepbox.py' lang='py'/>
    /// </example>
    public Guid AddBrep(Geometry.Brep brep)
    {
      return AddBrep(brep, null);
    }
    /// <summary>Adds a brep object to Rhino.</summary>
    /// <param name="brep">A duplicate of this brep is added to Rhino.</param>
    /// <param name="attributes">attributes to apply to brep.</param>
    /// <returns>A unique identifier for the object.</returns>
    public Guid AddBrep(Geometry.Brep brep, DocObjects.ObjectAttributes attributes)
    {
      return AddBrep(brep, attributes, null, false);
    }

    public Guid AddBrep(Geometry.Brep brep, DocObjects.ObjectAttributes attributes, DocObjects.HistoryRecord history, bool reference)
    {
      if (null == brep)
        return Guid.Empty;
      IntPtr brepPtr = brep.ConstPointer();
      IntPtr pConstAttributes = (attributes == null) ? IntPtr.Zero : attributes.ConstPointer();
      IntPtr pHistory = (history == null) ? IntPtr.Zero : history.Handle;
      return UnsafeNativeMethods.CRhinoDoc_AddBrep(m_doc.m_docId, brepPtr, pConstAttributes, pHistory, reference, -1);
    }

    public Guid AddBrep(Geometry.Brep brep, DocObjects.ObjectAttributes attributes, DocObjects.HistoryRecord history, bool reference, bool splitKinkySurfaces)
    {
      if (null == brep)
        return Guid.Empty;
      IntPtr brepPtr = brep.ConstPointer();
      IntPtr pConstAttributes = (attributes == null) ? IntPtr.Zero : attributes.ConstPointer();
      IntPtr pHistory = (history == null) ? IntPtr.Zero : history.Handle;
      return UnsafeNativeMethods.CRhinoDoc_AddBrep(m_doc.m_docId, brepPtr, pConstAttributes, pHistory, reference, splitKinkySurfaces?1:0);
    }

    public Guid[] AddExplodedInstancePieces(InstanceObject instance)
    {
      IntPtr pRhinoObject = instance.ConstPointer();
      using (var ids = new Rhino.Runtime.InteropWrappers.SimpleArrayGuid())
      {
        IntPtr pGuids = ids.NonConstPointer();
        UnsafeNativeMethods.CRhinoDoc_AddExplodedInstancePieces(m_doc.m_docId, pRhinoObject, pGuids);
        return ids.ToArray();
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="instanceDefinitionIndex"></param>
    /// <param name="instanceXform"></param>
    /// <returns></returns>
    /// <example>
    /// <code source='examples\cs\ex_nestedblock.cs' lang='cs'/>
    /// </example>
    public Guid AddInstanceObject(int instanceDefinitionIndex, Transform instanceXform)
    {
      return UnsafeNativeMethods.CRhinoDoc_AddInstanceObject(m_doc.m_docId, instanceDefinitionIndex, ref instanceXform, IntPtr.Zero);
    }

    public Guid AddInstanceObject(int instanceDefinitionIndex, Transform instanceXform, DocObjects.ObjectAttributes attributes)
    {
      if (null == attributes)
        return AddInstanceObject(instanceDefinitionIndex, instanceXform);
      IntPtr pAttributes = attributes.ConstPointer();
      return UnsafeNativeMethods.CRhinoDoc_AddInstanceObject(m_doc.m_docId, instanceDefinitionIndex, ref instanceXform, pAttributes);
    }

    /// <example>
    /// <code source='examples\vbnet\ex_leader.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_leader.cs' lang='cs'/>
    /// <code source='examples\py\ex_leader.py' lang='py'/>
    /// </example>
    public Guid AddLeader(Plane plane, IEnumerable<Point2d> points)
    {
      return AddLeader(null, plane, points);
    }
    public Guid AddLeader(Plane plane, IEnumerable<Point2d> points, DocObjects.ObjectAttributes attributes)
    {
      return AddLeader(null, plane, points, attributes);
    }

    public Guid AddLeader(string text, Plane plane, IEnumerable<Point2d> points, DocObjects.ObjectAttributes attributes)
    {
      return AddLeader(text, plane, points, attributes, null, false);
    }

    public Guid AddLeader(string text, Plane plane, IEnumerable<Point2d> points, DocObjects.ObjectAttributes attributes, DocObjects.HistoryRecord history, bool reference)
    {
      string s = null;
      if (!string.IsNullOrEmpty(text))
        s = text;
      Rhino.Collections.RhinoList<Point2d> pts = new Rhino.Collections.RhinoList<Point2d>(points);
      int count = pts.Count;
      if (count < 1)
        return Guid.Empty;
      IntPtr pConstAttributes = (attributes == null) ? IntPtr.Zero : attributes.ConstPointer();
      IntPtr pHistory = (history == null) ? IntPtr.Zero : history.Handle;
      return UnsafeNativeMethods.CRhinoDoc_AddLeader(m_doc.m_docId, s, ref plane, count, pts.m_items, pConstAttributes, pHistory, reference);
    }

    public Guid AddLeader(string text, Plane plane, IEnumerable<Point2d> points)
    {
      return AddLeader(text, plane, points, null);
    }

    public Guid AddLeader(string text, IEnumerable<Point3d> points)
    {
      Plane plane;
      //double max_deviation;
      PlaneFitResult rc = Plane.FitPlaneToPoints(points, out plane);//, out max_deviation);
      if (rc != PlaneFitResult.Success)
        return Guid.Empty;

      Rhino.Collections.RhinoList<Point2d> points2d = new Rhino.Collections.RhinoList<Point2d>();
      foreach (Point3d point3d in points)
      {
        double s, t;
        if (plane.ClosestParameter(point3d, out s, out t))
        {
          Point2d newpoint = new Point2d(s, t);
          if (points2d.Count > 0 && points2d.Last.DistanceTo(newpoint) < Rhino.RhinoMath.SqrtEpsilon)
            continue;
          points2d.Add(new Point2d(s, t));
        }
      }
      return AddLeader(text, plane, points2d);
    }
    public Guid AddLeader(IEnumerable<Point3d> points)
    {
      return AddLeader(null, points);
    }

    /// <example>
    /// <code source='examples\vbnet\ex_hatchcurve.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_hatchcurve.cs' lang='cs'/>
    /// <code source='examples\py\ex_hatchcurve.py' lang='py'/>
    /// </example>
    public Guid AddHatch(Hatch hatch)
    {
      return AddHatch(hatch, null);
    }
    public Guid AddHatch(Hatch hatch, ObjectAttributes attributes)
    {
      return AddHatch(hatch, attributes, null, false);
    }

    public Guid AddHatch(Hatch hatch, ObjectAttributes attributes, HistoryRecord history, bool reference)
    {
      IntPtr pConstHatch = hatch.ConstPointer();
      IntPtr pConstAttributes = (attributes == null) ? IntPtr.Zero : attributes.ConstPointer();
      IntPtr pHistory = (history == null) ? IntPtr.Zero : history.Handle;
      return UnsafeNativeMethods.CRhinoDoc_AddHatch(m_doc.m_docId, pConstHatch, pConstAttributes, pHistory, reference);
    }

    public Guid AddMorphControl(MorphControl morphControl)
    {
      return AddMorphControl(morphControl, null);
    }

    public Guid AddMorphControl(MorphControl morphControl, ObjectAttributes attributes)
    {
      IntPtr pConstMorph = morphControl.ConstPointer();
      IntPtr pAttributes = IntPtr.Zero;
      if (attributes != null)
        pAttributes = attributes.ConstPointer();
      return UnsafeNativeMethods.CRhinoDoc_AddMorphControl(m_doc.m_docId, pConstMorph, pAttributes);
    }

    //public Guid AddMorphControl(MorphControl morphControl, IEnumerable<RhinoObject> captives)
    #endregion

#region Object deletion
    /// <summary>
    /// Deletes objref.Object(). The deletion can be undone by calling UndeleteObject(). 
    /// </summary>
    /// <param name="objref">objref.Object() will be deleted.</param>
    /// <param name="quiet">If false, a message box will appear when an object cannot be deleted.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool Delete(DocObjects.ObjRef objref, bool quiet)
    {
      if (null == objref)
        return false;
      IntPtr pObjRef = objref.ConstPointer();
      return UnsafeNativeMethods.CRhinoDoc_DeleteObject(m_doc.m_docId, pObjRef, quiet);
    }
    /// <summary>
    /// Deletes object from document. The deletion can be undone by calling UndeleteObject(). 
    /// </summary>
    /// <param name="obj">The object to delete.</param>
    /// <param name="quiet">If false, a message box will appear when an object cannot be deleted.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool Delete(DocObjects.RhinoObject obj, bool quiet)
    {
      if (null == obj)
        return false;
      DocObjects.ObjRef objref = new Rhino.DocObjects.ObjRef(obj);
      bool rc = Delete(objref, quiet);
      objref.Dispose();
      return rc;
    }
    /// <summary>
    /// Deletes object from document. The deletion can be undone by calling UndeleteObject(). 
    /// </summary>
    /// <param name="objectId">Id of the object to delete.</param>
    /// <param name="quiet">If false, a message box will appear when an object cannot be deleted.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool Delete(Guid objectId, bool quiet)
    {
      DocObjects.ObjRef objref = new Rhino.DocObjects.ObjRef(objectId);
      bool rc = Delete(objref, quiet);
      objref.Dispose();
      return rc;
    }
    /// <summary>
    /// Deletes a collection of objects from the document.
    /// </summary>
    /// <param name="objectIds">Ids of all objects to delete.</param>
    /// <param name="quiet">If false, a message box will appear when an object cannot be deleted.</param>
    /// <returns>The number of successfully deleted objects.</returns>
    public int Delete(IEnumerable<Guid> objectIds, bool quiet)
    {
      if (objectIds == null) { throw new ArgumentNullException("objectIds"); }

      int count = 0;
      foreach (Guid id in objectIds)
      {
        if (Delete(id, quiet)) { count++; }
      }
      return count;
    }

    /// <summary>
    /// Removes object from document and deletes the pointer. Typically you will
    /// want to call Delete instead in order to keep the object on the undo list.
    /// </summary>
    /// <param name="runtimeSerialNumber">A runtime serial number of the object that will be deleted.</param>
    /// <returns>true if the object was purged; otherwise false.</returns>
    [CLSCompliant(false)]
    public bool Purge(uint runtimeSerialNumber)
    {
      return UnsafeNativeMethods.CRhinoDoc_PurgeObject(m_doc.m_docId, runtimeSerialNumber);
    }

    /// <summary>
    /// Removes object from document and deletes the pointer. Typically you will
    /// want to call Delete instead in order to keep the object on the undo list.
    /// </summary>
    /// <param name="rhinoObject">A Rhino object that will be deleted.</param>
    /// <returns>true if the object was purged; otherwise false.</returns>
    public bool Purge(Rhino.DocObjects.RhinoObject rhinoObject)
    {
      return Purge(rhinoObject.RuntimeSerialNumber);
    }

    [CLSCompliant(false)]
    public bool Undelete(uint runtimeSerialNumber)
    {
      return UnsafeNativeMethods.CRhinoDoc_UndeleteObject(m_doc.m_docId, runtimeSerialNumber);
    }

    public bool Undelete(Rhino.DocObjects.RhinoObject rhinoObject)
    {
      return Undelete(rhinoObject.RuntimeSerialNumber);
    }
    #endregion

#region Object selection
    /// <summary>
    /// Select a single object.
    /// </summary>
    /// <param name="objref">Object represented by this ObjRef is selected.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool Select(DocObjects.ObjRef objref)
    {
      return Select(objref, true);
    }
    /// <summary>
    /// Select or deselects a single object.
    /// </summary>
    /// <param name="objref">Object represented by this ObjRef is selected.</param>
    /// <param name="select">If true, the object will be selected, if false, it will be deselected.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool Select(DocObjects.ObjRef objref, bool select)
    {
      return Select(objref, select, true);
    }
    /// <summary>
    /// Select or deselects a single object.
    /// </summary>
    /// <param name="objref">Object represented by this ObjRef is selected.</param>
    /// <param name="select">If true, the object will be selected, if false, it will be deselected.</param>
    /// <param name="syncHighlight">
    /// If true, then the object is highlighted if it is selected 
    /// and unhighlighted if is is not selected.
    /// </param>
    /// <returns>true on success, false on failure.</returns>
    public bool Select(DocObjects.ObjRef objref, bool select, bool syncHighlight)
    {
      return Select(objref, select, syncHighlight, true);
    }
    /// <summary>
    /// Select or deselects a single object.
    /// </summary>
    /// <param name="objref">Object represented by this ObjRef is selected.</param>
    /// <param name="select">If true, the object will be selected, if false, it will be deselected.</param>
    /// <param name="syncHighlight">
    /// If true, then the object is highlighted if it is selected 
    /// and unhighlighted if is is not selected.
    /// </param>
    /// <param name="persistentSelect">
    /// Objects that are persistently selected stay selected when a command terminates.
    /// </param>
    /// <returns>true on success, false on failure.</returns>
    public bool Select(DocObjects.ObjRef objref, bool select, bool syncHighlight, bool persistentSelect)
    {
      return Select(objref, select, syncHighlight, persistentSelect, false, false, false);
    }
    /// <summary>
    /// Select or deselects a single object.
    /// </summary>
    /// <param name="objref">Object represented by this ObjRef is selected.</param>
    /// <param name="select">If true, the object will be selected, if false, it will be deselected.</param>
    /// <param name="syncHighlight">
    /// If true, then the object is highlighted if it is selected 
    /// and unhighlighted if is is not selected.
    /// </param>
    /// <param name="persistentSelect">
    /// Objects that are persistently selected stay selected when a command terminates.
    /// </param>
    /// <param name="ignoreGripsState">
    /// If true, then objects with grips on can be selected.
    /// If false, then the value returned by the object's IsSelectableWithGripsOn() function
    /// decides if the object can be selected when it has grips turned on.
    /// </param>
    /// <param name="ignoreLayerLocking">
    /// If true, then objects on locked layers can be selected. 
    /// </param>
    /// <param name="ignoreLayerVisibility">
    /// If true, then objects on hidden layers can be selectable.
    /// </param>
    /// <returns>true on success, false on failure.</returns>
    public bool Select(DocObjects.ObjRef objref, bool select, bool syncHighlight, bool persistentSelect, bool ignoreGripsState, bool ignoreLayerLocking, bool ignoreLayerVisibility)
    {
      if (objref == null) { throw new ArgumentNullException("objref"); }
      return objref.Object().Select(select, syncHighlight, persistentSelect, ignoreGripsState, ignoreLayerLocking, ignoreLayerVisibility) != 0;
    }

    /// <summary>
    /// Selects a collection of objects.
    /// </summary>
    /// <param name="objRefs">References to objects to select.</param>
    /// <returns>Number of objects successfully selected.</returns>
    public int Select(IEnumerable<DocObjects.ObjRef> objRefs)
    {
      return Select(objRefs, true);
    }
    /// <summary>
    /// Selects or deselects a collection of objects.
    /// </summary>
    /// <param name="objRefs">References to objects to select or deselect.</param>
    /// <param name="select">
    /// If true, objects will be selected. 
    /// If false, objects will be deselected.
    /// </param>
    /// <returns>Number of objects successfully selected or deselected.</returns>
    public int Select(IEnumerable<DocObjects.ObjRef> objRefs, bool select)
    {
      if (objRefs == null) { throw new ArgumentNullException("objRefs"); }
      int count = 0;
      foreach (DocObjects.ObjRef objref in objRefs)
      {
        if (Select(objref, select)) { count++; }
      }
      return count;
    }

    /// <summary>
    /// Select a single object.
    /// </summary>
    /// <param name="objectId">Id of object to select.</param>
    /// <returns>true on success, false on failure.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_dividebylength.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_dividebylength.cs' lang='cs'/>
    /// <code source='examples\py\ex_dividebylength.py' lang='py'/>
    /// </example>
    public bool Select(Guid objectId)
    {
      ObjRef objref = new ObjRef(objectId);
      return Select(objref);
    }
    /// <summary>
    /// Select or deselects a single object.
    /// </summary>
    /// <param name="objectId">Id of object to select.</param>
    /// <param name="select">If true, the object will be selected, if false, it will be deselected.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool Select(Guid objectId, bool select)
    {
      ObjRef objref = new ObjRef(objectId);
      return Select(objref, select);
    }
    /// <summary>
    /// Select or deselects a single object.
    /// </summary>
    /// <param name="objectId">Id of object to select.</param>
    /// <param name="select">If true, the object will be selected, if false, it will be deselected.</param>
    /// <param name="syncHighlight">
    /// If true, then the object is highlighted if it is selected 
    /// and unhighlighted if is is not selected.
    /// </param>
    /// <returns>true on success, false on failure.</returns>
    public bool Select(Guid objectId, bool select, bool syncHighlight)
    {
      ObjRef objref = new ObjRef(objectId);
      return Select(objref, select, syncHighlight);
    }
    /// <summary>
    /// Select or deselects a single object.
    /// </summary>
    /// <param name="objectId">Id of object to select.</param>
    /// <param name="select">If true, the object will be selected, if false, it will be deselected.</param>
    /// <param name="syncHighlight">
    /// If true, then the object is highlighted if it is selected 
    /// and unhighlighted if is is not selected.
    /// </param>
    /// <param name="persistentSelect">
    /// Objects that are persistently selected stay selected when a command terminates.
    /// </param>
    /// <returns>true on success, false on failure.</returns>
    public bool Select(Guid objectId, bool select, bool syncHighlight, bool persistentSelect)
    {
      ObjRef objref = new ObjRef(objectId);
      return Select(objref, select, syncHighlight, persistentSelect);
    }
    /// <summary>
    /// Select or deselects a single object.
    /// </summary>
    /// <param name="objectId">Id of object to select.</param>
    /// <param name="select">If true, the object will be selected, if false, it will be deselected.</param>
    /// <param name="syncHighlight">
    /// If true, then the object is highlighted if it is selected 
    /// and unhighlighted if is is not selected.
    /// </param>
    /// <param name="persistentSelect">
    /// Objects that are persistently selected stay selected when a command terminates.
    /// </param>
    /// <param name="ignoreGripsState">
    /// If true, then objects with grips on can be selected.
    /// If false, then the value returned by the object's IsSelectableWithGripsOn() function
    /// decides if the object can be selected when it has grips turned on.
    /// </param>
    /// <param name="ignoreLayerLocking">
    /// If true, then objects on locked layers can be selected. 
    /// </param>
    /// <param name="ignoreLayerVisibility">
    /// If true, then objects on hidden layers can be selectable.
    /// </param>
    /// <returns>true on success, false on failure.</returns>
    public bool Select(Guid objectId, bool select, bool syncHighlight, bool persistentSelect, bool ignoreGripsState, bool ignoreLayerLocking, bool ignoreLayerVisibility)
    {
      ObjRef objref = new ObjRef(objectId);
      return Select(objref, select, syncHighlight, persistentSelect, ignoreGripsState, ignoreLayerLocking, ignoreLayerVisibility);
    }

    /// <summary>
    /// Selects a collection of objects.
    /// </summary>
    /// <param name="objectIds">Ids of objects to select.</param>
    /// <returns>Number of objects successfully selected.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_curvesurfaceintersect.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_curvesurfaceintersect.cs' lang='cs'/>
    /// <code source='examples\py\ex_curvesurfaceintersect.py' lang='py'/>
    /// </example>
    public int Select(IEnumerable<Guid> objectIds)
    {
      return Select(objectIds, true);
    }
    /// <summary>
    /// Selects or deselects a collection of objects.
    /// </summary>
    /// <param name="objectIds">Ids of objects to select or deselect.</param>
    /// <param name="select">
    /// If true, objects will be selected. 
    /// If false, objects will be deselected.
    /// </param>
    /// <returns>Number of objects successfully selected or deselected.</returns>
    public int Select(IEnumerable<Guid> objectIds, bool select)
    {
      if (objectIds == null) { throw new ArgumentNullException("objectIds"); }
      int count = 0;
      foreach (Guid objectId in objectIds)
      {
        if (Select(objectId, select)) { count++; }
      }
      return count;
    }

    /// <summary>Unselect objects.</summary>
    /// <param name="ignorePersistentSelections">
    /// if true, then objects that are persistently selected will not be unselected.
    /// </param>
    /// <returns>Number of object that were unselected.</returns>
    public int UnselectAll(bool ignorePersistentSelections)
    {
      return UnsafeNativeMethods.CRhinoDoc_UnselectAll(m_doc.m_docId, ignorePersistentSelections);
    }
    /// <summary>Unselect objects.</summary>
    /// <returns>Number of object that were unselected.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_crvdeviation.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_crvdeviation.cs' lang='cs'/>
    /// <code source='examples\py\ex_crvdeviation.py' lang='py'/>
    /// </example>
    public int UnselectAll()
    {
      return UnselectAll(false);
    }
    #endregion

#region Object replacement
    /// <summary>
    /// Modifies an object's attributes.  Cannot be used to change object id.
    /// </summary>
    /// <param name="objref">reference to object to modify.</param>
    /// <param name="newAttributes">new attributes.</param>
    /// <param name="quiet">if true, then warning message boxes are disabled.</param>
    /// <returns>true if successful.</returns>
    public bool ModifyAttributes(DocObjects.ObjRef objref, DocObjects.ObjectAttributes newAttributes, bool quiet)
    {
      if (null == objref || null == newAttributes)
        return false;
      IntPtr pObjRef = objref.ConstPointer();
      IntPtr pAttrs = newAttributes.ConstPointer();
      return UnsafeNativeMethods.CRhinoDoc_ModifyObjectAttributes(m_doc.m_docId, pObjRef, pAttrs, quiet);
    }
    /// <summary>
    /// Modifies an object's attributes.  Cannot be used to change object id.
    /// </summary>
    /// <param name="obj">object to modify.</param>
    /// <param name="newAttributes">new attributes.</param>
    /// <param name="quiet">if true, then warning message boxes are disabled.</param>
    /// <returns>true if successful.</returns>
    public bool ModifyAttributes(DocObjects.RhinoObject obj, DocObjects.ObjectAttributes newAttributes, bool quiet)
    {
      if (null == obj || null == newAttributes)
        return false;
      return ModifyAttributes(obj.Id, newAttributes, quiet);
    }
    /// <summary>
    /// Modifies an object's attributes.  Cannot be used to change object id.
    /// </summary>
    /// <param name="objectId">Id of object to modify.</param>
    /// <param name="newAttributes">new attributes.</param>
    /// <param name="quiet">if true, then warning message boxes are disabled.</param>
    /// <returns>true if successful.</returns>
    public bool ModifyAttributes(Guid objectId, DocObjects.ObjectAttributes newAttributes, bool quiet)
    {
      if (Guid.Empty == objectId || null == newAttributes)
        return false;
      DocObjects.ObjRef objref = new Rhino.DocObjects.ObjRef(objectId);
      IntPtr pObjRef = objref.ConstPointer();
      IntPtr pAttrs = newAttributes.ConstPointer();
      bool rc = UnsafeNativeMethods.CRhinoDoc_ModifyObjectAttributes(m_doc.m_docId, pObjRef, pAttrs, quiet);
      objref.Dispose();
      return rc;
    }

#if RDK_CHECKED
    /// <summary>
    /// Modifies an object's render material assignment, this will set the
    /// objects material source to ObjectMaterialSource.MaterialFromObject.
    /// </summary>
    /// <param name="obj">Object to modify.</param>
    /// <param name="material">
    /// Material to assign to this object.
    /// </param>
    /// <returns>
    /// Returns true on success otherwise returns false.
    /// </returns>
    public bool ModifyRenderMaterial(RhinoObject obj, RenderMaterial material)
    {
      if (null == obj) return false;
      var material_id = (material == null ? Guid.Empty : material.Id);
      var pointer = obj.ConstPointer();
      var success = UnsafeNativeMethods.Rdk_RenderContent_SetObjectMaterialInstanceid(pointer, material_id);
      return (success != 0);
    }
    /// <summary>
    /// Modifies an object's render material assignment, this will set the
    /// objects material source to ObjectMaterialSource.MaterialFromObject.
    /// </summary>
    /// <param name="objRef">Object to modify.</param>
    /// <param name="material">
    /// Material to assign to this object.
    /// </param>
    /// <returns>
    /// Returns true on success otherwise returns false.
    /// </returns>
    public bool ModifyRenderMaterial(ObjRef objRef, RenderMaterial material)
    {
      if (null == objRef) return false;
      var obj = objRef.Object();
      return ModifyRenderMaterial(obj, material);
    }
    /// <summary>
    /// Modifies an object's render material assignment, this will set the
    /// objects material source to ObjectMaterialSource.MaterialFromObject.
    /// </summary>
    /// <param name="objectId">Id of object to modify.</param>
    /// <param name="material">
    /// Material to assign to this object.
    /// </param>
    /// <returns>
    /// Returns true on success otherwise returns false.
    /// </returns>
    public bool ModifyRenderMaterial(Guid objectId, RenderMaterial material)
    {
      var objref = new ObjRef(objectId);
      var obj = objref.Object();
      return ModifyRenderMaterial(obj, material);
    }
#endif
    /// <summary>
    /// 
    /// </summary>
    /// <param name="objRef"></param>
    /// <param name="channel"></param>
    /// <param name="mapping"></param>
    /// <returns></returns>
    public bool ModifyTextureMapping(ObjRef objRef, int channel, TextureMapping mapping)
    {
      var obj = (objRef == null ? null : objRef.Object());
      return ModifyTextureMapping(obj, channel, mapping);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="objId"></param>
    /// <param name="channel"></param>
    /// <param name="mapping"></param>
    /// <returns></returns>
    public bool ModifyTextureMapping(Guid objId, int channel, TextureMapping mapping)
    {
      var obj_ref = new ObjRef(objId);
      return ModifyTextureMapping(obj_ref, channel, mapping);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="channel"></param>
    /// <param name="mapping"></param>
    /// <returns></returns>
    public bool ModifyTextureMapping(RhinoObject obj, int channel, TextureMapping mapping)
    {
      if (null == obj) return false;
      var pointer = obj.ConstPointer();
      var mapping_pointer = (null == mapping ? IntPtr.Zero : mapping.ConstPointer());
      var success = UnsafeNativeMethods.ON_TextureMapping_SetObjectMapping(pointer, channel, mapping_pointer);
      return (success != 0);
    }
    /// <summary>
    /// Replaces one object with another. Conceptually, this function is the same as calling
    /// Setting new_object attributes = old_object attributes
    /// DeleteObject(old_object);
    /// AddObject(old_object);
    /// </summary>
    /// <param name="objref">reference to old object to be replaced. The objref.Object() will be deleted.</param>
    /// <param name="newObject">new replacement object - must not be in document.</param>
    /// <returns>true if successful.</returns>
    public bool Replace(ObjRef objref, RhinoObject newObject)
    {
      if (null == objref || null == newObject || newObject.Document != null)
        return false;

      // Once the deprecated functions are removed, we should switch to checking for custom subclasses
      bool is_proper_subclass = newObject is Custom.CustomBrepObject ||
                                newObject is Custom.CustomCurveObject ||
                                newObject is Custom.CustomMeshObject ||
                                newObject is Custom.CustomPointObject;
      if (!is_proper_subclass)
        throw new NotImplementedException();

      Type t = newObject.GetType();
      if (t.GetConstructor(Type.EmptyTypes) == null)
        throw new NotImplementedException("class must have a public parameterless constructor");


      IntPtr ptr_const_objref = objref.ConstPointer();
      IntPtr ptr_rhino_object = newObject.NonConstPointer();
      bool rc = UnsafeNativeMethods.CRhinoDoc_ReplaceObject1(m_doc.m_docId, ptr_const_objref, ptr_rhino_object);
      if (rc)
      {
        uint serial_number = UnsafeNativeMethods.CRhinoObject_RuntimeSN(ptr_rhino_object);
        if (serial_number > 0)
          newObject.m_rhinoobject_serial_number = serial_number;
        newObject.m_pRhinoObject = IntPtr.Zero;
        GC.SuppressFinalize(newObject);
        AddCustomObjectForTracking(serial_number, newObject, ptr_rhino_object);
      }
      return rc;
    }

    /// <summary>Replaces one object with new point object.</summary>
    /// <param name="objref">
    /// Reference to old object to be replaced. The object objref.Object() will be deleted.
    /// </param>
    /// <param name="point">new point to be added.  The point is copied.</param>
    /// <returns>true if successful.</returns>
    public bool Replace(ObjRef objref, Point3d point)
    {
      // 7 Jan. 2009 S. Baer
      // I looked at every call to ReplaceObject in core Rhino and we only use the return
      // object about 4 times out of ~353 calls. Those 4 calls just use the return object
      // to look at it's attributes. If we need this extra information, we can add other Replace
      // functions in the future
      if (null == objref)
        return false;
      IntPtr ptr_rhino_object = UnsafeNativeMethods.CRhinoDoc_ReplaceObject2(m_doc.m_docId, objref.ConstPointer(), point);
      return IntPtr.Zero != ptr_rhino_object;
    }

    /// <summary>Replaces one object with new point object.</summary>
    /// <param name="objectId">Id of object to be replaced.</param>
    /// <param name="point">new point to be added.  The point is copied.</param>
    /// <returns>true if successful.</returns>
    public bool Replace(Guid objectId, Point3d point)
    {
      using (ObjRef objref = new ObjRef(objectId))
      {
        return Replace(objref, point);
      }
    }

    /// <summary>Replaces one object with new textdot object.</summary>
    /// <param name="objref">
    /// Reference to old object to be replaced. The object objref.Object() will be deleted.
    /// </param>
    /// <param name="dot">new textdot to be added.  The textdot is copied.</param>
    /// <returns>true if successful.</returns>
    public bool Replace(ObjRef objref, TextDot dot)
    {
      if (null == objref || null == dot)
        return false;
      IntPtr ptr_const_objref = objref.ConstPointer();
      IntPtr ptr_const_dot = dot.ConstPointer();
      bool rc = UnsafeNativeMethods.CRhinoDoc_ReplaceTextDot(m_doc.m_docId, ptr_const_objref, ptr_const_dot);
      return rc;
    }

    /// <summary>Replaces one object with new textdot object.</summary>
    /// <param name="objectId">Id of object to be replaced.</param>
    /// <param name="dot">new textdot to be added.  The textdot is copied.</param>
    /// <returns>true if successful.</returns>
    public bool Replace(Guid objectId, TextDot dot)
    {
      using (ObjRef objref = new ObjRef(objectId))
      {
        return Replace(objref, dot);
      }
    }

    /// <summary>Replaces one object with new line curve object.</summary>
    /// <param name="objref">
    /// Reference to old object to be replaced. The object objref.Object() will be deleted.
    /// </param>
    /// <param name="line">new line to be added.  The line is copied.</param>
    /// <returns>true if successful.</returns>
    public bool Replace(ObjRef objref, Line line)
    {
      return Replace(objref, new LineCurve(line));
    }

    /// <summary>Replaces one object with new line curve object.</summary>
    /// <param name="objectId">Id of object to be replaced.</param>
    /// <param name="line">new line to be added.  The line is copied.</param>
    /// <returns>true if successful.</returns>
    public bool Replace(Guid objectId, Line line)
    {
      using (ObjRef objref = new ObjRef(objectId))
      {
        return Replace(objref, line);
      }
    }

    /// <summary>Replaces one object with new curve object.</summary>
    /// <param name="objref">
    /// Reference to old object to be replaced. The object objref.Object() will be deleted.
    /// </param>
    /// <param name="circle">new circle to be added.  The circle is copied.</param>
    /// <returns>true if successful.</returns>
    public bool Replace(ObjRef objref, Circle circle)
    {
      return Replace(objref, new ArcCurve(circle));
    }

    /// <summary>Replaces one object with new curve object.</summary>
    /// <param name="objectId">Id of object to be replaced.</param>
    /// <param name="circle">new circle to be added.  The circle is copied.</param>
    /// <returns>true if successful.</returns>
    public bool Replace(Guid objectId, Circle circle)
    {
      using (ObjRef objref = new ObjRef(objectId))
      {
        return Replace(objref, circle);
      }
    }

    /// <summary>Replaces one object with new curve object.</summary>
    /// <param name="objref">
    /// Reference to old object to be replaced. The object objref.Object() will be deleted.
    /// </param>
    /// <param name="arc">new arc to be added.  The arc is copied.</param>
    /// <returns>true if successful.</returns>
    public bool Replace(ObjRef objref, Arc arc)
    {
      return Replace(objref, new ArcCurve(arc));
    }

    /// <summary>Replaces one object with new curve object.</summary>
    /// <param name="objectId">Id of object to be replaced.</param>
    /// <param name="arc">new arc to be added.  The arc is copied.</param>
    /// <returns>true if successful.</returns>
    public bool Replace(Guid objectId, Arc arc)
    {
      using (ObjRef objref = new ObjRef(objectId))
      {
        return Replace(objref, arc);
      }
    }

    /// <summary>Replaces one object with new curve object.</summary>
    /// <param name="objref">
    /// Reference to old object to be replaced. The object objref.Object() will be deleted.
    /// </param>
    /// <param name="polyline">new polyline to be added.  The polyline is copied.</param>
    /// <returns>true if successful.</returns>
    public bool Replace(ObjRef objref, Polyline polyline)
    {
      return Replace(objref, new PolylineCurve(polyline));
    }

    /// <summary>Replaces one object with new curve object.</summary>
    /// <param name="objectId">Id of object to be replaced.</param>
    /// <param name="polyline">new polyline to be added.  The polyline is copied.</param>
    /// <returns>true if successful.</returns>
    public bool Replace(Guid objectId, Polyline polyline)
    {
      using (ObjRef objref = new ObjRef(objectId))
      {
        return Replace(objref, polyline);
      }
    }

    /// <summary>
    /// Replaces one object with new curve object.
    /// </summary>
    /// <param name="objref">reference to old object to be replaced. The objref.Object() will be deleted.</param>
    /// <param name="curve">
    /// New curve to be added. A duplicate of the curve is added to the Rhino model.
    /// </param>
    /// <returns>true if successful.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_insertknot.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_insertknot.cs' lang='cs'/>
    /// <code source='examples\py\ex_insertknot.py' lang='py'/>
    /// </example>
    public bool Replace(ObjRef objref, Curve curve)
    {
      if (null == objref || null == curve)
        return false;
      IntPtr ptr_const_objref = objref.ConstPointer();
      IntPtr ptr_const_curve = curve.ConstPointer();
      IntPtr ptr_curve_object = UnsafeNativeMethods.CRhinoDoc_ReplaceObject3(m_doc.m_docId, ptr_const_objref, ptr_const_curve);
      return (IntPtr.Zero != ptr_curve_object);
    }

    /// <summary>Replaces one object with new curve object.</summary>
    /// <param name="objectId">Id of object to be replaced.</param>
    /// <param name="curve">
    /// New curve to be added. A duplicate of the curve is added to the Rhino model.
    /// </param>
    /// <returns>true if successful.</returns>
    public bool Replace(Guid objectId, Curve curve)
    {
      using (ObjRef objref = new ObjRef(objectId))
      {
        return Replace(objref, curve);
      }
    }

    /// <summary>
    /// Replaces one object with new surface object.
    /// </summary>
    /// <param name="objref">reference to old object to be replaced. The objref.Object() will be deleted.</param>
    /// <param name="surface">
    /// new surface to be added
    /// A duplicate of the surface is added to the Rhino model.
    /// </param>
    /// <returns>true if successful.</returns>
    public bool Replace(ObjRef objref, Surface surface)
    {
      if (null == objref || null == surface)
        return false;
      IntPtr ptr_const_objref = objref.ConstPointer();
      IntPtr ptr_const_surface = surface.ConstPointer();
      IntPtr ptr_surface_object = UnsafeNativeMethods.CRhinoDoc_ReplaceObject4(m_doc.m_docId, ptr_const_objref, ptr_const_surface);
      return (IntPtr.Zero != ptr_surface_object);
    }

    /// <summary>Replaces one object with new surface object.</summary>
    /// <param name="objectId">Id of object to be replaced.</param>
    /// <param name="surface">
    /// new surface to be added
    /// A duplicate of the surface is added to the Rhino model.
    /// </param>
    /// <returns>true if successful.</returns>
    public bool Replace(Guid objectId, Surface surface)
    {
      using (ObjRef objref = new ObjRef(objectId))
      {
        return Replace(objref, surface);
      }
    }

    /// <summary>
    /// Replaces one object with new brep object.
    /// </summary>
    /// <param name="objref">reference to old object to be replaced. The objref.Object() will be deleted.</param>
    /// <param name="brep">
    /// new brep to be added
    /// A duplicate of the brep is added to the Rhino model.
    /// </param>
    /// <returns>true if successful.</returns>
    public bool Replace(ObjRef objref, Brep brep)
    {
      if (null == objref || null == brep)
        return false;
      IntPtr ptr_const_objref = objref.ConstPointer();
      IntPtr pre_const_brep = brep.ConstPointer();
      IntPtr pre_brep_object = UnsafeNativeMethods.CRhinoDoc_ReplaceObject5(m_doc.m_docId, ptr_const_objref, pre_const_brep);
      return (IntPtr.Zero != pre_brep_object);
    }

    /// <summary>Replaces one object with new brep object.</summary>
    /// <param name="objectId">Id of object to be replaced.</param>
    /// <param name="brep">
    /// new surface to be added
    /// A duplicate of the brep is added to the Rhino model.
    /// </param>
    /// <returns>true if successful.</returns>
    public bool Replace(Guid objectId, Brep brep)
    {
      using (ObjRef objref = new ObjRef(objectId))
      {
        return Replace(objref, brep);
      }
    }

    /// <summary>
    /// Replaces one object with new mesh object.
    /// </summary>
    /// <param name="objref">reference to old object to be replaced. The objref.Object() will be deleted.</param>
    /// <param name="mesh">
    /// new mesh to be added
    /// A duplicate of the mesh is added to the Rhino model.
    /// </param>
    /// <returns>true if successful.</returns>
    public bool Replace(ObjRef objref, Mesh mesh)
    {
      if (null == objref || null == mesh)
        return false;
      IntPtr ptr_const_objref = objref.ConstPointer();
      IntPtr ptr_const_mesh = mesh.ConstPointer();
      IntPtr ptr_mesh_object = UnsafeNativeMethods.CRhinoDoc_ReplaceObject6(m_doc.m_docId, ptr_const_objref, ptr_const_mesh);
      return (IntPtr.Zero != ptr_mesh_object);
    }

    /// <summary>Replaces one object with new mesh object.</summary>
    /// <param name="objectId">Id of object to be replaced.</param>
    /// <param name="mesh">
    /// new mesh to be added
    /// A duplicate of the mesh is added to the Rhino model.
    /// </param>
    /// <returns>true if successful.</returns>
    public bool Replace(Guid objectId, Mesh mesh)
    {
      using (ObjRef objref = new ObjRef(objectId))
      {
        return Replace(objref, mesh);
      }
    }

    /// <summary>
    /// Replaces one object with new text object.
    /// </summary>
    /// <param name="objref">reference to old object to be replaced. The objref.Object() will be deleted.</param>
    /// <param name="text">
    /// new text to be added
    /// A duplicate of the text is added to the Rhino model.
    /// </param>
    /// <returns>true if successful.</returns>
    public bool Replace(ObjRef objref, TextEntity text)
    {
      if (null == objref || null == text)
        return false;
      IntPtr ptr_const_objref = objref.ConstPointer();
      IntPtr ptr_const_text = text.ConstPointer();
      bool rc = UnsafeNativeMethods.CRhinoDoc_ReplaceTextEntity(m_doc.m_docId, ptr_const_objref, ptr_const_text);
      return rc;
    }

    /// <summary>
    /// Replaces one object with new text object.
    /// </summary>
    /// <param name="objectId">Id of object to be replaced.</param>
    /// <param name="text">
    /// new text to be added
    /// A duplicate of the text is added to the Rhino model.
    /// </param>
    /// <returns>true if successful.</returns>
    public bool Replace(Guid objectId, TextEntity text)
    {
      using (ObjRef objref = new ObjRef(objectId))
      {
        return Replace(objref, text);
      }
    }

    /// <summary>
    /// Replaces one object with new pointcloud object.
    /// </summary>
    /// <param name="objref">reference to old object to be replaced. The objref.Object() will be deleted.</param>
    /// <param name="pointcloud">
    /// new pointcloud to be added
    /// A duplicate of the pointcloud is added to the Rhino model.
    /// </param>
    /// <returns>true if successful.</returns>
    public bool Replace(ObjRef objref, PointCloud pointcloud)
    {
      if (null == objref || null == pointcloud)
        return false;
      IntPtr ptr_const_objref = objref.ConstPointer();
      IntPtr ptr_const_pointcloud = pointcloud.ConstPointer();
      bool rc = UnsafeNativeMethods.CRhinoDoc_ReplacePointCloud(m_doc.m_docId, ptr_const_objref, ptr_const_pointcloud);
      return rc;
    }
    
    /// <summary>
    /// Replaces one object with new pointcloud object.
    /// </summary>
    /// <param name="objectId">Id of object to be replaced.</param>
    /// <param name="pointcloud">
    /// new pointcloud to be added
    /// A duplicate of the pointcloud is added to the Rhino model.
    /// </param>
    /// <returns>true if successful.</returns>
    public bool Replace(Guid objectId, PointCloud pointcloud)
    {
      using (ObjRef objref = new ObjRef(objectId))
      {
        return Replace(objref, pointcloud);
      }
    }

    #endregion

#region Find geometry
    /// <summary>
    /// Gets the most recently added object that is still in the Document.
    /// </summary>
    /// <returns>The most recent (non-deleted) object in the document, or null if no such object exists.</returns>
    public Rhino.DocObjects.RhinoObject MostRecentObject()
    {
      IntPtr ptr = UnsafeNativeMethods.CRhinoDoc_MostRecentObject(m_doc.m_docId);
      return Rhino.DocObjects.RhinoObject.CreateRhinoObjectHelper(ptr);
    }
    /// <summary>
    /// Gets all the objects that have been added to the document since a given runtime serial number. 
    /// </summary>
    /// <param name="runtimeSerialNumber">Runtime serial number of the last object not to include in the list.</param>
    /// <returns>An array of objects or null if no objects were added since the given runtime serial number.</returns>
    [CLSCompliant(false)]
    public Rhino.DocObjects.RhinoObject[] AllObjectsSince(uint runtimeSerialNumber)
    {
      using (Rhino.Runtime.INTERNAL_RhinoObjectArray rhobjs = new Rhino.Runtime.INTERNAL_RhinoObjectArray())
      {
        IntPtr pArray = rhobjs.NonConstPointer();
        UnsafeNativeMethods.CRhinoDoc_AllObjectsSince(m_doc.m_docId, runtimeSerialNumber, pArray);
        return rhobjs.ToArray();
      }
    }

    // 27 Jan 2010 S. Baer
    // I think it is useful to have "quick" finders, but I'm not exactly sure if this is the right
    // approach. I couldn't find any code in Grasshopper that calls this function and want to take
    // some time to figure out how the interface should be structured. If we want a quick point finder,
    // the internal implementation would look different than what is written below.
    //
    ///// <summary>
    ///// Searches the document for the point which matches the given ID.
    ///// </summary>
    ///// <param name="objectID">Object ID to search for.</param>
    ///// <returns>The sought after Point coordinates, or Point3d.Unset if the point could not be found.</returns>
    //public Point3d FindPoint(Guid objectID)
    //{
    //  DocObjects.RhinoObject obj = this.Find(objectID);
    //  if (obj == null) { return Point3d.Unset; }
    //  if (obj.ObjectType != Rhino.DocObjects.ObjectType.Point) { return Point3d.Unset; }
    //  //return (DocObjects.PointObject)(obj).Point; //Doesn't work yet.
    //  return Point3d.Unset;
    //}
    //// TODO: write these functions for all other Object types too.
    #endregion

#region Object state changes (lock, hide, etc.)
    const int idxHideObject = 0;
    const int idxShowObject = 1;
    const int idxLockObject = 2;
    const int idxUnlockObject = 3;

    /// <summary>
    /// If objref.Object().IsNormal() is true, then the object will be hidden.
    /// </summary>
    /// <param name="objref">reference to object to hide.</param>
    /// <param name="ignoreLayerMode">
    /// if true, the object will be hidden even if it is on a layer that is locked or off.
    /// </param>
    /// <returns>true if the object was successfully hidden.</returns>
    public bool Hide(DocObjects.ObjRef objref, bool ignoreLayerMode)
    {
      if (null == objref)
        return false;
      IntPtr pObjRef = objref.ConstPointer();
      return UnsafeNativeMethods.CRhinoDoc_SetObjectState(m_doc.m_docId, pObjRef, ignoreLayerMode, idxHideObject);
    }
    /// <summary>
    /// If obj.IsNormal() is true, then the object will be hidden.
    /// </summary>
    /// <param name="obj">object to hide.</param>
    /// <param name="ignoreLayerMode">
    /// if true, the object will be hidden even if it is on a layer that is locked or off.
    /// </param>
    /// <returns>true if the object was successfully hidden.</returns>
    public bool Hide(DocObjects.RhinoObject obj, bool ignoreLayerMode)
    {
      if (null == obj)
        return false;
      return Hide(obj.Id, ignoreLayerMode);
    }
    /// <summary>
    /// If Object().IsNormal() is true, then the object will be hidden.
    /// </summary>
    /// <param name="objectId">Id of object to hide.</param>
    /// <param name="ignoreLayerMode">
    /// if true, the object will be hidden even if it is on a layer that is locked or off.
    /// </param>
    /// <returns>true if the object was successfully hidden.</returns>
    public bool Hide(Guid objectId, bool ignoreLayerMode)
    {
      if (Guid.Empty == objectId)
        return false;
      DocObjects.ObjRef objref = new Rhino.DocObjects.ObjRef(objectId);
      IntPtr pObjRef = objref.ConstPointer();
      bool rc = UnsafeNativeMethods.CRhinoDoc_SetObjectState(m_doc.m_docId, pObjRef, ignoreLayerMode, idxHideObject);
      objref.Dispose();
      return rc;
    }

    /// <summary>
    /// If objref.Object().IsHidden() is true, then the object will be returned to normal (visible and selectable) mode.
    /// </summary>
    /// <param name="objref">reference to normal object to show.</param>
    /// <param name="ignoreLayerMode">
    /// if true, the object will be shown even if it is on a layer that is locked or off.
    /// </param>
    /// <returns>true if the object was successfully shown.</returns>
    public bool Show(DocObjects.ObjRef objref, bool ignoreLayerMode)
    {
      if (null == objref)
        return false;
      IntPtr pObjRef = objref.ConstPointer();
      return UnsafeNativeMethods.CRhinoDoc_SetObjectState(m_doc.m_docId, pObjRef, ignoreLayerMode, idxShowObject);
    }
    /// <summary>
    /// If obj.IsHidden() is true, then the object will be returned to normal (visible and selectable) mode.
    /// </summary>
    /// <param name="obj">the normal object to show.</param>
    /// <param name="ignoreLayerMode">
    /// if true, the object will be shown even if it is on a layer that is locked or off.
    /// </param>
    /// <returns>true if the object was successfully shown.</returns>
    public bool Show(DocObjects.RhinoObject obj, bool ignoreLayerMode)
    {
      if (null == obj)
        return false;
      return Show(obj.Id, ignoreLayerMode);
    }
    /// <summary>
    /// If Object().IsHidden() is true, then the object will be returned to normal (visible and selectable) mode.
    /// </summary>
    /// <param name="objectId">Id of the normal object to show.</param>
    /// <param name="ignoreLayerMode">
    /// if true, the object will be shown even if it is on a layer that is locked or off.
    /// </param>
    /// <returns>true if the object was successfully shown.</returns>
    public bool Show(Guid objectId, bool ignoreLayerMode)
    {
      if (Guid.Empty == objectId)
        return false;
      DocObjects.ObjRef objref = new Rhino.DocObjects.ObjRef(objectId);
      IntPtr pObjRef = objref.ConstPointer();
      bool rc = UnsafeNativeMethods.CRhinoDoc_SetObjectState(m_doc.m_docId, pObjRef, ignoreLayerMode, idxShowObject);
      objref.Dispose();
      return rc;
    }

    /// <summary>
    /// If objref.Object().IsNormal() is true, then the object will be locked.
    /// </summary>
    /// <param name="objref">reference to normal object to lock.</param>
    /// <param name="ignoreLayerMode">
    /// if true, the object will be locked even if it is on a layer that is locked or off.
    /// </param>
    /// <returns>true if the object was successfully locked.</returns>
    public bool Lock(DocObjects.ObjRef objref, bool ignoreLayerMode)
    {
      if (null == objref)
        return false;
      IntPtr pObjRef = objref.ConstPointer();
      return UnsafeNativeMethods.CRhinoDoc_SetObjectState(m_doc.m_docId, pObjRef, ignoreLayerMode, idxLockObject);
    }
    /// <summary>
    /// If obj.IsNormal() is true, then the object will be locked.
    /// </summary>
    /// <param name="obj">normal object to lock.</param>
    /// <param name="ignoreLayerMode">
    /// if true, the object will be locked even if it is on a layer that is locked or off.
    /// </param>
    /// <returns>true if the object was successfully locked.</returns>
    public bool Lock(DocObjects.RhinoObject obj, bool ignoreLayerMode)
    {
      if (null == obj)
        return false;
      return Lock(obj.Id, ignoreLayerMode);
    }
    /// <summary>
    /// If objref.Object().IsNormal() is true, then the object will be locked.
    /// </summary>
    /// <param name="objectId">Id of normal object to lock.</param>
    /// <param name="ignoreLayerMode">
    /// if true, the object will be locked even if it is on a layer that is locked or off.
    /// </param>
    /// <returns>true if the object was successfully locked.</returns>
    public bool Lock(Guid objectId, bool ignoreLayerMode)
    {
      if (Guid.Empty == objectId)
        return false;
      DocObjects.ObjRef objref = new Rhino.DocObjects.ObjRef(objectId);
      IntPtr pObjRef = objref.ConstPointer();
      bool rc = UnsafeNativeMethods.CRhinoDoc_SetObjectState(m_doc.m_docId, pObjRef, ignoreLayerMode, idxLockObject);
      objref.Dispose();
      return rc;
    }

    /// <summary>
    /// If objref.Object().IsLocked() is true, then the object will be returned to normal (visible and selectable) mode.
    /// </summary>
    /// <param name="objref">reference to locked object to unlock.</param>
    /// <param name="ignoreLayerMode">
    /// if true, the object will be unlocked even if it is on a layer that is locked or off.
    /// </param>
    /// <returns>true if the object was successfully unlocked.</returns>
    public bool Unlock(DocObjects.ObjRef objref, bool ignoreLayerMode)
    {
      if (null == objref)
        return false;
      IntPtr pObjRef = objref.ConstPointer();
      return UnsafeNativeMethods.CRhinoDoc_SetObjectState(m_doc.m_docId, pObjRef, ignoreLayerMode, idxUnlockObject);
    }
    /// <summary>
    /// If obj.IsLocked() is true, then the object will be returned to normal (visible and selectable) mode.
    /// </summary>
    /// <param name="obj">locked object to unlock.</param>
    /// <param name="ignoreLayerMode">
    /// if true, the object will be unlocked even if it is on a layer that is locked or off.
    /// </param>
    /// <returns>true if the object was successfully unlocked.</returns>
    public bool Unlock(DocObjects.RhinoObject obj, bool ignoreLayerMode)
    {
      if (null == obj)
        return false;
      return Unlock(obj.Id, ignoreLayerMode);
    }
    /// <summary>
    /// If Object().IsLocked() is true, then the object will be returned to normal (visible and selectable) mode.
    /// </summary>
    /// <param name="objectId">Id of locked object to unlock.</param>
    /// <param name="ignoreLayerMode">
    /// if true, the object will be unlocked even if it is on a layer that is locked or off.
    /// </param>
    /// <returns>true if the object was successfully unlocked.</returns>
    public bool Unlock(Guid objectId, bool ignoreLayerMode)
    {
      if (Guid.Empty == objectId)
        return false;
      DocObjects.ObjRef objref = new Rhino.DocObjects.ObjRef(objectId);
      IntPtr pObjRef = objref.ConstPointer();
      bool rc = UnsafeNativeMethods.CRhinoDoc_SetObjectState(m_doc.m_docId, pObjRef, ignoreLayerMode, idxUnlockObject);
      objref.Dispose();
      return rc;
    }
    #endregion

#region Object transforms
    /// <summary>
    /// Gets the boundingbox for all objects (normal, locked and hidden) in this
    /// document that exist in "model" space. This bounding box does not include
    /// objects that exist in layout space.
    /// </summary>
    public BoundingBox BoundingBox
    {
      get
      {
        Point3d min = Point3d.Unset;
        Point3d max = Point3d.Unset;

        if (UnsafeNativeMethods.CRhinoDoc_BoundingBox(m_doc.m_docId, ref min, ref max, false))
          return new BoundingBox(min, max);

        return BoundingBox.Empty;
      }
    }
    /// <summary>
    /// Gets the boundingbox for all visible objects (normal and locked) in this
    /// document that exist in "model" space. This bounding box does not include
    /// hidden objects or any objects that exist in layout space.
    /// </summary>
    public BoundingBox BoundingBoxVisible
    {
      get
      {
        Point3d min = Point3d.Unset;
        Point3d max = Point3d.Unset;

        if (UnsafeNativeMethods.CRhinoDoc_BoundingBox(m_doc.m_docId, ref min, ref max, true))
          return new BoundingBox(min, max);

        return BoundingBox.Empty;
      }
    }

    /// <summary>
    /// Constructs a new object that is the transformation of the existing object
    /// and deletes the existing object if deleteOriginal is true.
    /// </summary>
    /// <param name="objref">
    /// reference to object to transform. The objref.Object() will be deleted if deleteOriginal is true.
    /// </param>
    /// <param name="xform">transformation to apply.</param>
    /// <param name="deleteOriginal">
    /// if true, the original object is deleted
    /// if false, the original object is not deleted.
    /// </param>
    /// <returns>
    /// Id of the new object that is the transformation of the existing_object.
    /// The new object has identical attributes.
    /// </returns>
    /// <remarks>
    /// If the object is locked or on a locked layer, then it cannot be transformed.
    /// </remarks>
    /// <example>
    /// <code source='examples\vbnet\ex_orientonsrf.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_orientonsrf.cs' lang='cs'/>
    /// <code source='examples\py\ex_orientonsrf.py' lang='py'/>
    /// </example>
    public Guid Transform(DocObjects.ObjRef objref, Transform xform, bool deleteOriginal)
    {
      if (null == objref)
        return Guid.Empty;
      IntPtr pObjRef = objref.NonConstPointer();
      return UnsafeNativeMethods.CRhinoDoc_TransformObject(m_doc.m_docId, pObjRef, ref xform, deleteOriginal, false, false);
    }
    /// <summary>
    /// Constructs a new object that is the transformation of the existing object
    /// and deletes the existing object if deleteOriginal is true.
    /// </summary>
    /// <param name="obj">
    /// Rhino object to transform. This object will be deleted if deleteOriginal is true.
    /// </param>
    /// <param name="xform">transformation to apply.</param>
    /// <param name="deleteOriginal">
    /// if true, the original object is deleted
    /// if false, the original object is not deleted.
    /// </param>
    /// <returns>
    /// Id of the new object that is the transformation of the existing_object.
    /// The new object has identical attributes.
    /// </returns>
    /// <remarks>
    /// If the object is locked or on a locked layer, then it cannot be transformed.
    /// </remarks>
    public Guid Transform(DocObjects.RhinoObject obj, Transform xform, bool deleteOriginal)
    {
      if (null == obj)
        return Guid.Empty;
      DocObjects.ObjRef objref = new Rhino.DocObjects.ObjRef(obj);
      Guid rc = Transform(objref, xform, deleteOriginal);
      objref.Dispose();
      return rc;
    }
    /// <summary>
    /// Constructs a new object that is the transformation of the existing object
    /// and deletes the existing object if deleteOriginal is true.
    /// </summary>
    /// <param name="objectId">
    /// Id of rhino object to transform. This object will be deleted if deleteOriginal is true.
    /// </param>
    /// <param name="xform">transformation to apply.</param>
    /// <param name="deleteOriginal">
    /// if true, the original object is deleted
    /// if false, the original object is not deleted.
    /// </param>
    /// <returns>
    /// Id of the new object that is the transformation of the existing_object.
    /// The new object has identical attributes.
    /// </returns>
    /// <remarks>
    /// If the object is locked or on a locked layer, then it cannot be transformed.
    /// </remarks>
    public Guid Transform(Guid objectId, Transform xform, bool deleteOriginal)
    {
      DocObjects.ObjRef objref = new Rhino.DocObjects.ObjRef(objectId);
      Guid rc = Transform(objref, xform, deleteOriginal);
      objref.Dispose();
      return rc;
    }

    /// <summary>
    /// Constructs a new object that is the transformation of the existing object
    /// and records history of the transformation if history recording is turned on.
    /// If history recording is not enabled, this function will act the same as
    /// Transform(objref, xform, false)
    /// </summary>
    /// <param name="objref">
    /// reference to object to transform.
    /// </param>
    /// <param name="xform">transformation to apply.</param>
    /// <returns>
    /// Id of the new object that is the transformation of the existing_object.
    /// The new object has identical attributes.
    /// </returns>
    /// <remarks>
    /// If the object is locked or on a locked layer, then it cannot be transformed.
    /// </remarks>
    public Guid TransformWithHistory(DocObjects.ObjRef objref, Transform xform)
    {
      if (null == objref)
        return Guid.Empty;
      IntPtr pObjRef = objref.NonConstPointer();
      return UnsafeNativeMethods.CRhinoDoc_TransformObject(m_doc.m_docId, pObjRef, ref xform, false, false, true);
    }
    /// <summary>
    /// Constructs a new object that is the transformation of the existing object
    /// and records history of the transformation if history recording is turned on.
    /// If history recording is not enabled, this function will act the same as
    /// Transform(obj, xform, false)
    /// </summary>
    /// <param name="obj">
    /// Rhino object to transform.
    /// </param>
    /// <param name="xform">transformation to apply.</param>
    /// <returns>
    /// Id of the new object that is the transformation of the existing_object.
    /// The new object has identical attributes.
    /// </returns>
    /// <remarks>
    /// If the object is locked or on a locked layer, then it cannot be transformed.
    /// </remarks>
    public Guid TransformWithHistory(DocObjects.RhinoObject obj, Transform xform)
    {
      if (null == obj)
        return Guid.Empty;
      DocObjects.ObjRef objref = new Rhino.DocObjects.ObjRef(obj);
      Guid rc = TransformWithHistory(objref, xform);
      objref.Dispose();
      return rc;
    }
    /// <summary>
    /// Constructs a new object that is the transformation of the existing object
    /// and records history of the transformation if history recording is turned on.
    /// If history recording is not enabled, this function will act the same as
    /// Transform(objectId, xform, false)
    /// </summary>
    /// <param name="objectId">
    /// Id of rhino object to transform.
    /// </param>
    /// <param name="xform">transformation to apply.</param>
    /// <returns>
    /// Id of the new object that is the transformation of the existing_object.
    /// The new object has identical attributes.
    /// </returns>
    /// <remarks>
    /// If the object is locked or on a locked layer, then it cannot be transformed.
    /// </remarks>
    public Guid TransformWithHistory(Guid objectId, Transform xform)
    {
      DocObjects.ObjRef objref = new Rhino.DocObjects.ObjRef(objectId);
      Guid rc = TransformWithHistory(objref, xform);
      objref.Dispose();
      return rc;
    }

    /// <summary>
    /// Duplicates the object that is referenced by objref.
    /// <para>Same as Transform(objref, <see cref="Geometry.Transform.Identity">Transform.Identity</see>, false)</para>
    /// </summary>
    /// <param name="objref">A Rhino object reference to follow for object duplication.</param>
    /// <returns>The new object ID.</returns>
    public Guid Duplicate(DocObjects.ObjRef objref)
    {
      return Transform(objref, Geometry.Transform.Identity, false);
    }
    /// <summary>
    /// Duplicates the object that is referenced by obj.
    /// <para>Same as TransformObject(obj, <see cref="Geometry.Transform.Identity">Transform.Identity</see>y, false)</para>
    /// </summary>
    /// <param name="obj">A Rhino object to duplicate.</param>
    /// <returns>The new object ID.</returns>
    public Guid Duplicate(DocObjects.RhinoObject obj)
    {
      return Transform(obj, Geometry.Transform.Identity, false);
    }
    /// <summary>
    /// Same as TransformObject(objref, ON_Xform.Identity, false)
    /// </summary>
    /// <param name="objectId">An ID to an object in the document that needs to be duplicated.</param>
    /// <returns>The new object ID.</returns>
    public Guid Duplicate(Guid objectId)
    {
      return Transform(objectId, Geometry.Transform.Identity, false);
    }

    //  Description:
    //    Creates a new object that is the transformation of the
    //    existing object and deletes the existing object if 
    //    bDeleteOriginal is true.
    //  Parameters:
    //    old_object - [in] reference to object to morph.  The object
    //        objref.Object() will be deleted if bDeleteOriginal is true.
    //    xform - [in] transformation to apply
    //    bAddNewObjectToDoc - [in] 
    //        if true, the new object is added to the document.
    //        if false, the new object is not added to the document.
    //    bDeleteOriginal - [in] 
    //        if true, the original object is deleted
    //        if false, the original object is not deleted
    //    bAddTransformHistory - [in]
    //        If true and history recording is turned on, then
    //        transformation history is recorded.  This will be
    //        adequate for simple transformation commands like
    //        rotate, move, scale, and so on that do not have
    //        auxillary input objects.  For fancier commands,
    //        that have an auxillary object, like the spine
    //        curve in ArrayAlongCrv, set bAddTransformHistory
    //        to false. 
    //  Returns:
    //    New object that is the morph of the existing_object.
    //    The new object has identical attributes.
    //  Remarks:
    //    If the object is locked or on a locked layer, then it cannot
    //    be transformed.
    //  CRhinoObject* TransformObject( 
    //    const CRhinoObject* old_object,
    //    const ON_Xform& xform,
    //    bool bAddNewObjectToDoc,
    //    bool bDeleteOriginal,
    //    bool bAddTransformHistory
    //    );


    //  /*
    //  Description:
    //    Transforms every object in a list.
    //  Parameters:
    //    it - [in] iterates through list of objects to transform
    //    xform - [in] transformation to apply
    //    bDeleteOriginal = [in] 
    //         if true, the original objects are deleted
    //         if false, the original objects are not deleted
    //    bIgnoreModes - [in] If true, locked and hidden objects
    //        are transformed.  If false objects that are locked,
    //        hidden, or on locked or hidden layers are not 
    //        transformed.
    //    bAddTransformHistory - [in]
    //        If true and history recording is turned on, then
    //        transformation history is recorded.  This will be
    //        adequate for simple transformation commands like
    //        rotate, move, scale, and so on that do not have
    //        auxillary input objects.  For fancier commands,
    //        that have an auxillary object, like the spine
    //        curve in ArrayAlongCrv, set bAddTransformHistory
    //        to false. 
    //  Returns:
    //    Number of objects that were transformed.
    //  Remarks:
    //    This is similar to calling TransformObject() for each object
    //    int the list except that this function will modify locked and
    //    hidden objects.  It is used for things like scaling the entire
    //    model when a unit system is changed.
    //  */
    //  int TransformObjects(
    //    CRhinoObjectIterator& it,
    //    const ON_Xform& xform,
    //    bool bDeleteOriginal = true,
    //    bool bIgnoreModes = false,
    //    bool bAddTransformHistory = false
    //    );

    //public Guid MorphObject(DocObjects.RhinoObject oldObject, ON_SpaceMorph morph, bool deleteOriginal)
    //{
    //  if (null == oldObject || null == morph)
    //    return Guid.Empty;

    //  Guid rc = morph.PerformMorph(m_docId, oldObject, deleteOriginal);
    //  return rc;
    //}
    //  CRhinoObject* MorphObject( const CRhinoObject* old_object, const ON_SpaceMorph& morph, bool bAddNewObjectToDoc, bool bDeleteOriginal );
    //[skipping]
    //  int PickObjects( const CRhinoPickContext&, CRhinoObjRefArray& ) const;
    //  BOOL SnapTo( const CRhinoSnapContext& snap_context, CRhinoSnapEvent& snap_event, const ON_SimpleArray<ON_3dPoint>* construction_points = 0,

    /// <summary>
    /// Altered grip positions on a RhinoObject are used to calculate an updated object
    /// that is added to the document.
    /// </summary>
    /// <param name="obj">object with modified grips to update.</param>
    /// <param name="deleteOriginal">if true, obj is deleted from the document.</param>
    /// <returns>new RhinoObject on success; otherwise null.</returns>
    public DocObjects.RhinoObject GripUpdate(DocObjects.RhinoObject obj, bool deleteOriginal)
    {
      if (null == obj)
        return null;

      IntPtr pRhinoObject = obj.ConstPointer();
      IntPtr pNewObject = UnsafeNativeMethods.RHC_RhinoUpdateGripOwner(pRhinoObject, deleteOriginal);
      return Rhino.DocObjects.RhinoObject.CreateRhinoObjectHelper(pNewObject);
    }

    // [skipping]
    //  void ClearMarks( CRhinoObjectIterator&, int=0 );
    //  void SetRedrawDisplayHint( unsigned int display_hint, ON::display_mode dm = ON::default_display ) const;
    #endregion

#region Object enumerator

    private IEnumerator<RhinoObject> GetEnumerator(ObjectEnumeratorSettings settings)
    {
      ObjectIterator it = new ObjectIterator(m_doc, settings);
      return it;
    }

    private class EnumeratorWrapper : IEnumerable<DocObjects.RhinoObject>
    {
      readonly IEnumerator<DocObjects.RhinoObject> m_enumerator;
      public EnumeratorWrapper(IEnumerator<DocObjects.RhinoObject> enumerator)
      {
        m_enumerator = enumerator;
      }

      public IEnumerator<Rhino.DocObjects.RhinoObject> GetEnumerator()
      {
        return m_enumerator;
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return m_enumerator;
      }
    }

    public int ObjectCount(ObjectEnumeratorSettings filter)
    {
      ObjectIterator it = new ObjectIterator(m_doc, filter);
      IntPtr ptr_iterator = it.NonConstPointer();
      string name_filter = filter.m_name_filter;
      int count = UnsafeNativeMethods.CRhinoObjectIterator_Count(ptr_iterator, name_filter);
      it.Dispose();
      return count;
    }

    /// <example>
    /// <code source='examples\vbnet\ex_findobjectsbyname.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_findobjectsbyname.cs' lang='cs'/>
    /// <code source='examples\py\ex_findobjectsbyname.py' lang='py'/>
    /// </example>
    public IEnumerable<RhinoObject> GetObjectList(ObjectEnumeratorSettings settings)
    {
      IEnumerator<RhinoObject> e = GetEnumerator(settings);
      return new EnumeratorWrapper(e);
    }

    /// <example>
    /// <code source='examples\vbnet\ex_dimstyle.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_dimstyle.cs' lang='cs'/>
    /// <code source='examples\py\ex_dimstyle.py' lang='py'/>
    /// </example>
    public IEnumerable<RhinoObject> GetObjectList(Type typeFilter)
    {
      ObjectEnumeratorSettings settings = new ObjectEnumeratorSettings();
      settings.ClassTypeFilter = typeFilter;
      ObjectIterator it = new ObjectIterator(m_doc, settings);
      return new EnumeratorWrapper(it);
    }

    [CLSCompliant(false)]
    public IEnumerable<RhinoObject> GetObjectList(ObjectType typeFilter)
    {
      ObjectEnumeratorSettings settings = new ObjectEnumeratorSettings();
      settings.ObjectTypeFilter = typeFilter;
      return GetObjectList(settings);
    }

    public IEnumerable<RhinoObject> GetSelectedObjects(bool includeLights, bool includeGrips)
    {
      ObjectEnumeratorSettings s = new ObjectEnumeratorSettings();
      s.IncludeLights = includeLights;
      s.IncludeGrips = includeGrips;
      s.IncludePhantoms = true;
      s.SelectedObjectsFilter = true;
      return GetObjectList(s);
    }


    // for IEnumerable<RhinoObject>
    public IEnumerator<RhinoObject> GetEnumerator()
    {
      ObjectEnumeratorSettings s = new ObjectEnumeratorSettings();
      return GetEnumerator(s);
    }

    // for IEnumerable
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      ObjectEnumeratorSettings s = new ObjectEnumeratorSettings();
      return GetEnumerator(s);
    }

    #endregion
  }

  public sealed class StringTable
  {
    private readonly RhinoDoc m_doc;
    internal StringTable(RhinoDoc doc)
    {
      m_doc = doc;
    }

    /// <summary>Document that owns this object table.</summary>
    public RhinoDoc Document
    {
      get { return m_doc; }
    }

    /// <summary>
    /// The number of user data strings in the current document.
    /// </summary>
    public int Count
    {
      get
      {
        return UnsafeNativeMethods.CRhinoDoc_GetDocTextCount(m_doc.m_docId);
      }
    }

    public string GetKey(int i)
    {
      using (var sh = new StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        UnsafeNativeMethods.CRhinoDoc_GetDocTextString(m_doc.m_docId, i, true, pString);
        return sh.ToString();
      }
    }

    public string GetValue(int i)
    {
      using (var sh = new StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        UnsafeNativeMethods.CRhinoDoc_GetDocTextString(m_doc.m_docId, i, false, pString);
        return sh.ToString();
      }
    }
    public string GetValue(string key)
    {
      using (var sh = new StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        UnsafeNativeMethods.CRhinoDoc_GetDocTextString2(m_doc.m_docId, key, pString);
        return sh.ToString();
      }
    }

    /// <summary>
    /// Gets a user data string from the document.
    /// </summary>
    /// <param name="section">The section at which to get the value.</param>
    /// <param name="entry">The entry to search for.</param>
    /// <returns>The user data.</returns>
    public string GetValue(string section, string entry)
    {
      if (String.IsNullOrEmpty(section) || String.IsNullOrEmpty(entry))
        return String.Empty;
      string key = section + "\\" + entry;
      return GetValue(key);
    }

    /// <summary>
    /// Returns a list of all the section names for user data strings in the document.
    /// <para>By default a section name is a key that is prefixed with a string separated by a backslash.</para>
    /// </summary>
    /// <returns>An array of section names. This can be empty, but not null.</returns>
    public string[] GetSectionNames()
    {
      int count = Count;
      System.Collections.Generic.SortedDictionary<string, bool> section_dict = new SortedDictionary<string, bool>();
      for (int i = 0; i < count; i++)
      {
        string key = GetKey(i);
        if (string.IsNullOrEmpty(key))
          continue;
        int index = key.IndexOf("\\", System.StringComparison.Ordinal);
        if (index > 0)
        {
          string section = key.Substring(0, index);
          if (!section_dict.ContainsKey(section))
            section_dict.Add(section, true);
        }
      }
      count = section_dict.Count;
      if (count < 1)
        return null;
      string[] rc = new string[count];
      section_dict.Keys.CopyTo(rc, 0);
      return rc;
    }

    /// <summary>
    /// Return list of all entry names for a given section of user data strings in the document.
    /// </summary>
    /// <param name="section">The section from which to retrieve section names.</param>
    /// <returns>An array of section names. This can be empty, but not null.</returns>
    public string[] GetEntryNames(string section)
    {
      section += "\\";
      int count = Count;
      List<string> rc = new List<string>();
      for (int i = 0; i < count; i++)
      {
        string key = GetKey(i);
        if (key != null && key.StartsWith(section))
        {
          rc.Add(key.Substring(section.Length));
        }
      }
      return rc.ToArray();
    }


    /// <summary>
    /// Adds or sets a user data string to the document.
    /// </summary>
    /// <param name="section">The section.</param>
    /// <param name="entry">The entry name.</param>
    /// <param name="value">The entry value.</param>
    /// <returns>
    /// The previous value if successful and a previous value existed.
    /// </returns>
    public string SetString(string section, string entry, string value)
    {
      string key = section;
      if ( !string.IsNullOrEmpty(entry) )
        key = section + "\\" + entry;
      return SetString(key, value);
    }

    public string SetString(string key, string value)
    {
      string rc = GetValue(key);
      UnsafeNativeMethods.CRhinoDoc_SetDocTextString(m_doc.m_docId, key, value);
      return rc;
    }

    /// <summary>
    /// Removes user data strings from the document.
    /// </summary>
    /// <param name="section">name of section to delete. If null, all sections will be deleted.</param>
    /// <param name="entry">name of entry to delete. If null, all entries will be deleted for a given section.</param>
    public void Delete(string section, string entry)
    {
      if (null == section)
      {
        UnsafeNativeMethods.CRhinoDoc_DeleteDocTextString(m_doc.m_docId, null);
        return;
      }

      if (null == entry)
      {
        string[] entries = GetEntryNames(section);
        for (int i = 0; i < entries.Length; i++)
        {
          string key = section + "\\" + entries[i];
          UnsafeNativeMethods.CRhinoDoc_DeleteDocTextString(m_doc.m_docId, key);
        }
        UnsafeNativeMethods.CRhinoDoc_DeleteDocTextString(m_doc.m_docId, section);
      }
      else
      {
        string key = section + "\\" + entry;
        UnsafeNativeMethods.CRhinoDoc_DeleteDocTextString(m_doc.m_docId, key);
      }
    }

    public void Delete(string key)
    {
      UnsafeNativeMethods.CRhinoDoc_DeleteDocTextString(m_doc.m_docId, key);
    }
  }
}

namespace Rhino.DocObjects
{
#region private helper enums
  [FlagsAttribute]
  enum object_state : int
  {
    None = 0,
    normal_objects = 1, // (not locked and not hidden)
    locked_objects = 2, // (locked objects or on locked layers)
    hidden_objects = 4, // (hidden objects or on hidden layers)
    idef_objects = 8, // objects in instance definitions (not the instance references)
    deleted_objects = 16,
    normal_or_locked_objects = 3, // normal or locked
    undeleted_objects = 7       // normal, locked, or hidden
    //all_objects = 0xFFFFFFFF      // normal, locked, hidden, or deleted
  }
  [FlagsAttribute]
  enum object_category : int
  {
    None = 0,
    active_objects = 1,    // objects that are part of current model and saved in file
    reference_objects = 2, // objects that are for reference and not saved in file
    active_and_reference_objects = 3
  }
  #endregion


  /// <summary>
  /// Settings used for getting an enumerator of objects in a document
  /// See Rhino.Collections.ObjectTable.GetEnumerator()
  /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_moveobjectstocurrentlayer.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_moveobjectstocurrentlayer.cs' lang='cs'/>
    /// <code source='examples\py\ex_moveobjectstocurrentlayer.py' lang='py'/>
    /// </example>
  public class ObjectEnumeratorSettings
  {
    // all variables are set to use same defaults as defined in CRhinoObjectIterator::Init
    internal object_category m_object_category = object_category.active_objects;
    internal object_state m_object_state = object_state.normal_or_locked_objects;
    bool m_include_lights;   //=false (initialized by Runtime)
    bool m_include_grips;    //=false (initialized by Runtime)
    bool m_include_phantoms; //=false (initialized by Runtime)
    bool m_selected_objects; //=false (initialized by Runtime)
    //internal bool m_bCheckSubObjects; //=false (initialized by Runtime)
    bool m_visible; //=false (initialized by Runtime)
    internal ObjectType m_objectfilter = ObjectType.None;
    int m_layerindex_filter = -1;
    Type m_classtype_filter; //=null (initialized by Runtime)
    RhinoViewport m_viewport_filter; //=null (initialized by Runtime)

    /// <example>
    /// <code source='examples\vbnet\ex_findobjectsbyname.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_findobjectsbyname.cs' lang='cs'/>
    /// <code source='examples\py\ex_findobjectsbyname.py' lang='py'/>
    /// </example>
    public ObjectEnumeratorSettings()
    {
    }

#region object state
    public bool NormalObjects
    {
      get
      {
        return (m_object_state & object_state.normal_objects) == object_state.normal_objects;
      }
      set
      {
        if (value)
          m_object_state |= object_state.normal_objects;
        else
          m_object_state &= ~object_state.normal_objects;
      }
    }
    public bool LockedObjects
    {
      get
      {
        return (m_object_state & object_state.locked_objects) == object_state.locked_objects;
      }
      set
      {
        if (value)
          m_object_state |= object_state.locked_objects;
        else
          m_object_state &= ~object_state.locked_objects;
      }
    }
    public bool HiddenObjects
    {
      get
      {
        return (m_object_state & object_state.hidden_objects) == object_state.hidden_objects;
      }
      set
      {
        if (value)
          m_object_state |= object_state.hidden_objects;
        else
          m_object_state &= ~object_state.hidden_objects;
      }
    }

    /// <summary>
    /// When true, ONLY Instance Definitions will be returned
    /// </summary>
    public bool IdefObjects
    {
      get
      {
        return (m_object_state & object_state.idef_objects) == object_state.idef_objects;
      }
      set
      {
        if (value)
          m_object_state |= object_state.idef_objects;
        else
          m_object_state &= ~object_state.idef_objects;
      }
    }
    public bool DeletedObjects
    {
      get
      {
        return (m_object_state & object_state.deleted_objects) == object_state.deleted_objects;
      }
      set
      {
        if (value)
          m_object_state |= object_state.deleted_objects;
        else
          m_object_state &= ~object_state.deleted_objects;
      }
    }
    #endregion

#region object category
    public bool ActiveObjects
    {
      get
      {
        return (m_object_category & object_category.active_objects) == object_category.active_objects;
      }
      set
      {
        if (value)
          m_object_category |= object_category.active_objects;
        else
          m_object_category &= ~object_category.active_objects;
      }
    }
    public bool ReferenceObjects
    {
      get
      {
        return (m_object_category & object_category.reference_objects) == object_category.reference_objects;
      }
      set
      {
        if (value)
          m_object_category |= object_category.reference_objects;
        else
          m_object_category &= ~object_category.reference_objects;
      }
    }
    #endregion

    /// <example>
    /// <code source='examples\vbnet\ex_objectiterator.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_objectiterator.cs' lang='cs'/>
    /// <code source='examples\py\ex_objectiterator.py' lang='py'/>
    /// </example>
    public bool IncludeLights
    {
      get { return m_include_lights; }
      set { m_include_lights = value; }
    }
    
    /// <example>
    /// <code source='examples\vbnet\ex_objectiterator.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_objectiterator.cs' lang='cs'/>
    /// <code source='examples\py\ex_objectiterator.py' lang='py'/>
    /// </example>
    public bool IncludeGrips
    {
      get { return m_include_grips; }
      set { m_include_grips = value; }
    }
    public bool IncludePhantoms
    {
      get { return m_include_phantoms; }
      set { m_include_phantoms = value; }
    }

    public bool SelectedObjectsFilter
    {
      get { return m_selected_objects; }
      set { m_selected_objects = value; }
    }

    public bool VisibleFilter
    {
      get { return m_visible; }
      set { m_visible = value; }
    }

    [CLSCompliant(false)]
    public ObjectType ObjectTypeFilter
    {
      get { return m_objectfilter; }
      set { m_objectfilter = value; }
    }

    public Type ClassTypeFilter
    {
      get { return m_classtype_filter; }
      set { m_classtype_filter = value; }
    }

    public int LayerIndexFilter
    {
      get { return m_layerindex_filter; }
      set { m_layerindex_filter = value; }
    }

    internal string m_name_filter; // = null; initialized to null by runtime
    /// <example>
    /// <code source='examples\vbnet\ex_findobjectsbyname.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_findobjectsbyname.cs' lang='cs'/>
    /// <code source='examples\py\ex_findobjectsbyname.py' lang='py'/>
    /// </example>
    public string NameFilter
    {
      get
      {
        if (string.IsNullOrEmpty(m_name_filter))
          return "*";
        return m_name_filter;
      }
      set
      {
        if (string.IsNullOrEmpty(value))
          m_name_filter = null;
        else
        {
          value = value.Trim();
          m_name_filter = value == "*" ? null : value;
        }
      }
    }

    /// <summary>
    /// Filter on value of object->IsActiveInViewport()
    /// </summary>
    public RhinoViewport ViewportFilter
    {
      get { return m_viewport_filter; }
      set { m_viewport_filter = value; }
    }
  }

  // ObjectIterator is not public. We only want to give the user an enumerator
  class ObjectIterator : IEnumerator<RhinoObject>
  {
#region IEnumerator Members
    bool m_first = true;
    Rhino.DocObjects.RhinoObject m_current;

    object System.Collections.IEnumerator.Current
    {
      get
      {
        return m_current;
      }
    }

    public bool MoveNext()
    {
      bool first = m_first;
      m_first = false;
      IntPtr ptr = NonConstPointer();
      string name_filter = null;
      if (null != m_settings)
        name_filter = m_settings.m_name_filter;
      IntPtr pRhinoObject = UnsafeNativeMethods.CRhinoObjectIterator_FirstNext(ptr, first, name_filter);
      if (IntPtr.Zero == pRhinoObject)
        return false;
      m_current = DocObjects.RhinoObject.CreateRhinoObjectHelper(pRhinoObject);
      if (m_settings != null && m_settings.ClassTypeFilter != null && m_current != null)
      {
        if (!m_settings.ClassTypeFilter.IsInstanceOfType(m_current))
        {
          m_current = null;
          return MoveNext();
        }
      }
      return true;
    }

    public void Reset()
    {
      m_first = true;
      m_current = null;
    }

    #endregion

#region IEnumerator<RhinoObject> Members

    public RhinoObject Current
    {
      get { return m_current; }
    }

    #endregion

    // This class is always constructed inside .NET and is
    // therefore never const
    IntPtr m_ptr; // CRhinoObjectIterator*
    internal IntPtr NonConstPointer() { return m_ptr; }
    readonly ObjectEnumeratorSettings m_settings;

    public ObjectIterator(RhinoDoc doc, ObjectEnumeratorSettings s)
    {
      m_settings = s;
      int doc_id = -1;
      if (doc != null)
        doc_id = doc.m_docId;

      IntPtr const_ptr_viewport = IntPtr.Zero;
      if (s.ViewportFilter != null)
        const_ptr_viewport = s.ViewportFilter.ConstPointer();

      m_ptr = UnsafeNativeMethods.CRhinoObjectIterator_New(doc_id, (int)s.m_object_state, (int)s.m_object_category);
      UnsafeNativeMethods.CRhinoObjectIterator_Initialize(m_ptr,
        s.IncludeLights,
        s.IncludeGrips,
        s.IncludePhantoms,
        s.SelectedObjectsFilter,
        false, /*s.m_bCheckSubObjects*/
        s.VisibleFilter,
        (uint)s.m_objectfilter,
        s.LayerIndexFilter,
        const_ptr_viewport);
    }

    ~ObjectIterator()
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
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.CRhinoObjectIterator_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }
  }
}
#endif
