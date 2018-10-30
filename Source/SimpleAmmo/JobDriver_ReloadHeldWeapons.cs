using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;

namespace SimpleAmmo
{
    public class JobDriver_ReloadHeldWeapons : JobDriver
    {

        private float _timeReloading = 0f;

        private CompAmmo AmmoComp => TargetThingA.TryGetComp<CompAmmo>();

        private CompProperties_Ammo AmmoProps => AmmoComp.Props;

        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil reloadToil = new Toil();
            reloadToil.initAction = () =>
            {
                if (AmmoProps.soundReloadStart != null)
                    AmmoProps.soundReloadStart.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map, false));
                if (AmmoProps.throwReloadingMote)
                {
                    Pawn actor = reloadToil.actor;
                    MoteMaker.ThrowText(actor.DrawPos, actor.Map, "Reloading".Translate());
                }
            };
            reloadToil.tickAction = () =>
            {
                Pawn actor = reloadToil.actor;
                actor.pather.StopDead();
                if (AmmoProps.soundReload != null && _timeReloading == 0f)
                    AmmoProps.soundReload.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map, false));
                int ticksToReload = AmmoProps.reloadTime.SecondsToTicks();
                float reloadSpeed = actor.GetStatValue(SA_StatDefOf.ReloadingSpeed);
                _timeReloading += reloadSpeed;
                if (_timeReloading >= ticksToReload)
                {
                    AmmoComp.Reload();
                    if (AmmoProps.reloadOneAtATime)
                        _timeReloading = 0f;
                    if (!AmmoComp.CanReload)
                    {
                        if (AmmoProps.soundReloadEnd != null)
                            AmmoProps.soundReloadEnd.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map, false));
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
