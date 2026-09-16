using UnityEngine;
using UnityEngine.UI;

namespace Simva
{
    public class ResponsiveOptionsLayout : MonoBehaviour
    {
        public float narrowThreshold = 560f;
        public int columnSpacing = 12;
        public int stackedSpacing = 6;

        private RectTransform rectTransform;
        private float lastWidth = -1f;
        private bool showingColumns = false;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            ShowColumns();
        }

        private void Update()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
                if (rectTransform == null) return;
            }
            float width = rectTransform.rect.width;
            if (Mathf.Approximately(width, lastWidth)) return;
            lastWidth = width;
            if (width <= 0f || width < narrowThreshold)
            {
                ShowStacked();
            }
            else
            {
                ShowColumns();
            }
        }

        private void ShowColumns()
        {
            if (showingColumns && GetComponent<HorizontalLayoutGroup>() != null) return;
            var old = GetComponent<VerticalLayoutGroup>();
            if (old != null) DestroyImmediate(old);
            var hlg = GetComponent<HorizontalLayoutGroup>();
            if (hlg == null) hlg = gameObject.AddComponent<HorizontalLayoutGroup>();
            if (hlg == null)
            {
                Debug.LogError("[Device] Failed to create options columns layout.");
                return;
            }
            hlg.padding = new RectOffset(0, 0, 0, 0);
            hlg.spacing = columnSpacing;
            hlg.childAlignment = TextAnchor.UpperCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;
            showingColumns = true;
        }

        private void ShowStacked()
        {
            if (!showingColumns && GetComponent<VerticalLayoutGroup>() != null) return;
            var old = GetComponent<HorizontalLayoutGroup>();
            if (old != null) DestroyImmediate(old);
            var vlg = GetComponent<VerticalLayoutGroup>();
            if (vlg == null) vlg = gameObject.AddComponent<VerticalLayoutGroup>();
            if (vlg == null)
            {
                Debug.LogError("[Device] Failed to create options stacked layout.");
                return;
            }
            vlg.padding = new RectOffset(0, 0, 0, 0);
            vlg.spacing = stackedSpacing;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            showingColumns = false;
        }
    }
}
