using UnityEngine;
using System.Collections;
using TMPro;

public class CityNameManager : MonoBehaviour {

    [SerializeField]
    private TextMeshPro cityNameLabel;

    private CFGStringGenerator nameGenerator;

    private void Awake() {
        nameGenerator = CFGStringGenerator.Parse("citynames");

        Generate();
    }

    public void Generate() {
        cityNameLabel.text = nameGenerator.Generate();
    }
}
