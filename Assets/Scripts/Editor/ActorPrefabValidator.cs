using UnityEditor;
using UnityEngine;
public static class ActorPrefabValidator {
    [MenuItem("LastDescent/Validate Selected Actor Prefab")]
    public static void Validate(){
        foreach (var obj in Selection.gameObjects){
            var ok = true;
            if (!obj.GetComponent<ActorKernel>()) { Debug.LogError($"{obj.name}: Missing ActorKernel"); ok=false; }
            if (!obj.GetComponent<Rigidbody2D>()) { Debug.LogError($"{obj.name}: Missing Rigidbody2D"); ok=false; }
            if (!obj.GetComponent<AttributesFeature>()) { Debug.LogError($"{obj.name}: Missing AttributesFeature"); ok=false; }
            if (!obj.transform.Find("Hurtbox")) { Debug.LogError($"{obj.name}: Missing Hurtbox child"); ok=false; }
            Debug.Log($"{obj.name}: {(ok ? "OK" : "Has issues")}");
        }
    }
}
