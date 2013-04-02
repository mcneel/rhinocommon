using System;
using System.Collections.Generic;
using System.Text;

namespace Rhino
{
    public interface IEpsilonComparable<in T>
    {
        bool EpsilonEquals(T other, double epsilon);
    }

    public interface IEpsilonFComparable<in T>
    {
        bool EpsilonEquals(T other, float epsilon);
    }

    public static class FloatingPointCompare
    {
        public static bool EpsilonEquals(double x, double y, double epsilon)
        {
            // IEEE standard says that any comparison between NaN should return false;
            if (double.IsNaN(x) || double.IsNaN(y))
                return false;
            if (double.IsPositiveInfinity(x))
                return double.IsPositiveInfinity(y);
            if (double.IsNegativeInfinity(x))
                return double.IsNegativeInfinity(y);

            // if both are smaller than epsilon, their difference may not be.
            // therefore compare in absolute sense
            if (Math.Abs(x) < epsilon && Math.Abs(y) < epsilon)
            {
                bool result = Math.Abs(x - y) < epsilon;
                return result;
            }

            return (x >= y - epsilon && x <= y + epsilon);
        }

        public static bool EpsilonEquals(float x, float y, float epsilon)
        {
            // IEEE standard says that any comparison between NaN should return false;
            if (float.IsNaN(x) || float.IsNaN(y))
                return false;
            if (float.IsPositiveInfinity(x))
                return float.IsPositiveInfinity(y);
            if (float.IsNegativeInfinity(x))
                return float.IsNegativeInfinity(y);

            // if both are smaller than epsilon, their difference may not be.
            // therefore compare in absolute sense
            if (Math.Abs(x) < epsilon && Math.Abs(y) < epsilon)
            {
                bool result = Math.Abs(x - y) < epsilon;
                return result;
            }

            return (x >= y - epsilon && x <= y + epsilon);
        }
    }
}
