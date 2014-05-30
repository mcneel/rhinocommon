//************************************************************************************
//
// Author: Colin Wade
//
// Copyright © 2013-2014 OMAX Corporation
//
//************************************************************************************

namespace Rhino.Drawing
{
    public struct Rectangle
    {
        int x, y, width, height;

        public Rectangle(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public int X { get { return x; } set { x = value; } }
        public int Y { get { return y; } set { y = value; } }

        public int Width { get { return width; } set { width = value; } }
        public int Height { get { return height; } set { height = value; } }

        public int Left { get { return x; } }
        public int Top { get { return y; } }

        public int Right { get { return x + width; } }
        public int Bottom { get { return y + height; } }

        public static Rectangle Empty = new Rectangle();

        public static Rectangle FromLTRB(int left, int top, int right, int bottom)
        {
            throw new System.NotImplementedException();
        }
    }
}
