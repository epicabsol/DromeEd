using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DromeEd.Controls;
using SharpDX;

namespace DromeEd.SceneNodes
{
    public class GeneralStaticSceneNode : AtomPhysicsSceneNode
    {
        public Drome.Objects.GeneralStatic GeneralStatic { get; }

        private List<Controls.SceneScreen.Mesh> Meshes = new List<Controls.SceneScreen.Mesh>();

        public GeneralStaticSceneNode(Controls.SceneScreen screen, Drome.Objects.GeneralStatic _object) : base(screen, _object, _object.AtomPhysicsData)
        {
            GeneralStatic = _object;

            if (GeneralStatic.ModelFilename == "")
                return;

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(Program.Filesystem.GetFileData(Program.Filesystem.GetFileEntry(GeneralStatic.ModelFilename))))
            using (System.IO.BinaryReader reader = new System.IO.BinaryReader(ms))
            {
                Drome.MD2File file = new Drome.MD2File(reader);
            
                foreach (Controls.SceneScreen.Mesh mesh in screen.LoadModel(file))
                {
                    Meshes.Add(mesh);
                }
            }
        }

        protected override void OnRender(SceneScreen screen)
        {
            //base.OnRender(screen);
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
