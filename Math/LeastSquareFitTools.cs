using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Ghost.Math
{
    /// <summary>
    /// 最小二乘拟合
    /// </summary>
    public struct LeastSquareFitTools
    {
        /// <summary>
        /// 最小二乘拟合空间平面方程。求解的平面方程为：Z = aX + bY + c
        /// </summary>
        /// <param name="input">三维离散点</param>
        /// <param name="result">计算结果</param>
        /// <returns>若系数阵为奇异矩阵，返回False，求解成功返回True</returns>
        public static bool PlaneFitting(in List<Vector3> input, out double[] result)
        {
            
            // 增广矩阵
            double[,] matrix = new double[3, 4];    

            // 计算增广矩阵各元素的值
            for (int i = 0; i < input.Count; i++)
            {
                // 第一行
                matrix[0, 0] += input[i].X * input[i].X;
                matrix[0, 1] += input[i].X * input[i].Y;
                matrix[0, 2] += input[i].X;
                matrix[0, 3] += input[i].X * input[i].Z;
                // 第二行
                matrix[1, 1] += input[i].Y * input[i].Y;
                matrix[1, 2] += input[i].Y;
                matrix[1, 3] += input[i].Y * input[i].Z;
                // 第三行
                matrix[2, 3] += input[i].Z;
            }
            
            matrix[1, 0] = matrix[0, 1];
            matrix[2, 0] = matrix[0, 2];
            matrix[2, 1] = matrix[1, 2];
            matrix[2, 2] = input.Count;

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    matrix[i, j] = matrix[i, j] / input.Count;
                }
            }
            // 求解线性方程组
            var solution = SystemOfLinearEquationsTools.GaussElimination(matrix);
            if (solution.flag)
            {
                result = solution.result;
                return true;
            }
            else
            {
                result = new double[3];
                return false;
            }
        }
    }
}
