using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace rework.Categories.Enemies
{
    internal class SmoughMods
    {
        //more turning and extra enemies
        [HarmonyPatch(typeof(AI_Knight), "FixedUpdate")]
        [HarmonyPostfix]
        public static void FixedUpdate_post(AI_Knight __instance, CharacterMoveRootAnim ___movement, Vector3 ___pDiff, Vector3 ___startTurnForward, bool ___turning180dir, bool ___turning180)
        {
            if (__instance.GetState() == AI_Brain.AIState.Attack)
            {
                Vector3 vector = PlayerGlobal.instance.transform.position - __instance.transform.position;
                var cancel = false;
                if (___turning180)
                {
                    float num = Vector3.Dot(___startTurnForward, ___pDiff.normalized);
                    Debug.Log("LHS: " + num);
                    if (num < 0f)
                    {
                        if (___turning180dir)
                        {
                            Debug.Log("LHS: CANCEL");
                            cancel = true;
                        }
                    }
                    else if (!___turning180dir)
                    {
                        Debug.Log("LHS: CANCEL");
                        cancel = true;
                    }
                }
                if (!cancel)
                {
                    float num2 = Mathf.Atan2(vector.normalized.x, vector.normalized.z) * 57.29578f;
                    __instance.transform.rotation = Quaternion.Euler(0f, Mathf.LerpAngle(__instance.transform.rotation.eulerAngles.y, num2, Time.fixedDeltaTime * 4), 0f);
                    ___movement.AbsorbCurrentAngle();

                }
            }

            //extra enemies
            if (__instance.gameObject.transform.localScale != Vector3.one * 0.91f && __instance.GetState() != AI_Brain.AIState.Idle)
            {
                __instance.gameObject.transform.localScale = Vector3.one * 0.91f;
                var gruntFab = Resources.FindObjectsOfTypeAll<AI_Grunt>()[0].gameObject;
                if (gruntFab != null)
                {
                    var newGrunt = UnityEngine.Object.Instantiate(gruntFab, __instance.gameObject.transform.position + new Vector3(-5, 0, 0), Quaternion.identity);
                    newGrunt.GetComponent<AI_Brain>().alwaysKnowWherePlayerIs = true;
                    newGrunt = UnityEngine.Object.Instantiate(gruntFab, __instance.gameObject.transform.position + new Vector3(5, 0, 0), Quaternion.identity);
                    newGrunt.GetComponent<AI_Brain>().alwaysKnowWherePlayerIs = true;
                    newGrunt = UnityEngine.Object.Instantiate(gruntFab, __instance.gameObject.transform.position + new Vector3(0, 0, 5), Quaternion.identity);
                    newGrunt.GetComponent<AI_Brain>().alwaysKnowWherePlayerIs = true;
                }
            }
        }

        //faster attacks
        [HarmonyPatch(typeof(AI_Knight), "Anim_StartAttackAim")]
        [HarmonyPrefix]
        public static bool Anim_StartAttackAim_pre(ref float value)
        {
            value *= 2;
            return true;
        }
    }
}
