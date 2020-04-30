using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigurationManager : MonoBehaviour {

    

    public static ConfigurationManager Instance;

    public int numCells;
    public float width;
    public float height;
    public bool visualize = false;
    

    private void OnEnable() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    public void SetNumCells(string value) {
        numCells = 0;
        int.TryParse(value, out numCells);
    }

    public void SetWidth(string value) {
        width = 0;
        float.TryParse(value, out width);
    }

    public void SetHeight(string value) {
        height = 0;
        float.TryParse(value, out height);
    }
}
