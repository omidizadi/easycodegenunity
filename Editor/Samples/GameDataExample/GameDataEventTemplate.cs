using System;

namespace easycodegenunity.Editor.Samples.GameDataExample
{
    internal class GameDataEventTemplate
    {
        private string _ORIGINAL_FIELD_;
        private event Action<string> On_GAME_DATA_FIELD_Changed;

        internal void Set_GAME_DATA_FIELD_(string newValue)
        {
            _ORIGINAL_FIELD_ = newValue;
            On_GAME_DATA_FIELD_Changed?.Invoke(newValue);
        }

        internal string Get_GAME_DATA_FIELD_()
        {
            return _ORIGINAL_FIELD_;
        }
    }
}