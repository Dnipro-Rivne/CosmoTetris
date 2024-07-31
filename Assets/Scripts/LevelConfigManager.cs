using System.Collections.Generic;
using UnityEngine;

public class LevelConfigManager : MonoBehaviour
{
    [SerializeField]
    private List<LevelConfig> levelConfigs;

    public static LevelConfigManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public LevelConfig GetLevelConfig(int level)
    {
        // var config = levelConfigs.Find(config => config.level == level);
        // if (config == null)
        // {
        //     Debug.LogError($"No level config found for level {level}");
        // }
        if (level > levelConfigs.Count)
            level = 0;
        return levelConfigs[level];
    }
}