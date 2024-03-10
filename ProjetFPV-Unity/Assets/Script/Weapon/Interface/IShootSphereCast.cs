namespace Weapon.Interface
{
    public interface IShootSphereCast : IRaycast

    {
    /// <summary>
    /// Will shoot one ray to the Camera forward direction within a radius
    /// </summary>
    /// <param name="maxDistance"></param>
    public void SphereCastSingleHitScan(float maxDistance, float radius);


    /// <summary>
    /// Will Shoot multiple Raycast according to dispersion parameters within a radius
    /// </summary>
    /// <param name="maxDistance"></param>
    public void SphereCastDispersionHitScan(float maxDistance, float radius);
    }
}