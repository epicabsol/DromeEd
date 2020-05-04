using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace DromeEd.Drome.Objects
{
    public class AtomPhysicsData
    {
        public Vector3 Position;
        public Quaternion Direction;
        public float FrictionCoeffecient;
        public float RestitutionCoeffecient;
        public uint SurfaceIndex = 0;

        public AtomPhysicsData()
        {

        }

        public AtomPhysicsData(System.IO.BinaryReader reader)
        {
            Position = reader.ReadVector3();
            Direction = reader.ReadQuaternion();
            FrictionCoeffecient = reader.ReadSingle();
            RestitutionCoeffecient = reader.ReadSingle();
            SurfaceIndex = reader.ReadUInt32();
        }

        public void Write(System.IO.BinaryWriter writer)
        {
            writer.Write(Position);
            writer.Write(Direction);
            writer.Write(FrictionCoeffecient);
            writer.Write(RestitutionCoeffecient);
            writer.Write(SurfaceIndex);
        }
    }
}
