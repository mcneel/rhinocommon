using System;
using System.Runtime.InteropServices;


namespace Rhino.Geometry
{
  [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 8)]
  public struct Point2f
  {
    #region members
    internal float m_x;
    internal float m_y;
    #endregion

    #region constructors
    /// <summary>
    /// Create a new two-dimensional vector from two components.
    /// </summary>
    /// <param name="x">X component of vector.</param>
    /// <param name="y">Y component of vector.</param>
    public Point2f(float x, float y)
    {
      m_x = x;
      m_y = y;
    }

    /// <summary>
    /// Create a new two-dimensional vector from two components.
    /// </summary>
    /// <param name="x">X component of vector.</param>
    /// <param name="y">Y component of vector.</param>
    public Point2f(double x, double y)
    {
      m_x = (float)x;
      m_y = (float)y;
    }

    /// <summary>
    /// Gets the standard Unset point.
    /// </summary>
    public static Point2f Unset
    {
      get
      {
        return new Point2f(RhinoMath.UnsetSingle, RhinoMath.UnsetSingle);
      }
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets a value indicating whether or not this Point is considered valid.
    /// </summary>
    public bool IsValid
    {
      get
      {
        return RhinoMath.IsValidSingle(m_x) &&
               RhinoMath.IsValidSingle(m_y);
      }
    }

    /// <summary>
    /// Gets or sets the X component of the vector.
    /// </summary>
    public float X { get { return m_x; } set { m_x = value; } }

    /// <summary>
    /// Gets or sets the Y component of the vector.
    /// </summary>
    public float Y { get { return m_y; } set { m_y = value; } }
    #endregion

    public override bool Equals(object obj)
    {
      return (obj is Point2f && this == (Point2f)obj);
    }
    public override int GetHashCode()
    {
      // MSDN docs recommend XOR'ing the internal values to get a hash code
      return m_x.GetHashCode() ^ m_y.GetHashCode();
    }
    public static bool operator ==(Point2f a, Point2f b)
    {
      return (a.m_x == b.m_x && a.m_y == b.m_y);
    }
    public static bool operator !=(Point2f a, Point2f b)
    {
      return (a.m_x != b.m_x || a.m_y != b.m_y);
    }
  }

  [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
  public struct Point3f
  {
    internal float m_x;
    internal float m_y;
    internal float m_z;

    public Point3f(float x, float y, float z)
    {
      m_x = x;
      m_y = y;
      m_z = z;
    }

    public float X { get { return m_x; } set { m_x = value; } }
    public float Y { get { return m_y; } set { m_y = value; } }
    public float Z { get { return m_z; } set { m_z = value; } }


    public override bool Equals(object obj)
    {
      return (obj is Point3f && this == (Point3f)obj);
    }
    public override int GetHashCode()
    {
      // MSDN docs recommend XOR'ing the internal values to get a hash code
      return m_x.GetHashCode() ^ m_y.GetHashCode() ^ m_z.GetHashCode();
    }
    public static bool operator ==(Point3f a, Point3f b)
    {
      return (a.m_x == b.m_x && a.m_y == b.m_y && a.m_z == b.m_z);
    }
    public static bool operator !=(Point3f a, Point3f b)
    {
      return (a.m_x != b.m_x || a.m_y != b.m_y || a.m_z != b.m_z);
    }
  }

  //skipping ON_4fPoint. I don't think I've ever seen this used in Rhino.

  [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 8)]
  public struct Vector2f
  {
    internal float m_x;
    internal float m_y;

    public float X { get { return m_x; } set { m_x = value; } }
    public float Y { get { return m_y; } set { m_y = value; } }

    public override bool Equals(object obj)
    {
      return (obj is Vector2f && this == (Vector2f)obj);
    }
    public override int GetHashCode()
    {
      // MSDN docs recommend XOR'ing the internal values to get a hash code
      return m_x.GetHashCode() ^ m_y.GetHashCode();
    }
    public static bool operator ==(Vector2f a, Vector2f b)
    {
      return (a.m_x == b.m_x && a.m_y == b.m_y);
    }
    public static bool operator !=(Vector2f a, Vector2f b)
    {
      return (a.m_x != b.m_x || a.m_y != b.m_y);
    }
  }

  [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
  public struct Vector3f
  {
    #region members
    internal float m_x;
    internal float m_y;
    internal float m_z;
    #endregion

    #region constructors
    /// <summary>
    /// Create a new vector from 3 single precision numbers.
    /// </summary>
    /// <param name="x">X component of vector.</param>
    /// <param name="y">Y component of vector.</param>
    /// <param name="z">Z component of vector.</param>
    public Vector3f(float x, float y, float z)
    {
      m_x = x;
      m_y = y;
      m_z = z;
    }
    #endregion

    #region properties
    public float X { get { return m_x; } set { m_x = value; } }
    public float Y { get { return m_y; } set { m_y = value; } }
    public float Z { get { return m_z; } set { m_z = value; } }
    #endregion

    public override bool Equals(object obj)
    {
      return (obj is Vector3f && this == (Vector3f)obj);
    }
    public override int GetHashCode()
    {
      // MSDN docs recommend XOR'ing the internal values to get a hash code
      return m_x.GetHashCode() ^ m_y.GetHashCode() ^ m_z.GetHashCode();
    }

    public override string ToString()
    {
      return String.Format(System.Globalization.CultureInfo.InvariantCulture, "({0},{1},{2})", m_x, m_y, m_z);
    }

    public static bool operator ==(Vector3f a, Vector3f b)
    {
      return (a.m_x == b.m_x && a.m_y == b.m_y && a.m_z == b.m_z);
    }
    public static bool operator !=(Vector3f a, Vector3f b)
    {
      return (a.m_x != b.m_x || a.m_y != b.m_y || a.m_z != b.m_z);
    }
  }
}