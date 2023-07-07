using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ghost.Math
{
    /// <summary>
    /// 向量工具
    /// </summary>
    public struct VectorTools
    {
        /// <summary>
        /// 向量加法
        /// </summary>
        /// <param name="a">向量A</param>
        /// <param name="b">向量B</param>
        /// <returns>若两个向量不是同型向量，返回False和空向量；否则，返回True和结果</returns>
        public static (bool flag, double[] result) Addition(in double[] a, in double[] b)
        {
            // 判断两个向量是否为同型向量
            if (a.GetLength(0) != b.GetLength(0))
            {
                return (false, new double[0]);
            }
            // 两个向量相加
            double[] result = new double[a.GetLength(0)];
            for (int i = 0; i < a.GetLength(0); i++)
            {
                result[i] = a[i] + b[i];
            }
            return (true, result);
        }

        /// <summary>
        /// 向量减法
        /// </summary>
        /// <param name="a">向量A，作为被减向量</param>
        /// <param name="b">向量B，作为减向量</param>
        /// <returns>若两个向量不是同型向量，返回False和空向量；否则，返回True和结果</returns>
        public static (bool flag, double[] result) Subtraction(in double[] a, in double[] b)
        {
            // 判断两个向量是否为同型向量
            if (a.GetLength(0) != b.GetLength(0))
            {
                return (false, new double[0]);
            }
            // 两个向量相减
            double[] result = new double[a.GetLength(0)];
            for (int i = 0; i < a.GetLength(0); i++)
            {
                result[i] = a[i] - b[i];
            }
            return (true, result);
        }

        /// <summary>
        /// 向量数乘
        /// </summary>
        /// <param name="a">数A</param>
        /// <param name="b">向量B</param>
        /// <returns>计算成功，返回结果</returns>
        public static double[] NumberMultiplication(double a, in double[] b)
        {
            // 向量数乘
            double[] result = new double[b.GetLength(0)];
            for (int i = 0; i < b.GetLength(0); i++)
            {
                result[i] = a * b[i];
            }
            return result;
        }

        /// <summary>
        /// 向量的模
        /// </summary>
        /// <param name="a">向量A</param>
        /// <returns>计算成功，返回结果</returns>
        public static double Length(in double[] a)
        {
            double result = 0.0;
            for (int i = 0; i < a.GetLength(0); i++)
            {
                result += a[i] * a[i];
            }
            result = System.Math.Sqrt(result);
            return result;
        }


    }
}
