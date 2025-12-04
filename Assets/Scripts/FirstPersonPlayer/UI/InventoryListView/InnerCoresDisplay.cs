using FirstPersonPlayer.Tools.ItemObjectTypes.CompositeObjects;
using Inventory;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using SharedUI.Interact;
using SharedUI.Inventory;
using UnityEngine;

namespace FirstPersonPlayer.UI.InventoryListView
{
    public class InnerCoresDisplay : MonoBehaviour, MMEventListener<MMInventoryEvent>
    {
        [SerializeField] GradeCoresUILVRow standardCoreRow;
        [SerializeField] GradeCoresUILVRow radiantCoreRow;
        [SerializeField] GradeCoresUILVRow stellarCoreRow;
        [SerializeField] GradeCoresUILVRow unreasonableCoreRow;

        [SerializeField] bool condensedView;

        [SerializeField] GatedLevelingUIController gatedLevelingUIController;


        void Start()
        {
            Refresh();
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(MMInventoryEvent eventType)
        {
            if (eventType.TargetInventoryName != GlobalInventoryManager.InnerCoresInventoryName) return;
            if (eventType.InventoryEventType == MMInventoryEventType.ContentChanged) Refresh();
        }

        public void Refresh()
        {
            var numStandard = GlobalInventoryManager.Instance.GetNumberOfInnerCoresInInventory(
                HarvestableInnerObject.InnerObjectValueGrade.StandardGrade);

            var numRadiant = GlobalInventoryManager.Instance.GetNumberOfInnerCoresInInventory(
                HarvestableInnerObject.InnerObjectValueGrade.Radiant);

            var numStellar = GlobalInventoryManager.Instance.GetNumberOfInnerCoresInInventory(
                HarvestableInnerObject.InnerObjectValueGrade.Stellar);

            var numUnreasonable = GlobalInventoryManager.Instance.GetNumberOfInnerCoresInInventory(
                HarvestableInnerObject.InnerObjectValueGrade.Unreasonable);

            // var numExotic = GlobalInventoryManager.Instance.GetNumberOfInnerCoresInInventory(
            //     HarvestableInnerObject.InnerObjectValueGrade.MiscExotic);

            standardCoreRow.Initialize(HarvestableInnerObject.InnerObjectValueGrade.StandardGrade, numStandard);
            radiantCoreRow.Initialize(HarvestableInnerObject.InnerObjectValueGrade.Radiant, numRadiant);
            stellarCoreRow.Initialize(HarvestableInnerObject.InnerObjectValueGrade.Stellar, numStellar);
            unreasonableCoreRow.Initialize(HarvestableInnerObject.InnerObjectValueGrade.Unreasonable, numUnreasonable);

            if (condensedView)
            {
                if (numStandard == 0) standardCoreRow.gameObject.SetActive(false);
                else standardCoreRow.gameObject.SetActive(true);

                if (numRadiant == 0) radiantCoreRow.gameObject.SetActive(false);
                else radiantCoreRow.gameObject.SetActive(true);

                if (numStellar == 0) stellarCoreRow.gameObject.SetActive(false);
                else stellarCoreRow.gameObject.SetActive(true);

                if (numUnreasonable == 0) unreasonableCoreRow.gameObject.SetActive(false);
                else unreasonableCoreRow.gameObject.SetActive(true);
            }
            else
            {
                if (numStandard == 0) standardCoreRow.convertToXPButton.gameObject.SetActive(false);
                else standardCoreRow.convertToXPButton.gameObject.SetActive(true);

                if (numRadiant == 0) radiantCoreRow.convertToXPButton.gameObject.SetActive(false);
                else radiantCoreRow.convertToXPButton.gameObject.SetActive(true);

                if (numStellar == 0) stellarCoreRow.convertToXPButton.gameObject.SetActive(false);
                else stellarCoreRow.convertToXPButton.gameObject.SetActive(true);

                if (numUnreasonable == 0) unreasonableCoreRow.convertToXPButton.gameObject.SetActive(false);
                else unreasonableCoreRow.convertToXPButton.gameObject.SetActive(true);
            }
        }
    }
}
