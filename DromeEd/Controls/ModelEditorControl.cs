using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;
using System.IO;
using ATD.VFS;

namespace DromeEd.Controls
{
    public partial class ModelEditorControl : UserControl
    {
        public class EditableObject : Transformable
        {
            public TreeNode TreeNode = null;
            public virtual bool Selected { get; set; } = false;
            public SceneNode RootSceneNode = null;
            public Drome.GeometryBlock.LOD.LODLevel LODLevel = Drome.GeometryBlock.LOD.LODLevel.GeometryAllLevels;
            public override Matrix Transform { get; set; }
        }

        public class ModelEditableObject : EditableObject
        {
            public ModelEditableObject(SharpDX.Direct3D11.Device device, Drome.RenderGroup rg, string nodeText)
            {
                TreeNode = new TreeNode(nodeText);
                SceneScreen.Mesh mesh = new SceneScreen.Mesh(device, rg);
                RootSceneNode = new StaticMeshNode(mesh);
            }

            public ModelEditableObject(SceneScreen.Mesh mesh, string nodeText)
            {
                TreeNode = new TreeNode(nodeText);
                RootSceneNode = new StaticMeshNode(mesh);
            }
        }

        public class AnchorEditableObject : EditableObject
        {
            public Drome.Anchor Anchor { get; }

            public override Matrix Transform
            {
                get
                {
                    return new Matrix(-Anchor.Right.X, -Anchor.Right.Y, -Anchor.Right.Z, 0.0f,
                                                    Anchor.Up.X, Anchor.Up.Y, Anchor.Up.Z, 0.0f,
                                                    Anchor.Forward.X, Anchor.Forward.Y, Anchor.Forward.Z, 0.0f,
                                                    Anchor.AnchorPos.X, Anchor.AnchorPos.Y, Anchor.AnchorPos.Z, 1.0f);
                }
                set
                {
                    Anchor.Right = new Vector3(-value.M11, -value.M12, -value.M13);
                    Anchor.Up = new Vector3(value.M21, value.M22, value.M23);
                    Anchor.Forward = new Vector3(value.M31, value.M32, value.M33);
                    Anchor.AnchorPos = new Vector3(value.M41, value.M42, value.M43);
                }
            }

            public AnchorEditableObject(Drome.Anchor anchor)
            {
                Anchor = anchor;
            }
        }

        public Drome.MD2File Model { get; private set; }

        public string Filename { get; private set; }

        public SceneScreen Screen { get; }

        private SceneScreen.Mesh LocatorMesh;

        //public Dictionary<Drome.GEO2Block.RenderGroup, SceneNode> RGProxies = new Dictionary<Drome.GEO2Block.RenderGroup, SceneNode>();
        private EditableObject _selectedObject;
        public EditableObject SelectedObject
        {
            get
            {
                return _selectedObject;
            }
            set
            {
                if (_selectedObject != null) _selectedObject.Selected = false;
                _selectedObject = value;
                if (value != null) value.Selected = true;
            }
        }

        public List<EditableObject> Objects { get; } = new List<EditableObject>();

        public ModelEditorControl()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            Screen = new SceneScreen(d3DRenderer1);
            d3DRenderer1.Screen = Screen;

            Screen.Camera.Yaw = MathUtil.PiOverFour;
            Screen.Camera.Pitch = MathUtil.PiOverFour;
            Screen.Updated += (sender, timeStep) =>
            {
                Screen.Camera.Offset.Threshold = Screen.Camera.Offset.TargetValue * 0.005f;
            };
            styledTreeView1.AfterSelect += (sender, args) =>
            {
                SelectedObject = args.Node.Tag as EditableObject;
                Program.MainWindow.PropertyGrid.SelectedObject = args.Node.Tag;
            };
        }

        private void ModelEditorControl_Load(object sender, EventArgs e)
        {
            toolStrip1.Renderer = new ToolstripRenderer();
        }

        public void Destroy()
        {
            LocatorMesh?.Dispose();
            d3DRenderer1.Destroy();
        }

