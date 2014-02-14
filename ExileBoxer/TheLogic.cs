using Loki.Bot;
using Loki.Bot.Logic.Behaviors;
using Loki.Bot.Navigation;
using Loki.Game;
using Loki.Game.Objects;
using Loki.TreeSharp;
using Loki.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Action = Loki.TreeSharp.Action;

namespace ExileBoxer
{
    public class TheLogic
    {
        public static Composite AcceptPartyInvite()
        {
            return new Sequence(
                new Action(ret => LokiPoe.InstanceInfo.PendingPartyInvites.First(p => p.CreatorAccountName == TheVariables.acceptPartyInviteFrom).Accept()),
                new Action(ret => TheVariables.acceptPartyInviteFrom = "")
                );
        }
        public static Composite LeaveParty()
        {
            return new Sequence(
                new Action(ret => LokiPoe.Gui.Party.LeaveCurrentParty()),
                new Action(ret => TheVariables.leaveParty = false)
                );
        }
        public static Composite TakeTP()
        {
            return new PrioritySelector(
                new Decorator(ret => LokiPoe.ObjectManager.Me.IsInTown,
                    new PrioritySelector(
                        new Decorator(ret => TheVariables.takePortalFromTownToArea.Distance > 15,
                            CommonBehaviors.MoveTo(ret => TheVariables.takePortalFromTownToArea.Position, ret => "moving to Portal", 13)
                            ),
                        new Decorator(ret => TheVariables.takePortalFromTownToArea.Distance <= 15,
                            new Action(ret => TheVariables.takePortalFromTownToArea.Interact(false, false, LokiPoe.InputTargetType.MustMatchTarget))
                            )
                        )
                    ),
                new Decorator(ret => !LokiPoe.ObjectManager.Me.IsInTown,
                    new PrioritySelector(
                        new Decorator(ret => TheVariables.takePortalFromAreaToTown.Distance > 15,
                            CommonBehaviors.MoveTo(ret => TheVariables.takePortalFromAreaToTown.Position, ret => "moving to Portal", 13)
                            ),
                        new Decorator(ret => TheVariables.takePortalFromAreaToTown.Distance <= 15,
                            new Sequence(
                                new Action(ret => TheVariables.takePortalFromAreaToTown.Interact(false, false, LokiPoe.InputTargetType.MustMatchTarget)),
                                new Action(ret => TheVariables.moveToMiddleOfTown = true)
                                )
                            )
                        )
                    )
                );
        }
        public static Composite MakeTP()
        {
            return new Sequence(
                new Action(ret => LokiPoe.ObjectManager.Me.Inventory.Main.FindItem("Portal Scroll").Use()),
                new Action(ret => TheVariables.makePortal = false)
                );
        }
        public static Composite MoveTo(Vector2i pos)
        {
            return new PrioritySelector(
                new Decorator(ret => TheVariables.distanceLeader > TheVariables.numUpDown2,
                    CommonBehaviors.MoveTo(ret => pos, ret => "", TheVariables.numUpDown3)
                    )
                );
        }
        public static Composite Fight()
        {
            return new PrioritySelector(
                new Decorator(ret => LokiPoe.ObjectManager.Objects.OfType<Monster>().Count(m => m.IsActive && m.Distance <= TheVariables.numUpDown1) > 0,
                    RoutineManager.Current.Combat)
                );
        }

        public static Composite SwitchTown(string areaId)
        {
            return new PrioritySelector(
                new Decorator(ret => TheVariables.townIdMe == TheVariables.targetTown,
                    new Sequence(
                        new Action(ret => ExileBoxer.Log.Debug("successfully switched town!")),
                        new Action(ret => TheVariables.targetTown = ""),
                        new Action(ret => {

                            if(TheVariables.townIdMe.Contains("2_town"))
                                TheVariables.moveToMiddleOfTown = false;
                            else
                                TheVariables.moveToMiddleOfTown = true;

                        }),
                        new Action(ret => RunStatus.Success)
                        )
                    ),
                new Decorator(ret => !ExileBoxer.WaypointAvailable(TheVariables.targetTown),
                    new Sequence(
                        new Action(ret => ExileBoxer.Log.Debug("We don't a Waypoint to " + TheVariables.targetTown + "! sorry.")),
                        new Action(ret => TheVariables.targetTown = ""),
                        new Action(ret => RunStatus.Success)
                        )
                    ),
                new Decorator(ret => TheVariables.townIdMe != TheVariables.targetTown && ExileBoxer.WaypointAvailable(TheVariables.targetTown),
                    MoveToAndActivateWP(TheVariables.targetTown)
                    )
                );
        }

