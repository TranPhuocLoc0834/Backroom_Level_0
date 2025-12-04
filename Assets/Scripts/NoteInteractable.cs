using UnityEngine;

public class NoteInteractable : MonoBehaviour
{
    public NoteData note;

    public void Interact()
    {
        NoteUI.Instance.Show(note);
    }
}