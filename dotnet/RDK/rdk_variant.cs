using System;
using System.Collections.Generic;
using System.Diagnostics;
using Rhino.Geometry;

#if RDK_UNCHECKED

namespace Rhino.Render
{
  public sealed class NamedValue
  {
    public NamedValue(string name, object value)
    {
      m_name = name;
      m_value = new Rhino.Render.Variant(value);
    }

    private string m_name = String.Empty;
    private Variant m_value = new Variant();

    public string Name
    {
      get { return m_name; }
      set { m_name = value; }
    }

    public object Value
    {
      get
      {
        return m_value.AsObject();
      }
      set
      {
        m_value.SetValue(value);
      }
    }
  }

  internal class XMLSectionUtilities
  {
    //Push the List<Rhino.Render.NamedValue> values into the IRhRdk_XMLSection
    public static void SetFromNamedValueList(IntPtr pXmlSection, List<Rhino.Render.NamedValue> list)
    {
      if (pXmlSection == IntPtr.Zero)
        return;

      if (null == list)
        return;

      foreach (Rhino.Render.NamedValue nv in list)
      {
        Rhino.Render.Variant variant = new Rhino.Render.Variant(nv.Value);
        UnsafeNativeMethods.Rdk_XmlSection_SetParam(pXmlSection, nv.Name, variant.ConstPointer());
      }
    }
          
    public static List<NamedValue> ConvertToNamedValueList(IntPtr pXmlSection)
    {
      if (IntPtr.Zero == pXmlSection)
        return null;

      List<Rhino.Render.NamedValue> list = new List<NamedValue>();

      IntPtr pIterator = UnsafeNativeMethods.Rdk_XmlSection_GetIterator(pXmlSection);

      //Fill the property list from the XML section
      if (pIterator != IntPtr.Zero)
      {
        while (true)
        {
          using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
          {
            Variant variant = new Variant();

            if (1 == UnsafeNativeMethods.Rdk_XmlSection_NextParam(pXmlSection, pIterator, sh.ConstPointer(), variant.ConstPointer()))
            {
              NamedValue nv = new NamedValue(sh.ToString(), variant.AsObject());
              list.Add(nv);
            }
            else
              break;
          }
        }

        UnsafeNativeMethods.Rdk_XmlSection_DeleteIterator(pIterator);
      }

      return list;
    }
  }

  internal sealed class Variant : IDisposable
  {
    public enum VariantTypes : int
    {
      Null = 0,
      Bool = 1,
      Integer = 2,
      Float = 3,
      Double = 4,
      Color = 5,
      Vector2d = 6,
      Vector3d = 7,
      String = 8,
      Pointer = 9,
      Uuid = 10,
      Matrix = 11,
      Time = 12,
      Buffer = 13,
      Point4d = 14,
    }

    /// <summary>
    /// Construct from a variant coming from C++
    /// </summary>
    /// <param name="pVariant"></param>
    internal Variant(IntPtr pVariant)
    {
      m_pVariant = UnsafeNativeMethods.Rdk_Variant_New(pVariant);
    }

    /// <summary>
    /// Construct as VariantTypes.Null
    /// </summary>
    public Variant()
    {
      m_pVariant = UnsafeNativeMethods.Rdk_Variant_New(IntPtr.Zero);
    }

    #region constructors
    public Variant(int  v)                    : this()  { SetValue(v); }
    public Variant(bool v)                    : this()  { SetValue(v); }
    public Variant(float v)                   : this()  { SetValue(v); }
    public Variant(double v)                  : this()  { SetValue(v); }
    public Variant(string v)                  : this()  { SetValue(v); }
    public Variant(System.Drawing.Color v)    : this()  { SetValue(v); }
    public Variant(Rhino.Display.Color4f v)   : this()  { SetValue(v); }
    public Variant(Rhino.Geometry.Vector2d v) : this()  { SetValue(v); }
    public Variant(Rhino.Geometry.Vector3d v) : this()  { SetValue(v); }
    public Variant(Rhino.Geometry.Point4d v)  : this()  { SetValue(v); }
    //public Variant(IntPtr v)                  : this()  { SetValue(v); }
    public Variant(Guid v)                    : this()  { SetValue(v); }
    public Variant(Rhino.Geometry.Transform v): this()  { SetValue(v); }
    public Variant(object v) : this() { SetValue(v); }
    public Variant(DateTime v)                : this()  { SetValue(v); }
    #endregion