        public void LoadModel(Drome.MD2File model, string filename)
        {
            styledTreeView1.Nodes.Clear();

            if (LocatorMesh == null)
            {
                SceneScreen.WorldVertex[] LocatorVertices = new SceneScreen.WorldVertex[]
{
                new SceneScreen.WorldVertex(1, 0, 0, 0, 0, 0, 1, 1, 1, 1),
                new SceneScreen.WorldVertex(-1, 0, 0, 0, 0, 0, 1, 1, 1, 1),
                new SceneScreen.WorldVertex(0, 1, 0, 0, 0, 0, 1, 1, 1, 1),
                new SceneScreen.WorldVertex(0, -1, 0, 0, 0, 0, 1, 1, 1, 1),
                new SceneScreen.WorldVertex(0, 0, 1, 0, 0, 0, 1, 1, 1, 1),
                new SceneScreen.WorldVertex(0, 0, -1, 0, 0, 0, 1, 1, 1, 1)
};
                uint[] LocatorIndices = new uint[]
                {
                0, 1,
                2, 3,
                4, 5
                };
                LocatorMesh = new SceneScreen.Mesh(d3DRenderer1.D3DDevice, LocatorVertices, LocatorIndices, SharpDX.Direct3D.PrimitiveTopology.LineList);
                LocatorMesh.DiffuseTexture = new SceneScreen.RenderTexture(Screen.GetTexture("__blank"));
            }

            Filename = filename;

            Model = model;
            // Model
            TreeNode ModelNode = new TreeNode("Model");
            Drome.MDLBlock mdl = Model.Blocks[Drome.BlockHeader.MAGIC_MODEL3] as Drome.MDLBlock;

            // Texture References
            TreeNode TexturesNode = new TreeNode("Texture References");
            foreach (Drome.TextureReference tex in mdl.TextureReferences)
            {
                TreeNode n = new TreeNode(tex.MapName + " (" + tex.MapType.ToString() + ")");
                TexturesNode.Nodes.Add(n);
            }
            ModelNode.Nodes.Add(TexturesNode);

            // Materials
            TreeNode MaterialsNode = new TreeNode("Materials");
            int i = 0;
            foreach (Drome.MaterialProps m in mdl.Materials)
            {
                TreeNode n = new TreeNode("Material " + i);
                MaterialsNode.Nodes.Add(n);
                i++;
            }
            ModelNode.Nodes.Add(MaterialsNode);

            // Meshes
            List<SceneScreen.Mesh> modelMeshes = Screen.LoadModel(Model);
            TreeNode LODNode = new TreeNode("LODs");
            //RGProxies.Clear();
            Objects.Clear();
            /*foreach (SceneScreen.Mesh mesh in modelMeshes)
            {
                Screen.Nodes.Add(new StaticMeshNode(mesh));
            }*/
            i = 0;
            Drome.GeometryBlock geo = model.Blocks[Drome.BlockHeader.MAGIC_GEOMETRY2] as Drome.GeometryBlock;
            LODDropDown.Items.Clear();
            foreach (Drome.GeometryBlock.LOD lod in geo.LODs)
            {
                if (!LODDropDown.Items.Contains(lod.Type.ToString()))
                    LODDropDown.Items.Add(lod.Type.ToString());
                TreeNode ln = new TreeNode(lod.Type.ToString());
                foreach (Drome.RenderGroup rg in lod.RenderGroups)
                {
                    //TreeNode n = new TreeNode("Group " + i);
                    //RGProxies.Add(rg, Screen.Nodes[i]);
                    ModelEditableObject obj = new ModelEditableObject(modelMeshes[i], "Group " + i);
                    Objects.Add(obj);
                    obj.LODLevel = lod.Type;
                    ln.Nodes.Add(obj.TreeNode);
                    Screen.Nodes.Add(obj.RootSceneNode);
                    i++;
                }
                LODNode.Nodes.Add(ln);
            }
            ModelNode.Nodes.Add(LODNode);

            // Anchors
            TreeNode AnchorsNode = new TreeNode("Anchors");
            if (Model.Blocks.ContainsKey(Drome.BlockHeader.MAGIC_ANCHOR))
            {
                Drome.AnchorBlock anchors = Model.Blocks[Drome.BlockHeader.MAGIC_ANCHOR] as Drome.AnchorBlock;
                foreach (Drome.Anchor a in anchors.Anchors)
                {
                    a.Forward.Normalize();
                    a.Right.Normalize();
                    a.Up.Normalize();
                    AnchorEditableObject anchor = new AnchorEditableObject(a);
                    Matrix anchorLocatorTransform = anchor.Transform;
                    anchorLocatorTransform.Right.Normalize();
                    anchorLocatorTransform.Up.Normalize();
                    anchorLocatorTransform.Forward.Normalize();
                    StaticMeshNode locatorNode = new StaticMeshNode(LocatorMesh)
                    {
                        LocalTransform = anchorLocatorTransform
                    };
                    Screen.Nodes.Add(locatorNode);
                    anchor.RootSceneNode = locatorNode;
                    TreeNode n = new TreeNode(a.AnchorName + ": " + a.LinkedFilename);
                    n.Tag = anchor;
                    AnchorsNode.Nodes.Add(n);
                    Objects.Add(anchor);

                    // Add preview of anchor instance
                    FileEntry file = null; 
                    if ((file = Program.Filesystem.GetFileEntry(Path.Combine(Path.GetDirectoryName(Filename), Path.GetFileNameWithoutExtension(Filename) + "_" + a.AnchorName + ".md2"))) != null)
                    {
                        using (MemoryStream ms = new MemoryStream(Program.Filesystem.GetFileData(file)))
                        using (BinaryReader reader = new BinaryReader(ms))
                        {
                            Drome.MD2File m = new Drome.MD2File(reader);
                            Drome.GeometryBlock ageo = m.Blocks[Drome.BlockHeader.MAGIC_GEOMETRY2] as Drome.GeometryBlock;
                            List<SceneScreen.Mesh> instanceMeshes = Screen.LoadModel(m);
                            /*foreach (SceneScreen.Mesh msh in instanceMeshes)
                            {
                                StaticMeshNode node = new StaticMeshNode(msh);
                                node.LocalTransform = anchorTransform;
                                Screen.Nodes.Add(node);
                            }*/
                            int j = 0;
                            EditableObject instance = new EditableObject();
                            instance.TreeNode = n;
                            instance.RootSceneNode = locatorNode;
                            foreach (Drome.GeometryBlock.LOD lod in ageo.LODs)
                            {
                                if (lod.Type != Drome.GeometryBlock.LOD.LODLevel.GeometryNormalDetail)
                                    continue;

                                foreach (Drome.RenderGroup rg in lod.RenderGroups)
                                {
                                    StaticMeshNode mesh = new StaticMeshNode(instanceMeshes[j]);
                                    mesh.Wireframe = true;
                                    //mesh.Solid = false;
                                    locatorNode.Children.Add(mesh);
                                    j++;
                                }
                            }
                            Objects.Add(instance);
                        }
                    }
                }
            }
            ModelNode.Nodes.Add(AnchorsNode);

            styledTreeView1.Nodes.Add(ModelNode);

            LODDropDown.SelectedIndex = 0;
        }

