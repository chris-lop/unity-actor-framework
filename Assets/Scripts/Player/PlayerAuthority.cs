using UnityEngine;

namespace LastDescent.Player
{
    public class PlayerAuthority : MonoBehaviour
    {
        [SerializeField] private bool isLocalAuthority = true;
        public bool IsLocalAuthority => isLocalAuthority;

        public void SetLocalAuthority(bool value) => isLocalAuthority = value;
    }
}
