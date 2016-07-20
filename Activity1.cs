using System;
using Android.Content;
using Android.Util;

namespace NetworkDetection
{
    using Android.App;
    using Android.Net;
    using Android.OS;
    using Android.Widget;


    [Activity(Label = "Network Detection", MainLauncher = true, Icon = "@drawable/icon")]
    public class Activity1 : Activity
    {
        private const string UnknownConnectionType = "N/A";
        private static readonly string Tag = typeof(Activity1).FullName;

        private ImageView _isConnectedImage;
        private ImageView _roamingImage;
        private ImageView _wifiImage;
		private TextView _connectionType;

        private NetworkStatusBroadcastReceiver _broadcastReceiver;
        public event EventHandler NetworkStatusChanged;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            _wifiImage = FindViewById<ImageView>(Resource.Id.wifi_image);
            _roamingImage = FindViewById<ImageView>(Resource.Id.roaming_image);
            _isConnectedImage = FindViewById<ImageView>(Resource.Id.is_connected_image);
			_connectionType = FindViewById<TextView>(Resource.Id.connection_type_text);

            SetInterfaceOffline();
        }

        protected override void OnStart()
        {
            base.OnStart();
            DetectNetwork();
            InitBroadcastReceiver();
        }

        private void InitBroadcastReceiver()
        {
            // Create the broadcast receiver and bind the event handler
            // so that the app gets updates of the network connectivity status
            _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            // Register the broadcast receiver
            Application.Context.RegisterReceiver(_broadcastReceiver,
              new IntentFilter(ConnectivityManager.ConnectivityAction));
        }

        private void OnNetworkStatusChanged(object sender, EventArgs e)
        {
            NetworkStatusChanged?.Invoke(this, EventArgs.Empty);
            DetectNetwork();
        }

        protected override void OnStop()
        {
            base.OnStop();
            if (_broadcastReceiver == null)
            {
                throw new InvalidOperationException(
                    "Network status monitoring not active.");
            }
            // Unregister the receiver so we no longer get updates.
            Application.Context.UnregisterReceiver(_broadcastReceiver);
            // Set the variable to nil, so that we know the receiver is no longer used.
            _broadcastReceiver.ConnectionStatusChanged -= OnNetworkStatusChanged;
            _broadcastReceiver = null;
        }

        private void DetectNetwork()
        {
			ConnectivityManager connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
            NetworkInfo activeConnection = connectivityManager.ActiveNetworkInfo;

            bool isOnline = activeConnection?.IsConnected ?? false;

            Log.Debug(Tag, "IsOnline = {0}", isOnline);

            if (!isOnline)
            {
                SetInterfaceOffline();
                return;
            }

            SetInterface(true, activeConnection.Type == ConnectivityType.Wifi, activeConnection.IsRoaming, activeConnection.TypeName);
            //SetConnectionStatus(true);
            //SetWifiStatus(activeConnection.Type == ConnectivityType.Wifi);
            //SetRoamingStatus(activeConnection.IsRoaming);
            //SetConnectionTypeTitle(activeConnection.TypeName);// Display the type of connection
            

            //----------------------------------------------------------------
            // Check for a WiFi connection
            //NetworkInfo wifiInfo = connectivityManager?.GetNetworkInfo(ConnectivityType.Wifi);
            //            var wifiInfo = connectivityManager?.Get;
            //            if (wifiInfo.IsConnected)
            //{
            //	Log.Debug(Tag, "Wifi connected.");
            //	_wifiImage.SetImageResource(Resource.Drawable.green_square);
            //} else
            //{
            //	Log.Debug(Tag, "Wifi disconnected.");
            //	_wifiImage.SetImageResource(Resource.Drawable.red_square);
            //}

            // Check if roaming
            //NetworkInfo mobileInfo = connectivityManager.GetNetworkInfo(ConnectivityType.Mobile);
            //if(mobileInfo.IsRoaming && mobileInfo.IsConnected)
            //{
            //	Log.Debug(Tag, "Roaming.");
            //	_roamingImage.SetImageResource(Resource.Drawable.green_square);
            //} else
            //{
            //	Log.Debug(Tag, "Not roaming.");
            //	_roamingImage.SetImageResource(Resource.Drawable.red_square);
            //}
        }

        private void SetInterfaceOffline()
        {
            SetInterface(false, false, false);
        }

        private void SetInterface(bool isConnected, bool isWifi, bool isRoaming, string coneccionType = UnknownConnectionType)
        {
            SetConnectionStatus(isConnected);
            SetWifiStatus(isWifi);
            SetRoamingStatus(isRoaming);
            SetConnectionTypeTitle(coneccionType);
        }

        private void SetConnectionStatus(bool isConnected)
        {
            Log.Debug(Tag, $"Network {(isConnected ? "connected" : "disconnected")}.");
            _isConnectedImage.SetImageResource(isConnected ? Resource.Drawable.green_square : Resource.Drawable.red_square);
        }

        private void SetWifiStatus(bool isWifiEnabled)
        {
            Log.Debug(Tag, $"Wifi {(isWifiEnabled ? "connected" : "disconnected")}.");
            _wifiImage.SetImageResource(isWifiEnabled ? Resource.Drawable.green_square : Resource.Drawable.red_square);
        }

        private void SetRoamingStatus(bool isRoamingEnabled)
        {
            Log.Debug(Tag, $"Roaming {(isRoamingEnabled ? "enabled" : "disabled")}.");
            _roamingImage.SetImageResource(isRoamingEnabled ? Resource.Drawable.green_square : Resource.Drawable.red_square);
        }

        private void SetConnectionTypeTitle(string connectionType)
        {
            _connectionType.Text = connectionType;
        }
    }
}
