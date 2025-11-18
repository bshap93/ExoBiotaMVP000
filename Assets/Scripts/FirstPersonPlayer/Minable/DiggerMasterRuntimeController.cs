using Digger.Modules.Runtime.Sources;
using Manager.Global;
using UnityEngine;

public class DiggerMasterRuntimeController : MonoBehaviour
{
    private DiggerMasterRuntime _diggerMasterRuntime;

    private void OnEnable()
    {
        _diggerMasterRuntime = GetComponent<DiggerMasterRuntime>();
        // There should be only one DiggerMasterRuntime in the scene.
        TerrainManager.Instance.currentDiggerMasterRuntime = _diggerMasterRuntime;
    }
}