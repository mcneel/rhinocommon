using System;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace Rhino.Geometry
{
  /// <summary>
  /// Represents the two coordinates of a point in two-dimensional space,
  /// using <see cref="Single"/>-precision floating point numbers.
  /// </summary>
  [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 8)]
  [DebuggerDisplay("({m_x}, {m_y})")]
  [Serializable]
  public struct Point2f : IEquatable<Point2f>, IComparable<Point2f>, IComparable
  {
    #region members
    internal float m_x;
    internal float m_y;
    #endregion

    #region constructors
    /// <summary>
    /// Initializes a new two-dimensional point from two components.
    /// </summary>
    /// <param name="x">X component of vector.</param>
    /// <param name="y">Y component of vector.</param>
    public Point2f(float x, float y)
    {
      m_x = x;
      m_y = y;
    }

    /// <summary>
    /// Initializes a new two-dimensional point from two double-precision floating point numbers as coordinates.
    /// <para>Coordinates will be internally converted to floating point numbers. This might cause precision loss.</para>
    /// </summary>
    /// <param name="x">X component of vector.</param>
    /// <param name="y">Y component of vector.</param>
    public Point2f(double x, double y)
    {
      m_x = (float)x;
      m_y = (float)y;
    }

    /// <summary>
    /// Gets the standard unset point.
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
    /// Gets a value indicating whether this point is considered valid.
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
    /// Gets or sets the X (first) component of the vector.
    /// </summary>
    public float X { get { return m_x; } set { m_x = value; } }

    /// <summary>
    /// Gets or sets the Y (second) component of the vector.
    /// </summary>
    public float Y { get { return m_y; } set { m_y = value; } }
    #endregion

    /// <summary>
    /// Determines whether the specified System.Object is a <see cref="Point2f"/> and has the same values as the present point.
    /// </summary>
    /// <param name="obj">The specified object.</param>
    /// <returns>true if obj is Point2f and has the same coordinates as this; otherwise false.</returns>
    public override bool Equals(object obj)
    {
      return (obj is Point2f && this == (Point2f)obj);
    }

    /// <summary>
    /// Determines whether the specified <see cref="Point2f"/> has the same values as the present point.
    /// </summary>
    /// <param name="point">The specified point.</param>
    /// <returns>true if point has the same coordinates as this; otherwise false.</returns>
    public bool Equals(Point2f point)
    {
      return this == point;
    }

    /// <summary>
    /// Compares this <see cref="Point2f" /> with another <see cref="Point2f" />.
    /// <para>Coordinates evaluation priority is first X, then Y.</para>
    /// </summary>
    /// <param name="other">The other <see cref="Point2f" /> to use in comparison.</param>
    /// <returns>
    /// <para> 0: if this is identical to other</para>
    /// <para>-1: if this.X &lt; other.X</para>
    /// <para>-1: if this.X == other.X and this.Y &lt; other.Y</para>
    /// <para>+1: otherwise.</para>
    /// </returns>
    public int CompareTo(Point2f other)
    {
      // dictionary order
      if (m_x < other.m_x)
        return -1;
      if (m_x > other.m_x)
        return 1;

      if (m_y < other.m_y)
        return -1;
      if (m_y > other.m_y)
        return 1;

      return 0;
    }

    int IComparable.CompareTo(object obj)
    {
      if (obj is Point2f)
        return CompareTo((Point2f)obj);
      else
        throw new ArgumentException("Input must be of type Point2f", "obj");
    }

    /// <summary>
    /// Computes a hash number that represents the current point.
    /// </summary>
    /// <returns>A hash code that is not unique for each point.</returns>
    public override int GetHashCode()
    {
      // MSDN docs recommend XOR'ing the internal values to get a hash code
      return m_x.GetHashCode() ^ m_y.GetHashCode();
    }

    /// <summary>
    /// Constructs the string representation for the current point.
    /// </summary>
    /// <returns>The point representation in the form X,Y.</returns>
    public override string ToString()
    {
      var culture = System.Globalization.CultureInfo.InvariantCulture;
      return String.Format("{0},{1}", m_x.ToString(culture), m_y.ToString(culture));
    }

    /// <summary>
    /// Determines whether two <see cref="Point2f"/> have equal values.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <returns>true if the coordinates of the two points are exactly equal; otherwise false.</returns>
    public static bool operator ==(Point2f a, Point2f b)
    {
      return (a.m_x == b.m_x && a.m_y == b.m_y);
    }

    /// <summary>
    /// Determines whether two <see cref="Point2f"/> have different values.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <returns>true if the two points differ in any coordinate; false otherwise.</returns>
    public static bool operator !=(Point2f a, Point2f b)
    {
      return (a.m_x != b.m_x || a.m_y != b.m_y);
    }

    /// <summary>
    /// Determines whether the first specified <see cref="Point2f"/> comes before
    /// (has inferior sorting value than) the second point.
    /// <para>Coordinates evaluation priority is first X, then Y.</para>
    /// </summary>
    /// <param name="a">First point.</param>
    /// <param name="b">Second point.</param>
    /// <returns>true if a.X is smaller than b.X, or a.X == b.X and a.Y is smaller than b.Y; otherwise, false.</returns>
    public static bool operator <(Point2f a, Point2f b)
    {
      return (a.X < b.X) || (a.X == b.X && a.Y < b.Y);
    }

    /// <summary>
    /// Determines whether the first specified <see cref="Point2f"/> comes before
    /// (has inferior sorting value than) the second point, or it is equal to it.
    /// <para>Coordinates evaluation priority is first X, then Y.</para>
    /// </summary>
    /// <param name="a">First point.</param>
    /// <param name="b">Second point.</param>
    /// <returns>true if a.X is smaller than b.X, or a.X == b.X and a.Y &lt;= b.Y; otherwise, false.</returns>
    public static bool operator <=(Point2f a, Point2f b)
    {
      return (a.X < b.X) || (a.X == b.X && a.Y <= b.Y);
    }

    /// <summary>
    /// Determines whether the first specified <see cref="Point2f"/> comes after
    /// (has superior sorting value than) the second point.
    /// <para>Coordinates evaluation priority is first X, then Y.</para>
    /// </summary>
    /// <param name="a">First point.</param>
    /// <param name="b">Second point.</param>
    /// <returns>true if a.X is larger than b.X, or a.X == b.X and a.Y is larger than b.Y; otherwise, false.</returns>
    public static bool operator >(Point2f a, Point2f b)
    {
      return (a.X > b.X) || (a.X == b.X && a.Y > b.Y);
    }

    /// <summary>
    /// Determines whether the first specified <see cref="Point2f"/> comes after
    /// (has superior sorting value than) the second point, or it is equal to it.
    /// <para>Coordinates evaluation priority is first X, then Y.</para>
    /// </summary>
    /// <param name="a">First point.</param>
    /// <param name="b">Second point.</param>
    /// <returns>true if a.X is larger than b.X, or a.X == b.X and a.Y &gt;= b.Y; otherwise, false.</returns>
    public static bool operator >=(Point2f a, Point2f b)
    {
      return (a.X > b.X) || (a.X == b.X && a.Y >= b.Y);
    }
  }

  /// <summary>
  /// Represents the three coordinates of a point in three-dimensional space,
  /// using <see cref="Single"/>-precision floating point numbers.
  /// </summary>
  // holding off on making this IComparable until I understand all
  // of the rules that FxCop states about IComparable classes
  [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
  [DebuggerDisplay("({m_x}, {m_y}, {m_z})")]
  [Serializable]
  public struct Point3f : IEquatable<Point3f>, IComparable<Point3f>, IComparable
  {
    internal float m_x;
    internal float m_y;
    internal float m_z;

    /// <summary>
    /// Initializes a new two-dimensional vector from two components.
    /// </summary>
    /// <param name="x">X component of vector.</param>
    /// <param name="y">Y component of vector.</param>
    /// <param name="z">Z component of vector.</param>
    public Point3f(float x, float y, float z)
    {
      m_x = x;
      m_y = y;
      m_z = z;
    }

    /// <summary>
    /// Gets or sets the X (first) component of the vector.
    /// </summary>
    public float X { get { return m_x; } set { m_x = value; } }

    /// <summary>
    /// Gets or sets the Y (second) component of the vector.
    /// </summary>
    public float Y { get { return m_y; } set { m_y = value; } }

    /// <summary>
    /// Gets or sets the Z (third) component of the vector.
    /// </summary>
    public float Z { get { return m_z; } set { m_z = value; } }

    /// <summary>
    /// Determines whether the specified System.Object is a Point3f and has the same values as the present point.
    /// </summary>
    /// <param name="obj">The specified object.</param>
    /// <returns>true if obj is Point3f and has the same coordinates as this; otherwise False.</returns>
    public override bool Equals(object obj)
    {
      return (obj is Point3f && this == (Point3f)obj);
    }

    /// <summary>
    /// Determines whether the specified Point3f has the same values as the present point.
    /// </summary>
    /// <param name="point">The specified point.</param>
    /// <returns>true if point has the same coordinates as this; otherwise false.</returns>
    public bool Equals(Point3f point)
    {
      return this == point;
    }

    /// <summary>
    /// Compares this <see cref="Point3f" /> with another <see cref="Point3f" />.
    /// <para>Component evaluation priority is first X, then Y, then Z.</para>
    /// </summary>
    /// <param name="other">The other <see cref="Point3d" /> to use in comparison.</param>
    /// <returns>
    /// <para> 0: if this is identical to other</para>
    /// <para>-1: if this.X &lt; other.X</para>
    /// <para>-1: if this.X == other.X and this.Y &lt; other.Y</para>
    /// <para>-1: if this.X == other.X and this.Y == other.Y and this.Z &lt; other.Z</para>
    /// <para>+1: otherwise.</para>
    /// </returns>
    public int CompareTo(Point3f other)
    {
      if (m_x < other.m_x)
        return -1;
      if (m_x > other.m_x)
        return 1;

      if (m_y < other.m_y)
        return -1;
      if (m_y > other.m_y)
        return 1;

      if (m_z < other.m_z)
        return -1;
      if (m_z > other.m_z)
        return 1;

      return 0;
    }

    int IComparable.CompareTo(object obj)
    {
      if (obj is Point3f)
        return CompareTo((Point3f)obj);
      else
        throw new ArgumentException("Input must be of type Point3f", "obj");
    }

    /// <summary>
    /// Computes a hash code for the present point.
    /// </summary>
    /// <returns>A non-unique integer that represents this point.</returns>
    public override int GetHashCode()
    {
      // MSDN docs recommend XOR'ing the internal values to get a hash code
      return m_x.GetHashCode() ^ m_y.GetHashCode() ^ m_z.GetHashCode();
    }

    /// <summary>
    /// Constructs the string representation for the current point.
    /// </summary>
    /// <returns>The point representation in the form X,Y,Z.</returns>
    public override string ToString()
    {
      return String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2}", m_x, m_y, m_z);
    }

    /// <summary>
    /// Determines whether two points have equal values.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <returns>true if the coordinates of the two points are exactly equal; otherwise false.</returns>
    public static bool operator ==(Point3f a, Point3f b)
    {
      return (a.m_x == b.m_x && a.m_y == b.m_y && a.m_z == b.m_z);
    }

    /// <summary>
    /// Determines whether two points have different values.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <returns>true if the two points differ in any coordinate; false otherwise.</returns>
    public static bool operator !=(Point3f a, Point3f b)
    {
      return (a.m_x != b.m_x || a.m_y != b.m_y || a.m_z != b.m_z);
    }

    /// <summary>
    /// Determines whether the first specified point comes before (has inferior sorting value than) the second point.
    /// <para>Coordinates evaluation priority is first X, then Y, then Z.</para>
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <returns>true if a.X is smaller than b.X,
    /// or a.X == b.X and a.Y is smaller than b.Y,
    /// or a.X == b.X and a.Y == b.Y and a.Z is smaller than b.Z;
    /// otherwise, false.</returns>
    public static bool operator <(Point3f a, Point3f b)
    {
      return a.CompareTo(b) < 0;
    }

    /// <summary>
    /// Determines whether the first specified point comes before
    /// (has inferior sorting value than) the second point, or it is equal to it.
    /// <para>Coordinates evaluation priority is first X, then Y, then Z.</para>
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <returns>true if a.X is smaller than b.X,
    /// or a.X == b.X and a.Y is smaller than b.Y,
    /// or a.X == b.X and a.Y == b.Y and a.Z &lt;= b.Z;
    /// otherwise, false.</returns>
    public static bool operator <=(Point3f a, Point3f b)
    {
      return a.CompareTo(b) <= 0;
    }

    /// <summary>
    /// Determines whether the first specified point comes after (has superior sorting value than) the second point.
    /// <para>Coordinates evaluation priority is first X, then Y, then Z.</para>
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <returns>true if a.X is larger than b.X,
    /// or a.X == b.X and a.Y is larger than b.Y,
    /// or a.X == b.X and a.Y == b.Y and a.Z is larger than b.Z;
    /// otherwise, false.</returns>
    public static bool operator >(Point3f a, Point3f b)
    {
      return a.CompareTo(b) > 0;
    }

    /// <summary>
    /// Determines whether the first specified point comes after
    /// (has superior sorting value than) the second point, or it is equal to it.
    /// <para>Coordinates evaluation priority is first X, then Y, then Z.</para>
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <returns>true if a.X is larger than b.X,
    /// or a.X == b.X and a.Y is larger than b.Y,
    /// or a.X == b.X and a.Y == b.Y and a.Z &gt;= b.Z;
    /// otherwise, false.</returns>
    public static bool operator >=(Point3f a, Point3f b)
    {
      return a.CompareTo(b) >= 0;
    }
  }

  //skipping ON_4fPoint. I don't think I've ever seen this used in Rhino.

  /// <summary>
  /// Represents the two components of a vector in two-dimensional space,
  /// using <see cref="Single"/>-precision floating point numbers.
  /// </summary>
  [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 8)]
  [DebuggerDisplay("({m_x}, {m_y})")]
  [Serializable]
  public struct Vector2f : IEquatable<Vector2f>, IComparable<Vector2f>, IComparable
  {
    internal float m_x;
    internal float m_y;

    /// <summary>
    /// Gets or sets the X (first) component of this vector.
    /// </summary>
    public float X { get { return m_x; } set { m_x = value; } }

    /// <summary>
    /// Gets or sets the Y (second) component of this vector.
    /// </summary>
    public float Y { get { return m_y; } set { m_y = value; } }

    /// <summary>
    /// Determines whether the specified System.Object is a Vector2f and has the same values as the present vector.
    /// </summary>
    /// <param name="obj">The specified object.</param>
    /// <returns>true if obj is Vector2f and has the same coordinates as this; otherwise false.</returns>
    public override bool Equals(object obj)
    {
      return (obj is Vector2f && this == (Vector2f)obj);
    }

    /// <summary>
    /// Determines whether the specified vector has the same values as the present vector.
    /// </summary>
    /// <param name="vector">The specified vector.</param>
    /// <returns>true if obj is Vector2f and has the same coordinates as this; otherwise false.</returns>
    public bool Equals(Vector2f vector)
    {
      return this == vector;
    }

    /// <summary>
    /// Compares this <see cref="Vector2f" /> with another <see cref="Vector2f" />.
    /// <para>Components evaluation priority is first X, then Y.</para>
    /// </summary>
    /// <param name="other">The other <see cref="Vector2f" /> to use in comparison.</param>
    /// <returns>
    /// <para> 0: if this is identical to other</para>
    /// <para>-1: if this.X &lt; other.X</para>
    /// <para>-1: if this.X == other.X and this.Y &lt; other.Y</para>
    /// <para>+1: otherwise.</para>
    /// </returns>
    public int CompareTo(Vector2f other)
    {
      if (m_x < other.m_x)
        return -1;
      if (m_x > other.m_x)
        return 1;

      if (m_y < other.m_y)
        return -1;
      if (m_y > other.m_y)
        return 1;

      return 0;
    }

    int IComparable.CompareTo(object obj)
    {
      if (obj is Vector2f)
        return CompareTo((Vector2f)obj);
      else
        throw new ArgumentException("Input must be of type Vector2f", "obj");
    }

    /// <summary>
    /// Computes a hash number that represents the current vector.
    /// </summary>
    /// <returns>A hash code that is not unique for each vector.</returns>
    public override int GetHashCode()
    {
      // MSDN docs recommend XOR'ing the internal values to get a hash code
      return m_x.GetHashCode() ^ m_y.GetHashCode();
    }

    /// <summary>
    /// Constructs the string representation of the current vector.
    /// </summary>
    /// <returns>The vector representation in the form X,Y,Z.</returns>
    public override string ToString()
    {
      var culture = System.Globalization.CultureInfo.InvariantCulture;
      return String.Format("{0},{1}", m_x.ToString(culture), m_y.ToString(culture));
    }

    /// <summary>
    /// Determines whether two vectors have equal values.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>true if the components of the two vectors are exactly equal; otherwise false.</returns>
    public static bool operator ==(Vector2f a, Vector2f b)
    {
      return (a.m_x == b.m_x && a.m_y == b.m_y);
    }

    /// <summary>
    /// Determines whether two vectors have different values.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>true if the two vectors differ in any component; false otherwise.</returns>
    public static bool operator !=(Vector2f a, Vector2f b)
    {
      return (a.m_x != b.m_x || a.m_y != b.m_y);
    }

    /// <summary>
    /// Determines whether the first specified vector comes before
    /// (has inferior sorting value than) the second vector.
    /// <para>Components have decreasing evaluation priority: first X, then Y.</para>
    /// </summary>
    /// <param name="a">First vector.</param>
    /// <param name="b">Second vector.</param>
    /// <returns>true if a.X is smaller than b.X, or a.X == b.X and a.Y is smaller than b.Y; otherwise, false.</returns>
    public static bool operator <(Vector2f a, Vector2f b)
    {
      return (a.X < b.X) || (a.X == b.X && a.Y < b.Y);
    }

    /// <summary>
    /// Determines whether the first specified vector comes before
    /// (has inferior sorting value than) the second vector, or it is equal to it.
    /// <para>Components have decreasing evaluation priority: first X, then Y.</para>
    /// </summary>
    /// <param name="a">First vector.</param>
    /// <param name="b">Second vector.</param>
    /// <returns>true if a.X is smaller than b.X, or a.X == b.X and a.Y &lt;= b.Y; otherwise, false.</returns>
    public static bool operator <=(Vector2f a, Vector2f b)
    {
      return (a.X < b.X) || (a.X == b.X && a.Y <= b.Y);
    }

    /// <summary>
    /// Determines whether the first specified vector comes after (has superior sorting value than) the second vector.
    /// <para>Components have decreasing evaluation priority: first X, then Y.</para>
    /// </summary>
    /// <param name="a">First vector.</param>
    /// <param name="b">Second vector.</param>
    /// <returns>true if a.X is larger than b.X, or a.X == b.X and a.Y is larger than b.Y; otherwise, false.</returns>
    public static bool operator >(Vector2f a, Vector2f b)
    {
      return (a.X > b.X) || (a.X == b.X && a.Y > b.Y);
    }

    /// <summary>
    /// Determines whether the first specified vector comes after
    /// (has superior sorting value than) the second vector, or it is equal to it.
    /// <para>Components have decreasing evaluation priority: first X, then Y.</para>
    /// </summary>
    /// <param name="a">First vector.</param>
    /// <param name="b">Second vector.</param>
    /// <returns>true if a.X is larger than b.X, or a.X == b.X and a.Y &gt;= b.Y; otherwise, false.</returns>
    public static bool operator >=(Vector2f a, Vector2f b)
    {
      return (a.X > b.X) || (a.X == b.X && a.Y >= b.Y);
    }
  }

  /// <summary>
  /// Represents the three components of a vector in three-dimensional space,
  /// using <see cref="Single"/>-precision floating point numbers.
  /// </summary>
  [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
  [DebuggerDisplay("({m_x}, {m_y}, {m_z})")]
  [Serializable]
  public struct Vector3f : IEquatable<Vector3f>, IComparable<Vector3f>, IComparable
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
    /// <summary>
    /// Gets or sets the X (first) component of this vector.
    /// </summary>
    public float X { get { return m_x; } set { m_x = value; } }

    /// <summary>
    /// Gets or sets the Y (second) component of this vector.
    /// </summary>
    public float Y { get { return m_y; } set { m_y = value; } }

    /// <summary>
    /// Gets or sets the Z (third) component of this vector.
    /// </summary>
    public float Z { get { return m_z; } set { m_z = value; } }
    #endregion

    /// <summary>
    /// Determines whether the specified System.Object is a Vector3f and has the same values as the present vector.
    /// </summary>
    /// <param name="obj">The specified object.</param>
    /// <returns>true if obj is Vector3f and has the same components as this; otherwise false.</returns>
    public override bool Equals(object obj)
    {
      return (obj is Vector3f && this == (Vector3f)obj);
    }

    /// <summary>
    /// Determines whether the specified vector has the same values as the present vector.
    /// </summary>
    /// <param name="vector">The specified vector.</param>
    /// <returns>true if vector has the same components as this; otherwise false.</returns>
    public bool Equals(Vector3f vector)
    {
      return this == vector;
    }

    /// <summary>
    /// Compares this <see cref="Vector3f" /> with another <see cref="Vector3f" />.
    /// <para>Component evaluation priority is first X, then Y, then Z.</para>
    /// </summary>
    /// <param name="other">The other <see cref="Vector3f" /> to use in comparison.</param>
    /// <returns>
    /// <para> 0: if this is identical to other</para>
    /// <para>-1: if this.X &lt; other.X</para>
    /// <para>-1: if this.X == other.X and this.Y &lt; other.Y</para>
    /// <para>-1: if this.X == other.X and this.Y == other.Y and this.Z &lt; other.Z</para>
    /// <para>+1: otherwise.</para>
    /// </returns>
    public int CompareTo(Vector3f other)
    {
      if (m_x < other.m_x)
        return -1;
      if (m_x > other.m_x)
        return 1;

      if (m_y < other.m_y)
        return -1;
      if (m_y > other.m_y)
        return 1;

      if (m_z < other.m_z)
        return -1;
      if (m_z > other.m_z)
        return 1;

      return 0;
    }

    int IComparable.CompareTo(object obj)
    {
      if (obj is Vector3f)
        return CompareTo((Vector3f)obj);
      else
        throw new ArgumentException("Input must be of type Vector3f", "obj");
    }

    /// <summary>
    /// Computes a hash number that represents the current vector.
    /// </summary>
    /// <returns>A hash code that is not unique for each vector.</returns>
    public override int GetHashCode()
    {
      // MSDN docs recommend XOR'ing the internal values to get a hash code
      return m_x.GetHashCode() ^ m_y.GetHashCode() ^ m_z.GetHashCode();
    }

    /// <summary>
    /// Constructs the string representation of the current vector.
    /// </summary>
    /// <returns>The vector representation in the form X,Y,Z.</returns>
    public override string ToString()
    {
      var culture = System.Globalization.CultureInfo.InvariantCulture;
      return String.Format("{0},{1},{2}",
        m_x.ToString(culture), m_y.ToString(culture), m_z.ToString(culture));
    }

    /// <summary>
    /// Determines whether two vectors have equal values.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>true if the components of the two vectors are exactly equal; otherwise false.</returns>
    public static bool operator ==(Vector3f a, Vector3f b)
    {
      return (a.m_x == b.m_x && a.m_y == b.m_y && a.m_z == b.m_z);
    }

    /// <summary>
    /// Determines whether two vectors have different values.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>true if the two vectors differ in any component; false otherwise.</returns>
    public static bool operator !=(Vector3f a, Vector3f b)
    {
      return (a.m_x != b.m_x || a.m_y != b.m_y || a.m_z != b.m_z);
    }

    /// <summary>
    /// Determines whether the first specified vector comes before
    /// (has inferior sorting value than) the second vector.
    /// <para>Components evaluation priority is first X, then Y, then Z.</para>
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>true if a.X is smaller than b.X,
    /// or a.X == b.X and a.Y is smaller than b.Y,
    /// or a.X == b.X and a.Y == b.Y and a.Z is smaller than b.Z;
    /// otherwise, false.</returns>
    public static bool operator <(Vector3f a, Vector3f b)
    {
      return a.CompareTo(b) < 0;
    }

    /// <summary>
    /// Determines whether the first specified vector comes before
    /// (has inferior sorting value than) the second vector, or it is equal to it.
    /// <para>Components evaluation priority is first X, then Y, then Z.</para>
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>true if a.X is smaller than b.X,
    /// or a.X == b.X and a.Y is smaller than b.Y,
    /// or a.X == b.X and a.Y == b.Y and a.Z &lt;= b.Z;
    /// otherwise, false.</returns>
    public static bool operator <=(Vector3f a, Vector3f b)
    {
      return a.CompareTo(b) <= 0;
    }

    /// <summary>
    /// Determines whether the first specified vector comes after (has superior sorting value than)
    /// the second vector.
    /// <para>Components evaluation priority is first X, then Y, then Z.</para>
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>true if a.X is larger than b.X,
    /// or a.X == b.X and a.Y is larger than b.Y,
    /// or a.X == b.X and a.Y == b.Y and a.Z is larger than b.Z;
    /// otherwise, false.</returns>
    public static bool operator >(Vector3f a, Vector3f b)
    {
      return a.CompareTo(b) > 0;
    }

    /// <summary>
    /// Determines whether the first specified vector comes after (has superior sorting value than)
    /// the second vector, or it is equal to it.
    /// <para>Components evaluation priority is first X, then Y, then Z.</para>
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>true if a.X is larger than b.X,
    /// or a.X == b.X and a.Y is larger than b.Y,
    /// or a.X == b.X and a.Y == b.Y and a.Z &gt;= b.Z;
    /// otherwise, false.</returns>
    public static bool operator >=(Vector3f a, Vector3f b)
    {
      return a.CompareTo(b) >= 0;
    }
  }
}