using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DromeEd.SceneNodes
{
    public class WeaponPickupSceneNode : GeneralStaticSceneNode
    {
        public Drome.Objects.WeaponPickup Pickup { get; }
        // uint Unk01
        // uint Unk02

        public WeaponPickupSceneNode(Controls.SceneScreen screen, Drome.Objects.WeaponPickup _object) : base(screen, _object)
        {
            Pickup = _object as Drome.Objects.WeaponPickup;
        }
    }
}
