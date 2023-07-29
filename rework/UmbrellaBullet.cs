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

        public void Awake()
        {
            var s = gameObject.AddComponent<SphereCollider>();
            s.center = transform.position;
            s.radius = 2;
        }

        public void FixedUpdate()
        {
            transform.position += speed * Time.fixedDeltaTime;
            transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles + rotationVector * Time.fixedDeltaTime);
        }

        public void Shoot(Vector3 direction, float speed)
        {
            this.speed = new Vector3(direction.x, 0, direction.z).normalized * speed;
            rotationVector = new Vector3(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360));
        }

        public void OnCollisionEnter(Collision collision)
        {
            var bulletComp = collision.gameObject.GetComponent<Bullet>();
            if (bulletComp != null)
            {
                bulletComp.Reflect(collision.GetContact(0).normal);
            }
        }
    }
}
