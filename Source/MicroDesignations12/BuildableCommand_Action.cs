using System;
using Verse;
using UnityEngine;

namespace MicroDesignations
{
    public class BuildableCommand_Action: Command_Action
    {
        public BuildableDef buildableDef;
        public ThingDef thingDef;

        protected override void DrawIcon(Rect rect, Material buttonMat = null)
        {
            Widgets.DefIcon(rect, buildableDef, thingDef, 0.85f, false);
        }
    }
}