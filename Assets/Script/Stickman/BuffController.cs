using UnityEngine;
using UnityEngine.UI;

namespace Stickman
{
    public class BuffController : MonoBehaviour
    {
        public int amount = 20;
        public Text label;

        public bool Taken { get; private set; }

        void Start()
        {
            if (label != null) label.text = "+" + amount;
        }

        public void Collect(StickmanController player)
        {
            if (Taken || player == null || !player.IsAlive) return;
            Taken = true;

            int before = player.attack;
            player.AddAttack(amount);

            Debug.Log($"[Buff] +{amount}  ({before} → {player.attack})");
            Destroy(gameObject);
        }
    }
}
