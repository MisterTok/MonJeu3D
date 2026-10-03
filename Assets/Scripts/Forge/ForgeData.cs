// Généré à partir de Forge_Master.xlsx (onglet Forge) — ne pas modifier à la main.
// Index 0 = niveau 1 de la forge. Les probabilités sont en % par cercle (10 cercles).
public static class ForgeData
{
    public const int MaxLevel = 35;
    // Coût en or d'UN nœud pour passer au niveau suivant (index = niveau visé - 1).
    public static readonly long[] NodeCost = { 0, 400, 700, 1500, 3500, 10000, 25000, 50000, 33000, 50000, 83300, 116000, 112000, 150000, 160000, 182000, 170000, 161429, 155000, 150000, 146000, 157000, 168000, 179000, 190000, 201000, 212000, 223000, 234000, 245000, 256000, 267000, 278000, 289000, 300000 };
    public static readonly int[] Nodes = { 0, 1, 1, 1, 1, 1, 1, 1, 3, 3, 3, 3, 4, 4, 5, 5, 6, 7, 8, 9, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10 };
    // Durée de l'amélioration en secondes.
    public static readonly long[] UpgradeSeconds = { 0, 300, 900, 1800, 3600, 7200, 27200, 47200, 67200, 87200, 107200, 127200, 147200, 167200, 187200, 207200, 227200, 247200, 277200, 307200, 337200, 367200, 397200, 427200, 457200, 487200, 517200, 547200, 577200, 607200, 637200, 667200, 697200, 727200, 757200 };
    // Coût en gemmes pour terminer l'amélioration immédiatement.
    public static readonly int[] GemCost = { 0, 1, 2, 4, 8, 17, 63, 109, 155, 201, 247, 293, 339, 385, 431, 477, 523, 569, 638, 707, 776, 845, 914, 983, 1050, 1120, 1190, 1250, 1320, 1390, 1460, 1530, 1600, 1670, 1740 };
    public static readonly float[][] CircleOdds = new float[][]
    {
        new float[] { 100.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f },
        new float[] { 99.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f },
        new float[] { 98.0f, 2.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f },
        new float[] { 96.0f, 4.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f },
        new float[] { 91.5f, 8.0f, 0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f },
        new float[] { 82.0f, 16.0f, 2.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f },
        new float[] { 64.0f, 32.0f, 4.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f },
        new float[] { 27.8f, 64.0f, 8.0f, 0.2f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f },
        new float[] { 13.0f, 70.0f, 16.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f },
        new float[] { 6.0f, 60.0f, 32.0f, 2.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f },
        new float[] { 0.0f, 31.9f, 64.0f, 4.0f, 0.1f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f },
        new float[] { 0.0f, 27.5f, 64.0f, 8.0f, 0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f },
        new float[] { 0.0f, 8.0f, 75.0f, 16.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f },
        new float[] { 0.0f, 0.0f, 66.0f, 32.0f, 2.0f, 0.05f, 0.0f, 0.0f, 0.0f, 0.0f },
        new float[] { 0.0f, 0.0f, 31.7f, 64.0f, 4.0f, 0.25f, 0.0f, 0.0f, 0.0f, 0.0f },
        new float[] { 0.0f, 0.0f, 21.5f, 70.0f, 8.0f, 0.5f, 0.0f, 0.0f, 0.0f, 0.0f },
        new float[] { 0.0f, 0.0f, 0.0f, 82.9f, 16.0f, 1.0f, 0.05f, 0.0f, 0.0f, 0.0f },
        new float[] { 0.0f, 0.0f, 0.0f, 65.7f, 32.0f, 2.0f, 0.25f, 0.0f, 0.0f, 0.0f },
        new float[] { 0.0f, 0.0f, 0.0f, 31.5f, 64.0f, 4.0f, 0.5f, 0.0f, 0.0f, 0.0f },
        new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 91.0f, 8.0f, 1.0f, 0.05f, 0.0f, 0.0f },
        new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 81.7f, 16.0f, 2.0f, 0.25f, 0.0f, 0.0f },
        new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 63.5f, 32.0f, 4.0f, 0.5f, 0.0f, 0.0f },
        new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 27.0f, 64.0f, 8.0f, 1.0f, 0.0f, 0.0f },
        new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 82.0f, 16.0f, 2.0f, 0.02f, 0.0f },
        new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 64.0f, 32.0f, 4.0f, 0.05f, 0.0f },
        new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 43.8f, 50.0f, 6.0f, 0.25f, 0.0f },
        new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 31.5f, 60.0f, 8.0f, 0.5f, 0.0f },
        new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 21.0f, 65.0f, 13.0f, 1.0f, 0.0f },
        new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 6.99f, 68.0f, 23.0f, 2.0f, 0.02f },
        new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 60.0f, 36.0f, 4.0f, 0.05f },
        new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 50.8f, 43.0f, 6.0f, 0.25f },
        new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 41.5f, 50.0f, 8.0f, 0.5f },
        new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 28.0f, 58.0f, 13.0f, 1.0f },
        new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 11.0f, 64.0f, 23.0f, 2.0f },
        new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 60.0f, 36.0f, 4.0f },
    };
}
