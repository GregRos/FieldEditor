using System;
using System.Collections.Generic;
using System.Linq;

namespace FieldEditor
{
	public static class Ext
	{
		public static void AddMany<T>(this ICollection<T> self, IEnumerable<T> items)
		{
			items.ForEach(self.Add);
		}

		public static bool ContainsCapsInsensitive(this string self, string substring)
		{
			if (self == null || substring == null)
				return self == substring;
			self = self.ToLower();
			substring = substring.ToLower();
			return self.Contains(substring);
		}

		public static int CountOccurrences<T>(this IEnumerable<T> seq, T target, IEqualityComparer<T> eq)
		{
			return seq.Count(x => eq.Equals(x, target));
		}

		public static IEnumerable<T> Distinct<T, TEq>(this IEnumerable<T> seq, Func<T, TEq> keySelector)
		{
			return from item in seq
			       group item by keySelector(item)
			       into duplicates
			       select duplicates.First();
		}

		public static IEnumerable<T> Duplicates<T>(this IEnumerable<T> seq, IEqualityComparer<T> eq = null)
		{
			eq = eq ?? EqualityComparer<T>.Default;
			var appearOnce = new HashSet<T>(eq);
			var appearTwice = new HashSet<T>(eq);
			foreach (var item in seq)
			{
				if (appearOnce.Contains(item))
					appearTwice.Add(item);
				else
					appearOnce.Add(item);
			}
			return appearTwice;
		}

		public static bool EqAny<T>(this T item, params T[] items)
		{
			return items.Contains(item);
		}

		/// <summary>
		/// Calls the specified delegate on each item in the sequence.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="seq"></param>
		/// <param name="act"></param>
		public static void ForEach<T>(this IEnumerable<T> seq, Action<T> act)
		{
			foreach (var item in seq) act(item);
		}

		/// <summary>
		/// Formats an object value using the preferred format of the specified Kind.
		/// </summary>
		/// <param name="self"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string Format(this Kind self, object value)
		{
			if (value == null) return string.Empty;
			var s = value.ToString();
			if (self.EqAny(Kind.Single, Kind.Double))
			{
				var typed = double.Parse(s);
				return typed % 1 == 0 ? "{0:0.0}".FormatWith(typed) : typed.ToString();
			}
			return value.ToString();
		}

		/// <summary>
		/// Calls the string.Format method.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static string FormatWith(this string s, params object[] args)
		{
			return string.Format(s, args);
		}
		/// <summary>
		/// Returns true if the value has at least one of the specified flags set.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="items"></param>
		/// <returns></returns>
		public static bool HasAnyFlag(this Enum item, params Enum[] items)
		{
			return items.Any(item.HasFlag);
		}
		/// <summary>
		/// Returns true if the specified object corresponds to a legal value for the Kind.
		/// </summary>
		/// <param name="kind"></param>
		/// <param name="o"></param>
		/// <returns></returns>
		public static bool IsLegalValue(this Kind kind, object o)
		{
			if (o == null) return false;
			var s = o.ToString();
			switch (kind)
			{
				case Kind.Double:
					double a;
					return double.TryParse(s, out a);
				case Kind.Single:
					float b;
					return float.TryParse(s, out b);
				case Kind.Int64:
					long c;
					return long.TryParse(s, out c);
				case Kind.Int32:
					int d;
					return int.TryParse(s, out d);
				case Kind.Int16:
					short e;
					return short.TryParse(s, out e);
				case Kind.Byte:
					byte f;
					return byte.TryParse(s, out f);
				default:
					throw new Exception("Invalid type code	");
			}
		}
		/// <summary>
		/// Returns true if the string is null or empty.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static bool IsNullOrEmpty(this string s)
		{
			return string.IsNullOrEmpty(s);
		}
		/// <summary>
		/// Returns true if the string is null, empty, or white space.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static bool IsNullOrWhiteSpace(this string s)
		{
			return string.IsNullOrWhiteSpace(s);
		}
		/// <summary>
		/// Returns the preferred value representation of the specified object, or throws an exception if the object is invalid.
		/// </summary>
		/// <param name="kind"></param>
		/// <param name="o"></param>
		/// <returns></returns>
		public static object Parse(this Kind kind, object o)
		{
			var s = o == null ? "" : o.ToString();
			switch (kind)
			{
				case Kind.Double:

					return double.Parse(s);
				case Kind.Single:
					return float.Parse(s);
				case Kind.Int64:

					return long.Parse(s);
				case Kind.Int32:

					return int.Parse(s);
				case Kind.Int16:

					return short.Parse(s);
				case Kind.Byte:

					return byte.Parse(s);
				default:
					throw new Exception("Invalid type code	");
			}
		}
		/// <summary>
		/// Removes multiple elements from a collection. 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="col"></param>
		/// <param name="items"></param>
		public static void RemoveMany<T>(this ICollection<T> col, IEnumerable<T> items)
		{
			foreach (var item in items.ToArray())
			{
				col.Remove(item);
			}
		}
		/// <summary>
		/// Removes the elements that match a specified predicate
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="col"></param>
		/// <param name="pred"></param>
		public static void RemoveWhere<T>(this ICollection<T> col, Func<T, bool> pred)
		{
			foreach (var item in col.ToArray())
			{
				if (pred(item)) col.Remove(item);
			}
		}
		/// <summary>
		/// Returns the number of bytes used to store fields of the given Kind.
		/// </summary>
		/// <param name="k"></param>
		/// <returns></returns>
		public static int Width(this Kind k)
		{
			switch (k)
			{
				case Kind.Double:
					return 8;
				case Kind.Single:
					return 4;
				case Kind.Byte:
					return 1;
				case Kind.Int16:
					return 2;
				case Kind.Int32:
					return 4;
				case Kind.Int64:
					return 8;
				default:
					throw new Exception("Invalid type code.");
			}
		}
	}
}