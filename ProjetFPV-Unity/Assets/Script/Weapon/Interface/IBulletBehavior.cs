using UnityEngine;

namespace Weapon.Interface
{
    public interface IBulletBehavior
    {
        public bool EnableMovement(bool logic);
        public float AddVelocity(float velocity);
        public float AddDamage(float damage);

        public Vector3 GetTheBulletDir(Vector3 dir);
    }
}