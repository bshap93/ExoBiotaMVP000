using System;
using FirstPersonPlayer.Interface;
using Manager;
using MoreMountains.Feedbacks;
using SharedUI.Interface;
using UnityEngine;
using Utilities.Interface;

namespace FirstPersonPlayer.Interactable
{
    public class MediStatHub : MonoBehaviour, IRequiresUniqueID, IInteractable, IBillboardable
    {
        public string uniqueID;
        [SerializeField] AnimationClip openAnimation;
        [SerializeField] AnimationClip closeAnimation;
        [SerializeField] MMFeedbacks openFeedbacks;
        [SerializeField] MMFeedbacks closeFeedbacks;
        public string GetName()
        {
            return "Medi-Stat Hub";
        }
        public Sprite GetIcon()
        {
            return ExaminationManager.Instance.iconRepository.mediStatHubIcon;
        }
        public string ShortBlurb()
        {
            return "A rest station capable of bio-core augments.";
        }
        public Sprite GetActionIcon()
        {
            return ExaminationManager.Instance.iconRepository.mediStatHubRestIcon;
        }
        public string GetActionText()
        {
            return "Utilize";
        }
        public void Interact()
        {
            throw new NotImplementedException();
        }
        public void OnInteractionStart()
        {
        }
        public void OnInteractionEnd(string param)
        {
        }
        public bool CanInteract()
        {
            throw new NotImplementedException();
        }
        public bool IsInteractable()
        {
            return true;
        }
        public void OnFocus()
        {
        }
        public void OnUnfocus()
        {
        }
        public string UniqueID => uniqueID;

        public void SetUniqueID()
        {
            uniqueID = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(uniqueID);
        }
    }
}
