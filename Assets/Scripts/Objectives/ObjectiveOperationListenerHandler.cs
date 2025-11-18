using Events;
using MoreMountains.Tools;
using UnityEngine;

namespace Objectives
{
    public class ObjectiveOperationListenerHandler : MonoBehaviour, MMEventListener<ObjectiveEvent>
    {
        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }


        public void OnMMEvent(ObjectiveEvent e)
        {
        }
    }
}
