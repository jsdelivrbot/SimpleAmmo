using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace SimpleAmmo
{
    public class PawnKindDefExtension : DefModExtension
    {

        public static readonly PawnKindDefExtension defaultValues = new PawnKindDefExtension();

        public IntRange weaponMagazineCount = IntRange.one;

        public Dictionary<ThingDef, IntRange> weaponMagazineCountSpecifics;

    }
}
