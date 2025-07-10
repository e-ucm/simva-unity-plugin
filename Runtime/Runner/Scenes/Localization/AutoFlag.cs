using UnityEngine;
using UnityEngine.UI;



namespace Simva
{
    // Rebuilds a path given the gameobject's name.
    // Imports the image that we need.
    // Creates a Sprite and adds this image.
    // Gives the button a listener to assign the game's language.
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Button))]
    public class AutoFlag : MonoBehaviour
    {
        Image img;
        Button button;

        private void Awake()
        {
            img = GetComponent<Image>();
            button = GetComponent<Button>();

            if(img == null)
            {
                SimvaPlugin.Instance.LogError("The component " + img.GetType().ToString() + " doesn't exit (Object " + this.gameObject.name + ")");
                return;
            }

            if(button == null)
            {
                SimvaPlugin.Instance.LogError("The component " + button.GetType().ToString() + " doesn't exit (Object " + this.gameObject.name + ")");
                return;
            }

            string path = "";
            Texture2D import = null;

            path = "Localization/" + gameObject.name + "/flag";
            SimvaPlugin.Instance.Log("Trying to load from: Resources/" + path);
            
            import = Resources.Load(path) as Texture2D;

            if (import == null)
            {
                SimvaPlugin.Instance.LogError("Error: " + path+ " doesn't exit (Object " + gameObject.name + ")");
                return;
            } else {
                img.sprite = Sprite.Create(import, new Rect(0, 0, import.width, import.height), Vector2.zero);
                SimvaPlugin.Instance.Log("Resources/" + path + " found and loaded");
            }
            
            button.onClick.AddListener(SelectLanguage);

        }

        void SelectLanguage()
        {
            LanguageSelectorController.instance.SetLanguageCode(gameObject.name);
            LanguageSelectorController.instance.FillDictionaryAndRunLoginScene();
        }
    }
}