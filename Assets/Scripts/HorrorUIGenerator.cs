using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Automatically creates a Slenderman-style horror game UI
/// Run this in editor or at runtime to generate the complete UI system
/// </summary>
public class HorrorUIGenerator : MonoBehaviour
{
    [Header("Generate UI")]
    [Tooltip("Click this button in inspector to generate UI")]
    public bool generateUI = false;
    
    [Header("UI Settings")]
    public bool createOnStart = false;
    public Font customFont;
    
    [ContextMenu("Generate Horror UI")]
    public void GenerateUI()
    {
        Debug.Log("<color=cyan>🎨 Generating Slenderman-style Horror UI...</color>");
        
        // Find or create canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Horror Game Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }
        
        Transform canvasTransform = canvas.transform;
        
        // Create HUD Panel
        GameObject hudPanel = CreateUIObject("HUD Panel", canvasTransform);
        RectTransform hudRect = hudPanel.GetComponent<RectTransform>();
        hudRect.anchorMin = Vector2.zero;
        hudRect.anchorMax = Vector2.one;
        hudRect.sizeDelta = Vector2.zero;
        
        // BATTERY UI (Bottom Left)
        CreateBatteryUI(hudPanel.transform);
        
        // STAMINA UI (Bottom Left, above battery)
        CreateStaminaUI(hudPanel.transform);
        
        // FLASHLIGHT INDICATOR (Bottom Right)
        CreateFlashlightIndicator(hudPanel.transform);
        
        // OBJECTIVE TEXT (Top Center)
        CreateObjectiveUI(hudPanel.transform);
        
        // PAGES COUNTER (Top Right)
        CreatePagesCounter(hudPanel.transform);
        
        // INTERACTION PROMPT (Center)
        CreateInteractionPrompt(hudPanel.transform);
        
        // WARNING TEXT (Top Center, below objective)
        CreateWarningText(hudPanel.transform);
        
        // DAMAGE VIGNETTE
        CreateDamageVignette(hudPanel.transform);
        
        // Add HorrorGameUI component to canvas
        HorrorGameUI gameUI = canvas.gameObject.GetComponent<HorrorGameUI>();
        if (gameUI == null)
        {
            gameUI = canvas.gameObject.AddComponent<HorrorGameUI>();
        }
        
        // Auto-assign references
        AutoAssignUIReferences(gameUI, hudPanel);
        
        Debug.Log("<color=green>✅ Horror UI Generated Successfully!</color>");
        Debug.Log("<color=yellow>🔧 Remember to assign Battery System, Stamina System, and Flashlight Controller references!</color>");
    }
    
    void CreateBatteryUI(Transform parent)
    {
        GameObject batteryPanel = CreateUIObject("Battery Panel", parent);
        RectTransform rect = batteryPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
        rect.pivot = new Vector2(0, 0);
        rect.anchoredPosition = new Vector2(30, 30);
        rect.sizeDelta = new Vector2(250, 50);
        
        // Background
        Image bg = batteryPanel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.7f);
        
