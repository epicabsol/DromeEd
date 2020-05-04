using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DromeEd.Drome.Objects
{
    [ObjectType("cWeaponPickup")]
    public class WeaponPickup : GeneralStatic
    {
        public enum PickupType : uint
        {
            Health,
            Weapon
        }

        public PickupType Type;
        public uint Unk01;
        public uint Unk02;

        public WeaponPickup(ObjectHeader header, System.IO.BinaryReader reader) : base(header, reader)
        {
            Type = (PickupType)reader.ReadUInt32();
            Unk01 = reader.ReadUInt32();
            Unk02 = reader.ReadUInt32();
        }
    }
}
