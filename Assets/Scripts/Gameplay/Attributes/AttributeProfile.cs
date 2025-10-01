
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LastDescent.Gameplay.Attributes
{
    [CreateAssetMenu(menuName = "LastDescent/Attributes/Attribute Profile")]
    public class AttributeProfile : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public AttributeId Id;
            public float BaseValue;
        }

        public List<Entry> Entries = new();
    }
}