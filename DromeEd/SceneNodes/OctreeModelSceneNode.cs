using DromeEd.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DromeEd.Drome;
using SharpDX;

namespace DromeEd.SceneNodes
{
    public class OctreeModelSceneNode : AtomPhysicsSceneNode
    {
        public Drome.Objects.OctreeModel OctreeModel { get; }

        private VOMFile VOMFile { get; }
        private List<SceneScreen.Mesh> Meshes = new List<SceneScreen.Mesh>();

        public OctreeModelSceneNode(SceneScreen screen, Drome.Objects.OctreeModel model) : base(screen, model, model.Physics)
        {
            this.OctreeModel = model;

            if (OctreeModel.VOMFilename == "")
                return;

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(Program.Filesystem.GetFileData(Program.Filesystem.GetFileEntry(OctreeModel.VOMFilename))))
            using (System.IO.BinaryReader reader = new System.IO.BinaryReader(ms))
            {
                VOMFile = new VOMFile(reader);

                List<SceneScreen.RenderTexture> textures = new List<SceneScreen.RenderTexture>();
                foreach (TextureReference texref in VOMFile.BitmapIndices)
                {
                    textures.Add(screen.LoadTextureReference(texref));
                }

                //VOMFile.ExportOBJ("vom.obj");
                foreach (RenderGroup rg in VOMFile.RenderGroups)
                {
                    SceneScreen.Mesh mesh = new SceneScreen.Mesh(screen.Renderer.D3DDevice, rg);
                    TextureBlend baseBlend = rg.TextureBlends.First(b => b.Effect == Texture.MapType.Base);
                    if (baseBlend.TextureIndex < textures.Count)
                    {
                        mesh.DiffuseTexture = textures[baseBlend.TextureIndex];
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("ERROR: TextureBlend.TextureIndex out of range!");
                        mesh.DiffuseTexture = new SceneScreen.RenderTexture(screen.GetTexture("__error"));
                    }
                    Meshes.Add(mesh);
                }
            }
        }

        protected override void OnRender(SceneScreen screen)
        {
            foreach (SceneScreen.Mesh mesh in Meshes)
            {
                screen.RenderMesh(mesh, WorldTransform);
            }
        }

        protected override void OnUpdate(float timeStep, Matrix parentTransform)
        {
            base.OnUpdate(timeStep, parentTransform);
            foreach (SceneScreen.Mesh mesh in Meshes)
            {
                mesh.DiffuseTexture.Update(timeStep);
            }
        }
    }
}