    /// <summary>
    /// Units associated with numeric values, see AsModelFloat etc
    /// </summary>
    public Rhino.UnitSystem Units
    {
      get
      {
        return (Rhino.UnitSystem)UnsafeNativeMethods.Rdk_Variant_GetUnits(ConstPointer());
      }
      set
      {
        UnsafeNativeMethods.Rdk_Variant_SetUnits(NonConstPointer(), (int)value);
      }
    }

    public bool IsNull
    {
      get
      {
        return 1==UnsafeNativeMethods.Rdk_Variant_IsNull(ConstPointer());
      }
      set
      {
        UnsafeNativeMethods.Rdk_Variant_SetNull(NonConstPointer());
      }
    }

    public bool Varies
    {
      get
      {
        return 1==UnsafeNativeMethods.Rdk_Variant_Varies(ConstPointer());
      }
      set
      {
        UnsafeNativeMethods.Rdk_Variant_SetVaries(NonConstPointer());
      }
    }

    public VariantTypes Type
    {
      get
      {
        return (VariantTypes)UnsafeNativeMethods.Rdk_Variant_Type(ConstPointer());
      }
    }

    #region value setters
    public void SetValue(int v)    
    { UnsafeNativeMethods.Rdk_Variant_SetIntValue(NonConstPointer(), v); }

    public void SetValue(bool v)    
    { UnsafeNativeMethods.Rdk_Variant_SetBoolValue(NonConstPointer(), v); }

    public void SetValue(double v) 
    { UnsafeNativeMethods.Rdk_Variant_SetDoubleValue(NonConstPointer(), v); }

    public void SetValue(float v) 
    { UnsafeNativeMethods.Rdk_Variant_SetFloatValue(NonConstPointer(), v); }

    public void SetValue(string v) 
    { UnsafeNativeMethods.Rdk_Variant_SetStringValue(NonConstPointer(), v); }

    public void SetValue(System.Drawing.Color v)
    { UnsafeNativeMethods.Rdk_Variant_SetOnColorValue(NonConstPointer(), v.ToArgb()); }

    public void SetValue(Rhino.Display.Color4f v)
    { UnsafeNativeMethods.Rdk_Variant_SetRdkColorValue(NonConstPointer(), v); }

    public void SetValue(Rhino.Geometry.Vector2d v)
    { UnsafeNativeMethods.Rdk_Variant_Set2dVectorValue(NonConstPointer(), v); }

    public void SetValue(Rhino.Geometry.Vector3d v)
    { UnsafeNativeMethods.Rdk_Variant_Set3dVectorValue(NonConstPointer(), v); }

    public void SetValue(Rhino.Geometry.Point4d v)
    { UnsafeNativeMethods.Rdk_Variant_Set4dPointValue(NonConstPointer(), v); }

    //public void SetValue(IntPtr v)
    //{ UnsafeNativeMethods.Rdk_Variant_SetPointerValue(NonConstPointer(), v); }

    public void SetValue(Guid v)
    { UnsafeNativeMethods.Rdk_Variant_SetUuidValue(NonConstPointer(), v); }

    public void SetValue(Rhino.Geometry.Transform v)
    { UnsafeNativeMethods.Rdk_Variant_SetXformValue(NonConstPointer(), v); }

