using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SharpDX;

namespace DromeEd.Drome
{
    public static class IOExtensions
    {
        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        public static void Write(this BinaryWriter writer, Vector2 v)
        {
            writer.Write(v.X);
            writer.Write(v.Y);
        }

        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static void Write(this BinaryWriter writer, Vector3 v)
        {
            writer.Write(v.X);
            writer.Write(v.Y);
            writer.Write(v.Z);
        }

        public static Vector4 ReadVector4(this BinaryReader reader)
        {
            return new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static void Write(this BinaryWriter writer, Vector4 v)
        {
            writer.Write(v.X);
            writer.Write(v.Y);
            writer.Write(v.Z);
            writer.Write(v.W);
        }

        public static Quaternion ReadQuaternion(this BinaryReader reader)
        {
            return new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static void Write(this BinaryWriter writer, Quaternion q)
        {
            writer.Write(q.X);
            writer.Write(q.Y);
            writer.Write(q.Z);
            writer.Write(q.W);
        }

        /// <summary>
        /// Reads a string stored with the given maximum size.
        /// </summary>
        /// <param name="reader">The BinaryReader to read from.</param>
        /// <param name="size">The number of bytes to read.</param>
        /// <returns></returns>
        public static string ReadStringN(this BinaryReader reader, int size)
        {
            byte[] bytes = reader.ReadBytes(size);
            string str = Encoding.ASCII.GetString(bytes);
            return str.Substring(0, str.IndexOf('\0'));
        }

        private const int String24Size = 24;
        /// <summary>
        /// Reads a String24 from a BinaryReader.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static string ReadString24(this BinaryReader reader)
        {
            return reader.ReadStringN(String24Size);
        }

        private const int FileNameSize = 128;
        /// <summary>
        /// Reads a cFileName from a BinaryReader.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static string ReadStringFileName(this BinaryReader reader)
        {
            return reader.ReadStringN(FileNameSize);
        }
    }
}
