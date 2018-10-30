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

    public class CompAmmo : ThingComp
    {

        #region Fields
        private int _ammoCount = 0;
        #endregion

        #region Properties
        public CompProperties_Ammo Props =>
            (CompProperties_Ammo)props;

        public Pawn HoldingPawn =>
            (parent.ParentHolder as Pawn_EquipmentTracker)?.pawn;

        public int AmmoCount
        {
            get => _ammoCount;
            set
            {
                _ammoCount = value;
                if (_ammoCount > Props.magazineCapacity)
                    _ammoCount = Props.magazineCapacity;
                if (_ammoCount < 0)
                    _ammoCount = 0;
            }
        }

        private int AmmoToFullyReload =>
            Props.magazineCapacity - AmmoCount;

        public bool CanReload =>
            AmmoToFullyReload > 0;

        public bool HasAmmo =>
            AmmoCount > 0;
        #endregion

        #region Methods

        #region DevMode
        public void EmptyMagazine() => AmmoCount = 0;
        public void FillMagazine() => AmmoCount = Props.magazineCapacity;
        #endregion

        public void Reload(bool forced = false)
        {
            int maxReloadableAmmoCount = (Props.reloadOneAtATime) ? 1 : AmmoToFullyReload;
            int ammoCountToReload = Math.Min(maxReloadableAmmoCount, (forced) ? int.MaxValue : HoldingPawn.GetHeldAmmoCountFor(this));
            if (Props.ammoDef != null && !forced)
            {
                int i = 0;
                int ammoCountLeftToTransfer = Mathf.CeilToInt(ammoCountToReload / Props.ammoMultiplier);
                while (ammoCountLeftToTransfer > 0 && i < HoldingPawn.inventory.innerContainer.Count)
                {
                    Thing thing = HoldingPawn.inventory.innerContainer[i];
                    if (thing.def == Props.ammoDef)
                    {
                        if (thing.stackCount <= ammoCountLeftToTransfer)
                        {
                            ammoCountLeftToTransfer -= thing.stackCount;
                            thing.Destroy();
                            i--;
                        }
                        else
                        {
                            thing.stackCount -= ammoCountLeftToTransfer;
                            break;
                        }
                    }
                    i++;
                }
            }
            AmmoCount += Mathf.RoundToInt(ammoCountToReload * Props.ammoMultiplier);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Find.Selector.SingleSelectedThing == parent)
                yield return new Gizmo_WeaponAmmoDisplay
                {
                    ammoComp = this
                };
            if (Prefs.DevMode)
            {
                foreach (Gizmo devCommand in AmmoUtility.AmmoDevGizmos(this))
                    yield return devCommand;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref _ammoCount, "ammoCount", 0);
        }
        #endregion

    }

}
