using UnityEngine;

public static class ResponseCurves
{
    public static float Linear(float x)
    {
        return Mathf.Clamp01(x);
    }

    public static float Quadratic(float x)
    {
        x = Mathf.Clamp01(x);
        return x * x;
    }

    public static float Inverse(float x)
    {
        return Mathf.Clamp01(1f - x);
    }
}