using UnityEngine;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;

public class ChatCommands : MonoBehaviour
{
    [SerializeField] private WeaponController blaster, charger, shotgun;
    [SerializeField] private PlayerWeaponsManager weaponsManager;
    [SerializeField] private Health playerHealth;
    private void Heal() => playerHealth.Heal(playerHealth.MaxHealth);

    private void GetWeapon(WeaponController weapon) => weaponsManager.AddWeapon(weapon);

    private CommandSymbol heal, getWeapon, getBlaster, getCharger, getShotgun;
    public CommandSymbol rootSymbol { get; private set; }

    private void Awake()
    {
        ConstructTree();
    }

    private void ConstructTree()
    {
        rootSymbol = new("/");
        heal = new("heal", () => Heal());
        getWeapon = new("get weapon");
        getBlaster = new("blaster", () => GetWeapon(blaster));
        getCharger = new("charger", () => GetWeapon(charger));
        getShotgun = new("shotgun", () => GetWeapon(shotgun));

        rootSymbol.children = new CommandSymbol[] { heal, getWeapon };
        getWeapon.children = new CommandSymbol[] { getBlaster, getCharger, getShotgun };
    }
}