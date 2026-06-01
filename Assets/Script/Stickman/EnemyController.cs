using System;
using UnityEngine;
using UnityEngine.UI;

namespace Stickman
{
    public class EnemyController : TargetEnityBase
    {
        public int Attack = 20;
        public Text AttackLabel;

        public event Action<EnemyController> OnDied;
        public bool IsAlive { get; private set; } = true;

        void Start()
        {
            RefreshLabel();
        }

        public void Die()
        {
            if (!IsAlive) return;
            IsAlive = false;
            OnDied?.Invoke(this);
            Destroy(gameObject);
        }

        void RefreshLabel()
        {
            if (AttackLabel != null) AttackLabel.text = Attack.ToString();
        }
    }
}
