using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Ghost.IO.LAS
{
    public class LAS
    {
        /// <summary>
        /// LAS 文件头
        /// </summary>
        private LAS_Header header;
        /// <summary>
        /// LAS 数据
        /// </summary>
        private DataTable data;

        /// <summary>
        /// 初始化
        /// </summary>
        public LAS()
        {
            this.header = new LAS_Header();
            this.data = new DataTable();

            // 向数据表中添加 X、Y、Z、R、G、B 数据列
            var column = new DataColumn
            {
                ColumnName = "X",
                DataType = Type.GetType("System.Double")
            };
            this.data.Columns.Add(column);
            column = new DataColumn
            {
                ColumnName = "Y",
                DataType = Type.GetType("System.Double")
            };
            this.data.Columns.Add(column);
            column = new DataColumn
            {
                ColumnName = "Z",
                DataType = Type.GetType("System.Double")
            };
            this.data.Columns.Add(column);
        }

        /// <summary>
        /// 获取 LAS 文件头
        /// </summary>
        public LAS_Header Header => this.header;
        /// <summary>
        /// 获取 LAS 数据
        /// </summary>
        public DataTable Data => this.data;

        /// <summary>
        /// 读取 LAS 文件。仅读取X、Y、Z坐标数据以及R、G、B颜色数据(若存在)。
        /// </summary>
        /// <param name="fileName">LAS 文件绝对路径</param>
        /// <returns>若成功读取，返回 True；否则，返回 False</returns>
        public bool Read(string fileName)
        {
            try
            {
                this.header = new LAS_Header();
                this.data.Rows.Clear();

                BinaryReader br = new BinaryReader(new FileStream(fileName, FileMode.Open));
                if (!Read_Header(br))
                    return false;
                if (!Read_Data(br))
                    return false;
                br.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 读取 LAS 文件头
        /// </summary>
        /// <param name="br">二进制读取流</param>
        /// <returns>若成功读取，返回 True；否则，返回 False</returns>
        private bool Read_Header(BinaryReader br)
        {
            try
            {
                this.header.FileSignature = br.ReadChars(4);
                // 若不为 LAS 文件返回 False
                if (new string(this.header.FileSignature) != "LASF")
                    return false;
                this.header.FileSourceId = br.ReadUInt16();
                this.header.GlobalEncoding = br.ReadUInt16();
                this.header.ProjectID_GUID_Data1 = br.ReadUInt32();
                this.header.ProjectID_GUID_Data2 = br.ReadUInt16();
                this.header.ProjectID_GUID_Data3 = br.ReadUInt16();
                this.header.ProjectID_GUID_Data4 = br.ReadBytes(8);
                this.header.VersionMajor = br.ReadByte();
                this.header.VersionMinor = br.ReadByte();
                this.header.SystemIdentifier = br.ReadChars(32);
                this.header.GeneratingSoftware = br.ReadChars(32);
                this.header.FileCreationDayOfYear = br.ReadUInt16();
                this.header.FileCreationYear = br.ReadUInt16();
                this.header.HeaderSize = br.ReadUInt16();
                this.header.OffsetToPointData = br.ReadUInt32();
                this.header.NumberOfVariableLengthRecords = br.ReadUInt32();
                this.header.PointDataRecordFormat = br.ReadByte();
                this.header.PointDataRecordLength = br.ReadUInt16();
                this.header.LegacyNumberOfPointRecords = br.ReadUInt32();
                this.header.LegacyNumberOfPointsByReturn = new uint[5];
                for (int i = 0; i < 5; i++)
                    this.header.LegacyNumberOfPointsByReturn[i] = br.ReadUInt32();
                this.header.XScaleFactor = br.ReadDouble();
                this.header.YScaleFactor = br.ReadDouble();
                this.header.ZScaleFactor = br.ReadDouble();
                this.header.XOffset = br.ReadDouble();
                this.header.YOffset = br.ReadDouble();
                this.header.ZOffset = br.ReadDouble();
                this.header.MaxX = br.ReadDouble();
                this.header.MinX = br.ReadDouble();
                this.header.MaxY = br.ReadDouble();
                this.header.MinY = br.ReadDouble();
                this.header.MaxZ = br.ReadDouble();
                this.header.MinZ = br.ReadDouble();
                if (this.header.VersionMajor == 1 && this.header.VersionMinor >= 3)
                {
                    this.header.StartOfWaveformDataPacketRecord = br.ReadUInt64();
                    if (this.header.VersionMinor == 4)
                    {
                        this.header.StartOfFirstExtendedVariableLengthRecord = br.ReadUInt64();
                        this.header.NumberOfExtendedVariableLengthRecords = br.ReadUInt32();
                        this.header.NumberOfPointRecords = br.ReadUInt64();
                        this.header.NumberOfPointsByReturn = new ulong[15];
                        for (int i = 0; i < 15; i++)
                            this.header.NumberOfPointsByReturn[i] = br.ReadUInt64();
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 读取 LAS 数据
        /// </summary>
        /// <param name="br">二进制读取流</param>
        /// <returns>若成功读取，返回 True；否则，返回 False</returns>
        private bool Read_Data(BinaryReader br)
        {
            br.ReadBytes((int)(this.header.OffsetToPointData - this.header.HeaderSize)); // 将流的位置提升若干字节至点数据的开始
            // 提取相关头文件参数
            var format = this.header.PointDataRecordFormat;
            var length = this.header.PointDataRecordLength;
            var isLAS14 = this.header.VersionMajor == '1' && this.header.VersionMinor == '4';
            var count = isLAS14 ? this.header.NumberOfPointRecords : this.header.LegacyNumberOfPointRecords;

            // 若点类型为2、3、5、7、8、10，创建 R、G、B 数据列
            switch (format)
            {
                case 2:
                case 3:
                case 5:
                case 7:
                case 8:
                case 10:
                    var column = new DataColumn
                    {
                        ColumnName = "R",
                        DataType = Type.GetType("System.Byte")
                    };
                    this.data.Columns.Add(column);
                    column = new DataColumn
                    {
                        ColumnName = "G",
                        DataType = Type.GetType("System.Byte")
                    };
                    this.data.Columns.Add(column);
                    column = new DataColumn
                    {
                        ColumnName = "B",
                        DataType = Type.GetType("System.Byte")
                    };
                    this.data.Columns.Add(column);
                    break;
            }

            // 读取 LAS 点数据
            for (ulong i = 0; i < count; i++)
            {
                // 读取坐标数据
                var row = this.data.NewRow();
                row["X"] = br.ReadInt32() * header.XScaleFactor + header.XOffset;
                row["Y"] = br.ReadInt32() * header.YScaleFactor + header.YOffset;
                row["Z"] = br.ReadInt32() * header.ZScaleFactor + header.ZOffset;

                // 若点类型为0、1、4、6、9，不含 R、G、B，跳过非关键信息，直达此条数据末尾
                switch (format)
                {
                    case 0:
                    case 1:
                    case 4:
                    case 6:
                    case 9:
                        br.ReadBytes(length - 12);
                        break;
                }

                // 若点类型为2、3、5、7、8、10，跳过非关键信息，直达 R、G、B 位置
                switch (format)
                {
                    case 2:
                        br.ReadBytes(8); break;
                    case 3:
                    case 5:
                        br.ReadBytes(16); break;
                    case 7:
                    case 8:
                    case 10:
                        br.ReadBytes(18); break;
                }

                // 若点类型为2、3、5、7、8、10，读取 R、G、B 信息
                switch (format)
                {
                    case 2:
                    case 3:
                    case 5:
                    case 7:
                    case 8:
                    case 10:
                        row["R"] = (byte)(br.ReadUInt16() / 256);
                        row["G"] = (byte)(br.ReadUInt16() / 256);
                        row["B"] = (byte)(br.ReadUInt16() / 256);
                        break;
                }

                // 若点类型为5、8、10，跳过非关键信息，直达此条数据末尾
                switch (format)
                {
                    case 5:
                        br.ReadBytes(length - 12 - 16 - 6);
                        break;
                    case 8:
                    case 10:
                        br.ReadBytes(length - 12 - 18 - 6);
                        break;
                }

                this.data.Rows.Add(row);
            }

            return true;
        }

        /// <summary>
        /// 写入 LAS 文件。仅写入X、Y、Z坐标数据以及R、G、B颜色数据(若存在)。
        /// </summary>
        /// <param name="fileName">LAS 文件绝对路径</param>
        /// <returns>若成功写入，返回 True；否则，返回 False</returns>
        public bool Write(string fileName)
        {
            if (this.data.Rows.Count == 0)
                return false;

            try
            {
                BinaryWriter bw = new BinaryWriter(new FileStream(fileName, FileMode.Create));
                if (!Write_Header(bw))
                    return false;
                if (!Write_Data(bw))
                    return false;
                bw.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 写入 LAS 文件头
        /// </summary>
        /// <param name="br">二进制写入流</param>
        /// <returns>若成功写入，返回 True；否则，返回 False</returns>
        private bool Write_Header(BinaryWriter bw)
        {
            try
            {
                bw.Write(new byte[4] { (byte)'L', (byte)'A', (byte)'S', (byte)'F' }); // LAS 文件标志
                bw.Write(new byte[20]);
                bw.Write(new byte[2] { 1, 2 }); // LAS 文件版本
                bw.Write(new byte[64]);
                bw.Write((ushort)DateTime.Now.DayOfYear);
                bw.Write((ushort)DateTime.Now.Year);
                bw.Write((ushort)227); // 头文件所占字节数
                bw.Write((uint)227);   // 点数据开始字节数
                bw.Write(new byte[4]);
                if (this.data.Columns.Contains("R") && this.data.Columns.Contains("G") && this.data.Columns.Contains("B"))
                {
                    bw.Write((byte)2);    // 点数据类型
                    bw.Write((ushort)26); // 点数据类型对应的字节数
                }
                else
                {
                    bw.Write((byte)0);
                    bw.Write((ushort)20);
                }
                if (this.data.Rows.Count == 0)
                {
                    return false;
                }
                bw.Write((uint)this.data.Rows.Count); // 点的数量
                bw.Write((uint)this.data.Rows.Count); // 第一次返回点的数量，
                bw.Write(new byte[16]);
                bw.Write(0.001);            // X、Y、Z坐标的比例系数
                bw.Write(0.001);
                bw.Write(0.001);
                bw.Write(0.0);              // X、Y、Z坐标的偏移值
                bw.Write(0.0);
                bw.Write(0.0);

                var boundBox = BoundBox();
                bw.Write(boundBox.min[0]);  // X 原始坐标范围
                bw.Write(boundBox.max[0]);
                bw.Write(boundBox.min[1]);  // Y 原始坐标范围
                bw.Write(boundBox.max[1]);
                bw.Write(boundBox.min[2]);  // Z 原始坐标范围
                bw.Write(boundBox.max[2]);
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 写入 LAS 数据
        /// </summary>
        /// <param name="bw">二进制写入流</param>
        /// <returns>若成功写入，返回 True；否则，返回 False</returns>
        private bool Write_Data(BinaryWriter bw)
        {
            try
            {
                for (int i = 0; i < this.data.Rows.Count; i++)
                {
                    var row = this.data.Rows[i];

                    bw.Write((int)((double)row["X"] * 1000));
                    bw.Write((int)((double)row["Y"] * 1000));
                    bw.Write((int)((double)row["Z"] * 1000));

                    if (this.data.Columns.Contains("R") && this.data.Columns.Contains("G") && this.data.Columns.Contains("B"))
                    {
                        bw.Write(new byte[8]);
                        bw.Write((ushort)((byte)row["R"] * 256));
                        bw.Write((ushort)((byte)row["G"] * 256));
                        bw.Write((ushort)((byte)row["B"] * 256));
                    }
                    else
                    {
                        bw.Write(new byte[8]);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 计算点云的X、Y、Z坐标最小值和最大值
        /// </summary>
        /// <returns>X、Y、Z坐标最小值和最大值</returns>
        private (double[] min, double[] max) BoundBox()
        {
            var row = this.data.Rows[0];
            var min = new double[] { (double)row["X"], (double)row["Y"], (double)row["Z"] };
            var max = new double[] { (double)row["X"], (double)row["Y"], (double)row["Z"] };

            for (int i = 1; i < this.data.Rows.Count; i++)
            {
                row = this.data.Rows[i];

                if ((double)row["X"] < min[0])
                    min[0] = (double)row["X"];
                else if ((double)row["X"] > max[0])
                    max[0] = (double)row["X"];

                if ((double)row["Y"] < min[1])
                    min[1] = (double)row["Y"];
                else if ((double)row["Y"] > max[1])
                    max[1] = (double)row["Y"];

                if ((double)row["Z"] < min[2])
                    min[2] = (double)row["Z"];
                else if ((double)row["Z"] > max[2])
                    max[2] = (double)row["Z"];
            }
            return (min, max);
        }
    }
}
