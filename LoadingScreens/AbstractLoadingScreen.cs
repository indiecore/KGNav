using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KGTools.General
{
	/// <summary>
	/// Interface for loading screen data.
	/// </summary>
	public interface ILoadingScreenData { }

	/// <summary>
	/// This class implements the required logic for showing a general purpose loading screen.
	/// </summary>
	public abstract class AbstractLoadingScreen : MonoBehaviour
	{

		#region Delegates & Events

		/// <summary>
		/// Delegate for events involving loading screens.
		/// </summary>
		/// <param name="eventScreen">The loading screen that is involved with the event.</param>
		public delegate void LoadingScreenEvent(AbstractLoadingScreen eventScreen);

		/// <summary>
		/// Called when the loading screen is first told to open, before the loading screen is 
		/// </summary>
		public event LoadingScreenEvent OnLoadingScreenWillOpen = null;

		/// <summary>
		/// Event that will be called when the loading screen is open and 
		/// </summary>
		public event LoadingScreenEvent OnLoadingScreenOpen = null;

		/// <summary>
		/// Event that will be called when the loading screen is preparing to close.
		/// </summary>
		public event LoadingScreenEvent OnLoadingScreenWillClose = null;

		/// <summary>
		/// Event that will be called when the loading screen is 
		/// </summary>
		public event LoadingScreenEvent OnLoadingScreenClosed = null;

		#endregion

		#region Data

		[SerializeField, Header("----- Loading Screen -----")]
		private string loadingScreenID = string.Empty;
		/// <summary>
		/// Identifying string for this loading screen.
		/// </summary>
		public string LoadingScreenID
		{
			get
			{
				return this.loadingScreenID;
			}
		}

		/// <summary>
		/// The minimum amount of time to display this loading screen for.
		/// </summary>
		[SerializeField]
		private float minimumDisplayTime = 0f;

		/// <summary>
		/// The time that this loading screen was opened.
		/// </summary>
		private float openTime = 0f;

		#endregion

		#region Logic

		/// <summary>
		/// Initialize this loading screen.
		/// </summary>
		/// <param name="initializationData">Loading screen data to initialize.</param>
		public virtual void InitializeLoadingScreen(ILoadingScreenData initializationData = null)
		{
			// By default don't do anything special on initialization.
		}

		/// <summary>
		/// Open this loading screen.
		/// </summary>
		/// <param name="loadingScreenOpen">Callback for when the loading screen is completely open and </param>
		public void Open(System.Action loadingScreenOpen = null)
		{
			this.gameObject.SetActive(true);
			this.StartCoroutine(this.OpenScreenInternal(loadingScreenOpen));
		}

		/// <summary>
		/// Forces the loading screen into the open state immediately.
		/// </summary>
		public void ForceOpen()
		{
			if (this.OnLoadingScreenWillOpen != null)
			{
				this.OnLoadingScreenWillOpen(this);
			}

			this.gameObject.SetActive(true);
			this.OpenImmediate();
			this.openTime = Time.realtimeSinceStartup;

			if (this.OnLoadingScreenOpen != null)
			{
				this.OnLoadingScreenOpen(this);
			}
		}

		/// <summary>
		/// Close this loading screen.
		/// </summary>
		/// <param name="loadingScreenClosed">Callback for when the loading screen is completely closed.</param>
		public void Close(System.Action loadingScreenClosed = null)
		{
			this.StartCoroutine(this.CloseScreenInternal(loadingScreenClosed));
		}

		/// <summary>
		/// Forces this loading screen to close immediately.
		/// </summary>
		public void ForceClose()
		{
			if (this.OnLoadingScreenWillClose != null)
			{
				this.OnLoadingScreenWillClose(this);
			}

			this.CloseImmediate();
			this.gameObject.SetActive(false);

			if (this.OnLoadingScreenClosed != null)
			{
				this.OnLoadingScreenClosed(this);
			}
		}

		/// <summary>
		/// Handles calling events in the correct sequence and 
		/// </summary>
		/// <param name="openCallback">Local callback that will be called when the loading screen is </param>
		private IEnumerator OpenScreenInternal(System.Action openCallback = null)
		{
			if (this.OnLoadingScreenWillOpen != null)
			{
				this.OnLoadingScreenWillOpen(this);
			}

			yield return this.StartCoroutine(this.OpenLoadingScreen());

			if (this.OnLoadingScreenOpen != null)
			{
				this.OnLoadingScreenOpen(this);
			}

			this.openTime = Time.realtimeSinceStartup;
			if (openCallback != null)
			{
				openCallback();
			}
		}

		/// <summary>
		/// Closes the internal screen.
		/// </summary>
		/// <param name="closeCallback">Local callback that will be called when the loading screen is completely closes.</param>
		private IEnumerator CloseScreenInternal(System.Action closeCallback = null)
		{
			if (this.OnLoadingScreenWillClose != null)
			{
				this.OnLoadingScreenWillClose(this);
			}

			yield return new WaitForSecondsRealtime(this.minimumDisplayTime - (Time.realtimeSinceStartup - openTime));
			yield return this.StartCoroutine(this.CloseLoadingScreen());

			if (this.OnLoadingScreenClosed != null)
			{
				this.OnLoadingScreenClosed(this);
			}

			if (closeCallback != null)
			{
				closeCallback();
			}

			this.gameObject.SetActive(false);
		}

		/// <summary>
		/// Handles the navigation scene manager informing loading percentage.
		/// </summary>
		/// <param name="loadPercentage">The loading percentage.</param>
		public virtual void SetLoadPercentage(float loadPercentage)
		{
			// No default behaviour.
		}

		/// <summary>
		/// This function will handle the 
		/// </summary>
		/// <param name="loadingScreenOpen">Callback that will be called when the loading screen is totally open.</param>
		protected abstract IEnumerator OpenLoadingScreen();

		/// <summary>
		/// This function will handle the specifics of opening the loading screen in the abstract loading
		/// </summary>
		/// <param name="loadingScreenClosed">Callback for when the loading screen is closed.</param>
		protected abstract IEnumerator CloseLoadingScreen();

		/// <summary>
		/// Open this loading screen immediately.
		/// </summary>
		protected abstract void OpenImmediate();


		/// <summary>
		/// Close this loading screen immediately.
		/// </summary>
		protected abstract void CloseImmediate();

		#endregion

	}
}
