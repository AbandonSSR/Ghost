using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Ghost.Math
{
    /// <summary>
    /// 空间解析几何
    /// </summary>
    public struct SpaceParsingGeometricTools
    {
        /// <summary>
        /// 点 P 到空间平面的距离
        /// </summary>
        /// <param name="p">点 P </param>
        /// <param name="c">空间平面方程系数。平面方程：aX + bY + cZ + D = 0</param>
        /// <returns>返回距离</returns>
        public static double DistanceOfPointToPlane(Vector3 p, in double[] c)
        {
            double a = System.Math.Abs(c[0] * p.X + c[1] * p.Y + c[2] * p.Z + c[3]);
            double b = System.Math.Sqrt(c[0] * c[0] + c[1] * c[1] + c[2] * c[2]);
            return a / b;
        }
    }
}
