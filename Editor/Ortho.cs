using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.SceneManagement;

namespace VaroniaBackOffice
{
    public class Ortho : EditorWindow
    {
        private float orthographicSize = 10f;
        private float cameraHeight = 50f;
        private Camera tempCam;
        private bool isSetupMode = false;

        [MenuItem("Varonia/Ortho View")]
        public static void ShowWindow()
        {
            Ortho window = GetWindow<Ortho>("Ortho Capture");
            window.minSize = new Vector2(300, 180);
        }

        void OnGUI()
        {
            if (!isSetupMode)
            {
                DrawStartGui();
            }
            else
            {
                DrawSetupGui();
            }
        }

        private void DrawStartGui()
        {
            EditorGUILayout.HelpBox("Click Setup to configure the orthographic view in the Game window.", MessageType.Info);
            if (GUILayout.Button("🛠 Start Setup", GUILayout.Height(50)))
            {
                isSetupMode = true;
                UpdateCamera(); 
            }
        }

        private void DrawSetupGui()
        {
            GUILayout.Label("View Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            orthographicSize = EditorGUILayout.Slider("Zoom (Size)", orthographicSize, 1f, 150f);
            cameraHeight = EditorGUILayout.Slider("Altitude (Y)", cameraHeight, 1f, 300f);
            
            if (EditorGUI.EndChangeCheck())
            {
                UpdateCamera();
            }

            GUILayout.FlexibleSpace();

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("📸 CAPTURE & CLOSE (1080p)", GUILayout.Height(50)))
            {
                CaptureAndExit();
            }
            
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Cancel"))
            {
                this.Close();
            }
        }

        private void UpdateCamera()
        {
            GameObject go = GameObject.Find("TempOrthoCam");
            if (go == null) go = new GameObject("TempOrthoCam");

            tempCam = go.GetComponent<Camera>();
            if (tempCam == null) tempCam = go.AddComponent<Camera>();

            tempCam.orthographic = true;
            tempCam.orthographicSize = orthographicSize;
            tempCam.transform.position = new Vector3(0, cameraHeight, 0);
            tempCam.transform.rotation = Quaternion.Euler(90, 0, -90);
            
            EditorApplication.ExecuteMenuItem("Window/General/Game");
            SceneView.RepaintAll();
        }

        private void CaptureAndExit()
        {
            if (tempCam == null) return;

            RenderTexture rt = new RenderTexture(1920, 1080, 24);
            tempCam.targetTexture = rt;
            Texture2D screenShot = new Texture2D(1920, 1080, TextureFormat.RGB24, false);

            tempCam.Render();

            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            screenShot.Apply();

            byte[] bytes = screenShot.EncodeToJPG(95);

            // --- Updated Filename Logic ---
            // Format: SceneName_ZoomValue.jpg
            string sceneName = SceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(sceneName)) sceneName = "UntitledScene";
            
            string fileName = $"{sceneName}_{orthographicSize}.jpg";
            
            string path = Path.Combine(Application.dataPath, fileName);
            File.WriteAllBytes(path, bytes);

            tempCam.targetTexture = null;
            RenderTexture.active = null;
            DestroyImmediate(rt);
            
            AssetDatabase.Refresh();
            Debug.Log($"✅ Capture successful: {path}");

            this.Close();
        }

        private void OnDestroy()
        {
            GameObject go = GameObject.Find("TempOrthoCam");
            if (go != null) DestroyImmediate(go);
        }
    }
}