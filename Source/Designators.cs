using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RoofSupportTool
{
    [StaticConstructorOnStartup]
    public class RoofSupportToolDesignatorAdd : Designator
    {
        private static List<IntVec3> ringDrawCells = new List<IntVec3>();
        private static bool[] rotNeeded = new bool[4];

        static RoofSupportToolDesignatorAdd()
        {
            var resolvedDesignatorGetter = Utils.GetFieldAccessor<DesignationCategoryDef, List<Designator>>("resolvedDesignators");
            var orders = DefDatabase<DesignationCategoryDef>.AllDefs.FirstOrDefault(def => def.defName == "Orders");
            resolvedDesignatorGetter(orders).Add(new RoofSupportToolDesignatorAdd());
        }

        private readonly DesignationDef desDef = DesignationDefOf.Plan;

        public RoofSupportToolDesignatorAdd()
        {
            this.soundDragSustain = SoundDefOf.DesignateDragStandard;
            this.soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
            this.useMouseIcon = true;
            this.desDef = DesignationDefOf.Plan;
            this.defaultLabel = "Roof Support Tool";
            this.defaultDesc = "Displays roof support range and can place plan to show radius.";
            this.icon = ContentFinder<Texture2D>.Get("RoofSupportTool");
            this.soundSucceeded = SoundDefOf.DesignatePlanAdd;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(Map))
            {
                return false;
            }

            return true;
        }

        // Places the planning designators in radius around center.
        public override void DesignateSingleCell(IntVec3 c)
        {
            foreach (var cell in GenRadial.RadialCellsAround(c, 6.71f, true))
            {
                // Skip cells out of bounds.
                if (!cell.InBounds(Map) || cell.InNoBuildEdgeArea(Map))
                    continue;

                // Clear center cell to help see where to place wall.
                if (cell == c)
                {
                    foreach (Designation current in Map.designationManager.AllDesignationsAt(c).ToList<Designation>())
                    {
                        if (current.def == this.desDef)
                        {
                            base.Map.designationManager.RemoveDesignation(current);
                        }
                    }
                }

                // Mark radius, exluding walls. 
                else if (Map.designationManager.DesignationAt(cell, this.desDef) == null)
                {
                    bool holdsRoof = false;
                    List<Thing> thingList = GridsUtility.GetThingList(cell, Map);
                    for (int i = 0; i < thingList.Count; i++)
                    {
                        Thing thing = thingList[i];
                        if (thing.def.holdsRoof)
                        {
                            holdsRoof = true;
                            break;
                        }
                    }
                    if (!holdsRoof)
                        Map.designationManager.AddDesignation(new Designation(new LocalTargetInfo(cell), this.desDef));
                }
            }
        }

        // Draws the ring.
        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
            GenDraw.DrawNoBuildEdgeLines();
            if (!ArchitectCategoryTab.InfoRect.Contains(UI.MousePositionOnUIInverted))
            {
                GenDraw.DrawRadiusRing(UI.MouseCell(), 6.71f);
            }
        }
    }
}
