using UnityEngine;
namespace Stickman
{   
    public class FPSControlller : MonoBehaviour
    {
        void Start()
        {
            Application.targetFrameRate = 60;
        }
    }
}
