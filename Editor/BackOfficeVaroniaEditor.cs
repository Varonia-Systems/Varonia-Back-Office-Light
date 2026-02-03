using UnityEngine;
using UnityEditor;

namespace VaroniaBackOffice
{
    [CustomEditor(typeof(BackOfficeVaronia))]
    public class BackOfficeVaroniaEditor : Editor
    {
        private Texture2D _logo;
        private const float LogoSize = 250f;

        private void OnEnable()
        {
            string[] guids = AssetDatabase.FindAssets("VaroniaLogo t:Texture2D");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _logo = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
        }

        public override void OnInspectorGUI()
        {
            BackOfficeVaronia script = (BackOfficeVaronia)target;

            // --- Logo Section ---
            if (_logo != null)
            {
                var rect = GUILayoutUtility.GetRect(Screen.width - 38, LogoSize, GUI.skin.box);
                GUI.DrawTexture(rect, _logo, ScaleMode.ScaleToFit);
            }

            EditorGUILayout.Space(5);

            // --- Documentation Section ---
            EditorGUILayout.HelpBox("IMPORTANT: The game flow relies on OnStartWithTuto or OnStartSkipTuto events.", MessageType.Info);

            EditorGUILayout.Space(5);

            // --- Status Section (Read-Only) ---
            EditorGUILayout.LabelField("System Status", EditorStyles.boldLabel);
            GUI.enabled = false;
            EditorGUILayout.Toggle("Is Game Started", script.IsStarted);
            
            if (Application.isPlaying && script.mqttClient != null)
                EditorGUILayout.EnumPopup("Current Soft State", script.mqttClient.SoftState);
            else
                EditorGUILayout.TextField("Current Soft State", "N/A (Offline)");
            GUI.enabled = true;

            EditorGUILayout.Space(10);

            // --- Config Section (Read-Only) ---
            EditorGUILayout.LabelField("JSON Configuration", EditorStyles.boldLabel);
            GUI.enabled = false;
            SerializedProperty configProp = serializedObject.FindProperty("config");
            EditorGUILayout.PropertyField(configProp, true);
            GUI.enabled = true;

            EditorGUILayout.Space(10);

            // --- Events Section ---
            EditorGUILayout.LabelField("Start Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnStartWithTuto"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnStartSkipTuto"));

            EditorGUILayout.Space(10);

            // --- Debug Section ---
            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("Debug Controls", EditorStyles.boldLabel);
                
                // Game Start Buttons
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Start With Tuto", GUILayout.Height(25))) script.TriggerStartGame(false);
                if (GUILayout.Button("Start Skip Tuto", GUILayout.Height(25))) script.TriggerStartGame(true);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);

                // Soft State Debug Buttons
                if (script.mqttClient != null)
                {
                    EditorGUILayout.LabelField("Force Soft State (MQTT Simulation)");
                    EditorGUILayout.BeginHorizontal();
                    
                    // Button for each important state (adjust names to your ESoftState enum)
                    if (GUILayout.Button("Set Launch")) script.mqttClient.SetSoftState(ESoftState.GAME_LAUNCHED);
                    if (GUILayout.Button("Set InLobby")) script.mqttClient.SetSoftState(ESoftState.GAME_INLOBBY);
                    if (GUILayout.Button("Set InParty")) script.mqttClient.SetSoftState(ESoftState.GAME_INPARTY);
                    
                    EditorGUILayout.EndHorizontal();
                }

                Repaint();
            }
            else
            {
                EditorGUILayout.HelpBox("Debug buttons are only available in Play Mode.", MessageType.None);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}