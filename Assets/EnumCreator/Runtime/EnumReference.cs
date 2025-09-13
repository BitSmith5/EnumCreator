using UnityEngine;

namespace EnumCreator
{
    [System.Serializable]
    public struct EnumReference<T>
    {
        [SerializeField] private T value;
        public T Value => value;
    }
}
