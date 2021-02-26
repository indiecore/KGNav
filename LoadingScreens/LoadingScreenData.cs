using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KGTools.General
{

	/// <summary>
	/// This class contains the data for creating a loading screen.
	/// </summary>
	public class LoadingScreenData
	{

		private string loadingScreenID = string.Empty;
		/// <summary>
		/// The ID of the loading screen to use for this load attempt.
		/// </summary>
		public string LoadingScreenID
		{
			get
			{
				return this.loadingScreenID;
			}
		}

		private ILoadingScreenData data = null;
		/// <summary>
		/// The loading screen data for the provided loading screen.
		/// </summary>
		public ILoadingScreenData Data
		{
			get
			{
				return this.data;
			}
		}

		#region Constructor

		/// <summary>
		/// Create a new loading screen data object containing the information 
		/// </summary>
		/// <param name="loadingScreenID">The ID of the loading screen to use for the </param>
		/// <param name="loadingScreenSetup">Setup data for the requested loading screen. This object will be passed to the </param>
		public LoadingScreenData(string loadingScreenID, ILoadingScreenData loadingScreenSetup = null)
		{
			this.loadingScreenID = loadingScreenID;
			this.data = loadingScreenSetup;
		}

		#endregion

	}
}
