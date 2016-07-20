using System;
using Android.Content;
using Android.Telephony;

namespace NetworkDetection
{
    [BroadcastReceiver]
    public class SignalStrengthBroadcastReceiver : PhoneStateListener
    {
        public delegate void SignalStrengthsChangedDelegate(SignalStrength pStrength);
        public event SignalStrengthsChangedDelegate SignalStrengthsChanged;

        public override void OnSignalStrengthsChanged(SignalStrength pStrength)
        {
            SignalStrengthsChanged?.Invoke(pStrength);
        }

        public delegate void SignalStrengthChangedDelegate(int pStrength);
        public event SignalStrengthChangedDelegate SignalStrengthChanged;

        [Obsolete("deprecated")]
        public override void OnSignalStrengthChanged(int pStrength)
        {
            SignalStrengthChanged?.Invoke(pStrength);
        }
    }
}