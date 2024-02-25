using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace rework.Categories.Enemies
{
    internal class SpinnyPotMods
    {
        //faster spinning and hopping
        [HarmonyPatch(typeof(PotMimicMelee), "Start")]
        [HarmonyPrefix]
        public static bool Start_pre(PotMimicMelee __instance)
        {
            __instance.spinSpeed = 10;
            __instance.hopSpeed = 15;
            return true;
        }
    }
}
