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
                        new Action(ret => TheVariables.moveToMiddleOfTown = true),
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
                                CommonBehaviors.MoveTo(ret => TheVariables.town2middle, ret => "", 20),
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
                                CommonBehaviors.MoveTo(ret => TheVariables.town2middle, ret => "", 20),
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
                                CommonBehaviors.MoveTo(ret => TheVariables.town3middle, ret => "", 20),
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
            AreaTransition at = null;
            
            try
            {
                //at = TheVariables.availableAreaTransitions.First(w => w.Name == TheVariables.takeAreaTransition);
                at = LokiPoe.ObjectManager.Objects.OfType<AreaTransition>().FirstOrDefault();
            }
            catch(Exception e)
            {
                ExileBoxer.Log.Debug(e.ToString());
                BotMain.Stop("errör");
            }

            if (at == null)
                return new Sequence(
                    new Action(ret => ExileBoxer.Log.Debug("failed miserably")),
                    new Action(ret => ExileBoxer.Log.Debug("### " + LokiPoe.ObjectManager.Objects.OfType<AreaTransition>().FirstOrDefault().Name)),
                    new Action(ret => TheVariables.takeAreaTransition = "")
                    );

            return new PrioritySelector(
                new Decorator(ret => at.Name == TheVariables.areaNameMe,
                    new Sequence(
                        new Action(ret => TheVariables.takeAreaTransition = string.Empty),
                        new Action(ret => at = null),
                        new Action(ret => ExileBoxer.Log.Debug("successfully took area transition! congratz!"))
                        //new Action(ret => Navigator.PlayerMover.Stop())
                        //should add a move to townmiddle, if we entered a town this way...
                        )
                    ),
                new Decorator(ret => at.Name != TheVariables.areaNameMe,
                    new PrioritySelector(
                        new Decorator(ret => at.Distance > 20,
                            CommonBehaviors.MoveTo(ret => at.Position, ret => "moving to area transition: " + at.Name, 5)
                            ),
                        new Decorator(ret => at.Distance <= 10,
                            new PrioritySelector(
                                new Decorator(ret => !newInstance,
                                    new PrioritySelector(
                                        new Decorator(ret => !TheVariables.activateInstanceTimer.IsRunning,
                                            new Sequence(
                                                new Action(ret => ExileBoxer.Log.Debug("starting activateInstanceTimer")),
                                                new Action(ret => TheVariables.activateInstanceTimer.Start())
                                                )
                                            ),
                                        new Decorator(ret => TheVariables.activateInstanceTimer.IsRunning && TheVariables.activateInstanceTimer.ElapsedMilliseconds > 765,
                                            new PrioritySelector(
                                                new Decorator(ret => LokiPoe.Gui.IsInstanceManagerOpen,
                                                    new Sequence(
                                                        new Action(ret => ExileBoxer.Log.Debug("something went wrong! ERROR: MoveToAndTakeAreaTransition#!newInstance"))
                                                        //add a "clear gui()" pls...!
                                                        )
                                                    ),
                                                new Decorator(ret => !LokiPoe.Gui.IsInstanceManagerOpen,
                                                    new Sequence(
                                                        new Action(ret => ExileBoxer.Log.Debug("opening InstanceManager for: " + at.Name)),
                                                        new Action(ret => TheVariables.activateInstanceTimer.Reset()),
                                                        new Action(ret => at.Interact(false, false, LokiPoe.InputTargetType.MustMatchTarget))
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
                                                new Action(ret => TheVariables.activateInstanceTimer.Start())
                                                )
                                            ),
                                        new Decorator(ret => TheVariables.activateInstanceTimer.IsRunning && TheVariables.activateInstanceTimer.ElapsedMilliseconds > 765,
                                            new PrioritySelector(
                                                new Decorator(ret => LokiPoe.Gui.IsInstanceManagerOpen,
                                                    new PrioritySelector(
                                                        new Decorator(ret => !TheVariables.activateInstanceManagerTimer.IsRunning,
                                                            new Action(ret => TheVariables.activateInstanceManagerTimer.Start())
                                                            ),
                                                        new Decorator(ret => TheVariables.activateInstanceManagerTimer.IsRunning && TheVariables.activateInstanceManagerTimer.ElapsedMilliseconds > 1645,
                                                            new Sequence(
                                                                new Action(ret => LokiPoe.Gui.InstanceManager.JoinNew()),
                                                                new Action(ret => TheVariables.activateInstanceManagerTimer.Reset())
                                                                )
                                                            )
                                                        )
                                                    ),
                                                new Decorator(ret => !LokiPoe.Gui.IsInstanceManagerOpen,
                                                    new Sequence(
                                                        new Action(ret => ExileBoxer.Log.Debug("opening InstanceManager for: " + at.Name)),
                                                        new Action(ret => TheVariables.activateInstanceTimer.Reset()),
                                                        new Action(ret => at.Interact(true, false, LokiPoe.InputTargetType.MustMatchTarget))
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
