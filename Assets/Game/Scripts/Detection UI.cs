using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class DetectionUI : MonoBehaviour
    {
        [SerializeField] private Image undetectedVisual;
        //[SerializeField] private Image dangerVisual;
        [SerializeField] private Image detectedVisual;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            undetectedVisual.enabled = true;

            //dangerVisual.enabled = false;

            detectedVisual.enabled = false;
        }

        private void Update()
        {
            if(GameManager.Instance.PlayerDetected)
            {
                undetectedVisual.enabled = false;
                detectedVisual.enabled = true;
            }
            else
            {
                undetectedVisual.enabled = true;
                detectedVisual.enabled = false;
            }
        }


    }
}
