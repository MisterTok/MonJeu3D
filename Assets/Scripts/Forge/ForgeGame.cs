using UnityEngine;
using UnityEngine.SceneManagement;

// Point d'entrée : construit le jeu au lancement, quelle que soit la scène ouverte.
public class ForgeGame : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (FindAnyObjectByType<ForgeGame>() != null) return;
        // On masque le contenu de la scène modèle (caméra, lumière…) : tout est recréé par code.
        foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
            go.SetActive(false);
        new GameObject("ForgeGame").AddComponent<ForgeGame>();
    }

    void Awake()
    {
        Application.targetFrameRate = 60;
        Screen.orientation = ScreenOrientation.Portrait;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        GameState.Load();

        // Caméra de fond : efface tout l'écran (les autres caméras n'occupent que des bandes).
        var bg = new GameObject("Caméra fond").AddComponent<Camera>();
        bg.clearFlags = CameraClearFlags.SolidColor;
        bg.backgroundColor = new Color(0.02f, 0.006f, 0.005f);
        bg.cullingMask = 0;
        bg.depth = -10f;

        var world = ForgeWorld.Build();
        var battle = BattleWorld.Build();
        ForgeUI.Build(world, battle);
    }

#if UNITY_EDITOR
    // Outil de mise au point : capture d'écran en pleine résolution quand le fichier ../_dl/shot_request apparaît.
    float shotCheck;
    void Update()
    {
        shotCheck -= Time.unscaledDeltaTime;
        if (shotCheck > 0f) return;
        shotCheck = 0.5f;
        string dir = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../../_dl"));
        string req = System.IO.Path.Combine(dir, "shot_request");
        if (!System.IO.File.Exists(req)) return;
        string name = System.IO.File.ReadAllText(req).Trim();
        System.IO.File.Delete(req);
        if (string.IsNullOrEmpty(name)) name = "shot";
        if (name == "discard")
        {
            // Outil de test : jette la pièce en attente sans récompense.
            GameState.Data.pending = new Item();
            GameState.Save();
            FindAnyObjectByType<ForgeWorld>().DismissItem(false);
            FindAnyObjectByType<ForgeUI>().SendMessage("HidePopup");
            return;
        }
        if (name.StartsWith("item"))
        {
            // Outil de test : pièce d'un cercle donné (ex. « item6 »).
            int c = 6; int.TryParse(name.Substring(4), out c);
            var it = GameState.MakeItem(Random.Range(0, GameState.SlotCount), Mathf.Clamp(c, 0, 9), 12);
            GameState.Data.pending = it;
            FindAnyObjectByType<ForgeWorld>().ShowItemInstant(it);
            FindAnyObjectByType<ForgeUI>().SendMessage("ShowPopup", it);
            return;
        }
        if (name.StartsWith("forge")) { GameState.Data.hammers = Mathf.Max(GameState.Data.hammers, 1); FindAnyObjectByType<ForgeUI>()?.SendMessage("OnForge"); return; }
        ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(dir, name + ".png"));
    }
#endif

    void OnApplicationPause(bool paused) { if (paused) GameState.Save(); }
    void OnApplicationQuit() { GameState.Save(); }
}
