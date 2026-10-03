// Compétences : 18 sorts infernaux (valeurs du jeu de référence, SkillLibrary / SkillPassiveLibrary / SkillSummonConfig).
// Les dégâts et la vie sont multipliés par GameState.SkillScale pour rester cohérents avec nos objets.
public static class SkillData
{
    // Effet en combat.
    public const int Strike = 0;  // projectile sur l'ennemi de tête
    public const int Volley = 1;  // frappe tous les ennemis présents
    public const int Heal = 2;    // soigne le héros pendant la durée
    public const int Rage = 3;    // dégâts en plus pendant la durée
    public const int Aura = 4;    // dégâts et vie en plus pendant la durée
    public const int Drone = 5;   // un œil tire régulièrement pendant la durée

    public class SkillDef
    {
        public string name, desc; public int rarity, kind; public float cooldown, duration; public double damage, health;
        public UnityEngine.Color color;
        public SkillDef(string n, int r, int k, float cd, float dur, double dmg, double hp, UnityEngine.Color c, string d)
        { name = n; rarity = r; kind = k; cooldown = cd; duration = dur; damage = dmg; health = hp; color = c; desc = d; }
    }

    static UnityEngine.Color C(float r, float g, float b) => new UnityEngine.Color(r, g, b);

    // Icônes peintes (Craftpix, hors dépôt : Resources/ForgeSprites/SkillIcons/<id>.png). null si absentes -> symbole.
    static readonly UnityEngine.Texture2D[] icons = new UnityEngine.Texture2D[32];
    static readonly bool[] iconTried = new bool[32];
    public static UnityEngine.Texture2D Icon(int id)
    {
        if (id < 0 || id >= icons.Length) return null;
        if (!iconTried[id]) { iconTried[id] = true; icons[id] = UnityEngine.Resources.Load<UnityEngine.Texture2D>("ForgeSprites/SkillIcons/" + id); }
        return icons[id];
    }

    public static readonly SkillDef[] Skills =
    {
        new SkillDef("Chair grillée", 0, Heal, 8f, 10f, 0, 100, C(0.45f, 1f, 0.4f), "Soigne le héros pendant {d}s : {h} PV au total"),
        new SkillDef("Flèches d'os", 0, Strike, 7f, 0f, 200, 0, C(0.95f, 0.9f, 0.75f), "Tire une salve sur l'ennemi de tête : {a} dégâts"),
        new SkillDef("Hurlement", 0, Volley, 6f, 0f, 150, 0, C(1f, 0.75f, 0.4f), "Onde de choc sur tous les ennemis : {a} dégâts"),
        new SkillDef("Furie", 1, Rage, 8f, 10f, 1280, 0, C(1f, 0.25f, 0.15f), "+{a} d'attaque pendant {d}s"),
        new SkillDef("Étoiles de soufre", 1, Strike, 4f, 0f, 1280, 0, C(1f, 0.9f, 0.2f), "Lance des étoiles brûlantes : {a} dégâts"),
        new SkillDef("Barrage de boulets", 1, Volley, 5f, 0f, 1280, 0, C(1f, 0.5f, 0.15f), "Pilonne tous les ennemis : {a} dégâts"),
        new SkillDef("Bénédiction impie", 2, Aura, 8f, 10f, 13500, 108000, C(0.9f, 0.4f, 1f), "+{a} d'attaque et +{h} PV pendant {d}s"),
        new SkillDef("Pluie de flèches", 2, Volley, 10f, 0f, 135000, 0, C(0.85f, 0.85f, 1f), "Une pluie de flèches sur tous les ennemis : {a} dégâts"),
        new SkillDef("Ronces ardentes", 2, Strike, 5f, 0f, 27648, 0, C(0.5f, 1f, 0.3f), "Des ronces transpercent l'ennemi de tête : {a} dégâts"),
        new SkillDef("Cri de guerre", 3, Aura, 8f, 10f, 128000, 1024000, C(1f, 0.85f, 0.3f), "+{a} d'attaque et +{h} PV pendant {d}s"),
        new SkillDef("Bombe infernale", 3, Volley, 6f, 0f, 600000, 0, C(1f, 0.4f, 0.1f), "Explosion sur tous les ennemis : {a} dégâts"),
        new SkillDef("Météore", 3, Volley, 9f, 0f, 1000000, 0, C(1f, 0.3f, 0.05f), "Un météore s'écrase sur les ennemis : {a} dégâts"),
        new SkillDef("Ruée des damnés", 4, Volley, 20f, 0f, 2160000, 0, C(0.6f, 0.9f, 1f), "Une horde d'âmes balaie les ennemis : {a} dégâts"),
        new SkillDef("Ver des abysses", 4, Strike, 8f, 0f, 4320000, 0, C(0.8f, 0.5f, 0.3f), "Un ver géant dévore l'ennemi de tête : {a} dégâts"),
        new SkillDef("Foudre noire", 4, Strike, 3f, 0f, 2160000, 0, C(0.55f, 0.6f, 1f), "La foudre frappe l'ennemi de tête : {a} dégâts"),
        new SkillDef("Ferveur démoniaque", 5, Aura, 8f, 8f, 6220800, 49766400, C(1f, 0.2f, 0.5f), "+{a} d'attaque et +{h} PV pendant {d}s"),
        new SkillDef("Raid ailé", 5, Volley, 10f, 0f, 13824000, 0, C(0.7f, 1f, 0.9f), "Des gargouilles mitraillent les ennemis : {a} dégâts"),
        new SkillDef("Œil de Lucifer", 5, Drone, 8f, 10f, 15000000, 0, C(1f, 0.15f, 0.1f), "Un œil flottant tire toutes les 2s pendant {d}s : {a} par tir"),
    };

