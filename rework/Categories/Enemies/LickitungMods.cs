using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;

namespace rework.Categories.Enemies
{
    internal class LickitungMods
    {
        //spawn mage bullets
        [HarmonyPatch(typeof(AI_GrimaceKnight), nameof(AI_GrimaceKnight.SetState), typeof(AI_Brain.AIState))]
        [HarmonyPostfix]
        public static void SetState_post(AI_GrimaceKnight __instance, AI_Brain.AIState newState)
        {
            if (newState == AI_Brain.AIState.SwordSlam)
            {
                if (Rework.LoadThings("mb")[0])
                {
                    var dir = __instance.gameObject.transform.forward;
                    dir.y = 0.3f;
                    GameObject bullet;
                    //var amount = UnityEngine.Random.Range(6, 12);
                    var amount = 12;
                    for (int a = 0; a <= 360; a += 360 / amount)
                    {
                        bullet = UnityEngine.Object.Instantiate(Rework.mageBulletPrefab, __instance.gameObject.transform.position + dir * 2, Quaternion.identity);
                        bullet.GetComponent<Bullet>().Shoot(Quaternion.Euler(0, a, 0) * __instance.gameObject.transform.forward, 4);
                    }
                }
            }
        }
    }
}
