using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KGTools.General
{
	/// <summary>
	/// Extremely basic implementation of a loading screen.
	/// </summary>
	public class BasicLoadingScreen : AbstractLoadingScreen
	{
		/// <summary>
		/// Simply disable this loading screen object.
		/// </summary>
		protected override void CloseImmediate()
		{
			this.gameObject.SetActive(false);
		}

		/// <summary>
		/// Simply disable this loading screen object.
		/// </summary>
		protected override IEnumerator CloseLoadingScreen()
		{
			this.gameObject.SetActive(false);
			yield break;
		}

		/// <summary>
		/// Simply enable this loading screen object.
		/// </summary>
		protected override void OpenImmediate()
		{
			this.gameObject.SetActive(true);
		}

		/// <summary>
		/// Simply enable this loading screen object.
		/// </summary>
		protected override IEnumerator OpenLoadingScreen()
		{
			this.gameObject.SetActive(true);
			yield break;
		}
	}
}
