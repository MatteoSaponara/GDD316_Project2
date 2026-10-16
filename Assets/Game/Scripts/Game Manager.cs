
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance => instance;

        private static GameManager instance;

        public bool PlayerDetected => playerDetected;

        private bool playerDetected;

        // Reference to Player object
        public GameObject Player => player;
        [SerializeField] private GameObject player;

        // How long 
        [SerializeField] private float detectionTimeout = 10f;

        private float detectionTimer;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.LogError("GameManager singleton already exists!");
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            FindAnyObjectByType<WinDoor>().OnEnter += HandleWin;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            playerDetected = false;

            // Find the new player after the scene reloads.
            PlayerMovement playerMovement =
                FindAnyObjectByType<PlayerMovement>();

            if (playerMovement != null)
            {
                player = playerMovement.gameObject;
            }
        }

        private void Start()
        {
            playerDetected = false;
            FindAnyObjectByType<WinDoor>().OnEnter += HandleWin;
        }

        // Runs detection timer if the player has been detected
        private void Update()
        {
            if (!playerDetected)
            {
                return;
            }

            detectionTimer += Time.deltaTime;

            if (detectionTimer >= detectionTimeout)
            {
                playerDetected = false;
                detectionTimer = 0f;

                Debug.Log("Enemies have lost the player's position.");
            }
        }

        public void ReportPlayerDetected()
        {
            playerDetected = true;
            Debug.Log("Everyone knows the player's position!");
            detectionTimer = 0f;
        }

        public void HandleWin()
        {
            Debug.Log("You Win!");

            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();

            if (playerMovement != null)
            {
                playerMovement.DisableCameraControl();
            }

            Time.timeScale = 0f;
        }

        public void GameOver()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

    }
}
