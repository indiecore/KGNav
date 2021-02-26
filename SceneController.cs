using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KGTools.General
{

	/// <summary>
	/// Scene controller is the basic class the root component of a scene.
	/// </summary>
	public abstract class SceneController : MonoBehaviour
	{
		
		#region Data

		/// <summary>
		/// The scene data object that contains the information for this scene.
		/// </summary>
		protected SceneData sceneData = null;

		#endregion

		#region Scene Controller Functions

		/// <summary>
		/// This function will initialize the scene data.
		/// </summary>
		/// <param name="sceneData">Scene data for this scene controller.</param>
		public void InitializeScene(SceneData sceneData)
		{
			this.sceneData = sceneData;
			this.OnSceneCreate(sceneData.Data);
		}

		/// <summary>
		/// Function is called when the scene is first avaliable to be modified, on the first frame after Unity loads it.
		/// The scene will not be visible.
		/// </summary>
		/// <param name="sceneData">Specific scene data passed in for this scene controller.</param>
		protected abstract void OnSceneCreate(ISceneData sceneData = null);

		/// <summary>
		/// This function will be called when the scene is going to be enabled, before the loading screen is closed.
		/// </summary>
		public virtual IEnumerator OnSceneWillEnable()
		{
			yield break;
		}

		/// <summary>
		/// This function will be called when the scene is visible and the loading screen has closed.
		/// </summary>
		public virtual IEnumerator OnSceneEnabled()
		{
			yield break;
		}

		/// <summary>
		/// This function will be called when the scene is going to be disabled before the loading screen is opened.
		/// </summary>
		public virtual IEnumerator OnSceneWillDisable()
		{
			yield break;
		}

		/// <summary>
		/// This function will be called when the loading screen is up and the scene is no longer visible.
		/// </summary>
		public virtual IEnumerator OnSceneDisabled()
		{
			yield break;
		}

		#endregion

	}

}
