using UnityEngine;

[CreateAssetMenu(menuName = "LastDescent/Actor")]
public class ActorDefinition : ScriptableObject
{
    public AttributeSetDefinition AttributeSet;
    public AbilityDefinition[] Abilities;
    public int Team = 0; // 0=Player,1=Enemy,etc.
    public float DetectionRange = 3f;
}
