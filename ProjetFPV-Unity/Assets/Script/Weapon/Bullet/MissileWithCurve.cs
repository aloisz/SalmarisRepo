using Player;
using UnityEngine;

namespace Weapon.Bullet
{
    public class MissileWithCurve : Missile
    {
        private Vector3 dirToFollow;
        
        protected override void CollideWithWalkableMask(Collision collision)
        {
            /*RaycastHit hit;
            if (Physics.Raycast(transform.position, -transform.up, out hit, 1000))
            {
                Debug.DrawRay();
            }*/
        }

        private void TrackPlayer()
        {
            rb.drag = drag;
            rb.velocity = bullet.bulletDir;
            rb.isKinematic = true;
            //rb.isKinematic = false;
        }
    }
}