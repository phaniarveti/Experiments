﻿using System;

namespace Umbraco.Core.ObjectResolution
{
	/// <summary>
	/// A Resolver to return and set a Single registered object.
	/// </summary>
	/// <typeparam name="TResolved"></typeparam>
	/// <typeparam name="TResolver"> </typeparam>
	/// <remarks>
	/// Used for 'singly' registered objects. An example is like the MVC Controller Factory, only one exists application wide and it can
	/// be get/set.
	/// </remarks>
	internal abstract class SingleObjectResolverBase<TResolver, TResolved> : ResolverBase<TResolver>
		where TResolved : class
		where TResolver : class
	{
		TResolved _resolved;
		readonly bool _canBeNull;

		protected SingleObjectResolverBase()
			: this(false)
		{ }

		protected SingleObjectResolverBase(TResolved value)
			: this(false)
		{
			_resolved = value;
		}

		protected SingleObjectResolverBase(bool canBeNull)
		{
			_canBeNull = canBeNull;
		}

		protected SingleObjectResolverBase(TResolved value, bool canBeNull)
		{
			_resolved = value;
			_canBeNull = canBeNull;
		}

		/// <summary>
		/// Gets/sets the value of the object
		/// </summary>
		protected TResolved Value
		{
			get
			{
				return _resolved;
			}

			set
			{
				if (!_canBeNull && value == null)
					throw new ArgumentNullException("value");
				_resolved = value;
			}
		}
	}
}
