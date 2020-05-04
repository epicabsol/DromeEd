using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace DromeEd.SceneNodes
{
    public abstract class DirectionalObjectSceneNode : PositionalObjectSceneNode
    {
        public abstract Quaternion Direction { get; set; }

        public DirectionalObjectSceneNode(Controls.SceneScreen screen, Drome.Object _object) : base(screen, _object)
        {

        }

        protected override void OnUpdate(float timeStep, Matrix parentTransform)
        {
            LocalTransform = Matrix.RotationQuaternion(Direction);
            LocalTransform *= Matrix.Translation(Position);
            WorldTransform = LocalTransform * parentTransform;
        }
    }
}
