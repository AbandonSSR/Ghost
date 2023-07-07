using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Ghost.IO.PLY
{
    public class PLY
    {
        /// <summary>
        /// PLY 文件存储格式
        /// </summary>
        private PLY_Storage_Mode format;
        /// <summary>
        /// PLY 文件元素
        /// </summary>
        private List<PLY_Element> elements;

        /// <summary>
        /// 初始化
        /// </summary>
        public PLY()
        {
            this.format = PLY_Storage_Mode.Undefined;
            this.elements = new List<PLY_Element>();
        }

        /// <summary>
        /// 读取 PLY 文件
        /// </summary>
        /// <param name="fileName">PLY 文件绝对路径</param>
        /// <returns>若成功读取，返回 True；否则返回 False</returns>
        public bool PLY_Read(string fileName)
        {
            if (this.elements.Count != 0 || this.format != PLY_Storage_Mode.Undefined)
            {
                this.elements.Clear();
                this.format = PLY_Storage_Mode.Undefined;
            }

            try
            {
                FileStream fs = File.OpenRead(fileName);
                if (!PLY_Read_Header(fs))
                    return false;

                switch (this.format)
                {
                    case PLY_Storage_Mode.ASCII:
                        if (!PLY_Read_Ascii(fs))
                            return false;
                        break;
                    case PLY_Storage_Mode.LITTLE_ENDIAN:
                    case PLY_Storage_Mode.BIG_ENDIAN:
                        if (!PLY_Read_Binary(fs))
                            return false;
                        break;
                    default: return false;
                }
                fs.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 读取 PLY 文件头
        /// </summary>
        /// <param name="fs">读取文件流</param>
        /// <returns>若成功读取，返回 True；否则返回 False</returns>
        private bool PLY_Read_Header(FileStream fs)
        {
            // 检查是否为 PLY 文件
            var line = ReadLine(fs);
            if (line != "ply")
                return false;

            // 解析 PLY 文件存储格式
            line = ReadLine(fs);
            if (!PLY_Read_Header_Format(line))
                return false;

            while (true)
            {
                line = ReadLine(fs);

                // 检查是否能够正常读取 PLY 文件
                if (line == null)
                    return false;

                // 跳过"comment" 和"obj_info"所在行
                if (line.StartsWith("comment") || line.StartsWith("obj_info"))
                    continue;

                // 解析 PLY 文件元素
                if (line.StartsWith("element"))
                {
                    if (!PLY_Read_Header_Element(line))
                        return false;
                    continue;
                }

                // 解析 PLY 文件属性
                if (line.StartsWith("property"))
                {
                    if (!PLY_Read_Header_Property(line))
                        return false;
                    continue;
                }

                // 检查 PLY 文件头已读取完毕
                if (line == "end_header")
                {
                    if (this.format == PLY_Storage_Mode.Undefined)
                        return false;
                    break;
                }

            }
            return true;
        }
        /// <summary>
        /// 解析 PLY 文件存储格式
        /// </summary>
        /// <param name="line">数据</param>
        /// <returns>若成功读取，返回 True；否则返回 False</returns>
        private bool PLY_Read_Header_Format(string line)
        {
            var ts = line.Split(' ');
            // 检查是否有三个字段，且第三个字段为"1.0"
            if (ts.Length != 3 || ts[2] != "1.0")
                return false;
            // 判断 PLY 文件存储格式
            this.format = PLY_Read_Header_Storage_Mode(ts[1]);
            // 检查 PLY 文件格式是否已解析
            if (this.format == PLY_Storage_Mode.Undefined)
                return false;
            else
                return true;
        }
        /// <summary>
        /// 解析 PLY 文件元素类型
        /// </summary>
        /// <param name="line">数据</param>
        /// <returns>若成功读取，返回 True；否则返回 False</returns>
        private bool PLY_Read_Header_Element(string line)
        {
            PLY_Element element = new PLY_Element();

            var ts = line.Split(' ');
            // 检查是否有三个字段
            if (ts.Length != 3)
                return false;
            // 检查第三个字段是否为整数，且是否小于0
            if (!int.TryParse(ts[2], out int count) || count < 0)
                return false;
            element.Count = count;
            element.Name = ts[1];
            switch (ts[1])
            {
                case "cell": element.Type = PLY_Element_Type.Cell; break;
                case "edge": element.Type = PLY_Element_Type.Edge; break;
                case "face": element.Type = PLY_Element_Type.Face; break;
                case "material": element.Type = PLY_Element_Type.Material; break;
                case "vertex": element.Type = PLY_Element_Type.Vertex; break;
            }

            this.elements.Add(element);
            return true;
        }
        /// <summary>
        /// 解析 PLY 文件某元素的属性
        /// </summary>
        /// <param name="line">数据</param>
        /// <returns>若成功读取，返回 True；否则返回 False</returns>
        private bool PLY_Read_Header_Property(string line)
        {
            var position = elements.Count;

            var ts = line.Split(' ');
            if (ts.Length == 3)
            {
                var dataType = PLY_Read_Header_Data_Type(ts[1]);
                if (dataType == PLY_Data_Type.Undefined)
                    return false;

                PLY_Property property = new PLY_Property
                {
                    Type = dataType,
                    Name = ts[2]
                };
                this.elements[position - 1].Properties.Add(property);

            }
            else if (ts.Length == 5 && ts[1] == "list")
            {
                var listCountType = PLY_Read_Header_Data_Type(ts[2]);
                var dataType = PLY_Read_Header_Data_Type(ts[3]);
                if (listCountType == PLY_Data_Type.Undefined || dataType == PLY_Data_Type.Undefined)
                    return false;

                PLY_Property property = new PLY_Property
                {
                    Type = dataType,
                    ListCountType = listCountType,
                    Name = ts[4]
                };
                this.elements[position - 1].Properties.Add(property);
            }
            else
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 读取 ASCII格式 PLY 文件 
        /// </summary>
        /// <param name="fs">文件流</param>
        /// <returns>若成功读取，返回 True；否则返回 False</returns>
        private bool PLY_Read_Ascii(FileStream fs)
        {
            try
            {
                var sr = new StreamReader(fs);
                foreach (PLY_Element element in this.elements)
                {
                    element.Data = CreateDataTableFromElement(element);
                    for (int i = 0; i < element.Count; i++)
                    {
                        var line = sr.ReadLine();
                        if (line == null)
                            return false;
                        var ts = line.Split(' ');
                        var tsIndex = 0UL;

                        var row = element.Data.NewRow();
                        foreach (PLY_Property property in element.Properties)
                        {
                            if (!property.IsListProperty)
                            {
                                switch (property.Type)
                                {
                                    case PLY_Data_Type.Int8: row[property.Name] = SByte.Parse(ts[tsIndex]); break;
                                    case PLY_Data_Type.UInt8: row[property.Name] = Byte.Parse(ts[tsIndex]); break;
                                    case PLY_Data_Type.Int16: row[property.Name] = Int16.Parse(ts[tsIndex]); break;
                                    case PLY_Data_Type.UInt16: row[property.Name] = UInt16.Parse(ts[tsIndex]); break;
                                    case PLY_Data_Type.Int32: row[property.Name] = Int32.Parse(ts[tsIndex]); break;
                                    case PLY_Data_Type.UInt32: row[property.Name] = UInt32.Parse(ts[tsIndex]); break;
                                    case PLY_Data_Type.Int64: row[property.Name] = Int64.Parse(ts[tsIndex]); break;
                                    case PLY_Data_Type.UInt64: row[property.Name] = UInt64.Parse(ts[tsIndex]); break;
                                    case PLY_Data_Type.Float32: row[property.Name] = Single.Parse(ts[tsIndex]); break;
                                    case PLY_Data_Type.Float64: row[property.Name] = Double.Parse(ts[tsIndex]); break;
                                }
                                tsIndex++;
                            }
                            else
                            {
                                var count = 0UL;
                                switch (property.ListCountType)
                                {
                                    case PLY_Data_Type.UInt8: count = Byte.Parse(ts[tsIndex]); break;
                                    case PLY_Data_Type.UInt16: count = Byte.Parse(ts[tsIndex]); break;
                                    case PLY_Data_Type.UInt32: count = Byte.Parse(ts[tsIndex]); break;
                                    case PLY_Data_Type.UInt64: count = Byte.Parse(ts[tsIndex]); break;
                                }
                                switch (property.Type)
                                {
                                    case PLY_Data_Type.Int8:
                                        {
                                            var array = new SByte[count];
                                            for (ulong j = 0; j < count; j++)
                                                array[j] = SByte.Parse(ts[tsIndex + j + 1]);
                                            row[property.Name] = array;
                                        }
                                        break;
                                    case PLY_Data_Type.UInt8:
                                        {
                                            var array = new Byte[count];
                                            for (ulong j = 0; j < count; j++)
                                                array[j] = Byte.Parse(ts[tsIndex + j + 1]);
                                            row[property.Name] = array;
                                        }
                                        break;
                                    case PLY_Data_Type.Int16:
                                        {
                                            var array = new Int16[count];
                                            for (ulong j = 0; j < count; j++)
                                                array[j] = Int16.Parse(ts[tsIndex + j + 1]);
                                            row[property.Name] = array;
                                        }
                                        break;
                                    case PLY_Data_Type.UInt16:
                                        {
                                            var array = new UInt16[count];
                                            for (ulong j = 0; j < count; j++)
                                                array[j] = UInt16.Parse(ts[tsIndex + j + 1]);
                                            row[property.Name] = array;
                                        }
                                        break;
                                    case PLY_Data_Type.Int32:
                                        {
                                            var array = new Int32[count];
                                            for (ulong j = 0; j < count; j++)
                                                array[j] = Int32.Parse(ts[tsIndex + j + 1]);
                                            row[property.Name] = array;
                                        }
                                        break;
                                    case PLY_Data_Type.UInt32:
                                        {
                                            var array = new UInt32[count];
                                            for (ulong j = 0; j < count; j++)
                                                array[j] = UInt32.Parse(ts[tsIndex + j + 1]);
                                            row[property.Name] = array;
                                        }
                                        break;
                                    case PLY_Data_Type.Int64:
                                        {
                                            var array = new Int64[count];
                                            for (ulong j = 0; j < count; j++)
                                                array[j] = Int64.Parse(ts[tsIndex + j + 1]);
                                            row[property.Name] = array;
                                        }
                                        break;
                                    case PLY_Data_Type.UInt64:
                                        {
                                            var array = new UInt64[count];
                                            for (ulong j = 0; j < count; j++)
                                                array[j] = UInt64.Parse(ts[tsIndex + j + 1]);
                                            row[property.Name] = array;
                                        }
                                        break;
                                    case PLY_Data_Type.Float32:
                                        {
                                            var array = new Single[count];
                                            for (ulong j = 0; j < count; j++)
                                                array[j] = Single.Parse(ts[tsIndex + j + 1]);
                                            row[property.Name] = array;
                                        }
                                        break;
                                    case PLY_Data_Type.Float64:
                                        {
                                            var array = new Double[count];
                                            for (ulong j = 0; j < count; j++)
                                                array[j] = Double.Parse(ts[tsIndex + j + 1]);
                                            row[property.Name] = array;
                                        }
                                        break;
                                }
                                tsIndex += count + 1;
                            }
                        }
                        element.Data.Rows.Add(row);
                    }
                }
                sr.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 读取 Binary格式 PLY 文件 
        /// </summary>
        /// <param name="fs">文件流</param>
        /// <returns>若成功读取，返回 True；否则返回 False</returns>
        private bool PLY_Read_Binary(FileStream fs)
        {
            if (BitConverter.IsLittleEndian && format == PLY_Storage_Mode.BIG_ENDIAN)
                return false;
            if (!BitConverter.IsLittleEndian && format == PLY_Storage_Mode.LITTLE_ENDIAN)
                return false;

            try
            {
                var br = new BinaryReader(fs);
                foreach (PLY_Element element in elements)
                {
                    element.Data = CreateDataTableFromElement(element);
                    for (int i = 0; i < element.Count; i++)
                    {
                        var row = element.Data.NewRow();
                        foreach (PLY_Property property in element.Properties)
                        {
                            if (!property.IsListProperty)
                            {
                                switch (property.Type)
                                {
                                    case PLY_Data_Type.Int8: row[property.Name] = br.ReadSByte(); break;
                                    case PLY_Data_Type.UInt8: row[property.Name] = br.ReadByte(); break;
                                    case PLY_Data_Type.Int16: row[property.Name] = br.ReadInt16(); break;
                                    case PLY_Data_Type.UInt16: row[property.Name] = br.ReadUInt16(); break;
                                    case PLY_Data_Type.Int32: row[property.Name] = br.ReadInt32(); break;
                                    case PLY_Data_Type.UInt32: row[property.Name] = br.ReadUInt32(); break;
                                    case PLY_Data_Type.Int64: row[property.Name] = br.ReadInt64(); break;
                                    case PLY_Data_Type.UInt64: row[property.Name] = br.ReadUInt64(); break;
                                    case PLY_Data_Type.Float32: row[property.Name] = br.ReadSingle(); break;
                                    case PLY_Data_Type.Float64: row[property.Name] = br.ReadDouble(); break;
                                }
                            }
                            else
                            {
                                var count = 0UL;
                                switch (property.ListCountType)
                                {
                                    case PLY_Data_Type.UInt8: count = br.ReadByte(); break;
                                    case PLY_Data_Type.UInt16: count = br.ReadUInt16(); break;
                                    case PLY_Data_Type.UInt32: count = br.ReadUInt32(); break;
                                    case PLY_Data_Type.UInt64: count = br.ReadUInt64(); break;
                                }
                                switch (property.Type)
                                {
                                    case PLY_Data_Type.Int8:
                                        {
                                            var array = new SByte[count];
                                            for (ulong j = 0; j < count; j++)
                                                array[j] = br.ReadSByte();
                                            row[property.Name] = array;
                                        }
                                        break;
                                    case PLY_Data_Type.UInt8:
                                        {
                                            var array = new Byte[count];
                                            for (ulong j = 0; j < count; j++)
                                                array[j] = br.ReadByte();
                                            row[property.Name] = array;
                                        }
                                        break;
                                    case PLY_Data_Type.Int16:
                                        {
                                            var array = new Int16[count];
                                            for (ulong j = 0; j < count; j++)
                                                array[j] = br.ReadInt16();
                                            row[property.Name] = array;
                                        }
                                        break;
                                    case PLY_Data_Type.UInt16:
                                        {
                                            var array = new UInt16[count];
                                            for (ulong j = 0; j < count; j++)
                                                array[j] = br.ReadUInt16();
                                            row[property.Name] = array;
                                        }
                                        break;
                                    case PLY_Data_Type.Int32:
                                        {
                                            var array = new Int32[count];
                                            for (ulong j = 0; j < count; j++)
                                                array[j] = br.ReadInt32();
                                            row[property.Name] = array;
                                        }
                                        break;
                                    case PLY_Data_Type.UInt32:
                                        {
                                            var array = new UInt32[count];
                                            for (ulong j = 0; j < count; j++)
                                                array[j] = br.ReadUInt32();
                                            row[property.Name] = array;
                                        }
                                        break;
                                    case PLY_Data_Type.Int64:
                                        {
                                            var array = new Int64[count];
                                            for (ulong j = 0; j < count; j++)
                                                array[j] = br.ReadInt64();
                                            row[property.Name] = array;
                                        }
                                        break;
                                    case PLY_Data_Type.UInt64:
                                        {
                                            var array = new UInt64[count];
                                            for (ulong j = 0; j < count; j++)
                                                array[j] = br.ReadUInt64();
                                            row[property.Name] = array;
                                        }
                                        break;
                                    case PLY_Data_Type.Float32:
                                        {
                                            var array = new Single[count];
                                            for (ulong j = 0; j < count; j++)
                                                array[j] = br.ReadSingle();
                                            row[property.Name] = array;
                                        }
                                        break;
                                    case PLY_Data_Type.Float64:
                                        {
                                            var array = new Double[count];
                                            for (ulong j = 0; j < count; j++)
                                                array[j] = br.ReadDouble();
                                            row[property.Name] = array;
                                        }
                                        break;
                                }
                            }
                        }
                        element.Data.Rows.Add(row);
                    }
                }
                br.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 写入 PLY 文件
        /// </summary>
        /// <param name="fileName">PLY 文件绝对路径</param>
        /// <returns>若成功写入，返回 True；否则返回 False</returns>
        public bool PLY_Write(string fileName)
        {
            if (this.elements.Count == 0 || this.format == PLY_Storage_Mode.Undefined)
            {
                return false;
            }

            try
            {
                if (this.format == PLY_Storage_Mode.ASCII)
                    return PLY_Write_Ascii(fileName);
                else
                    return PLY_Write_Binary(fileName);
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 写入 ASCII 格式 PLY 文件
        /// </summary>
        /// <param name="fileName">PLY 文件绝对路径</param>
        /// <returns>成功写入，返回 True;否则，返回 False</returns>
        private bool PLY_Write_Ascii(string fileName)
        {
            try
            {
                StreamWriter sw = new StreamWriter(fileName);
                // 写入 PLY 文件头
                sw.WriteLine("ply");
                sw.WriteLine("format ascii 1.0");
                foreach (PLY_Element element in elements)
                {
                    sw.WriteLine($"element {element.Name} {element.Count}");
                    foreach (PLY_Property property in element.Properties)
                    {
                        if (!property.IsListProperty)
                            sw.WriteLine($"property {PLY_Write_Header_Data_Type(property.Type)} {property.Name}");
                        else
                            sw.WriteLine($"property list {PLY_Write_Header_Data_Type(property.ListCountType)} {PLY_Write_Header_Data_Type(property.Type)} {property.Name}");
                    }
                }
                sw.WriteLine("end_header");
                // 写入 PLY 文件数据
                foreach (PLY_Element element in elements)
                {
                    for (int i = 0; i < element.Count; i++)
                    {
                        var row = element.Data.Rows[i];
                        StringBuilder sb = new StringBuilder();
                        foreach (PLY_Property property in element.Properties)
                        {
                            if (!property.IsListProperty)
                            {
                                sb.Append(row[property.Name].ToString() + " ");
                            }
                            else
                            {
                                var array = (Array)row[property.Name];
                                sb.Append(array.Length.ToString() + " ");
                                foreach (Object value in array)
                                    sb.Append(value.ToString() + " ");
                            }
                        }
                        sw.WriteLine(sb.ToString());
                    }
                }
                sw.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 写入 Binaey 格式 PLY 文件
        /// </summary>
        /// <param name="fileName">PLY 文件绝对路径</param>
        /// <returns>成功写入，返回 True;否则，返回 False</returns>
        private bool PLY_Write_Binary(string fileName)
        {
            if (BitConverter.IsLittleEndian && format == PLY_Storage_Mode.BIG_ENDIAN)
                return false;
            if (!BitConverter.IsLittleEndian && format == PLY_Storage_Mode.LITTLE_ENDIAN)
                return false;

            try
            {
                BinaryWriter bw = new BinaryWriter(new FileStream(fileName, FileMode.Create));
                // 写入 PLY 文件头
                bw.Write("ply\n".ToArray());
                bw.Write($"format {PLY_Write_Header_Storage_Mode(this.format)} 1.0\n".ToArray());
                foreach (PLY_Element element in elements)
                {
                    bw.Write($"element {element.Name} {element.Count}\n".ToArray());
                    foreach (PLY_Property property in element.Properties)
                    {
                        if (!property.IsListProperty)
                            bw.Write($"property {PLY_Write_Header_Data_Type(property.Type)} {property.Name}\n".ToArray());
                        else
                            bw.Write($"property list {PLY_Write_Header_Data_Type(property.ListCountType)} {PLY_Write_Header_Data_Type(property.Type)} {property.Name}\n".ToArray());
                    }
                }
                bw.Write("end_header\n".ToArray());
                // 写入 PLY 文件数据
                foreach (PLY_Element element in elements)
                {
                    for (int i = 0; i < element.Count; i++)
                    {
                        var row = element.Data.Rows[i];
                        foreach (PLY_Property property in element.Properties)
                        {
                            if (!property.IsListProperty)
                            {
                                switch (property.Type)
                                {
                                    case PLY_Data_Type.Int8: bw.Write((SByte)row[property.Name]); break;
                                    case PLY_Data_Type.UInt8: bw.Write((Byte)row[property.Name]); break;
                                    case PLY_Data_Type.Int16: bw.Write((Int16)row[property.Name]); break;
                                    case PLY_Data_Type.UInt16: bw.Write((UInt16)row[property.Name]); break;
                                    case PLY_Data_Type.Int32: bw.Write((Int32)row[property.Name]); break;
                                    case PLY_Data_Type.UInt32: bw.Write((UInt32)row[property.Name]); break;
                                    case PLY_Data_Type.Int64: bw.Write((Int64)row[property.Name]); break;
                                    case PLY_Data_Type.UInt64: bw.Write((UInt64)row[property.Name]); break;
                                    case PLY_Data_Type.Float32: bw.Write((Single)row[property.Name]); break;
                                    case PLY_Data_Type.Float64: bw.Write((Double)row[property.Name]); break;
                                }
                            }
                            else
                            {
                                var array = (Array)row[property.Name];
                                switch (property.ListCountType)
                                {
                                    case PLY_Data_Type.UInt8: bw.Write((Byte)array.Length); break;
                                    case PLY_Data_Type.UInt16: bw.Write((UInt16)array.Length); break;
                                    case PLY_Data_Type.UInt32: bw.Write((UInt32)array.Length); break;
                                    case PLY_Data_Type.UInt64: bw.Write((UInt64)array.Length); break;
                                }
                                switch (property.Type)
                                {
                                    case PLY_Data_Type.Int8:
                                        foreach (SByte value in array)
                                            bw.Write(value);
                                        break;
                                    case PLY_Data_Type.UInt8:
                                        foreach (Byte value in array)
                                            bw.Write(value);
                                        break;
                                    case PLY_Data_Type.Int16:
                                        foreach (Int16 value in array)
                                            bw.Write(value);
                                        break;
                                    case PLY_Data_Type.UInt16:
                                        foreach (UInt16 value in array)
                                            bw.Write(value);
                                        break;
                                    case PLY_Data_Type.Int32:
                                        foreach (Int32 value in array)
                                            bw.Write(value);
                                        break;
                                    case PLY_Data_Type.UInt32:
                                        foreach (UInt32 value in array)
                                            bw.Write(value);
                                        break;
                                    case PLY_Data_Type.Int64:
                                        foreach (Int64 value in array)
                                            bw.Write(value);
                                        break;
                                    case PLY_Data_Type.UInt64:
                                        foreach (UInt64 value in array)
                                            bw.Write(value);
                                        break;
                                    case PLY_Data_Type.Float32:
                                        foreach (Single value in array)
                                            bw.Write(value);
                                        break;
                                    case PLY_Data_Type.Float64:
                                        foreach (Double value in array)
                                            bw.Write(value);
                                        break;
                                }
                            }
                        }
                    }
                }
                bw.Close();
                return true;
            }
            catch
            {
                return false;
            }

        }

        /// <summary>
        /// PLY 文件的元素
        /// </summary>
        public List<PLY_Element> Elements => elements;
        /// <summary>
        /// 修改 PLY 文件存储格式
        /// </summary>
        public PLY_Storage_Mode Format { set { this.format = value; } }
        /// <summary>
        /// 添加 PLY 文件元素
        /// </summary>
        /// <param name="element">元素</param>
        public void AddElements(PLY_Element element) => this.Elements.Add(element);

        /// <summary>
        /// 从 PLY 元素创建数据表
        /// </summary>
        /// <param name="element"> PLY 元素</param>
        /// <returns>数据表</returns>
        private static DataTable CreateDataTableFromElement(PLY_Element element)
        {
            var table = new DataTable(element.Name);
            for (int i = 0; i < element.Properties.Count; i++)
            {
                var column = new DataColumn();
                column.ColumnName = element.Properties[i].Name;
                if (element.Properties[i].IsListProperty)
                {
                    switch (element.Properties[i].Type)
                    {
                        case PLY_Data_Type.Int8: column.DataType = Type.GetType("System.SByte[]"); break;
                        case PLY_Data_Type.UInt8: column.DataType = Type.GetType("System.Byte[]"); break;
                        case PLY_Data_Type.Int16: column.DataType = Type.GetType("System.Int16[]"); break;
                        case PLY_Data_Type.UInt16: column.DataType = Type.GetType("System.UInt16[]"); break;
                        case PLY_Data_Type.Int32: column.DataType = Type.GetType("System.Int32[]"); break;
                        case PLY_Data_Type.UInt32: column.DataType = Type.GetType("System.UInt32[]"); break;
                        case PLY_Data_Type.Int64: column.DataType = Type.GetType("System.Int64[]"); break;
                        case PLY_Data_Type.UInt64: column.DataType = Type.GetType("System.UInt64[]"); break;
                        case PLY_Data_Type.Float32: column.DataType = Type.GetType("System.Single[]"); break;
                        case PLY_Data_Type.Float64: column.DataType = Type.GetType("System.Double[]"); break;
                    }
                }
                else
                {
                    switch (element.Properties[i].Type)
                    {
                        case PLY_Data_Type.Int8: column.DataType = Type.GetType("System.SByte"); break;
                        case PLY_Data_Type.UInt8: column.DataType = Type.GetType("System.Byte"); break;
                        case PLY_Data_Type.Int16: column.DataType = Type.GetType("System.Int16"); break;
                        case PLY_Data_Type.UInt16: column.DataType = Type.GetType("System.UInt16"); break;
                        case PLY_Data_Type.Int32: column.DataType = Type.GetType("System.Int32"); break;
                        case PLY_Data_Type.UInt32: column.DataType = Type.GetType("System.UInt32"); break;
                        case PLY_Data_Type.Int64: column.DataType = Type.GetType("System.Int64"); break;
                        case PLY_Data_Type.UInt64: column.DataType = Type.GetType("System.UInt64"); break;
                        case PLY_Data_Type.Float32: column.DataType = Type.GetType("System.Single"); break;
                        case PLY_Data_Type.Float64: column.DataType = Type.GetType("System.Double"); break;
                    }
                }
                table.Columns.Add(column);
            }
            return table;
        }
        /// <summary>
        /// 从文件流中读取一行数据
        /// </summary>
        /// <param name="fs">文件流</param>
        /// <returns>读取的一行数据</returns>
        private static string ReadLine(FileStream fs)
        {
            var sb = new StringBuilder();
            var data = fs.ReadByte();

            while (data == '\n' || data == '\r')
                data = fs.ReadByte();

            while (data != -1 && data != '\n' && data != '\r')
            {
                sb.Append((char)data);
                data = fs.ReadByte();
            }

            return sb.ToString();
        }
        /// <summary>
        /// 将 string 类型转为 PLY_Data_Type 类型
        /// </summary>
        /// <param name="type">数据类型</param>
        /// <returns>若成功转换，返回对应的类型；否则返回 未定义类型</returns>
        private static PLY_Data_Type PLY_Read_Header_Data_Type(string type)
        {
            return type switch
            {
                "char" => PLY_Data_Type.Int8,
                "uchar" => PLY_Data_Type.UInt8,
                "short" => PLY_Data_Type.Int16,
                "ushort" => PLY_Data_Type.UInt16,
                "int" => PLY_Data_Type.Int32,
                "uint" => PLY_Data_Type.UInt32,
                "long" => PLY_Data_Type.Int64,
                "ulong" => PLY_Data_Type.UInt64,
                "float" => PLY_Data_Type.Float32,
                "double" => PLY_Data_Type.Float64,
                "int8" => PLY_Data_Type.Int8,
                "uint8" => PLY_Data_Type.UInt8,
                "int16" => PLY_Data_Type.Int16,
                "uint16" => PLY_Data_Type.UInt16,
                "int32" => PLY_Data_Type.Int32,
                "uint32" => PLY_Data_Type.UInt32,
                "int64" => PLY_Data_Type.Int64,
                "uint64" => PLY_Data_Type.UInt64,
                "float32" => PLY_Data_Type.Float32,
                "float64" => PLY_Data_Type.Float64,
                "sbyte" => PLY_Data_Type.Int8,
                "ubyte" => PLY_Data_Type.UInt8,
                _ => PLY_Data_Type.Undefined
            };
        }
        /// <summary>
        /// 将 PLY_Data_Type 类型转为 string 类型
        /// </summary>
        /// <param name="type">数据类型</param>
        /// <returns>若成功转换，返回对应的字符串；否则返回空字符串</returns>
        private static string PLY_Write_Header_Data_Type(PLY_Data_Type type)
        {
            return type switch
            {
                PLY_Data_Type.Int8 => "char",
                PLY_Data_Type.UInt8 => "uchar",
                PLY_Data_Type.Int16 => "short",
                PLY_Data_Type.UInt16 => "ushort",
                PLY_Data_Type.Int32 => "int",
                PLY_Data_Type.UInt32 => "uint",
                PLY_Data_Type.Int64 => "long",
                PLY_Data_Type.UInt64 => "ulong",
                PLY_Data_Type.Float32 => "float",
                PLY_Data_Type.Float64 => "double",
                _ => ""
            };
        }
        /// <summary>
        /// 将 string 类型转为 PLY_Stroage_Mode 类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>若成功转换，返回对应的类型；否则返回未定义类型</returns>
        private static PLY_Storage_Mode PLY_Read_Header_Storage_Mode(string type)
        {
            return type switch
            {
                "ascii" => PLY_Storage_Mode.ASCII,
                "binary_little_endian" => PLY_Storage_Mode.LITTLE_ENDIAN,
                "binary_big_endian" => PLY_Storage_Mode.BIG_ENDIAN,
                _ => PLY_Storage_Mode.Undefined
            };
        }
        /// <summary>
        /// 将 PLY_Stroage_Mode 类型转为 string 类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>若成功转换，返回对应的字符串；否则返回空字符串</returns>
        private static string PLY_Write_Header_Storage_Mode(PLY_Storage_Mode type)
        {
            return type switch
            {
                PLY_Storage_Mode.ASCII => "ascii",
                PLY_Storage_Mode.LITTLE_ENDIAN => "binary_little_endian",
                PLY_Storage_Mode.BIG_ENDIAN => "binary_big_endian",
                _ => ""
            };
        }
    }
}
