using System.Collections.Generic;
using UnityEngine;

namespace EnumCreator
{
    [CreateAssetMenu(menuName = "Enum Creator/Enum Definition", fileName = "NewEnumDefinition")]
    public class EnumDefinition : ScriptableObject
    {
        [SerializeField] private string enumName = "MyEnum";
        [SerializeField] private string @namespace = "Game.Enums";
        [SerializeField] private List<string> values = new List<string>() { "Default" };
        [SerializeField] private List<string> removedValues = new List<string>(); // for soft-deleted items
        [SerializeField] private bool useFlags = false;

        public string EnumName => enumName;
        public string Namespace => @namespace;
        public IReadOnlyList<string> Values => values;
        public IReadOnlyList<string> RemovedValues => removedValues;
        public bool UseFlags => useFlags;

        public List<string> MutableValues => values;
        public List<string> MutableRemovedValues => removedValues;
    }
}
