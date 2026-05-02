using UnityEngine;

public class FinalBossAnimatorDriver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Parameter Names")]
    [SerializeField] private string isDeadBool = "IsDead";
    [SerializeField] private string frostSlashTrigger = "FrostSlash";

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    public void SetDead()
    {
        if (animator == null) return;

        SafeResetTrigger(frostSlashTrigger);
        SafeSetBool(isDeadBool, true);
    }

    public void SetFrostSlash()
    {
        if (animator == null) return;

        SafeSetTrigger(frostSlashTrigger);
    }

    public void SetIdle()
    {
        if (animator == null) return;

        SafeResetTrigger(frostSlashTrigger);
    }

    private void SafeSetBool(string parameterName, bool value)
    {
        if (HasParameter(parameterName))
            animator.SetBool(parameterName, value);
        else
            Debug.LogWarning($"[FinalBossAnimatorDriver] Animator bool '{parameterName}' does not exist.", this);
    }

    private void SafeSetTrigger(string parameterName)
    {
        if (HasParameter(parameterName))
            animator.SetTrigger(parameterName);
        else
            Debug.LogWarning($"[FinalBossAnimatorDriver] Animator trigger '{parameterName}' does not exist.", this);
    }

    private void SafeResetTrigger(string parameterName)
    {
        if (HasParameter(parameterName))
            animator.ResetTrigger(parameterName);
    }

    private bool HasParameter(string parameterName)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return false;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName)
                return true;
        }

        return false;
    }
}