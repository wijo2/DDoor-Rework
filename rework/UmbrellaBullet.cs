using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace rework
{
    class UmbrellaBullet : MonoBehaviour
    {
        public Vector3 speed = new Vector3();
        public Vector3 rotationVector = new Vector3();
        public float sizeMultiplier = 1f;

        public void Awake()
        {
            gameObject.layer = 0;
            gameObject.transform.Find("Armature").position += new Vector3(0, -1f, 0);

            var s = gameObject.AddComponent<SphereCollider>();
            s.radius = 2.2f * sizeMultiplier;
            s.isTrigger = true;
            var r = gameObject.AddComponent<Rigidbody>();
            r.useGravity = false;
            //r.isKinematic = true;
        }

        public void FixedUpdate()
        {
            transform.position += speed * Time.fixedDeltaTime;

            if (transform.localEulerAngles.x > 270) { rotationVector.x = 0; }
            transform.localRotation = Quaternion.Euler( ModuloVector(transform.localEulerAngles + rotationVector * Time.fixedDeltaTime * 3) );
        }

        public void Shoot(Vector3 direction, float speed, float size)
        {
            this.speed = new Vector3(direction.x, 0, direction.z).normalized * speed;
            rotationVector = new Vector3(UnityEngine.Random.Range(-180, 180), UnityEngine.Random.Range(-180, 180), UnityEngine.Random.Range(-180, 180));
            sizeMultiplier = size;
        }

        public void OnTriggerEnter(Collider collider)
        {
            var bulletComp = collider.gameObject.GetComponent<HitBackProjectile>();
            if (bulletComp != null)
            {
                var d = bulletComp.gameObject.GetComponent<Damageable>();
                if (d != null)
                {
                    d.ReceiveDamage(0.5f, 0, transform.position, collider.ClosestPoint(transform.position));
                    //collider.transform.localPosition += collision.contacts[0].point - transform.position * 5;
                }
            }
            else if(collider.gameObject.GetComponent<UmbrellaBullet>() == null && !collider.isTrigger)
            {
                UnityEngine.Object.Destroy(gameObject);
            }
        }

        public Vector3 ModuloVector(Vector3 v) => new Vector3(ModuloAngle(v.x), ModuloAngle(v.y), ModuloAngle(v.z));
        public float ModuloAngle(float a) => a >= 0 ? (a <= 360 ? a : a - 360) : a + 360;
    }
}
