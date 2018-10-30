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
    class JobGiver_OpportunisticReload : ThinkNode_JobGiver
    {

        protected override Job TryGiveJob(Pawn pawn)
        {
            //List<Thing> potentialWeapons = AllHeldAmmoUsersFor(pawn).ToList();
            //if (potentialWeapons.NullOrEmpty())
            //    return null;

            Thing weapon = FirstReloadableWeaponFrom(AllHeldAmmoUsersFor(pawn).ToList());
            if (weapon == null)
                return null;

            if (!pawn.CanReloadWeapon(weapon.TryGetComp<CompAmmo>()))
                return null;

            return new Job(SA_JobDefOf.ReloadHeldWeapons, weapon);
        }

        private IEnumerable<Thing> AllHeldAmmoUsersFor(Pawn pawn)
        {
            if (pawn.equipment != null)
                if (pawn.equipment.Primary != null && pawn.equipment.Primary.TryGetComp<CompAmmo>() != null)
                    yield return pawn.equipment.Primary;

            if (pawn.inventory != null)
                foreach (Thing weapon in pawn.inventory.GetDirectlyHeldThings().Where(t => t.def.IsWeapon))
                    if (weapon.TryGetComp<CompAmmo>() != null)
                        yield return weapon;
        }

        private Thing FirstReloadableWeaponFrom(List<Thing> list)
        {
            for (int i = 0; i < list.Count; i++)
                if (list[i].TryGetComp<CompAmmo>().CanReload)
                    return list[i];
            return null;
        }

    }
}
