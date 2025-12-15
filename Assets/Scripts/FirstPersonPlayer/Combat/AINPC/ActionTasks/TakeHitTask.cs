using FirstPersonPlayer.Combat.AINPC.Creatures;
using MoreMountains.Feedbacks;
using NodeCanvas.Framework;
using UnityEngine;

namespace FirstPersonPlayer.Combat.AINPC.ActionTasks
{
    public class TakeHitTask : ActionTask
    {
        public readonly BBParameter<AnimationClip> HitAnimationClip = null;
        public readonly BBParameter<float> HitDelay = 0.5f;
        public readonly BBParameter<MMFeedbacks> HitFeedbacks = null;

        EnemyController _controller;

        bool _hasBeenHit;
        bool _inCooldown;
        
        float timer;

        protected override string OnInit()
        {
            _controller = agent.GetComponent<EnemyController>();
            return _controller ? null : "EnemyController component not found on the agent.";
        }

        protected override void OnExecute()
        {
            timer = HitDelay.value;
            _hasBeenHit = false;
            _inCooldown = false;
            _controller.PlayHitAnimation(HitAnimationClip.value);
            HitFeedbacks.value?.PlayFeedbacks();
        }
        
        protected override void OnUpdate()
        {
            timer -= Time.deltaTime;

            // Delay phase
            if (!_hasBeenHit && timer <= 0f)
            {
                _hasBeenHit = true;
            }

        }
    }
}
