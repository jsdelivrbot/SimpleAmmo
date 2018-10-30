using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using Harmony;

namespace SimpleAmmo
{

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {

        private static readonly Type patchType = typeof(HarmonyPatches);

        static HarmonyPatches()
        {

            HarmonyInstance h = HarmonyInstance.Create("XeoNovaDan.SimpleAmmo");

            h.Patch(AccessTools.Method(typeof(Verb_LaunchProjectile), "TryCastShot"),
                new HarmonyMethod(patchType, nameof(Prefix_Verb_LaunchProjectile_TryCastShot)));

            h.Patch(AccessTools.Method(typeof(Verb), nameof(Verb.TryStartCastOn)),
                new HarmonyMethod(patchType, nameof(Prefix_Verb_TryStartCastOn)));

            h.Patch(AccessTools.Method(typeof(Pawn), nameof(Pawn.GetGizmos)),
                postfix: new HarmonyMethod(patchType, nameof(Postfix_Pawn_GetGizmos)));

            h.Patch(AccessTools.Method(typeof(PawnInventoryGenerator), nameof(PawnInventoryGenerator.GenerateInventoryFor)),
                postfix: new HarmonyMethod(patchType, nameof(Postfix_PawnInventoryGenerator_GenerateInventoryFor)));

        }

        public static bool Prefix_Verb_TryStartCastOn(Verb __instance)
        {
            if (__instance.EquipmentSource != null)
            {
                CompAmmo ammoComp = __instance.EquipmentSource.GetComp<CompAmmo>();
                if (ammoComp != null)
                {
                    if (!ammoComp.HasAmmo)
                    {
                        if (ammoComp.CanReload)
                            __instance.CasterPawn.TryReloadWeapon(ammoComp);
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool Prefix_Verb_LaunchProjectile_TryCastShot(Verb_LaunchProjectile __instance)
        {
            CompAmmo ammoComp = __instance.EquipmentSource.TryGetComp<CompAmmo>();
            if (ammoComp != null)
            {
                if (ammoComp.HasAmmo)
                    ammoComp.AmmoCount--;
                else
                {
                    __instance.CasterPawn.TryReloadWeapon(ammoComp);
                    return false;
                }
            }
            return true;
        }

        public static void Postfix_Pawn_GetGizmos(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            Thing primaryEq = __instance.equipment?.Primary;
            CompAmmo ammoComp = primaryEq?.TryGetComp<CompAmmo>();
            if (__instance.IsColonistPlayerControlled && ammoComp != null)
            {
                if (Find.Selector.SelectedObjectsListForReading.Count == 1)
                    __result = __result.Add(new Gizmo_WeaponAmmoDisplay() { ammoComp = ammoComp });
                __result = __result.Add(new Command_Action()
                {
                    defaultLabel = "ReloadCommand_Label".Translate(),
                    defaultDesc = "ReloadCommand_Desc".Translate(),
                    icon = ContentFinder<Texture2D>.Get(primaryEq.def?.graphicData?.texPath ?? string.Empty),
                    action = () => __instance.TryReloadWeapon(ammoComp)
                });
                if (Prefs.DevMode)
                {
                    foreach (Gizmo devCommand in AmmoUtility.AmmoDevGizmos(ammoComp))
                        __result = __result.Add(devCommand);
                }
            }
        }

        public static void Postfix_PawnInventoryGenerator_GenerateInventoryFor(Pawn p)
        {
            AmmoUtility.GiveAmmoIfNeeded(p);
        }

    }

}
