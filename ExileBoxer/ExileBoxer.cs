using log4net;
using Loki.Bot;
using Loki.Bot.Logic.Behaviors;
using Loki.Bot.Navigation;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.NativeWrappers;
using Loki.Game.Objects;
using Loki.TreeSharp;
using Loki.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Action = Loki.TreeSharp.Action;

namespace ExileBoxer
{
    public class ExileBoxer : IBot
    {
        #region Variables and Stuff
        public static GUI gui { get; set; }
        public static readonly ILog Log = Logger.GetLoggerInstanceForType();
        #endregion
        #region IBot
        public void Start() 
        {
            Logic = new PrioritySelector(
                //reset TheVariables - works
                new Decorator(ret => LokiPoe.ObjectManager.Me.IsInTown,
                    new Sequence(
                        new Action(ret => TheVariables.takePortalFromAreaToTown = null),
                        new Action(ret => TheVariables.makePortal = false),
                        new Action(ret => RunStatus.Failure)
                        )
                    ),
                new Decorator(ret => !LokiPoe.ObjectManager.Me.IsInTown,
                    new Sequence(
                        new Action(ret => TheVariables.takePortalFromTownToArea = null),
                        new Action(ret => RunStatus.Failure)
                        )
                    ),


                //Party stuff - works
                new Decorator(ret => TheVariables.acceptPartyInviteFrom.Length > 1,
                    TheLogic.AcceptPartyInvite()
                    ),
                new Decorator(ret => TheVariables.leaveParty,
                    TheLogic.LeaveParty()
                    ),


                //town-only-stuff
                new Decorator(ret => LokiPoe.ObjectManager.Me.IsInTown && TheVariables.moveToMiddleOfTown,
                    TheLogic.MoveToMiddleOfTown()
                    ),
                new Decorator(ret => LokiPoe.ObjectManager.Me.IsInTown && TheVariables.targetTown.Length > 1,
                    TheLogic.SwitchTown(TheVariables.targetTown)
                    ),


                //portal stuff - works
                new Decorator(ret => TheVariables.takePortalFromTownToArea != null || TheVariables.takePortalFromAreaToTown != null,
                    TheLogic.TakeTP()
                    ),
                new Decorator(ret => TheVariables.makePortal && !LokiPoe.ObjectManager.Me.IsInTown,
                    TheLogic.MakeTP()
                    ),

                
                //inArea or town stuff
                new Decorator(ret => TheVariables.takeAreaTransition.Length > 1 && TheVariables.temp,
                    TheLogic.MoveToAndTakeAreaTransition()
                    //new Action(ret => ExileBoxer.Log.Debug("i want to take a transition now.."))
                    ),


                //inArea stuff #1
                //fight! return Success if we are fighting, so move does not get triggered pls!
                new Decorator(ret => !LokiPoe.ObjectManager.Me.IsInTown && TheVariables.checkBox2,
                    TheLogic.Fight()
                    ),

                //inArea stuff #2 - MOVE TO SHOULD BE THE VERY LAST THING PLEASE!!!!! - works
                new Decorator(ret => !LokiPoe.ObjectManager.Me.IsInTown && TheVariables.checkBox1,
                    new PrioritySelector(
                        new Decorator(ret => TheVariables.distanceLeader > TheVariables.numUpDown2,
                            @CommonBehaviors.MoveTo(ret => TheVariables.posLeader, ret => "", TheVariables.numUpDown3)
                        ),
                        new Decorator(ret => TheVariables.distanceLeader <= TheVariables.numUpDown3 && LokiPoe.ObjectManager.Me.IsMoving,
                            new Action(ret => Navigator.PlayerMover.Stop())
                            )
                        )
                    ),
                new Decorator(ret => !LokiPoe.ObjectManager.Me.IsInTown && !TheVariables.checkBox1,
                    new Action(ret => Navigator.PlayerMover.Stop())
                    )
                );

            gui = new GUI();
            gui.OnInit();

            Log.Debug("you should open the GUI by pressing 'Bot-Config' ... ");
            Log.Debug("you should open the GUI by pressing 'Bot-Config' ... ");
            Log.Debug("you should open the GUI by pressing 'Bot-Config' ... ");
            Log.Debug("you should open the GUI by pressing 'Bot-Config' ... ");
            Log.Debug("you should open the GUI by pressing 'Bot-Config' ... ");
            Log.Debug("you should open the GUI by pressing 'Bot-Config' ... ");
            Log.Debug("you should open the GUI by pressing 'Bot-Config' ... ");
            Log.Debug("you should open the GUI by pressing 'Bot-Config' ... ");
            Log.Debug("you should open the GUI by pressing 'Bot-Config' ... ");
            Log.Debug("you should open the GUI by pressing 'Bot-Config' ... ");
            Log.Debug("you should open the GUI by pressing 'Bot-Config' ... ");
            Log.Debug("you should open the GUI by pressing 'Bot-Config' ... ");
            Log.Debug("you should open the GUI by pressing 'Bot-Config' ... ");
            Log.Debug("you should open the GUI by pressing 'Bot-Config' ... ");
            Log.Debug("you should open the GUI by pressing 'Bot-Config' ... ");
            Log.Debug("you should open the GUI by pressing 'Bot-Config' ... ");
            Log.Debug("you should open the GUI by pressing 'Bot-Config' ... ");
            Log.Debug("you should open the GUI by pressing 'Bot-Config' ... you really should!");
            Log.Debug("you should open the GUI by pressing 'Bot-Config' ... ");
            Log.Debug("you should open the GUI by pressing 'Bot-Config' ... ");
            Log.Debug("you should open the GUI by pressing 'Bot-Config' ... :-)");
            Log.Debug("you should open the GUI by pressing 'Bot-Config' ... ");
            Log.Debug("you should open the GUI by pressing 'Bot-Config' ... ");

        }
        public void Pulse()
        {
            using (LokiPoe.AcquireFrame())
            {
                LokiPoe.ObjectManager.ClearCache();

                if (LokiPoe.ObjectManager.Me.PartyStatus == PartyStatus.PartyMember)
                {

                    TheVariables.posLeader = getPosition(LokiPoe.InstanceInfo.PartyMembers.First(pp => pp.MemberStatus == PartyStatus.PartyLeader).PlayerEntry);
                    TheVariables.nameLeader = LokiPoe.InstanceInfo.PartyMembers.First(p => p.MemberStatus == PartyStatus.PartyLeader).PlayerEntry.Name;
                    TheVariables.areaIdMe = LokiPoe.LocalData.WorldAreaId;
                    TheVariables.areaNameMe = LokiPoe.LocalData.WorldAreaName;
                    TheVariables.areaIdLeader = LokiPoe.InstanceInfo.PartyMembers.First(p => p.MemberStatus == PartyStatus.PartyLeader).PlayerEntry.AreaId;
                    TheVariables.areaNameLeader = LokiPoe.InstanceInfo.PartyMembers.First(p => p.MemberStatus == PartyStatus.PartyLeader).PlayerEntry.AreaName;
                    TheVariables.distanceLeader = getDistance(LokiPoe.InstanceInfo.PartyMembers.First(pp => pp.MemberStatus == PartyStatus.PartyLeader).PlayerEntry);
                    TheVariables.townIdLeader = string.Concat(TheVariables.areaIdLeader.Substring(0, 4), "town");
                    TheVariables.townIdMe = string.Concat(LokiPoe.LocalData.WorldAreaId.Substring(0,4), "town");

                    if (TheVariables.takeAreaTransition.Length < 1)
                        TheVariables.temp = false;
                    else
                        TheVariables.temp = true;

                    if (TheVariables.areaIdLeader == TheVariables.townIdLeader)
                        TheVariables.inTownLeader = true;
                    else
                        TheVariables.inTownLeader = false;

                    if (TheVariables.areaIdMe == TheVariables.townIdMe)
                        TheVariables.inTownMe = true;
                    else
                        TheVariables.inTownMe = false;

                    if (portalFromAreaToTown == null)
                        TheVariables.portalFromAreaToTown = null;
                    else
                        TheVariables.portalFromAreaToTown = portalFromAreaToTown;

                    if (portalFromTownToArea == null)
                        TheVariables.portalFromTownToArea = null;
                    else
                        TheVariables.portalFromTownToArea = portalFromTownToArea;

                    foreach(AreaTransition a in LokiPoe.ObjectManager.Objects.OfType<AreaTransition>())
                    {
                        bool xd = false;
                        
                        foreach(var b in TheVariables.availableAreaTransitions)
                        {
                            if(b.Name == a.Name)
                                xd = true;
                        }

                        if (!xd)
                            TheVariables.availableAreaTransitions.Add(a);
                    }

                    foreach(var a in TheVariables.availableAreaTransitions)
                    {
                        bool xd = false;

                        foreach(AreaTransition b in LokiPoe.ObjectManager.Objects.OfType<AreaTransition>())
                        {
                            if (b.Name == a.Name)
                                xd = true;
                        }

                        if (!xd)
                            TheVariables.availableAreaTransitions.Remove(a);
                    }
                }

                TheVariables.posMe = LokiPoe.ObjectManager.Me.Position;

                gui.OnPulse();
                gui.Update();
            }
        }


