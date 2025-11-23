using UnityEngine;
using StarterAssets;

public class FlashlightController : MonoBehaviour
{
    public GameObject lightBulb;  
    public Material flashlightGlass;        // Cái object chứa Light component
    public StarterAssetsInputs input;

    private bool isOn = false;
    private Light bulbLight;
    private Material glassMat;

    void Start()
    {
        if (input == null)
            input = GetComponentInParent<StarterAssetsInputs>();

        bulbLight = lightBulb.GetComponent<Light>();
        bulbLight.enabled = isOn;
 
        SetGlassEmission(isOn);
    }

    void Update()
    {
        if (input.turnOnFlashlight)
        {
            isOn = !isOn;
            bulbLight.enabled = isOn;

            input.turnOnFlashlight = false;
        }
    }
    void SetGlassEmission(bool state)
    {
        if (state)
        {
            flashlightGlass.EnableKeyword("_EMISSION");
            flashlightGlass.SetColor("_EmissionColor", Color.white * 0.3f); 
        }
        else
        {
            flashlightGlass.SetColor("_EmissionColor", Color.white * 0.05f);
            flashlightGlass.DisableKeyword("_EMISSION");
        }
    }
}
