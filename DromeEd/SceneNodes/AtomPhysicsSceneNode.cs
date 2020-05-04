using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace DromeEd.SceneNodes
{
    public class AtomPhysicsSceneNode : DirectionalObjectSceneNode 
    {
        public Drome.Objects.AtomPhysicsData PhysicsData { get; }

        public override Vector3 Position { get => PhysicsData.Position; set => PhysicsData.Position = value; }
        public override Quaternion Direction { get => PhysicsData.Direction; set => PhysicsData.Direction = value; }
        public float FrictionCoeffecient { get => PhysicsData.FrictionCoeffecient; set => PhysicsData.FrictionCoeffecient = value; }
        public float RestitutionCoeffecient { get => PhysicsData.RestitutionCoeffecient; set => PhysicsData.RestitutionCoeffecient = value; }
        public uint SurfaceIndex { get => PhysicsData.SurfaceIndex; set => PhysicsData.SurfaceIndex = value; }

        public AtomPhysicsSceneNode(Controls.SceneScreen screen, Drome.Object _object, Drome.Objects.AtomPhysicsData physics) : base(screen, _object)
        {
            PhysicsData = physics;
        }
    }
}
