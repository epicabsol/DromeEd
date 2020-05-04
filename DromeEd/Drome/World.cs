using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DromeEd.Drome
{
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class ObjectTypeAttribute : Attribute
    {
        public string TypeName { get; }

        public ObjectTypeAttribute(string typeName)
        {
            TypeName = typeName;
        }
    }

    /// <summary>
    /// Uses Reflection to enumerate all loadable WRL cObject classes.
    /// </summary>
    public static class ClassRegistry
    {
        private class ObjectClass
        {
            public Type Type = null;
            public string Name = "";

            public ObjectClass(Type type, string name)
            {
                Type = type;
                Name = name;
            }
        }

        private static List<ObjectClass> Classes = new List<ObjectClass>();

        public static void Initialize()
        {
            foreach (System.Reflection.TypeInfo t in System.Reflection.Assembly.GetAssembly(typeof(ClassRegistry)).DefinedTypes)
            {
                foreach (ObjectTypeAttribute att in t.GetCustomAttributes(typeof(ObjectTypeAttribute), false))
                {
                    ObjectClass c = new ObjectClass(t.UnderlyingSystemType, att.TypeName);
                    Classes.Add(c);
                    System.Diagnostics.Debug.WriteLine("Found native object for class '" + c.Name + "'");
                    break;
                }
            }
        }

        public static Object MakeInstance(ObjectHeader header, BinaryReader reader)
        {
            foreach (ObjectClass c in Classes)
            {
                if (c.Name == header.ClassName)
                    return Activator.CreateInstance(c.Type, header, reader) as Object;
            }
            System.Diagnostics.Debug.WriteLine("[WARNING]: Class '" + header.ClassName + "' is not in the class registry!");
            return new Object(header, reader);
        }
    }

    public class ObjectHeader
    {
        public const string MAGIC_OBJECT = "OBMG";

        public string Magic { get; }    // 0x00 + 0x04
        public string ClassName { get; set; }    // 0x04 + 0x18
        public uint Version { get; set; }    // 0x1C + 0x04
        public uint Size { get; set; }       // 0x20 + 0x04

        public ObjectHeader(BinaryReader reader)
        {
            Magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
            if (Magic != MAGIC_OBJECT)
                throw new FormatException("Invalid cObject magic: '" + Magic + "' is not '" + MAGIC_OBJECT + "'.");
            ClassName = Encoding.ASCII.GetString(reader.ReadBytes(0x18)).TrimEnd((char)0);
            Version = reader.ReadUInt32();
            Size = reader.ReadUInt32();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Encoding.ASCII.GetBytes(Magic.PadRight(4, (char)0)));
            writer.Write(Encoding.ASCII.GetBytes(ClassName.PadRight(4, (char)0)));
            writer.Write(Version);
            writer.Write(Size);
        }
    }

    [ObjectType("cObject")]
    public class Object
    {
        // cObject Header: 0x00 + 0x24
        public ObjectHeader Header { get; }

        // sInitCObject: 0x24 + 0x34
        public uint StateFlags { get; } // 0x24 + 0x04
        public string InstanceName { get; set; } // 0x28 + 0x18
        public string ParentInstanceName { get; set; } // 0x40 + 0x18

        public Object(ObjectHeader header, BinaryReader reader)
        {
            Header = header;
            StateFlags = reader.ReadUInt32();
            InstanceName = Encoding.ASCII.GetString(reader.ReadBytes(0x18)).TrimEnd((char)0);
            ParentInstanceName = Encoding.ASCII.GetString(reader.ReadBytes(0x18)).TrimEnd((char)0);
        }
    }

    public class World
    {
        public const string MAGIC_WORLD = "RC2W";
        public const int WORLD_VERSION_DROME = 0x0E;

        public string Magic { get; } = MAGIC_WORLD;
        public uint Version { get; } = WORLD_VERSION_DROME;
        public List<Object> Objects { get; } = new List<Object>();

        public World(BinaryReader reader)
        {
            Magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
            if (Magic != MAGIC_WORLD)
                throw new FormatException("World file's magic is incorrect.");
            Version = reader.ReadUInt32();
            if (Version > WORLD_VERSION_DROME)
                throw new FormatException("World version is newer than the Drome worlds!");

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                ObjectHeader header = new ObjectHeader(reader);
                long position = reader.BaseStream.Position;

                // TODO: Instantiate object
                //Object obj = new Object(header, reader);
                Object obj = ClassRegistry.MakeInstance(header, reader);
                Objects.Add(obj);

                reader.BaseStream.Position = position + header.Size;
            }
        }
    }
}
