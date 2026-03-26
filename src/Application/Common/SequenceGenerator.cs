namespace Application.Common
{
    public static class SequenceGenerator
    {
        private static int _lastNumber = 0;

        public static int Next()
        {
            return Interlocked.Increment(ref _lastNumber);
        }
    }
}
