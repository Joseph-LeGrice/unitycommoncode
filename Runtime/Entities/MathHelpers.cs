using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;


public static class MathHelpers
{
    public static float GetAngle(float3 from , float3 to)
    {
        from = math.normalize(from);
        to = math.normalize(to);
        
        return math.acos(math.clamp(math.dot(from, to), -1f, 1f));
    }
    
    public static quaternion GetFromToRotation(float3 from , float3 to)
    {
        from = math.normalize(from);
        to = math.normalize(to);
        
        float angle = math.acos(math.clamp(math.dot(from, to), -1f, 1f));
        float3 axis = math.cross(from, to);
        return quaternion.AxisAngle(axis, angle);
    }
}
