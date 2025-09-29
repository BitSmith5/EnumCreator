using UnityEngine;

public class Player : MonoBehaviour
{
    public enum Character
    {
        Nick = 0,
        Sarah = 1,
        Riley = 2,
        Margarita = 4,
        Mike = 8
    }

    [System.Flags]
    public enum Abilities
    {
        Jump = 1,
        Run = 2,
        Swim = 4,
        Fly = 8,
        Dance = 16,
        Walk = 32
    }
}
