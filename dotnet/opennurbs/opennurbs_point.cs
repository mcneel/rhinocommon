using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Rhino.Geometry
{
  [StructLayout(LayoutKind.Sequential, Pack = 8, Size = 16)]
  [DebuggerDisplay("({m_t0}, {m_t1})")]
  [Serializable]
  public struct Interval
  {
    #region Members
    private double m_t0;
    private double m_t1;
    #endregion

    #region Constructors
    public Interval(double t0, double t1)
    {
      m_t0 = t0;
      m_t1 = t1;
    }
    public Interval(Interval other)
    {
      m_t0 = other.m_t0;
      m_t1 = other.m_t1;
    }
    #endregion

    #region Operators
    public static bool operator ==(Interval a, Interval b)
    {
      return a.CompareTo(b) == 0 ? true : false;
    }
    public static bool operator !=(Interval a, Interval b)
    {
      return a.CompareTo(b) == 0 ? false : true;
    }

    public override int GetHashCode()
    {
      // MSDN docs recommend XOR'ing the internal values to get a hash code
      return m_t0.GetHashCode() ^ m_t1.GetHashCode();
    }
    public override bool Equals(object obj)
    {
      return (obj is Interval && this == (Interval)obj);
    }

    ///<returns>
    /// 0  this is identical to other
    ///-1  this[0] &lt; other[0]
    ///+1  this[0] &gt; other[0]
    ///-1  this[0] == other[0] and this[1] &lt; other[1]
    ///+1  this[0] == other[0] and this[1] &gt; other[1]
    ///</returns>
    public int CompareTo(Interval other)
    {
      if (m_t0 < other.m_t0)
        return -1;
      if (m_t0 > other.m_t0)
        return 1;
      if (m_t1 < other.m_t1)
        return -1;
      if (m_t1 > other.m_t1)
        return 1;
      return 0;
    }
    #endregion

    #region Constants
    // David thinks: This is not really "empty" is it? Empty would be {0,0}.
    /////<summary>Sets interval to (RhinoMath.UnsetValue, RhinoMath.UnsetValue)</summary>
    //public static Interval Empty
    //{
    //  get { return new Interval(RhinoMath.UnsetValue, RhinoMath.UnsetValue); }
    //}

    /// <summary>
    /// Gets an Interval whose limits are RhinoMath.UnsetValue.
    /// </summary>
    public static Interval Unset
    {
      get
      {
        return new Interval(RhinoMath.UnsetValue, RhinoMath.UnsetValue);
      }
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the lower bound of the Interval.
    /// </summary>
    public double T0 { get { return m_t0; } set { m_t0 = value; } }

    /// <summary>
    /// Gets or sets the upper bound of the Interval.
    /// </summary>
    public double T1 { get { return m_t1; } set { m_t1 = value; } }

    /// <summary>
    /// Gets or sets the indexed bound of this Interval.
    /// </summary>
    /// <param name="index">Bound index (0 = lower; 1 = upper)</param>
    public double this[int index]
    {
      get
      {
        if (0 == index) { return m_t0; }
        if (1 == index) { return m_t1; }

        // IronPython works with indexing is we thrown an IndexOutOfRangeException
        throw new IndexOutOfRangeException();
      }
      set
      {
        if (0 == index) { m_t0 = value; }
        else if (1 == index) { m_t1 = value; }
        else { throw new IndexOutOfRangeException(); }
      }
    }

    /// <summary>
    /// Gets the smaller of T0 and T1.
    /// </summary>
    public double Min
    {
      get { return ((RhinoMath.IsValidDouble(m_t0) && RhinoMath.IsValidDouble(m_t1)) ? (m_t0 <= m_t1 ? m_t0 : m_t1) : RhinoMath.UnsetValue); }
    }

    /// <summary>
    /// Gets the larger of T0 and T1.
    /// </summary>
    public double Max
    {
      get { return ((RhinoMath.IsValidDouble(m_t0) && RhinoMath.IsValidDouble(m_t1)) ? (m_t0 <= m_t1 ? m_t1 : m_t0) : RhinoMath.UnsetValue); }
    }

    /// <summary>
    /// Gets the average of T0 and T1.
    /// </summary>
    public double Mid
    {
      get { return ((RhinoMath.IsValidDouble(m_t0) && RhinoMath.IsValidDouble(m_t1)) ? ((m_t0 == m_t1) ? m_t0 : (0.5 * (m_t0 + m_t1))) : RhinoMath.UnsetValue); }
    }

    /// <summary>
    /// Gets the (signed) length of the numeric range. 
    /// If the interval is decreasing, a negative length will be returned.
    /// </summary>
    public double Length
    {
      get { return ((RhinoMath.IsValidDouble(m_t0) && RhinoMath.IsValidDouble(m_t1)) ? m_t1 - m_t0 : 0.0); }
    }

    /// <summary>
    /// Gets a value indicating whether or not this Interval is valid. 
    /// Valid intervals must contain valid numbers.
    /// </summary>
    public bool IsValid
    {
      get { return RhinoMath.IsValidDouble(m_t0) && RhinoMath.IsValidDouble(m_t1); }
    }

    // If we decide that Interval.Empty should indeed be replaced with Interval.Unset, this function becomes pointless
    /////<summary>Returns true if T[0] == T[1] == ON.UnsetValue</summary>
    //public bool IsEmpty
    //{
    //  get { return (RhinoMath.UnsetValue == m_t0 && RhinoMath.UnsetValue == m_t1); }
    //}

    /// <summary>
    /// Returns true if T0 == T1 != ON.UnsetValue.
    /// </summary>
    public bool IsSingleton
    {
      get { return (RhinoMath.IsValidDouble(m_t0) && m_t0 == m_t1); }
    }

    /// <summary>
    /// Returns true if T0 &lt; T1
    /// </summary>
    public bool IsIncreasing
    {
      get { return (RhinoMath.IsValidDouble(m_t0) && m_t0 < m_t1); }
    }

    /// <summary> 
    /// Returns true if T[0] &gt; T[1]
    /// </summary>
    public bool IsDecreasing
    {
      get { return (RhinoMath.IsValidDouble(m_t1) && m_t1 < m_t0); }
    }
    #endregion

    #region Methods
    public override string ToString()
    {
      return String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1}", m_t0, m_t1);
    }

    /// <summary>
    /// Grow the Interval to include the given number.
    /// </summary>
    /// <param name="value">Number to include in this interval.</param>
    public void Grow(double value)
    {
      if (!RhinoMath.IsValidDouble(value)) { return; }

      if (IsDecreasing) { Swap(); }
      if (m_t0 > value) { m_t0 = value; }
      if (m_t1 < value) { m_t1 = value; }
    }

    /// <summary>
    /// Ensures this interval is either singleton or increasing.
    /// </summary>
    public void MakeIncreasing()
    {
      if (IsDecreasing) { Swap(); }
    }

    /// <summary>
    /// Changes interval to [-T1, -T0].
    /// </summary>
    public void Reverse()
    {
      if (IsValid)
      {
        double temp = m_t0;
        m_t0 = -m_t1;
        m_t1 = -temp;
      }
    }

    /// <summary>
    /// Swaps T0 and T1.
    /// </summary>
    public void Swap()
    {
      double temp = m_t0;
      m_t0 = m_t1;
      m_t1 = temp;
    }

    #region Evaluation
    ///<summary>Convert normalized parameter to interval value, or pair of values.</summary>
    ///<returns>Interval parameter min*(1.0-normalizedParameter) + max*normalizedParameter</returns>
    ///<seealso>NormalizedParameterAt</seealso>
    public double ParameterAt(double normalizedParameter)
    {
      return (RhinoMath.IsValidDouble(normalizedParameter) ? ((1.0 - normalizedParameter) * m_t0 + normalizedParameter * m_t1) : RhinoMath.UnsetValue);
    }

    ///<summary>Convert normalized parameter to interval value, or pair of values.</summary>
    ///<returns>Interval parameter min*(1.0-normalizedParameter) + max*normalized_paramete</returns>
    ///<seealso>NormalizedParameterAt</seealso>
    public Interval ParameterIntervalAt(Interval normalizedInterval)
    {
      double t0 = ParameterAt(normalizedInterval.m_t0);
      double t1 = ParameterAt(normalizedInterval.m_t1);
      return new Interval(t0, t1);
    }

    ///<summary>Convert interval value, or pair of values, to normalized parameter.</summary>
    ///<returns>Normalized parameter x so that min*(1.0-x) + max*x = intervalParameter.</returns>
    ///<seealso>ParameterAt</seealso>
    public double NormalizedParameterAt(double intervalParameter)
    {
      double x;
      if (RhinoMath.IsValidDouble(intervalParameter))
      {
        if (m_t0 != m_t1)
        {
          x = (intervalParameter == m_t1) ? 1.0 : (intervalParameter - m_t0) / (m_t1 - m_t0);
        }
        else
          x = m_t0;
      }
      else
      {
        x = RhinoMath.UnsetValue;
      }
      return x;
    }

    ///<summary>Convert interval value, or pair of values, to normalized parameter.</summary>
    ///<returns>Normalized parameter x so that min*(1.0-x) + max*x = intervalParameter.</returns>
    ///<seealso>ParameterAt</seealso>
    public Interval NormalizedIntervalAt(Interval intervalParameter)
    {
      double t0 = NormalizedParameterAt(intervalParameter.m_t0);
      double t1 = NormalizedParameterAt(intervalParameter.m_t1);
      return new Interval(t0, t1);
    }

    /// <summary>
    /// Test a parameter for Interval inclusion.
    /// </summary>
    /// <param name="t">Parameter to test.</param>
    /// <returns>True if t is contained within or is coincident with the limits of this Interval.</returns>
    public bool IncludesParameter(double t)
    {
      return IncludesParameter(t, false);
    }
    /// <summary>
    /// Test a parameter for Interval inclusion.
    /// </summary>
    /// <param name="t">Parameter to test.</param>
    /// <param name="strict">If true, the parameter must be fully on the inside of the Interval.</param>
    /// <returns>True if t is contained within the limits of this Interval.</returns>
    public bool IncludesParameter(double t, bool strict)
    {
      if (!RhinoMath.IsValidDouble(t)) { return false; }
      if (strict)
      {
        if ((m_t0 <= m_t1) && (m_t0 < t) && (t < m_t1)) { return true; }
        if ((m_t1 <= m_t0) && (m_t1 < t) && (t < m_t0)) { return true; }
      }
      else
      {
        if ((m_t0 <= m_t1) && (m_t0 <= t) && (t <= m_t1)) { return true; }
        if ((m_t1 <= m_t0) && (m_t1 <= t) && (t <= m_t0)) { return true; }
      }

      return false;
    }

    /// <summary>
    /// Test another interval for Interval inclusion.
    /// </summary>
    /// <param name="interval">Interval to test</param>
    /// <returns>True if the other interval is contained within or is coincident with the limits of this Interval.</returns>
    public bool IncludesInterval(Interval interval)
    {
      return IncludesInterval(interval, false);
    }
    /// <summary>
    /// Test another interval for Interval inclusion.
    /// </summary>
    /// <param name="interval">Interval to test.</param>
    /// <param name="strict">If true, the other interval must be fully on the inside of the Interval.</param>
    /// <returns>True if the other interval is contained within the limits of this Interval.</returns>
    public bool IncludesInterval(Interval interval, bool strict)
    {
      return (IncludesParameter(interval.m_t0, strict) && IncludesParameter(interval.m_t1, strict));
    }

    #endregion
    #endregion

    #region Static methods
    ///<summary>
    ///If the intersection is not empty, then 
    ///intersection = [max(a.Min(),b.Min()), min(a.Max(),b.Max())]
    ///
    ///The interval [ON.UnsetValue,ON.UnsetValue] is considered to be
    ///the empty set interval.  The result of any intersection involving an
    ///empty set interval or disjoint intervals is the empty set interval.
    ///</summary>
    public static Interval FromIntersection(Interval a, Interval b)
    {
      Interval rc = new Interval();
      UnsafeNativeMethods.ON_Interval_Intersection(ref rc, a, b);
      return rc;
    }

    ///<summary>
    ///The union of an empty set and an increasing interval is the increasing
    ///interval.  The union of two empty sets is empty. The union of an empty
    ///set an a non-empty interval is the non-empty interval.
    ///The union of two non-empty intervals is
    ///union = [min(a.Min(),b.Min()), max(a.Max(),b.Max()),]
    ///Union() returns true if the union is not empty.
    ///</summary>
    public static Interval FromUnion(Interval a, Interval b)
    {
      Interval rc = new Interval();
      UnsafeNativeMethods.ON_Interval_Union(ref rc, a, b);
      return rc;
    }
    #endregion
  }

  [StructLayout(LayoutKind.Sequential, Pack = 8, Size = 16)]
  [DebuggerDisplay("({m_x}, {m_y})")]
  [Serializable]
  public struct Point2d
  {
    private double m_x;
    private double m_y;

    public double X { get { return m_x; } set { m_x = value; } }
    public double Y { get { return m_y; } set { m_y = value; } }

    #region constructors
    public Point2d(double x, double y)
    {
      m_x = x;
      m_y = y;
    }

    public Point2d(Vector2d vector)
    {
      m_x = vector.X;
      m_y = vector.Y;
    }

    public Point2d(Point2d point)
    {
      m_x = point.m_x;
      m_y = point.m_y;
    }

    public Point2d(Point3d point)
    {
      m_x = point.m_x;
      m_y = point.m_y;
    }
    #endregion

    #region operators
    //static Point2d^ operator *=(Point2d^ point, double t);
    //static Point2d^ operator /=(Point2d^ point, double t);
    //static Point2d^ operator +=(Point2d^ point, Point2d^ other);
    //static Point2d^ operator +=(Point2d^ point, Vector2d^ vector);
    //static Point2d^ operator -=(Point2d^ point, Point2d^ other);
    //static Point2d^ operator -=(Point2d^ point, Vector2d^ vector);

    public static Point2d operator *(Point2d point, double t)
    {
      return new Point2d(point.X * t, point.Y * t);
    }
    public static Point2d Multiply(Point2d point, double t)
    {
      return new Point2d(point.X * t, point.Y * t);
    }

    public static Point2d operator *(double t, Point2d point)
    {
      return new Point2d(point.X * t, point.Y * t);
    }
    public static Point2d Multiply(double t, Point2d point)
    {
      return new Point2d(point.X * t, point.Y * t);
    }

    public static Point2d operator /(Point2d point, double t)
    {
      return new Point2d(point.X / t, point.Y / t);
    }
    public static Point2d Divide(Point2d point, double t)
    {
      return new Point2d(point.X / t, point.Y / t);
    }

    public static Point2d operator +(Point2d point, Vector2d vector)
    {
      return new Point2d(point.X + vector.X, point.Y + vector.Y);
    }
    public static Point2d Add(Point2d point, Vector2d vector)
    {
      return new Point2d(point.X + vector.X, point.Y + vector.Y);
    }

    public static Point2d operator +(Vector2d vector, Point2d point)
    {
      return new Point2d(point.X + vector.X, point.Y + vector.Y);
    }
    public static Point2d Add(Vector2d vector, Point2d point)
    {
      return new Point2d(point.X + vector.X, point.Y + vector.Y);
    }

    public static Point2d operator +(Point2d point1, Point2d point2)
    {
      return new Point2d(point1.X + point2.X, point1.Y + point2.Y);
    }
    public static Point2d Add(Point2d point1, Point2d point2)
    {
      return new Point2d(point1.X + point2.X, point1.Y + point2.Y);
    }

    public static Point2d operator -(Point2d point, Vector2d vector)
    {
      return new Point2d(point.X - vector.X, point.Y - vector.Y);
    }
    public static Point2d Subtract(Point2d point, Vector2d vector)
    {
      return new Point2d(point.X - vector.X, point.Y - vector.Y);
    }

    public static Vector2d operator -(Point2d point1, Point2d point2)
    {
      return new Vector2d(point1.X - point2.X, point1.Y - point2.Y);
    }
    public static Vector2d Subtract(Point2d point1, Point2d point2)
    {
      return new Vector2d(point1.X - point2.X, point1.Y - point2.Y);
    }

    public static bool operator ==(Point2d a, Point2d b)
    {
      return (a.m_x == b.m_x && a.m_y == b.m_y) ? true : false;
    }
    public static bool operator !=(Point2d a, Point2d b)
    {
      return (a.m_x != b.m_x || a.m_y != b.m_y) ? true : false;
    }

    #endregion

    public override bool Equals(object obj)
    {
      return (obj is Point2d && this == (Point2d)obj);
    }
    public override int GetHashCode()
    {
      // MSDN docs recommend XOR'ing the internal values to get a hash code
      return m_x.GetHashCode() ^ m_y.GetHashCode();
    }

    public override string ToString()
    {
      return String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1}", X, Y);
    }

    public double this[int index]
    {
      get
      {
        if (0 == index)
          return m_x;
        if (1 == index)
          return m_y;
        // IronPython works with indexing is we thrown an IndexOutOfRangeException
        throw new IndexOutOfRangeException();
      }
      set
      {
        if (0 == index)
          X = value;
        else if (1 == index)
          Y = value;
        else
          throw new IndexOutOfRangeException();
      }
    }

    ///<summary>
    ///If any coordinate of a point is UnsetValue, then the point is not valid.
    ///</summary>
    public bool IsValid
    {
      get { return RhinoMath.IsValidDouble(X) && RhinoMath.IsValidDouble(Y); }
    }

    public double MinimumCoordinate
    {
      get
      {
        double c;
        if (RhinoMath.IsValidDouble(X))
        {
          c = System.Math.Abs(X);
          if (RhinoMath.IsValidDouble(Y) && System.Math.Abs(Y) < c)
            c = System.Math.Abs(Y);
        }
        else if (RhinoMath.IsValidDouble(Y))
        {
          c = System.Math.Abs(Y);
        }
        else
          c = RhinoMath.UnsetValue;
        return c;
      }
    }

    public double MaximumCoordinate
    {
      get
      {
        double c;
        if (RhinoMath.IsValidDouble(X))
        {
          c = System.Math.Abs(X);
          if (RhinoMath.IsValidDouble(Y) && System.Math.Abs(Y) > c)
            c = System.Math.Abs(Y);
        }
        else if (RhinoMath.IsValidDouble(Y))
        {
          c = System.Math.Abs(Y);
        }
        else
          c = RhinoMath.UnsetValue;
        return c;
      }
    }

    public static Point2d Origin
    {
      get { return new Point2d(0, 0); }
    }
    public static Point2d Unset
    {
      get { return new Point2d(RhinoMath.UnsetValue, RhinoMath.UnsetValue); }
    }

    public double DistanceTo(Point2d other)
    {
      double d;
      if (IsValid && other.IsValid)
      {
        Vector2d v = other - this;
        d = v.Length;
      }
      else
      {
        d = 0.0;
      }
      return d;
    }
  }

  [StructLayout(LayoutKind.Sequential, Pack = 8, Size = 24)]
  [DebuggerDisplay("({m_x}, {m_y}, {m_z})")]
  [Serializable]
  public struct Point3d
  {
    #region members
    internal double m_x;
    internal double m_y;
    internal double m_z;
    #endregion

    #region constructors
    /// <summary>Create a point with defined x,y,z values</summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <example>
    /// <code source='examples\vbnet\ex_addcircle.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addcircle.cs' lang='cs'/>
    /// <code source='examples\py\ex_addcircle.py' lang='py'/>
    /// </example>
    public Point3d(double x, double y, double z)
    {
      m_x = x;
      m_y = y;
      m_z = z;
    }
    public Point3d(Vector3d vector)
    {
      m_x = vector.m_x;
      m_y = vector.m_y;
      m_z = vector.m_z;
    }
    public Point3d(Point3f point)
    {
      m_x = point.X;
      m_y = point.Y;
      m_z = point.Z;
    }
    public Point3d(Point3d point)
    {
      m_x = point.X;
      m_y = point.Y;
      m_z = point.Z;
    }
    public Point3d(Point4d point)
    {
      m_x = point.m_x; m_y = point.m_y; m_z = point.m_z;
      double w = (point.m_w != 1.0 && point.m_w != 0.0) ? 1.0 / point.m_w : 1.0;
      m_x *= w;
      m_y *= w;
      m_z *= w;
    }

    public static Point3d Origin
    {
      get { return new Point3d(0, 0, 0); }
    }
    public static Point3d Unset
    {
      get { return new Point3d(RhinoMath.UnsetValue, RhinoMath.UnsetValue, RhinoMath.UnsetValue); }
    }
    #endregion

    #region operators
    public static Point3d operator *(Point3d point, double t)
    {
      return new Point3d(point.m_x * t, point.m_y * t, point.m_z * t);
    }
    public static Point3d Multiply(Point3d point, double t)
    {
      return new Point3d(point.m_x * t, point.m_y * t, point.m_z * t);
    }

    public static Point3d operator *(double t, Point3d point)
    {
      return new Point3d(point.m_x * t, point.m_y * t, point.m_z * t);
    }
    public static Point3d Multiply(double t, Point3d point)
    {
      return new Point3d(point.m_x * t, point.m_y * t, point.m_z * t);
    }

    public static Point3d operator /(Point3d point, double t)
    {
      return new Point3d(point.m_x / t, point.m_y / t, point.m_z / t);
    }
    public static Point3d Divide(Point3d point, double t)
    {
      return new Point3d(point.m_x / t, point.m_y / t, point.m_z / t);
    }

    public static Point3d operator +(Point3d point, Point3d point2)
    {
      return new Point3d(point.m_x + point2.m_x, point.m_y + point2.m_y, point.m_z + point2.m_z);
    }
    public static Point3d Add(Point3d point, Point3d point2)
    {
      return new Point3d(point.m_x + point2.m_x, point.m_y + point2.m_y, point.m_z + point2.m_z);
    }

    public static Point3d operator +(Point3d point, Vector3d vector)
    {
      return new Point3d(point.m_x + vector.m_x, point.m_y + vector.m_y, point.m_z + vector.m_z);
    }
    public static Point3d Add(Point3d point, Vector3d vector)
    {
      return new Point3d(point.m_x + vector.m_x, point.m_y + vector.m_y, point.m_z + vector.m_z);
    }
    public static Point3d operator +(Point3d point, Vector3f vector)
    {
      return new Point3d(point.m_x + vector.m_x, point.m_y + vector.m_y, point.m_z + vector.m_z);
    }
    public static Point3d Add(Point3d point, Vector3f vector)
    {
      return new Point3d(point.m_x + vector.m_x, point.m_y + vector.m_y, point.m_z + vector.m_z);
    }

    public static Point3d operator +(Vector3d vector, Point3d point)
    {
      return new Point3d(point.m_x + vector.m_x, point.m_y + vector.m_y, point.m_z + vector.m_z);
    }
    public static Point3d Add(Vector3d vector, Point3d point)
    {
      return new Point3d(point.m_x + vector.m_x, point.m_y + vector.m_y, point.m_z + vector.m_z);
    }

    public static Point3d operator -(Point3d point, Vector3d vector)
    {
      return new Point3d(point.m_x - vector.m_x, point.m_y - vector.m_y, point.m_z - vector.m_z);
    }
    public static Point3d Subtract(Point3d point, Vector3d vector)
    {
      return new Point3d(point.m_x - vector.m_x, point.m_y - vector.m_y, point.m_z - vector.m_z);
    }

    public static Vector3d operator -(Point3d point, Point3d point2)
    {
      return new Vector3d(point.m_x - point2.m_x, point.m_y - point2.m_y, point.m_z - point2.m_z);
    }
    public static Vector3d Subtract(Point3d point, Point3d point2)
    {
      return new Vector3d(point.m_x - point2.m_x, point.m_y - point2.m_y, point.m_z - point2.m_z);
    }

    public static bool operator ==(Point3d a, Point3d b)
    {
      return (a.m_x == b.m_x && a.m_y == b.m_y && a.m_z == b.m_z);
    }
    public static bool operator !=(Point3d a, Point3d b)
    {
      return (a.m_x != b.m_x || a.m_y != b.m_y || a.m_z != b.m_z);
    }

    public static implicit operator ControlPoint(Point3d pt)
    {
      return new ControlPoint(pt);
    }
    //David: made this operator explicit on jan-22 2011, it was causing problems with the VB compiler.
    public static explicit operator Vector3d(Point3d pt)
    {
      return new Vector3d(pt);
    }
    //David: made this operator explicit on jan-22 2011, it was causing problems with the VB compiler.
    public static explicit operator Point3d(Vector3d vec)
    {
      return new Point3d(vec);
    }
    public static implicit operator Point3d(Point3f pt)
    {
      return new Point3d(pt);
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets or sets the X coordinate of this point.
    /// </summary>
    public double X { get { return m_x; } set { m_x = value; } }
    /// <summary>
    /// Gets or sets the Y coordinate of this point.
    /// </summary>
    public double Y { get { return m_y; } set { m_y = value; } }
    /// <summary>
    /// Gets or sets the Z coordinate of this point.
    /// </summary>
    public double Z { get { return m_z; } set { m_z = value; } }
    /// <summary>
    /// Gets or sets an indexed coordinate of this point.
    /// </summary>
    /// <param name="index">
    /// The coordinate index. Valid values are:
    /// <para>0 = X coordinate</para>
    /// <para>1 = Y coordinate</para>
    /// <para>2 = Z coordinate</para>
    /// </param>
    public double this[int index]
    {
      get
      {
        if (0 == index)
          return m_x;
        if (1 == index)
          return m_y;
        if (2 == index)
          return m_z;
        // IronPython works with indexing is we thrown an IndexOutOfRangeException
        throw new IndexOutOfRangeException();
      }
      set
      {
        if (0 == index)
          m_x = value;
        else if (1 == index)
          m_y = value;
        else if (2 == index)
          m_z = value;
        else
          throw new IndexOutOfRangeException();
      }
    }

    ///<summary>
    ///If any coordinate of a point is UnsetValue, then the point is not valid.
    ///</summary>
    public bool IsValid
    {
      get { return RhinoMath.IsValidDouble(m_x) && RhinoMath.IsValidDouble(m_y) && RhinoMath.IsValidDouble(m_z); }
    }

    /// <summary>
    /// Gets the smallest (both positive and negative) coordinate value in this point.
    /// </summary>
    public double MinimumCoordinate
    {
      get
      {
        double c;
        if (RhinoMath.IsValidDouble(m_x))
        {
          c = System.Math.Abs(m_x);
          if (RhinoMath.IsValidDouble(m_y) && System.Math.Abs(m_y) < c)
            c = System.Math.Abs(m_y);
          if (RhinoMath.IsValidDouble(m_z) && System.Math.Abs(m_z) < c)
            c = System.Math.Abs(m_z);
        }
        else if (RhinoMath.IsValidDouble(m_y))
        {
          c = System.Math.Abs(m_y);
          if (RhinoMath.IsValidDouble(m_z) && System.Math.Abs(m_z) < c)
            c = System.Math.Abs(m_z);
        }
        else if (RhinoMath.IsValidDouble(m_z))
        {
          c = System.Math.Abs(m_z);
        }
        else
          c = RhinoMath.UnsetValue;
        return c;
      }
    }

    /// <summary>
    /// Gets the largest (both positive and negative) coordinate value in this point.
    /// </summary>
    public double MaximumCoordinate
    {
      get
      {
        double c;
        if (RhinoMath.IsValidDouble(m_x))
        {
          c = System.Math.Abs(m_x);
          if (RhinoMath.IsValidDouble(m_y) && System.Math.Abs(m_y) > c)
            c = System.Math.Abs(m_y);
          if (RhinoMath.IsValidDouble(m_z) && System.Math.Abs(m_z) > c)
            c = System.Math.Abs(m_z);
        }
        else if (RhinoMath.IsValidDouble(m_y))
        {
          c = System.Math.Abs(m_y);
          if (RhinoMath.IsValidDouble(m_z) && System.Math.Abs(m_z) > c)
            c = System.Math.Abs(m_z);
        }
        else if (RhinoMath.IsValidDouble(m_z))
        {
          c = System.Math.Abs(m_z);
        }
        else
          c = RhinoMath.UnsetValue;
        return c;
      }
    }


    #endregion

    #region methods
    public override bool Equals(object obj)
    {
      return (obj is Point3d && this == (Point3d)obj);
    }
    public override int GetHashCode()
    {
      // MSDN docs recommend XOR'ing the internal values to get a hash code
      return m_x.GetHashCode() ^ m_y.GetHashCode() ^ m_z.GetHashCode();
    }

    /// <summary>
    /// Interpolate between two points.
    /// </summary>
    /// <param name="pA">First point.</param>
    /// <param name="pB">Second point.</param>
    /// <param name="t">Interpolation parameter. 
    /// If t=0 then this point is set to pA. 
    /// If t=1 then this point is set to pB. 
    /// Values of t in between 0.0 and 1.0 result in points between pA and pB.</param>
    public void Interpolate(Point3d pA, Point3d pB, double t)
    {
      m_x = pA.m_x + t * (pB.m_x - pA.m_x);
      m_y = pA.m_y + t * (pB.m_y - pA.m_y);
      m_z = pA.m_z + t * (pB.m_z - pA.m_z);
    }

    public override string ToString()
    {
      return String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2}", m_x, m_y, m_z);
    }

    /// <summary>
    /// Compute the distance between two points.
    /// </summary>
    /// <param name="other">Other point for distance measurement.</param>
    /// <returns>The distance between this point and other.</returns>
    public double DistanceTo(Point3d other)
    {
      double d;
      if (IsValid && other.IsValid)
      {
        double dx = other.m_x - m_x;
        double dy = other.m_y - m_y;
        double dz = other.m_z - m_z;
        d = Vector3d.GetLengthHelper(dx, dy, dz);
      }
      else
      {
        d = 0.0;
      }
      return d;
    }

    /// <summary>
    /// Transform the point. The transformation matrix acts on the left of the point
    /// i.e., result = transformation*point
    /// </summary>
    /// <param name="xform">Transformation to apply.</param>
    public void Transform(Transform xform)
    {
      //David: this method doesn't test for validity. Should it?
      double ww = xform.m_30 * m_x + xform.m_31 * m_y + xform.m_32 * m_z + xform.m_33;
      if (ww != 0.0) { ww = 1.0 / ww; }

      double tx = ww * (xform.m_00 * m_x + xform.m_01 * m_y + xform.m_02 * m_z + xform.m_03);
      double ty = ww * (xform.m_10 * m_x + xform.m_11 * m_y + xform.m_12 * m_z + xform.m_13);
      double tz = ww * (xform.m_20 * m_x + xform.m_21 * m_y + xform.m_22 * m_z + xform.m_23);
      m_x = tx;
      m_y = ty;
      m_z = tz;
    }
    #endregion

    /// <summary>
    /// Test if a set of points are coplanar within a certain tolerance
    /// </summary>
    /// <param name="points"></param>
    /// <param name="tolerance">A good default is RhinoMath.ZeroTolerance</param>
    /// <returns></returns>
    public static bool ArePointsCoplanar(System.Collections.Generic.IEnumerable<Point3d> points, double tolerance)
    {
      int count;
      Point3d[] arrPoints = Rhino.Collections.Point3dList.GetConstPointArray(points, out count);
      if (count < 1 || null == arrPoints)
        throw new ArgumentException("points must contain at least 1 point");
      return UnsafeNativeMethods.RHC_RhinoArePointsCoplanar(count, arrPoints, tolerance);
    }

    /// <summary>
    /// Finds duplicates in the supplied list of points and returns a
    /// new array of points without duplicates.
    /// </summary>
    /// <param name="points"></param>
    /// <param name="tolerance">
    /// The minimum distance between points. Points that fall within this
    /// tolerance will be discarded.</param>
    /// <returns>Array of points with duplicates removed, or null on error</returns>
    public static Point3d[] CullDuplicates(System.Collections.Generic.IEnumerable<Point3d> points, double tolerance)
    {
      if (null == points)
        return null;

      // This code duplicates the static function CullDuplicatePoints in tl_brep_intersect.cpp
      Rhino.Collections.Point3dList point_list = new Rhino.Collections.Point3dList(points);
      int count = point_list.Count;
      if (0 == count)
        return null;

      bool[] dup_list = new bool[count];
      Rhino.Collections.Point3dList non_dups = new Rhino.Collections.Point3dList(count);

      for (int i = 0; i < count; i++)
      {
        // Check if the entry has been flagged as a duplicate
        if (dup_list[i] == false)
        {
          non_dups.Add(point_list[i]);
          // Only compare with entries that haven't been checked
          for (int j = i + 1; j < count; j++)
          {
            if (point_list[i].DistanceTo(point_list[j]) <= tolerance)
              dup_list[j] = true;
          }
        }
      }

      return non_dups.ToArray();
    }

    /// <summary>
    /// Sort a list of points so they will be connected in a "reasonable polyline" order. Also remove
    /// points from the list that are closer together than a minimum distance
    /// </summary>
    /// <param name="points">points to sort</param>
    /// <param name="minimumDistance">minimum distance to use. Throw out ones closer than this</param>
    /// <returns>new list of sorted points</returns>
    public static Point3d[] SortAndCullPointList(System.Collections.Generic.IEnumerable<Point3d> points, double minimumDistance)
    {
      int count;
      Point3d[] arrPoints = Rhino.Collections.Point3dList.GetConstPointArray(points, out count);
      if (count < 1 || null == arrPoints)
        return null;
      bool rc = UnsafeNativeMethods.TLC_SortPointList(arrPoints, ref count, minimumDistance);
      if (false == rc)
        return null;
      if (count < arrPoints.Length)
      {
        Point3d[] destPoints = new Point3d[count];
        System.Array.Copy(arrPoints, destPoints, count);
        arrPoints = destPoints;
      }
      return arrPoints;
    }
  }

  [StructLayout(LayoutKind.Sequential, Pack = 8, Size = 32)]
  [DebuggerDisplay("({m_x}, {m_y}, {m_z}, [{m_w}])")]
  [Serializable]
  public struct Point4d
  {
    internal double m_x;
    internal double m_y;
    internal double m_z;
    internal double m_w;

    public Point4d(double x, double y, double z, double w)
    {
      m_x = x;
      m_y = y;
      m_z = z;
      m_w = w;
    }

    public Point4d(Point3d point)
    {
      m_x = point.m_x;
      m_y = point.m_y;
      m_z = point.m_z;
      m_w = 1.0;
    }

    public double X { get { return m_x; } set { m_x = value; } }
    public double Y { get { return m_y; } set { m_y = value; } }
    public double Z { get { return m_z; } set { m_z = value; } }
    public double W { get { return m_w; } set { m_w = value; } }


    #region operators
    public static Point4d operator +(Point4d point, Point4d point2)
    {
      Point4d rc = new Point4d(point.m_x, point.m_y, point.m_z, point.m_w);
      if (point2.m_w == point.m_w)
      {
        rc.m_x += point2.m_x;
        rc.m_y += point2.m_y;
        rc.m_z += point2.m_z;
      }
      else if (point2.m_w == 0)
      {
        rc.m_x += point2.m_x;
        rc.m_y += point2.m_y;
        rc.m_z += point2.m_z;
      }
      else if (point.m_w == 0)
      {
        rc.m_x += point2.m_x;
        rc.m_y += point2.m_y;
        rc.m_z += point2.m_z;
        rc.m_w = point2.m_w;
      }
      else
      {
        double sw1 = (point.m_w > 0.0) ? Math.Sqrt(point.m_w) : -Math.Sqrt(-point.m_w);
        double sw2 = (point2.m_w > 0.0) ? Math.Sqrt(point2.m_w) : -Math.Sqrt(-point2.m_w);
        double s1 = sw2 / sw1;
        double s2 = sw1 / sw2;
        rc.m_x = point.m_x * s1 + point2.m_x * s2;
        rc.m_y = point.m_y * s1 + point2.m_y * s2;
        rc.m_z = point.m_z * s1 + point2.m_z * s2;
        rc.m_w = sw1 * sw2;
      }
      return rc;
    }
    public static Point4d Add(Point4d point, Point4d point2)
    {
      return point + point2;
    }

    public static Point4d operator -(Point4d point, Point4d point2)
    {
      Point4d rc = new Point4d(point.m_x, point.m_y, point.m_z, point.m_w);
      if (point2.m_w == point.m_w)
      {
        rc.m_x -= point2.m_x;
        rc.m_y -= point2.m_y;
        rc.m_z -= point2.m_z;
      }
      else if (point2.m_w == 0.0)
      {
        rc.m_x -= point2.m_x;
        rc.m_y -= point2.m_y;
        rc.m_z -= point2.m_z;
      }
      else if (point.m_w == 0.0)
      {
        rc.m_x -= point2.m_x;
        rc.m_y -= point2.m_y;
        rc.m_z -= point2.m_z;
        rc.m_w = point2.m_w;
      }
      else
      {
        double sw1 = (point.m_w > 0.0) ? Math.Sqrt(point.m_w) : -Math.Sqrt(-point.m_w);
        double sw2 = (point2.m_w > 0.0) ? Math.Sqrt(point2.m_w) : -Math.Sqrt(-point2.m_w);
        double s1 = sw2 / sw1;
        double s2 = sw1 / sw2;
        rc.m_x = point.m_x * s1 - point2.m_x * s2;
        rc.m_y = point.m_y * s1 - point2.m_y * s2;
        rc.m_z = point.m_z * s1 - point2.m_z * s2;
        rc.m_w = sw1 * sw2;
      }
      return rc;
    }

    public static Point4d Subtract(Point4d point, Point4d point2)
    {
      return point - point2;
    }

    public static Point4d operator *(Point4d point, double d)
    {
      return new Point4d(point.m_x * d, point.m_y * d, point.m_z * d, point.m_w * d);
    }
    public static Point4d Multiply(Point4d point, double d)
    {
      return point * d;
    }

    public static double operator *(Point4d point, Point4d point2)
    {
      return (point.m_x * point2.m_x) + 
        (point.m_y * point2.m_y) +
        (point.m_z * point2.m_z) +
        (point.m_w * point2.m_w);
    }

    public static bool operator ==(Point4d a, Point4d b)
    {
      return UnsafeNativeMethods.ON_4dPoint_Equality(a, b);
    }
    public static bool operator !=(Point4d a, Point4d b)
    {
      return !(a == b);
    }
    #endregion

    public override bool Equals(object obj)
    {
      return (obj is Point4d && this == (Point4d)obj);
    }

    public override int GetHashCode()
    {
      // operator == uses normalized values to compare. This should
      // also be done for GetHashCode so we get similar results
      Point4d x = this;
      UnsafeNativeMethods.ON_4dPoint_Normalize(ref x);
      // MSDN docs recommend XOR'ing the internal values to get a hash code
      return x.m_x.GetHashCode() ^ x.m_y.GetHashCode() ^ x.m_z.GetHashCode() ^ x.m_w.GetHashCode();
    }

    public static Point4d Unset
    {
      get { return new Point4d(RhinoMath.UnsetValue, RhinoMath.UnsetValue, RhinoMath.UnsetValue, RhinoMath.UnsetValue); }
    }

  }

  [StructLayout(LayoutKind.Sequential, Pack = 8, Size = 16)]
  [DebuggerDisplay("({m_x}, {m_y})")]
  [Serializable]
  public struct Vector2d
  {
    private double m_x;
    private double m_y;

    public Vector2d(double x, double y)
    {
      m_x = x;
      m_y = y;
    }

    public double X { get { return m_x; } set { m_x = value; } }
    public double Y { get { return m_y; } set { m_y = value; } }

    public double Length
    {
      get { return UnsafeNativeMethods.ON_2dVector_Length(this); }
    }

    #region operators
    public static bool operator ==(Vector2d a, Vector2d b)
    {
      return (a.m_x == b.m_x && a.m_y == b.m_y) ? true : false;
    }
    public static bool operator !=(Vector2d a, Vector2d b)
    {
      return (a.m_x != b.m_x || a.m_y != b.m_y) ? true : false;
    }

    #endregion

    public override bool Equals(object obj)
    {
      return (obj is Vector2d && this == (Vector2d)obj);
    }
    public override int GetHashCode()
    {
      // MSDN docs recommend XOR'ing the internal values to get a hash code
      return m_x.GetHashCode() ^ m_y.GetHashCode();
    }

    public override string ToString()
    {
      return String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1}", X, Y);
    }

    public static Vector2d Unset
    {
      get { return new Vector2d(RhinoMath.UnsetValue, RhinoMath.UnsetValue); }
    }

  }

  // holding off on making this IComparable until I understand all
  // of the rules that FxCop states about IComparable classes
  [StructLayout(LayoutKind.Sequential, Pack = 8, Size = 24)]
  [DebuggerDisplay("({m_x}, {m_y}, {m_z})")]
  [Serializable]
  public struct Vector3d //: IComparable<Vector3d>
  {
    #region fields
    internal double m_x;
    internal double m_y;
    internal double m_z;
    #endregion

    #region constructors
    public Vector3d(double x, double y, double z)
    {
      m_x = x;
      m_y = y;
      m_z = z;
    }
    public Vector3d(Point3d point)
    {
      m_x = point.m_x;
      m_y = point.m_y;
      m_z = point.m_z;
    }
    public Vector3d(Vector3f vector)
    {
      m_x = vector.m_x;
      m_y = vector.m_y;
      m_z = vector.m_z;
    }
    public Vector3d(Vector3d vector)
    {
      m_x = vector.m_x;
      m_y = vector.m_y;
      m_z = vector.m_z;
    }

    public static Vector3d Zero
    {
      get { return new Vector3d(0.0, 0.0, 0.0); }
    }
    public static Vector3d XAxis
    {
      get { return new Vector3d(1.0, 0.0, 0.0); }
    }
    public static Vector3d YAxis
    {
      get { return new Vector3d(0.0, 1.0, 0.0); }
    }
    public static Vector3d ZAxis
    {
      get { return new Vector3d(0.0, 0.0, 1.0); }
    }
    public static Vector3d Unset
    {
      get { return new Vector3d(RhinoMath.UnsetValue, RhinoMath.UnsetValue, RhinoMath.UnsetValue); }
    }
    #endregion

    #region operators
    public static Vector3d operator *(Vector3d vector, double t)
    {
      return new Vector3d(vector.m_x * t, vector.m_y * t, vector.m_z * t);
    }
    /// <summary>
    /// provided for languages that do not support operator overloading
    /// </summary>
    /// <param name="vector"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static Vector3d Multiply(Vector3d vector, double t)
    {
      return new Vector3d(vector.m_x * t, vector.m_y * t, vector.m_z * t);
    }

    public static Vector3d operator *(double t, Vector3d vector)
    {
      return new Vector3d(vector.m_x * t, vector.m_y * t, vector.m_z * t);
    }
    /// <summary>
    /// provided for languages that do not support operator overloading
    /// </summary>
    /// <param name="t"></param>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Vector3d Multiply(double t, Vector3d vector)
    {
      return new Vector3d(vector.m_x * t, vector.m_y * t, vector.m_z * t);
    }

    public static Vector3d operator /(Vector3d vector, double t)
    {
      return new Vector3d(vector.m_x / t, vector.m_y / t, vector.m_z / t);
    }
    /// <summary>
    /// provided for languages that do not support operator overloading
    /// </summary>
    /// <param name="vector"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static Vector3d Divide(Vector3d vector, double t)
    {
      return new Vector3d(vector.m_x / t, vector.m_y / t, vector.m_z / t);
    }

    public static Vector3d operator +(Vector3d vector1, Vector3d vector2)
    {
      return new Vector3d(vector1.m_x + vector2.m_x, vector1.m_y + vector2.m_y, vector1.m_z + vector2.m_z);
    }
    /// <summary>
    /// provided for languages that do not support operator overloading
    /// </summary>
    /// <param name="vector1"></param>
    /// <param name="vector2"></param>
    /// <returns></returns>
    public static Vector3d Add(Vector3d vector1, Vector3d vector2)
    {
      return new Vector3d(vector1.m_x + vector2.m_x, vector1.m_y + vector2.m_y, vector1.m_z + vector2.m_z);
    }

    public static Vector3d operator -(Vector3d vector1, Vector3d vector2)
    {
      return new Vector3d(vector1.m_x - vector2.m_x, vector1.m_y - vector2.m_y, vector1.m_z - vector2.m_z);
    }
    /// <summary>
    /// provided for languages that do not support operator overloading
    /// </summary>
    /// <param name="vector1"></param>
    /// <param name="vector2"></param>
    /// <returns></returns>
    public static Vector3d Subtract(Vector3d vector1, Vector3d vector2)
    {
      return new Vector3d(vector1.m_x - vector2.m_x, vector1.m_y - vector2.m_y, vector1.m_z - vector2.m_z);
    }

    public static double operator *(Vector3d vector1, Vector3d vector2)
    {
      return (vector1.m_x * vector2.m_x + vector1.m_y * vector2.m_y + vector1.m_z * vector2.m_z);
    }
    /// <summary>
    /// provided for languages that do not support operator overloading
    /// </summary>
    /// <param name="vector1"></param>
    /// <param name="vector2"></param>
    /// <returns></returns>
    public static double Multiply(Vector3d vector1, Vector3d vector2)
    {
      return (vector1.m_x * vector2.m_x + vector1.m_y * vector2.m_y + vector1.m_z * vector2.m_z);
    }

    public static Vector3d operator -(Vector3d vector)
    {
      return new Vector3d(-vector.m_x, -vector.m_y, -vector.m_z);
    }
    /// <summary>
    /// provided for languages that do not support operator overloading
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Vector3d Negate(Vector3d vector)
    {
      return new Vector3d(-vector.m_x, -vector.m_y, -vector.m_z);
    }

    public static bool operator ==(Vector3d a, Vector3d b)
    {
      return (a.m_x == b.m_x && a.m_y == b.m_y && a.m_z == b.m_z) ? true : false;
    }
    public static bool operator !=(Vector3d a, Vector3d b)
    {
      return (a.m_x != b.m_x || a.m_y != b.m_y || a.m_z != b.m_z) ? true : false;
    }

    /// <summary>
    /// Compute the Cross Product of two vectors. 
    /// The cross product is a vector that is perpendicular to both a and b.
    /// </summary>
    /// <param name="a">First vector for cross product.</param>
    /// <param name="b">Second vector for cross product.</param>
    /// <returns>The cross product of a and b</returns>
    public static Vector3d CrossProduct(Vector3d a, Vector3d b)
    {
      return new Vector3d(a.m_y * b.m_z - b.m_y * a.m_z, a.m_z * b.m_x - b.m_z * a.m_x, a.m_x * b.m_y - b.m_x * a.m_y);
    }

    /// <summary>
    /// Compute the angle between two vectors.
    /// </summary>
    /// <param name="a">First vector for angle.</param>
    /// <param name="b">Second vector for angle.</param>
    /// <returns>The angle (in radians) between a and b or RhinoMath.UnsetValue if the input is invalid.</returns>
    public static double VectorAngle(Vector3d a, Vector3d b)
    {
      if (!a.Unitize() || !b.Unitize())
        return RhinoMath.UnsetValue;

      //compute dot product
      double dot = a.m_x * b.m_x + a.m_y * b.m_y + a.m_z * b.m_z;
      // remove any "noise"
      if (dot > 1.0) dot = 1.0;
      if (dot < -1.0) dot = -1.0;
      double radians = Math.Acos(dot);
      return radians;
    }

    public static implicit operator Vector3d(Vector3f vec)
    {
      return new Vector3d(vec);
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets or sets the X component of the vector.
    /// </summary>
    public double X { get { return m_x; } set { m_x = value; } }
    /// <summary>
    /// Gets or sets the Y component of the vector.
    /// </summary>
    public double Y { get { return m_y; } set { m_y = value; } }
    /// <summary>
    /// Gets or sets the Z component of the vector.
    /// </summary>
    public double Z { get { return m_z; } set { m_z = value; } }
    /// <summary>
    /// Gets or sets the component at the given index.
    /// </summary>
    /// <param name="index">Index of vector component. Valid values are: 
    /// <para>0 = X-component</para>
    /// <para>1 = Y-component</para>
    /// <para>2 = Z-component</para>
    /// </param>
    public double this[int index]
    {
      get
      {
        if (0 == index)
          return m_x;
        if (1 == index)
          return m_y;
        if (2 == index)
          return m_z;
        // IronPython works with indexing when we thrown an IndexOutOfRangeException
        throw new IndexOutOfRangeException();
      }
      set
      {
        if (0 == index)
          m_x = value;
        else if (1 == index)
          m_y = value;
        else if (2 == index)
          m_z = value;
        else
          throw new IndexOutOfRangeException();
      }
    }

    /// <summary>
    /// Gets a value indicating whether this vector is valid. 
    /// A valid vector must contain valid numbers for x, y and z.
    /// </summary>
    public bool IsValid
    {
      get
      {
        return RhinoMath.IsValidDouble(m_x) &&
               RhinoMath.IsValidDouble(m_y) &&
               RhinoMath.IsValidDouble(m_z);
      }
    }

    /// <summary>
    /// Gets the value of the smallest component.
    /// </summary>
    public double MinimumCoordinate
    {
      get
      {
        Point3d p = new Point3d(this);
        return p.MinimumCoordinate;
      }
    }
    /// <summary>
    /// Gets the value of the largest component.
    /// </summary>
    public double MaximumCoordinate
    {
      get
      {
        Point3d p = new Point3d(this);
        return p.MaximumCoordinate;
      }
    }

    /// <summary>
    /// Gets the length of this vector.
    /// </summary>
    public double Length
    {
      get { return GetLengthHelper(m_x, m_y, m_z); }
    }
    /// <summary>
    /// Gets the squared length of this vector.
    /// </summary>
    public double SquareLength
    {
      get { return (m_x * m_x) + (m_y * m_y) + (m_z * m_z); }
    }
    /// <summary>
    /// Gets a value indicating whether or not this is a unit vector. 
    /// A unit vector has a length of 1.0.
    /// </summary>
    public bool IsUnitVector
    {
      get
      {
        // checks for invalid values and returns 0.0 if there are any
        double length = GetLengthHelper(m_x, m_y, m_z);
        return Math.Abs(length - 1.0) <= RhinoMath.SqrtEpsilon;
      }
    }

    /// <summary>
    /// Test a vector to see if it is very short.
    /// </summary>
    /// <param name="tolerance">
    /// A nonzero value used as the coordinate zero tolerance.
    /// </param>
    /// <returns>(Math.Abs(X) &lt;= tiny_tol) AND (Math.Abs(Y) &lt;= tiny_tol) AND (Math.Abs(Z) &lt;= tiny_tol)</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addline.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addline.cs' lang='cs'/>
    /// <code source='examples\py\ex_addline.py' lang='py'/>
    /// </example>
    public bool IsTiny(double tolerance)
    {
      return UnsafeNativeMethods.ON_3dVector_IsTiny(this, tolerance);
    }
    /// <summary>
    /// Gets a value indicating whether the X, Y, and Z values are all equal to 0.0.
    /// </summary>
    public bool IsZero
    {
      get
      {
        return (m_x == 0.0 && m_y == 0.0 && m_z == 0.0);
      }
    }
    #endregion

    #region methods
    public override bool Equals(object obj)
    {
      return (obj is Vector3d && this == (Vector3d)obj);
    }
    public int CompareTo(Vector3d other)
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

      if (m_z < other.m_z)
        return -1;
      if (m_z > other.m_z)
        return 1;

      return 0;
    }

    public override int GetHashCode()
    {
      // MSDN docs recommend XOR'ing the internal values to get a hash code
      return m_x.GetHashCode() ^ m_y.GetHashCode() ^ m_z.GetHashCode();
    }
    public override string ToString()
    {
      return String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2}", m_x, m_y, m_z);
    }

    /// <summary>
    /// Unitize this vector. A unit vector has a length of 1.0. 
    /// An invalid or zero length vector cannot be unitized.
    /// </summary>
    /// <returns>True on success, false on failure.</returns>
    public bool Unitize()
    {
      bool rc = UnsafeNativeMethods.ON_3dVector_Unitize(ref this);
      return rc;
    }

    /// <summary>
    /// Transform the vector in place. The transformation matrix acts on
    /// the left of the vector; i.e., result = transformation*vector
    /// </summary>
    /// <param name="transformation">Transformation matrix to apply.</param>
    public void Transform(Transform transformation)
    {
      double xx = transformation.m_00 * m_x + transformation.m_01 * m_y + transformation.m_02 * m_z;
      double yy = transformation.m_10 * m_x + transformation.m_11 * m_y + transformation.m_12 * m_z;
      double zz = transformation.m_20 * m_x + transformation.m_21 * m_y + transformation.m_22 * m_z;

      m_x = xx;
      m_y = yy;
      m_z = zz;
    }

    /// <summary>
    /// Rotate this vector around an axis.
    /// </summary>
    /// <param name="angleRadians">Angle of rotation (in radians).</param>
    /// <param name="rotationAxis">Axis of rotation.</param>
    /// <returns>True on success, false on failure.</returns>
    public bool Rotate(double angleRadians, Vector3d rotationAxis)
    {
      if (RhinoMath.UnsetValue == angleRadians) { return false; }
      if (!rotationAxis.IsValid) { return false; }

      UnsafeNativeMethods.ON_3dVector_Rotate(ref this, angleRadians, rotationAxis);
      return true;
    }

    ///<summary>
    /// Reverse (invert) this vector. If this vector is invalid, the 
    /// reverse will also be invalid and false will be returned.
    ///</summary>
    ///<returns>True on success, false if the vector is invalid.</returns>
    public bool Reverse()
    {
      bool rc = true;

      if (RhinoMath.UnsetValue != m_x) { m_x = -m_x; } else { rc = false; }
      if (RhinoMath.UnsetValue != m_y) { m_y = -m_y; } else { rc = false; }
      if (RhinoMath.UnsetValue != m_z) { m_z = -m_z; } else { rc = false; }

      return rc;
    }

    /// <summary>
    /// Test to see whether this vector is parallel to within one degree of another one. 
    /// </summary>
    /// <param name="other">Vector to compare to.</param>
    /// <returns>
    /// Parallel indicator:
    /// <para>+1 = both vectors are parallel.</para>
    /// <para>0 = vectors are not parallel or at least one of the vectors is zero.</para>
    /// <para>-1 = vectors are anti-parallel.</para>
    /// </returns>
    public int IsParallelTo(Vector3d other)
    {
      return IsParallelTo(other, RhinoMath.DefaultAngleTolerance);
    }

    /// <summary>
    /// Test to see whether this vector is parallel to within a custom angle tolerance of another one. 
    /// </summary>
    /// <param name="other">Vector to compare to.</param>
    /// <param name="angleTolerance">Angle tolerance (in radians)</param>
    /// <returns>
    /// Parallel indicator:
    /// <para>+1 = both vectors are parallel.</para>
    /// <para>0 = vectors are not parallel or at least one of the vectors is zero.</para>
    /// <para>-1 = vectors are anti-parallel.</para>
    /// </returns>
    public int IsParallelTo(Vector3d other, double angleTolerance)
    {
      int rc = UnsafeNativeMethods.ON_3dVector_IsParallelTo(this, other, angleTolerance);
      return rc;
    }

    // Use this for comparing ON_3dVectors until we figure out what to do about
    // overriding == and != and implementing GetHashCode()
    internal static bool ValueCompare(Vector3d vector, Vector3d other)
    {
      return (vector.m_x == other.m_x && vector.m_y == other.m_y && vector.m_z == other.m_z);
    }

    ///<summary>
    /// Test to see whether this vector is perpendicular to within one degree of another one. 
    ///</summary>
    /// <param name="other">Vector to compare to.</param>
    ///<returns>True if both vectors are perpendicular, false if otherwise.</returns>
    public bool IsPerpendicularTo(Vector3d other)
    {
      return IsPerpendicularTo(other, RhinoMath.DefaultAngleTolerance);
    }

    ///<summary>
    /// Test to see whether this vector is perpendicular to within a custom angle tolerance of another one. 
    ///</summary>
    /// <param name="other">Vector to compare to.</param>
    /// <param name="angleTolerance">Angle tolerance (in radians)</param>
    ///<returns>True if both vectors are perpendicular, false if otherwise.</returns>
    public bool IsPerpendicularTo(Vector3d other, double angleTolerance)
    {
      bool rc = false;
      double ll = Length * other.Length;
      if (ll > 0.0)
      {
        if (Math.Abs((m_x * other.m_x + m_y * other.m_y + m_z * other.m_z) / ll) <= Math.Sin(angleTolerance))
          rc = true;
      }
      return rc;
    }

    ///<summary>
    /// Set this vector to be perpendicular to another vector. 
    /// Result is not unitized.
    ///</summary>
    /// <param name="other"></param>
    ///<returns>True on success, false if input vector is zero or invalid.</returns>
    public bool PerpendicularTo(Vector3d other)
    {
      return UnsafeNativeMethods.ON_3dVector_PerpendicularTo(ref this, other);
    }
    internal static double GetLengthHelper(double dx, double dy, double dz)
    {
      if (!RhinoMath.IsValidDouble(dx) ||
          !RhinoMath.IsValidDouble(dy) ||
          !RhinoMath.IsValidDouble(dz))
        return 0.0;

      double len;
      double fx = Math.Abs(dx);
      double fy = Math.Abs(dy);
      double fz = Math.Abs(dz);
      if (fy >= fx && fy >= fz)
      {
        len = fx; fx = fy; fy = len;
      }
      else if (fz >= fx && fz >= fy)
      {
        len = fx; fx = fz; fz = len;
      }

      // 15 September 2003 Dale Lear
      //     For small denormalized doubles (positive but smaller
      //     than DBL_MIN), some compilers/FPUs set 1.0/fx to +INF.
      //     Without the ON_DBL_MIN test we end up with
      //     microscopic vectors that have infinite length!
      //
      //     Since this code starts with floats, none of this
      //     should be necessary, but it doesn't hurt anything.
      const double ON_DBL_MIN = 2.2250738585072014e-308;
      if (fx > ON_DBL_MIN)
      {
        len = 1.0 / fx;
        fy *= len;
        fz *= len;
        len = fx * Math.Sqrt(1.0 + fy * fy + fz * fz);
      }
      else if (fx > 0.0 && RhinoMath.IsValidDouble(fx))
        len = fx;
      else
        len = 0.0;
      return len;
    }
    #endregion
  }

  [StructLayout(LayoutKind.Sequential, Pack = 8, Size = 48)]
  [DebuggerDisplay("Pt({m_P.X},{m_P.Y},{m_P.Z}) Dir({m_V.X},{m_V.Y},{m_V.Z})")]
  [Serializable]
  public struct Ray3d
  {
    Point3d m_P;
    Vector3d m_V;
    public Ray3d(Point3d position, Vector3d direction)
    {
      m_P = position;
      m_V = direction;
    }

    public Point3d Position
    {
      get { return m_P; }
    }
    public Vector3d Direction
    {
      get { return m_V; }
    }

    public Point3d PointAt(double t)
    {
      if (!m_P.IsValid || !m_V.IsValid)
        return Point3d.Unset;

      Vector3d v = m_V * t;
      Point3d rc = m_P + v;
      return rc;
    }

    #region operators
    public static bool operator ==(Ray3d a, Ray3d b)
    {
      return (a.m_P == b.m_P && a.m_V == b.m_V) ? true : false;
    }
    public static bool operator !=(Ray3d a, Ray3d b)
    {
      return (a.m_P != b.m_P || a.m_V != b.m_V) ? true : false;
    }
    #endregion

    public override bool Equals(object obj)
    {
      return (obj is Ray3d && this == (Ray3d)obj);
    }
    public override int GetHashCode()
    {
      // MSDN docs recommend XOR'ing the internal values to get a hash code
      return m_P.GetHashCode() ^ m_V.GetHashCode();
    }
  }

  // 27 Jan 2010 - S. Baer
  // Removed PlaneEquation from library. Don't add until we actually need it
  //[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 32)]
  //public struct PlaneEquation
  //{
  //  Vector3d m_N;
  //  double m_D;
  //}

  //[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 16)]
  //public struct SurfaceCurvature
  //{
  //  double m_K1;
  //  double m_K2;
  //}
}
