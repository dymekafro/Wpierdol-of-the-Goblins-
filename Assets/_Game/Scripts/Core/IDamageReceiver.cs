using UnityEngine;

namespace WPG.Core
{
    public interface IDamageReceiver
    {
        void ReceiveDamage(int amount, Vector3 hitPoint);
    }
}
