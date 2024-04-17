using UnityEngine;

namespace Weapon.Interface
{
    public interface IExplosion
    {
        public void Explosion();

        public void HitScanExplosion(LayerMask newTarget);
    }
}