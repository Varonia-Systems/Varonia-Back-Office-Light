using System;
using System.Collections.Generic;
using M2MqttUnity;
using Newtonsoft.Json;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace VaroniaBackOffice
{
    public class MQTTVaronia : M2MqttUnityClient
    {

        public ESoftState SoftState { get; private set; }
        
        
        protected void Start()
        {
            BackOfficeVaronia.OnConfigLoaded += HandleConfigLoaded;

            // If config is already loaded (singleton pattern safety), trigger it manually
            if (BackOfficeVaronia.Instance != null && BackOfficeVaronia.Instance.config != null)
            {
                HandleConfigLoaded();
            }
        }
        
        
        private void HandleConfigLoaded()
        {
            Debug.Log("[MQTT] Config received via Event. Initializing connection...");
            var cfg = BackOfficeVaronia.Instance.config;
            this.brokerAddress = cfg.MQTT_ServerIP;
    
            // Now connect
            Connect();
        }
        
        
        
        protected override void OnConnected()
        {
            base.OnConnected();
            Debug.Log("[Back Office Varonia] Successfully connected to the broker");

            Subscribe();
            
        }

        
        
        public void Subscribe()
        {
            client.Subscribe(new string[] { "ServerToUnity/" + BackOfficeVaronia.Instance.config.MQTT_IDClient }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        }
        
        public void PublishMsg(string Msg)
        {
            try
            {
                client.Publish("UnityToServer/" + BackOfficeVaronia.Instance.config.MQTT_IDClient, System.Text.Encoding.UTF8.GetBytes(Msg), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
            }
            catch (Exception)
            {
                Debug.LogError("[Back Office Varonia] Error while publishing message");
            }
        }
        
        
        

        protected override void DecodeMessage(string topic, byte[] message) // Receive Message
        {
            
            
            var payload = JsonConvert.DeserializeObject<MQTT_Payload>(System.Text.Encoding.UTF8.GetString(message));

            
            try 
            {
                // Native Unity deserialization (No plugin required)
                
                if (payload != null) 
                {
                    Debug.Log("[MQTT] Payload successfully parsed.");
                }


                if (payload.sMethod == "GET_SOFTPARTYSTART_RESULT")
                    BackOfficeVaronia.Instance.TriggerStartGame(false);
                
         
                if (payload.sMethod == "GET_SOFTPARTYSKIPTUTOANDSTART_RESULT")
                    BackOfficeVaronia.Instance.TriggerStartGame(true);
            
         
            
            }
            catch (System.Exception e) 
            {
                Debug.LogError($"[MQTT] Failed to deserialize payload: {e.Message}");
            }
            
        }
        
        
        
        protected override void OnConnectionFailed(string errorMessage)
        {
            Debug.LogError($"[Back Office Varonia] MQTT Fail to connect : {errorMessage}");
        }
        
     
        public void SetSoftState(ESoftState eSoft)
        {
            
            PublishMsg(JsonConvert.SerializeObject(new MQTT_Payload() { sMethod = "SET_SOFTSTATE", CallerDeviceID = BackOfficeVaronia.Instance.config.MQTT_IDClient, Items = { { "SoftState", eSoft } } }));
            Debug.Log($"[MQTT] Published SoftState: {eSoft}");
            SoftState = eSoft;
        }
        
        
        
        public void SetSoftPartyStarted()
        {
            if (!String.IsNullOrEmpty(BackOfficeVaronia.Instance.config.MQTT_ServerIP))
            {
                PublishMsg(JsonConvert.SerializeObject(new MQTT_Payload() { sMethod = "SET_SOFTPARTYSTARTED", CallerDeviceID = BackOfficeVaronia.Instance.config.MQTT_IDClient }));
            }
        }

     
        public void SetSoftPartyClosed()
        {
            if (!String.IsNullOrEmpty(BackOfficeVaronia.Instance.config.MQTT_ServerIP))
            {
                PublishMsg(JsonConvert.SerializeObject(new MQTT_Payload() { sMethod = "SET_SOFTPARTYCLOSED", CallerDeviceID =BackOfficeVaronia.Instance.config.MQTT_IDClient }));
            }
        }
        
        
        
        
        
        
    }


    public class MQTT_Payload
    {
        public int CallerDeviceID { get; set; }
        public int TargetDeviceID { get; set; }
      
        public string sMethod { get; set; }
        public Dictionary<string, object> Items { get; set; }

        public MQTT_Payload()
        {
            Items = new Dictionary<string, object>();
        }
    }
    
    }
    

