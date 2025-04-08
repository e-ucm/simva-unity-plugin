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
                Debug.LogError("The component " + img.GetType().ToString() + " doesn't exit (Object " + this.gameObject.name + ")");
                return;
            }

            if(button == null)
            {
                Debug.LogError("The component " + button.GetType().ToString() + " doesn't exit (Object " + this.gameObject.name + ")");
                return;
            }

            string path = "";
            Texture2D import = null;

            var localizationPath = "Localization/" + gameObject.name + "/";
            var all = Resources.LoadAll(localizationPath);
            Debug.Log(localizationPath);
            foreach (var item in all)
            {
                Debug.Log("Found: " + item.name + " (" + item.GetType() + ")");
            }

            path = localizationPath + "flag";
            Debug.Log("Trying to load from: Resources/" + path);
            
            import = Resources.Load(path) as Texture2D;

            if (import == null)
            {
                //Debug.LogError("Error: " + path+ " doesn't exit (Object " + gameObject.name + ")");
                //return;
            } else {
                img.sprite = Sprite.Create(import, new Rect(0, 0, import.width, import.height), Vector2.zero);
            }
            
            button.onClick.AddListener(SelectLanguage);

        }

        void SelectLanguage()
        {
            LanguageSelectorController.instance.SetLanguage(gameObject.name);
        }


        // VISUAL EFFECT ONLY. DOES NOT AFFECT EXECUTION AND CAN BE DELETED.
        public void Expand()
        {
            transform.localScale = new Vector2(1.2f, 1.2f);
        }

        public void Contract()
        {
            transform.localScale = new Vector2(1.0f, 1.0f);
        }
    }
}