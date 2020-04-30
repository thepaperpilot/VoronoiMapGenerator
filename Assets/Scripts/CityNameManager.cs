using UnityEngine;
using System.Collections;
using TMPro;

public class CityNameManager : MonoBehaviour {

    [SerializeField]
    private TextMeshProUGUI cityNameLabel;
    [SerializeField]
    private Screenshotter screenshotter;
    [SerializeField]
    private TMP_InputField filenameInput;
    [SerializeField]
    private TMP_InputField cityNameInput;

    private CFGStringGenerator nameGenerator;

    private void Awake() {
        nameGenerator = CFGStringGenerator.Parse("citynames");

        Generate();
    }

    public void Generate() {
        SetName(nameGenerator.Generate());
    }

    public void SetName(string name) {
        cityNameLabel.text = name;
        screenshotter.SetFilename(name);
        filenameInput.text = name;
        // The text seems to not be left-aligned unless we update the textComponent manually
        filenameInput.textComponent.text = name;
        cityNameInput.text = name;
        cityNameInput.textComponent.text = name;
    }
}
