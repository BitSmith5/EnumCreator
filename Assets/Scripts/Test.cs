using UnityEngine;

public class Test : MonoBehaviour
{
    public enum MyEnum
    {
        ValueA,
        ValueB,
        ValueC,
        Nick = 3,
        Sarah = 4,
        Riley = 5,
        Margarita = 6,
        Vickie = 7,
        Boris = 8,
        Gizmo = 9,
        Sophie = 10,
        Mike = 11
    }

    [System.Flags]
    public enum Weapons
    {
        Sword = 1,
        Axe = 2,
        Gun = 4,
        Laser = 8,
        Spoon = 16,
        Brick = 32,
        Rock = 64,
        Car = 128,
        orange = 256,
        Grape = 512
    }

    public MyEnum enumValue;
    public Weapons weapons;
    public Player.Character character;
    public Player.Abilities abilities;

    public string playerName;
}
