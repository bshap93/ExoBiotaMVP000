using MoreMountains.Feedbacks;
using NodeCanvas.Framework;
using OccaSoftware.ResponsiveSmokes.Runtime;
using ParadoxNotion.Design;
using UnityEngine;

namespace Utilities.NodeCanvas.ActionTasks
{
    [Category("BioOrganism")]
    public class PuffGasActionTask : ActionTask
    {
        public InteractiveSmoke smoke;
        public MMFeedbacks releaseFeedbacks;

        private bool _hazardActive;

        protected override void OnExecute()
        {
            ReleaseGas();
        }

        private void ReleaseGas()
        {
            releaseFeedbacks?.PlayFeedbacks();

            if (smoke)
            {
                smoke.gameObject.SetActive(true);
                smoke.Smoke();
                _hazardActive = true;
            }
            else
            {
                // Fallback: hazard ends immediately
                _hazardActive = false;
            }

            EndAction(true);
        }
    }
}