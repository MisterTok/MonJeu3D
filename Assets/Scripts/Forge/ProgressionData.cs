// Données de progression issues des tableaux de la communauté Forge Master (Discord, déc. 2025),
// adaptées au thème « forge de l'enfer ». Les statistiques sont les valeurs de base.
public static class ProgressionData
{
    public static readonly string[] Rarities = { "Commune", "Rare", "Épique", "Légendaire", "Ultime", "Mythique" };
    public static readonly UnityEngine.Color[] RarityColors =
    {
        new UnityEngine.Color(0.85f, 0.85f, 0.85f), new UnityEngine.Color(0.35f, 0.65f, 1f), new UnityEngine.Color(0.4f, 0.9f, 0.4f),
        new UnityEngine.Color(1f, 0.85f, 0.25f), new UnityEngine.Color(1f, 0.35f, 0.3f), new UnityEngine.Color(0.8f, 0.45f, 1f),
    };

    // ---------- Arbre technologique : 5 paliers × 5 niveaux (durée en secondes, coût en potions rouges) ----------
    public static readonly long[][] TechSeconds =
    {
        new long[] { 300, 600, 1200, 2400, 4800 },                       // Palier I   : 5m → 1h20
        new long[] { 9600, 19200, 38400, 76800, 85980 },                 // Palier II  : 2h40 → 23h53
        new long[] { 93600, 104400, 118800, 133200, 151200 },            // Palier III : 1j2h → 1j18h
        new long[] { 169200, 187200, 212400, 237600, 266400 },           // Palier IV  : 1j23h → 3j2h
        new long[] { 298800, 334800, 374400, 417600, 468000 },           // Palier V   : 3j11h → 5j10h
    };
    public static readonly long[][] TechPotions =
    {
        new long[] { 40, 56, 78, 110, 154 },
        new long[] { 215, 301, 422, 590, 826 },
        new long[] { 1150, 1310, 1500, 1710, 1950 },
        new long[] { 2220, 2540, 2890, 3300, 3760 },
        new long[] { 4280, 4880, 5570, 6350, 7240 },
    };

