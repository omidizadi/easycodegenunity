using System;
using easycodegenunity.Editor.Core;

namespace easycodegenunity.Editor.Samples.GameDataExample
{
    internal class GameDataEventTemplate
    {
        private Placeholder _ORIGINAL_FIELD_;
        private event Action<Placeholder> On_GAME_DATA_FIELD_Changed;

        private int randomField;

        public int RandomField
        {
            get => randomField;
            set
            {
                if (randomField != value)
                {
                    randomField = value;
                    On_GAME_DATA_FIELD_Changed?.Invoke(_ORIGINAL_FIELD_);
                }
            }
        }

        internal void Set_GAME_DATA_FIELD_(Placeholder newValue)
        {
            _ORIGINAL_FIELD_ = newValue;
            On_GAME_DATA_FIELD_Changed?.Invoke(newValue);
        }

        internal Placeholder Get_GAME_DATA_FIELD_()
        {
            return _ORIGINAL_FIELD_;
        }
    }
}