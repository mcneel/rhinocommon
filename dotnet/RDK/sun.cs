#pragma warning disable 1591
using System;
using System.Runtime.InteropServices;

namespace Rhino.Render
{
#if RDK_CHECKED
  /// <summary>
  /// 
  /// </summary>
  public class Sun
  {
    Rhino.RhinoDoc m_doc;

    // Only access to this class is through the Sun property on the document's light table.
    // That property calls CheckForRdk so we don't need to "recheck" for functions/properties
    // in this class.
    internal Sun(Rhino.RhinoDoc doc) { m_doc = doc; }

    /// <summary>Turn to sun on/off in this document.</summary>
    public bool Enabled
    {
      get
      {
        IntPtr pConstSun = ConstPointer();
        return UnsafeNativeMethods.Rdk_Sun_Enabled(ConstPointer()); }
      set { UnsafeNativeMethods.Rdk_Sun_SetEnabled(NonConstPointer(), value); }
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
    /// Sets position of the sun based on azimuth and altitude values.
    /// </summary>
    /// <param name="azimuthDegrees"></param>
    /// <param name="altitudeDegrees"></param>
    public void SetPosition(double azimuthDegrees, double altitudeDegrees)
    {
      IntPtr pSun = NonConstPointer();
      UnsafeNativeMethods.Rdk_Sun_SetAzimuthAltitude(pSun, azimuthDegrees, altitudeDegrees);
    }

    /// <summary>
    /// Sets position of the sun based on physical location and time.
    /// </summary>
    /// <param name="when"></param>
    /// <param name="whenKind"></param>
    /// <param name="latitudeDegrees"></param>
    /// <param name="longitudeDegrees"></param>
    public void SetPosition(DateTime when, DateTimeKind whenKind, double latitudeDegrees, double longitudeDegrees)
    {
      IntPtr pSun = NonConstPointer();
      UnsafeNativeMethods.Rdk_Sun_SetLatitudeLongitude(pSun, latitudeDegrees, longitudeDegrees);
      bool local = true;
      if (whenKind == DateTimeKind.Utc)
        local = false;
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
      IntPtr pSun = NonConstPointer();
      UnsafeNativeMethods.Rdk_Sun_ShowDialog(pSun);
    }

    internal IntPtr ConstPointer()
    {
      return UnsafeNativeMethods.Rdk_DocSunInterface(m_doc.m_docId);
    }

    IntPtr NonConstPointer()
    {
      return UnsafeNativeMethods.Rdk_DocSunInterface(m_doc.m_docId);
    }
  }
#endif




#if RDK_UNCHECKED
  public class Skylight
  {
    private Rhino.RhinoDoc m_doc;

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
