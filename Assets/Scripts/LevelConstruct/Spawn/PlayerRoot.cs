using Manager.Global;
using UnityEngine;

public class PlayerRoot : MonoBehaviour
{
    private void Awake()
    {
        GameStateManager.Instance.RegisterPlayerRoot(transform);
    }
}