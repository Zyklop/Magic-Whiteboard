namespace HSR.PresWriter.InputEmulation
{
    public static class AdvancedInputEmulatorFactory
    {
        public static IAdvancedInputEmulator GetInputEmulator(int width, int height)
        {
            if(Touch.IsSupported)
                return new AdvancedTouchEmulator(1, width, height);
            return new AdvancedMouseEmulator(width, height);
        }
    }
}
