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

        List<Vector2> path = new List<Vector2>(256);
        HashSet<EnemyController> handledEnemies = new HashSet<EnemyController>();
        HashSet<BuffController> handledBuffs = new HashSet<BuffController>();

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
                Action cb = onCompleted;
                onCompleted = null;
                path.Clear();
                handledEnemies.Clear();
                handledBuffs.Clear();
                cb?.Invoke();
            }
        }

        bool CheckEncounter(Vector2 pos)
        {
            if (GameplayManager.Instance.Buffs != null)
            {
                for (int i = 0; i < GameplayManager.Instance.Buffs.Count; i++)
                {
                    BuffController b = GameplayManager.Instance.Buffs[i];
                    if (b == null || b.Taken || handledBuffs.Contains(b)) continue;
                    if (!GameplayManager.Instance.IsHit(pos, b.gameObject)) continue;

                    handledBuffs.Add(b);
                    pauseTimer = stopDuration;
                    b.Collect(Owner);
                    if (IsEndpointTarget(b.gameObject)) index = path.Count;
                    return true;
                }
            }

            if (GameplayManager.Instance.Enemies != null && Owner != null && Owner.AttackController != null)
            {
                for (int i = 0; i < GameplayManager.Instance.Enemies.Count; i++)
                {
                    EnemyController e = GameplayManager.Instance.Enemies[i];
                    if (e == null || !e.IsAlive || handledEnemies.Contains(e)) continue;
                    if (!GameplayManager.Instance.IsHit(pos, e.gameObject)) continue;

                    handledEnemies.Add(e);
                    pauseTimer = stopDuration;
                    Owner.AttackController.Fight(e);
                    if (Owner != null && Owner.IsAlive && IsEndpointTarget(e.gameObject))
                        index = path.Count;
                    return true;
                }
            }

            return false;
        }


        bool IsEndpointTarget(GameObject obj)
        {
            if (obj == null || path.Count == 0) return false;

            return GameplayManager.Instance != null && GameplayManager.Instance.IsHit(path[path.Count - 1], obj);
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
