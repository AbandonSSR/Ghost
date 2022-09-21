using System;
using System.Data;

namespace Ghost.IO.PLY
{
    /// <summary>
    /// PLY 文件存储模式
    /// </summary>
    public enum PLY_Storage_Mode
    {
        ASCII,
        LITTLE_ENDIAN,
        BIG_ENDIAN,
        Undefined
    }
    /// <summary>
    /// PLY 文件元素类型
    /// </summary>
    public enum PLY_Element_Type
    {
        Cell,
        Edge,
        Face,
        Material,
        Vertex,
        Undefined
    }
    /// <summary>
    /// PLY 文件数据类型
    /// </summary>
    public enum PLY_Data_Type
    {
        Int8, UInt8,
        Int16, UInt16,
        Int32, UInt32,
        Int64, UInt64,
        Float32, Float64,
        Undefined
    }
    /// <summary>
    /// PLY 文件元素的属性
    /// </summary>
    public class PLY_Property
    {
        /// <summary>
        /// 属性的类型
        /// </summary>
        public PLY_Data_Type Type;
        /// <summary>
        /// 列表数量的类型
        /// </summary>
        public PLY_Data_Type ListCountType;
        /// <summary>
        /// 属性的名字
        /// </summary>
        public string Name;
        /// <summary>
        /// 初始化
        /// </summary>
        public PLY_Property()
        {
            this.Type = PLY_Data_Type.Undefined;
            this.ListCountType = PLY_Data_Type.Undefined;
            this.Name = string.Empty;
        }
        /// <summary>
        /// 此属性是否为列表类型
        /// </summary>
        public bool IsListProperty => ListCountType != PLY_Data_Type.Undefined;
    }
    /// <summary>
    /// PLY 文件元素
    /// </summary>
    public class PLY_Element
    {
        /// <summary>
        /// 元素的类型
        /// </summary>
        public PLY_Element_Type Type;
        /// <summary>
        /// 元素的名称
        /// </summary>
        public string Name;
        /// <summary>
        /// 此元素所含数据的数量
        /// </summary>
        public int Count;
        /// <summary>
        /// 元素的属性
        /// </summary>
        public List<PLY_Property> Properties;
        /// <summary>
        /// 此元素的数据
        /// </summary>
        public DataTable Data;
        /// <summary>
        /// 初始化
        /// </summary>
        public PLY_Element()
        {
            this.Type = PLY_Element_Type.Undefined;
            this.Name = string.Empty;
            this.Count = 0;
            this.Properties = new List<PLY_Property>();
            this.Data = new DataTable();
        }
    }
}