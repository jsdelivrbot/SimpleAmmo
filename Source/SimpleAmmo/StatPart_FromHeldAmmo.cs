using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace SimpleAmmo
{
    public class StatPart_FromHeldAmmo : StatPart
    {

        public override string ExplanationPart(StatRequest req)
        {
            if (req.HasThing && req.Thing.TryGetComp<CompAmmo>() is CompAmmo ammoComp && ammoComp.Props.ammoDef != null)
            {
                float massAdjustment = ammoComp.AmmoCount * ammoComp.Props.ammoDef.GetStatValueAbstract(StatDefOf.Mass) / ammoComp.Props.ammoMultiplier;
                return $"{"FromHeldAmmo".Translate()}: {parentStat.Worker.ValueToString(massAdjustment, false, ToStringNumberSense.Offset)}";
            }
            return null;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.HasThing && req.Thing.TryGetComp<CompAmmo>() is CompAmmo ammoComp && ammoComp.Props.ammoDef != null)
            {
                val += ammoComp.AmmoCount * ammoComp.Props.ammoDef.GetStatValueAbstract(StatDefOf.Mass) / ammoComp.Props.ammoMultiplier;
            }
        }

    }
}
