using Player;
using UnityEngine;

namespace Weapon.Bullet
{
    public class MissileWithCurve : Missile
    {
        private Vector3 dirToFollow;
        protected override void FixedUpdate()
        {
            transform.rotation = Quaternion.LookRotation(bullet.bulletDir, Vector3.forward);
            if (!EnableMovement(bullet.isMoving)) return;
            TrackPlayer();
            EnableMovement(false);
        }

        private void TrackPlayer()
        {
            rb.drag = drag;
            rb.velocity = bullet.bulletDir;
            //rb.isKinematic = false;
        }
    }
}