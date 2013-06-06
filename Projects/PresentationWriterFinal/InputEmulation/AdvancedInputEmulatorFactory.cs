namespace InputEmulation
{
    public static class AdvancedInputEmulatorFactory
    {
        public static IAdvancedInputEmulator GetInputEmulator(int width, int height)
        {
            if(Touch.IsSupported)
                return new AdvancedTouchEmulator(1);
            return new AdvancedMouseEmulator(width, height);
        }
    }
}
