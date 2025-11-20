using UnityEngine;
using UnityEngine.UI;

public class CreateInteractionUI : MonoBehaviour
{
    [ContextMenu("Create Interaction UI")]
    public void CreateUI()
    {
        // Find or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("✓ Created Canvas");
        }

        // Create UI Panel
        GameObject panel = new GameObject("InteractionUI");
        panel.transform.SetParent(canvas.transform, false);
        
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.2f);
        panelRect.anchorMax = new Vector2(0.5f, 0.2f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(400, 100);
        
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        
        // Create Text
        GameObject textObj = new GameObject("InteractionText");
        textObj.transform.SetParent(panel.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        Text text = textObj.AddComponent<Text>();
        text.text = "Press E to Hide";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 24;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        
        panel.SetActive(false);
        
        Debug.Log("<color=green>✓ Interaction UI Created!</color>");
        Debug.Log("Assign the 'InteractionUI' panel and 'InteractionText' to your HideableLocker script.");
    }
}
