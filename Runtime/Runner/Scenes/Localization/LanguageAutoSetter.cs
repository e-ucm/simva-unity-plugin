using UnityEngine;
using UnityEngine.UI;

namespace Simva
{
    public class LanguageAutoSetter : MonoBehaviour
    {
        [SerializeField] private Text textComponent;
        [SerializeField] private TextMesh textMeshComponent;
        [SerializeField] private string languageKey;

        private void Awake()
        {
            FillName();
        }
        void FillName()
        {
            var name = SimvaPlugin.Instance.GetName(languageKey);
            if (textComponent != null)
            {
                textComponent.text = name;
            }
            else if (textMeshComponent != null)
            {
                textMeshComponent.text = name;
            }
        }
    }
}