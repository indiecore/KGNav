using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KGTools.General
{

	/// <summary>
	/// This function is a scene manager that controls a navigation stack of scene objects.
	/// </summary>
	[Service("NavigationSceneManager")]
	public abstract class NavigationSceneManager : MonoBehaviour
	{

		#region Constants

		/// <summary>
		/// Magic value used by Unity async functions to signal that they are actually completed and awaiting permission to activate.
		/// </summary>
		private const float AWAITING_ACTIVATION_PERCENTAGE = 0.9f;

		/// <summary>
		/// Scene object error message.
		/// </summary>
		private const string SCENE_ROOT_ERROR_MSG = "A Managed KGTools Scene must have a single root object with the SceneController component attached. {0} does not.";

		#endregion

		#region Delegates

		/// <summary>
		/// Delegate for waiting on async loads.
		/// </summary>
		/// <returns>True when the async load is set to finished, false otherwise.</returns>
		private delegate bool LoadingDelegate();

		#endregion

		#region Data
		
		/// <summary>
		/// The thread priority for loading tasks.
		/// Default to low so that loading screen animations function correctly.
		/// </summary>
		[SerializeField, Header("----- Scene Management -----")]
		private ThreadPriority defaultLoadingThreadPriority = ThreadPriority.Low;

		/// <summary>
		/// The scene stack for this navigation scene manager.
		/// </summary>
		private Stack<SceneData> sceneStack = null;

		/// <summary>
		/// How many scene are current loaded.
		/// </summary>
		public int SceneCount
		{
			get
			{
				int count = 0;
				if (this.sceneStack != null)
				{
					count = sceneStack.Count;
				}

				return count;
			}
		}

		/// <summary>
		/// Current active scene.
		/// </summary>
		public SceneData CurrentScene
		{
			get
			{
				SceneData currentScene = null;
				if (this.sceneStack != null && this.sceneStack.Count > 0)
				{
					currentScene = sceneStack.Peek();
				}
				
				return currentScene;
			}
		}

		/// <summary>
		/// Flag indicating if this navigation scene manager has been initialized.
		/// </summary>
		private bool initialized = false;

		#endregion

		#region MonoBehaviour

		/// <summary>
		/// If this is not initialized manually by the time Awake is called we should auto-initialize it.
		/// </summary>
		private void Awake()
		{
			this.Initalize();
		}

		#endregion

		#region Manager Logic

		/// <summary>
		/// Set this scene manager up.
		/// </summary>
		public void Initalize()
		{
			if (!this.initialized)
			{
				this.InitializeNavigationSceneManager();
			}
		}

		/// <summary>
		/// Handle the actual initialization of the navigation scene manager.
		/// </summary>
		protected virtual void InitializeNavigationSceneManager()
		{
			Application.backgroundLoadingPriority = this.defaultLoadingThreadPriority;
			this.sceneStack = new Stack<SceneData>();
			this.initialized = true;
		}

		#endregion

		#region Scene Management

		/// <summary>
		/// Pushes a new scene onto the scene stack.
		/// </summary>
		/// <param name="sceneID">The unity name of the string to load.</param>
		/// <param name="sceneData">The scene data from the </param>
		/// <param name="loadingScreenData">Data controlling the loading screen that will be used to load into this scene.</param>
		public void PushScene(string sceneID, ISceneData sceneData, LoadingScreenData loadingScreenData)
		{
			this.StartCoroutine(this.PushSceneInternal(sceneID, sceneData, loadingScreenData));
		}

		/// <summary>
		/// Pushes a new scene onto the scene stack.
		/// </summary>
		/// <param name="sceneID">The unity name of the scene to load.</param>
		/// <param name="loadingScreenData">Data controlling the loading screen that will be used to load into this scene.</param>
		public void PushScene(string sceneID, LoadingScreenData loadingScreenData)
		{
			this.PushScene(sceneID, null, loadingScreenData);
		}

		/// <summary>
		/// Pops a scene off the stack and goes back to the previous scene.
		/// </summary>
		/// <param name="loadingScreenData">The data controlling the loading screen that will mask the transition back to the previous scene.</param>
		public void PopScene(LoadingScreenData loadingScreenData)
		{
			this.StartCoroutine(this.PopSceneInternal(loadingScreenData));
		}

		/// <summary>
		/// This function will force push a scene controller and set it up as the current scene manager.
		/// </summary>
		/// <param name="controller">The scene controller to force setup of.</param>
		/// <param name="loadingScreenData">The loading screen data to use to hide the </param>
		public void ForceSetupScene(SceneController controller, LoadingScreenData loadingScreenData)
		{
			this.StartCoroutine(this.ForcePushScene(controller, loadingScreenData));
		}

		#endregion

		#region Internal Scene Management

		/// <summary>
		/// Handles actually loading a scene and 
		/// </summary>
		/// <param name="sceneID">The unity name of the string to load.</param>
		/// <param name="sceneData">The scene data from the </param>
		/// <param name="loadingScreenData">Data controlling the loading screen that will be used to load into this scene.</param>
		private IEnumerator PushSceneInternal(string sceneID, ISceneData providedSceneData, LoadingScreenData loadingScreenData)
		{
			AbstractLoadingScreen selectedLoadingScreen = this.GetLoadingScreen(loadingScreenData.LoadingScreenID);
			selectedLoadingScreen.InitializeLoadingScreen(loadingScreenData.Data);
			
			List<LoadingDelegate> loadList = new List<LoadingDelegate>();

			// If there is a current scene we have to first close out the scene is currently active.
			if (this.CurrentScene != null)
			{
				// We have to first close out the scene is currently active.
				yield return this.StartCoroutine(this.CurrentScene.SceneRoot.OnSceneWillDisable());
			}

			// Now open the loading screen.
			bool loadOpen = false;
			selectedLoadingScreen.Open(() => {
				loadOpen = true;
			});

			// Wait until the loading screen is open.
			yield return new WaitUntil(() => loadOpen);

			// Now that the loading screen is open we can unload the current scene if there is one.
			if (this.CurrentScene != null)
			{
				yield return this.StartCoroutine(this.CurrentScene.SceneRoot.OnSceneDisabled());
			}

			// At the same time we want to load the new scene.
			AsyncOperation loadNewScene = SceneManager.LoadSceneAsync(sceneID, LoadSceneMode.Additive);
			loadList.Add(() => loadNewScene.progress >= AWAITING_ACTIVATION_PERCENTAGE);

			loadNewScene.allowSceneActivation = false;
			WaitForEndOfFrame eof = new WaitForEndOfFrame();

			while (!loadNewScene.isDone)
			{
				if (loadNewScene.progress < AWAITING_ACTIVATION_PERCENTAGE)
				{
					selectedLoadingScreen.SetLoadPercentage(loadNewScene.progress);
				}
				else
				{
					loadNewScene.allowSceneActivation = true;
				}

				yield return eof;
			}

			if (this.CurrentScene != null)
			{
				if (!this.CurrentScene.cacheScene)
				{
					AsyncOperation oldSceneUnload = SceneManager.UnloadSceneAsync(this.CurrentScene.SceneID);
					yield return new WaitUntil(() => oldSceneUnload.isDone);
				}
				else
				{
					// If the old scene is set to be cached we should just disable the root object.
					this.CurrentScene.SceneRoot.gameObject.SetActive(false);
				}
			}

			Scene newScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneID);
			// If this is the very first scene that has been loaded we should remove the scene that is active by default.
			if ((this.sceneStack.Count == 0))
			{
				UnityEngine.SceneManagement.Scene initializationScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
				AsyncOperation unloadCurrent = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(initializationScene);
				yield return new WaitUntil(() => unloadCurrent.isDone);
			}

			// Cleanup resources.
			AsyncOperation unloadResource = Resources.UnloadUnusedAssets();
			yield return new WaitUntil(() => unloadResource.isDone);


			UnityEngine.SceneManagement.SceneManager.SetActiveScene(newScene);

			GameObject[] newSceneRoots = newScene.GetRootGameObjects();
			Debug.AssertFormat(newSceneRoots.Length == 1, SCENE_ROOT_ERROR_MSG, newSceneRoots.First().name);
			
			SceneController rootObject = newSceneRoots.First().GetComponent<SceneController>();
			Debug.AssertFormat(rootObject != null, SCENE_ROOT_ERROR_MSG, newSceneRoots.First().name);

			// Now that the new scene is set up we need to create a SceneData entry for it.
			SceneData newSceneData = new SceneData(sceneID, rootObject, providedSceneData);

			// The new scene is now the current scene.
			this.sceneStack.Push(newSceneData);
			selectedLoadingScreen.SetLoadPercentage(1f);

			// Initialize the scene.
			rootObject.InitializeScene(newSceneData);

			// Run any enabling setup for this scene.
			yield return this.StartCoroutine(this.CurrentScene.SceneRoot.OnSceneWillEnable());

			// Now that the scene is setup for display we should run the loading screen out.
			selectedLoadingScreen.Close(() => {
				loadOpen = false;
			});

			yield return new WaitWhile(() => loadOpen);
			// The loading screen is closed so we should play the scene activation.
			yield return this.StartCoroutine(this.CurrentScene.SceneRoot.OnSceneEnabled());
		}

		/// <summary>
		/// This pops the current scene off the stack, unloads it and then loads the previous scene on the stack.
		/// </summary>
		/// <param name="loadingScreenData">The data for selecting the loading screen.</param>
		private IEnumerator PopSceneInternal(LoadingScreenData loadingScreenData)
		{
			AbstractLoadingScreen selectedLoadingScreen = this.GetLoadingScreen(loadingScreenData.LoadingScreenID);
			selectedLoadingScreen.InitializeLoadingScreen(loadingScreenData.Data);

			if (this.sceneStack.Count <= 1)
			{
				Debug.LogError("Cannot unload last scene on stack.");
				yield break;
			}

			SceneData oldScene = this.sceneStack.Pop();
			SceneData newScene = this.sceneStack.Peek();

			// First run the notifications for the current scene.
			yield return this.StartCoroutine(oldScene.SceneRoot.OnSceneWillDisable());

			bool loadOpen = false;
			selectedLoadingScreen.Open(() => {
				loadOpen = true;
			});

			yield return new WaitUntil(() => loadOpen);
			yield return this.StartCoroutine(oldScene.SceneRoot.OnSceneDisabled());

			// If the scene root was unloaded we need to reload it.
			if (newScene.SceneRoot == null)
			{
				AsyncOperation loadNewScene = SceneManager.LoadSceneAsync(newScene.SceneID, LoadSceneMode.Additive);
				loadNewScene.allowSceneActivation = false;

				WaitForEndOfFrame eof = new WaitForEndOfFrame();
				while (!loadNewScene.isDone)
				{
					if (loadNewScene.progress < AWAITING_ACTIVATION_PERCENTAGE)
					{
						selectedLoadingScreen.SetLoadPercentage(loadNewScene.progress);
					}
					else
					{
						loadNewScene.allowSceneActivation = true;
					}

					yield return eof;
				}
			}

			// Now that the new scene is loaded we need to unload the old scene.
			AsyncOperation unloadScene = SceneManager.UnloadSceneAsync(oldScene.SceneID);
			yield return new WaitUntil(() => unloadScene.isDone);

			// Cleanup resources.
			AsyncOperation unloadResource = Resources.UnloadUnusedAssets();
			yield return new WaitUntil(() => unloadResource.isDone);

			Scene newSceneObject = SceneManager.GetSceneByName(newScene.SceneID);
			SceneManager.SetActiveScene(newSceneObject);
			
			GameObject[] newSceneRoots = newSceneObject.GetRootGameObjects();
			Debug.AssertFormat(newSceneRoots.Length == 1, SCENE_ROOT_ERROR_MSG, newSceneRoots.First().name);
			
			SceneController rootObject = newSceneRoots.First().GetComponent<SceneController>();
			Debug.AssertFormat(rootObject != null, SCENE_ROOT_ERROR_MSG, newSceneRoots.First().name);

			newScene.SceneRoot = rootObject;
			selectedLoadingScreen.SetLoadPercentage(1f);

			// Initialize the scene.
			rootObject.InitializeScene(newScene);

			// Run any enabling setup for this scene.
			yield return this.StartCoroutine(this.CurrentScene.SceneRoot.OnSceneWillEnable());

			// Now that the scene is setup for display we should run the loading screen out.
			selectedLoadingScreen.Close(() => {
				loadOpen = false;
			});

			yield return new WaitWhile(() => loadOpen);
			// The loading screen is closed so we can call the scene enabled function.
			yield return this.StartCoroutine(this.CurrentScene.SceneRoot.OnSceneEnabled());
		}

		/// <summary>
		/// This function will force start a scene based on the root object of it.
		/// </summary>
		/// <param name="rootObject">The scene controller object to shove onto the stack.</param>
		/// <param name="loadingScreenData">The loading screen data for the loading screen to use.</param>
		private IEnumerator ForcePushScene(SceneController rootObject, LoadingScreenData loadingScreenData)
		{
			AbstractLoadingScreen selectedLoadingScreen = this.GetLoadingScreen(loadingScreenData.LoadingScreenID);
			selectedLoadingScreen.InitializeLoadingScreen(loadingScreenData.Data);
			selectedLoadingScreen.ForceOpen();

			bool loadOpen = true;

			// Now that the new scene is set up we need to create a SceneData entry for it.
			SceneData newSceneData = new SceneData(rootObject.name, rootObject, null);

			// The new scene is now the current scene.
			this.sceneStack.Push(newSceneData);
			selectedLoadingScreen.SetLoadPercentage(1f);

			// Initialize the scene.
			rootObject.InitializeScene(newSceneData);

			// Run any enabling setup for this scene.
			yield return this.StartCoroutine(this.CurrentScene.SceneRoot.OnSceneWillEnable());

			// Now that the scene is setup for display we should run the loading screen out.
			selectedLoadingScreen.Close(() => {
				loadOpen = false;
			});

			yield return new WaitWhile(() => loadOpen);
			// The loading screen is closed so we should play the scene activation.
			yield return this.StartCoroutine(this.CurrentScene.SceneRoot.OnSceneEnabled());
		}

		#endregion

		#region Loading Screen Access

		/// <summary>
		/// Function to accept general loading screen access.
		/// Gets a loading screen via a loading screen ID.
		/// </summary>
		/// <param name="loadingScreenID">The ID of the loading screen to access.</param>
		/// <returns>The selected loading screen or null if that screen cannot be found.</returns>
		protected abstract AbstractLoadingScreen GetLoadingScreen(string loadingScreenID);

		#endregion

	}
}
