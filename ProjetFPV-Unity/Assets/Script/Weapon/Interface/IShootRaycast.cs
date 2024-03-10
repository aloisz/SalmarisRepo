namespace Weapon.Interface
{
    public interface IShootRaycast : IRaycast
    {
        /// <summary>
        /// Will shoot one ray to the Camera forward direction
        /// </summary>
        /// <param name="maxDistance"></param>
        public void RaycastSingleHitScan(float maxDistance);

        
        /// <summary>
        /// Will Shoot multiple Raycast according to dispersion parameters
        /// </summary>
        /// <param name="maxDistance"></param>
        public void RaycastDispersionHitScan(float maxDistance);
    }
}