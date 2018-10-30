using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace SimpleAmmo
{

    [StaticConstructorOnStartup]
    public class Gizmo_WeaponAmmoDisplay : Gizmo
    {

        private static readonly Texture2D FullBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.35f, 0.35f, 0.2f));
        private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(Color.black);

        public CompAmmo ammoComp;

        public override float GetWidth(float maxWidth) => 140f;

        // Adapted copypasta from Gizmo_RefuelableFuelStatus
        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
        {
            Rect overRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), Height);
            Find.WindowStack.ImmediateWindow(48927408, overRect, WindowLayer.GameUI, () =>
            {
                Rect rect = overRect.AtZero().ContractedBy(6f);
                Rect rect2 = rect;
                rect2.height = overRect.height / 2f;
                Text.Font = GameFont.Tiny;
                string ammoLabel = ammoComp.Props.ammoDef?.LabelPluralCap ?? "Ammo".Translate();
                Widgets.Label(rect2, ammoLabel);
                Rect rect3 = rect;
                rect3.yMin = overRect.height / 2f;
                float fillPercent = (float)ammoComp.AmmoCount / ammoComp.Props.magazineCapacity;
                Widgets.FillableBar(rect3, fillPercent, FullBarTex, EmptyBarTex, false);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect3, ammoComp.AmmoCount.ToString() + " / " + ammoComp.Props.magazineCapacity.ToString());
                Text.Anchor = TextAnchor.UpperLeft;
            });
            return new GizmoResult(GizmoState.Clear);
        }

    }
}
