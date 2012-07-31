#pragma warning disable 1591
using System;

namespace Rhino.Render
{
#if RDK_CHECKED
  /// <summary>
  /// Represents the Sun on a little portion of Earth.
  /// </summary>
  public class Sun : IDisposable
  {
    public static Rhino.Geometry.Vector3d SunDirection(double latitude, double longitude, DateTime when)
    {
      Rhino.Geometry.Vector3d rc = new Geometry.Vector3d();
      bool local = (when.Kind == DateTimeKind.Local || when.Kind == DateTimeKind.Unspecified);
      UnsafeNativeMethods.Rdk_Sun_SunDirection(latitude, longitude, local, when.Year, when.Month, when.Day, when.Hour, when.Minute, when.Second, ref rc);
      return rc;
    }

    readonly Rhino.RhinoDoc m_doc;
    IntPtr m_pLocalSun;

    // Only access to this class is through the Sun property on the document's light table.
    // That property calls CheckForRdk so we don't need to "recheck" for functions/properties
    // in this class.
    internal Sun(Rhino.RhinoDoc doc)
    {
      m_doc = doc;
      m_pLocalSun = IntPtr.Zero;
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Create a non-document controlled Sun
    /// </summary>
    public Sun()
    {
      m_pLocalSun = UnsafeNativeMethods.Rdk_SunNew();
      m_doc = null;
    }

    ~Sun()
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
      if (m_pLocalSun != IntPtr.Zero)
      {
        UnsafeNativeMethods.Rdk_SunDelete(m_pLocalSun);
      }
      m_pLocalSun = IntPtr.Zero;
    }


    /// <summary>Turn to sun on/off in this document.</summary>
    public bool Enabled
    {
      get
      {
        if (null == m_doc)
          return false;

        IntPtr pConstSun = ConstPointer();
        return UnsafeNativeMethods.Rdk_Sun_Enabled(pConstSun);
      }
      set
      {
        if( m_doc!=null )
          UnsafeNativeMethods.Rdk_Sun_SetEnabled(NonConstPointer(), value);
      }
    }

    /// <summary>
    /// Angle in degrees on world X-Y plane that should be considered north in the model. Angle is
    /// measured starting at X-Axis and travels counterclockwise. Y-Axis would be a north angle of 90
    /// degrees.
    /// </summary>
    public double North
    {
      get
      {
        IntPtr pConstSun = ConstPointer();
        return UnsafeNativeMethods.Rdk_Sun_North(pConstSun);
      }
      set
      {
        IntPtr pSun = NonConstPointer();
        UnsafeNativeMethods.Rdk_Sun_SetNorth(pSun, value);
      }
    }

    public Rhino.Geometry.Vector3d Vector
    {
      get
      {
        Rhino.Geometry.Vector3d v = new Rhino.Geometry.Vector3d();
        UnsafeNativeMethods.Rdk_Sun_Vector(ConstPointer(), ref v);
        return v;
      }
      //set { UnsafeNativeMethods.Rdk_Sun_SetVector(NonConstPointer(), value); }
    }

    /// <summary>
    /// Sets position of the Sun based on azimuth and altitude values.
    /// </summary>
    /// <param name="azimuthDegrees">The azimut sun angle in degrees.</param>
    /// <param name="altitudeDegrees">The altitude sun angle in degrees.</param>
    public void SetPosition(double azimuthDegrees, double altitudeDegrees)
    {
      IntPtr pSun = NonConstPointer();
      UnsafeNativeMethods.Rdk_Sun_SetAzimuthAltitude(pSun, azimuthDegrees, altitudeDegrees);
    }

    /// <summary>
    /// Sets position of the sun based on physical location and time.
    /// </summary>
    /// <param name="when">A datetime instance.
    /// <para>If the date <see cref="System.DateTime.Kind">Kind</see> is <see cref="System.DateTimeKind.Local">DateTimeKind.Local</see>,
    /// or <see cref="System.DateTimeKind.Unspecified">DateTimeKind.Unspecified</see>, the date is considered local.</para></param>
    /// <param name="latitudeDegrees">The latitude, in degrees, of the location on Earth.</param>
    /// <param name="longitudeDegrees">The longitude, in degrees, of the location on Earth.</param>
    public void SetPosition(DateTime when, double latitudeDegrees, double longitudeDegrees)
    {
      IntPtr pSun = NonConstPointer();
      UnsafeNativeMethods.Rdk_Sun_SetLatitudeLongitude(pSun, latitudeDegrees, longitudeDegrees);
      bool local = (when.Kind == DateTimeKind.Local || when.Kind == DateTimeKind.Unspecified);
      UnsafeNativeMethods.Rdk_Sun_SetDateTime(pSun, local, when.Year, when.Month, when.Day, when.Hour, when.Minute, when.Second);
    }

