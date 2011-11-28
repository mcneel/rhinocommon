#pragma warning disable 1591
using System;

namespace Rhino.Geometry
{
  /// <summary>
  /// Arbitrarily sized matrix of values. If you are working with a
  /// 4x4 matrix, then you may want to use the Transform class instead.
  /// </summary>
  public class Matrix : IDisposable
  {
    IntPtr m_ptr; //ON_Matrix*
    int m_rows;
    int m_columns;

    public Matrix(int rowCount, int columnCount)
    {
      if (rowCount < 0 )
        throw new ArgumentOutOfRangeException("rowCount", "must be >= 0");
      if (columnCount < 0)
        throw new ArgumentOutOfRangeException("columnCount", "must be >= 0");
      m_rows = rowCount;
      m_columns = columnCount;
      m_ptr = UnsafeNativeMethods.ON_Matrix_New(rowCount, columnCount);
    }

    public Matrix(Transform xform)
    {
      m_rows = 4;
      m_columns = 4;
      m_ptr = UnsafeNativeMethods.ON_Matrix_New2(ref xform);
    }

    #region IDisposable implementation
    ~Matrix()
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
      if (m_ptr!=IntPtr.Zero)
      {
        UnsafeNativeMethods.ON_Matrix_Delete(m_ptr);
      }
      m_ptr = IntPtr.Zero;
    }
    #endregion


    /// <summary>
    /// Gets or sets the matrix value at the given row and column indixes.
    /// </summary>
    /// <param name="row">Index of row to access</param>
    /// <param name="column">Index of column to access</param>
    /// <returns>The value at [row, column]</returns>
    /// <value>The new value at [row, column]</value>
    public double this[int row, int column]
    {
      get
      {
        if (row < 0 || row>=m_rows)
          throw new IndexOutOfRangeException("row index out of range");
        if (column < 0 || column>=m_columns)
          throw new IndexOutOfRangeException("column index out of range");

        return UnsafeNativeMethods.ON_Matrix_GetValue(m_ptr, row, column);
      }
      set
      {
        if (row < 0 || row >= m_rows)
          throw new IndexOutOfRangeException("row index out of range");
        if (column < 0 || column >= m_columns)
          throw new IndexOutOfRangeException("column index out of range");
        UnsafeNativeMethods.ON_Matrix_SetValue(m_ptr, row, column, value);
      }
    }

    public bool IsValid
    {
      get { return (m_ptr != IntPtr.Zero && m_columns > 0 && m_rows > 0); }
    }

    public bool IsSquare
    {
      get { return (m_rows > 0 && m_columns == m_rows); }
    }

    public int RowCount { get { return m_rows; } }

    public int ColumnCount { get { return m_columns; } }

    public void Zero()
    {
      UnsafeNativeMethods.ON_Matrix_Zero(m_ptr);
    }

    /// <summary>
    /// Sets diagonal value and zeros off diagonal values
    /// </summary>
    /// <param name="d"></param>
    public void SetDiagonal(double d)
    {
      UnsafeNativeMethods.ON_Matrix_SetDiagonal(m_ptr, d);
    }

    public bool Transpose()
    {
      return UnsafeNativeMethods.ON_Matrix_Transpose(m_ptr);
    }

    public bool SwapRows(int rowA, int rowB)
    {
      return UnsafeNativeMethods.ON_Matrix_Swap(m_ptr, true, rowA, rowB);
    }

    public bool SwapColumns(int columnA, int columnB)
    {
      return UnsafeNativeMethods.ON_Matrix_Swap(m_ptr, false, columnA, columnB);
    }

