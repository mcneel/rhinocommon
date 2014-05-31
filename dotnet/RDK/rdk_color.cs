#pragma warning disable 1591
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
//using System.Security.Permissions;

namespace Rhino.Display
{
  /// <summary>
  /// Color defined by 4 floating point values.
  /// </summary>
  [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 16)]
  [DebuggerDisplay("{m_r}, {m_g}, {m_b}, {m_a}")]
  //[Serializable]
  public struct Color4f //: ISerializable
  {
    readonly float m_r;
    readonly float m_g;
    readonly float m_b;
    readonly float m_a;

    public Color4f(Rhino.Drawing.Color color)
    {
      m_r = color.R / 255.0f;
      m_g = color.G / 255.0f;
      m_b = color.B / 255.0f;
      m_a = color.A / 255.0f;
    }

    public Color4f(Color4f color)
    {
      m_r = color.R;
      m_g = color.G;
      m_b = color.B;
      m_a = color.A;
    }

    public Color4f(float red, float green, float blue, float alpha)
    {
      m_r = red;
      m_g = green;
      m_b = blue;
      m_a = alpha;
    }

    //private Color4f(SerializationInfo info, StreamingContext context)
    //{
    //  m_r = info.GetSingle("R");
    //  m_g = info.GetSingle("G");
    //  m_b = info.GetSingle("B");
    //  m_a = info.GetSingle("A");
    //}

    //[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
    //void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    //{
    //  info.AddValue("R", m_r);
    //  info.AddValue("G", m_g);
    //  info.AddValue("B", m_b);
    //  info.AddValue("A", m_a);
    //}


    public static Color4f Empty
    {
      get { return new Color4f(0, 0, 0, 0); }
    }

    public static Color4f Black
    {
      get { return new Color4f(0, 0, 0, 1); }
    }

    public static Color4f White
    {
      get { return new Color4f(1, 1, 1, 1); }
    }

    public float R { get { return m_r; } }
    public float G { get { return m_g; } }
    public float B { get { return m_b; } }
    public float A { get { return m_a; } }

    public static bool operator ==(Color4f a, Color4f b)
    {
      return (a.m_r == b.m_r && a.m_g == b.m_g && a.m_b == b.m_b && a.m_a == b.m_a);
    }

    public static bool operator !=(Color4f a, Color4f b)
    {
      return (a.m_r != b.m_r || a.m_g != b.m_g || a.m_b != b.m_b || a.m_a != b.m_a);
    }

    public override bool Equals(object obj)
    {
      return (obj is Color4f && this == (Color4f)obj);
    }

    public override int GetHashCode()
    {
      // MSDN docs recommend XOR'ing the internal values to get a hash code
      return m_r.GetHashCode() ^ m_g.GetHashCode() ^ m_b.GetHashCode() ^ m_a.GetHashCode();
    }


    public Color4f BlendTo(float t, Color4f col)
    {
      float r = m_r + (t * (col.m_r - m_r));
      float g = m_g + (t * (col.m_g - m_g));
      float b = m_b + (t * (col.m_b - m_b));
      float a = m_a + (t * (col.m_a - m_a));

      return new Color4f(r, g, b, a);
    }

    public Rhino.Drawing.Color AsSystemColor()
    {
      return Rhino.Drawing.Color.FromArgb((int)(m_a * 255.0f),
                                           (int)(m_r * 255.0f),
                                           (int)(m_g * 255.0f),
                                           (int)(m_b * 255.0f));
    }
  }
}
