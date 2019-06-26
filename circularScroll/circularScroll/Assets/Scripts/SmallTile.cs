using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmallTile : MonoBehaviour {
    [SerializeField] private Text textBox;
    protected bool placeholderTile = false;
    public void ConfigureFor(string text="",bool placeholder=false)
    {
        if(placeholder)
        {
            textBox.text = text;
        }
        else
        {
            CleanItem();
            placeholderTile = true;
            textBox.text = string.Empty;
        }
    }
    public void SetActiveSafely(bool value)
    {
        gameObject.SetActive(value);
    }
    public void CleanItem()
    {
        if (textBox!=null){
            textBox.text = string.Empty;
        }
        placeholderTile = false;

    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
