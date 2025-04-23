using UnityEngine;
using UnityEngine.UI;

namespace Simva
{
    public class GetSimvaStringName : MonoBehaviour
{

    private void Awake()
    {
        FillName();
    }

    void FillName()
    {
        if(gameObject.GetComponent<Text>() != null)
        {
            gameObject.GetComponent<Text>().text = LanguageSelectorController.instance.GetName(gameObject.name);
        }
        else
        {
			Text textobject = gameObject.GetComponentInChildren<Text>();
			if (textobject != null)
			{
				textobject.text = LanguageSelectorController.instance.GetName(gameObject.name);
			} else
			{
				TextMesh textmesh = gameObject.GetComponentInChildren<TextMesh>();
				if (textmesh != null)
				{
					textmesh.text = LanguageSelectorController.instance.GetName(gameObject.name);
				}
			}
			
        }
	}
}
}