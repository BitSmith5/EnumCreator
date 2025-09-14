using UnityEngine;
using Game.Enums;

public class Test : MonoBehaviour
{
    public Weapons currentWeapon = Weapons.Dog;
    public Player currentPlayer = Player.Ally;
    public Pets myPets = Pets.Dog | Pets.Cat;
}