        public System.Windows.Window ConfigWindow
        {
            get 
            {
                gui.Show();
                return null;
            }
        }
        public string Name { get { return "ExileBoxer"; } }
        public string Description { get { return "ExileBoxer by randomstraw"; } }
        public Composite Logic { get; set; }
        public PulseFlags PulseFlags { get { return PulseFlags.All; } }
        public bool RequiresGameInput { get { return true; } } //TRVE
        public void Stop() 
        {
            gui.Close();
            gui.Dispose();
        }
        public void Dispose() { }
        #endregion

        #region Helpers
        public static int getDistance(PlayerEntry pe)
        {
            using (LokiPoe.AcquireFrame())
            {
                LokiPoe.ObjectManager.ClearCache();

                if (pe.AreaId != LokiPoe.LocalData.WorldAreaId)
                    return -1;

                if (!isInMyInstance(pe))
                    return -2;

                return (int)LokiPoe.ObjectManager.Objects.OfType<Player>().First(p => p.Name == pe.Name).Distance;
            }
        }
        public static Vector2i getPosition(PlayerEntry pe)
        {
            using (LokiPoe.AcquireFrame())
            {
                LokiPoe.ObjectManager.ClearCache();

                if (pe.AreaId != LokiPoe.LocalData.WorldAreaId)
                    return new Vector2i(-1, -1);

                if (!isInMyInstance(pe))
                    return new Vector2i(-2, -2);

                return LokiPoe.ObjectManager.Objects.OfType<Player>().First(p => p.Name == pe.Name).Position;
            }
        }
        public static bool isInMyInstance(PlayerEntry pe)
        {
            using (LokiPoe.AcquireFrame())
            {
                LokiPoe.ObjectManager.ClearCache();

                bool result = false;

                try
                {
                    if (LokiPoe.ObjectManager.Objects.OfType<Player>().First(x => x.Name == pe.Name).Distance > 0)
                        result = true;
                }
                catch (Exception e)
                {
                    result = false;
                    //TheFunctions.Debug(e.ToString()); //working now, no need to debugspam
                }

                return result;
            }
        }
        public static Portal portalFromAreaToTown
        {
            get
            {
                using (LokiPoe.AcquireFrame())
                {
                    LokiPoe.ObjectManager.ClearCache();

                    if (LokiPoe.ObjectManager.Me.IsInTown)
                        return null;

                    try
                    {
                        if (LokiPoe.ObjectManager.Portals.Count(p => p.Distance < 200) < 1)
                            return null;

                        return LokiPoe.ObjectManager.Portals.First(p => p.Distance < 200);
                    }
                    catch(Exception e)
                    {
                        return null;
                    }
                }
            }
        }
        public static Portal portalFromTownToArea
        {
            get
            {
                using (LokiPoe.AcquireFrame())
                {
                    LokiPoe.ObjectManager.ClearCache();

                    if (!LokiPoe.ObjectManager.Me.IsInTown)
                        return null;

                    try
                    {
                        PortalObject po = LokiPoe.LocalData.TownPortals.First(o => o.AreaId == LokiPoe.InstanceInfo.PartyMembers.First(x => x.MemberStatus == PartyStatus.PartyLeader).PlayerEntry.AreaId);

                        if (po == null)
                            return null;

                        Portal p = LokiPoe.ObjectManager.Portals.First(o => o.Name.Contains(po.OwnerName));

                        if (p == null)
                            return null;

                        return p;
                    }
                    catch(Exception e)
                    {
                        return null;
                    }
                }
            }
        }
        public static bool haveTPScrolls
        {
            get
            {
                using (LokiPoe.AcquireFrame())
                {
                    LokiPoe.ObjectManager.ClearCache();

                    return LokiPoe.ObjectManager.Me.Inventory.Main.FindItem("Portal Scroll").Item.Components.StackComponent.StackCount > 1;
                }
            }
        }
        public static NetworkObject getWaypointOfCurrentArea()
        {
            if (LokiPoe.ObjectManager.Waypoint == null)
                return null;

            return LokiPoe.ObjectManager.Waypoint;
        }
        public static bool WaypointAvailable(string destination)
        {
            bool res = false;

            foreach (var wp in LokiPoe.Gui.Waypoint.AvailableWaypoints)
            {
                if (wp.Id.Equals(destination))
                {
                    TheVariables.desiredWP = wp;
                    res = true;
                }
            }

            return res;
        }
        #endregion
    }
}
