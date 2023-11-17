using Sirenix.OdinInspector;
[System.Serializable]
public class Dialogue
{
    [MultiLineProperty(5)]
    public string[] lines; // The dialogue lines
    public int shopLineIndex; // The index of the line after which the shop opens
}
