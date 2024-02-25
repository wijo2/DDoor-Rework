using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;

namespace rework.Categories
{
    internal class SpellMods
    {
        //arrows take 2 magic
        [HarmonyPatch(typeof(_ArrowPower), "Update")]
        [HarmonyPrefix]
        public static bool Update_pre(_ArrowPower __instance)
        {
            if (__instance.arrowPrefab.name == "ARROW")
            {
                __instance.requiredCharge = 2;
            }
            return true;
        }

        //flame charges slowly
        [HarmonyPatch(typeof(ArrowFireBlast), "Start")]
        [HarmonyPrefix]
        public static void Start_pre(ArrowFireBlast __instance)
        {
            __instance.timeForFullCharge = 1;
        }
    }
}
