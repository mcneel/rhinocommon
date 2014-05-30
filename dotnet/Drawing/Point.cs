//************************************************************************************
//
// Author: Colin Wade
//
// Copyright © 2013-2014 OMAX Corporation
//
//************************************************************************************

namespace Rhino.Drawing
{
    public struct Point
    {
        int x, y;

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int X { get { return x; } set { x = value; } }
        public int Y { get { return y; } set { y = value; } }

        public static Point Empty = new Point();
    }
}
