using System.Threading.Tasks;
using EditorScripts;
using Helpers.ScriptableObjects;
using Manager;
using Sirenix.OdinInspector;
using Structs;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities.Static;

[DefaultExecutionOrder(-1000)] // ensure this runs before any other scripts
public class BootLoader : MonoBehaviour
{
    public static bool ForceNewGame; // set before scene load

    [SerializeField] bool useOverrideSpawnInfo;
    [SerializeField] bool playWithTutorialOn;
    [SerializeField] bool playWithDevToolsOn;
    [SerializeField] bool restartFromBeginning;

    [SerializeField] [InlineProperty] [HideLabel]
    SpawnInfoEditor overrideSpawnInfo;

    async void Awake()
    {
        await ConductBootLoad();
    }

    public async Task ConductBootLoad()
    {
        SpawnRegistry.Init();

        DontDestroyOnLoad(gameObject); // survive until we unload Boot

        // Load persistent managers first so SaveManager/PlayerSpawnManager exist
        await SceneManager.LoadSceneAsync("Core", LoadSceneMode.Additive);
        await SceneManager.LoadSceneAsync("Actors", LoadSceneMode.Additive);
        await SceneManager.LoadSceneAsync("DialogueScene", LoadSceneMode.Additive);
        await SceneManager.LoadSceneAsync("Overseer", LoadSceneMode.Additive);
        if (playWithTutorialOn) await SceneManager.LoadSceneAsync("Tutorial", LoadSceneMode.Additive);
        // if (playWithDevToolsOn) await SceneManager.LoadSceneAsync("DevTools", LoadSceneMode.Additive);

        SpawnRegistry.Init();


        var config = new SaveManagerConfig();
        if (!playWithTutorialOn)
            config.DisabledGlobalManagers.Add(GlobalManagerType.TutorialSave);

        // honor either the title screen PlayerPrefs flag or the editor toggle
        if (ForceNewGame || restartFromBeginning)
        {
            config.ForceReset = true;
            ForceNewGame = false; // reset static for next time
        }

        SaveManager.Instance.ApplyConfig(config);
        if (config.ForceReset)
            // actually perform the reset now so spawn selection sees a "fresh" state
            SaveManager.Instance.ResetGameSave(); // wipes managers/saves:contentReference[oaicite:5]{index=5}
        // SaveManager.Instance.SaveAll(); // persist the clean state:contentReference[oaicite:6]{index=6}
        // --- now decide where to spawn ---

        SpawnInfo info;

        if (useOverrideSpawnInfo)
        {
            info = overrideSpawnInfo.ToSpawnInfo();
        }
        else
        {
            // After a reset, PlayerSpawnManager will have written a default spawn.
            // So HasSave / LoadSlot will return that default spawn (good).
            if (!config.ForceReset &&
                PlayerSpawnManager.Instance.HasSave()) // uses ES3 existence check:contentReference[oaicite:7]{index=7}
                info = PlayerSpawnManager.Instance
                    .LoadSlot(); // returns last (or default) spawn:contentReference[oaicite:8]{index=8}
            else
                info = new SpawnInfo
                {
                    SceneName = "Overworld",
                    Mode = GameMode.DirigibleFlight,
                    SpawnPointId = "EnterValleySpawn"
                };
        }

        // prefer async/await style to mix nicely with the rest of Awake()
        await SpawnSystem.LoadAndSpawnAsync(info); // see §2


        SaveManager.Instance.LoadAll();


        // 5) tidy up – Boot scene no longer needed
        SceneManager.UnloadSceneAsync("Boot");
    }
}
