using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OpenLaMulana
{
    internal class SpriteDef
    {
        private Vector2 _origin = Vector2.Zero;
        private List<Rectangle> _assemblyData = new List<Rectangle>();
        private int _sprID;
        private Vector2 _size;

        public SpriteDef(int sprID)
        {
            _sprID = sprID;
        }

        internal void AddAssemblyData(Rectangle frameData)
        {
            _assemblyData.Add(frameData);
        }

        public List<Rectangle> GetAssemblyData() {
            return _assemblyData;
        }

        public int GetID() {
            return _sprID;
        }

        public Vector2 GetOrigin() {
            return _origin;
        }

        public Vector2 GetSize() {
            return _size;
        }

        internal void SetOrigin(Vector2 origin) {
            _origin = origin;
        }

        internal void SetSize(Vector2 size) {
            _size = size;
        }
    }
}