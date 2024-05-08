using UnityEngine;

namespace Weapon.Interface
{
    public interface IBulletBehavior
    {
        public bool EnableMovement(bool logic);
        public float AddVelocity(float velocity);
        public float AddDamage(float damage);
        public string PoolingKeyName(string key);

        public Vector3 SetTheBulletDir(Vector3 dir);
    }
}