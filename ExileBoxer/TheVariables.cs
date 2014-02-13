using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;
using Loki.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExileBoxer
{
    public class TheVariables
    {
        public static bool temp = false;


        public static string acceptPartyInviteFrom = string.Empty;
        public static string nameLeader = string.Empty;
        public static string areaIdLeader = string.Empty;
        public static string areaIdMe = string.Empty;
        public static string areaNameMe = string.Empty;
        public static string areaNameLeader = string.Empty;
        public static string targetTown = string.Empty;
        public static string townIdLeader = "0_0_0";
        public static string townIdMe = "0_0_0";
        public static string takeAreaTransition = string.Empty;

        public static bool leaveParty = false;
        public static bool makePortal = false;
        public static bool moveToMiddleOfTown = false;
        public static bool checkBox1 = false;
        public static bool checkBox2 = false;
        public static bool inTownMe = false;
        public static bool inTownLeader = false;

        public static Stopwatch makePortalTimer = new Stopwatch();
        public static Stopwatch takeWpTimer = new Stopwatch();
        public static Stopwatch activateInstanceTimer = new Stopwatch();
        public static Stopwatch activateInstanceManagerTimer = new Stopwatch();

        public static Portal takePortalFromTownToArea = null;
        public static Portal takePortalFromAreaToTown = null;
        public static Portal portalFromTownToArea = null;
        public static Portal portalFromAreaToTown = null;

        public static int numUpDown1 = 0;
        public static int numUpDown2 = 0;
        public static int numUpDown3 = 0;
        public static int distanceLeader = 0;

        public static Vector2i posLeader = new Vector2i();
        public static Vector2i posMe = new Vector2i();
        public static Vector2i town1middle = new Vector2i(252, 245);
        public static Vector2i town2middle = new Vector2i(185, 169);
        public static Vector2i town3middle = new Vector2i(251, 293);

        public static WorldAreaEntry desiredWP = new WorldAreaEntry();

        public static List<AreaTransition> availableAreaTransitions = new List<AreaTransition>();
    }
}
