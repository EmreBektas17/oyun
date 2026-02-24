#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PuzzleGame.RoomSystem;
using PuzzleGame.UI;

namespace PuzzleGame.EditorTools
{
    public class RoomSetupEditor : EditorWindow
    {
        [MenuItem("PuzzleGame/Setup Room UI")]
        public static void CreateRoomUI()
        {
            // 1. Create Canvas
            Canvas canvas = GameObject.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // 2. Create EventSystem
            if (GameObject.FindObjectOfType<EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
                System.Type inputModuleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                if (inputModuleType != null)
                {
                    eventSystem.AddComponent(inputModuleType);
                }
                else
                {
                    eventSystem.AddComponent<StandaloneInputModule>();
                }
#else
                eventSystem.AddComponent<StandaloneInputModule>();
#endif
            }

            // 3. Create Main Room Game Object
            GameObject roomManagerObj = new GameObject("RoomManager");
            RoomViewManager roomManager = roomManagerObj.AddComponent<RoomViewManager>();

            // 4. Create Room Image
            GameObject roomImageObj = new GameObject("RoomImage");
            roomImageObj.transform.SetParent(canvas.transform, false);
            Image roomImage = roomImageObj.AddComponent<Image>();
            roomImage.color = Color.white;
            
            // Stretch to fill
            RectTransform roomRect = roomImage.GetComponent<RectTransform>();
            roomRect.anchorMin = Vector2.zero;
            roomRect.anchorMax = Vector2.one;
            roomRect.offsetMin = Vector2.zero;
            roomRect.offsetMax = Vector2.zero;

            roomManager.roomImage = roomImage;

            // 5. Create Navigation Buttons
            GameObject btnLeftObj = CreateButton("BtnLeftArrow", canvas.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(100, 100), new Vector2(50, 0));
            GameObject btnRightObj = CreateButton("BtnRightArrow", canvas.transform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(100, 100), new Vector2(-50, 0));

            Button btnLeft = btnLeftObj.GetComponent<Button>();
            Button btnRight = btnRightObj.GetComponent<Button>();

            // Link buttons to RoomManager
            UnityEngine.Events.UnityAction leftAction = new UnityEngine.Events.UnityAction(roomManager.PrevView);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnLeft.onClick, leftAction);

            UnityEngine.Events.UnityAction rightAction = new UnityEngine.Events.UnityAction(roomManager.NextView);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnRight.onClick, rightAction);

            // 6. Create Hotspot Containers
            GameObject hotspotsParent = new GameObject("Hotspots");
            hotspotsParent.transform.SetParent(canvas.transform, false);
            SetStretchRect(hotspotsParent.AddComponent<RectTransform>());

            roomManager.hotspotsMain = CreateHotspotContainer("Hotspots_Main", hotspotsParent.transform);
            roomManager.hotspotsRight = CreateHotspotContainer("Hotspots_Right", hotspotsParent.transform);
            roomManager.hotspotsLeft = CreateHotspotContainer("Hotspots_Left", hotspotsParent.transform);

            // 7. Create Zoom Panel
            GameObject zoomPanelObj = new GameObject("ZoomPanel");
            zoomPanelObj.transform.SetParent(canvas.transform, false);
            Image zoomPanelImg = zoomPanelObj.AddComponent<Image>();
            zoomPanelImg.color = new Color(0, 0, 0, 0.9f); // Dark background
            SetStretchRect(zoomPanelObj.GetComponent<RectTransform>());

            ZoomUIManager zoomUIManager = zoomPanelObj.AddComponent<ZoomUIManager>();
            zoomUIManager.zoomPanel = zoomPanelObj;

            GameObject zoomImageObj = new GameObject("ZoomImage");
            zoomImageObj.transform.SetParent(zoomPanelObj.transform, false);
            Image zoomedImg = zoomImageObj.AddComponent<Image>();
            zoomImageObj.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 600);
            zoomUIManager.zoomImage = zoomedImg;

            GameObject btnBackObj = CreateButton("BtnBack", zoomPanelObj.transform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(100, 50), new Vector2(-60, -30));
            Button btnBack = btnBackObj.GetComponent<Button>();
            
            UnityEngine.Events.UnityAction backAction = new UnityEngine.Events.UnityAction(zoomUIManager.CloseZoom);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnBack.onClick, backAction);

            zoomPanelObj.SetActive(false);

            Debug.Log("Puzzle Game Room UI successfully generated!");
            Selection.activeGameObject = roomManagerObj;
        }

        private static GameObject CreateButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Vector2 anchoredPosition)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = anchoredPosition;

            Image img = btnObj.AddComponent<Image>();
            img.color = Color.gray; // Placeholder color
            btnObj.AddComponent<Button>();
            return btnObj;
        }

        private static GameObject CreateHotspotContainer(string name, Transform parent)
        {
            GameObject container = new GameObject(name);
            container.transform.SetParent(parent, false);
            SetStretchRect(container.AddComponent<RectTransform>());
            return container;
        }

        private static void SetStretchRect(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
#endif
