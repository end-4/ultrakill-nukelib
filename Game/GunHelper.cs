using System;
using UnityEngine;

namespace NukeLib.Game;

/// <summary>
/// Class that contains convenience functions for dealing with weapons
/// </summary>
public static class GunHelper {
    /// <summary>
    /// Gets the variation of a weapon, given its gameObject and slot index
    /// </summary>
    /// <param name="weapon">The weapon</param>
    /// <param name="weaponIndex">The slot index, starting with 0 being revolver</param>
    /// <returns>The variation index. 0 = Blue; 1 = Green; 2 = Red</returns>
    [Obsolete]
    public static int GetVariation(GameObject weapon, int weaponIndex) {
        // This is not very clean, but the game gives us no choice
        // Reference + "I've seen worse": https://github.com/daemon251/Ultrakill-WeaponVariantBinds/blob/580ecf6f0e150495639bcaec6ee5f48193b76bed/PluginConfig.cs#L219
        int currVariant = -1;
        if (weapon == null) return -1;
        switch (weaponIndex) {
            case 0:
                var rComp = weapon.GetComponent<Revolver>();
                if (rComp != null) currVariant = rComp.gunVariation;
                break;
            case 1:
                var sComp = weapon.GetComponent<Shotgun>();
                if (sComp != null) {
                    currVariant = sComp.variation;
                } else {
                    var shComp = weapon.GetComponent<ShotgunHammer>();
                    if (shComp != null) currVariant = shComp.variation;
                }

                break;
            case 2:
                var nComp = weapon.GetComponent<Nailgun>();
                if (nComp != null) currVariant = (4 - nComp.variation) % 3;
                break;
            case 3:
                var raiComp = weapon.GetComponent<Railcannon>();
                if (raiComp != null) currVariant = raiComp.variation;
                break;
            case 4:
                var rocComp = weapon.GetComponent<RocketLauncher>();
                if (rocComp != null) currVariant = rocComp.variation;
                break;
        }

        return currVariant;
    }

    /// <summary>
    /// Gets the variation of a weapon, given its GameObject
    /// </summary>
    /// <param name="weapon">The weapon's GameObject</param>
    /// <returns>The variation index. 0 = Blue; 1 = Green; 2 = Red</returns>
    public static int GetVariation(GameObject weapon) {
        if (weapon == null) return -1;
        var weaponIconComp = weapon.GetComponent<WeaponIcon>();
        if (weaponIconComp == null) return -1;
        var weaponIcon = weaponIconComp.weaponDescriptor.icon;
        return (int)weaponIconComp.weaponDescriptor.variationColor;
    }
}
