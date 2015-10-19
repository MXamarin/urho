﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Urho
{
	/// <summary>
	/// </summary>
	internal class RefCountedCache
	{
		Dictionary<IntPtr, ReferenceHolder<RefCounted>> knownObjects = new Dictionary<IntPtr, ReferenceHolder<RefCounted>>(256); //based on samples (average)

		public int Count => knownObjects.Count;

		public void Add(RefCounted refCounted)
		{
			lock (knownObjects)
			{
				ReferenceHolder<RefCounted> knownObject;
				if (knownObjects.TryGetValue(refCounted.Handle, out knownObject))
				{
					knownObject?.Reference?.Dispose();
				}

				knownObjects[refCounted.Handle] = new ReferenceHolder<RefCounted>(refCounted, weak: refCounted.Refs() < 1);
			}
		}

		public bool Remove(IntPtr ptr)
		{
			lock (knownObjects)
			{
				return knownObjects.Remove(ptr);
			}
		}

		public ReferenceHolder<RefCounted> Get(IntPtr ptr)
		{
			lock (knownObjects)
			{
				ReferenceHolder<RefCounted> refCounted;
				knownObjects.TryGetValue(ptr, out refCounted);
				return refCounted;
			}
		}

		public void Clean()
		{
			lock (knownObjects)
			{
				foreach (var referenceHolder in knownObjects.ToArray())
				{
					referenceHolder.Value?.Reference?.Dispose();
				}
				knownObjects.Clear();
			}
		}
	}
}
