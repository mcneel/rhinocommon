using System;
using System.Runtime.Serialization;

namespace Rhino.Geometry
{
  [Serializable]
  public class Light : GeometryBase, ISerializable
  {
    internal Light(IntPtr native_ptr, Rhino.DocObjects.RhinoObject parent_object, Rhino.DocObjects.ObjRef obj_ref)
      : base(native_ptr, parent_object, obj_ref)
    { }

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new Light(IntPtr.Zero, null, null);
    }

    public Light()
    {
      IntPtr pLight = UnsafeNativeMethods.ON_Light_New();
      ConstructNonConstObject(pLight);
    }

    // serialization constructor
    protected Light(SerializationInfo info, StreamingContext context)
      : base (info, context)
    {
    }

    /// <summary>
    /// Turn light on or off
    /// </summary>
    public bool IsEnabled
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_Light_IsEnabled(pConstThis);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Light_SetEnabled(pThis, value);
      }
    }

    const int idxLightStyle = 0;
    const int idxCoordinateSystem = 1;
    int GetInt(int which)
    {
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.ON_Light_GetInt(pConstThis, which);
    }
    void SetInt(int which, int val)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_Light_SetInt(pThis, which, val);
    }

    public LightStyle LightStyle
    {
      get { return (LightStyle)GetInt(idxLightStyle); }
      set { SetInt(idxLightStyle, (int)value); }
    }

    public bool IsPointLight
    {
      get
      {
        LightStyle ls = LightStyle;
        return ls == LightStyle.CameraPoint || ls == LightStyle.WorldPoint;
      }
    }
    public bool IsDirectionalLight
    {
      get
      {
        LightStyle ls = LightStyle;
        return ls == LightStyle.CameraDirectional || ls == LightStyle.WorldDirectional;
      }
    }
    public bool IsSpotLight
    {
      get
      {
        LightStyle ls = LightStyle;
        return ls == LightStyle.CameraSpot || ls == LightStyle.WorldSpot;
      }
    }
    public bool IsLinearLight
    {
      get { return LightStyle == LightStyle.WorldLinear; }
    }
    public bool IsRectangularLight
    {
      get { return LightStyle == LightStyle.WorldRectangular; }
    }

    public bool IsSunLight
    {
      get 
      {
          return UnsafeNativeMethods.Rdk_Sun_IsSunLight(ConstPointer()); 
      }
    }

    /// <summary>
    /// Determined by LightStyle
    /// </summary>
    public DocObjects.CoordinateSystem CoordinateSystem
    {
      get { return (DocObjects.CoordinateSystem)GetInt(idxCoordinateSystem);}
    }

    public Point3d Location
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        Point3d rc = new Point3d();
        UnsafeNativeMethods.ON_Light_GetLocation(pConstThis, ref rc);
        return rc;
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Light_SetLocation(pThis, value);
      }
    }

    const int idxDirection = 0;
    const int idxPerpendicularDirection = 1;
    const int idxLength = 2;
    const int idxWidth = 3;
    Vector3d GetVector(int which)
    {
      IntPtr pConstThis = ConstPointer();
      Vector3d rc = new Vector3d();
      UnsafeNativeMethods.ON_Light_GetVector(pConstThis, ref rc, which);
      return rc;
    }
    void SetVector(int which, Vector3d v)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_Light_SetVector(pThis, v, which);
    }

    public Vector3d Direction
    {
      get{ return GetVector(idxDirection); }
      set { SetVector(idxDirection, value); }
    }

    public Vector3d PerpendicularDirection
    {
      get { return GetVector(idxPerpendicularDirection); }
    }

    const int idxIntensity = 0;
    const int idxPowerWatts = 1;
    const int idxPowerLumens = 2;
    const int idxPowerCandela = 3;
    const int idxSpotAngleRadians = 4;
    const int idxSpotExponent = 5;
    const int idxHotSpot = 6;
    const int idxShadowIntensity = 7;
    double GetDouble(int which)
    {
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.ON_Light_GetDouble(pConstThis, which);
    }
    void SetDouble(int which, double val)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_Light_SetDouble(pThis, which, val);
    }

    public double Intensity
    {
      get { return GetDouble(idxIntensity); }
      set { SetDouble(idxIntensity, value); }
    }

    public double PowerWatts
    {
      get { return GetDouble(idxPowerWatts); }
      set { SetDouble(idxPowerWatts, value); }
    }
    public double PowerLumens
    {
      get { return GetDouble(idxPowerLumens); }
      set { SetDouble(idxPowerLumens, value); }
    }
    public double PowerCandela
    {
      get { return GetDouble(idxPowerCandela); }
      set { SetDouble(idxPowerCandela, value); }
    }

    const int idxAmbient = 0;
    const int idxDiffuse = 1;
    const int idxSpecular = 2;
    System.Drawing.Color GetColor(int which)
    {
      IntPtr pConstThis = ConstPointer();
      int argb = UnsafeNativeMethods.ON_Light_GetColor(pConstThis, which);
      return System.Drawing.Color.FromArgb(argb);
    }
    void SetColor(int which, System.Drawing.Color c)
    {
      IntPtr pThis = NonConstPointer();
      int argb = c.ToArgb();
      UnsafeNativeMethods.ON_Light_SetColor(pThis, which, argb);
    }

    public System.Drawing.Color Ambient
    {
      get { return GetColor(idxAmbient); }
      set { SetColor(idxAmbient, value); }
    }
    public System.Drawing.Color Diffuse
    {
      get { return GetColor(idxDiffuse); }
      set { SetColor(idxDiffuse, value); }
    }
    public System.Drawing.Color Specular
    {
      get { return GetColor(idxSpecular); }
      set { SetColor(idxSpecular, value); }
    }

    /// <summary>
    /// attenuation settings (ignored for "directional" and "ambient" lights)
    /// attenuation = 1/(a0 + d*a1 + d^2*a2) where d = distance to light
    /// </summary>
    /// <param name="a0"></param>
    /// <param name="a1"></param>
    /// <param name="a2"></param>
    public void SetAttenuation(double a0, double a1, double a2)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_Light_SetAttenuation(pThis, a0, a1, a2);
    }
    /// <summary>
    /// attenuation settings (ignored for "directional" and "ambient" lights)
    /// attenuation = 1/(a0 + d*a1 + d^2*a2) where d = distance to light
    /// </summary>
    /// <param name="d"></param>
    /// <returns>0 if a0 + d*a1 + d^2*a2 &lt;= 0</returns>
    public double GetAttenuation(double d)
    {
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.ON_Light_GetAttenuation(pConstThis, d);
    }

    /// <summary>
    /// ignored for non-spot lights
    /// angle = 0 to pi/2  (0 to 90 degrees)
    /// </summary>
    public double SpotAngleRadians
    {
      get{ return GetDouble(idxSpotAngleRadians); }
      set{ SetDouble(idxSpotAngleRadians, value); }
    }

    /// <summary>
    /// The spot exponent varies from 0.0 to 128.0 and provides
    /// an exponential interface for controling the focus or 
    /// concentration of a spotlight (like the 
    /// OpenGL GL_SPOT_EXPONENT parameter).  The spot exponent
    /// and hot spot parameters are linked; changing one will
    /// change the other.
    /// A hot spot setting of 0.0 corresponds to a spot exponent of 128.
    /// A hot spot setting of 1.0 corresponds to a spot exponent of 0.0.
    /// </summary>
    public double SpotExponent
    {
      get { return GetDouble(idxSpotExponent); }
      set { SetDouble(idxSpotExponent, value); }
    }

    /// <summary>
    /// The hot spot setting runs from 0.0 to 1.0 and is used to
    /// provides a linear interface for controling the focus or 
    /// concentration of a spotlight.
    /// A hot spot setting of 0.0 corresponds to a spot exponent of 128.
    /// A hot spot setting of 1.0 corresponds to a spot exponent of 0.0.
    /// </summary>
    public double HotSpot
    {
      get { return GetDouble(idxHotSpot); }
      set { SetDouble(idxHotSpot, value); }
    }

    public bool GetSpotLightRadii(out double innerRadius, out double outerRadius)
    {
      innerRadius = 0;
      outerRadius = 0;
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.ON_Light_GetSpotLightRadii(pConstThis, ref innerRadius, ref outerRadius);
    }

    /// <summary>
    /// linear and rectangular light parameter
    /// (ignored for non-linear/rectangular lights)
    /// </summary>
    public Vector3d Length
    {
      get { return GetVector(idxLength); }
      set { SetVector(idxLength, value); }
    }

    /// <summary>
    /// linear and rectangular light parameter
    /// (ignored for non-linear/rectangular lights)
    /// </summary>
    public Vector3d Width
    {
      get { return GetVector(idxWidth); }
      set { SetVector(idxWidth, value); }
    }

    public double SpotLightShadowIntensity
    {
      get { return GetDouble(idxShadowIntensity); }
      set { SetDouble(idxShadowIntensity, value); }
    }

    public string Name
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.ON_Light_GetName(pConstThis, pString);
          return sh.ToString();
        }
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Light_SetName(pThis, value);
      }
    }
  }
}