    // ---------- Compagnons ----------
    // Type : 0 = équilibré, 1 = dégâts (x1,5 dégâts, x0,5 vie), 2 = vie (x0,5 dégâts, x1,5 vie).
    public class PetDef
    {
        public string name, model; public int rarity, type; public float size, fly;
        public PetDef(string n, int r, int t, string m, float s, float f = 0f) { name = n; rarity = r; type = t; model = m; size = s; fly = f; }
    }
    public static readonly float[] PetTypeDamage = { 1f, 1.5f, 0.5f };
    public static readonly float[] PetTypeHealth = { 1f, 0.5f, 1.5f };
    // Valeurs de base (niveau 1) par rareté, d'après le tableau de décembre 2025 ; +1 % par niveau.
    public static readonly double[] PetBaseDamage = { 100, 500, 2500, 12500, 62500, 312500 };
    public static readonly double[] PetBaseHealth = { 800, 4000, 20000, 100000, 500000, 2500000 };
    // Invocation d'œufs avec des coquilles : 100 coquilles par œuf. Le niveau d'invocation monte avec le nombre
    // d'invocations et améliore les chances de rareté (table de référence, en %).
    public const int EggSummonCost = 100;
    public static readonly int[] EggSummonRequired = {2,2,2,3,3,4,4,5,5,6,6,7,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23};
    public static readonly float[][] EggSummonOdds =
    {
        new float[]{100.000f,0.000f,0.000f,0.000f,0.000f,0.000f},
        new float[]{99.500f,0.500f,0.000f,0.000f,0.000f,0.000f},
        new float[]{99.000f,1.000f,0.000f,0.000f,0.000f,0.000f},
        new float[]{98.000f,2.000f,0.000f,0.000f,0.000f,0.000f},
        new float[]{95.000f,5.000f,0.000f,0.000f,0.000f,0.000f},
        new float[]{90.000f,10.000f,0.000f,0.000f,0.000f,0.000f},
        new float[]{79.664f,20.000f,0.336f,0.000f,0.000f,0.000f},
        new float[]{77.640f,21.800f,0.560f,0.000f,0.000f,0.000f},
        new float[]{75.305f,23.762f,0.933f,0.000f,0.000f,0.000f},
        new float[]{72.544f,25.901f,1.555f,0.000f,0.000f,0.000f},
        new float[]{69.176f,28.232f,2.592f,0.000f,0.000f,0.000f},
        new float[]{64.908f,30.772f,4.320f,0.000f,0.000f,0.000f},
        new float[]{59.258f,33.542f,7.200f,0.000f,0.000f,0.000f},
        new float[]{51.439f,36.561f,12.000f,0.000f,0.000f,0.000f},
        new float[]{40.149f,39.851f,20.000f,0.000f,0.000f,0.000f},
        new float[]{35.862f,43.438f,20.700f,0.000f,0.000f,0.000f},
        new float[]{31.228f,47.347f,21.425f,0.000f,0.000f,0.000f},
        new float[]{26.217f,51.609f,22.174f,0.000f,0.000f,0.000f},
        new float[]{20.796f,56.253f,22.950f,0.000f,0.000f,0.000f},
        new float[]{15.000f,61.246f,23.754f,0.000f,0.000f,0.000f},
        new float[]{15.000f,60.415f,24.585f,0.000f,0.000f,0.000f},
        new float[]{15.000f,59.554f,25.446f,0.000f,0.000f,0.000f},
        new float[]{15.000f,58.664f,26.336f,0.000f,0.000f,0.000f},
        new float[]{15.000f,57.742f,27.258f,0.000f,0.000f,0.000f},
        new float[]{15.000f,56.788f,28.212f,0.000f,0.000f,0.000f},
        new float[]{15.000f,55.801f,29.199f,0.000f,0.000f,0.000f},
        new float[]{15.000f,54.779f,30.221f,0.000f,0.000f,0.000f},
        new float[]{15.000f,53.648f,31.279f,0.073f,0.000f,0.000f},
        new float[]{15.000f,52.505f,32.374f,0.121f,0.000f,0.000f},
        new float[]{15.000f,51.291f,33.507f,0.202f,0.000f,0.000f},
        new float[]{15.000f,49.984f,34.680f,0.336f,0.000f,0.000f},
        new float[]{15.000f,48.547f,35.894f,0.560f,0.000f,0.000f},
        new float[]{15.000f,46.917f,37.150f,0.933f,0.000f,0.000f},
        new float[]{15.000f,44.995f,38.450f,1.555f,0.000f,0.000f},
        new float[]{15.000f,42.612f,39.796f,2.592f,0.000f,0.000f},
        new float[]{15.000f,39.491f,41.189f,4.320f,0.000f,0.000f},
        new float[]{15.000f,35.170f,42.630f,7.200f,0.000f,0.000f},
        new float[]{15.000f,28.878f,44.122f,12.000f,0.000f,0.000f},
        new float[]{15.000f,19.333f,45.667f,20.000f,0.000f,0.000f},
        new float[]{15.000f,15.000f,49.500f,20.500f,0.000f,0.000f},
        new float[]{15.000f,15.000f,48.988f,21.013f,0.000f,0.000f},
        new float[]{15.000f,15.000f,48.462f,21.538f,0.000f,0.000f},
        new float[]{15.000f,15.000f,47.924f,22.076f,0.000f,0.000f},
        new float[]{15.000f,15.000f,47.372f,22.628f,0.000f,0.000f},
        new float[]{15.000f,15.000f,46.806f,23.194f,0.000f,0.000f},
        new float[]{15.000f,15.000f,46.154f,23.774f,0.073f,0.000f},
        new float[]{15.000f,15.000f,45.511f,24.368f,0.121f,0.000f},
        new float[]{15.000f,15.000f,44.821f,24.977f,0.202f,0.000f},
        new float[]{15.000f,15.000f,44.062f,25.602f,0.336f,0.000f},
        new float[]{15.000f,15.000f,43.198f,26.242f,0.560f,0.000f},
        new float[]{15.000f,15.000f,42.169f,26.898f,0.933f,0.000f},
        new float[]{15.000f,15.000f,40.875f,27.570f,1.555f,0.000f},
        new float[]{15.000f,15.000f,39.149f,28.259f,2.592f,0.000f},
        new float[]{15.000f,15.000f,36.714f,28.966f,4.320f,0.000f},
        new float[]{15.000f,15.000f,33.110f,29.690f,7.200f,0.000f},
        new float[]{15.000f,15.000f,27.568f,30.432f,12.000f,0.000f},
        new float[]{15.000f,15.000f,18.807f,31.193f,20.000f,0.000f},
        new float[]{15.000f,15.000f,15.000f,34.840f,20.160f,0.000f},
        new float[]{15.000f,15.000f,15.000f,34.679f,20.321f,0.000f},
        new float[]{15.000f,15.000f,15.000f,34.516f,20.484f,0.000f},
        new float[]{15.000f,15.000f,15.000f,34.352f,20.648f,0.000f},
        new float[]{15.000f,15.000f,15.000f,34.187f,20.813f,0.000f},
        new float[]{15.000f,15.000f,15.000f,34.021f,20.979f,0.000f},
        new float[]{15.000f,15.000f,15.000f,33.853f,21.147f,0.000f},
        new float[]{15.000f,15.000f,15.000f,33.684f,21.316f,0.000f},
        new float[]{15.000f,15.000f,15.000f,33.513f,21.487f,0.000f},
        new float[]{15.000f,15.000f,15.000f,33.341f,21.659f,0.000f},
        new float[]{15.000f,15.000f,15.000f,33.168f,21.832f,0.000f},
        new float[]{15.000f,15.000f,15.000f,32.993f,22.007f,0.000f},
        new float[]{15.000f,15.000f,15.000f,32.817f,22.183f,0.000f},
        new float[]{15.000f,15.000f,15.000f,32.567f,22.360f,0.073f},
        new float[]{15.000f,15.000f,15.000f,32.340f,22.539f,0.121f},
        new float[]{15.000f,15.000f,15.000f,32.079f,22.719f,0.202f},
        new float[]{15.000f,15.000f,15.000f,31.763f,22.901f,0.336f},
        new float[]{15.000f,15.000f,15.000f,31.356f,23.084f,0.560f},
        new float[]{15.000f,15.000f,15.000f,30.798f,23.269f,0.933f},
        new float[]{15.000f,15.000f,15.000f,29.990f,23.455f,1.555f},
        new float[]{15.000f,15.000f,15.000f,28.765f,23.643f,2.592f},
        new float[]{15.000f,15.000f,15.000f,26.848f,23.832f,4.320f},
        new float[]{15.000f,15.000f,15.000f,23.777f,24.023f,7.200f},
        new float[]{15.000f,15.000f,15.000f,18.785f,24.215f,12.000f},
        new float[]{15.000f,15.000f,15.000f,15.000f,20.000f,20.000f},
        new float[]{15.000f,15.000f,15.000f,15.000f,19.740f,20.260f},
        new float[]{15.000f,15.000f,15.000f,15.000f,19.477f,20.523f},
        new float[]{15.000f,15.000f,15.000f,15.000f,19.210f,20.790f},
        new float[]{15.000f,15.000f,15.000f,15.000f,18.940f,21.060f},
        new float[]{15.000f,15.000f,15.000f,15.000f,18.666f,21.334f},
        new float[]{15.000f,15.000f,15.000f,15.000f,18.388f,21.612f},
        new float[]{15.000f,15.000f,15.000f,15.000f,18.107f,21.893f},
        new float[]{15.000f,15.000f,15.000f,15.000f,17.823f,22.177f},
        new float[]{15.000f,15.000f,15.000f,15.000f,17.535f,22.465f},
        new float[]{15.000f,15.000f,15.000f,15.000f,17.243f,22.757f},
        new float[]{15.000f,15.000f,15.000f,15.000f,16.947f,23.053f},
        new float[]{15.000f,15.000f,15.000f,15.000f,16.647f,23.353f},
        new float[]{15.000f,15.000f,15.000f,15.000f,16.343f,23.657f},
        new float[]{15.000f,15.000f,15.000f,15.000f,16.036f,23.964f},
        new float[]{15.000f,15.000f,15.000f,15.000f,15.724f,24.276f},
        new float[]{15.000f,15.000f,15.000f,15.000f,15.409f,24.591f},
        new float[]{15.000f,15.000f,15.000f,15.000f,15.089f,24.911f},
        new float[]{15.000f,15.000f,15.000f,15.000f,15.000f,25.000f}
    };
    public static readonly long[] EggHatchSeconds = { 1800, 7200, 14400, 28800, 57600, 115200 };
    // Nombre de couveuses : 2 au départ, la 3e coûte 200 gemmes, la 4e 400.
    public const int IncubatorStart = 2, IncubatorMax = 4;
    public static readonly int[] IncubatorCost = { 0, 0, 200, 400 };
    // Nombre de statistiques secondaires d'un compagnon selon sa rareté.
    public static readonly int[] PetSubCount = { 1, 1, 1, 2, 2, 2 };
    public static readonly PetDef[] Pets =
    {
        new PetDef("Rat des braises", 0, 0, "Characters/Rat", 0.4f),
        new PetDef("Crapaud de soufre", 0, 1, "Characters/Frog", 0.45f),
        new PetDef("Limace ardente", 0, 0, "Characters/Slime", 0.45f),
        new PetDef("Corbeau charognard", 0, 2, "Pets/bird", 0.45f, 1.1f),
        new PetDef("Chat noir", 0, 0, "Pets/Cat", 0.5f),
        new PetDef("Poussin de feu", 0, 2, "Pets/Chick", 0.4f),
        new PetDef("Serpent de cendre", 1, 2, "Characters/Snake", 0.6f),
        new PetDef("Chien des ombres", 1, 2, "Pets/Dog", 0.6f),
        new PetDef("Araignée des abysses", 1, 0, "Characters/Spider", 0.45f),
        new PetDef("Guêpe infernale", 1, 1, "Characters/Wasp", 0.5f, 1.2f),
        new PetDef("Chauve-souris vampire", 1, 1, "Characters/Bat", 0.5f, 1.3f),
        new PetDef("Tricératops de lave", 2, 2, "Pets/Triceratops", 0.8f),
        new PetDef("Raptor d'ombre", 2, 1, "Pets/Velociraptor", 0.8f),
        new PetDef("Renard des flammes", 2, 0, "Pets/RedFox", 0.55f),
        new PetDef("Aigle de braise", 2, 1, "Pets/Eagle", 0.6f, 1.4f),
        new PetDef("Squelette serviteur", 2, 2, "Characters/Skeleton", 1.0f),
        new PetDef("Cerbère", 3, 1, "Pets/Wolf", 0.8f),
        new PetDef("Golem de fer", 3, 0, "Pets/Robot", 1.1f),
        new PetDef("Tyran de cendre", 3, 1, "Pets/Trex", 1.1f),
        new PetDef("Dragon rouge", 4, 1, "Characters/Dragon", 0.9f, 1.2f),
        new PetDef("Stégosaure de magma", 4, 2, "Pets/Stegosaurus", 1.0f),
        new PetDef("Démon astral", 4, 0, "Pets/Alien", 1.0f),
        new PetDef("Titan d'os", 5, 0, "Pets/Apatosaurus", 1.3f),
        new PetDef("Raie spectrale", 5, 2, "Pets/Mantaray", 0.9f, 1.6f),
        new PetDef("Léviathan", 5, 1, "Pets/Whale", 1.2f, 2.0f),
    };

