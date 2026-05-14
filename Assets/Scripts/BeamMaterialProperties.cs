using UnityEngine;

public enum BeamMaterialType { Wood, Steel, Cable }

public static class BeamMaterialProperties
{
    private static readonly float[] MaxStress    = { 4000f,  22000f, 14000f };
    private static readonly float[] CrossSection = { 0.04f,  0.025f, 0.008f };
    private static readonly float[] Stiffness    = { 0.55f,  1.0f,   0.35f  };

    public static float GetMaxStress(BeamMaterialType t)    => MaxStress[(int)t];
    public static float GetCrossSection(BeamMaterialType t) => CrossSection[(int)t];
    public static float GetStiffness(BeamMaterialType t)    => Stiffness[(int)t];
    public static bool  IsCable(BeamMaterialType t)         => t == BeamMaterialType.Cable;
}