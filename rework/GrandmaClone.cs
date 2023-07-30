using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;

namespace rework
{
    public class GrandmaClone : MonoBehaviour
    {
        public static int howManyCurrently = 0;
        public static int rage = 0;
        public static int grandmasTotal = 1; //how many have existed, so basically spawning phase
        public static AI_Brain.AIState[] MoveStates = new AI_Brain.AIState[] { 
            AI_Brain.AIState.Laser,
            AI_Brain.AIState.TeleportIn,
            AI_Brain.AIState.Teleport,
            AI_Brain.AIState.Patrol
        };

        public int howManyeth;
        public GrandmaBoss ai;
        public float hp; //needed because something resets to max hp
        public bool sethp = false;
        public bool shouldBeActive = false;
        

        public void OnEnable()
        {
            howManyCurrently++;
        }

        public void OnDisable()
        {
            howManyCurrently--;
        }


        public void Start()
        {
            howManyeth = howManyCurrently;
            ai = GetComponent<GrandmaBoss>();
            Helper.CopyPrivateValue<GrandmaBoss>(GrandmaBoss.instance, ai, "throwCounter");
            gameObject.SetActive(false);
        }

        public void Update()
        {
            if (!sethp) { GetComponent<DamageableBoss>().ForceHealth(hp); sethp = true; gameObject.GetComponent<DamageableBoss>().ReceiveDamage(0.01f, 0, Vector3.zero, Vector3.zero, hitForce: 0); }

            var state = ai.GetState();
            if (MoveStates.Contains(state))
            {
                if (howManyCurrently == 1)
                {
                    transform.localPosition = new Vector3(-1 * GrandmaBoss.instance.transform.localPosition.x, GrandmaBoss.instance.transform.localPosition.y, -1 * GrandmaBoss.instance.transform.localPosition.z);
                }
                if (howManyCurrently == 2)
                {
                    var s = Mathf.Sqrt(3) / 2;
                    if (howManyeth == 2)
                    {
                        s = -1 * s;
                    }
                    var x = GrandmaBoss.instance.transform.localPosition.x;
                    var z = GrandmaBoss.instance.transform.localPosition.z;
                    transform.localPosition = new Vector3(-0.5f * x - s * z, GrandmaBoss.instance.transform.localPosition.y, s * x - 0.5f * z);
                }
            }
        }

        public static void CalcHpAndEnable()
        {
            //Rework.L("hp check");
            var hp = GrandmaBoss.instance.GetComponent<DamageableBoss>().GetCurrentHealth();
            foreach (var gran in Rework.grandmaClones)
            {
                if (gran.GetComponent<GrandmaClone>().shouldBeActive)
                {
                    hp += gran.GetComponent<DamageableBoss>().GetCurrentHealth();
                }
            }
            if (grandmasTotal == 1 && hp < 90) { Rework.grandmaClones[0].GetComponent<GrandmaClone>().shouldBeActive = true; grandmasTotal++; }
            else if (grandmasTotal == 2 && hp < 110) { Rework.grandmaClones.Last().GetComponent<GrandmaClone>().shouldBeActive = true; grandmasTotal++; }
        }

        public static void Reset()
        {
            grandmasTotal = 1;
            howManyCurrently = 0;
            rage = 0;
            Rework.grandmaClones.Clear();
        }
    }
}
