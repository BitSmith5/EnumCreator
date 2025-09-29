using UnityEngine;

public class Test : MonoBehaviour
{
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
    public Weapons weapons;
    public Player.Character character;
    public Player.Abilities abilities;
}
