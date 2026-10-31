using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Reflection;
using System.Reflection.Metadata;
using System.Windows.Forms;

namespace Mordex.Windows.Common.Layouts
{
	/// <summary>
	/// Decides which types a type name read from a layout may stand for. Every control uses this policy.
	/// Allowed are simple data types (numbers, text, dates, Guid, colors, sizes, points, rectangles, padding),
	/// one-dimensional arrays and nullable forms of them, public enums from assemblies that are already loaded,
	/// and types the host registers. A layout name never loads an assembly and never selects any other type.
	/// </summary>
	public static class LayoutTypePolicy
	{
		private static readonly object s_lock = new object();

		private static readonly Dictionary<string, Type> s_types = CreateBuiltInTypes();

		private static Dictionary<string, Type> CreateBuiltInTypes()
		{
			Type[] types =
			{
				typeof(string), typeof(bool), typeof(char), typeof(byte), typeof(sbyte), typeof(short), typeof(ushort),
				typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal),
				typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan), typeof(Guid), typeof(DBNull),
				typeof(Color), typeof(Size), typeof(SizeF), typeof(Point), typeof(PointF), typeof(Rectangle), typeof(RectangleF),
				typeof(Padding)
			};
			Dictionary<string, Type> map = new Dictionary<string, Type>(StringComparer.Ordinal);
			foreach (Type type in types)
			{
				map[type.FullName] = type;
			}
			return map;
		}

		/// <summary>
		/// Lets layouts name a host data type. Register only types whose type converter is safe to run on text
		/// from a layout file. Arrays and <see cref="Nullable{T}"/> of a registered type are allowed without
		/// registering them.
		/// </summary>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="type"/> is generic (open or closed), an array, a pointer or a by-ref type. A layout can name
		/// only <see cref="Nullable{T}"/> as a generic type, so a registered generic type could never be resolved.
		/// </exception>
		public static void Register(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}
			if (type.IsGenericType || type.ContainsGenericParameters || type.IsPointer || type.IsByRef || type.IsArray || type.FullName == null)
			{
				throw new ArgumentException("Only a non-generic, non-array type can be registered.", nameof(type));
			}
			lock (s_lock)
			{
				s_types[type.FullName] = type;
			}
		}

		/// <summary>
		/// Converts text read from a layout to the type that <paramref name="typeName"/> names. Returns the text
		/// unchanged when the name is empty or not allowed by this policy, or when the text is not a value of that type.
		/// </summary>
		public static object ConvertOrKeepText(string text, string typeName)
		{
			Type type = text == null ? null : Resolve(typeName);
			if (type == null)
			{
				return text;
			}
			try
			{
				return PropertyValue.ConvertValue(text, type) ?? text;
			}
			catch (Exception ex) when (PropertyValue.IsConversionFailure(ex))
			{
				return text;
			}
		}

		/// <summary>Returns the type for a name read from a layout, or null when the name is not allowed.</summary>
		public static Type Resolve(string typeName)
		{
			if (string.IsNullOrWhiteSpace(typeName) || !TypeName.TryParse(typeName.AsSpan(), out TypeName parsed))
			{
				return null;
			}
			return Resolve(parsed);
		}

		private static Type Resolve(TypeName name)
		{
			if (name.IsArray)
			{
				Type element = name.IsSZArray ? Resolve(name.GetElementType()) : null;
				return element != null && !element.IsArray ? element.MakeArrayType() : null;
			}
			if (name.IsPointer || name.IsByRef)
			{
				return null;
			}
			if (name.IsConstructedGenericType)
			{
				ImmutableArray<TypeName> arguments = name.GetGenericArguments();
				if (name.GetGenericTypeDefinition().FullName != typeof(Nullable<>).FullName || arguments.Length != 1)
				{
					return null;
				}
				Type argument = Resolve(arguments[0]);
				return argument != null && argument.IsValueType && Nullable.GetUnderlyingType(argument) == null ? typeof(Nullable<>).MakeGenericType(argument) : null;
			}
			lock (s_lock)
			{
				if (s_types.TryGetValue(name.FullName, out Type known))
				{
					return known;
				}
			}
			return FindLoadedPublicEnum(name.FullName, name.AssemblyName?.Name);
		}

		// Looks in the named assembly first. Older layouts name framework assemblies that are only facades now
		// (mscorlib, System.Drawing), so every loaded assembly is tried after that.
		private static Type FindLoadedPublicEnum(string fullName, string assemblyName)
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			if (assemblyName != null)
			{
				foreach (Assembly assembly in assemblies)
				{
					if (string.Equals(assembly.GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase))
					{
						Type named = PublicEnumIn(assembly, fullName);
						if (named != null)
						{
							return named;
						}
					}
				}
			}
			foreach (Assembly assembly in assemblies)
			{
				Type candidate = PublicEnumIn(assembly, fullName);
				if (candidate != null)
				{
					return candidate;
				}
			}
			return null;
		}

		private static Type PublicEnumIn(Assembly assembly, string fullName)
		{
			if (assembly.IsDynamic)
			{
				return null;
			}
			Type candidate = assembly.GetType(fullName, throwOnError: false);
			return candidate != null && candidate.IsEnum && candidate.IsVisible ? candidate : null;
		}
	}
}
