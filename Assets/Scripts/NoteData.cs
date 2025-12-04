using UnityEngine;

[CreateAssetMenu(menuName = "Clue/Note")]
public class NoteData : ScriptableObject
{
    [TextArea(5, 20)]
    public string content;
    public Sprite image;  // nếu có ảnh
}