    public const int MaxLevel = 100;
    // Valeurs d'activation : x2,5 entre le niveau 1 et le niveau 100.
    public static double LevelMult(int level) => System.Math.Pow(2.5, (level - 1) / 99.0);

    // Bonus passif (toute la collection) : dégâts au niveau 1 et pente linéaire par niveau ; la vie vaut 8 x les dégâts.
    public static readonly double[] PassiveDamage = { 10, 80, 640, 5120, 40960, 327680 };
    public static readonly double[] PassiveSlope = { 0.1, 0.025, 0.01695, 0.01695, 0.01695, 0.01695 };

    // Invocation : 40 tickets par compétence, par paquets de 5 ou 25.
    public const int SummonCost = 40;
    public const int SummonSmall = 5, SummonBig = 25;
    public static readonly int[] SummonRequired = {2,2,4,6,8,10,12,14,16,18,20,30,40,50,60,80,100,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110,110};
    public static readonly float[][] SummonOdds =
    {
        new float[]{100.000f,0.000f,0.000f,0.000f,0.000f,0.000f},
        new float[]{99.950f,0.050f,0.000f,0.000f,0.000f,0.000f},
        new float[]{99.900f,0.100f,0.000f,0.000f,0.000f,0.000f},
        new float[]{99.750f,0.250f,0.000f,0.000f,0.000f,0.000f},
        new float[]{99.000f,1.000f,0.000f,0.000f,0.000f,0.000f},
        new float[]{98.000f,2.000f,0.000f,0.000f,0.000f,0.000f},
        new float[]{96.000f,4.000f,0.000f,0.000f,0.000f,0.000f},
        new float[]{94.889f,5.080f,0.031f,0.000f,0.000f,0.000f},
        new float[]{93.486f,6.452f,0.063f,0.000f,0.000f,0.000f},
        new float[]{91.681f,8.194f,0.125f,0.000f,0.000f,0.000f},
        new float[]{89.344f,10.406f,0.250f,0.000f,0.000f,0.000f},
        new float[]{86.285f,13.215f,0.500f,0.000f,0.000f,0.000f},
        new float[]{82.217f,16.783f,1.000f,0.000f,0.000f,0.000f},
        new float[]{76.685f,21.315f,2.000f,0.000f,0.000f,0.000f},
        new float[]{68.930f,27.070f,4.000f,0.000f,0.000f,0.000f},
        new float[]{61.181f,34.379f,4.440f,0.000f,0.000f,0.000f},
        new float[]{51.410f,43.661f,4.928f,0.000f,0.000f,0.000f},
        new float[]{39.048f,55.450f,5.471f,0.031f,0.000f,0.000f},
        new float[]{23.444f,70.421f,6.072f,0.063f,0.000f,0.000f},
        new float[]{17.500f,75.635f,6.740f,0.125f,0.000f,0.000f},
        new float[]{17.500f,74.768f,7.482f,0.250f,0.000f,0.000f},
        new float[]{17.500f,73.695f,8.305f,0.500f,0.000f,0.000f},
        new float[]{17.500f,72.282f,9.218f,1.000f,0.000f,0.000f},
        new float[]{17.500f,70.268f,10.232f,2.000f,0.000f,0.000f},
        new float[]{17.500f,67.142f,11.358f,4.000f,0.000f,0.000f},
        new float[]{17.500f,65.673f,12.607f,4.220f,0.000f,0.000f},
        new float[]{17.500f,64.054f,13.994f,4.452f,0.000f,0.000f},
        new float[]{17.500f,62.270f,15.533f,4.697f,0.000f,0.000f},
        new float[]{17.500f,60.303f,17.242f,4.955f,0.000f,0.000f},
        new float[]{17.500f,58.134f,19.138f,5.228f,0.000f,0.000f},
        new float[]{17.500f,55.741f,21.244f,5.515f,0.000f,0.000f},
        new float[]{17.500f,53.101f,23.580f,5.819f,0.000f,0.000f},
        new float[]{17.500f,50.187f,26.174f,6.139f,0.000f,0.000f},
        new float[]{17.500f,46.970f,29.053f,6.476f,0.000f,0.000f},
        new float[]{17.500f,43.418f,32.249f,6.833f,0.000f,0.000f},
        new float[]{17.500f,39.495f,35.797f,7.208f,0.000f,0.000f},
        new float[]{17.500f,35.161f,39.734f,7.605f,0.000f,0.000f},
        new float[]{17.500f,30.372f,44.105f,8.023f,0.000f,0.000f},
        new float[]{17.500f,25.048f,48.957f,8.464f,0.031f,0.000f},
        new float[]{17.500f,19.166f,54.342f,8.930f,0.063f,0.000f},
        new float[]{17.500f,16.500f,56.454f,9.421f,0.125f,0.000f},
        new float[]{17.500f,16.500f,55.811f,9.939f,0.250f,0.000f},
        new float[]{17.500f,16.500f,55.014f,10.486f,0.500f,0.000f},
        new float[]{17.500f,16.500f,53.937f,11.063f,1.000f,0.000f},
        new float[]{17.500f,16.500f,52.329f,11.671f,2.000f,0.000f},
        new float[]{17.500f,16.500f,49.687f,12.313f,4.000f,0.000f},
        new float[]{17.500f,16.500f,48.790f,12.990f,4.220f,0.000f},
        new float[]{17.500f,16.500f,47.843f,13.705f,4.452f,0.000f},
        new float[]{17.500f,16.500f,46.845f,14.458f,4.697f,0.000f},
        new float[]{17.500f,16.500f,45.791f,15.254f,4.955f,0.000f},
        new float[]{17.500f,16.500f,44.680f,16.093f,5.228f,0.000f},
        new float[]{17.500f,16.500f,43.507f,16.978f,5.515f,0.000f},
        new float[]{17.500f,16.500f,42.270f,17.911f,5.819f,0.000f},
        new float[]{17.500f,16.500f,40.965f,18.896f,6.139f,0.000f},
        new float[]{17.500f,16.500f,39.588f,19.936f,6.476f,0.000f},
        new float[]{17.500f,16.500f,38.135f,21.032f,6.833f,0.000f},
        new float[]{17.500f,16.500f,36.603f,22.189f,7.208f,0.000f},
        new float[]{17.500f,16.500f,34.986f,23.409f,7.605f,0.000f},
        new float[]{17.500f,16.500f,33.280f,24.697f,8.023f,0.000f},
        new float[]{17.500f,16.500f,31.480f,26.055f,8.464f,0.000f},
        new float[]{17.500f,16.500f,29.582f,27.488f,8.930f,0.000f},
        new float[]{17.500f,16.500f,27.579f,29.000f,9.421f,0.000f},
        new float[]{17.500f,16.500f,25.466f,30.595f,9.939f,0.000f},
        new float[]{17.500f,16.500f,23.236f,32.278f,10.486f,0.000f},
        new float[]{17.500f,16.500f,20.884f,34.053f,11.063f,0.000f},
        new float[]{17.500f,16.500f,16.500f,37.829f,11.671f,0.000f},
        new float[]{17.500f,16.500f,16.500f,37.187f,12.313f,0.000f},
        new float[]{17.500f,16.500f,16.500f,36.479f,12.990f,0.031f},
        new float[]{17.500f,16.500f,16.500f,35.733f,13.705f,0.063f},
        new float[]{17.500f,16.500f,16.500f,34.917f,14.458f,0.125f},
        new float[]{17.500f,16.500f,16.500f,33.996f,15.254f,0.250f},
        new float[]{17.500f,16.500f,16.500f,32.907f,16.093f,0.500f},
        new float[]{17.500f,16.500f,16.500f,31.522f,16.978f,1.000f},
        new float[]{17.500f,16.500f,16.500f,29.589f,17.911f,2.000f},
        new float[]{17.500f,16.500f,16.500f,26.604f,18.896f,4.000f},
        new float[]{17.500f,16.500f,16.500f,25.332f,19.936f,4.232f},
        new float[]{17.500f,16.500f,16.500f,23.990f,21.032f,4.477f},
        new float[]{17.500f,16.500f,16.500f,22.574f,22.189f,4.737f},
        new float[]{17.500f,16.500f,16.500f,21.079f,23.409f,5.012f},
        new float[]{17.500f,16.500f,16.500f,19.500f,24.697f,5.303f},
        new float[]{17.500f,16.500f,16.500f,17.835f,26.055f,5.610f},
        new float[]{17.500f,16.500f,16.500f,16.500f,27.064f,5.936f},
        new float[]{17.500f,16.500f,16.500f,16.500f,26.720f,6.280f},
        new float[]{17.500f,16.500f,16.500f,16.500f,26.356f,6.644f},
        new float[]{17.500f,16.500f,16.500f,16.500f,25.971f,7.029f},
        new float[]{17.500f,16.500f,16.500f,16.500f,25.563f,7.437f},
        new float[]{17.500f,16.500f,16.500f,16.500f,25.132f,7.868f},
        new float[]{17.500f,16.500f,16.500f,16.500f,24.675f,8.325f},
        new float[]{17.500f,16.500f,16.500f,16.500f,24.192f,8.808f},
        new float[]{17.500f,16.500f,16.500f,16.500f,23.682f,9.318f},
        new float[]{17.500f,16.500f,16.500f,16.500f,23.141f,9.859f},
        new float[]{17.500f,16.500f,16.500f,16.500f,22.569f,10.431f},
        new float[]{17.500f,16.500f,16.500f,16.500f,21.964f,11.036f},
        new float[]{17.500f,16.500f,16.500f,16.500f,21.324f,11.676f},
        new float[]{17.500f,16.500f,16.500f,16.500f,20.647f,12.353f},
        new float[]{17.500f,16.500f,16.500f,16.500f,19.930f,13.070f},
        new float[]{17.500f,16.500f,16.500f,16.500f,19.172f,13.828f},
        new float[]{17.500f,16.500f,16.500f,16.500f,18.370f,14.630f},
        new float[]{17.500f,16.500f,16.500f,16.500f,17.522f,15.478f},
        new float[]{17.500f,16.500f,16.500f,16.500f,16.500f,16.500f}
    };
    // Doublons nécessaires pour passer du niveau L au niveau L+1 (index L-1).
    public static readonly int[] CopiesForNext = {2,2,2,2,2,3,3,3,3,3,4,4,4,4,5,5,5,5,5,5,5,6,6,6,6,7,7,7,7,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8};
}
