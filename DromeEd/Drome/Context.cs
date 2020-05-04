using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DromeEd.Drome
{
    public class Context
    {
        public static Context Current { get; set; }

        public enum NextGenGame : byte
        {
            LegoRacers2 = 1,
            DromeRacers = 2,
        }

        public string GamePath { get; private set; }

        public NextGenGame Game { get; private set; }

        public Context(NextGenGame game, string gamePath)
        {
            Game = game;
            GamePath = gamePath;
        }
    }
}
