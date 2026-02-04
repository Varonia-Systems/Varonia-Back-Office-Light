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
        public static BackOfficeVaronia Instance { get; private set; }
        public static event Action OnConfigLoaded;

        [Header("Events")]
        public UnityEvent OnStartWithTuto;
        public UnityEvent OnStartSkipTuto;

        [Header("Status")]
        [SerializeField] private bool _isStarted = false;
        public bool IsStarted => _isStarted;

        [Header("Settings")]
        public GlobalConfig config;
        
        [HideInInspector]
        public MQTTVaronia mqttClient;

        private void Awake()
        {
            mqttClient = GetComponent<MQTTVaronia>();
            InitializeSingleton();
            LoadConfig();
        }

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
        /// Loads the config from JSON. If the file doesn't exist, it creates a new one with default values.
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
                    Debug.Log($"[BackOfficeVaronia] Config loaded from {filePath}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[BackOfficeVaronia] JSON Parse Error: {e.Message}");
                }
            }
            else
            {
                // --- AUTO-GENERATION LOGIC ---
                Debug.LogWarning("[BackOfficeVaronia] Config file missing. Creating default GlobalConfig.json");
                
                // 1. Create a new instance (it will use the default values defined in the class)
                config = new GlobalConfig(); 
                
                // 2. Save it to disk immediately
                SaveConfig();
                
                Debug.Log($"[BackOfficeVaronia] Config loaded from {filePath}");
            }

            OnConfigLoaded?.Invoke();
        }

        /// <summary>
        /// Serializes the current config object and saves it to the persistent path.
        /// </summary>
        public void SaveConfig()
        {
            try
            {
                string rootPath = Application.persistentDataPath.Replace(Application.companyName + "/" + Application.productName, "Varonia");
                string filePath = Path.Combine(rootPath, "GlobalConfig.json");
                
                string json = config.ToJson(); // Using the Newtonsoft method we discussed
                File.WriteAllText(filePath, json);
                
                Debug.Log($"[BackOfficeVaronia] Config saved successfully to {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[BackOfficeVaronia] Failed to save config: {e.Message}");
            }
        }

        public void TriggerStartGame(bool skipTuto)
        {
            if (_isStarted) return;
            _isStarted = true;
            
            if (skipTuto) OnStartSkipTuto?.Invoke();
            else OnStartWithTuto?.Invoke();
        }
    }
}