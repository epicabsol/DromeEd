using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ATD.VFS
{
    public class FileEntry
    {
        public string Filename;
        public uint Offset;
        public uint Size;

        public FileEntry(string filename, uint offset, uint size)
        {
            Filename = filename;
            Offset = offset;
            Size = size;
        }

        public FileEntry(BinaryReader reader)
        {
            Filename = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(128)).TrimEnd(new char[] { (char)0 });
            Offset = reader.ReadUInt32();
            Size = reader.ReadUInt32();
        }

        public override string ToString()
        {
            return Offset.ToString("X8") + "+" + Size.ToString("X8") + " \"" + Filename + "\"";
        }
    }

    public class BlockEntry
    {
        public int Offset;
        public int CompressedSize;
        public int PaddedSize;
        public int Overlap; // Possibly compression ratio
        public int Files; // Possibly number of files in the block

        public BlockEntry(int offset, int compressedSize, int paddedSize)
        {
            Offset = offset;
            CompressedSize = compressedSize;
            PaddedSize = paddedSize;
        }

        public BlockEntry(BinaryReader reader, uint version)
        {
            Offset = reader.ReadInt32();
            CompressedSize = reader.ReadInt32();
            PaddedSize = reader.ReadInt32();
            if (version == 1)
            {
                Overlap = reader.ReadInt32();
                Files = reader.ReadInt32();
            }
        }

        public void Write(BinaryWriter writer, uint version)
        {
            writer.Write(Offset);
            writer.Write(CompressedSize);
            writer.Write(PaddedSize);
            if (version == 1)
            {
                writer.Write(Overlap);
                writer.Write(Files);
            }
        }

        public override string ToString()
        {
            return CompressedSize.ToString("X8") + ": " + Offset.ToString("X8") + " + " + PaddedSize.ToString("X8");
        }
    }

    public class Filesystem : IDisposable
    {
        public Dictionary<string, FileEntry> Files = new Dictionary<string, FileEntry>();
        public List<BlockEntry> Blocks = new List<BlockEntry>();

        private MemoryStream GameData = null;

        private const uint BlockSizeUncompressed = 0x20000;

        public uint CompressionVersion { get; private set; }

        public event EventHandler<float> ProgressChangedEvent = null;
        public event EventHandler<string> LogMessageEvent = null;

        public Filesystem()
        {

        }

        private volatile bool _cancel;
        private bool IsCancelled => _cancel;
        public void Cancel()
        {
            _cancel = true;
        }

        private void Log(string message)
        {
            try
            {
                LogMessageEvent?.Invoke(this, message);
            }
            catch { }
        }

        private void ReportProgress(float progress)
        {
            try
            {
                ProgressChangedEvent?.Invoke(this, progress);
            }
            catch { }
        }

        public void LoadArchive(string directory)
        {
            _cancel = false;
            // Load file entries
            Log("Loading file entries...");
            using (FileStream stream = new FileStream(directory + "\\FileList.inf", FileMode.Open))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                while (stream.Position < stream.Length)
                {
                    if (IsCancelled) throw new TaskCanceledException();
                    FileEntry file = new FileEntry(reader);
                    Files.Add(file.Filename.ToLower(), file);
                }
            }

            // Load block entries
            Log("Loading block entries...");
            using (FileStream stream = new FileStream(directory + "\\Compress.inf", FileMode.Open))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                // Detect the version of compression by reading the length of the first block and then looking for the same value
                // as the start offset of the next block
                stream.Seek(0x08, SeekOrigin.Begin);
                uint len1 = reader.ReadUInt32(); // Read the length of the first block
                stream.Seek(0x0C, SeekOrigin.Begin);
                uint offsetv2 = reader.ReadUInt32(); // Read the offset of the second block if version 2
                stream.Seek(0x14, SeekOrigin.Begin);
                uint offsetv1 = reader.ReadUInt32(); // Read the offset of the second block if version 1

                if (len1 == offsetv1)
                    CompressionVersion = 1;
                else if (len1 == offsetv2)
                    CompressionVersion = 2;
                else
                    throw new Exception("Couldn't determine compression version! len1 = " + len1 + ", offsetv1 = " + offsetv1 + ", offsetv2 = " + offsetv2);

                stream.Seek(0x00, SeekOrigin.Begin);
                while (stream.Position < stream.Length)
                {
                    if (IsCancelled) throw new TaskCanceledException();
                    Blocks.Add(new BlockEntry(reader, CompressionVersion));
                }
            }

            GameData = new MemoryStream((int)BlockSizeUncompressed * Blocks.Count);
            GameData.SetLength((int)BlockSizeUncompressed * Blocks.Count);

            // Uncompress the blocks from the GTC into memory
            Log("Loading archive...");
            using (FileStream stream = new FileStream(directory + "\\GameData.gtc", FileMode.Open))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                for (int i = 0; i < Blocks.Count; i++)
                {
                    if (IsCancelled) throw new TaskCanceledException();
                    ReportProgress(i / (float)Blocks.Count);
                    BlockEntry block = Blocks[i];
                    int end = (int)(block.Offset + block.PaddedSize);
                    GameData.Position = i * BlockSizeUncompressed;

                    stream.Seek(end - block.CompressedSize, SeekOrigin.Begin);

                    if (block.PaddedSize == BlockSizeUncompressed)
                    {
                        GameData.Write(reader.ReadBytes((int)BlockSizeUncompressed), 0, (int)BlockSizeUncompressed);
                        //Console.WriteLine("[UNCOMPRESSED]");
                    }
                    else
                    {


                        bool back_ref = false;
                        uint single = 0;
                        uint back_ref_size = 0;
                        int back_ref_jump = 0;

                        do
                        {
                            //Copy the command bits and move past.
                            uint command = reader.ReadUInt32();

                            //Read the bits from right to left, but only until the entire block or file is read.
                            for (uint j = 0; j < 32 && stream.Position < end && GameData.Position < GameData.Length; ++j)
                            {
                                //Get the boolean value of the bit.
                                bool bit = (command >> (int)j & 1) > 0;

                                //Check if already in a backref.
                                if (back_ref)
                                {
                                    //Check if we have detected what kind of backref yet.
                                    if (single > 0)
                                    {
                                        //We are in a single byte backref, we need to read the size.

                                        //Add the value of the bit if set.
                                        if (bit)
                                        {
                                            //This addition logic only works because we never go above 2 bits
                                            back_ref_size += single;
                                        }

                                        //If we have read the last bit, copy the bytes and exit the backref.
                                        if (!(--single > 0))
                                        {
                                            //We know how big the backref is now, copy the data from the decompressed data from the specified backref jump.
                                            for (uint k = 0; k < back_ref_size; ++k)
                                            {
                                                long pos = GameData.Position;
                                                GameData.Position += back_ref_jump;
                                                byte b = (byte)GameData.ReadByte();
                                                GameData.Position = pos;
                                                GameData.WriteByte(b);
                                                //gamedata.posi += copy_data(&gamedata, &gamedata, 1, back_ref_jump);
                                            }

                                            back_ref = false;
                                        }
                                    }
                                    else
                                    {
                                        //We have not yet determined what kind of backref this is.
                                        if (bit)
                                        {
                                            //A self-contained multi-byte, read it now and continue on.

                                            //Read the first 3 bits.
                                            byte b = (byte)reader.ReadByte();
                                            byte b2 = (byte)reader.ReadByte();

                                            back_ref_size = (uint)b & 0x07;

                                            //Read the block offset.
                                            back_ref_jump = -8192 + (((b & 0xF8) >> 3) | (b2 << 5));

                                            //gamedata_gtc.posi += 2;

                                            //If all the bits are 0, this is a 3 byte block, else it is a 2 byte block.
                                            if (!(back_ref_size > 0))
                                            {
                                                byte b3 = reader.ReadByte();
                                                //Read the 7 bits that are the backref block size.
                                                back_ref_size = (uint)b3 & 0x7F;

                                                //Check the last 1 bit to see if the backref starts 2x further back.
                                                if (((uint)b3 & 0x80) > 0)
                                                {
                                                    back_ref_jump -= 8192;
                                                }

                                                //gamedata_gtc.posi++;

                                                //If the backref size is still 0, read the next 2 bytes to get the size.
                                                if (!(back_ref_size > 0))
                                                {
                                                    back_ref_size = reader.ReadUInt16();
                                                    /*back_ref_size = (((byte)gamedata_gtc.data[gamedata_gtc.posi]))

                                                                    | (((byte)gamedata_gtc.data[gamedata_gtc.posi + 1]) << 8)
                                                                    ;
                                                    gamedata_gtc.posi += 2;*/
                                                }
                                                else
                                                {
                                                    //The minimum size of a backref is 2, so add 2 to the total.
                                                    back_ref_size += 2;
                                                }
                                            }
                                            else
                                            {
                                                //The minimum size of a backref is 2, so add 2 to the total.
                                                back_ref_size += 2;
                                            }

                                            //Copy the data and more ahead.
                                            for (uint k = 0; k < back_ref_size; ++k)
                                            {
                                                long offset = GameData.Position;
                                                GameData.Position += back_ref_jump;
                                                byte b4 = (byte)GameData.ReadByte();
                                                GameData.Position = offset;
                                                GameData.WriteByte(b4);
                                                //gamedata.posi += copy_data(&gamedata, &gamedata, 1, back_ref_jump);
                                            }

                                            back_ref = false;
                                        }
                                        else
                                        {
                                            //This is a single byte backref, we need to read the next 2 bits.
                                            single = 2;
                                            //The minimum single backref size is 2.
                                            back_ref_size = 2;

                                            //We do not know how large the backref block is yet, but we must read where, in case the size runs into the next command block.
                                            back_ref_jump = -256 + (reader.ReadByte());

                                            //gamedata_gtc.posi++;
                                        }
                                    }
                                }
                                else
                                {
                                    //Not currently in a backref, check if literal or declaring a backref.
                                    if (bit)
                                    {
                                        //Backref, setup to read what kind from the next bit(s).
                                        back_ref = true;
                                        single = 0;
                                    }
                                    else
                                    {
                                        //Literal byte, copy it.
                                        GameData.WriteByte(reader.ReadByte());
                                        //gamedata.posi += copy_data(&gamedata_gtc, &gamedata, 1);
                                        //gamedata_gtc.posi++;
                                    }
                                }
                            }
                        }
                        while (stream.Position < end);



                        //Console.WriteLine("[COMPRESSED] -> UNCOMPRESSED");
                    }
                }
            }
        }

        public void LoadRaw(string directory, int version)
        {
            this.GameData = new MemoryStream();
            Stack<string> recursePaths = new Stack<string>();
            recursePaths.Push(Path.Combine(directory, "Game Data"));

            while (recursePaths.Count > 0)
            {
                string dir = recursePaths.Pop();
                foreach (string filename in System.IO.Directory.EnumerateFiles(dir))
                {
                    long start = GameData.Length;
                    using (System.IO.FileStream stream = new FileStream(filename, FileMode.Open))
                    {
                        stream.CopyTo(GameData);
                    }
                    string relativePath = filename.Substring(directory.Length + 1).ToLower();
                    this.Files.Add(relativePath, new FileEntry(relativePath, (uint)start, (uint)(GameData.Length - start)));
                }
                foreach (string subdir in System.IO.Directory.EnumerateDirectories(dir))
                {
                    recursePaths.Push(subdir);
                }
            }
        }

        public void WriteArchive(string directory, bool compressed)
        {

        }

        public void WriteRaw(string directory)
        {

        }

        public void Dispose()
        {
            GameData?.Dispose();
            GameData = null;
        }

        public FileEntry GetFileEntry(string filename)
        {
            if (Files.ContainsKey(filename.ToLower()))
                return Files[filename.ToLower()];
            else
                return null;
        }

        public byte[] GetFileData(FileEntry file)
        {
            GameData.Position = file.Offset;
            byte[] result = new byte[file.Size];
            GameData.Read(result, 0, (int)file.Size);
            return result;
        }

        public IEnumerable<FileEntry> FileEntries { get => Files.Values; }
    }
}
