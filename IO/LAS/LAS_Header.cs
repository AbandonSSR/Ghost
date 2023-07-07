using System;
using System.Data;

namespace Ghost.IO.LAS
{
    /// <summary>
    ///  LAS 文件的头文件
    /// </summary>
    public struct LAS_Header
    {
        /// <summary>
        /// 文件签名
        /// </summary>
        public char[] FileSignature { get; set; }
        /// <summary>
        /// 文件来源ID
        /// </summary>
        public ushort FileSourceId { get; set; }
        /// <summary>
        /// 全球编码
        /// </summary>
        public ushort GlobalEncoding { get; set; }
        /// <summary>
        /// 项目ID_1
        /// </summary>
        public uint ProjectID_GUID_Data1 { get; set; }
        /// <summary>
        /// 项目ID_2
        /// </summary>
        public ushort ProjectID_GUID_Data2 { get; set; }
        /// <summary>
        /// 项目ID_3
        /// </summary>
        public ushort ProjectID_GUID_Data3 { get; set; }
        /// <summary>
        /// 项目ID_4
        /// </summary>
        public byte[] ProjectID_GUID_Data4 { get; set; }
        /// <summary>
        /// 主版本号
        /// </summary>
        public byte VersionMajor { get; set; }
        /// <summary>
        /// 次版本号
        /// </summary>
        public byte VersionMinor { get; set; }
        /// <summary>
        /// 系统标识符
        /// </summary>
        public char[] SystemIdentifier { get; set; }
        /// <summary>
        /// 生成LAS文件的软件
        /// </summary>
        public char[] GeneratingSoftware { get; set; }
        /// <summary>
        /// 创建文件时所在年的第几天
        /// </summary>
        public ushort FileCreationDayOfYear { get; set; }
        /// <summary>
        /// 创建文件时所在年
        /// </summary>
        public ushort FileCreationYear { get; set; }
        /// <summary>
        /// 公共头文件区所用字节数
        /// </summary>
        public ushort HeaderSize { get; set; }
        /// <summary>
        /// 数据点的起始字节位置
        /// </summary>
        public uint OffsetToPointData { get; set; }
        /// <summary>
        /// 可变长度区的数量
        /// </summary>
        public uint NumberOfVariableLengthRecords { get; set; }
        /// <summary>
        /// 点数据的格式
        /// </summary>
        public byte PointDataRecordFormat { get; set; }
        /// <summary>
        /// 每一个点数据的长度
        /// </summary>
        public ushort PointDataRecordLength { get; set; }
        /// <summary>
        /// 点的数量（传统的）
        /// </summary>
        public uint LegacyNumberOfPointRecords { get; set; }
        /// <summary>
        /// 返回的点的数量（传统的）
        /// </summary>
        public uint[] LegacyNumberOfPointsByReturn { get; set; }
        /// <summary>
        /// X比例系数
        /// </summary>
        public double XScaleFactor { get; set; }
        /// <summary>
        /// Y比例系数
        /// </summary>
        public double YScaleFactor { get; set; }
        /// <summary>
        /// Z比例系数
        /// </summary>
        public double ZScaleFactor { get; set; }
        /// <summary>
        /// X偏移量
        /// </summary>
        public double XOffset { get; set; }
        /// <summary>
        /// Y偏移量
        /// </summary>
        public double YOffset { get; set; }
        /// <summary>
        /// Z偏移量
        /// </summary>
        public double ZOffset { get; set; }
        /// <summary>
        /// X的最大值
        /// </summary>
        public double MaxX { get; set; }
        /// <summary>
        /// X的最小值
        /// </summary>
        public double MinX { get; set; }
        /// <summary>
        /// Y的最大值
        /// </summary>
        public double MaxY { get; set; }
        /// <summary>
        /// Y的最小值
        /// </summary>
        public double MinY { get; set; }
        /// <summary>
        /// Z的最大值
        /// </summary>
        public double MaxZ { get; set; }
        /// <summary>
        /// Z的最小值
        /// </summary>
        public double MinZ { get; set; }
        /// <summary>
        /// 波形数据包记录的开始
        /// </summary>
        public ulong StartOfWaveformDataPacketRecord { get; set; }
        /// <summary>
        /// 第一个扩展可变长度记录的开始
        /// </summary>
        public ulong StartOfFirstExtendedVariableLengthRecord { get; set; }
        /// <summary>
        /// 扩展可变长度记录的数量
        /// </summary>
        public uint NumberOfExtendedVariableLengthRecords { get; set; }
        /// <summary>
        /// 点的数量
        /// </summary>
        public ulong NumberOfPointRecords { get; set; }
        /// <summary>
        /// 返回的点的数量
        /// </summary>
        public ulong[] NumberOfPointsByReturn { get; set; }
    }
}
