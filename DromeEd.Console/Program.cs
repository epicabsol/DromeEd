using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATD.VFS;
using DromeEd.Drome;
using System.Diagnostics;

namespace DromeEd.Console
{
    class Program
    {
        public const string BaseDir = @"C:\Users\Admin\Desktop\Drome Racers\LEGO Drome Racers";

        public const string modelFile = @"GAME DATA\GRAPHICS\CARS\MODELS\TECHNIC OFF ROAD\8469\OFF_8469OPTIM.MD2";
        public const string texFile = @"GAME DATA\GRAPHICS\MENUS\PITS\CHARACTERS\SHICANE\TEXTURES\WRENCH0 COPY.PC TEXTURE";

        static void LR2ModelExport(System.IO.StreamWriter log)
        {
            Context.Current = new Context(Context.NextGenGame.LegoRacers2, "");

            System.Windows.Forms.OpenFileDialog odialog = new System.Windows.Forms.OpenFileDialog();
            odialog.Filter = "MD2 Model (*.MD2)|*.MD2";
            if (odialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(odialog.FileName, System.IO.FileMode.Open))
                using (System.IO.BinaryReader reader = new System.IO.BinaryReader(fs))
                {
                    MD2File model = new MD2File(reader);

                    foreach (Block b in model.Blocks.Values)
                    {
                        Debug.WriteLine(b.Header.Signature + ": " + b.Header.Length.ToString("X8") + " bytes");
                    }

                    //System.Console.ReadLine();

                    System.Windows.Forms.SaveFileDialog sdialog = new System.Windows.Forms.SaveFileDialog();
                    sdialog.Filter = "Wavefront OBJ (*.obj)|*.obj";
                    if (sdialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        model.ExportOBJ(sdialog.FileName);
                    }
                }
            }
        }

