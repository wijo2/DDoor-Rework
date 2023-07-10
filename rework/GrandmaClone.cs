using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace rework
{
    public class GrandmaClone : MonoBehaviour
    {
        public static int howManyCurrently = 0;
        public static int rage = 0;
        public static int grandmasTotal = 1; //how many have existed, so basically spawning phase

        public int howManyeth;
        public GrandmaBoss ai;
        

        public void Start()
        {
            grandmasTotal++;
            howManyCurrently++;
            howManyeth = howManyCurrently;
            ai = GetComponent<GrandmaBoss>();
        }

        public void Update()
        {
            var state = ai.GetState();
            //if (gameObject.active == false && GrandmaBoss.instance.GetState() == AI_Brain.AIState.TeleportIn) { gameObject.SetActive(true); ai.SetState(AI_Brain.AIState.TeleportIn); }

            if (/*state != AI_Brain.AIState.Laser && */state != AI_Brain.AIState.PrepShoot)
            {
                var c = Rework.grandmaClones.Count();
                if (c == 1)
                {
                    transform.localPosition = new Vector3(-1 * GrandmaBoss.instance.transform.localPosition.x, GrandmaBoss.instance.transform.localPosition.y, -1 * GrandmaBoss.instance.transform.localPosition.z);
                }
                if (c == 2)
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

        public static void calcHpAndSpawn()
        {
            Rework.L("hp check");
            var hp = GrandmaBoss.instance.GetComponent<DamageableBoss>().GetCurrentHealth();
            foreach (var gran in Rework.grandmaClones)
            {
                hp += gran.GetComponent<DamageableBoss>().GetCurrentHealth();
            }
            if (grandmasTotal == 1 && hp < 100) { Rework.CreateGranClone(100, 150); }
            else if (grandmasTotal == 2 && hp < 100) { Rework.CreateGranClone(50, 150); }
        }

        public static void Reset()
        {
            howManyCurrently = 0;
            rage = 0;
            grandmasTotal = 1;
        }
    }
}
