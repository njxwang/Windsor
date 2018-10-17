// Copyright 2004-2018 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.MicroKernel
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

	using Castle.Core;
	using Castle.Windsor;

	/// <summary>
	/// Represents a collection of named and typed arguments used for dependencies resolved via <see cref="IWindsorContainer.Resolve{T}(Castle.MicroKernel.Arguments)"/>
	/// Please see: https://github.com/castleproject/Windsor/blob/master/docs/arguments.md
	/// </summary>
	public class Arguments : IDictionary
	{
		private static readonly ArgumentsComparer Comparer = new ArgumentsComparer();

		private readonly IDictionary dictionary;
		private bool isReadOnly;

		/// <summary>
		/// Constructor for creating named/typed dependency arguments for <see cref="IWindsorContainer.Resolve{T}(Castle.MicroKernel.Arguments)"/>
		/// </summary>
		public Arguments()
		{
			dictionary = new Dictionary<object, object>(Comparer);
		}

		/// <summary>
		/// Constructor for creating named dependency arguments for <see cref="IWindsorContainer.Resolve{T}(Castle.MicroKernel.Arguments)"/>
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public Arguments(string name, object value) : this()
		{
			dictionary[name] = value;
		}

		/// <summary>
		/// Constructor for creating typed dependency arguments for <see cref="IWindsorContainer.Resolve{T}(Castle.MicroKernel.Arguments)"/>
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		public Arguments(Type type, object value) : this()
		{
			dictionary[type] = value;
		}

		/// <summary>
		/// Constructor for creating named/typed dependency arguments for <see cref="IWindsorContainer.Resolve{T}(Castle.MicroKernel.Arguments)"/>
		/// </summary>
		public Arguments(IDictionary values) : this()
		{
			foreach (DictionaryEntry entry in values)
			{
				Add(entry.Key, entry.Value);
			}
		}

		/// <summary>
		/// Indexer for creating named/typed dependency arguments for <see cref="IWindsorContainer.Resolve{T}(Castle.MicroKernel.Arguments)"/>
		/// </summary>
		public object this[object key]
		{
			get => dictionary[key];
			set
			{
				EnsureWritable();
				dictionary[key] = value;
			}
		}

		public int Count => dictionary.Count;

		public ICollection Keys => dictionary.Keys;

		public ICollection Values => dictionary.Values;

		bool ICollection.IsSynchronized => dictionary.IsSynchronized;

		object ICollection.SyncRoot => dictionary.SyncRoot;

		bool IDictionary.IsFixedSize => dictionary.IsFixedSize;

		bool IDictionary.IsReadOnly => isReadOnly;

		public static readonly Arguments Empty = new Arguments { isReadOnly = true };

		public void Add(object key, object value)
		{
			EnsureWritable();
			dictionary.Add(key, value);
		}

		public void Clear()
		{
			EnsureWritable();
			dictionary.Clear();
		}

		public Arguments Clone()
		{
			return new Arguments(dictionary);
		}

		void ICollection.CopyTo(Array array, int index)
		{
			dictionary.CopyTo(array, index);
		}

		public bool Contains(object key)
		{
			return dictionary.Contains(key);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IDictionaryEnumerator GetEnumerator()
		{
			return dictionary.GetEnumerator();
		}

		/// <summary>
		/// Inserts a named argument. If the argument already exists it will be overwritten.
		/// </summary>
		public Arguments Insert(string key, object value)
		{
			this[key] = value;
			return this;
		}

		/// <summary>
		/// Inserts a set of named and/or typed arguments. If an argument already exists it will be overwritten.
		/// </summary>
		public Arguments Insert(IDictionary values)
		{
			foreach (DictionaryEntry entry in values)
			{
				if (entry.Key is string || entry.Key is Type)
				{
					this[entry.Key] = entry.Value;
				}
				else
				{
					throw new ArgumentException($"The argument '{entry.Key}' should be of type string or System.Type.");
				}
			}
			return this;
		}

		/// <summary>
		/// Inserts a set of named arguments, <see cref="Dictionary{TKey,TValue}"/> implements <see cref="IReadOnlyDictionary{TKey,TValue}"/>.
		/// </summary>
		public Arguments InsertNamed(IReadOnlyDictionary<string, object> values)
		{
			foreach (var entry in values)
			{
				this[entry.Key] = entry.Value;
			}
			return this;
		}

		/// <summary>
		/// Inserts a set of named arguments, this supports plain types and anonymous types.
		/// </summary>
		public Arguments InsertProperties(object instance)
		{
			foreach (DictionaryEntry item in new ReflectionBasedDictionaryAdapter(instance))
			{
				this[item.Key] = item.Value;
			}

			return this;
		}

		/// <summary>
		/// Inserts a new typed argument with given instance. If an argument for this type already exists it will be overwritten.
		/// </summary>
		public Arguments InsertTyped(Type key, object value)
		{
			this[key] = value;
			return this;
		}

		/// <summary>
		/// Inserts a new typed argument with given type. If an argument for this type already exists it will be overwritten.
		/// </summary>
		public Arguments InsertTyped<TDependencyType>(TDependencyType value)
		{
			InsertTyped(typeof(TDependencyType), value);
			return this;
		}

		/// <summary>
		/// Inserts many new typed arguments from a params array. If an argument for this type already exists it will be overwritten.
		/// </summary>
		public Arguments InsertTyped(params object[] values)
		{
			foreach (var value in values)
			{
				InsertTyped(value.GetType(), value);
			}
			return this;
		}

		public void Remove(object key)
		{
			EnsureWritable();
			dictionary.Remove(key);
		}

		private void EnsureWritable()
		{
			if (isReadOnly)
			{
				throw new NotSupportedException("Collection is read-only");
			}
		}

		private sealed class ArgumentsComparer : IEqualityComparer<object>
		{
			public new bool Equals(object x, object y)
			{
				if (x is string a)
				{
					return StringComparer.OrdinalIgnoreCase.Equals(a, y as string);
				}
				return object.Equals(x, y);
			}

			public int GetHashCode(object obj)
			{
				if (obj is string str)
				{
					return StringComparer.OrdinalIgnoreCase.GetHashCode(str);
				}
				return obj.GetHashCode();
			}
		}
	}
}