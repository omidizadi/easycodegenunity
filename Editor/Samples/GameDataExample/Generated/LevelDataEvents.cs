using System;
using UnityEngine;

namespace easycodegenunity.Editor.Samples.GameDataExample
{
    public partial struct LevelData
    {
        public event Action<int> OnLevelNumberChanged;
        public event Action<string> OnLevelNameChanged;
        public event Action<float> OnLevelExpChanged;
        public event Action<int> OnLevelEnemyCountChanged;
        public event Action<float> OnLevelItemDropRateChanged;
        public void SetLevelNumber(int newValue)
        {
            levelNumber = newValue;
            OnLevelNumberChanged?.Invoke(newValue);
        }

        public int GetLevelNumber()
        {
            return levelNumber;
        }

        public void SetLevelName(string newValue)
        {
            levelName = newValue;
            OnLevelNameChanged?.Invoke(newValue);
        }

        public string GetLevelName()
        {
            return levelName;
        }

        public void SetLevelExp(float newValue)
        {
            levelExp = newValue;
            OnLevelExpChanged?.Invoke(newValue);
        }

        public float GetLevelExp()
        {
            return levelExp;
        }

        public void SetLevelEnemyCount(int newValue)
        {
            levelEnemyCount = newValue;
            OnLevelEnemyCountChanged?.Invoke(newValue);
        }

        public int GetLevelEnemyCount()
        {
            return levelEnemyCount;
        }

        public void SetLevelItemDropRate(float newValue)
        {
            levelItemDropRate = newValue;
            OnLevelItemDropRateChanged?.Invoke(newValue);
        }

        public float GetLevelItemDropRate()
        {
            return levelItemDropRate;
        }
    }
}