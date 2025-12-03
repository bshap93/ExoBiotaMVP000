using FirstPersonPlayer.Tools.ItemObjectTypes.CompositeObjects;
using Inventory;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
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
        }
    }
}
