using System;
using System.Runtime.InteropServices;

#if RHINO_SDK
namespace Rhino.DocObjects
{
  /// <summary>
  /// Provides a base class for <see cref="Rhino.Geometry.AnnotationBase"/>-derived
  /// objects that are placed in a document.
  /// </summary>
  public /*abstract*/ class AnnotationObjectBase : RhinoObject
  {
    internal AnnotationObjectBase(uint serialNumber)
      : base(serialNumber) { }

    /// <summary>
    /// Gets the text that is displayed to users.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_gettext.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_gettext.cs' lang='cs'/>
    /// <code source='examples\py\ex_gettext.py' lang='py'/>
    /// </example>
    public string DisplayText
    {
      get
      {
        IntPtr ptr_const_this = ConstPointer();
        IntPtr rc = UnsafeNativeMethods.CRhinoAnnotationObject_DisplayText(ptr_const_this);
        return rc == IntPtr.Zero ? string.Empty : Marshal.PtrToStringUni(rc);
      }
    }

  }

  /// <summary>
  /// Represents a <see cref="Rhino.Geometry.Leader"/> that
  /// is picked in a document
  /// </summary>
  public class LeaderObject : AnnotationObjectBase
  {
    internal LeaderObject(uint serialNumber)
      : base(serialNumber) { }

    internal override CommitGeometryChangesFunc GetCommitFunc()
    {
      return UnsafeNativeMethods.CRhinoLeaderObject_InternalCommitChanges;
    }

    /// <summary>
    /// Gets the text entity geometry of this text object.
    /// </summary>
    public Rhino.Geometry.Leader LeaderGeometry
    {
        get
        {
            Rhino.Geometry.Leader rc = Geometry as Rhino.Geometry.Leader;
            return rc;
        }
    }
  }


  /// <summary>
  /// Represents a <see cref="Rhino.Geometry.LinearDimension"/>
  /// that is placed in a document.
  /// </summary>
  public class LinearDimensionObject : AnnotationObjectBase
  {
    internal LinearDimensionObject(uint serialNumber)
      : base(serialNumber) { }

    /// <summary>
    /// Gets the <see cref="DocObjects.DimensionStyle"/>
    /// associated with this LinearDimensionObject.
    /// </summary>
    public DimensionStyle DimensionStyle
    {
      get
      {
        Rhino.Geometry.LinearDimension ld = Geometry as Geometry.LinearDimension;
        if( ld==null || Document==null )
          return null;
        return Document.DimStyles[ld.DimensionStyleIndex];
      }
    }

    internal override CommitGeometryChangesFunc GetCommitFunc()
    {
      return UnsafeNativeMethods.CRhinoLinearDimension_InternalCommitChanges;
    }
  }

  /// <summary>
  /// A radius style dimension
  /// </summary>
  public class RadialDimensionObject : AnnotationObjectBase
  {
    internal RadialDimensionObject(uint serialNumber)
      : base(serialNumber) { }

    internal override CommitGeometryChangesFunc GetCommitFunc()
    {
      return UnsafeNativeMethods.CRhinoRadialDimension_InternalCommitChanges;
    }
  }

  /// <summary>
  /// Angular style dimension
  /// </summary>
  public class AngularDimensionObject : AnnotationObjectBase
  {
    internal AngularDimensionObject(uint serialNumber)
      : base(serialNumber) { }

    internal override CommitGeometryChangesFunc GetCommitFunc()
    {
      return UnsafeNativeMethods.CRhinoAngularDimension_InternalCommitChanges;
    }
  }

  //public class OrdinateDimension : AnnotationObject { }

  /// <summary>
  /// Represents a text dot that is a document.
  /// </summary>
  public class TextDotObject : RhinoObject
  {
    internal TextDotObject(uint serialNumber)
      : base(serialNumber) { }

    internal override CommitGeometryChangesFunc GetCommitFunc()
    {
      return UnsafeNativeMethods.CRhinoTextDot_InternalCommitChanges;
    }
  }
  //public class AnnotationTextEx : AnnotationEx { }

  /// <summary>
  /// Represents a text object in a document.
  /// <para>This is a wrapper for CRhinoAnnotationText.</para>
  /// </summary>
  public class TextObject : AnnotationObjectBase
  {
    internal TextObject(uint serialNumber)
      : base(serialNumber)
    { }

    internal override CommitGeometryChangesFunc GetCommitFunc()
    {
      return UnsafeNativeMethods.CRhinoAnnotationText_InternalCommitChanges;
    }

    /// <summary>
    /// Gets the text entity geometry of this text object.
    /// </summary>
    public Rhino.Geometry.TextEntity TextGeometry
    {
      get
      {
        Rhino.Geometry.TextEntity rc = Geometry as Rhino.Geometry.TextEntity;
        return rc;
      }
    }

  }
}


