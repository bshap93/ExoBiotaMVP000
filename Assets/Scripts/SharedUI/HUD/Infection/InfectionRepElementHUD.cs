using Manager.Status;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SharedUI.HUD.Infection
{
    public class InfectionRepElementHUD : MonoBehaviour
    {
        [SerializeField] Image infectionRepImage;
        [SerializeField] TMP_Text infectionNameText;
        [SerializeField] MinutesTillNextInfectionPb infectionRepProgressBar;
        public void SetNewInfection(InfectionManager.OngoingInfection infectionInfo)
        {
            infectionNameText.text = infectionInfo.infectionName;
            infectionRepProgressBar.UpdateUI(infectionInfo.progressionTowardSupplantation, 1f);
            switch (infectionInfo.infectionID)
            {
            }
        }
    }
}
