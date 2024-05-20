using DG.Tweening;
using Player;
using UnityEngine;

namespace Weapon.Bullet
{
    public class MissileWithCurve : Missile
    {
        private Vector3 dirToFollow;
        [SerializeField] private AnimationCurve walkableCurve;
        
        protected override void CollideWithWalkableMask(Collision collision)
        {
            /*RaycastHit hit;
            if (Physics.Raycast(transform.position, -transform.up, out hit, 1000))
            {
                Debug.DrawRay(transform.position, hit.point, Color.red, 2);
            }*/
            bullet.gravityApplied = 0;
            transform.DOMoveY(transform.position.y + 1, 2).SetEase(walkableCurve);
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