using UnityEngine;

namespace WPG.Core
{
    /// <summary>
    /// Tani cache Camera.main — odświeżany co ~1s lub gdy referencja zniknie.
    /// Zastępuje kosztowne Camera.main (FindGameObjectsWithTag) wołane co klatkę
    /// w wielu obiektach (paski HP, numery obrażeń itp.).
    /// </summary>
    public static class CameraCache
    {
        private const float RefreshInterval = 1f;

        private static Camera _camera;
        private static float _nextRefreshAt;

        public static Camera Main
        {
            get
            {
                if (_camera == null || Time.unscaledTime >= _nextRefreshAt)
                {
                    _camera = Camera.main;
                    _nextRefreshAt = Time.unscaledTime + RefreshInterval;
                }
                return _camera;
            }
        }
    }
}
