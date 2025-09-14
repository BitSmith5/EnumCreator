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
        [SerializeField] private List<string> tooltips = new List<string>() { "" };  // New list for comments/tooltips
        [SerializeField] private List<string> removedValues = new List<string>();
        [SerializeField] private List<int> removedValueNumbers = new List<int>();  // Store original numeric values for removed values
        [SerializeField] private bool useFlags = false;

        public string EnumName => enumName;
        public string Namespace => @namespace;
        public IReadOnlyList<string> Values => values;
        public IReadOnlyList<string> RemovedValues => removedValues;
        public IReadOnlyList<int> RemovedValueNumbers => removedValueNumbers;
        public IReadOnlyList<string> Tooltips => tooltips;
        public bool UseFlags => useFlags;

        public List<string> MutableValues => values;
        public List<string> MutableRemovedValues => removedValues;
        public List<int> MutableRemovedValueNumbers => removedValueNumbers;
        public List<string> MutableTooltips => tooltips;  // Expose tooltips for editor modification

    }
}
