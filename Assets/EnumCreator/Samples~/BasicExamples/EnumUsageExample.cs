using UnityEngine;
using Game.Enums; // This would be your generated enum namespace

namespace EnumCreator.Examples
{
    /// <summary>
    /// Example script demonstrating how to use generated enums from EnumCreator Pro
    /// </summary>
    public class EnumUsageExample : MonoBehaviour
    {
        [Header("Enum Examples")]
        [SerializeField] private WeaponType currentWeapon = WeaponType.Sword;
        [SerializeField] private BuildingType selectedBuilding = BuildingType.House;
        [SerializeField] private PlayerState playerState = PlayerState.Idle;
        
        [Header("Flags Example")]
        [SerializeField] private PetFlags activePets = PetFlags.Dog | PetFlags.Cat;
        
        void Start()
        {
            DemonstrateEnumUsage();
        }
        
        void DemonstrateEnumUsage()
        {
            Debug.Log($"Current Weapon: {currentWeapon}");
            Debug.Log($"Selected Building: {selectedBuilding}");
            Debug.Log($"Player State: {playerState}");
            Debug.Log($"Active Pets: {activePets}");
            
            // Example of enum switching
            switch (currentWeapon)
            {
                case WeaponType.Sword:
                    Debug.Log("Using sword - high damage, slow attack");
                    break;
                case WeaponType.Bow:
                    Debug.Log("Using bow - ranged attack");
                    break;
                case WeaponType.Staff:
                    Debug.Log("Using staff - magical abilities");
                    break;
            }
            
            // Example of flags checking
            if (activePets.HasFlag(PetFlags.Dog))
            {
                Debug.Log("Dog is active!");
            }
            
            if (activePets.HasFlag(PetFlags.Cat))
            {
                Debug.Log("Cat is active!");
            }
        }
        
        [ContextMenu("Cycle Weapon")]
        void CycleWeapon()
        {
            // Cycle through weapon types
            int currentIndex = (int)currentWeapon;
            int nextIndex = (currentIndex + 1) % System.Enum.GetValues(typeof(WeaponType)).Length;
            currentWeapon = (WeaponType)nextIndex;
            
            Debug.Log($"Switched to: {currentWeapon}");
        }
        
        [ContextMenu("Add Random Pet")]
        void AddRandomPet()
        {
            PetFlags[] allPets = { PetFlags.Dog, PetFlags.Cat, PetFlags.Bird, PetFlags.Fish };
            PetFlags randomPet = allPets[Random.Range(0, allPets.Length)];
            
            activePets |= randomPet;
            Debug.Log($"Added pet: {randomPet}. Active pets: {activePets}");
        }
        
        [ContextMenu("Remove Random Pet")]
        void RemoveRandomPet()
        {
            if (activePets == PetFlags.None) return;
            
            // Find active pets
            var activePetList = new System.Collections.Generic.List<PetFlags>();
            foreach (PetFlags pet in System.Enum.GetValues(typeof(PetFlags)))
            {
                if (pet != PetFlags.None && activePets.HasFlag(pet))
                {
                    activePetList.Add(pet);
                }
            }
            
            if (activePetList.Count > 0)
            {
                PetFlags petToRemove = activePetList[Random.Range(0, activePetList.Count)];
                activePets &= ~petToRemove;
                Debug.Log($"Removed pet: {petToRemove}. Active pets: {activePets}");
            }
        }
    }
}