namespace Rhino.Display
{
  /// <summary>
  /// 3D aligned text with font settings.
  /// </summary>
  public class Text3d : IDisposable
  {
    #region members
    IntPtr m_ptr = IntPtr.Zero; //CRhinoAnnotationText* - not created until it is actually needed
    bool m_bDirty = true;

    Rhino.Geometry.Plane m_plane = Rhino.Geometry.Plane.WorldXY;

    string m_text = string.Empty;
    double m_height = 1;
    string m_fontface; // = null; initialized to null by runtime

    #endregion

    #region constructors
    /// <summary>
    /// Constructs a new instance of Text3d.
    /// </summary>
    /// <param name="text">Text string.</param>
    public Text3d(string text)
    {
      m_text = text;
    }

    /// <summary>
    /// Constructs a new instance of Text3d.
    /// </summary>
    /// <param name="text">Text string.</param>
    /// <param name="plane">3D Plane for text.</param>
    /// <param name="height">Height (in units) for text.</param>
    public Text3d(string text, Rhino.Geometry.Plane plane, double height)
    {
      m_text = text;
      m_plane = plane;
      m_height = height;
    }

    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~Text3d()
    {
      Dispose(false);
    }

    /// <summary>
    /// Actively reclaims unmanaged resources that this instance uses.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// For derived class implementers.
    /// <para>This method is called with argument true when class user calls Dispose(), while with argument false when
    /// the Garbage Collector invokes the finalizer, or Finalize() method.</para>
    /// <para>You must reclaim all used unmanaged resources in both cases, and can use this chance to call Dispose on disposable fields if the argument is true.</para>
    /// <para>Also, you must call the base virtual method within your overriding method.</para>
    /// </summary>
    /// <param name="disposing">true if the call comes from the Dispose() method; false if it comes from the Garbage Collector finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.ON_Object_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }

    internal IntPtr NonConstPointer()
    {
      if (IntPtr.Zero == m_ptr)
      {
        m_ptr = UnsafeNativeMethods.CRhinoAnnotationText_New();
        if (IntPtr.Zero == m_ptr)
          return IntPtr.Zero;
      }

      if (m_bDirty)
      {
        // This class needs to be reworked. We really shouldn't need the CRhinoAnnotationText
        UnsafeNativeMethods.CRhinoAnnotationText_Set(m_ptr, m_text, ref m_plane, m_height); //, m_bold, m_italic);
        m_bDirty = false;
      }
      return m_ptr;
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets or sets the text string for this Text3d object.
    /// </summary>
    public string Text
    {
      get { return m_text; }
      set
      {
        if (!string.Equals(value, m_text, StringComparison.Ordinal))
        {
          m_text = value;
          m_bDirty = true;
        }
      }
    }

    /// <summary>
    /// Gets or sets the 3D aligned plane for this Text3d object. 
    /// </summary>
    public Rhino.Geometry.Plane TextPlane
    {
      get
      {
        return m_plane;
      }
      set
      {
        if (m_plane != value)
        {
          m_plane = value;
          m_bDirty = true;
        }
      }
    }

    /// <summary>
    /// Gets or sets the height (in units) of this Text3d object. 
    /// The height should be a positive number larger than zero.
    /// </summary>
    public double Height
    {
      get { return m_height; }
      set
      {
        if (m_height != value)
        {
          m_height = value;
          m_bDirty = true;
        }
      }
    }

    static string default_font_facename;
    /// <summary>
    /// Gets or sets the FontFace name.
    /// </summary>
    public string FontFace
    {
      get
      {
        if (null == m_fontface)
        {
          if (string.IsNullOrEmpty(default_font_facename))
          {
            default_font_facename = ApplicationSettings.AppearanceSettings.DefaultFontFaceName;
          }
          m_fontface = default_font_facename;
        }
        return m_fontface;
      }
      set
      {
        m_fontface = value;
      }
    }

    /// <summary>
    /// Gets or sets whether this Text3d object will be drawn in Bold.
    /// </summary>
    public bool Bold { get; set; }

    /// <summary>
    /// Gets or sets whether this Text3d object will be drawn in Italics.
    /// </summary>
    public bool Italic { get; set; }

    /// <summary>
    /// Gets the boundingbox for this Text3d object.
    /// </summary>
    public Rhino.Geometry.BoundingBox BoundingBox
    {
      get
      {
        IntPtr ptr = NonConstPointer();
        Rhino.Geometry.BoundingBox box = Rhino.Geometry.BoundingBox.Empty;

        UnsafeNativeMethods.CRhinoAnnotationText_BoundingBox(ptr, ref box);
        return box;
      }
    }
    #endregion
  }
}
#endif