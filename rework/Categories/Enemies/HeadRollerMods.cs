using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace rework.Categories.Enemies
{
    internal class HeadRollerMods
    {
        //bullets when starts rolling
        [HarmonyPatch(typeof(AI_HeadRoller), nameof(AI_HeadRoller.SetState), typeof(AI_Brain.AIState))]
        [HarmonyPrefix]
        public static bool SetState_pre(AI_HeadRoller __instance, AI_Brain.AIState newState)
        {
            if (newState != AI_Brain.AIState.Dash) { return true; }

            if (Rework.LoadThings("mb")[0])
            {
                var dir = __instance.gameObject.transform.forward;
                dir.y = 0.3f;
                GameObject bullet;
                var amount = UnityEngine.Random.Range(10, 15);
                //var amount = 12;
                for (int a = 0; a <= 360; a += 360 / amount)
                {
                    bullet = UnityEngine.Object.Instantiate(Rework.mageBulletPrefab, __instance.gameObject.transform.position + dir * 2, Quaternion.identity);
                    bullet.GetComponent<Bullet>().Shoot(Quaternion.Euler(0, a, 0) * __instance.gameObject.transform.forward, 4);
                }
            }
            return true;
        }
    }
}