    public bool Invert(double zeroTolerance)
    {
      return UnsafeNativeMethods.ON_Matrix_Invert(m_ptr, zeroTolerance);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// When a.ColumnCount != b.RowCount
    /// </exception>
    public static Matrix operator *(Matrix a, Matrix b)
    {
      if (a.ColumnCount != b.RowCount)
        throw new ArgumentException("a.ColumnCount != b.RowCount");
      if (a.RowCount < 1 || a.ColumnCount < 1 || b.ColumnCount < 1)
        throw new ArgumentException("either a of b are Invalid");

      Matrix rc = new Matrix(a.RowCount, b.ColumnCount);
      UnsafeNativeMethods.ON_Matrix_Multiply(rc.m_ptr, a.m_ptr, b.m_ptr);
      return rc;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// When the two matrics are not the same size
    /// </exception>
    public static Matrix operator +(Matrix a, Matrix b)
    {
      if (a.ColumnCount != b.ColumnCount || a.RowCount != b.RowCount)
        throw new ArgumentException("ColumnCount and RowCount must be same for both matrices");
      if (a.RowCount < 1 || a.ColumnCount < 1)
        throw new ArgumentException("either a of b are Invalid");

      Matrix rc = new Matrix(a.RowCount, a.ColumnCount);
      UnsafeNativeMethods.ON_Matrix_Add(rc.m_ptr, a.m_ptr, b.m_ptr);
      return rc;
    }

    public void Scale(double s)
    {
      UnsafeNativeMethods.ON_Matrix_Scale(m_ptr, s);
    }

    /// <summary>Row reduce a matrix to calculate rank and determinant</summary>
    /// <param name="zeroTolerance">
    /// (&gt;=0.0) zero tolerance for pivot test.  If a the absolute value of
    /// a pivot is &lt;= zeroTolerance, then the pivot is assumed to be zero.
    /// </param>
    /// <param name="determinant">value of determinant is returned here</param>
    /// <param name="pivot">value of the smallest pivot is returned here</param>
    /// <returns>Rank of the matrix</returns>
    /// <remarks>
    /// The matrix itself is row reduced so that the result is an upper
    /// triangular matrix with 1's on the diagonal.
    /// </remarks>
    public int RowReduce( double zeroTolerance, out double determinant, out double pivot)
    {
      determinant = 0;
      pivot = 0;
      return UnsafeNativeMethods.ON_Matrix_RowReduce(m_ptr, zeroTolerance, ref determinant, ref pivot);
    }

    /// <summary>
    /// Row reduce a matrix as the first step in solving M*X=b where
    /// b is a column of values.
    /// </summary>
    /// <param name="zeroTolerance">
    /// (&gt;=0.0) zero tolerance for pivot test. If the absolute value of a pivot
    /// is &lt;= zero_tolerance, then the pivot is assumed to be zero.
    /// </param>
    /// <param name="b">an array of RowCount values that is row reduced with the matrix
    /// </param>
    /// <param name="pivot">the value of the smallest pivot is returned here</param>
    /// <returns>Rank of the matrix</returns>
    /// <remarks>
    /// The matrix itself is row reduced so that the result is an upper
    /// triangular matrix with 1's on the diagonal.
    /// </remarks>
    public int RowReduce(double zeroTolerance, double[] b, out double pivot)
    {
      if (b.Length != RowCount)
        throw new ArgumentOutOfRangeException("b.Length!=RowCount");
      pivot = 0;
      return UnsafeNativeMethods.ON_Matrix_RowReduce2(m_ptr, zeroTolerance, b, ref pivot);
    }

    /// <summary>
    /// Row reduce a matrix as the first step in solving M*X=b where
    /// b is a column of 3d points.
    /// </summary>
    /// <param name="zeroTolerance">
    /// (&gt;=0.0) zero tolerance for pivot test. If the absolute value of a pivot
    /// is &lt;= zero_tolerance, then the pivot is assumed to be zero.
    /// </param>
    /// <param name="b">an array of RowCount 3d points that is row reduced with the matrix
    /// </param>
    /// <param name="pivot">the value of the smallest pivot is returned here</param>
    /// <returns>Rank of the matrix</returns>
    /// <remarks>
    /// The matrix itself is row reduced so that the result is an upper
    /// triangular matrix with 1's on the diagonal.
    /// </remarks>
    public int RowReduce(double zeroTolerance, Point3d[] b, out double pivot)
    {
      if (b.Length != RowCount)
        throw new ArgumentOutOfRangeException("b.Length!=RowCount");
      pivot = 0;
      return UnsafeNativeMethods.ON_Matrix_RowReduce3(m_ptr, zeroTolerance, b, ref pivot);
    }

    /// <summary>
    /// Solve M*x=b where M is upper triangular with a unit diagonal and
    /// b is a column of values.
    /// </summary>
    /// <param name="zeroTolerance"></param>
    /// <param name="b"></param>
    /// <returns>
    /// Array of length ColumnCount on success. null on error
    /// </returns>
    public double[] BackSolve(double zeroTolerance, double[] b)
    {
      double[] x = new double[ColumnCount];
      if (UnsafeNativeMethods.ON_Matrix_BackSolve(m_ptr, zeroTolerance, b.Length, b, x))
        return x;
      return null;
    }

    /// <summary>
    /// Solve M*x=b where M is upper triangular with a unit diagonal and
    /// b is a column of 3d points.
    /// </summary>
    /// <param name="zeroTolerance"></param>
    /// <param name="b"></param>
    /// <returns>
    /// Array of length ColumnCount on success. null on error
    /// </returns>
    public Point3d[] BackSolvePoints(double zeroTolerance, Point3d[] b)
    {
      Point3d[] x = new Point3d[ColumnCount];
      if (UnsafeNativeMethods.ON_Matrix_BackSolve2(m_ptr, zeroTolerance, b.Length, b, x))
        return x;
      return null;
    }

    const int idxIsRowOrthoganal = 0;
    const int idxIsRowOrthoNormal = 1;
    const int idxIsColumnOrthoganal = 2;
    const int idxIsColumnOrthoNormal = 3;
    bool GetBool(int which)
    {
      return UnsafeNativeMethods.ON_Matrix_GetBool(m_ptr, which);
    }

    public bool IsRowOrthoganal
    {
      get { return GetBool(idxIsRowOrthoganal); }
    }
    public bool IsColumnOrthoganal
    {
      get { return GetBool(idxIsColumnOrthoganal); }
    }
    public bool IsRowOrthoNormal
    {
      get { return GetBool(idxIsRowOrthoNormal); }
    }
    public bool IsColumnOrthoNormal
    {
      get { return GetBool(idxIsColumnOrthoNormal); }
    }
  }
}