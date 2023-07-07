using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ghost.Math
{
    /// <summary>
    /// 线性方程组工具
    /// </summary>
    public struct SystemOfLinearEquationsTools
    {
        /// <summary>
        /// EPSILON值
        /// </summary>
        private const double EPSILON = 0.00000001;

        /// <summary>
        /// 高斯 - 约当列主元消去法
        /// </summary>
        /// <param name="a">增广矩阵</param>
        /// <returns>若系数阵不为方阵，返回False和空向量；否则，返回True和结果</returns>
        public static (bool flag, double[] result) GaussElimination(in double[,] a)
        {
            int row = a.GetLength(0), column = a.GetLength(1);
            // 判断系数矩阵是否为方阵
            if (row != column - 1)
                return (false, new double[0]);

            // 备份原增广矩阵
            double[,] matrix = a;

            // 将合并矩阵的前n列化为单位矩阵
            for (int i = 0; i < row; i++)
            {
                // 从对角元素起，向下寻找绝对值最大的行号
                int location = i;
                double max = System.Math.Abs(matrix[i, i]);
                double temp;
                for (int j = i + 1; j < row; j++)
                {
                    if (System.Math.Abs(matrix[j, i]) > max)
                    {
                        location = j;
                        max = System.Math.Abs(matrix[j, i]);
                    }
                }
                // 若列主元素为0，则矩阵为奇异矩阵，没有唯一解
                if (System.Math.Abs(max) == 0)
                    return (false, new double[0]);

                // 交换两行元素
                for (int j = i; j < column; j++)
                {
                    temp = matrix[i, j];
                    matrix[i, j] = matrix[location, j];
                    matrix[location, j] = temp;
                }

                // 将此列对角线上元素化为1，并对其他元素消元
                temp = matrix[i, i];
                for (int j = i; j < column; j++)
                {
                    matrix[i, j] /= temp;
                }
                for (int j = 0; j < row; j++)
                {
                    if (i != j)
                    {
                        temp = -matrix[j, i];
                        for (int k = i; k < column; k++)
                            matrix[j, k] += temp * matrix[i, k];
                    }
                }
            }

            // 获取唯一解
            double[] result = new double[row];
            for (int i = 0; i < row; i++)
            {
                result[i] = matrix[i, column - 1];
            }

            return (true, result);
        }
    }
}
