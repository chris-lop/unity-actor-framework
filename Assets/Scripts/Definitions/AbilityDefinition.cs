using UnityEngine;

public enum TargetingKind { Self, Direction, Area, SingleTarget }
[CreateAssetMenu(menuName="LastDescent/Ability")]
public class AbilityDefinition : ScriptableObject {
    public string Id = "BasicAttack";
    public float Cooldown = 0.5f;
    public TargetingKind Targeting = TargetingKind.Direction;
    public GameObject ProjectilePrefab; // optional
    public float Damage = 10f;          // simple for now
}