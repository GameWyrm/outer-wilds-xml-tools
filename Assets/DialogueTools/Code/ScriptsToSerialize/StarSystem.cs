using System;
using UnityEngine;
using Newtonsoft.Json;

namespace XmlTools
{
    [Serializable]
    public class StarSystem
    {
        [Serializable]
        public class EntryPositionInfo
        {
            public string id;
            public MVector2 position;
        }

        [Serializable]
        public class CuriosityColorInfo
        {
            public MColor color;
            public MColor highlightColor;
            public string id;
        }

        [Serializable]
        public class MColor
        {
            public int r;
            public int g;
            public int b;
            public int a;

            public MColor(int r, int g, int b, int a)
            {
                this.r = r;
                this.g = g;
                this.b = b;
                this.a = a;
            }

            // I can't see a situation where you'd ever want Alpha to *not* be 1 when importing
            [JsonConstructor]
            public MColor(int r, int g, int b)
            {
                this.r = r;
                this.g = g;
                this.b = b;
                this.a = 1;
            }

            public static implicit operator Color(MColor mColor)
            {
                return new Color(
                    mColor.r / 255f,
                    mColor.g / 255f,
                    mColor.b / 255f,
                    mColor.a / 255f
                    );
            }

            public static implicit operator MColor(Color color)
            {
                return new MColor(
                    Mathf.RoundToInt(color.r * 255),
                    Mathf.RoundToInt(color.g * 255),
                    Mathf.RoundToInt(color.b * 255),
                    Mathf.RoundToInt(color.a * 255)
                    );
            }
        }

        [Serializable]
        public class MVector2
        {
            public float x;
            public float y;
        }

        public EntryPositionInfo[] entryPositions;
        public string[] initialReveal;
        public CuriosityColorInfo[] curiosities;
    }
}