        [STAThread()]
        static void Main(string[] args)
        {
            System.IO.StreamWriter log = new System.IO.StreamWriter("log.txt", false);

            /*LR2ModelExport(log);
            log.Dispose();
            return;*/

            Filesystem fs = new Filesystem();
            fs.LoadArchive(BaseDir);

            /*foreach (FileEntry file in fs.Files.Values)
            {
                log.WriteLine(file);
            }
            log.WriteLine();
            foreach (BlockEntry block in fs.Blocks)
            {
                log.WriteLine(block);
            }*/

            //System.Console.WriteLine("Ready to parse MD2.");
            //System.Console.ReadKey();

            /*FileEntry entry = fs.GetFileEntry(texFile);

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(fs.GetFileData(entry)))
            using (System.IO.BinaryReader reader = new System.IO.BinaryReader(ms))
            {
                ///MD2File model = new MD2File(reader);
                //(model.Blocks["GEO2"] as GEO2Block).ExportOBJ("export.obj");


                Texture texture = new Texture(reader);
                string path = System.IO.Path.ChangeExtension("dump\\" + entry.Filename.ToLower(), ".tga");
                string directory = System.IO.Path.GetDirectoryName(path);
                System.IO.Directory.CreateDirectory(directory);
                if (texture.PCTFormat != Texture.ImageFormat.DXT3)
                {
                    texture.DumpTGA(path);
                    System.Console.WriteLine("Converted " + entry.Filename);
                }
                else
                    System.Console.WriteLine("Skipping DXT3");
                //(model.Blocks["GEO2"] as GEO2Block).ExportOBJ(path);

                //log.WriteLine(texture.ToString());
            }*/

            ClassRegistry.Initialize();

            foreach (FileEntry e in fs.FileEntries)
            {
                /*if (System.IO.Path.GetExtension(e.Filename).ToLower() == ".md2")
                {
                    System.Console.WriteLine("Exporting model \"" + e.Filename + "\"");
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream(fs.GetFileData(e)))
                    using (System.IO.BinaryReader reader = new System.IO.BinaryReader(ms))
                    {
                        MD2File model = new MD2File(reader);
                        //string path = System.IO.Path.ChangeExtension("dump\\" + e.Filename.ToLower(), ".obj");
                        //string directory = System.IO.Path.GetDirectoryName(path);
                        //System.IO.Directory.CreateDirectory(directory);
                        //(model.Blocks["GEO2"] as GEO2Block).ExportOBJ(path);

                        //log.WriteLine(e.Filename);
                        //if (model.Blocks.ContainsKey(BlockHeader.MAGIC_ANCHOR))
                        //{
                        //    AnchorBlock ab = model.Blocks[BlockHeader.MAGIC_ANCHOR] as AnchorBlock;
                        //    log.WriteLine("  " + ab.Anchors.Count + " anchors:");
                        //    foreach (Anchor a in ab.Anchors)
                        //    {
                        //        log.WriteLine("    " + a.MaterialIndex.ToString("X8") + ": " + a.Name + " (" + a.LinkedFilename + ")");
                        //    }
                        //}
                    }
                }
                if (System.IO.Path.GetExtension(e.Filename).ToLower() == ".pc texture")
                {
                    try
                    {
                        //log.WriteLine("Exporting texture \"" + e.Filename + "\"");
                        //System.Console.WriteLine("Exporting texture \"" + e.Filename + "\"");
                        using (System.IO.MemoryStream ms = new System.IO.MemoryStream(fs.GetFileData(e)))
                        using (System.IO.BinaryReader reader = new System.IO.BinaryReader(ms))
                        {
                            Texture texture = new Texture(reader);
                            string path = System.IO.Path.ChangeExtension("dump\\" + e.Filename.ToLower(), ".tga");
                            string directory = System.IO.Path.GetDirectoryName(path);
                            log.WriteLine(texture.Unk08.ToString("X8") + " [GOOD]: " + e.Filename);
                            System.IO.Directory.CreateDirectory(directory);
                            if (texture.PCTFormat == Texture.ImageFormat.DXT3)
                            {
                                texture.DumpTGA(path);
                                System.Console.WriteLine("Converted " + e.Filename);
                            }
                            //else
                            //    System.Console.WriteLine("Skipping DXT3");


                            //(model.Blocks["GEO2"] as GEO2Block).ExportOBJ(path);

                            //log.WriteLine(texture.ToString());
                        }
                    }   
                    catch
                    {
                        //System.Console.WriteLine("[ERROR]: other");
                        using (System.IO.MemoryStream ms = new System.IO.MemoryStream(fs.GetFileData(e)))
                        {
                            System.IO.Directory.CreateDirectory("error/" + System.IO.Path.GetDirectoryName(e.Filename));
                            System.IO.File.WriteAllBytes("error/" + e.Filename, ms.ToArray());
                        }
                    }
                }
                if (System.IO.Path.GetExtension(e.Filename).ToLower() == ".ifl")
                {
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream(fs.GetFileData(e)))
                    {
                        IFLFile ifl = new IFLFile(ms, e.Filename);
                        System.Console.WriteLine(e.Filename + " - " + ifl.Frames.Count + " frames");
                        foreach (IFLFile.IFLFrame frame in ifl.Frames)
                        {
                            System.Console.WriteLine("    " + frame.ToString());
                        }
                    }
                }*/
                if (System.IO.Path.GetExtension(e.Filename).ToLower() == ".wrl")
                {
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream(fs.GetFileData(e)))
                    using (System.IO.BinaryReader reader = new System.IO.BinaryReader(ms))
                    {
                        World w = new World(reader);
                        System.Console.WriteLine("World '" + e.Filename + "' - " + w.Objects.Count + " Objects:");
                        log.WriteLine("World '" + e.Filename + "' - " + w.Objects.Count + " Objects:");

                        foreach (Drome.Object o in w.Objects)
                        {
                            //log.WriteLine("[" + o.GetType().Name + "] " + o.Header.ClassName + " '" + o.InstanceName + (o.ParentInstanceName.Length > 0 ? "' (parent '" + o.ParentInstanceName + "')" : "'"));

                        }
                    }
                }
                /*if (System.IO.Path.GetExtension(e.Filename).ToLower() == ".oli")
                {
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream(fs.GetFileData(e)))
                    using (System.IO.BinaryReader reader = new System.IO.BinaryReader(ms))
                    {
                        OLIFile oli = new OLIFile(reader);
                        oli.DumpOBJ("olidump_" + System.IO.Path.GetFileNameWithoutExtension(e.Filename) + ".obj");
                    }
                }*/
            }

            System.Console.WriteLine("Done.");
            System.Console.ReadKey();

            fs.Dispose();
            log.Dispose();
        }
    }
}
