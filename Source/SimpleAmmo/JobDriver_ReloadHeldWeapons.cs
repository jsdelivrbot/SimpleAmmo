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
    public class JobDriver_ReloadHeldWeapons : JobDriver
    {

        private float _timeReloading = 0f;

        private CompAmmo AmmoComp => TargetThingA.TryGetComp<CompAmmo>();

        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil reloadToil = new Toil();
            reloadToil.initAction = () =>
            {
                if (AmmoComp.Props.throwReloadingMote)
                {
                    Pawn actor = reloadToil.actor;
                    MoteMaker.ThrowText(actor.DrawPos, actor.Map, "Reloading".Translate());
                }
            };
            reloadToil.tickAction = () =>
            {
                Pawn actor = reloadToil.actor;
                actor.pather.StopDead();
                int ticksToReload = AmmoComp.Props.reloadTime.SecondsToTicks();
                float reloadSpeed = actor.GetStatValue(SA_StatDefOf.ReloadingSpeed);
                _timeReloading += reloadSpeed;
                if (_timeReloading >= ticksToReload)
                {
                    AmmoComp.Reload();
                    if (AmmoComp.Props.reloadOneAtATime)
                        _timeReloading = 0f;
                    if (!AmmoComp.CanReload)
                    {
                        EndJobWith(JobCondition.Succeeded);
                        return;
                    }
                }
            };
            reloadToil.defaultCompleteMode = ToilCompleteMode.Never;
            Func<float> progGetter = () => (AmmoComp.Props.reloadOneAtATime) ? ((float)AmmoComp.AmmoCount / AmmoComp.Props.magazineCapacity) : (_timeReloading / AmmoComp.Props.reloadTime.SecondsToTicks());
            reloadToil.WithProgressBar(TargetIndex.A, progGetter);
            yield return reloadToil;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _timeReloading, "timeReloading", 0f);
        }

    }
}
