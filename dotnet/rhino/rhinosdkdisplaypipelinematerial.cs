using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Rhino.Display
{
  /*
  [Obsolete("Use DisplayMaterial instead. This class will be removed in a future Rhino 5 WIP")]
  public class Material : IDisposable
  {
    #region fields
    private IntPtr m_ptr;
    internal IntPtr ConstPointer() { return m_ptr; }
    internal IntPtr NonConstPointer() { return m_ptr; }
    #endregion

    #region constructors
    /// <summary>
    /// Create a default material.
    /// </summary>
    public Material()
    {
      m_ptr = UnsafeNativeMethods.CDisplayPipelineMaterial_New(IntPtr.Zero);
    }
    /// <summary>
    /// Duplicate another material.
    /// </summary>
    public Material(Material other)
    {
      IntPtr ptr = other.ConstPointer();
      m_ptr = UnsafeNativeMethods.CDisplayPipelineMaterial_New(ptr);
    }
    /// <summary>
    /// Create a default material with a specific diffuse color.
    /// </summary>
    /// <param name="diffuse">Diffuse color of material. The alpha component of the Diffuse color is ignored.</param>
    public Material(Color diffuse)
    {
      int argb = StripAlpha(diffuse.ToArgb());
      m_ptr = UnsafeNativeMethods.CDisplayPipelineMaterial_New1(argb);
    }
    /// <summary>
    /// Create a default material with a specific diffuse color and transparency.
    /// </summary>
    /// <param name="diffuse">Diffuse color of material. The alpha component of the Diffuse color is ignored.</param>
    /// <param name="transparency">Transparency factor (0.0 = opaque, 1.0 = transparent)</param>
    public Material(Color diffuse, double transparency)
    {
      int argb = StripAlpha(diffuse.ToArgb());
      m_ptr = UnsafeNativeMethods.CDisplayPipelineMaterial_New2(argb, transparency);
    }
    /// <summary>
    /// Create a material with custom properties.
    /// </summary>
    /// <param name="diffuse">Diffuse color of material. The alpha component of the Diffuse color is ignored.</param>
    /// <param name="specular">Specular color of material. The alpha component of the Specular color is ignored.</param>
    /// <param name="ambient">Ambient color of material. The alpha component of the Ambient color is ignored.</param>
    /// <param name="emission">Emission color of material. The alpha component of the Emission color is ignored.</param>
    /// <param name="shine">Shine (highlight size) of material.</param>
    /// <param name="transparency">Transparency of material (0.0 = opaque, 1.0 = transparent)</param>
    public Material(Color diffuse, Color specular, Color ambient, Color emission, double shine, double transparency)
    {
      int argbDiffuse = StripAlpha(diffuse.ToArgb());
      int argbSpec = StripAlpha(specular.ToArgb());
      int argbAmbient = StripAlpha(ambient.ToArgb());
      int argbEmission = StripAlpha(emission.ToArgb());

      m_ptr = UnsafeNativeMethods.CDisplayPipelineMaterial_New3(argbDiffuse, argbSpec, argbAmbient, argbEmission, shine, transparency);
    }

    ~Material()
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
        UnsafeNativeMethods.CDisplayPipelineMaterial_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets or sets the Diffuse color of the Material. 
    /// The alpha component of the color will be ignored.
    /// </summary>
    public Color Diffuse
    {
      get { return GetColor(idxDiffuse); }
      set { SetColor(idxDiffuse, value); }
    }
    /// <summary>
    /// Gets or sets the Specular color of the Material. 
    /// The alpha component of the color will be ignored.
    /// </summary>
    public Color Specular
    {
      get { return GetColor(idxSpecular); }
      set { SetColor(idxSpecular, value); }
    }
    /// <summary>
    /// Gets or sets the Ambient color of the Material. 
    /// The alpha component of the color will be ignored.
    /// </summary>
    public Color Ambient
    {
      get { return GetColor(idxAmbient); }
      set { SetColor(idxAmbient, value); }
    }
    /// <summary>
    /// Gets or sets the Emissive color of the Material. 
    /// The alpha component of the color will be ignored.
    /// </summary>
    public Color Emission
    {
      get { return GetColor(idxEmission); }
      set { SetColor(idxEmission, value); }
    }

    /// <summary>
    /// Gets or sets the shine factor of the material {0.0 to 1.0}
    /// </summary>
    public double Shine
    {
      get { return GetDouble(idxShine); }
      set { SetDouble(idxShine, value); }
    }
    /// <summary>
    /// Gets or sets the transparency of the material {0.0 = opaque to 1.0 = transparent}
    /// </summary>
    public double Transparency
    {
      get { return GetDouble(idxTransparency); }
      set { SetDouble(idxTransparency, value); }
    }
    #endregion

    #region methods
    const int idxDiffuse = 0;
    const int idxSpecular = 1;
    const int idxAmbient = 2;
    const int idxEmission = 3;
    const int idxBackDiffuse = 4;
    const int idxBackSpecular = 5;
    const int idxBackAmbient = 6;
    const int idxBackEmission = 7;

    private static int m_alpha_only = Color.FromArgb(255, 0, 0, 0).ToArgb();
    private int StripAlpha(int argb)
    {
      return argb | m_alpha_only;
    }

    private Color GetColor(int which)
    {
      IntPtr ptr = ConstPointer();
      int argb = UnsafeNativeMethods.CDisplayPipelineMaterial_GetColor(ptr, which);
      return Color.FromArgb(StripAlpha(argb));
    }
    private void SetColor(int which, Color c)
    {
      IntPtr ptr = NonConstPointer();
      int argb = StripAlpha(c.ToArgb());
      UnsafeNativeMethods.CDisplayPipelineMaterial_SetColor(ptr, which, argb);
    }

    const int idxShine = 0;
    const int idxTransparency = 1;

    private double GetDouble(int which)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CDisplayPipelineMaterial_GetSetDouble(ptr, which, false, 0);
    }
    private void SetDouble(int which, double value)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.CDisplayPipelineMaterial_GetSetDouble(ptr, which, true, value);
    }
    #endregion
  }
  */

  public class DisplayMaterial : IDisposable
  {
    #region fields
    private IntPtr m_ptr;
    internal IntPtr ConstPointer() { return m_ptr; }
    internal IntPtr NonConstPointer() { return m_ptr; }
    #endregion

    #region constructors
    /// <summary>
    /// Create a default material.
    /// </summary>
    public DisplayMaterial()
    {
      m_ptr = UnsafeNativeMethods.CDisplayPipelineMaterial_New(IntPtr.Zero);
    }
    /// <summary>
    /// Duplicate another material.
    /// </summary>
    public DisplayMaterial(DisplayMaterial other)
    {
      IntPtr ptr = other.ConstPointer();
      m_ptr = UnsafeNativeMethods.CDisplayPipelineMaterial_New(ptr);
    }
    /// <summary>
    /// Create a default material with a specific diffuse color.
    /// </summary>
    /// <param name="diffuse">Diffuse color of material. The alpha component of the Diffuse color is ignored.</param>
    public DisplayMaterial(Color diffuse)
    {
      int argb = StripAlpha(diffuse.ToArgb());
      m_ptr = UnsafeNativeMethods.CDisplayPipelineMaterial_New1(argb);
    }
    /// <summary>
    /// Create a default material with a specific diffuse color and transparency.
    /// </summary>
    /// <param name="diffuse">Diffuse color of material. The alpha component of the Diffuse color is ignored.</param>
    /// <param name="transparency">Transparency factor (0.0 = opaque, 1.0 = transparent)</param>
    public DisplayMaterial(Color diffuse, double transparency)
    {
      int argb = StripAlpha(diffuse.ToArgb());
      m_ptr = UnsafeNativeMethods.CDisplayPipelineMaterial_New2(argb, transparency);
    }
    /// <summary>
    /// Create a material with custom properties.
    /// </summary>
    /// <param name="diffuse">Diffuse color of material. The alpha component of the Diffuse color is ignored.</param>
    /// <param name="specular">Specular color of material. The alpha component of the Specular color is ignored.</param>
    /// <param name="ambient">Ambient color of material. The alpha component of the Ambient color is ignored.</param>
    /// <param name="emission">Emission color of material. The alpha component of the Emission color is ignored.</param>
    /// <param name="shine">Shine (highlight size) of material.</param>
    /// <param name="transparency">Transparency of material (0.0 = opaque, 1.0 = transparent)</param>
    public DisplayMaterial(Color diffuse, Color specular, Color ambient, Color emission, double shine, double transparency)
    {
      int argbDiffuse = StripAlpha(diffuse.ToArgb());
      int argbSpec = StripAlpha(specular.ToArgb());
      int argbAmbient = StripAlpha(ambient.ToArgb());
      int argbEmission = StripAlpha(emission.ToArgb());

      m_ptr = UnsafeNativeMethods.CDisplayPipelineMaterial_New3(argbDiffuse, argbSpec, argbAmbient, argbEmission, shine, transparency);
    }

    ~DisplayMaterial()
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
        UnsafeNativeMethods.CDisplayPipelineMaterial_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets or sets the Diffuse color of the Material. 
    /// The alpha component of the color will be ignored.
    /// </summary>
    public Color Diffuse
    {
      get { return GetColor(idxDiffuse); }
      set { SetColor(idxDiffuse, value); }
    }
    /// <summary>
    /// Gets or sets the Specular color of the Material. 
    /// The alpha component of the color will be ignored.
    /// </summary>
    public Color Specular
    {
      get { return GetColor(idxSpecular); }
      set { SetColor(idxSpecular, value); }
    }
    /// <summary>
    /// Gets or sets the Ambient color of the Material. 
    /// The alpha component of the color will be ignored.
    /// </summary>
    public Color Ambient
    {
      get { return GetColor(idxAmbient); }
      set { SetColor(idxAmbient, value); }
    }
    /// <summary>
    /// Gets or sets the Emissive color of the Material. 
    /// The alpha component of the color will be ignored.
    /// </summary>
    public Color Emission
    {
      get { return GetColor(idxEmission); }
      set { SetColor(idxEmission, value); }
    }

    /// <summary>
    /// Gets or sets the shine factor of the material {0.0 to 1.0}
    /// </summary>
    public double Shine
    {
      get { return GetDouble(idxShine); }
      set { SetDouble(idxShine, value); }
    }
    /// <summary>
    /// Gets or sets the transparency of the material {0.0 = opaque to 1.0 = transparent}
    /// </summary>
    public double Transparency
    {
      get { return GetDouble(idxTransparency); }
      set { SetDouble(idxTransparency, value); }
    }
    #endregion

    #region methods
    const int idxDiffuse = 0;
    const int idxSpecular = 1;
    const int idxAmbient = 2;
    const int idxEmission = 3;
    const int idxBackDiffuse = 4;
    const int idxBackSpecular = 5;
    const int idxBackAmbient = 6;
    const int idxBackEmission = 7;

    private static int m_alpha_only = Color.FromArgb(255, 0, 0, 0).ToArgb();
    private int StripAlpha(int argb)
    {
      return argb | m_alpha_only;
    }

    private Color GetColor(int which)
    {
      IntPtr ptr = ConstPointer();
      int argb = UnsafeNativeMethods.CDisplayPipelineMaterial_GetColor(ptr, which);
      return Color.FromArgb(StripAlpha(argb));
    }
    private void SetColor(int which, Color c)
    {
      IntPtr ptr = NonConstPointer();
      int argb = StripAlpha(c.ToArgb());
      UnsafeNativeMethods.CDisplayPipelineMaterial_SetColor(ptr, which, argb);
    }

    const int idxShine = 0;
    const int idxTransparency = 1;

    private double GetDouble(int which)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CDisplayPipelineMaterial_GetSetDouble(ptr, which, false, 0);
    }
    private void SetDouble(int which, double value)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.CDisplayPipelineMaterial_GetSetDouble(ptr, which, true, value);
    }
    #endregion
  }
}