    public double Azimuth
    {
      get
      {
        IntPtr pConstSun = ConstPointer();
        return UnsafeNativeMethods.Rdk_Sun_Azimuth(pConstSun);
      }
    }

    public double Altitude
    {
      get
      {
        IntPtr pConstSun = ConstPointer();
        return UnsafeNativeMethods.Rdk_Sun_Altitude(pConstSun);
      }
    }

    public double Latitude
    {
      get
      {
        IntPtr pConstSun = ConstPointer();
        return UnsafeNativeMethods.Rdk_Sun_Latitude(pConstSun);
      }
    }

    public double Longitude
    {
      get
      {
        IntPtr pConstSun = ConstPointer();
        return UnsafeNativeMethods.Rdk_Sun_Longitude(pConstSun);
      }
    }

    public DateTime GetDateTime(DateTimeKind kind)
    {
      if( kind==DateTimeKind.Unspecified )
        throw new ArgumentException("kind must be specified");
      IntPtr pConstSun = ConstPointer();

      int month = 0, year = 0, day = 0, hours = 0, minutes = 0, seconds = 0;
      if( kind== DateTimeKind.Local )
        UnsafeNativeMethods.Rdk_Sun_LocalDateTime(pConstSun, ref year, ref month, ref day, ref hours, ref minutes, ref seconds);
      else
        UnsafeNativeMethods.Rdk_Sun_UTCDateTime(pConstSun, ref year, ref month, ref day, ref hours, ref minutes, ref seconds);
      
      return new DateTime(year, month, day, hours, minutes, seconds);
    }
    
    /// <summary>Show the tabbed sun dialog.</summary>
    public void ShowDialog()
    {
      if (m_doc != null)
      {
        IntPtr pSun = NonConstPointer();
        UnsafeNativeMethods.Rdk_Sun_ShowDialog(pSun);
      }
    }

    internal IntPtr ConstPointer()
    {
      if (m_pLocalSun != IntPtr.Zero)
        return UnsafeNativeMethods.Rdk_SunInterface(m_pLocalSun);
      return UnsafeNativeMethods.Rdk_DocSunInterface(m_doc.m_docId);
    }

    IntPtr NonConstPointer()
    {
      if (m_pLocalSun != IntPtr.Zero)
        return UnsafeNativeMethods.Rdk_SunInterface(m_pLocalSun);
      return UnsafeNativeMethods.Rdk_DocSunInterface(m_doc.m_docId);
    }
  }
#endif




#if RDK_UNCHECKED
  public class Skylight
  {
    private readonly Rhino.RhinoDoc m_doc;

    internal Skylight(Rhino.RhinoDoc doc)
    {
      m_doc = doc;
    }

    public bool Enabled
    {
      get { return UnsafeNativeMethods.Rdk_Sun_SkylightOn(ConstDocSunPointer()); }
      set { UnsafeNativeMethods.Rdk_Sun_SetSkylightOn(NonConstDocSunPointer(), value); }
    }

    public double ShadowIntensity
    {
      get { return UnsafeNativeMethods.Rdk_Sun_SkylightShadowIntensity(ConstDocSunPointer()); }
      set { UnsafeNativeMethods.Rdk_Sun_SetSkylightShadowIntensity(NonConstDocSunPointer(), value); }
    }

    public bool CustomEnvironmentOn
    {
      get { return UnsafeNativeMethods.Rdk_Sun_SkylightCustomEnvironmentOn(ConstDocSunPointer()); }
      set { UnsafeNativeMethods.Rdk_Sun_SetSkylightCustomEnvironmentOn(NonConstDocSunPointer(), value); }
    }

    public Guid CustomEnvironment
    {
      get { return UnsafeNativeMethods.Rdk_Sun_SkylightCustomEnvironment(ConstDocSunPointer()); }
      set { UnsafeNativeMethods.Rdk_Sun_SetSkylightCustomEnvironment(NonConstDocSunPointer(), value); }
    }


    IntPtr ConstDocSunPointer()
    {
      return UnsafeNativeMethods.Rdk_DocSun(m_doc.m_docId);
    }

    IntPtr NonConstDocSunPointer()
    {
      return UnsafeNativeMethods.Rdk_DocSun(m_doc.m_docId);
    }
  }
#endif

}
