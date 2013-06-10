namespace HSR.PresWriter.InputEmulation
{
    public static class AdvancedInputEmulatorFactory
    {
        /// <summary>
        /// Checking the OS version
        /// </summary>
        /// <param name="width">Screen resolution</param>
        /// <param name="height">Screen resolution</param>
        /// <returns>The correct Emulator for the current OS</returns>
        public static IAdvancedInputEmulator GetInputEmulator(int width, int height)
        {
            if(Touch.IsSupported)
                return new AdvancedTouchEmulator(1, width, height);
            return new AdvancedMouseEmulator(width, height);
        }
    }
}
