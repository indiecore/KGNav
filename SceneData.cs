using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KGTools.General
{

	/// <summary>
	/// Interface for tagging scene data as such.
	/// Used to indicate when a data object is actually a scene data object.
	/// </summary>
	public interface ISceneData { }

	/// <summary>
	/// Scene data contains the information for specific unity scenes.
	/// </summary>
	public class SceneData
	{

		#region Data

		private string sceneID = string.Empty;
		/// <summary>
		/// ID for the scene. Will be the same as the actual unity scene name.
		/// </summary>
		public string SceneID
		{
			get
			{
				return this.sceneID;
			}
		}
		
		private ISceneData data = null;
		/// <summary>
		/// Object containing specific data for the scene controller.
		/// </summary>
		public ISceneData Data
		{
			get
			{
				return this.data;
			}
		}

		private SceneController sceneRoot = null;
		/// <summary>
		/// The scene root object that is the root of the scene this data refers to.
		/// </summary>
		public SceneController SceneRoot
		{
			get
			{
				return this.sceneRoot;
			}

			set
			{
				this.sceneRoot = value;
			}
		}

		/// <summary>
		/// If set to true the scene will not be unloaded when it is not the active scene.
		/// </summary>
		public bool cacheScene = false;

		#endregion

		#region Constructor

		/// <summary>
		/// Create a new scene data object.
		/// </summary>
		/// <param name="sceneID">Name of the scene.</param>
		/// <param name="rootObject">The root object controller for the scene.</param>
		/// <param name="sceneRootData">Actually data instance to pass to the scene controller.</param>
		public SceneData(string sceneID, SceneController rootObject, ISceneData sceneRootData = null)
		{
			this.sceneID = sceneID;
			this.sceneRoot = rootObject;
			this.data = sceneRootData;
		}

		#endregion

	}

}
