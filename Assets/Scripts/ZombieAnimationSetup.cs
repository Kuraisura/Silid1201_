using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ZombieAnimationSetup : MonoBehaviour
{
    [Header("Animation Clips - Assign in Inspector")]
    public AnimationClip idleAnimation;
    public AnimationClip walkAnimation;
    public AnimationClip runAnimation;
    public AnimationClip attackAnimation;
    public AnimationClip screamAnimation;
    
    [Header("Animation Settings")]
    [Range(0.1f, 3f)]
    public float walkSpeedThreshold = 0.1f;  // Speed > this = Walk
    
    [Range(0.5f, 3f)]
    public float runSpeedThreshold = 1.5f;   // Speed > this = Run
    
    [Header("Transition Settings")]
    [Range(0f, 1f)]
    public float transitionDuration = 0.2f;
    
    [Header("Debug")]
    public bool showCurrentState = false;
    
    private Animator animator;
    private string currentStateName = "Idle";
    
    void Start()
    {
        animator = GetComponent<Animator>();
        
        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"⚠ {gameObject.name} has no Animator Controller assigned!");
        }
        
        // Verify parameters exist
        CheckParameter("Speed", AnimatorControllerParameterType.Float);
        CheckParameter("AttackTrigger", AnimatorControllerParameterType.Trigger);
        CheckParameter("ScreamTrigger", AnimatorControllerParameterType.Trigger);
        
        Debug.Log($"✓ Zombie Animation Setup initialized on {gameObject.name}");
    }
    
    void CheckParameter(string paramName, AnimatorControllerParameterType expectedType)
    {
        bool found = false;
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName && param.type == expectedType)
            {
                found = true;
                break;
            }
        }
        
        if (!found)
        {
            Debug.LogWarning($"⚠ Parameter '{paramName}' ({expectedType}) not found in Animator Controller!");
        }
    }
    
    void Update()
    {
        if (showCurrentState)
        {
            UpdateCurrentStateName();
        }
    }
    
    void UpdateCurrentStateName()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        if (stateInfo.IsName("Idle"))
            currentStateName = "Idle";
        else if (stateInfo.IsName("Walk") || stateInfo.IsName("Zombie Walk"))
            currentStateName = "Walk";
        else if (stateInfo.IsName("Run") || stateInfo.IsName("Zombie Run"))
            currentStateName = "Run";
        else if (stateInfo.IsName("Attack") || stateInfo.IsName("Zombie Attack"))
            currentStateName = "Attack";
        else if (stateInfo.IsName("Scream") || stateInfo.IsName("Zombie Scream"))
            currentStateName = "Scream";
    }
    
    void OnGUI()
    {
        if (!showCurrentState) return;
        
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.green;
        
        // Get position above zombie
        Vector3 worldPos = transform.position + Vector3.up * 2.5f;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        
        if (screenPos.z > 0)
        {
            GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y - 50, 200, 30), currentStateName, style);
            
            // Show speed value
            float speed = animator.GetFloat("Speed");
            style.fontSize = 16;
            style.normal.textColor = Color.yellow;
            GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y - 25, 200, 30), $"Speed: {speed:F2}", style);
        }
    }
    
    [ContextMenu("Show Animation Info")]
    public void ShowAnimationInfo()
    {
        Debug.Log("═══ ZOMBIE ANIMATION INFO ═══");
        Debug.Log($"GameObject: {gameObject.name}");
        
        if (animator.runtimeAnimatorController != null)
        {
            Debug.Log($"Controller: {animator.runtimeAnimatorController.name}");
        }
        else
        {
            Debug.LogWarning("No Animator Controller assigned!");
        }
        
        Debug.Log("\n--- Animation Clips Assigned ---");
        Debug.Log($"Idle: {(idleAnimation != null ? idleAnimation.name : "NOT ASSIGNED")}");
        Debug.Log($"Walk: {(walkAnimation != null ? walkAnimation.name : "NOT ASSIGNED")}");
        Debug.Log($"Run: {(runAnimation != null ? runAnimation.name : "NOT ASSIGNED")}");
        Debug.Log($"Attack: {(attackAnimation != null ? attackAnimation.name : "NOT ASSIGNED")}");
        Debug.Log($"Scream: {(screamAnimation != null ? screamAnimation.name : "NOT ASSIGNED")}");
        
        Debug.Log("\n--- Animator Parameters ---");
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            string value = "";
            switch (param.type)
            {
                case AnimatorControllerParameterType.Float:
                    value = animator.GetFloat(param.name).ToString("F2");
                    break;
                case AnimatorControllerParameterType.Int:
                    value = animator.GetInteger(param.name).ToString();
                    break;
                case AnimatorControllerParameterType.Bool:
                    value = animator.GetBool(param.name).ToString();
                    break;
                case AnimatorControllerParameterType.Trigger:
                    value = "(Trigger)";
                    break;
            }
            Debug.Log($"{param.name} ({param.type}): {value}");
        }
        
        Debug.Log("\n--- Current State ---");
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        
        if (clipInfo.Length > 0)
        {
            Debug.Log($"Playing: {clipInfo[0].clip.name}");
            Debug.Log($"Weight: {clipInfo[0].weight}");
        }
        
        Debug.Log($"Speed: {animator.GetFloat("Speed"):F2}");
        Debug.Log($"Normalized Time: {stateInfo.normalizedTime:F2}");
    }
}
