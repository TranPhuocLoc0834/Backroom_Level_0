using UnityEngine;
using StarterAssets;
using System.Collections;

public class FlashlightController : MonoBehaviour
{
    public GameObject lightBulb;  
    public Material flashlightGlass;        // Cái object chứa Light component
    public StarterAssetsInputs input;
     public AudioSource audioSource;
    public AudioClip soundToggle;   

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

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }
    void Update()
    {
         if (input.turnOnFlashlight)
        {
            input.turnOnFlashlight = false;
            if (soundToggle && audioSource)
            {
                StartCoroutine(ToggleFlashlightAfterSound());
            }
            else
            {
                isOn = !isOn;
                bulbLight.enabled = isOn;
                SetGlassEmission(isOn);
            }
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
    IEnumerator ToggleFlashlightAfterSound()
    {
        audioSource.PlayOneShot(soundToggle);
        yield return new WaitForSeconds(soundToggle.length);

        isOn = !isOn;
        bulbLight.enabled = isOn;
        SetGlassEmission(isOn);
    }
}
