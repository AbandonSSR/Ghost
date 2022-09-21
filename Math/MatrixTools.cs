using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ghost.Math
{
    /// <summary>
    /// 矩阵工具
    /// </summary>
    public struct MatrixTools
    {
        /// <summary>
        /// 矩阵加法
        /// </summary>
        /// <param name="a">矩阵A</param>
        /// <param name="b">矩阵B</param>
        /// <returns>若两个矩阵不是同型矩阵，返回False和空矩阵；否则，返回True和结果</returns>
        public static (bool flag, double[,] result) Addition(in double[,] a, in double[,] b)
        {
            int row = a.GetLength(0), column = a.GetLength(1);
            // 判断两个矩阵是否为同型矩阵
            if (row != b.GetLength(0) || column != b.GetLength(1))
                return (false, new double[0, 0]);

            // 两个矩阵相加
            double[,] result = new double[row, column];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    result[i, j] = a[i, j] + b[i, j];
                }
            }

            return (true, result);
        }

        /// <summary>
        /// 矩阵减法
        /// </summary>
        /// <param name="a">矩阵A，作为被减矩阵</param>
        /// <param name="b">矩阵B，作为减矩阵</param>
        /// <returns>若两个矩阵不是同型矩阵，返回False和空矩阵；否则，返回True和结果</returns>
        public static (bool flag, double[,] result) Subtraction(in double[,] a, in double[,] b)
        {
            int row = a.GetLength(0), column = a.GetLength(1);
            // 判断两个矩阵是否为同型矩阵
            if (row != b.GetLength(0) || column != b.GetLength(1))
                return (false, new double[0, 0]);

            // 两个矩阵相加
            double[,] result = new double[row, column];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    result[i, j] = a[i, j] - b[i, j];
                }
            }

            return (true, result);
        }

        /// <summary>
        /// 矩阵数乘
        /// </summary>
        /// <param name="a">数A</param>
        /// <param name="b">矩阵B</param>
        /// <returns>计算成功，返回结果</returns>
        public static double[,] NumberMultiplication(in double a, in double[,] b)
        {
            int row = b.GetLength(0), column = b.GetLength(1);

            // 矩阵数乘
            double[,] result = new double[row, column];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    result[i, j] = a * b[i, j];
                }
            }

            return result;
        }

        /// <summary>
        /// 矩阵乘法
        /// </summary>
        /// <param name="a">矩阵A</param>
        /// <param name="b">矩阵B</param>
        /// <returns>若两个矩阵不满足矩阵乘法要求，返回False和空矩阵；否则，返回True和结果</returns>
        public static (bool flag, double[,] result) Multiplication(in double[,] a, in double[,] b)
        {
            int row = a.GetLength(0), column = b.GetLength(1);
            // 判断两个矩阵是否满足乘法要求
            if ( a.GetLength(1) != b.GetLength(0))
                return (false, new double[0, 0]);

            // 两个矩阵相乘
            double[,] result = new double[row, column];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    for (int k = 0; k < a.GetLength(1); k++)
                    {
                        result[i, j] += a[i, k] * b[k, j];
                    }
                }
            }

            return (true, result);
        }

        /// <summary>
        /// 矩阵的转置
        /// </summary>
        /// <param name="a">矩阵A</param>
        /// <returns>计算成功，返回结果</returns>
        public static double[,] Transpose(in double[,] a)
        {
            int row = a.GetLength(0), column = a.GetLength(1);

            // 矩阵的转置
            double[,] result = new double[column, row];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    result[j, i] = a[i, j];
                }
            }

            return result;
        }

        /// <summary>
        /// 矩阵的行列式
        /// </summary>
        /// <param name="a">矩阵A</param>
        /// <returns>若矩阵不是方阵，返回False和空矩阵；否则，返回True和结果</returns>
        public static (bool flag, double result) Determinant(in double[,] a)
        {
            int row = a.GetLength(0), column = a.GetLength(1);
            double[,] matrix = a; //备份矩阵
            // 判断是否为方阵
            if (row != column)
                return (false, 0.0);

            // 化为上三角形式
            uint switchtime = 0; // 记录交换行的次数
            for (int i = 0; i < row - 1; i++)
            {
                if (matrix[i, i] == 0)
                {
                    // 若对角线元素为0，寻找非零行并交换
                    bool flag = true;
                    int location = i;
                    for (int j = i + 1; j < row; j++)
                    {
                        if (a[j, i] != 0) // 若a[j ,i]元素不等于0，则交换次数+1，并记录行号
                        {
                            switchtime++;
                            location = j;
                            break;
                        }
                        if (j == row - 1) // 若该列从对角线元素起全部为0，则标记为false
                            flag = false;
                    }
                    if (!flag) // 若某一列从对角线元素起全部为0，则行列式为0
                        return (true, 0);

                    // 交换两行元素
                    for (int j = i; j < column; j++)
                    {
                        double temp = matrix[i, j];
                        matrix[i, j] = matrix[location, j];
                        matrix[location, j] = temp;
                    }
                }
                // 类似于高斯消去法
                for (int j = i + 1; j < row; j++)
                {
                    double temp = -matrix[j, i] / matrix[i, i];
                    for (int k = i; k < column; k++)
                        matrix[j, k] += temp * matrix[i, k];
                }
            }
            // 计算行列式
            double result = 1;
            for (int i = 0; i < row; i++)
                result *= matrix[i, i];
            if (switchtime % 2 != 0) // 若交换次数为偶数，则结果不变；若交换次数为奇数，则结果为相反数
                return (true, -result);
            else
                return (true, result);
        }

        /// <summary>
        /// 矩阵的逆矩阵。高斯-约当列主元消去法
        /// </summary>
        /// <param name="a">矩阵A</param>
        /// <returns>若矩阵不是方阵或为奇异矩阵，返回False和空矩阵；否则，返回True和结果</returns>
        public static (bool flag, double[,] result) Inverse(in double[,] a)
        {
            // 判断是否为方阵
            int row = a.GetLength(0), column = a.GetLength(1);
            if (row != column)
                return (false, new double[0,0]);

            /* 高斯-约当消去法 */
            // 矩阵A与单位矩阵合并
            double[,] matrix = new double[row, 2 * column];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < 2 * column; j++)
                {
                    if (j < column)
                        matrix[i, j] = a[i, j];
                    if (j == column + i)
                        matrix[i, j] = 1;
                }
            }
            // 将合并矩阵的前n列化为单位矩阵
            for (int i = 0; i < row; i++)
            {
                // 从对角元素起，向下寻找绝对值最大的行号
                int location = i;
                double max = System.Math.Abs(matrix[i, i]);
                double temp;
                for (int j = i+1; j < row; j++)
                {
                    if (System.Math.Abs(matrix[j, i]) > max)
                    {
                        location = j;
                        max = System.Math.Abs(matrix[j, i]);
                    }
                }
                // 若列主元素为0，则矩阵为奇异矩阵，没有唯一解
                if (max == 0)
                    return (false, new double[0, 0]);

                // 交换两行元素
                for (int j = i; j < 2 * column; j++)
                {
                    temp = matrix[i, j];
                    matrix[i, j] = matrix[location, j];
                    matrix[location, j] = temp;
                }

                // 将此列对角线上元素化为1，并对其他元素消元
                temp = matrix[i, i];
                for (int j = i; j < 2 * column; j++)
                {
                    matrix[i, j] /= temp;
                }
                for (int j = 0; j < row; j++)
                {
                    if (i != j)
                    {
                        temp = -matrix[j, i];
                        for (int k = i; k < 2 * column; k++)
                            matrix[j, k] += temp * matrix[i, k];
                    }
                }
            }

            // 获取逆矩阵
            double[,] result = new double[row, column];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    result[i, j] = matrix[i, j + column];
                }
            }
            return (true, result);
        }
    }
}
