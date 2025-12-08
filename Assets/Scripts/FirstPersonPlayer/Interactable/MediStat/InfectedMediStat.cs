using FirstPersonPlayer.Interface;
using Helpers.Events;
using LevelConstruct.Interactable.ItemInteractables;
using UnityEngine;
using Utilities.Interface;

namespace FirstPersonPlayer.Interactable.MediStat
{
    public class InfectedMediStat : ActionConsole, IRequiresUniqueID, IInteractable
    {
        public override void Interact()
        {
            AlertEvent.Trigger(
                AlertReason.PlayTestEndYesOrNo,
                "Use the Infected Medi-Stat to alter your brain to communicate with Sheolite? This will end the playtest.",
                "Infected Medi-Stat", AlertType.ChoiceModal, onConfirm: () => QuitGame());
        }


        public override void OnInteractionStart()
        {
        }

        void QuitGame()
        {
            Application.Quit();
        }
        public override void OnInteractionEnd()
        {
        }
        protected override string GetActionText(bool recognizableOnSight)
        {
            return "Utilize";
        }
        public override void SetConsoleToLacksPowerState()
        {
            currentConsoleState = ActionConsoleState.LacksPower;
        }
        public override void SetConsoleToPoweredOnState()
        {
            currentConsoleState = ActionConsoleState.PoweredOn;
        }
        public override void SetConsoleToHailPlayerState()
        {
        }
    }
}
