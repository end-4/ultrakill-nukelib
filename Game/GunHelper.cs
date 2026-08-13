using UnityEngine;

namespace NukeLib.Game;

public static class GunHelper {
    public static int GetVariation(GameObject weapon, int weaponIndex) {
        int currVariant = -1;
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
}
