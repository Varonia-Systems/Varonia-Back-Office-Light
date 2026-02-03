using Newtonsoft.Json;
using UnityEngine;

namespace VaroniaBackOffice
{
    /// <summary>
    /// Represents the global configuration for the Varonia application.
    /// Maps directly to the GlobalConfig.json file.
    /// </summary>
    [System.Serializable]
    public class GlobalConfig
    {
        [Header("Network")]
        /// <summary> The IP address of the main game server. </summary>
        public string ServerIP; 
        
        /// <summary> The IP address of the MQTT broker. </summary>
        public string MQTT_ServerIP; 
        
        /// <summary> Unique client identifier for the MQTT connection. </summary>
        public int MQTT_IDClient; 

        [Header("Preferences")]
        /// <summary> Role of the device (e.g., Server_Player, Client_Spectator). </summary>
        public DeviceMode DeviceMode; 
        
        /// <summary> Selected UI and localized content language. </summary>
        public string Language; 
        
        /// <summary> Player's dominant hand for input/VR. </summary>
        public MainHand MainHand;  
        
        /// <summary> Local display name for the player. </summary>
        public string PlayerName; 

        /// <summary>
        /// Deserializes a JSON string into a GlobalConfig object using Newtonsoft.Json.
        /// </summary>
        /// <param name="jsonString">The raw JSON data.</param>
        /// <returns>A populated GlobalConfig object or null if deserialization fails.</returns>
        public static GlobalConfig CreateFromJson(string jsonString)
        {
            try 
            {
                return JsonConvert.DeserializeObject<GlobalConfig>(jsonString);
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"[GlobalConfig] Deserialization Error: {e.Message}");
                return null; 
            }
        }
    }
}