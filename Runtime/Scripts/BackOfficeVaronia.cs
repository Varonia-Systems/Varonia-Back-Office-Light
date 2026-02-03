using UnityEngine;
using System.IO;
using System;
using UnityEngine.Events;

namespace VaroniaBackOffice
{
    /// <summary>
    /// Central manager for the Varonia Back Office. 
    /// Handles configuration loading, singleton persistence, and global game start events.
    /// </summary>
    public class BackOfficeVaronia : MonoBehaviour
    {
        /// <summary> Static instance for global access (Singleton). </summary>
        public static BackOfficeVaronia Instance { get; private set; }
        
        /// <summary> Event triggered once the GlobalConfig.json has been successfully loaded. </summary>
        public static event Action OnConfigLoaded;

        [Header("Events")]
        /// <summary> Triggered when the game starts with the tutorial sequence enabled. </summary>
        public UnityEvent OnStartWithTuto;
        
        /// <summary> Triggered when the game starts and the tutorial is bypassed. </summary>
        public UnityEvent OnStartSkipTuto;

        [Header("Status")]
        [SerializeField] private bool _isStarted = false;
        
        /// <summary> Indicates whether a start event has already been triggered. </summary>
        public bool IsStarted => _isStarted;

        [Header("Settings")]
        /// <summary> Holds the deserialized configuration data (IPs, IDs, Player settings). </summary>
        public GlobalConfig config;
        
        [HideInInspector]
        /// <summary> Reference to the MQTT client component attached to this GameObject. </summary>
        public MQTTVaronia mqttClient;

        private void Awake()
        {
            mqttClient = GetComponent<MQTTVaronia>();
            InitializeSingleton();
            LoadConfig();
        }

        /// <summary>
        /// Ensures only one instance of the BackOffice exists and persists across scenes.
        /// </summary>
        private void InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Reads GlobalConfig.json from the persistent data path (Varonia root folder) 
        /// and populates the config object.
        /// </summary>
        public void LoadConfig()
        {
            string rootPath = Application.persistentDataPath.Replace(Application.companyName + "/" + Application.productName, "Varonia");
            string filePath = Path.Combine(rootPath, "GlobalConfig.json");

            if (!Directory.Exists(rootPath)) Directory.CreateDirectory(rootPath);

            if (File.Exists(filePath))
            {
                try
                {
                    string jsonContent = File.ReadAllText(filePath);
                    config = GlobalConfig.CreateFromJson(jsonContent);
                    OnConfigLoaded?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[BackOfficeVaronia] JSON Parse Error: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Logic to trigger the game start. Prevents multiple triggers via the _isStarted flag.
        /// </summary>
        /// <param name="skipTuto">If true, invokes OnStartSkipTuto; otherwise invokes OnStartWithTuto.</param>
        public void TriggerStartGame(bool skipTuto)
        {
            if (_isStarted) return;

            _isStarted = true;
            
            if (skipTuto)
            {
                Debug.Log("[BackOfficeVaronia] Event: Start Skip Tuto triggered.");
                OnStartSkipTuto?.Invoke();
            }
            else
            {
                Debug.Log("[BackOfficeVaronia] Event: Start With Tuto triggered.");
                OnStartWithTuto?.Invoke();
            }
        }
    }
}