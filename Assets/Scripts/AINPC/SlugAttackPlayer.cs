using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace AINPC
{
    [Category("AttackMoves")]
    public class SlugAttackPlayer : ActionTask
    {
        // NodeCanvas Blackboard Parameters
        public BBParameter<float> attackDelay = 0.2f;

        EnemyController controller;
        public BBParameter<float> cooldownAfterAttack = 0.5f;
        bool hasAttacked;
        bool inCooldown;


        float timer;
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
            timer = attackDelay.value;
            hasAttacked = false;
            inCooldown = false;
        }

        //Called once per frame while the action is active.
        protected override void OnUpdate()
        {
            timer -= Time.deltaTime;

            // Delay phase
            if (!hasAttacked && timer <= 0f)
            {
                controller.StartAttack();
                hasAttacked = true;
            }

            // Wait for attack to finish
            if (hasAttacked && !inCooldown && !controller.IsAttacking)
            {
                timer = cooldownAfterAttack.value;
                inCooldown = true;
            }

            // Cooldown phase
            if (inCooldown && timer <= 0f) EndAction(true);
            // Wait until attack finishes
        }

        //Called when the task is disabled.
        protected override void OnStop()
        {
        }

        //Called when the task is paused.
        protected override void OnPause()
        {
        }

        enum AttackPhase
        {
            Delay,
            Attacking,
            Cooldown
        }
    }
}
