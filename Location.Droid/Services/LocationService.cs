using System;

using Android.App;
using Android.Util;
using Android.Content;
using Android.OS;
using Android.Locations;
using Android.Gms.Location;
using Android.Gms.Common.Apis;

namespace Location.Droid.Services
{
	[Service]
	public class LocationService : Service
	, GoogleApiClient.IConnectionCallbacks
	, GoogleApiClient.IOnConnectionFailedListener
	, Android.Gms.Location.ILocationListener
	{
		public event EventHandler<LocationChangedEventArgs> LocationChanged = delegate { };

		public LocationService() 
		{
		}
			
		GoogleApiClient apiClient;
		LocationRequest locRequest;
		readonly string logTag = "LocationService";
		IBinder binder;

		public override void OnCreate ()
		{
			base.OnCreate ();
			Log.Debug (logTag, "OnCreate called in the Location Service");
			apiClient = new GoogleApiClient.Builder (this, this, this)
				.AddApi (LocationServices.API)
				.Build ();
			locRequest = new LocationRequest ();
			apiClient.Connect ();

			Log.Debug (logTag, "OnCreate called in the Location Service");
		}

		// This gets called when StartService is called in our App class
		public override StartCommandResult OnStartCommand (Intent intent, StartCommandFlags flags, int startId)
		{
			Log.Debug (logTag, "LocationService started");

			return StartCommandResult.Sticky;
		}

		// This gets called once, the first time any client bind to the Service
		// and returns an instance of the LocationServiceBinder. All future clients will
		// reuse the same instance of the binder
		public override IBinder OnBind (Intent intent)
		{
			Log.Debug (logTag, "Client now bound to service");

			binder = new LocationServiceBinder (this);
			return binder;
		}

		// Handle location updates from the location manager
		public void StartLocationUpdates () 
		{
			locRequest.SetPriority (100);
			locRequest.SetFastestInterval (500);
			locRequest.SetInterval (1000);
			LocationServices.FusedLocationApi.RequestLocationUpdates (apiClient, locRequest, this);

			Log.Debug (logTag, "Now sending location updates");
		}

		public override void OnDestroy ()
		{
			base.OnDestroy ();
			LocationServices.FusedLocationApi.RemoveLocationUpdates (apiClient, this);
			apiClient.Disconnect ();
			Log.Debug (logTag, "Service has been terminated");
		}

		#region ILocationListener implementation
		// ILocationListener is a way for the Service to subscribe for updates
		// from the System location Service

		public void OnLocationChanged (Android.Locations.Location location)
		{
			this.LocationChanged (this, new LocationChangedEventArgs (location));

			// This should be updating every time we request new location updates
			// both when teh app is in the background, and in the foreground
			Log.Debug (logTag, String.Format ("Latitude is {0}", location.Latitude));
			Log.Debug (logTag, String.Format ("Longitude is {0}", location.Longitude));
			Log.Debug (logTag, String.Format ("Altitude is {0}", location.Altitude));
			Log.Debug (logTag, String.Format ("Speed is {0}", location.Speed));
			Log.Debug (logTag, String.Format ("Accuracy is {0}", location.Accuracy));
			Log.Debug (logTag, String.Format ("Bearing is {0}", location.Bearing));
		}

		public void OnConnected (Bundle bundle)
		{
			StartLocationUpdates ();
			Log.Info (logTag, "Now connected to client");
		}
			
		public void OnConnectionFailed (Android.Gms.Common.ConnectionResult bundle)
		{
			// This method is used to handle connection issues with the Google Play Services Client (LocationClient). 
			// You can check if the connection has a resolution (bundle.HasResolution) and attempt to resolve it

			// You must implement this to implement the IGooglePlayServicesClientOnConnectionFailedListener Interface
			Log.Info("LocationClient", "Connection failed, attempting to reach google play services");
		}

		public void OnConnectionSuspended (int i)
		{
			Log.Info (logTag, "Connection suspended");
		}
		#endregion

	}
}

