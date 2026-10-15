using UnityEngine;
using System;

namespace Game
{
    public class WinDoor : MonoBehaviour
    {
        public Action OnEnter;

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PlayerMovement>() != null)
            {
                OnEnter?.Invoke();
            }

        }
    }
}