    // ---------- Montures ----------
    // Bonus de base en % de dégâts et de vie (tableau de décembre 2025), +1 % de ce bonus par niveau.
    public static readonly float[] MountBonusPct = { 10f, 40f, 80f, 150f, 250f, 400f };
    public const int MountSummonCost = 50;
    public static readonly int[] MountSummonRequired = {20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20};
    public static readonly float[][] MountSummonOdds =
    {
        new float[]{100.000f,0.000f,0.000f,0.000f,0.000f,0.000f},
        new float[]{99.990f,0.010f,0.000f,0.000f,0.000f,0.000f},
        new float[]{99.983f,0.017f,0.000f,0.000f,0.000f,0.000f},
        new float[]{99.971f,0.029f,0.000f,0.000f,0.000f,0.000f},
        new float[]{99.951f,0.049f,0.000f,0.000f,0.000f,0.000f},
        new float[]{99.916f,0.084f,0.000f,0.000f,0.000f,0.000f},
        new float[]{99.858f,0.142f,0.000f,0.000f,0.000f,0.000f},
        new float[]{99.759f,0.241f,0.000f,0.000f,0.000f,0.000f},
        new float[]{99.590f,0.410f,0.000f,0.000f,0.000f,0.000f},
        new float[]{99.302f,0.698f,0.000f,0.000f,0.000f,0.000f},
        new float[]{98.814f,1.186f,0.000f,0.000f,0.000f,0.000f},
        new float[]{97.984f,2.016f,0.000f,0.000f,0.000f,0.000f},
        new float[]{96.573f,3.427f,0.000f,0.000f,0.000f,0.000f},
        new float[]{94.174f,5.826f,0.000f,0.000f,0.000f,0.000f},
        new float[]{90.095f,9.905f,0.000f,0.000f,0.000f,0.000f},
        new float[]{83.162f,16.838f,0.000f,0.000f,0.000f,0.000f},
        new float[]{80.000f,20.000f,0.000f,0.000f,0.000f,0.000f},
        new float[]{75.000f,25.000f,0.000f,0.000f,0.000f,0.000f},
        new float[]{68.750f,31.250f,0.000f,0.000f,0.000f,0.000f},
        new float[]{60.938f,39.063f,0.000f,0.000f,0.000f,0.000f},
        new float[]{51.172f,48.828f,0.000f,0.000f,0.000f,0.000f},
        new float[]{38.892f,61.035f,0.073f,0.000f,0.000f,0.000f},
        new float[]{23.585f,76.294f,0.121f,0.000f,0.000f,0.000f},
        new float[]{15.000f,84.798f,0.202f,0.000f,0.000f,0.000f},
        new float[]{15.000f,84.664f,0.336f,0.000f,0.000f,0.000f},
        new float[]{15.000f,84.440f,0.560f,0.000f,0.000f,0.000f},
        new float[]{15.000f,84.067f,0.933f,0.000f,0.000f,0.000f},
        new float[]{15.000f,83.445f,1.555f,0.000f,0.000f,0.000f},
        new float[]{15.000f,82.408f,2.592f,0.000f,0.000f,0.000f},
        new float[]{15.000f,80.680f,4.320f,0.000f,0.000f,0.000f},
        new float[]{15.000f,77.800f,7.200f,0.000f,0.000f,0.000f},
        new float[]{15.000f,73.000f,12.000f,0.000f,0.000f,0.000f},
        new float[]{15.000f,65.000f,20.000f,0.000f,0.000f,0.000f},
        new float[]{15.000f,61.000f,24.000f,0.000f,0.000f,0.000f},
        new float[]{15.000f,56.200f,28.800f,0.000f,0.000f,0.000f},
        new float[]{15.000f,50.440f,34.560f,0.000f,0.000f,0.000f},
        new float[]{15.000f,43.528f,41.472f,0.000f,0.000f,0.000f},
        new float[]{15.000f,35.161f,49.766f,0.073f,0.000f,0.000f},
        new float[]{15.000f,25.159f,59.720f,0.121f,0.000f,0.000f},
        new float[]{15.000f,15.000f,69.798f,0.202f,0.000f,0.000f},
        new float[]{15.000f,15.000f,69.664f,0.336f,0.000f,0.000f},
        new float[]{15.000f,15.000f,69.440f,0.560f,0.000f,0.000f},
        new float[]{15.000f,15.000f,69.067f,0.933f,0.000f,0.000f},
        new float[]{15.000f,15.000f,68.445f,1.555f,0.000f,0.000f},
        new float[]{15.000f,15.000f,67.408f,2.592f,0.000f,0.000f},
        new float[]{15.000f,15.000f,65.680f,4.320f,0.000f,0.000f},
        new float[]{15.000f,15.000f,62.800f,7.200f,0.000f,0.000f},
        new float[]{15.000f,15.000f,58.000f,12.000f,0.000f,0.000f},
        new float[]{15.000f,15.000f,50.000f,20.000f,0.000f,0.000f},
        new float[]{15.000f,15.000f,48.500f,21.500f,0.000f,0.000f},
        new float[]{15.000f,15.000f,46.888f,23.113f,0.000f,0.000f},
        new float[]{15.000f,15.000f,45.154f,24.846f,0.000f,0.000f},
        new float[]{15.000f,15.000f,43.291f,26.709f,0.000f,0.000f},
        new float[]{15.000f,15.000f,41.287f,28.713f,0.000f,0.000f},
        new float[]{15.000f,15.000f,39.061f,30.866f,0.073f,0.000f},
        new float[]{15.000f,15.000f,36.698f,33.181f,0.121f,0.000f},
        new float[]{15.000f,15.000f,34.129f,35.670f,0.202f,0.000f},
        new float[]{15.000f,15.000f,31.319f,38.345f,0.336f,0.000f},
        new float[]{15.000f,15.000f,28.219f,41.221f,0.560f,0.000f},
        new float[]{15.000f,15.000f,24.755f,44.312f,0.933f,0.000f},
        new float[]{15.000f,15.000f,20.809f,47.636f,1.555f,0.000f},
        new float[]{15.000f,15.000f,15.000f,52.408f,2.592f,0.000f},
        new float[]{15.000f,15.000f,15.000f,50.680f,4.320f,0.000f},
        new float[]{15.000f,15.000f,15.000f,47.800f,7.200f,0.000f},
        new float[]{15.000f,15.000f,15.000f,43.000f,12.000f,0.000f},
        new float[]{15.000f,15.000f,15.000f,35.000f,20.000f,0.000f},
        new float[]{15.000f,15.000f,15.000f,34.260f,20.740f,0.000f},
        new float[]{15.000f,15.000f,15.000f,33.493f,21.507f,0.000f},
        new float[]{15.000f,15.000f,15.000f,32.697f,22.303f,0.000f},
        new float[]{15.000f,15.000f,15.000f,31.872f,23.128f,0.000f},
        new float[]{15.000f,15.000f,15.000f,30.943f,23.984f,0.073f},
        new float[]{15.000f,15.000f,15.000f,30.008f,24.872f,0.121f},
        new float[]{15.000f,15.000f,15.000f,29.007f,25.792f,0.202f},
        new float[]{15.000f,15.000f,15.000f,27.918f,26.746f,0.336f},
        new float[]{15.000f,15.000f,15.000f,26.704f,27.736f,0.560f},
        new float[]{15.000f,15.000f,15.000f,25.305f,28.762f,0.933f},
        new float[]{15.000f,15.000f,15.000f,23.619f,29.826f,1.555f},
        new float[]{15.000f,15.000f,15.000f,21.478f,30.930f,2.592f},
        new float[]{15.000f,15.000f,15.000f,18.606f,32.074f,4.320f},
        new float[]{15.000f,15.000f,15.000f,15.000f,32.800f,7.200f},
        new float[]{15.000f,15.000f,15.000f,15.000f,28.000f,12.000f},
        new float[]{15.000f,15.000f,15.000f,15.000f,20.000f,20.000f},
        new float[]{15.000f,15.000f,15.000f,15.000f,19.740f,20.260f},
        new float[]{15.000f,15.000f,15.000f,15.000f,19.477f,20.523f},
        new float[]{15.000f,15.000f,15.000f,15.000f,19.210f,20.790f},
        new float[]{15.000f,15.000f,15.000f,15.000f,18.940f,21.060f},
        new float[]{15.000f,15.000f,15.000f,15.000f,18.666f,21.334f},
        new float[]{15.000f,15.000f,15.000f,15.000f,18.388f,21.612f},
        new float[]{15.000f,15.000f,15.000f,15.000f,18.107f,21.893f},
        new float[]{15.000f,15.000f,15.000f,15.000f,17.823f,22.177f},
        new float[]{15.000f,15.000f,15.000f,15.000f,17.535f,22.465f},
        new float[]{15.000f,15.000f,15.000f,15.000f,17.243f,22.757f},
        new float[]{15.000f,15.000f,15.000f,15.000f,16.947f,23.053f},
        new float[]{15.000f,15.000f,15.000f,15.000f,16.647f,23.353f},
        new float[]{15.000f,15.000f,15.000f,15.000f,16.343f,23.657f},
        new float[]{15.000f,15.000f,15.000f,15.000f,16.036f,23.964f},
        new float[]{15.000f,15.000f,15.000f,15.000f,15.724f,24.276f},
        new float[]{15.000f,15.000f,15.000f,15.000f,15.409f,24.591f},
        new float[]{15.000f,15.000f,15.000f,15.000f,15.089f,24.911f},
        new float[]{15.000f,15.000f,15.000f,15.000f,15.000f,25.000f}
    };
    public class MountDef
    {
        public string name, model; public int rarity; public float size, ride, fly;
        public MountDef(string n, int r, string m, float s, float rd, float f = 0f) { name = n; rarity = r; model = m; size = s; ride = rd; fly = f; }
    }
    // size = hauteur du modèle ; ride = hauteur de selle (où se tient le héros).
    public static readonly MountDef[] Mounts =
    {
        new MountDef("Porc des fosses", 0, "Pets/Pig", 0.8f, 0.6f),
        new MountDef("Mouton noir", 0, "Pets/Sheep", 0.85f, 0.65f),
        new MountDef("Bouc des cendres", 0, "Pets/Llama", 1.2f, 0.75f),
        new MountDef("Cheval squelette", 1, "Pets/Horse", 1.5f, 0.95f),
        new MountDef("Zèbre infernal", 1, "Pets/Zebra", 1.5f, 0.95f),
        new MountDef("Raptor des marais", 1, "Pets/Velociraptor", 1.2f, 0.75f),
        new MountDef("Loup géant", 1, "Pets/Wolf", 1.1f, 0.75f),
        new MountDef("Tricératops de guerre", 2, "Pets/Triceratops", 1.3f, 1.0f),
        new MountDef("Cerbère de selle", 2, "Pets/Dog", 1.1f, 0.75f),
        new MountDef("Stégosaure de magma", 3, "Pets/Stegosaurus", 1.5f, 1.1f),
        new MountDef("Tyran de cendre", 3, "Pets/Trex", 1.9f, 1.25f),
        new MountDef("Dragon rouge", 4, "Characters/Dragon", 1.6f, 1.0f, 0.4f),
        new MountDef("Titan d'os", 4, "Pets/Apatosaurus", 2.2f, 1.5f),
        new MountDef("Raie spectrale", 5, "Pets/Mantaray", 1.0f, 0.5f, 0.6f),
        new MountDef("Léviathan", 5, "Pets/Whale", 1.3f, 0.9f, 0.7f),
    };

    // ---------- Statistiques secondaires des pièces : valeur maximale par pièce (en %) ----------
    public static readonly string[] SubstatNames =
    {
        "Chance de critique", "Dégâts critiques", "Vitesse d'attaque", "Double frappe", "Dégâts",
        "Dégâts des compétences", "Dégâts à distance", "Dégâts en mêlée", "Blocage", "Vol de vie",
        "Régénération", "Réduction de recharge", "Santé"
    };
    public static readonly float[] SubstatMaxPct = { 12f, 100f, 40f, 40f, 15f, 30f, 15f, 50f, 5f, 20f, 6f, 7f, 15f };
    // Au total, chance de critique et double frappe sont plafonnées à 100 %.
}
