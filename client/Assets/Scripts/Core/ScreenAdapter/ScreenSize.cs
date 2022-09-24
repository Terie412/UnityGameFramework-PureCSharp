namespace Core
{
    public struct ScreenSize
    {
        public int width;
        public int height;

        public ScreenSize(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public bool Equals(ScreenSize other)
        {
            return width == other.width && height == other.height;
        }
    }
}