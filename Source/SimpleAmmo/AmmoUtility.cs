using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace SimpleAmmo
{

    [StaticConstructorOnStartup]
    public static class AmmoUtility
    {

        static AmmoUtility()
        {
            foreach (AmmoDef aDef in DefDatabase<AmmoDef>.AllDefs)
            {
                if (aDef.UnusedByPlayer)
                    aDef.tradeability = Tradeability.Sellable;
                if (!aDef.Unused)
                    aDef.description += AddAmmoUserList(aDef);
            }
            foreach (RecipeDef rDef in DefDatabase<RecipeDef>.AllDefs.Where(r => !r.products.NullOrEmpty()))
            {
                ThingDef firstProductDef = rDef.products.First().thingDef;
                if (firstProductDef is AmmoDef aDef && aDef.UnusedByPlayer)
                    rDef.recipeUsers.Clear();
            }
        }

        private static string AddAmmoUserList (AmmoDef def)
        {
            StringBuilder weaponListBuilder = new StringBuilder();
            weaponListBuilder.AppendLine();
            weaponListBuilder.AppendLine();
            weaponListBuilder.AppendLine("AmmoUserListReportTop".Translate());
            weaponListBuilder.AppendLine();
            foreach (ThingDef tDef in def.UsingWeapons)
                weaponListBuilder.AppendLine($" - {tDef.LabelCap}");
            return weaponListBuilder.ToString();
        }

        public static IEnumerable<ThingDef> AmmoUserList =>
            DefDatabase<ThingDef>.AllDefsListForReading.Where(t => t.IsWeapon && t.HasComp(typeof(CompAmmo)));

        public static IEnumerable<Gizmo> AmmoDevGizmos (CompAmmo ammoComp)
        {
            yield return new Command_Action()
            {
                defaultLabel = "Empty magazine",
                action = () => ammoComp.EmptyMagazine()
            };
            yield return new Command_Action()
            {
                defaultLabel = "Fill magazine",
                action = () => ammoComp.FillMagazine()
            };
            yield return new Command_Action()
            {
                defaultLabel = "Force reload",
                action = () => ammoComp.Reload(true)
            };
        }

        public static bool CanReloadWeapon(this Pawn pawn, CompAmmo ammoComp) =>
            pawn.CurJobDef != SA_JobDefOf.ReloadHeldWeapons && !pawn.Downed && pawn.GetHeldAmmoCountFor(ammoComp) > 0;

        public static void TryReloadWeapon(this Pawn pawn, CompAmmo ammoComp)
        {
            if (ammoComp.CanReload && pawn.CanReloadWeapon(ammoComp))
            {
                Log.Message(pawn.CurJob.ToStringSafe());
                pawn.jobs.StartJob(new Job(SA_JobDefOf.ReloadHeldWeapons, pawn.equipment.Primary), JobCondition.InterruptForced,
                    resumeCurJobAfterwards: true, cancelBusyStances: false);
            }  
        }

        public static int GetHeldAmmoCountFor(this Pawn pawn, CompAmmo ammoComp)
        {
            ThingDef ammoDef = ammoComp.Props.ammoDef;
            if (ammoDef == null)
                return int.MaxValue;
            return pawn.inventory.GetDirectlyHeldThings().TotalStackCountOfDef(ammoDef);
        }

        public static void GiveAmmoIfNeeded(Pawn p)
        {
            if (p.equipment?.Primary?.TryGetComp<CompAmmo>() is CompAmmo ammoComp)
            {
                // Doesn't require ammo things
                if (ammoComp.Props.ammoDef == null)
                    return;

                // Standard procedure
                CompProperties_Ammo ammoProps = ammoComp.Props;
                PawnKindDefExtension extension = p.kindDef.GetModExtension<PawnKindDefExtension>() ?? PawnKindDefExtension.defaultValues;
                ThingDef ammoDef = ammoProps.ammoDef;
                float ammoMass = ammoDef.GetStatValueAbstract(StatDefOf.Mass);
                int ammoCountToGenerate = GenMath.RoundRandom(ammoProps.magazineCapacity / ammoProps.ammoMultiplier * extension.weaponMagazineCount.RandomInRange);
                while (ammoCountToGenerate > 0)
                {
                    Thing newAmmo = ThingMaker.MakeThing(ammoDef);
                    if (MassUtility.WillBeOverEncumberedAfterPickingUp(p, newAmmo, 1))
                        break;
                    int stackCount = Mathf.Min(ammoCountToGenerate, ammoDef.stackLimit, MassUtility.CountToPickUpUntilOverEncumbered(p, newAmmo));
                    newAmmo.stackCount = stackCount;
                    p.inventory.TryAddItemNotForSale(newAmmo);
                    ammoCountToGenerate -= stackCount;
                }
            }
        }

    }
}
