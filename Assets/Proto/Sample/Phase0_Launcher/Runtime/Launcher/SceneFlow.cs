using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Proto.Launcher
{
    public enum GenreId
    {
        None = 0,
        Turn3d = 1,
        TD = 2,
        RailShooter = 3
    }

    [DisallowMultipleComponent]
    public sealed class SceneFlow : MonoBehaviour
    {
        private const string SceneLauncher = "Proto_Launcher";
        private const string SceneTurn3d = "Proto_Turn3d";
        private const string SceneTd = "Proto_TD";
        private const string SceneRailShooter = "Proto_RailShooter";

        private static SceneFlow s_instance;

        public static SceneFlow Instance => s_instance;
        public GenreId CurrentGenre { get; private set; } = GenreId.None;
        public bool IsLoading { get; private set; }

        public event Action<GenreId> GenreLoadStarted;
        public event Action<GenreId> GenreLoadCompleted;
        public event Action ReturnedToLauncher;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (s_instance != null) return;

            var go = new GameObject(nameof(SceneFlow));
            s_instance = go.AddComponent<SceneFlow>();
            DontDestroyOnLoad(go);
        }

        private void Awake()
        {
            if (s_instance != null && s_instance != this)
            {
                Debug.LogWarning($"[SceneFlow] Duplicate instance detected on '{name}'. Destroying.", this);
                Destroy(gameObject);
                return;
            }

            s_instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadGenre(GenreId genre)
        {
            if (IsLoading)
            {
                Debug.LogWarning($"[SceneFlow] Load already in progress (requested={genre}, current={CurrentGenre}).");
                return;
            }

            string sceneName = GenreToScene(genre);
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError($"[SceneFlow] Unknown genre '{genre}'. Load aborted.");
                return;
            }

            IsLoading = true;
            GenreLoadStarted?.Invoke(genre);

            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (op == null)
            {
                Debug.LogError($"[SceneFlow] LoadSceneAsync returned null for '{sceneName}'. Check EditorBuildSettings.");
                IsLoading = false;
                return;
            }

            op.completed += _ =>
            {
                CurrentGenre = genre;
                IsLoading = false;
                GenreLoadCompleted?.Invoke(genre);
            };
        }

        public void ReturnToLauncher()
        {
            if (IsLoading) return;

            IsLoading = true;
            var op = SceneManager.LoadSceneAsync(SceneLauncher, LoadSceneMode.Single);
            if (op == null)
            {
                Debug.LogError("[SceneFlow] Launcher scene not registered in EditorBuildSettings.");
                IsLoading = false;
                return;
            }

            op.completed += _ =>
            {
                CurrentGenre = GenreId.None;
                IsLoading = false;
                ReturnedToLauncher?.Invoke();
            };
        }

        private static string GenreToScene(GenreId genre)
        {
            switch (genre)
            {
                case GenreId.Turn3d: return SceneTurn3d;
                case GenreId.TD: return SceneTd;
                case GenreId.RailShooter: return SceneRailShooter;
                default: return null;
            }
        }
    }
}
