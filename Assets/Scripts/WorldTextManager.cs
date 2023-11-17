//using UnityEngine;
//using TMPro;

//public class WorldTextManager : MonoBehaviour
//{
//    public static WorldTextManager Instance;

//    [SerializeField]
//    GameObject textPrefab;

//    [HideInInspector]
//    public Canvas canvas;

//    private void Awake()
//    {
//        if (Instance == null)
//        {
//            Instance = this;
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//        canvas = GetComponent<Canvas>();
//    }

//    public TextMeshProUGUI CreateWorldText(Transform parent, string text, Vector3 localPosition, Quaternion localRotation)
//    {
//        GameObject textObject = Instantiate(textPrefab, parent);
//        textObject.transform.localPosition = localPosition;
//        textObject.transform.localRotation = localRotation;

//        TextMeshProUGUI textMeshPro = textObject.GetComponent<TextMeshProUGUI>();
//        if (textMeshPro != null)
//        {
//            textMeshPro.text = text;
//        }

//        return textMeshPro;
//    }


//    public void UpdateWorldText(TextMeshProUGUI textObject, string newText)
//    {
//        // Update the text of the textObject
//    }

//    public void RemoveWorldText(TextMeshProUGUI textObject)
//    {
//        // Remove or disable the textObject
//    }
//}
