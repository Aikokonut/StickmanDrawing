using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Stickman
{
    public class StickmanController : MonoBehaviour
    {
        [Header("Combat")]
        public int attack = 20;
        public Text attackLabel;

        [Header("Siblings (assign in Inspector)")]
        public AttackController AttackController;
        public MovementController MovementController;
        public DrawLine DrawLine;

        public event Action<StickmanController> OnDied;
        public bool IsAlive { get; private set; } = true;

        void Awake()
        {
            if (AttackController != null) AttackController.Owner = this;
            if (MovementController != null) MovementController.Owner = this;
        }

        void Start()
        {
            RefreshLabel();
            if (DrawLine != null) DrawLine.OnPathDrawn += HandlePathDrawn;
        }

        void OnDestroy()
        {
            if (DrawLine != null) DrawLine.OnPathDrawn -= HandlePathDrawn;
        }

        void HandlePathDrawn(IReadOnlyList<Vector2> path)
        {
            if (MovementController == null || !IsAlive) return;
            if (DrawLine != null) DrawLine.enabled = false;
            MovementController.FollowPath(path, OnDone);
        }

        void OnDone()
        {
            if (DrawLine != null) { DrawLine.Clear(); DrawLine.enabled = true; }
        }

        public void AddAttack(int amount)
        {
            attack = Mathf.Max(0, attack + amount);
            RefreshLabel();
        }

        public void Die()
        {
            if (!IsAlive) return;
            IsAlive = false;
            OnDied?.Invoke(this);
            if (MovementController != null) MovementController.Stop();
            Destroy(gameObject);
        }

        void RefreshLabel()
        {
            if (attackLabel != null) attackLabel.text = attack.ToString();
        }
    }
}
