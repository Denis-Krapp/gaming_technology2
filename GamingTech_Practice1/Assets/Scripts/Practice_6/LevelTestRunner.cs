using UnityEngine;

public class LevelTestRunner : MonoBehaviour
{
    public int testSeed = 1;
    public int width = 64;
    public int height = 36;
    public int fillPercent = 45;
    public int iterations = 4;

    private void Start()
    {
        LevelGenerator gen = new LevelGenerator(width, height, fillPercent, iterations, testSeed);
        bool success = gen.GenerateAndValidate(out int attempts);

        if (success)
        {
            Debug.Log($"Level successfully generated with seed {gen.seed} after {attempts} attempt(s)!");
            Debug.Log($"Start: {gen.startPos} | Exit: {gen.exitPos} | Path Length: {gen.pathNodes.Count}");
            Debug.Log($"Items -> Common: {gen.lastItemCounts.common}, Uncommon: {gen.lastItemCounts.uncommon}, Rare: {gen.lastItemCounts.rare}, Epic: {gen.lastItemCounts.epic}, Legendary: {gen.lastItemCounts.legendary}");
            Debug.Log("\n" + gen.PrintLevelToString());
        }
        else
        {
            Debug.LogWarning("Failed to generate valid level.");
        }
    }
}