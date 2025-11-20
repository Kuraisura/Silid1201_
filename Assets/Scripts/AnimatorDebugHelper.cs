using UnityEngine;

public class AnimatorDebugHelper : MonoBehaviour
{
    public Animator animator;
    public bool showInGame = true;
    
    void OnGUI()
    {
        if (!showInGame || animator == null) return;
        
        GUIStyle style = new GUIStyle();
        style.fontSize = 16;
        style.normal.textColor = Color.white;
        
        int y = 10;
        
        GUI.Label(new Rect(10, y, 400, 30), $"Current State: {GetCurrentStateName()}", style);
        y += 25;
        
        GUI.Label(new Rect(10, y, 400, 30), $"Speed Parameter: {animator.GetFloat("Speed"):F2}", style);
        y += 25;
        
        // Show all animator parameters
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
            
            GUI.Label(new Rect(10, y, 400, 30), $"{param.name}: {value}", style);
            y += 25;
        }
    }
    
    string GetCurrentStateName()
    {
        if (animator == null) return "No Animator";
        
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName("Idle") ? "Idle" :
               stateInfo.IsName("Walk") ? "Walk" :
               stateInfo.IsName("Run") ? "Run" :
               stateInfo.IsName("Attack") ? "Attack" :
               stateInfo.IsName("Scream") ? "Scream" :
               "Unknown";
    }
}
