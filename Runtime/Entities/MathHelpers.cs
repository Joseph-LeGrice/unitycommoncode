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
    
    public static quaternion GetFromToRotation(float3 from, float3 to)
    {
        from = math.normalize(from);
        to = math.normalize(to);
        
        float angle = math.acos(math.clamp(math.dot(from, to), -1f, 1f));
        float3 axis = math.cross(from, to);
        return quaternion.AxisAngle(axis, angle);
    }
    
    public static quaternion RotateTowards(float3 from, float3 to, float maxDegreesDelta)
    {
        from = math.normalize(from);
        to = math.normalize(to);
        
        float angle = math.acos(math.clamp(math.dot(from, to), -1f, 1f));
        angle = math.degrees(angle);
        angle = math.sign(angle) * math.min(math.abs(angle), maxDegreesDelta);
        angle = math.radians(angle);
        
        float3 axis = math.cross(from, to);
        return quaternion.AxisAngle(axis, angle);
    }
}
