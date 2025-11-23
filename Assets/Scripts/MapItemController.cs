using UnityEngine;

public class MapController : MonoBehaviour {
    public GameObject mapUI;

    void Update() {
        if (Input.GetKeyDown(KeyCode.M)) {
            mapUI.SetActive(!mapUI.activeSelf);
        }
    }
}
