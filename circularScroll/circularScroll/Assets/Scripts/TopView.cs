using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TopView : MonoBehaviour {
    const string sampleJson = "sample.json";
	
	void Start () {
        Debug.Log("start");
        string fileName = Path.Combine(Application.dataPath, sampleJson);
        LoadJson(fileName);
    }
    public void LoadJson(string fileName)
    {
        using (StreamReader r = new StreamReader(fileName))
        {
            string json = r.ReadToEnd();
            Debug.Log("json" + json);
            ListItem items = JsonUtility.FromJson<ListItem>(json);
            Debug.Log("***" + items.Values.Length);
            for(int i=0;i<items.Values.Length;i++)
            {
                Debug.Log(items.Values[i].Text);
            }           
        }
    }

}
[Serializable]
public class ListItem { 
    public Values[] Values;
}
[Serializable]
public class Values
{
    public string Text;
}
