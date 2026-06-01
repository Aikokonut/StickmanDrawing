using UnityEngine;
namespace Stickman
{
public class SwitchController : MonoBehaviour
{
   public DoorController Door;
   public void OnTriggerEnter2D(Collider2D other)
   {
    if (other.gameObject.CompareTag("Player"))
    {
        Door.Open();
    }
   }
}
}