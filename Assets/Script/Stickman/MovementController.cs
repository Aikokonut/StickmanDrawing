using System;
using System.Collections.Generic;
using UnityEngine;

namespace Stickman
{
    public class MovementController : MonoBehaviour
    {
        public float timeToMove = 2f;
        public float stopDuration = 1f;

        public bool faceMovementDirection = true;

        [HideInInspector] public StickmanController Owner;

        readonly List<Vector2> path = new List<Vector2>(256);
        readonly HashSet<EnemyController> handledEnemies = new HashSet<EnemyController>();
        readonly HashSet<BuffController> handledBuffs = new HashSet<BuffController>();

        int index;
        float speed;
        float pauseTimer;
        Action onCompleted;
        Transform target;

        public bool IsMoving { get; private set; }

        public void FollowPath(IReadOnlyList<Vector2> pathPoints, Action onComplete = null)
        {
            if (pathPoints == null || pathPoints.Count < 2)
            {
                IsMoving = false;
                onComplete?.Invoke();
                return;
            }

            target = (Owner != null) ? Owner.transform : transform;

            path.Clear();
            for (int i = 0; i < pathPoints.Count; i++) path.Add(pathPoints[i]);

            float total = 0f;
            for (int i = 1; i < path.Count; i++) total += Vector2.Distance(path[i - 1], path[i]);
            speed = total / Mathf.Max(0.01f, timeToMove);

            handledEnemies.Clear();
            handledBuffs.Clear();
            pauseTimer = 0f;
            index = 1;
            onCompleted = onComplete;
            IsMoving = true;

            SetPos(path[0]);
        }

        public void Stop()
        {
            IsMoving = false;
            path.Clear();
            handledEnemies.Clear();
            handledBuffs.Clear();
            onCompleted = null;
        }

        void Update()
        {
            if (!IsMoving || target == null) return;

            if (pauseTimer > 0f)
            {
                pauseTimer -= Time.deltaTime;
                return;
            }

            Vector2 pos = target.position;
            float step = speed * Time.deltaTime;

            while (step > 0f && index < path.Count)
            {
                Vector2 wp = path[index];
                Vector2 toWp = wp - pos;
                float dist = toWp.magnitude;

                if (dist <= step)
                {
                    pos = wp;
                    step -= dist;
                    index++;
                }
                else
                {
                    Vector2 dir = toWp / dist;
                    pos += dir * step;
                    step = 0f;
                    Face(dir.x);
                }
            }

            SetPos(pos);

            if (CheckEncounter(pos)) return;

            if (index >= path.Count)
            {
                IsMoving = false;
                var cb = onCompleted;
                onCompleted = null;
                path.Clear();
                handledEnemies.Clear();
                handledBuffs.Clear();
                cb?.Invoke();
            }
        }

        bool CheckEncounter(Vector2 pos)
        {
            var gm = GameplayManager.Instance;
            if (gm == null) return false;

            if (gm.Buffs != null)
            {
                for (int i = 0; i < gm.Buffs.Count; i++)
                {
                    var b = gm.Buffs[i];
                    if (b == null || b.Taken || handledBuffs.Contains(b)) continue;
                    if (!gm.IsHit(pos, b.gameObject)) continue;

                    handledBuffs.Add(b);
                    pauseTimer = stopDuration;
                    b.Collect(Owner);
                    return true;
                }
            }

            if (gm.Enemies != null && Owner != null && Owner.AttackController != null)
            {
                for (int i = 0; i < gm.Enemies.Count; i++)
                {
                    var e = gm.Enemies[i];
                    if (e == null || !e.IsAlive || handledEnemies.Contains(e)) continue;
                    if (!gm.IsHit(pos, e.gameObject)) continue;

                    handledEnemies.Add(e);
                    pauseTimer = stopDuration;
                    Owner.AttackController.Fight(e);
                    return true;
                }
            }

            return false;
        }

        void SetPos(Vector2 p)
        {
            Vector3 v = target.position;
            v.x = p.x;
            v.y = p.y;
            target.position = v;
        }

        void Face(float dirX)
        {
            if (!faceMovementDirection || Mathf.Abs(dirX) < 0.0001f) return;
            Vector3 s = target.localScale;
            s.x = Mathf.Abs(s.x) * Mathf.Sign(dirX);
            target.localScale = s;
        }
    }
}
