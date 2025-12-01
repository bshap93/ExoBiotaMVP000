using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace AINPC
{
    [Category("AttackMoves")]
    public class SlugAttackPlayer : ActionTask
    {
        EnemyController controller;
        
        public BBParameter<float> attackDelay = 0.2f;
        public BBParameter<float> cooldown = 1.2f;
        
        private float timer;
        private enum AttackPhase { Delay, Attacking, Cooldown }
        private AttackPhase phase;

        //Use for initialization. This is called only once in the lifetime of the task.
        //Return null if init was successfull. Return an error string otherwise
        protected override string OnInit()
        {
            controller = agent.GetComponent<EnemyController>();
            return controller ? null : "EnemyController component not found on the agent.";
        }

        //This is called once each time the task is enabled.
        //Call EndAction() to mark the action as finished, either in success or failure.
        //EndAction can be called from anywhere.
        protected override void OnExecute()
        {
            controller.StartAttack();
            Debug.Log("SlugAttackPlayer: OnExecute - Attack started.");
            phase = AttackPhase.Delay;

        }

        //Called once per frame while the action is active.
        protected override void OnUpdate()
        {
            timer -= Time.deltaTime;

            switch (phase)
            {
                case AttackPhase.Delay:
                    if (timer <= 0f)
                    {
                        controller.StartAttack();       // (A) Trigger animation
                        phase = AttackPhase.Attacking;
                    }
                    break;

                case AttackPhase.Attacking:
                    if (!controller.IsAttacking)        // (B) Wait until it finishes
                    {
                        timer = cooldown.value;         // (C) Enter cooldown
                        phase = AttackPhase.Cooldown;
                    }
                    break;

                case AttackPhase.Cooldown:
                    if (timer <= 0f)
                        EndAction(true);
                    break;
            }
            // if (!controller.IsAttacking) EndAction(true);
        }

        //Called when the task is disabled.
        protected override void OnStop()
        {
        }

        //Called when the task is paused.
        protected override void OnPause()
        {
        }
    }
}
