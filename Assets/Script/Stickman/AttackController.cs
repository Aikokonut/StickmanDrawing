using UnityEngine;

namespace Stickman
{
    public class AttackController : MonoBehaviour
    {
        [HideInInspector] public StickmanController Owner;

        public void Fight(EnemyController enemy)
        {
            if (Owner == null || !Owner.IsAlive) return;
            if (enemy == null || !enemy.IsAlive) return;

            int my = Owner.attack;
            int his = enemy.Attack;

            if (my >= his)
            {
                Owner.AddAttack(his);
                Debug.Log($"[Combat] Player({my}) WIN Enemy({his})  → attack = {Owner.attack}");
                enemy.Die();
            }
            else
            {
                Debug.Log($"[Combat] Player({my}) LOOSE Enemy({his})");
                Owner.Die();
            }
        }
    }
}
