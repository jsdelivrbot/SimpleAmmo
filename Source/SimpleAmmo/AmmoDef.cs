using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace SimpleAmmo
{
    public class AmmoDef : ThingDef
    {

        private string labelShort;
        private string labelPlural;

        public string LabelShort =>
            labelShort ?? label;

        public string LabelPluralCap =>
            labelPlural?.CapitalizeFirst() ?? LabelCap;

        public IEnumerable<ThingDef> UsingWeapons
        {
            get
            {
                foreach (ThingDef def in AmmoUtility.AmmoUserList)
                    if (def.GetCompProperties<CompProperties_Ammo>().ammoDef == this)
                        yield return def;
            }
            
        }

        public bool Unused =>
            UsingWeapons.Count() == 0;

        public bool UnusedByPlayer =>
            Unused || !PlayerAcquirable;

        //public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        //{
        //    foreach (StatDrawEntry s in base.SpecialDisplayStats(req))
        //        yield return s;
        //    yield return new StatDrawEntry(
        //        StatCategoryDefOf.BasicsNonPawn,
        //        "UsedBy".Translate(),
        //        "ClickForDetails".Translate(),
        //        50,
        //        GetUsingWeaponList());
        //}

        //private string GetUsingWeaponList()
        //{
        //    StringBuilder weaponListBuilder = new StringBuilder();
        //    weaponListBuilder.AppendLine("AmmoUserListReportTop".Translate());
        //    weaponListBuilder.AppendLine();
        //    foreach (ThingDef def in UsingWeapons)
        //        weaponListBuilder.AppendLine($" - {def.LabelCap}");
        //    if (Unused)
        //        weaponListBuilder.AppendLine("N/A");
        //    return weaponListBuilder.ToString();
        //}

    }
}
