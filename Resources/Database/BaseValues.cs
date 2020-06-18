using NepBot.Resources.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NepBot.Resources.Database
{
    [Serializable]
    public class BaseValues
    {
        private int _max;
        private int _min;
        private int _current;
        private int _increaseRate;
        private int _temporary = 0;

        public int Temporary
        {
            get { return _temporary; }
            set
            {
                _temporary = value;
            }
        }

        public int IncreaseRate
        {
            get { return _increaseRate; }
            set { _increaseRate = value; }
        }

        public BaseValues(int max, int gainsMod, int curr = 0, int min = 0)
        {
            _max = max;
            _min = min;
            _current = curr;
            IncreaseRate = gainsMod;
        }

        public void LevelUp(int level, int _modifierAdjustment = 0)
        {
            _max = level * (IncreaseRate + _modifierAdjustment);
            //_current = _max;
        }

        public int Current
        {
            get { return _current; }
            set
            {
                _current = value + Temporary;
                if (_current < _min)
                    _current = _min;
                if (_current > _max)
                    _current = _max;
            }
        }

        public int Max
        {
            get { return _max; }
            set
            {
                _max = value + Temporary;
                if (_max < _min)
                    _max = _min;
                if (_max < _current)
                    _max = _current;
            }
        }

        public int Min
        {
            get { return _min; }
        }
    }
}
