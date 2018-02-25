using System;
using System.Drawing.Drawing2D;

namespace GameEngineX.Utility.Extensions {
    internal static class MatrixExtensions {

        internal static Matrix CreateMatrix(float[] elements) {
            if (elements.Length != 6)
                throw new ArgumentException("There must be exactly six elements in a matrix.", nameof(elements));
            
            return new Matrix(elements[0], elements[1], elements[2], elements[3], elements[4], elements[5]);
        }

    }
}
