using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace idseefeld.de.imagecropper
{
	public class PropertyHelper
	{
		/// <summary>
		/// Recursively finds a control with the specified identifier. 
		/// extracted from idseefeld.de.imagecropper.uploadfield
		/// also use in idseefeld.de.imagecropper.imagecropper
		/// </summary>
		/// <typeparam name="T">
		/// The type of control to be found.
		/// </typeparam>
		/// <param name="parent">
		/// The parent control from which the search will start.
		/// </param>
		/// <param name="id">
		/// The identifier of the control to be found.
		/// </param>
		/// <returns>
		/// The control with the specified identifier, otherwise <see langword="null"/> if the control 
		/// is not found.
		/// </returns>
		internal static T FindControlRecursive<T>(Control parent, string id) where T : Control
		{
			if ((parent is T) && (parent.ID == id))
			{
				return (T)parent;
			}

			foreach (Control control in parent.Controls)
			{
				var foundControl = FindControlRecursive<T>(control, id);
				if (foundControl != null)
				{
					return foundControl;
				}
			}
			return default(T);
		}
	}
}
