namespace WPG.Player
{
    /// <summary>
    /// Skaluje offset kamery Invector proporcjonalnie do rozmiaru postaci
    /// (analogicznie do ThirdPersonCamera.ApplyCharacterScale).
    /// </summary>
    public static class InvectorCameraScale
    {
        const float BaseDefaultDistance = 2.5f;
        const float BaseHeight = 1.4f;

        public static void Apply(vThirdPersonCamera cam, float scale)
        {
            if (cam == null || scale <= 0.01f) return;
            cam.defaultDistance = BaseDefaultDistance * scale;
            cam.height = BaseHeight * scale;
        }
    }
}
