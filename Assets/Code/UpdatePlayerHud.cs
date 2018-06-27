using System;
using System.Collections;
using System.Linq;
using System.Timers;
using Boo.Lang;
using Unity.Collections;
using Unity.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AsteroidsArcadeClone
{
    [AlwaysUpdateSystem, UpdateAfter(typeof(CollisionSystem))]
    public class UpdatePlayerHUD : ComponentSystem
    {
        struct PlayerData
        {
            public int Length;
            [ReadOnly] public EntityArray Entity;
            [ReadOnly] public ComponentDataArray<PlayerScore> Score;
            [ReadOnly] public ComponentDataArray<PlayerInput> Input;
            [ReadOnly] public ComponentDataArray<Health> Health;
        }

        struct EnemyData
        {
            public int Length;
            [ReadOnly] public EntityArray Entity;
            [ReadOnly] public ComponentDataArray<Enemy> Enemy;
        }

        struct EnemySpawner
        {
            public int Length;
            public ComponentDataArray<EnemySpawnSystemState> Spawner;
        }

        [Inject] EnemySpawner m_EnemySpawner;
        [Inject] PlayerData m_Players;
        [Inject] EnemyData m_Enemies;

        private Button NewGameButton;
        private Button RestartButton;
        private Button ExitButton;

        private Text HealthText;
        private Text TimerText;
        private Text EnemiesText;
        private Text ScoreText;
        private Text GameOverText;
        private Text HighscoresText;

        private Highscores highscores;
        private int currentScore;

        [Serializable]
        private struct Highscores
        {
            [SerializeField] private int[] _values;

            public int[] values => _values;

            public void Update(int value)
            {
                if (_values == null)
                {
                    _values = new int[0];
                }

                if (value == 0) return;

                var list = _values.ToList();
                list.Add(value);
                _values = list.ToArray();
                PlayerPrefs.SetString("Highscores", JsonUtility.ToJson(this));
            }

            public static Highscores Load()
            {
                if (PlayerPrefs.HasKey("Highscores"))
                {
                    string hs = PlayerPrefs.GetString("Highscores");
                    return JsonUtility.FromJson<Highscores>(hs);
                }

                return default(Highscores);
            }
        }

        public static State state = State.NewGame;

        public enum State
        {
            NewGame,
            Playing,
            GameOver
        }

        public void SetupGameObjects()
        {
            NewGameButton = GameObject.Find("NewGameButton").GetComponent<Button>();
            RestartButton = GameObject.Find("RestartButton").GetComponent<Button>();
            ExitButton = GameObject.Find("ExitButton").GetComponent<Button>();
            TimerText = GameObject.Find("TimerText").GetComponent<Text>();
            HealthText = GameObject.Find("HealthText").GetComponent<Text>();
            EnemiesText = GameObject.Find("EnemiesText").GetComponent<Text>();
            ScoreText = GameObject.Find("ScoreText").GetComponent<Text>();
            GameOverText = GameObject.Find("GameOverText").GetComponent<Text>();
            HighscoresText = GameObject.Find("HighscoresText").GetComponent<Text>();

            NewGameButton.onClick.AddListener(AsteroidsArcadeBootstrap.NewGame);
            RestartButton.onClick.AddListener(AsteroidsArcadeBootstrap.RestartGame);
            ExitButton.onClick.AddListener(() =>
            {
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                World.Active.Dispose();
                Application.Quit();
#endif
            });

            highscores = Highscores.Load();
        }

        private void UpdateTimeToSpawn()
        {
            var showTimer = m_Players.Length > 0 && m_EnemySpawner.Length > 0;
            TimerText.gameObject.SetActive(showTimer);
            if (showTimer)
            {
                TimerText.text =
                    $"Incoming in: {m_EnemySpawner.Spawner[0].delay - m_EnemySpawner.Spawner[0].counter:F}s";
            }
        }

        private void UpdateState()
        {
            if (state == State.Playing)
            {
                var playersLost = m_Players.Length < 1;
                var playerWon = m_Enemies.Length < 1 && m_EnemySpawner.Length < 1;

                if (playersLost || playerWon)
                {
                    if (playerWon)
                    {
                        GameOverText.color = Color.green;
                        GameOverText.text = "YOU WON !!!";
                    }
                    else
                    {
                        GameOverText.color = Color.red;
                        GameOverText.text = "YOU DIED !!!";
                    }

                    //destroy players
                    for (int i = 0; i < m_Players.Length; i++)
                    {
                        PostUpdateCommands.DestroyEntity(m_Players.Entity[i]);
                    }

                    //destroy enemies
                    for (int i = 0; i < m_Enemies.Length; i++)
                    {
                        PostUpdateCommands.DestroyEntity(m_Enemies.Entity[i]);
                    }

                    highscores.Update(currentScore);

                    state = State.GameOver;
                }
            }
        }

        private void UpdateGUI()
        {
            switch (state)
            {
                case State.NewGame:
                    //Enabled
                    NewGameButton.gameObject.SetActive(true);
                    HighscoresText.gameObject.SetActive(true);

                    //Disabled
                    HealthText.gameObject.SetActive(false);
                    EnemiesText.gameObject.SetActive(false);
                    ScoreText.gameObject.SetActive(false);
                    GameOverText.gameObject.SetActive(false);
                    RestartButton.gameObject.SetActive(false);
                    ExitButton.gameObject.SetActive(true);
                    TimerText.gameObject.SetActive(false);

                    UpdateHighscore();
                    Cursor.visible = true;
                    break;
                case State.Playing:
                    //Enabled
                    HealthText.gameObject.SetActive(true);
                    EnemiesText.gameObject.SetActive(true);
                    ScoreText.gameObject.SetActive(true);

                    //Disabled
                    NewGameButton.gameObject.SetActive(false);
                    GameOverText.gameObject.SetActive(false);
                    RestartButton.gameObject.SetActive(false);
                    HighscoresText.gameObject.SetActive(false);
                    ExitButton.gameObject.SetActive(false);

                    UpdateLives();
                    UpdateEnemyCount();
                    UpdatePlayerScore();
                    UpdateTimeToSpawn();
                    Cursor.visible = false;
                    break;
                case State.GameOver:

                    //Enabled
                    GameOverText.gameObject.SetActive(true);
                    HighscoresText.gameObject.SetActive(true);
                    RestartButton.gameObject.SetActive(true);
                    ExitButton.gameObject.SetActive(true);

                    //Disabled
                    NewGameButton.gameObject.SetActive(false);
                    HealthText.gameObject.SetActive(false);
                    EnemiesText.gameObject.SetActive(false);
                    ScoreText.gameObject.SetActive(false);
                    TimerText.gameObject.SetActive(false);

                    UpdateHighscore();
                    Cursor.visible = true;
                    break;
            }
        }

        protected override void OnUpdate()
        {
            UpdateState();
            UpdateGUI();
        }

        private void UpdateLives()
        {
            int displayedHealth = m_Players.Health[0].Value;
            HealthText.text = $"Lives: {displayedHealth}";
        }

        private void UpdatePlayerScore()
        {
            if (ScoreText != null)
            {
                if (m_Players.Length > 0)
                {
                    int displayedScore = m_Players.Score[0].Value;
                    currentScore = displayedScore;
                    ScoreText.gameObject.SetActive(true);
                    ScoreText.text = $"Score: {displayedScore}";
                }
                else
                {
                    ScoreText.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateEnemyCount()
        {
            if (EnemiesText != null)
            {
                if (m_Enemies.Length > 0)
                {
                    EnemiesText.gameObject.SetActive(true);
                    EnemiesText.text = $"Asteroids: {m_Enemies.Length}";
                }
                else
                {
                    EnemiesText.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateHighscore()
        {
            if (HighscoresText != null)
            {
                var text = "Highscores:";
                if (highscores.values != null)
                {
                    foreach (var hs in highscores.values.OrderByDescending(x => x))
                    {
                        text += $"\n{hs}";
                    }
                }

                HighscoresText.text = text;
            }
        }
    }
}