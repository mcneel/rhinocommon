#pragma warning disable 1591
using System;

#if RDK_UNCHECKED
namespace Rhino.Render
{
  public class SimulatedTexture : IDisposable
  {
    private IntPtr m_pSim = IntPtr.Zero;
    private readonly Rhino.Render.SimulatedEnvironment m_parent_simulated_environment;
    private readonly bool m_bAutoDelete = true;

    public SimulatedTexture()
    {
      m_pSim = UnsafeNativeMethods.Rdk_SimulatedTexture_New();
    }

    internal SimulatedTexture(Rhino.Render.SimulatedEnvironment parent)
    {
      m_parent_simulated_environment = parent;
    }

    internal SimulatedTexture(IntPtr p)
    {
      m_pSim = p;
      m_bAutoDelete = false;
    }

    ~SimulatedTexture()
    {
      Dispose(false);
    }

    public static int BitmapSize
    {
      get
      {
        return UnsafeNativeMethods.Rdk_SimulatedTexture_TextureSize();
      }
      set
      {
        UnsafeNativeMethods.Rdk_SimulatedTexture_SetTextureSize(value);
      }
    }

    public Rhino.Geometry.Transform LocalMappingTransform
    {
      get
      {
        Rhino.Geometry.Transform xform = new Rhino.Geometry.Transform();
        UnsafeNativeMethods.Rdk_SimulatedTexture_LocalMappingTransform(ConstPointer(), ref xform);
        return xform;
      }
    }

    public String Filename
    {
      get
      {
        using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.Rdk_SimulatedTexture_Filename(ConstPointer(), pString);
          return sh.ToString();
        }
      }
      set
      {
        UnsafeNativeMethods.Rdk_SimulatedTexture_SetFilename(ConstPointer(), value);
      }
    }

    public String OriginalFilename
    {
      get
      {
        using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.Rdk_SimulatedTexture_OriginalFilename(ConstPointer(), pString);
          return sh.ToString();
        }
      }
    }

    public Rhino.Geometry.Vector2d Repeat
    {
      get
      {
        Rhino.Geometry.Vector2d v = new Rhino.Geometry.Vector2d();
        UnsafeNativeMethods.Rdk_SimulatedTexture_Repeat(ConstPointer(), ref v);
        return v;
      }
      set
      {
        UnsafeNativeMethods.Rdk_SimulatedTexture_SetRepeat(ConstPointer(), value);
      }
    }

    public Rhino.Geometry.Vector2d Offset
    {
      get
      {
        Rhino.Geometry.Vector2d v = new Rhino.Geometry.Vector2d();
        UnsafeNativeMethods.Rdk_SimulatedTexture_Offset(ConstPointer(), ref v);
        return v;
      }
      set
      {
        UnsafeNativeMethods.Rdk_SimulatedTexture_SetOffset(ConstPointer(), value);
      }
    }

    public double Rotation
    {
      get
      {
        return UnsafeNativeMethods.Rdk_SimulatedTexture_Rotation(ConstPointer());
      }
      set
      {
        UnsafeNativeMethods.Rdk_SimulatedTexture_SetRotation(ConstPointer(), value);
      }
    }

    public bool Repeating
    {
      get
      {
        return UnsafeNativeMethods.Rdk_SimulatedTexture_Repeating(ConstPointer());
      }
      set
      {
        UnsafeNativeMethods.Rdk_SimulatedTexture_SetRepeating(ConstPointer(), value);
      }
    }

    public int MappingChannel
    {
      get
      {
        return UnsafeNativeMethods.Rdk_SimulatedTexture_MappingChannel(ConstPointer());
      }
      set
      {
        UnsafeNativeMethods.Rdk_SimulatedTexture_SetMappingChannel(ConstPointer(), value);
      }
    }

    public enum ProjectionModes : int
    {
      MappingChannel = 0,
      View = 1,
      Wcs = 2,
      Emap = 3,
      WcsBox = 4,
      Screen = 5,
    }

    public ProjectionModes ProjectionMode
    {
      get
      {
        return (ProjectionModes)UnsafeNativeMethods.Rdk_SimulatedTexture_ProjectionMode(m_pSim);
      }
      set
      {
        UnsafeNativeMethods.Rdk_SimulatedTexture_SetProjectionMode(ConstPointer(), (int)value);
      }
    }

    public bool HasTransparentColor
    {
      get
      {
        return UnsafeNativeMethods.Rdk_SimulatedTexture_HasTransparentColor(ConstPointer());
      }
      set
      {
        UnsafeNativeMethods.Rdk_SimulatedTexture_SetHasTransparentColor(ConstPointer(), value);
      }
    }

    public Rhino.Display.Color4f TransparentColor
    {
      get
      {
        Rhino.Display.Color4f color = new Rhino.Display.Color4f();
        UnsafeNativeMethods.Rdk_SimulatedTexture_TransparentColor(ConstPointer(), ref color);
        return color;
      }
      set
      {
        UnsafeNativeMethods.Rdk_SimulatedTexture_SetTransparentColor(ConstPointer(), value);
      }
    }

    public double TransparentColorSensitivity
    {
      get
      {
        return UnsafeNativeMethods.Rdk_SimulatedTexture_TransparentColorSensitivity(ConstPointer());
      }
      set
      {
        UnsafeNativeMethods.Rdk_SimulatedTexture_SetTransparentColorSensitivity(ConstPointer(), value);
      }
    }

    public bool Filtered
    {
      get
      {
        return UnsafeNativeMethods.Rdk_SimulatedTexture_Filtered(ConstPointer());
      }
      set
      {
        UnsafeNativeMethods.Rdk_SimulatedTexture_SetFiltered(ConstPointer(), value);
      }
    }

    //TODO:
    /* Calling GetColorAdjuster() can only be done once - it will return NULL the second time.
      The call transfers ownership of the IColorAdjuster object to the caller. */
    //CRhRdkTexture::IColorAdjuster* GetColorAdjuster(void) const;

    /* The call transfers ownership of the IColorAdjuster object to this object. */
    //void SetColorAdjuster(CRhRdkTexture::IColorAdjuster*);

    public double UnitsToMeters(double units)
    {
      return UnsafeNativeMethods.Rdk_SimulatedTexture_UnitsToMeters(ConstPointer(), units);
    }

    public double MetersToUnits(double units)
    {
      return UnsafeNativeMethods.Rdk_SimulatedTexture_MetersToUnits(ConstPointer(), units);
    }

    public Rhino.DocObjects.Texture Texture()
    {
      Rhino.DocObjects.Texture texture = new Rhino.DocObjects.Texture(this);

      return texture;
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_pSim)
      {
        if (m_bAutoDelete)
        {
          UnsafeNativeMethods.Rdk_SimulatedTexture_Delete(m_pSim);
        }
        m_pSim = IntPtr.Zero;
      }
    }

    public IntPtr ConstPointer()
    {
      if (m_pSim != IntPtr.Zero)
      {
        return m_pSim;
      }
      return UnsafeNativeMethods.Rdk_SimulatedEnvironment_Texture(m_parent_simulated_environment.ConstPointer());
    }



  }
}
#endif