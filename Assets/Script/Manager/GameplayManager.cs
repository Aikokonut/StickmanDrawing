using System.Collections.Generic;
using UnityEngine;

namespace Stickman
{
    public class GameplayManager : MonoBehaviour
    {
        public static GameplayManager Instance { get; private set; }

        [Header("References")]
        public StickmanController player;
        public List<EnemyController> Enemies;
        public List<BuffController> Buffs;

        [Header("UI")]
        public GameObject winPanel;
        public GameObject losePanel;

        [Header("Drawing")]
        public float targetHitRadius = 0.6f;

        int aliveEnemies;
        bool ended;

        void Awake()
        {
            Instance = this;
        }
        void Start()
        {
            aliveEnemies = 0;
            if (Enemies != null)
            {
                for (int i = 0; i < Enemies.Count; i++)
                {
                    EnemyController e = Enemies[i];
                    if (e == null) continue;
                    aliveEnemies++;
                    e.OnDied += OnEnemyDied;
                }
            }

            if (player != null)
            {
                player.OnDied += OnPlayerDied;
                if (player.DrawLine != null) player.DrawLine.PathFinalizer = TrimPath;
            }

            if (winPanel != null) winPanel.SetActive(false);
            if (losePanel != null) losePanel.SetActive(false);

            if (aliveEnemies == 0) Win();
        }

        readonly HashSet<int> _seenTargets = new HashSet<int>();

        int TrimPath(IReadOnlyList<Vector2> path)
        {
            _seenTargets.Clear();
            int lastNewEntry = 0;
            int prevIdx = -1;

            for (int i = 1; i < path.Count; i++)
            {
                int idx = FindTargetIndexAt(path[i]);
                if (idx >= 0 && idx != prevIdx && _seenTargets.Add(idx))
                    lastNewEntry = i;
                prevIdx = idx;
            }

            return lastNewEntry == 0 ? 0 : lastNewEntry + 1;
        }

        public bool IsHit(Vector2 pos, GameObject target)
        {
            if (target == null) return false;
            float r2 = targetHitRadius * targetHitRadius;
            return ((Vector2)target.transform.position - pos).sqrMagnitude <= r2;
        }

        int FindTargetIndexAt(Vector2 world)
        {
            float r2 = targetHitRadius * targetHitRadius;
            int enemyCount = Enemies != null ? Enemies.Count : 0;

            if (Enemies != null)
            {
                for (int i = 0; i < enemyCount; i++)
                {
                    EnemyController e = Enemies[i];
                    if (e == null || !e.IsAlive) continue;
                    if (((Vector2)e.transform.position - world).sqrMagnitude <= r2) return i;
                }
            }

            if (Buffs != null)
            {
                for (int i = 0; i < Buffs.Count; i++)
                {
                    BuffController b = Buffs[i];
                    if (b == null || b.Taken) continue;
                    if (((Vector2)b.transform.position - world).sqrMagnitude <= r2) return enemyCount + i;
                }
            }

            return -1;
        }

        void OnEnemyDied(EnemyController _)
        {
            aliveEnemies--;
            if (aliveEnemies <= 0) Win();
        }

        void OnPlayerDied(StickmanController _) => Lose();

        void Win()
        {
            if (ended) return;
            ended = true;
            Debug.Log("[Game] WIN — all enemies are dead");
            if (winPanel != null) winPanel.SetActive(true);
        }

        void Lose()
        {
            if (ended) return;
            ended = true;
            Debug.Log("[Game] LOOSE — player is dead");
            if (losePanel != null) losePanel.SetActive(true);
        }
    }
}
