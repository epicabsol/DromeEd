using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ATD.VFS;

namespace DromeEd.Controls
{
    public partial class WorldEditorControl : UserControl
    {
        public Drome.World World { get; private set; } = null;

        public SceneScreen Screen { get; } = null;

        public WorldEditorControl()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            Screen = new SceneScreen(Renderer);
            Renderer.Screen = Screen;
        }

        public void Destroy()
        {

        }

        public void LoadWorld(Drome.World world)
        {
            World = world;

            foreach (Drome.Object o in world.Objects)
            {
                TreeNode node = new TreeNode(o.InstanceName);
                SceneTreeView.Nodes.Add(node);

                // HACK: should use Reflection with an Attribute instead of this hard-coded list.
                if (o.Header.ClassName == "cGeneralStatic")
                {
                    SceneNodes.GeneralStaticSceneNode n = new SceneNodes.GeneralStaticSceneNode(Screen, o as Drome.Objects.GeneralStatic);
                    Screen.Nodes.Add(n);
                }
                else if (o.Header.ClassName == "cWeaponPickup")
                {
                    SceneNodes.WeaponPickupSceneNode n = new SceneNodes.WeaponPickupSceneNode(Screen, o as Drome.Objects.WeaponPickup);
                    Screen.Nodes.Add(n);
                }
                else if (o.Header.ClassName == "cOctreeModel")
                {
                    SceneNodes.OctreeModelSceneNode n = new SceneNodes.OctreeModelSceneNode(Screen, o as Drome.Objects.OctreeModel);
                    Screen.Nodes.Add(n);
                }
            }
        }

        public void LoadWorld(FileEntry file)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(Program.Filesystem.GetFileData(file)))
            using (System.IO.BinaryReader reader = new System.IO.BinaryReader(ms))
            {
                LoadWorld(new Drome.World(reader));
            }
        }

        private void WorldEditorControl_Load(object sender, EventArgs e)
        {
            toolStrip1.Renderer = new ToolstripRenderer();
        }
    }
}
