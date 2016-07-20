using System;
using Android.Content;
using Android.Telephony;
using Android.Util;

namespace NetworkDetection
{
    using Android.App;
    using Android.Net;
    using Android.OS;
    using Android.Widget;

    [Activity(Label = "Network Detection", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private const string UnknownConnectionType = "N/A";
        private static readonly string Tag = typeof(MainActivity).FullName;

        private ImageView _isConnectedImage;
        private ImageView _roamingImage;
        private ImageView _wifiImage;
        private ImageView _signalStrengthImage;
        private TextView _connectionType;
        private TextView _signalStrength;

        private TelephonyManager _telephonyManager;
        
        private NetworkStatusBroadcastReceiver _broadcastReceiver;
        public event EventHandler NetworkStatusChanged;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            FindViews();

            InitSignalStrengthListener();

            SetInterfaceOffline();
        }

        private void FindViews()
        {
            _wifiImage = FindViewById<ImageView>(Resource.Id.wifi_image);
            _roamingImage = FindViewById<ImageView>(Resource.Id.roaming_image);
            _isConnectedImage = FindViewById<ImageView>(Resource.Id.is_connected_image);
            _signalStrengthImage = FindViewById<ImageView>(Resource.Id.imgSignalStrength);
            _connectionType = FindViewById<TextView>(Resource.Id.connection_type_text);
            _signalStrength = FindViewById<TextView>(Resource.Id.txtSignalStrength);
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

        private void InitSignalStrengthListener()
        {
            _telephonyManager = (TelephonyManager)GetSystemService(TelephonyService);
            var signalStrengthReceiver = new SignalStrengthBroadcastReceiver();
            _telephonyManager.Listen(signalStrengthReceiver, PhoneStateListenerFlags.SignalStrengths);
            signalStrengthReceiver.SignalStrengthsChanged += SignalStrengthReceiver_SignalStrengthsChanged;
        }

        protected override void OnStart()
        {
            base.OnStart();
            DetectNetwork();
            InitBroadcastReceiver();
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
        
        private void OnNetworkStatusChanged(object sender, EventArgs e)
        {
            NetworkStatusChanged?.Invoke(this, EventArgs.Empty);
            DetectNetwork();
        }
        
        private void SignalStrengthReceiver_SignalStrengthsChanged(SignalStrength pStrength)
        {
            /*
            var parts = pStrength.ToString().Split(' ');
            part[0] = "Signalstrength:"  _ignore this, it's just the title_
            parts[1] = GsmSignalStrength
            parts[2] = GsmBitErrorRate
            parts[3] = CdmaDbm
            parts[4] = CdmaEcio
            parts[5] = EvdoDbm
            parts[6] = EvdoEcio
            parts[7] = EvdoSnr
            parts[8] = LteSignalStrength
            parts[9] = LteRsrp
            parts[10] = LteRsrq
            parts[11] = LteRssnr
            parts[12] = LteCqi
            parts[13] = gsm|lte|cdma
            parts[14] = _not really sure what this number is_
             */
            var signalTypeName = Enum.GetName(typeof(NetworkType), _telephonyManager.NetworkType);

            // Update the UI with text and an image.
            _signalStrengthImage.SetImageLevel(pStrength.Level);
            _signalStrength.Text = $"{signalTypeName} Signal Quality 0-4 ({pStrength.Level})";
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