    public void SetValue(DateTime v)
    {
      DateTime startTime = new DateTime(1970, 1, 1);
      UInt32 time_t = Convert.ToUInt32(Math.Abs((DateTime.Now - startTime).TotalSeconds));
      UnsafeNativeMethods.Rdk_Variant_SetTimeValue(NonConstPointer(), time_t);
    }

    public void SetValue(object v)
    {
      if (v is bool) SetValue((bool)v);
      else if (v is int) SetValue((int)v);
      else if (v is float) SetValue((float)v);
      else if (v is double) SetValue((double)v);
      else if (v is string) SetValue((string)v);
      else if (v is System.Drawing.Color) SetValue((System.Drawing.Color)v);
      else if (v is Rhino.Display.Color4f) SetValue((Rhino.Display.Color4f)v);
      else if (v is Rhino.Geometry.Vector2d) SetValue((Rhino.Geometry.Vector2d)v);
      else if (v is Rhino.Geometry.Vector3d) SetValue((Rhino.Geometry.Vector3d)v);
      else if (v is Rhino.Geometry.Point4d) SetValue((Rhino.Geometry.Point4d)v);
      else if (v is Guid) SetValue((Guid)v);
      else if (v is Rhino.Geometry.Transform) SetValue((Rhino.Geometry.Transform)v);
      else if (v is DateTime) SetValue((DateTime)v);
      else
      {
        throw new InvalidOperationException("Type not supported for Rhino.Rhino.Variant");
      }
    }

    
    #endregion

    #region value getters
    public object AsObject()
    {
      VariantTypes vt = this.Type;

      switch (vt)
      {
        case VariantTypes.Null:
          return null;
        case VariantTypes.Bool:
          return ToBool();
        case VariantTypes.Integer:
          return ToInt();
        case VariantTypes.Float:
          return ToFloat();
        case VariantTypes.Double:
          return ToDouble();
        case VariantTypes.Color:
          return ToColor4f();
        case VariantTypes.Vector2d:
          return ToVector2d();
        case VariantTypes.Vector3d:
          return ToVector3d();
        case VariantTypes.Point4d:
          return ToPoint4d();
        case VariantTypes.String:
          return ToString();
        case VariantTypes.Uuid:
          return ToGuid();
        case VariantTypes.Matrix:
          return ToTransform();
        case VariantTypes.Time:
          return ToDateTime();
      }
      throw new InvalidOperationException("Type not supported by Rhino.Render.Variant");
    }

    public int ToInt()
    { return UnsafeNativeMethods.Rdk_Variant_GetIntValue(ConstPointer()); }

    public bool ToBool()
    { return 1==UnsafeNativeMethods.Rdk_Variant_GetBoolValue(ConstPointer()); }

    public double ToDouble()
    { return UnsafeNativeMethods.Rdk_Variant_GetDoubleValue(ConstPointer()); }

    public float ToFloat()
    { return UnsafeNativeMethods.Rdk_Variant_GetFloatValue(ConstPointer()); }

    public override string ToString()
    {
      using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
      {
        UnsafeNativeMethods.Rdk_Variant_GetStringValue(ConstPointer(), sh.NonConstPointer());
        return sh.ToString();
      }
    }

    public System.Drawing.Color ToSystemColor()
    {
      return System.Drawing.Color.FromArgb(UnsafeNativeMethods.Rdk_Variant_GetOnColorValue(ConstPointer()));
    }

    public Rhino.Display.Color4f ToColor4f()
    {
      Rhino.Display.Color4f v = new Rhino.Display.Color4f();
      UnsafeNativeMethods.Rdk_Variant_GetRdkColorValue(ConstPointer(), ref v);
      return v;
    }

    public Rhino.Geometry.Vector2d ToVector2d()
    {
      Rhino.Geometry.Vector2d v = new Rhino.Geometry.Vector2d();
      UnsafeNativeMethods.Rdk_Variant_Get2dVectorValue(ConstPointer(), ref v);
      return v;
    }

