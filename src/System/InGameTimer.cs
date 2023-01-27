using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenLaMulana.System
{
    public class InGameTimer : IGameEntity
    {
        long _inGameTimer = 0;
        long _limit = (long)TimeSpan.MaxValue.TotalSeconds;
        float _countDuration = 1f; //every 1s.
        float _currentTime = 0f;

        int IGameEntity.Depth { get; set; } = (int)Global.DrawOrder.UI;

        Effect IGameEntity.ActiveShader => null;

        bool IGameEntity.LockTo30FPS { get; set; } = false;

        public InGameTimer()
        {

        }
        
        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {

        }

        public void Update(GameTime gameTime)
        {
            if (_inGameTimer >= _limit)
                return;

            _currentTime += (float)gameTime.ElapsedGameTime.TotalSeconds; //Time passed since last Update() 

            if (_currentTime >= _countDuration)
            {
                _inGameTimer++;
                _currentTime -= _countDuration; // "use up" the time
                                              //any actions to perform
            }
        }

        public override string ToString()
        {
            long currTime = Global.InGameTimer.GetTime();
            TimeSpan span = TimeSpan.FromSeconds(Math.Clamp(currTime, 0, _limit));
            string hoursString = span.TotalHours.ToString("000000000");
            int shownDigits = 2;
            int shownDigitsIterator = shownDigits;
            int potentialShownDigits = 0;
            for (int i = hoursString.Length - shownDigits; i >= 0; i--)
            {
                shownDigits++;
                if (hoursString[i] == '0')
                {
                    int j = i;
                    bool foundDigits = true;
                    while (hoursString[j] == '0')
                    {
                        if (j <= 0)
                        {
                            shownDigits--;
                            foundDigits = false;
                            break;
                        }
                        potentialShownDigits++;
                        j--;
                        i--;
                    }

                    if (hoursString[j] == '0' && j > 0)
                    {
                        foundDigits = true;
                    }
                    if (foundDigits)
                        shownDigits += potentialShownDigits;
                    else
                        shownDigits--;

                    potentialShownDigits = 0;
                }
            }
            shownDigits += potentialShownDigits;
            shownDigits = Math.Clamp(shownDigits, 2, 9);

            string fromTimeString = string.Format("{0}:{1}:{2}", hoursString.Substring(hoursString.Length - shownDigits, shownDigits), span.Minutes.ToString("00"), span.Seconds.ToString("00"));

            return fromTimeString;
        }

        public long GetTime() {
            return _inGameTimer;
        }
    }
}
