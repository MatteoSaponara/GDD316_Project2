using UnityEngine;
using TMPro;

namespace Game
    
{
    public class WinUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI winText;

        private void Start( )
        {
            winText.enabled = false;
            FindAnyObjectByType<WinDoor>().OnEnter += HandleWin;
        }

        private void OnDisable()
        {
            FindAnyObjectByType<WinDoor>().OnEnter -= HandleWin;
        }

        private void HandleWin()
        {
            winText.enabled = true;
        }
    }
}