    public Rhino.Geometry.Vector3d ToVector3d()
    {
      Rhino.Geometry.Vector3d v = new Rhino.Geometry.Vector3d();
      UnsafeNativeMethods.Rdk_Variant_Get3dVectorValue(ConstPointer(), ref v);
      return v;
    }

    public Rhino.Geometry.Point4d ToPoint4d()
    {
      Rhino.Geometry.Point4d v = new Rhino.Geometry.Point4d();
      UnsafeNativeMethods.Rdk_Variant_Get4dPointValue(ConstPointer(), ref v);
      return v;
    }

    //public IntPtr AsPointer()
    //{ return UnsafeNativeMethods.Rdk_Variant_GetPointerValue(ConstPointer()); }

    public Guid ToGuid()
    { return UnsafeNativeMethods.Rdk_Variant_GetUuidValue(ConstPointer()); }

    public Rhino.Geometry.Transform ToTransform()
    {
      Rhino.Geometry.Transform v = new Rhino.Geometry.Transform();
      UnsafeNativeMethods.Rdk_Variant_GetXformValue(ConstPointer(), ref v);
      return v;
    }

    public DateTime ToDateTime()
    {
      System.DateTime dt = new DateTime(1970, 1, 1);
      dt.AddSeconds(UnsafeNativeMethods.Rdk_Variant_GetTimeValue(ConstPointer()));
      return dt;
    }
    #endregion

    #region units support
    /// <summary>
    /// Retrieve the value as a float in model units. Null or varying returns 0.0.
    /// The value will be converted from the variant's units to model units if necessary.
		/// \see Units(). \see SetUnits().
    /// </summary>
    /// <param name="document"></param>
    /// <returns></returns>
    public float AsModelFloat(Rhino.RhinoDoc document)
    {
      return UnsafeNativeMethods.Rdk_Variant_AsModelFloat(ConstPointer(), document.m_docId);
    }

    /// <summary>
    /// Retrieve the value as a double in model units. Null or varying returns 0.0.
    /// The value will be converted from the variant's units to model units if necessary.
		/// \see Units(). \see SetUnits().
    /// </summary>
    /// <param name="document"></param>
    /// <returns></returns>
    public double AsModelDouble(Rhino.RhinoDoc document)
    {
      return UnsafeNativeMethods.Rdk_Variant_AsModelDouble(ConstPointer(), document.m_docId);
    }

    /// <summary>
    /// Set the value to a float in model units.
    /// The value will be converted from model units to the variant's units if necessary. 
    /// </summary>
    /// <param name="f"></param>
    /// <param name="document"></param>
    public void SetAsModelFloat(float f, Rhino.RhinoDoc document)
    {
      UnsafeNativeMethods.Rdk_Variant_SetAsModelFloat(NonConstPointer(), f, document.m_docId);
    }

    /// <summary>
    /// Set the value to a double in model units.
    /// The value will be converted from model units to the variant's units if necessary. 
    /// </summary>
    /// <param name="d"></param>
    /// <param name="document"></param>
    public void SetAsModelDouble(double d, Rhino.RhinoDoc document)
    {
      UnsafeNativeMethods.Rdk_Variant_SetAsModelDouble(NonConstPointer(), d, document.m_docId);
    }
    #endregion

    #region internals
    private IntPtr m_pVariant = IntPtr.Zero;
    internal IntPtr ConstPointer()
    {
      return m_pVariant;
    }
    internal IntPtr NonConstPointer()
    {
      return m_pVariant;
    }
    #endregion

    #region IDisposable pattern implementation
    ~Variant()
    {
      Dispose(false);
    }
    public void Dispose()
    {
      Dispose(true);
    }
    private bool disposed = false;
    private void Dispose(bool disposing)
    {
      if (!disposed)
      {
        UnsafeNativeMethods.Rdk_Variant_Delete(m_pVariant);
        m_pVariant = IntPtr.Zero;
      }
      disposed = true;
    }
    #endregion
  }

}

#endif