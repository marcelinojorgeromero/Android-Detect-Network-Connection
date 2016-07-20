using System;
using Android.Content;

namespace NetworkDetection
{
    [BroadcastReceiver]
    public class NetworkStatusBroadcastReceiver : BroadcastReceiver
    {
        public event EventHandler ConnectionStatusChanged;
        public override void OnReceive(Context context, Intent intent)
        {
            ConnectionStatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}