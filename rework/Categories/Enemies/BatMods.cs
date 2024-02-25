using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace rework.Categories.Enemies
{
    internal class BatMods
    {
        //hp set and invisibility introduced
        [HarmonyPatch(typeof(AI_Bat), "FixedUpdate")]
        [HarmonyPostfix]
        public static void FixedUpdate_post(AI_Bat __instance)
        {
            var dmg = __instance.gameObject.GetComponentInChildren<Damageable>();
            if (dmg != null && dmg.GetCurrentHealthPercent() == 1)
            {
                dmg.maxHealth = 2;
                dmg.HealToFull();
            }
            if (__instance.GetState() == AI_Brain.AIState.Chase)
            {
                __instance.gameObject.GetComponentInChildren<Renderer>().enabled = false;
                return;
            }
            __instance.gameObject.GetComponentInChildren<Renderer>().enabled = true;
        }
    }
}
