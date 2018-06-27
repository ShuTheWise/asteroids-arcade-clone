using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace AsteroidsArcadeClone
{
    public class AsteroidsArcadeSettings : MonoBehaviour
    {
        [Header("Player Movement")] [Tooltip("Accelecation or whatnot")]
        public float playerMoveSpeed = 15.0f;

        [Tooltip("Speed of player rotaring around his position")]
        public float playerLookSpeed = 15.0f;

        [Header("Player Stats")]  [Tooltip("Time in which you are invulnerable after you die")]
        public float playerInvulnerableCooldown = 1f;

        [Tooltip("Speed of player rotaring around his position")]
        public int playerLives = 3;

        [Tooltip("After that time passes you can fire another bullet")]
        public float playerFireCoolDown = 0.1f;

        [Tooltip("Don't assign a big value")] public float playerInteriaFade = 0.001f;
        public float playerCollisionRadius = 3.0f;

        [Tooltip("Particle effect, purely visual")]
        public GameObject playerThrustersPrefab;

        [Header("Player Shot Stats")] public float bulletTimeToLive = 2.0f;
        public float bulletMoveSpeed = 30.0f;


        [Header("Enemies stats")] [Tooltip("How many times you have to hit an enemy to kill him")]
        public int enemyHealth = 1;

        public int enemyCount = 15;
        public float enemyCollisionRadius = 2f;
        public float enemySpawnDelay = 3f;

        [Tooltip("Offset from outer edges of the screen")]
        public float enemySpawnPositionOffset = 5f;

        public float enemyMinSpeed = 2.0f;
        public float enemyMaxSpeed = 8.0f;
        [Tooltip("Applies only if enemy is outside of playfield")] public float enemyMaxLifetime = 40f;


        [Header("Enimies hit scores")] public int enemyBigAsteroidPoints = 3;
        public int enemyMediumAsteroidPoints = 2;
        public int enemySmallAsteroidPoints = 1;


        [Header("Other")] public Rect playfield = new Rect {x = -30.0f, y = -30.0f, width = 60.0f, height = 60.0f};
    }
#if UNITY_EDITOR

    [CustomEditor(typeof(AsteroidsArcadeSettings))]
    [CanEditMultipleObjects]
    public class LookAtPointEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.Label("Dear Game Design, I have gone through the trouble of writing some tooltips for you, just read them please -your programmer", EditorStyles.helpBox);
        }
    }
#endif
}