        public void LoadModel(System.IO.BinaryReader reader, string filename)
        {
            LoadModel(new Drome.MD2File(reader), filename);
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Wavefront OBJ Mesh (*.obj)|*.obj|Original MD2 File (*.md2)|*.md2";
            dialog.FileName = System.IO.Path.GetFileNameWithoutExtension(Filename);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (dialog.FilterIndex == 1)
                {
                    // Export OBJ
                    Drome.GeometryBlock geo = Model.Blocks[Drome.BlockHeader.MAGIC_GEOMETRY2] as Drome.GeometryBlock;
                    if (geo == null)
                        geo = Model.Blocks[Drome.BlockHeader.MAGIC_GEOMETRY1] as Drome.GeometryBlock;
                    if (geo == null)
                        geo = Model.Blocks[Drome.BlockHeader.MAGIC_GEOMETRY] as Drome.GeometryBlock;

                    if (geo == null)
                    {
                        MessageBox.Show("Couldn't find acceptable GEO block in MD2.");
                    }
                    else
                    {
                        Model.ExportOBJ(dialog.FileName);
                    }
                }
                else if (dialog.FilterIndex == 2)
                {
                    // Write original MD2 data
                    try
                    {
                        File.WriteAllBytes(dialog.FileName, Program.Filesystem.GetFileData(Program.Filesystem.GetFileEntry(Filename)));
                    }
                    catch (IOException ex)
                    {
                        Clipboard.SetText(ex.ToString());
                        MessageBox.Show("IOException writing file: \n\n" + ex.ToString() + "\n\n (Copied to clipboard)");
                    }
                }
            }
        }

        private void LODDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (LODDropDown.SelectedIndex == -1)
                return;
            Drome.GeometryBlock.LOD.LODLevel level = (Drome.GeometryBlock.LOD.LODLevel)Enum.Parse(typeof(Drome.GeometryBlock.LOD.LODLevel), (string)LODDropDown.SelectedItem);

            foreach (EditableObject obj in Objects)
            {
                obj.RootSceneNode.Visible = (obj.LODLevel == Drome.GeometryBlock.LOD.LODLevel.GeometryAllLevels || obj.LODLevel == level);
            }
        }
    }
}
