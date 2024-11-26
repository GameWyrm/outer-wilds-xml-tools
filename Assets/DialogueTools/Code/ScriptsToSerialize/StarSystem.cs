using System;

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
