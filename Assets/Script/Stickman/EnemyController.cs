using System;
using UnityEngine;
using UnityEngine.UI;

namespace Stickman
{
    public class EnemyController : MonoBehaviour
    {
        public int Attack = 20;
        public Text attackLabel;

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
            if (attackLabel != null) attackLabel.text = Attack.ToString();
        }
    }
}
