using System;
using System.Runtime.InteropServices;

#if USING_RDK
namespace Rhino.Render
{
  public class SimulatedTexture : IDisposable
  {
    private IntPtr m_pSim = IntPtr.Zero;
    private Rhino.Render.SimulatedEnvironment m_parent_simulated_environment;
    private bool m_bAutoDelete = true;

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

    public Rhino.Geometry.Transform LocalMappingTransform
    {
      get
      {
        Rhino.Geometry.Transform xform = new Rhino.Geometry.Transform();
        UnsafeNativeMethods.Rdk_SimulatedTexture_LocalMappingTransform(m_pSim, ref xform);
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
          UnsafeNativeMethods.Rdk_SimulatedTexture_Filename(m_pSim, pString);
          return sh.ToString();
        }
      }
      set
      {
        UnsafeNativeMethods.Rdk_SimulatedTexture_SetFilename(m_pSim, value);
      }
    }

    public String OriginalFilename
    {
      get
      {
        using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.Rdk_SimulatedTexture_OriginalFilename(m_pSim, pString);
          return sh.ToString();
        }
      }
    }

    public Rhino.Geometry.Vector2d Repeat
    {
      get
      {
        Rhino.Geometry.Vector2d v = new Rhino.Geometry.Vector2d();
        UnsafeNativeMethods.Rdk_SimulatedTexture_Repeat(m_pSim, ref v);
        return v;
      }
      set
      {
        UnsafeNativeMethods.Rdk_SimulatedTexture_SetRepeat(m_pSim, value);
      }
    }

    public Rhino.Geometry.Vector2d Offset
    {
      get
      {
        Rhino.Geometry.Vector2d v = new Rhino.Geometry.Vector2d();
        UnsafeNativeMethods.Rdk_SimulatedTexture_Offset(m_pSim, ref v);
        return v;
      }
      set
      {
        UnsafeNativeMethods.Rdk_SimulatedTexture_SetOffset(m_pSim, value);
      }
    }

    public double Rotation
    {
      get
      {
        return UnsafeNativeMethods.Rdk_SimulatedTexture_Rotation(m_pSim);
      }
      set
      {
        UnsafeNativeMethods.Rdk_SimulatedTexture_SetRotation(m_pSim, value);
      }
    }

    public bool Repeating
    {
      get
      {
        return UnsafeNativeMethods.Rdk_SimulatedTexture_Repeating(m_pSim);
      }
      set
      {
        UnsafeNativeMethods.Rdk_SimulatedTexture_SetRepeating(m_pSim, value);
      }
    }

    public int MappingChannel
    {
      get
      {
        return UnsafeNativeMethods.Rdk_SimulatedTexture_MappingChannel(m_pSim);
      }
      set
      {
        UnsafeNativeMethods.Rdk_SimulatedTexture_SetMappingChannel(m_pSim, value);
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
        UnsafeNativeMethods.Rdk_SimulatedTexture_SetProjectionMode(m_pSim, (int)value);
      }
    }

    public bool HasTransparentColor
    {
      get
      {
        return UnsafeNativeMethods.Rdk_SimulatedTexture_HasTransparentColor(m_pSim);
      }
      set
      {
        UnsafeNativeMethods.Rdk_SimulatedTexture_SetHasTransparentColor(m_pSim, value);
      }
    }

    public Rhino.Display.Color4f TransparentColor
    {
      get
      {
        Rhino.Display.Color4f color = new Rhino.Display.Color4f();
        UnsafeNativeMethods.Rdk_SimulatedTexture_TransparentColor(m_pSim, ref color);
        return color;
      }
      set
      {
        UnsafeNativeMethods.Rdk_SimulatedTexture_SetTransparentColor(m_pSim, value);
      }
    }

    public double TransparentColorSensitivity
    {
      get
      {
        return UnsafeNativeMethods.Rdk_SimulatedTexture_TransparentColorSensitivity(m_pSim);
      }
      set
      {
        UnsafeNativeMethods.Rdk_SimulatedTexture_SetTransparentColorSensitivity(m_pSim, value);
      }
    }

    public bool Filtered
    {
      get
      {
        return UnsafeNativeMethods.Rdk_SimulatedTexture_Filtered(m_pSim);
      }
      set
      {
        UnsafeNativeMethods.Rdk_SimulatedTexture_SetFiltered(m_pSim, value);
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
      return UnsafeNativeMethods.Rdk_SimulatedTexture_UnitsToMeters(m_pSim, units);
    }

    public double MetersToUnits(double units)
    {
      return UnsafeNativeMethods.Rdk_SimulatedTexture_MetersToUnits(m_pSim, units);
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