using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace rework.Categories.Enemies
{
    internal class MageMods
    {
        //fast bullets
        [HarmonyPatch(typeof(AI_MageBrain), "shootBullet")]
        [HarmonyPrefix]
        public static bool shootBullet_pre(AI_MageBrain __instance)
        {
            Vector3 vector = PlayerGlobal.instance.transform.position - __instance.shootBone.position;
            float num = -__instance.spreadAngle * 0.5f;
            float num2 = __instance.spreadAngle / __instance.spreadMaxBullets * 2f;
            int num3 = 0;
            while ((float)num3 < __instance.spreadMaxBullets)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(__instance.bulletPrefab, __instance.shootBone.position, Quaternion.identity);
                SoundEngine.Event("MageAttack", gameObject);
                Bullet component = gameObject.GetComponent<Bullet>();
                if (component)
                {
                    component.Shoot(Quaternion.Euler(0f, num, 0f) * vector.normalized, 8f);
                }
                num += num2;
                num3++;
            }
            return false;
        }

        //range and tele timer modified
        [HarmonyPatch(typeof(AI_MageBrain), "Start")]
        [HarmonyPrefix]
        public static bool Start_pre(AI_MageBrain __instance)
        {
            __instance.attackRange = 10;
            __instance.timeBetweenTeleports = 0.1f;
            return true;
        }

        //stagger time set
        [HarmonyPatch(typeof(AI_MageBrain), "SetState")]
        [HarmonyPostfix]
        public static void SetState_post(AI_Brain.AIState newState, ref float ___timer)
        {
            if (newState == AI_Brain.AIState.Feared)
            {
                ___timer = 1;
            }
        }
    }
}
