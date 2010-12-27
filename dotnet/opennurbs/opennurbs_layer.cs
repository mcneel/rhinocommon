using System;
using System.Runtime.InteropServices;

namespace Rhino.DocObjects
{
  /*
  /// <summary>
  /// Provides layer definition information
  /// </summary>
  [Obsolete("Use Layer, this class will be removed in a future WIP")]
  public class LayerInfo : Runtime.CommonObject
  {
    public LayerInfo()
    {
      IntPtr ptr = UnsafeNativeMethods.ON_Layer_New();
      ConstructNonConstObject(ptr);
    }

    // Track layers in the document by doc ID and index
    int m_doc_id;
    int m_doc_index;
    internal LayerInfo(int doc_id, int doc_index)
    {
      m_doc_id = doc_id;
      m_doc_index = doc_index;
    }

    internal override IntPtr _InternalDuplicate(out bool applymempressure)
    {
      applymempressure = false;
      IntPtr pConstPointer = ConstPointer();
      return UnsafeNativeMethods.ON_Object_Duplicate(pConstPointer);
    }

    internal override IntPtr _InternalGetConstPointer()
    {
      IntPtr pConstLayer = UnsafeNativeMethods.CRhinoLayerTable_GetLayerPointer(m_doc_id, m_doc_index);
      return pConstLayer;
    }

    /// <summary>
    /// Creates a LayerInfo with the current default layer properties.
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
    public static LayerInfo GetDefaultLayerProperties()
    {
      LayerInfo layer = new LayerInfo();
      IntPtr ptr = layer.NonConstPointer();
      UnsafeNativeMethods.CRhinoLayerTable_GetDefaultLayerProperties(ptr);
      return layer;
    }

    /// <summary>
    /// Set layer to default settings
    /// </summary>
    public void Default()
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.ON_Layer_Default(ptr);
    }

    public string Name
    {
      get
      {
        using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
        {
          IntPtr pConstThis = ConstPointer();
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.ON_Layer_GetLayerName(pConstThis, pString);
          return sh.ToString();
        }
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.ON_Layer_SetLayerName(ptr, value);
      }
    }

    /// <summary>layer display color</summary>
    public System.Drawing.Color Color
    {
      get
      {
        IntPtr ptr = ConstPointer();
        int argb = UnsafeNativeMethods.ON_Layer_GetColor(ptr, true);
        return System.Drawing.Color.FromArgb(argb);
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        int argb = value.ToArgb();
        UnsafeNativeMethods.ON_Layer_SetColor(ptr, argb, true);
      }
    }

    /// <summary>plotting color</summary>
    public System.Drawing.Color PlotColor
    {
      get
      {
        IntPtr ptr = ConstPointer();
        int argb = UnsafeNativeMethods.ON_Layer_GetColor(ptr, false);
        return System.Drawing.Color.FromArgb(argb);
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        int argb = value.ToArgb();
        UnsafeNativeMethods.ON_Layer_SetColor(ptr, argb, false);
      }
    }

    internal const int idxLinetypeIndex = 0;
    internal const int idxRenderMaterialIndex = 1;
    internal const int idxLayerIndex = 2;
    internal const int idxIgesLevel = 3;

    int GetInt( int which )
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Layer_GetInt(ptr, which);
    }
    void SetInt(int which, int val)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.ON_Layer_SetInt(ptr, which, val);
    }

    public int LinetypeIndex
    {
      get { return GetInt(idxLinetypeIndex); }
      set { SetInt(idxLinetypeIndex, value); }
    }

    internal const int idxIsVisible = 0;
    internal const int idxIsLocked = 1;
    internal const int idxIsExpanded = 2;
    bool GetBool(int which)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Layer_GetSetBool(ptr, which, false, false);
    }
    void SetBool(int which, bool val)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.ON_Layer_GetSetBool(ptr, which, true, val);
    }

    /// <summary>true if objects on layer are visible</summary>
    public bool IsVisible
    {
      get { return GetBool(idxIsVisible); }
      set { SetBool(idxIsVisible, value); }
    }

    /// <summary>true if objects on layer are locked</summary>
    public bool IsLocked
    {
      get { return GetBool(idxIsLocked); }
      set { SetBool(idxIsLocked, value); }
    }

    /// <summary>
    /// If true, when the layer table is displayed in a tree control then
    /// the list of child layers is shown in the control.
    /// </summary>
    public bool IsExpanded
    {
      get { return GetBool(idxIsExpanded); }
      set { SetBool(idxIsExpanded, value); }
    }

    //[skipping]
    //  bool IsVisibleAndNotLocked() const;
    //  bool IsVisibleAndLocked() const;

    /// <summary>
    /// Index of render material for objects on this layer that have
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

    /// <summary>index of this layer</summary>
    public int LayerIndex
    {
      get { return GetInt(idxLayerIndex); }
      set { SetInt(idxLayerIndex, value); }
    }

    /// <summary>IGES level for this layer</summary>
    public int IgesLevel
    {
      get { return GetInt(idxIgesLevel); }
      set { SetInt(idxIgesLevel, value); }
    }

    /// <summary>
    /// Thickness of the plotting pen in millimeters.
    /// A thickness of 0.0 indicates the "default" pen weight should be used.
    /// </summary>
    public double PlotWeight
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_Layer_GetPlotWeight(ptr);
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.ON_Layer_SetPlotWeight(ptr, value);
      }
    }


    public Guid Id
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_Layer_GetGuid(ptr, true);
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.ON_Layer_SetGuid(ptr, true, value);
      }
    }

    /// <summary>
    /// Layers are origanized in a hierarchical structure (like file folders).
    /// If a layer is in a parent layer, then m_parent_layer_id is the id of the parent layer.
    /// </summary>
    public Guid ParentLayerId
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_Layer_GetGuid(ptr, false);
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.ON_Layer_SetGuid(ptr, false, value);
      }
    }

    public override bool IsValid
    {
      get
      {
        return InternalIsValid();
      }
    }

//  // Rendering material:
//  //   If you want something simple and fast, set 
//  //   m_material_index to the index of your rendering material 
//  //   and ignore m_rendering_attributes.
//  //   If you are developing a fancy plug-in renderer, and a user is
//  //   assigning one of your fabulous rendering materials to this
//  //   layer, then add rendering material information to the 
//  //   m_rendering_attributes.m_materials[] array. 
//  //
//  // Developers:
//  //   As soon as m_rendering_attributes.m_materials[] is not empty,
//  //   rendering material queries slow down.  Do not populate
//  //   m_rendering_attributes.m_materials[] when setting 
//  //   m_material_index will take care of your needs.
//  ON_RenderingAttributes m_rendering_attributes;
  
  
//  // Layer display attributes.
//  //   If m_display_material_id is nil, then m_color is the layer color
//  //   and defaults are used for all other display attributes.
//  //   If m_display_material_id is not nil, then some complicated
//  //   scheme is used to decide what objects on this layer look like.
//  //   In all cases, m_color is a good choice if you don't want to
//  //   deal with m_display_material_id.  In Rhino, m_display_material_id
//  //   is used to identify a registry entry that contains user specific
//  //   display preferences.
//  ON_UUID m_display_material_id;


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
  }
  */
}
