using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace DromeEd.SceneNodes
{
    public abstract class PositionalObjectSceneNode : ObjectSceneNode
    {
        public abstract Vector3 Position { get; set; }

        public PositionalObjectSceneNode(Controls.SceneScreen screen, Drome.Object _object) : base(screen, _object)
        {

        }

        protected override void OnUpdate(float timeStep, Matrix parentTransform)
        {
            LocalTransform = Matrix.Translation(Position);
            WorldTransform = LocalTransform * parentTransform;
        }
    }
}
