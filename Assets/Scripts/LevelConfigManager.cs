using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class LevelConfigManager
{
    private static List<LevelConfig> levelConfigs;

    static LevelConfigManager()
    {
        // Load all LevelConfig assets from Resources/LevelConfigs folder
        levelConfigs = Resources.LoadAll<LevelConfig>("LevelConfigs").ToList();
        Debug.Log($"Loaded {levelConfigs.Count} level configs from Resources/LevelConfigs.");
        
        foreach (var config in levelConfigs)
        {
            Debug.Log($"Loaded config: {config.name}, level: {config.level}");
        }
    }

    public static LevelConfig GetLevelConfig(int level)
    {
        var config = levelConfigs.FirstOrDefault(config => config.level == level);
        if (config == null)
        {
            Debug.LogError($"No level config found for level {level}");
        }
        return config;
    }
}