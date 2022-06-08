using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnFData.Model
{
    public struct Highlight
    {
        public double X;
        public double Y;
        public double Width;
        public double Height;
        public Highlight (double x, double y, double width, double height)
        {
            X = x; 
            Y = y; 
            Width = width;
            Height = height;
        }
    }
}