        public static Composite MoveToAndActivateWP(string destination)
        {
            NetworkObject wp = ExileBoxer.getWaypointOfCurrentArea();

            return new PrioritySelector(
                new Decorator(ret => wp.Distance > 15,
                    CommonBehaviors.MoveTo(ret => wp.Position, ret => "(town) moving to next Waypoint", 15)
                    ),
                new Decorator(ret => wp.Distance <= 15,
                    new PrioritySelector(
                        new Decorator(ret => !LokiPoe.Gui.Waypoint.IsWorldPanelWindowOpen,
                            new Action(ret => wp.Interact(false, false, LokiPoe.InputTargetType.MustMatchTarget))
                            ),
                        new Decorator(ret => LokiPoe.Gui.Waypoint.IsWorldPanelWindowOpen,
                            new PrioritySelector(
                                new Decorator(ret => !TheVariables.takeWpTimer.IsRunning,
                                    new Sequence(
                                        new Action(ret => ExileBoxer.Log.Debug("starting takeWpTimer")),
                                        new Action(ret => TheVariables.takeWpTimer.Start())
                                        )
                                    ),
                                new Decorator(ret => TheVariables.takeWpTimer.IsRunning && TheVariables.takeWpTimer.ElapsedMilliseconds > 1000,
                                    new Sequence(
                                        //new Action(ret => ExileBoxer.Log.Debug("clicky clicky @ " + LokiPoe.Gui.Waypoint.AvailableWaypoints.First(w => w.Id == destination).ToString())),
                                        //new Action(ret => LokiPoe.Gui.Waypoint.Take(LokiPoe.Gui.Waypoint.AvailableWaypoints.First(w => w.Id == destination))),
                                        new Action(ret => LokiPoe.Gui.Waypoint.Take(TheVariables.desiredWP, false)),
                                        new Action(ret => TheVariables.takeWpTimer.Reset()),
                                        new Action(ret => RunStatus.Success)
                                        )
                                    )
                                )
                            )
                        )
                    )
                );
        }
        public static Composite MoveToMiddleOfTown()
        {
            return new PrioritySelector(
                new Decorator(ret => TheVariables.townIdMe.Substring(2, 1) == "1",
                    new PrioritySelector(
                        new Decorator(ret => TheVariables.town1middle.Distance(LokiPoe.ObjectManager.Me.Position) > 20,
                            CommonBehaviors.MoveTo(ret => TheVariables.town1middle, ret => "", 10)
                            ),
                        new Decorator(ret => TheVariables.town1middle.Distance(LokiPoe.ObjectManager.Me.Position) <= 15,
                            new Sequence(
                                CommonBehaviors.MoveTo(ret => TheVariables.town2middle, ret => "", 10),
                                new Action(ret => ExileBoxer.Log.Debug("moved to middle of 1_town")),
                                new Action(ret => TheVariables.moveToMiddleOfTown = false)
                                )
                            )
                        )
                    ),
                new Decorator(ret => TheVariables.townIdMe.Substring(2, 1) == "2",
                    new PrioritySelector(
                        new Decorator(ret => TheVariables.town2middle.Distance(LokiPoe.ObjectManager.Me.Position) > 20,
                            CommonBehaviors.MoveTo(ret => TheVariables.town2middle, ret => "", 10)
                            ),
                        new Decorator(ret => TheVariables.town2middle.Distance(LokiPoe.ObjectManager.Me.Position) <= 15,
                            new Sequence(
                                CommonBehaviors.MoveTo(ret => TheVariables.town2middle, ret => "", 10),
                                new Action(ret => ExileBoxer.Log.Debug("moved to middle of 2_town")),
                                new Action(ret => TheVariables.moveToMiddleOfTown = false)
                                )
                            )
                        )
                    ),
                new Decorator(ret => TheVariables.townIdMe.Substring(2, 1) == "3",
                    new PrioritySelector(
                        new Decorator(ret => TheVariables.town3middle.Distance(LokiPoe.ObjectManager.Me.Position) > 20,
                            CommonBehaviors.MoveTo(ret => TheVariables.town3middle, ret => "", 10)
                            ),
                        new Decorator(ret => TheVariables.town3middle.Distance(LokiPoe.ObjectManager.Me.Position) <= 15,
                            new Sequence(
                                CommonBehaviors.MoveTo(ret => TheVariables.town3middle, ret => "", 10),
                                new Action(ret => ExileBoxer.Log.Debug("moved to middle of 3_town")),
                                new Action(ret => TheVariables.moveToMiddleOfTown = false)
                                )
                            )
                        )
                    )
                );
        }
        public static Composite MoveToAndTakeAreaTransition(bool newInstance = false)
        {
            try
            {
                if (LokiPoe.ObjectManager.Objects.OfType<AreaTransition>().Count() < 1)
                    return new Action(ret => BotMain.Stop("areaTransition error"));

            }
            catch(Exception e)
            {
                ExileBoxer.Log.Debug(e.ToString());
                BotMain.Stop("errör");
            }

            return new PrioritySelector(
                new Decorator(ret => !TheVariables.globalTimer.IsRunning,
                    new Action(ret => TheVariables.globalTimer.Start())
                    ),
                new Decorator(ret => TheVariables.globalTimer.IsRunning && TheVariables.globalTimer.ElapsedMilliseconds > 349,
                    new PrioritySelector(
                        new Decorator(ret => TheVariables.currentAreaTransition.Length < 1,
                            new Sequence(
                                new Action(ret => TheVariables.currentAreaTransition = LokiPoe.ObjectManager.Objects.OfType<AreaTransition>().First().Name),
                                new Action(ret => TheVariables.globalTimer.Reset()),
                                new Action(ret => RunStatus.Success)
                                )
                            ),
                        new Decorator(ret => TheVariables.currentAreaTransition.Length > 0,
                            new PrioritySelector(
                                new Decorator(ret => LokiPoe.ObjectManager.Objects.OfType<AreaTransition>().First().Name != TheVariables.currentAreaTransition,
                                    new Sequence(
                                        new Action(ret => TheVariables.takeNearestAreaTransition = false),
                                        new Action(ret => TheVariables.currentAreaTransition = string.Empty),
                                        new Action(ret => ExileBoxer.Log.Debug("successfully took area transition! congratz!")),
                                        new Action(ret => TheVariables.globalTimer.Reset())
                                        //new Action(ret => Navigator.PlayerMover.Stop())
                                        //should add a move to townmiddle, if we entered a town this way...
                                        )
                                    ),
                                new Decorator(ret => LokiPoe.ObjectManager.Objects.OfType<AreaTransition>().First().Name == TheVariables.currentAreaTransition,
                                    new PrioritySelector(
                                        new Decorator(ret => LokiPoe.ObjectManager.Objects.OfType<AreaTransition>().First().Distance > 20,
                                            CommonBehaviors.MoveTo(ret => LokiPoe.ObjectManager.Objects.OfType<AreaTransition>().First().Position, ret => "moving to area transition: " + LokiPoe.ObjectManager.Objects.OfType<AreaTransition>().First().Name, 3)
                                            ),
                                        new Decorator(ret => LokiPoe.ObjectManager.Objects.OfType<AreaTransition>().First().Distance <= 15,
                                            new PrioritySelector(
                                                new Decorator(ret => !newInstance,
                                                    new PrioritySelector(
                                                        new Decorator(ret => !TheVariables.activateInstanceTimer.IsRunning,
                                                            new Sequence(
                                                                new Action(ret => ExileBoxer.Log.Debug("starting activateInstanceTimer")),
                                                                new Action(ret => TheVariables.activateInstanceTimer.Start()),
                                                                new Action(ret => TheVariables.globalTimer.Reset())
                                                                )
                                                            ),
                                                        new Decorator(ret => TheVariables.activateInstanceTimer.IsRunning && TheVariables.activateInstanceTimer.ElapsedMilliseconds > 765,
                                                            new PrioritySelector(
                                                                new Decorator(ret => LokiPoe.Gui.IsInstanceManagerOpen,
                                                                    new Sequence(
                                                                        new Action(ret => ExileBoxer.Log.Debug("something went wrong! ERROR: MoveToAndTakeAreaTransition#!newInstance")),
                                                                        new Action(ret => TheVariables.globalTimer.Reset())
                                                                        )
                                                                    ),
                                                                new Decorator(ret => !LokiPoe.Gui.IsInstanceManagerOpen,
                                                                    new Sequence(
                                                                        new Action(ret =>
                                                                        {
                                                                            ExileBoxer.Log.Debug("PUSHEDX SAYS LOG THIS: " + LokiPoe.ObjectManager.Objects.OfType<AreaTransition>().First().Interact(false, false, LokiPoe.InputTargetType.MustMatchTarget));
                                                                            ExileBoxer.Log.Debug("taking transition " + LokiPoe.ObjectManager.Objects.OfType<AreaTransition>().First().Name);
                                                                        }),
                                                                        new Action(ret => TheVariables.activateInstanceTimer.Reset()),
                                                                        new Action(ret => TheVariables.globalTimer.Reset())
                                                                        )
                                                                    )
                                                                )
                                                            )
                                                        )
                                                    ),
                                                new Decorator(ret => newInstance,
                                                    new PrioritySelector(
                                                        new Decorator(ret => !TheVariables.activateInstanceTimer.IsRunning,
                                                            new Sequence(
                                                                new Action(ret => ExileBoxer.Log.Debug("starting activateInstanceTimer")),
                                                                new Action(ret => TheVariables.activateInstanceTimer.Start()),
                                                                new Action(ret => TheVariables.globalTimer.Reset())
                                                                )
                                                            ),
                                                        new Decorator(ret => TheVariables.activateInstanceTimer.IsRunning && TheVariables.activateInstanceTimer.ElapsedMilliseconds > 765,
                                                            new PrioritySelector(
                                                                new Decorator(ret => LokiPoe.Gui.IsInstanceManagerOpen,
                                                                    new PrioritySelector(
                                                                        new Decorator(ret => !TheVariables.activateInstanceManagerTimer.IsRunning,
                                                                            new Sequence(
                                                                                new Action(ret => TheVariables.activateInstanceManagerTimer.Start()),
                                                                                new Action(ret => TheVariables.globalTimer.Reset())
                                                                                )
                                                                            ),
                                                                        new Decorator(ret => TheVariables.activateInstanceManagerTimer.IsRunning && TheVariables.activateInstanceManagerTimer.ElapsedMilliseconds > 1645,
                                                                            new Sequence(
                                                                                new Action(ret => LokiPoe.Gui.InstanceManager.JoinNew()),
                                                                                new Action(ret => TheVariables.activateInstanceManagerTimer.Reset()),
                                                                                new Action(ret => TheVariables.globalTimer.Reset())
                                                                                )
                                                                            )
                                                                        )
                                                                    ),
                                                                new Decorator(ret => !LokiPoe.Gui.IsInstanceManagerOpen,
                                                                    new Sequence(
                                                                        new Action(ret => ExileBoxer.Log.Debug("opening InstanceManager for: " + LokiPoe.ObjectManager.Objects.OfType<AreaTransition>().First().Name)),
                                                                        new Action(ret => TheVariables.activateInstanceTimer.Reset()),
                                                                        new Action(ret => LokiPoe.ObjectManager.Objects.OfType<AreaTransition>().First().Interact(true, false, LokiPoe.InputTargetType.MustMatchTarget)),
                                                                        new Action(ret => TheVariables.globalTimer.Reset())
                                                                        )
                                                                    )
                                                                )
                                                            )
                                                        )
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                );
        }
        public static Composite MoveToAndTakeIslandTransition()
        {
            try
            {
                if (LokiPoe.ObjectManager.Objects.OfType<AreaTransition>().Count() < 1)
                    return new Action(ret => BotMain.Stop("islandTransition error"));

            }
            catch (Exception e)
            {
                ExileBoxer.Log.Debug(e.ToString());
                BotMain.Stop("!!!errör!!!");
            }

            return new PrioritySelector(
                new Decorator(ret => !TheVariables.globalTimer.IsRunning,
                    new Action(ret => TheVariables.globalTimer.Start())
                    ),
                new Decorator(ret => TheVariables.globalTimer.IsRunning && TheVariables.globalTimer.ElapsedMilliseconds > 100,
                    new PrioritySelector(
                        new Decorator(ret => TheVariables.takeNearestIslandTransitionOldPosition.X > 0 && TheVariables.takeNearestIslandTransitionOldPosition.Distance(LokiPoe.ObjectManager.Me.Position) >= 50,
                            new Sequence(
                                new Action(ret => ExileBoxer.Log.Debug("oldPos.Distance = " + TheVariables.takeNearestIslandTransitionOldPosition.Distance(LokiPoe.ObjectManager.Me.Position))),
                                new Action(ret => ExileBoxer.Log.Debug("successfully took island transition, PAUSE is true now. Press button to re-enable!")),
                                new Action(ret => TheVariables.takeNearestIslandTransitionOldPosition = new Vector2i(0,0)),
                                new Action(ret => TheVariables.takeNearestIslandTransition = false),
                                new Action(ret => TheVariables.activateInstanceTimer.Reset()),
                                new Action(ret => TheVariables.globalTimer.Reset()),
                                new Action(ret => TheVariables.PAUSE = true)
                                )
                            ),
                        new Decorator(ret => TheVariables.takeNearestIslandTransitionOldPosition.X <= 0 || TheVariables.takeNearestIslandTransitionOldPosition.Distance(LokiPoe.ObjectManager.Me.Position) < 50,
                            new PrioritySelector(
                                new Decorator(ret => LokiPoe.ObjectManager.Objects.OfType<AreaTransition>().First().Distance > 20,
                                    CommonBehaviors.MoveTo(ret => LokiPoe.ObjectManager.Objects.OfType<AreaTransition>().First().Position, ret => "moving to island transition: " + LokiPoe.ObjectManager.Objects.OfType<AreaTransition>().First().Name, 3)
                                    ),
                                new Decorator(ret => LokiPoe.ObjectManager.Objects.OfType<AreaTransition>().First().Distance <= 15,
                                    new PrioritySelector(
                                        new Decorator(ret => !TheVariables.activateInstanceTimer.IsRunning,
                                            new Sequence(
                                                new Action(ret => ExileBoxer.Log.Debug("starting activateInstanceTimer")),
                                                new Action(ret => TheVariables.activateInstanceTimer.Start()),
                                                new Action(ret => TheVariables.globalTimer.Reset())
                                                )
                                            ),
                                        new Decorator(ret => TheVariables.activateInstanceTimer.IsRunning && TheVariables.activateInstanceTimer.ElapsedMilliseconds > 765,
                                            new PrioritySelector(
                                                new Decorator(ret => LokiPoe.Gui.IsInstanceManagerOpen,
                                                    new Sequence(
                                                        new Action(ret => ExileBoxer.Log.Debug("something went wrong! ERROR: MoveToAndTakeAreaTransition#!newInstance")),
                                                        new Action(ret => TheVariables.globalTimer.Reset())
                                                        )
                                                    ),
                                                new Decorator(ret => TheVariables.takeNearestIslandTransitionOldPosition.X <= 0,
                                                    new Sequence(
                                                        new Action(ret => TheVariables.takeNearestIslandTransitionOldPosition = LokiPoe.ObjectManager.Me.Position),
                                                        new Action(ret => ExileBoxer.Log.Debug("set oldPos = " + TheVariables.takeNearestIslandTransitionOldPosition)),
                                                        new Action(ret => TheVariables.globalTimer.Reset())
                                                    )
                                                ),
                                                new Decorator(ret => !LokiPoe.Gui.IsInstanceManagerOpen,
                                                    new Sequence(
                                                        new Action(ret =>
                                                        {
                                                            ExileBoxer.Log.Debug("PUSHEDX SAYS LOG THIS: " + LokiPoe.ObjectManager.Objects.OfType<AreaTransition>().First().Interact(false, false, LokiPoe.InputTargetType.MustMatchTarget));
                                                            ExileBoxer.Log.Debug("taking transition " + LokiPoe.ObjectManager.Objects.OfType<AreaTransition>().First().Name);
                                                        }),
                                                        new Action(ret => TheVariables.activateInstanceTimer.Reset()),
                                                        new Action(ret => TheVariables.globalTimer.Reset())
                                                        )
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            )


                        
                           
                        )
                    )
                );
        }
    }
}