        // Icon
        GameObject icon = CreateUIObject("Battery Icon", batteryPanel.transform);
        RectTransform iconRect = icon.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0, 0.5f);
        iconRect.anchorMax = new Vector2(0, 0.5f);
        iconRect.pivot = new Vector2(0, 0.5f);
        iconRect.anchoredPosition = new Vector2(10, 0);
        iconRect.sizeDelta = new Vector2(30, 30);
        Image iconImg = icon.AddComponent<Image>();
        iconImg.color = Color.yellow;
        icon.name = "Battery Icon";
        
        // Fill Bar Background
        GameObject barBg = CreateUIObject("Bar Background", batteryPanel.transform);
        RectTransform barBgRect = barBg.GetComponent<RectTransform>();
        barBgRect.anchorMin = new Vector2(0, 0.5f);
        barBgRect.anchorMax = new Vector2(1, 0.5f);
        barBgRect.pivot = new Vector2(0, 0.5f);
        barBgRect.anchoredPosition = new Vector2(50, 0);
        barBgRect.sizeDelta = new Vector2(-110, 20);
        Image barBgImg = barBg.AddComponent<Image>();
        barBgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        // Fill Bar
        GameObject fillBar = CreateUIObject("Battery Fill Bar", barBg.transform);
        RectTransform fillRect = fillBar.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        Image fillImg = fillBar.AddComponent<Image>();
        fillImg.color = Color.green;
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillAmount = 1f;
        
        // Text
        GameObject text = CreateTextObject("Battery Text", batteryPanel.transform, "100%");
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(1, 0.5f);
        textRect.anchorMax = new Vector2(1, 0.5f);
        textRect.pivot = new Vector2(1, 0.5f);
        textRect.anchoredPosition = new Vector2(-10, 0);
        textRect.sizeDelta = new Vector2(50, 30);
    }
    
    void CreateStaminaUI(Transform parent)
    {
        GameObject staminaPanel = CreateUIObject("Stamina Panel", parent);
        RectTransform rect = staminaPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
        rect.pivot = new Vector2(0, 0);
        rect.anchoredPosition = new Vector2(30, 90);
        rect.sizeDelta = new Vector2(250, 50);
        
        // Background
        Image bg = staminaPanel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.7f);
        
        // Icon
        GameObject icon = CreateUIObject("Stamina Icon", staminaPanel.transform);
        RectTransform iconRect = icon.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0, 0.5f);
        iconRect.anchorMax = new Vector2(0, 0.5f);
        iconRect.pivot = new Vector2(0, 0.5f);
        iconRect.anchoredPosition = new Vector2(10, 0);
        iconRect.sizeDelta = new Vector2(30, 30);
        Image iconImg = icon.AddComponent<Image>();
        iconImg.color = Color.cyan;
        icon.name = "Stamina Icon";
        
        // Fill Bar Background
        GameObject barBg = CreateUIObject("Bar Background", staminaPanel.transform);
        RectTransform barBgRect = barBg.GetComponent<RectTransform>();
        barBgRect.anchorMin = new Vector2(0, 0.5f);
        barBgRect.anchorMax = new Vector2(1, 0.5f);
        barBgRect.pivot = new Vector2(0, 0.5f);
        barBgRect.anchoredPosition = new Vector2(50, 0);
        barBgRect.sizeDelta = new Vector2(-110, 20);
        Image barBgImg = barBg.AddComponent<Image>();
        barBgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        // Fill Bar
        GameObject fillBar = CreateUIObject("Stamina Fill Bar", barBg.transform);
        RectTransform fillRect = fillBar.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        Image fillImg = fillBar.AddComponent<Image>();
        fillImg.color = Color.cyan;
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillAmount = 1f;
        
        // Text
        GameObject text = CreateTextObject("Stamina Text", staminaPanel.transform, "100%");
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(1, 0.5f);
        textRect.anchorMax = new Vector2(1, 0.5f);
        textRect.pivot = new Vector2(1, 0.5f);
        textRect.anchoredPosition = new Vector2(-10, 0);
        textRect.sizeDelta = new Vector2(50, 30);
    }
    
    void CreateFlashlightIndicator(Transform parent)
    {
        GameObject indicator = CreateUIObject("Flashlight Indicator", parent);
        RectTransform rect = indicator.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 0);
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(1, 0);
        rect.anchoredPosition = new Vector2(-30, 30);
        rect.sizeDelta = new Vector2(60, 60);
        
        Image img = indicator.AddComponent<Image>();
        img.color = Color.gray;
        
        // Inner circle
        GameObject inner = CreateUIObject("Inner Circle", indicator.transform);
        RectTransform innerRect = inner.GetComponent<RectTransform>();
        innerRect.anchorMin = new Vector2(0.5f, 0.5f);
        innerRect.anchorMax = new Vector2(0.5f, 0.5f);
        innerRect.pivot = new Vector2(0.5f, 0.5f);
        innerRect.anchoredPosition = Vector2.zero;
        innerRect.sizeDelta = new Vector2(40, 40);
        Image innerImg = inner.AddComponent<Image>();
        innerImg.color = Color.yellow;
    }
    
    void CreateObjectiveUI(Transform parent)
    {
        GameObject objective = CreateTextObject("Objective Text", parent, "COLLECT ALL 8 PAGES");
        RectTransform rect = objective.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1);
        rect.anchorMax = new Vector2(0.5f, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.anchoredPosition = new Vector2(0, -30);
        rect.sizeDelta = new Vector2(600, 50);
        
        TextMeshProUGUI text = objective.GetComponent<TextMeshProUGUI>();
        text.fontSize = 28;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
    }
    
    void CreatePagesCounter(Transform parent)
    {
        GameObject counter = CreateTextObject("Pages Collected Text", parent, "0/8");
        RectTransform rect = counter.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-30, -30);
        rect.sizeDelta = new Vector2(200, 80);
        
        TextMeshProUGUI text = counter.GetComponent<TextMeshProUGUI>();
        text.fontSize = 48;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Right;
    }
    
    void CreateInteractionPrompt(Transform parent)
    {
        GameObject prompt = CreateTextObject("Interaction Prompt", parent, "[E] INTERACT");
        RectTransform rect = prompt.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, -100);
        rect.sizeDelta = new Vector2(400, 50);
        
        TextMeshProUGUI text = prompt.GetComponent<TextMeshProUGUI>();
        text.fontSize = 24;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        
        // Add background
        GameObject bg = CreateUIObject("Background", prompt.transform);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = new Vector2(20, 10);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.8f);
        bg.transform.SetAsFirstSibling();
    }
    
    void CreateWarningText(Transform parent)
    {
        GameObject warning = CreateTextObject("Warning Text", parent, "WARNING MESSAGE");
        RectTransform rect = warning.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1);
        rect.anchorMax = new Vector2(0.5f, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.anchoredPosition = new Vector2(0, -100);
        rect.sizeDelta = new Vector2(800, 60);
        
        TextMeshProUGUI text = warning.GetComponent<TextMeshProUGUI>();
        text.fontSize = 32;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(1f, 0.3f, 0.3f, 1f);
    }
    
    void CreateDamageVignette(Transform parent)
    {
        GameObject vignette = CreateUIObject("Damage Vignette", parent);
        RectTransform rect = vignette.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        
        Image img = vignette.AddComponent<Image>();
        img.color = new Color(1, 0, 0, 0);
        vignette.transform.SetAsFirstSibling();
    }
    
    GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.localScale = Vector3.one;
        return obj;
    }
    
    GameObject CreateTextObject(string name, Transform parent, string initialText)
    {
        GameObject obj = CreateUIObject(name, parent);
        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        text.text = initialText;
        text.fontSize = 20;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Left;
        text.fontStyle = FontStyles.Normal;
        
        // Add outline for better visibility
        text.outlineWidth = 0.2f;
        text.outlineColor = Color.black;
        
        return obj;
    }
    
    void AutoAssignUIReferences(HorrorGameUI gameUI, GameObject hudPanel)
    {
        gameUI.batteryFillBar = FindChildByName(hudPanel.transform, "Battery Fill Bar")?.GetComponent<Image>();
        gameUI.batteryIcon = FindChildByName(hudPanel.transform, "Battery Icon")?.GetComponent<Image>();
        gameUI.batteryText = FindChildByName(hudPanel.transform, "Battery Text")?.GetComponent<TextMeshProUGUI>();
        
        gameUI.staminaFillBar = FindChildByName(hudPanel.transform, "Stamina Fill Bar")?.GetComponent<Image>();
        gameUI.staminaIcon = FindChildByName(hudPanel.transform, "Stamina Icon")?.GetComponent<Image>();
        gameUI.staminaText = FindChildByName(hudPanel.transform, "Stamina Text")?.GetComponent<TextMeshProUGUI>();
        
        gameUI.objectiveText = FindChildByName(hudPanel.transform, "Objective Text")?.GetComponent<TextMeshProUGUI>();
        gameUI.pagesCollectedText = FindChildByName(hudPanel.transform, "Pages Collected Text")?.GetComponent<TextMeshProUGUI>();
        
        gameUI.interactionPrompt = FindChildByName(hudPanel.transform, "Interaction Prompt")?.GetComponent<TextMeshProUGUI>();
        gameUI.warningText = FindChildByName(hudPanel.transform, "Warning Text")?.GetComponent<TextMeshProUGUI>();
        
        gameUI.flashlightIndicator = FindChildByName(hudPanel.transform, "Inner Circle")?.GetComponent<Image>();
        gameUI.damageVignette = FindChildByName(hudPanel.transform, "Damage Vignette")?.GetComponent<Image>();
        
        Debug.Log("<color=green>✅ UI References auto-assigned to HorrorGameUI component</color>");
    }
    
    GameObject FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
                return child.gameObject;
        }
        return null;
    }
    
    void Start()
    {
        if (createOnStart)
        {
            GenerateUI();
        }
    }
    
    void OnValidate()
    {
        if (generateUI)
        {
            generateUI = false;
            GenerateUI();
        }
    }
}
