using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace rework.Categories.Enemies
{
    internal class ArcherMods
    {
        //splitting arrows
        [HarmonyPatch(typeof(ArrowBullet), "hitWall", typeof(Collision))]
        [HarmonyPostfix]
        public static void hitWall(ArrowBullet __instance)
        {
            if (Rework.LoadThings("ga")[0] && __instance.pulseScale.x == 8.0f && __instance.gameObject.layer == 12)
            {
                var dir = (PlayerGlobal.instance.gameObject.transform.position - __instance.gameObject.transform.position);
                dir.y = 0;
                dir.Normalize();
                var eul = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z)).eulerAngles.y;
                GameObject bullet;
                var amount = UnityEngine.Random.Range(3, 5);
                var angle = 40;
                //var amount = 3; //4 but math
                for (int a = angle / -2; a <= angle / 2; a += angle / amount)
                {
                    bullet = UnityEngine.Object.Instantiate(Rework.arrowPrefab, __instance.gameObject.transform.position + dir / 2, Quaternion.identity);
                    var rigid = bullet.GetComponent<Rigidbody>();
                    rigid.constraints = RigidbodyConstraints.FreezeRotation;
                    rigid.isKinematic = false;

                    var compo = bullet.GetComponent<ArrowBullet>();
                    compo.pulseScale.x = 8.01f;
                    compo.Shoot(Quaternion.Euler(0, a, 0) * dir, 1f);
                }
                __instance.pulseScale.x = 8.01f;
            }
        }

        //remove stagger
        [HarmonyPatch(typeof(AI_Ghoul), nameof(AI_Ghoul.Stagger))]
        [HarmonyPrefix]
        public static bool Stagger_pre()
        {
            return false;
        }
    }
}
