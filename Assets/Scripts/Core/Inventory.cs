using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame.Core
{
    public class Inventory : MonoBehaviour
    {
        public static Inventory Instance { get; private set; }

        private HashSet<string> items = new HashSet<string>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // Optional: DontDestroyOnLoad(gameObject); if you want inventory to persist across scenes
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void AddItem(string itemId)
        {
            if (!string.IsNullOrEmpty(itemId) && !items.Contains(itemId))
            {
                items.Add(itemId);
                Debug.Log($"Item added to inventory: {itemId}");
            }
        }

        public bool HasItem(string itemId)
        {
            return items.Contains(itemId);
        }

        public void RemoveItem(string itemId)
        {
            if (items.Contains(itemId))
            {
                items.Remove(itemId);
                Debug.Log($"Item removed from inventory: {itemId}");
            }
        }
    }
}
