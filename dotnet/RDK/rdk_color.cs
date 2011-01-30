using System;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace Rhino.Display
{
  // Equivalent to RhRdkColor
  [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 16)]
  [DebuggerDisplay("{m_r}, {m_g}, {m_b}, m_a")]
  [Serializable]
  public struct Color4f
  {
    readonly float m_r;
    readonly float m_g;
    readonly float m_b;
    readonly float m_a;

    public Color4f(System.Drawing.Color color)
    {
      m_r = color.R / 255.0f;
      m_g = color.G / 255.0f;
      m_b = color.B / 255.0f;
      m_a = color.A / 255.0f;
    }

    public static Color4f Empty
    {
      get { return new Color4f(0, 0, 0, 0); }
    }

    public Color4f(float red, float green, float blue, float alpha)
    {
      m_r = red;
      m_g = green;
      m_b = blue;
      m_a = alpha;
    }

    public float R { get { return m_r; } }
    public float G { get { return m_g; } }
    public float B { get { return m_b; } }
    public float A { get { return m_a; } }

    public static bool operator ==(Color4f a, Color4f b)
    {
      return (a.m_r == b.m_r && a.m_g == b.m_g && a.m_b == b.m_b && a.m_a == b.m_a) ? true : false;
    }
    public static bool operator !=(Color4f a, Color4f b)
    {
      return (a.m_r != b.m_r || a.m_g != b.m_g || a.m_b != b.m_b || a.m_a != b.m_a) ? true : false;
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

  }
}
