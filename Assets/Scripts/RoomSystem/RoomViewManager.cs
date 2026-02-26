using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.RoomSystem
{
    public class RoomViewManager : MonoBehaviour
    {
        public enum View { Main, Right, Left }

        [Header("UI References")]
        [Tooltip("The main Image component where the room views are rendered.")]
        public Image roomImage;

        [Header("View Sprites")]
        public Sprite mainViewSprite;
        public Sprite rightViewSprite;
        public Sprite leftViewSprite;

        [Header("Hotspot Containers")]
        [Tooltip("Parent GameObject for Main view hotspots")]
        public GameObject hotspotsMain;
        [Tooltip("Parent GameObject for Right view hotspots")]
        public GameObject hotspotsRight;
        [Tooltip("Parent GameObject for Left view hotspots")]
        public GameObject hotspotsLeft;

        private View currentView;

        private void Start()
        {
            SetView(View.Main);
        }

        public void NextView()
        {
            switch (currentView)
            {
                case View.Main:
                    SetView(View.Right);
                    break;
                case View.Right:
                    SetView(View.Left);
                    break;
                case View.Left:
                    SetView(View.Main);
                    break;
            }
        }

        public void PrevView()
        {
            switch (currentView)
            {
                case View.Main:
                    SetView(View.Left);
                    break;
                case View.Left:
                    SetView(View.Right);
                    break;
                case View.Right:
                    SetView(View.Main);
                    break;
            }
        }

        private void SetView(View view)
        {
            currentView = view;

            // Deactivate all hotspots first
            if (hotspotsMain != null) hotspotsMain.SetActive(false);
            if (hotspotsRight != null) hotspotsRight.SetActive(false);
            if (hotspotsLeft != null) hotspotsLeft.SetActive(false);

            // Update sprite and activate correct hotspots
            if (roomImage != null)
            {
                switch (view)
                {
                    case View.Main:
                        roomImage.sprite = mainViewSprite;
                        if (hotspotsMain != null) hotspotsMain.SetActive(true);
                        break;
                    case View.Right:
                        roomImage.sprite = rightViewSprite;
                        if (hotspotsRight != null) hotspotsRight.SetActive(true);
                        break;
                    case View.Left:
                        roomImage.sprite = leftViewSprite;
                        if (hotspotsLeft != null) hotspotsLeft.SetActive(true);
                        break;
                }
            }
            else
            {
                Debug.LogWarning("Room Image reference is missing in RoomViewManager!");
            }
        }
    }
}
