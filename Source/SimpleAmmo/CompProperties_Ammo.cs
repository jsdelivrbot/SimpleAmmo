using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace SimpleAmmo
{

    public class CompProperties_Ammo : CompProperties
    {

        public CompProperties_Ammo()
        {
            compClass = typeof(CompAmmo);
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            foreach (StatDrawEntry s in base.SpecialDisplayStats(req))
                yield return s;
            if (ammoDef != null)
                yield return new StatDrawEntry(
                    StatCategoryDefOf.Weapon,
                    "Ammo".Translate(),
                    ammoDef.LabelPluralCap,
                    9999,
                    ammoDef.description);

            yield return new StatDrawEntry(StatCategoryDefOf.Weapon,
                "AmmoCapacity".Translate(),
                magazineCapacity.ToString(),
                9998,
                "AmmoCapacity_Desc".Translate());

            yield return new StatDrawEntry(StatCategoryDefOf.Weapon,
                "ReloadTime".Translate(),
                (reloadOneAtATime) ? "ReloadTime_OneAtATime".Translate(reloadTime, ammoDef?.LabelShort ?? "Ammo".Translate().UncapitalizeFirst()) : $"{reloadTime} s",
                9997,
                "ReloadTime_Desc".Translate());
        }

        public AmmoDef ammoDef;
        public SoundDef soundReloadStart;
        public SoundDef soundReload;
        public SoundDef soundReloadEnd;
        public float ammoMultiplier = 1f;
        public int magazineCapacity = 1;
        public bool reloadOneAtATime = false;
        public float reloadTime = 1f;
        public bool throwReloadingMote = true;

    }

}
