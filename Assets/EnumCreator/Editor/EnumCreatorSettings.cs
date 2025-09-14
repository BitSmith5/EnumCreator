using UnityEngine;

namespace EnumCreator
{
    [CreateAssetMenu(fileName = "EnumCreatorSettings", menuName = "Enum Creator/Settings")]
    public class EnumCreatorSettings : ScriptableObject
    {
        public string defaultNamespace = "MyGame.Enums";
    }
}
