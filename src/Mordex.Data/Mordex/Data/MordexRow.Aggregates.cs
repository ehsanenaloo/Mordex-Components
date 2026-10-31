using System;
using System.Collections.Generic;
using System.Threading;

namespace Mordex.Data
{
	partial class MordexRow
	{
		private static long _structureStamp;

		private AggregateCache _aggregates;

		// For tests and the benchmark: every total is computed over all rows, as before the cache.
		internal static bool DisableAggregateCache { get; set; }

		// Counts every change to the set of rows under any row (rows added, removed, shown or hidden). Cached totals are only
		// valid for the stamp they were made at.
		internal static long StructureStamp => Volatile.Read(ref _structureStamp);

		internal static void NoteStructureChanged()
		{
			Interlocked.Increment(ref _structureStamp);
		}

		/// <summary>Tells the totals of the rows above this data row that a value of the row changed outside of a data change notice.</summary>
		public void NoteValueChanged()
		{
			for (MordexRow row = ParentRow; row != null; row = row.ParentRow)
			{
				row._aggregates?.NoteChanged(this);
			}
		}

		private bool CanCacheAggregate(MordexFieldBase field, AggregateFunction function, bool includeAllRows)
		{
			// The rows below a data row are only collected for a table that is its own parent; the flag changes nothing for other tables.
			if (DisableAggregateCache || field == null || function == AggregateFunction.None)
			{
				return false;
			}
			return !((MordexTableBase)((IMordexField)field).Table).IsSelfReferencingTable();
		}

		private bool TryGetCachedAggregate(MordexFieldBase field, AggregateFunction function, bool includeAllRows, long stamp, out object result)
		{
			result = null;
			return _aggregates != null && _aggregates.TryGet(this, field, function, includeAllRows, stamp, out result);
		}

		private object ComputeAndRemember(MordexFieldBase field, AggregateFunction function, bool includeAllRows, long stamp, MordexRow[] rows)
		{
			object result = ComputeAggregate(field, function, rows);
			(_aggregates ??= new AggregateCache()).Store(this, field, function, includeAllRows, stamp, rows, result);
			return result;
		}

		// Sums, counts and extremes of one field over the data rows under an owner row, kept up to date from the rows that
		// changed. The result is the one ComputeAggregate gives over all rows.
		private sealed class AggregateCache
		{
			// The parts of the kept state beyond the rows themselves. A part is kept only once a total that needs it was asked
			// for, and each part is read, converted or compared only for a total that needs it: a count reads no value, a value
			// count reads values without converting or comparing them, a sum converts them, and a minimum or maximum compares
			// them. So a total never calls a value provider, a conversion or a comparison that the total over all rows does not
			// call, and cannot fail where that total does not.
			[Flags]
			private enum Kept
			{
				None = 0,
				Values = 1,
				Sum = 2,
				Extremes = 4
			}

			// Each group of fields is kept apart. A row listed in a stale set has fields that are older than its value.
			private struct Part
			{
				internal object Value;
				internal bool NonNull;
				internal bool Numeric;
				internal decimal Number;
				// The value that the extremes count for the row.
				internal object ExtValue;
				internal bool ExtNonNull;
			}

			private sealed class State
			{
				internal MordexFieldBase Field;
				internal MordexTableBase Table;
				internal bool IncludeAllRows;
				internal long Stamp;
				private int _compareVersion;
				private System.Globalization.CultureInfo _culture;
				internal readonly Dictionary<AggregateFunction, object> Results = new Dictionary<AggregateFunction, object>();
				internal readonly HashSet<MordexRow> Dirty = new HashSet<MordexRow>(ReferenceEqualityComparer.Instance);
				internal bool SawDirty;
				internal Dictionary<MordexRow, Part> Members;
				// The value of the row changed and was not read again.
				internal readonly HashSet<MordexRow> StaleValue = new HashSet<MordexRow>(ReferenceEqualityComparer.Instance);
				// The number kept for the row is not the one of its value.
				internal readonly HashSet<MordexRow> StaleNumber = new HashSet<MordexRow>(ReferenceEqualityComparer.Instance);
				// The value kept for the extremes is not the value of the row.
				internal readonly HashSet<MordexRow> StaleExtreme = new HashSet<MordexRow>(ReferenceEqualityComparer.Instance);
				internal Kept Kept;
				internal decimal Sum;
				// The sum of the absolute values of the kept numbers.
				internal decimal Magnitude;
				internal int Numeric;
				internal int NonNull;
				internal int ExtCount;
				internal object Min;
				internal object Max;
				internal bool MinStale;
				internal bool MaxStale;

				// The kept totals hold for one setting of the comparison of the field and for one culture.
				internal bool Compares(MordexFieldBase field)
				{
					return _compareVersion == field.CompareVersion && Equals(_culture, field.CompareCulture);
				}

				internal void NoteCompare(MordexFieldBase field)
				{
					_compareVersion = field.CompareVersion;
					_culture = field.CompareCulture;
				}
			}

			private readonly List<State> _states = new List<State>();

			private readonly object _lock = new object();

			private State Find(MordexFieldBase field, bool includeAllRows)
			{
				foreach (State state in _states)
				{
					if (state.Field == field && state.IncludeAllRows == includeAllRows)
					{
						return state;
					}
				}
				return null;
			}

			internal void NoteChanged(MordexRow row)
			{
				if (!(row is MordexDataRow))
				{
					return;
				}
				lock (_lock)
				{
					foreach (State state in _states)
					{
						if (row.Table == state.Table)
						{
							state.Dirty.Add(row);
						}
					}
				}
			}

			internal bool TryGet(MordexRow owner, MordexFieldBase field, AggregateFunction function, bool includeAllRows, long stamp, out object result)
			{
				result = null;
				lock (_lock)
				{
					State state = Find(field, includeAllRows);
					if (state == null || state.Stamp != stamp || !state.Compares(field))
					{
						return false;
					}
					if (state.Dirty.Count > 0)
					{
						state.SawDirty = true;
						if (state.Members == null)
						{
							return false;
						}
						ApplyMembership(owner, state);
					}
					if (state.Results.TryGetValue(function, out result))
					{
						return true;
					}
					return state.Members != null && Refresh(state, Needs(function)) && Derive(state, function, out result) && Remember(state, function, result);
				}
			}

			private static bool Remember(State state, AggregateFunction function, object result)
			{
				state.Results[function] = result;
				return true;
			}

			internal void Store(MordexRow owner, MordexFieldBase field, AggregateFunction function, bool includeAllRows, long stamp, MordexRow[] rows, object result)
			{
				lock (_lock)
				{
					State state = Find(field, includeAllRows);
					if (state == null)
					{
						state = new State { Field = field, Table = (MordexTableBase)((IMordexField)field).Table, IncludeAllRows = includeAllRows };
						_states.Add(state);
					}
					bool sameData = state.Stamp == stamp && state.Compares(field);
					bool edited = sameData && (state.SawDirty || state.Dirty.Count > 0);
					if (!sameData || edited)
					{
						state.Results.Clear();
					}
					if (!sameData)
					{
						Drop(state);
						state.SawDirty = false;
					}
					state.Stamp = stamp;
					state.NoteCompare(field);
					state.Dirty.Clear();
					state.Results[function] = result;
					if (!IsDerivable(function))
					{
						return;
					}
					Kept needed = Needs(function);
					if (state.Members != null)
					{
						// Kept up to date already; a part it did not keep yet is read from all rows, and an extreme is found again.
						Kept missing = needed & ~state.Kept;
						bool extremesLost = (function == AggregateFunction.Max && state.MaxStale) || (function == AggregateFunction.Min && state.MinStale);
						if (missing != Kept.None || extremesLost)
						{
							Rebuild(state, rows, missing, extremesLost);
							Refresh(state, needed);
						}
					}
					else if (edited)
					{
						// Rows are being edited in this table: keep the contribution of each row, so that the next change costs
						// the rows that changed and not all rows.
						state.SawDirty = true;
						Build(state, rows, needed);
						Refresh(state, needed);
					}
				}
			}

			private static bool IsDerivable(AggregateFunction function)
			{
				return function == AggregateFunction.Count || function == AggregateFunction.Sum || function == AggregateFunction.Average
					|| function == AggregateFunction.ValueCount || function == AggregateFunction.Max || function == AggregateFunction.Min;
			}

			private static Kept Needs(AggregateFunction function)
			{
				switch (function)
				{
				case AggregateFunction.Sum:
				case AggregateFunction.Average:
					return Kept.Values | Kept.Sum;
				case AggregateFunction.Max:
				case AggregateFunction.Min:
					return Kept.Values | Kept.Extremes;
				case AggregateFunction.ValueCount:
					return Kept.Values;
				default:
					return Kept.None;
				}
			}

			// Forgets the kept rows, so that the next total is computed over all rows and kept again from there.
			private static void Drop(State state)
			{
				state.Members = null;
				state.Kept = Kept.None;
				state.StaleValue.Clear();
				state.StaleNumber.Clear();
				state.StaleExtreme.Clear();
				state.Dirty.Clear();
				state.Results.Clear();
				ResetValues(state);
				ResetNumbers(state);
				ResetExtremes(state);
			}

			private static void ResetValues(State state)
			{
				state.NonNull = 0;
			}

			private static void ResetNumbers(State state)
			{
				state.Sum = 0m;
				state.Magnitude = 0m;
				state.Numeric = 0;
			}

			private static void ResetExtremes(State state)
			{
				state.ExtCount = 0;
				state.Min = null;
				state.Max = null;
				state.MinStale = false;
				state.MaxStale = false;
			}

			// Starts the kept state with the given parts; no row is read yet, so every kept part of every row is stale.
			private static void Build(State state, MordexRow[] rows, Kept kept)
			{
				Drop(state);
				state.Members = new Dictionary<MordexRow, Part>(rows.Length, ReferenceEqualityComparer.Instance);
				foreach (MordexRow row in rows)
				{
					state.Members[row] = default;
				}
				AddParts(state, kept);
			}

			// Keeps more parts over the rows it has, or finds the extremes again. The rows are the ones the total was computed over.
			private static void Rebuild(State state, MordexRow[] rows, Kept missing, bool extremesLost)
			{
				Dictionary<MordexRow, Part> members = state.Members;
				foreach (MordexRow row in rows)
				{
					if (!members.ContainsKey(row))
					{
						members[row] = default;
						if ((state.Kept & Kept.Values) != 0)
						{
							state.StaleValue.Add(row);
						}
					}
				}
				if (members.Count != rows.Length)
				{
					HashSet<MordexRow> current = new HashSet<MordexRow>(rows, ReferenceEqualityComparer.Instance);
					List<MordexRow> gone = new List<MordexRow>();
					foreach (MordexRow row in members.Keys)
					{
						if (!current.Contains(row))
						{
							gone.Add(row);
						}
					}
					try
					{
						foreach (MordexRow row in gone)
						{
							Remove(state, row);
						}
					}
					catch
					{
						Drop(state);
						throw;
					}
				}
				if (extremesLost)
				{
					state.Kept &= ~Kept.Extremes;
					missing |= Kept.Extremes;
				}
				AddParts(state, missing);
			}

			// Adds a part to the kept state: its fields start empty and every row is stale for it.
			private static void AddParts(State state, Kept parts)
			{
				List<MordexRow> keys = new List<MordexRow>(state.Members.Keys);
				if ((parts & Kept.Values) != 0)
				{
					ResetValues(state);
					foreach (MordexRow row in keys)
					{
						Part part = state.Members[row];
						part.Value = null;
						part.NonNull = false;
						state.Members[row] = part;
					}
					state.StaleValue.UnionWith(keys);
				}
				if ((parts & Kept.Sum) != 0)
				{
					ResetNumbers(state);
					foreach (MordexRow row in keys)
					{
						Part part = state.Members[row];
						part.Numeric = false;
						part.Number = 0m;
						state.Members[row] = part;
					}
					state.StaleNumber.UnionWith(keys);
				}
				if ((parts & Kept.Extremes) != 0)
				{
					ResetExtremes(state);
					foreach (MordexRow row in keys)
					{
						Part part = state.Members[row];
						part.ExtValue = null;
						part.ExtNonNull = false;
						state.Members[row] = part;
					}
					state.StaleExtreme.UnionWith(keys);
				}
				state.Kept |= parts;
			}

			private static bool Same(object a, object b)
			{
				return a != null && b != null && a.GetType() == b.GetType() && a.Equals(b);
			}

			// Counts a value of the extremes in. Which of two equal values comes first depends on the row order, so then the
			// extreme is found again when it is asked for.
			private static void ExtAdd(State state, object value)
			{
				state.ExtCount++;
				if (!state.MaxStale)
				{
					if (state.Max == null)
					{
						state.Max = value;
					}
					else
					{
						int order = state.Field.CompareValues(state.Max, value);
						if (order < 0)
						{
							state.Max = value;
						}
						else if (order == 0 && !Same(state.Max, value))
						{
							state.MaxStale = true;
						}
					}
				}
				if (!state.MinStale)
				{
					if (state.Min == null)
					{
						state.Min = value;
					}
					else
					{
						int order = state.Field.CompareValues(state.Min, value);
						if (order > 0)
						{
							state.Min = value;
						}
						else if (order == 0 && !Same(state.Min, value))
						{
							state.MinStale = true;
						}
					}
				}
			}

			private static void ExtRemove(State state, object value)
			{
				state.ExtCount--;
				if (state.ExtCount == 0)
				{
					state.Min = null;
					state.Max = null;
					state.MinStale = false;
					state.MaxStale = false;
					return;
				}
				// Only a value equal to the extreme can change it; then it is found again when the result is asked for.
				if (!state.MaxStale && state.Max != null && state.Field.CompareValues(state.Max, value) == 0)
				{
					state.MaxStale = true;
				}
				if (!state.MinStale && state.Min != null && state.Field.CompareValues(state.Min, value) == 0)
				{
					state.MinStale = true;
				}
			}

			// Takes a row out of every kept part.
			private static void Remove(State state, MordexRow row)
			{
				if (!state.Members.TryGetValue(row, out Part part))
				{
					return;
				}
				state.Members.Remove(row);
				state.StaleValue.Remove(row);
				state.StaleNumber.Remove(row);
				state.StaleExtreme.Remove(row);
				if (part.NonNull)
				{
					state.NonNull--;
				}
				if (part.Numeric && (state.Kept & Kept.Sum) != 0)
				{
					state.Numeric--;
					state.Sum -= part.Number;
				}
				if (part.ExtNonNull && (state.Kept & Kept.Extremes) != 0)
				{
					// No comparison is made for a row that leaves: the extremes are found again when they are asked for.
					state.ExtCount--;
					if (state.ExtCount == 0)
					{
						ResetExtremes(state);
					}
					else
					{
						state.MinStale = true;
						state.MaxStale = true;
					}
				}
			}

			// True when the row is still one of the data rows ComputeAggregate would collect under the owner.
			private static bool IsMember(MordexRow owner, MordexRow row)
			{
				for (MordexRow node = row; node != owner; node = node.ParentRow)
				{
					if (node == null || !node.Visible)
					{
						return false;
					}
				}
				return true;
			}

			// Brings the membership of the changed rows up to date. No value is read: a row that stays keeps its parts, which are
			// read again only when a total that needs them is asked for.
			private static void ApplyMembership(MordexRow owner, State state)
			{
				try
				{
					foreach (MordexRow row in state.Dirty)
					{
						if (!IsMember(owner, row))
						{
							Remove(state, row);
							continue;
						}
						if (!state.Members.ContainsKey(row))
						{
							state.Members[row] = default;
						}
						if ((state.Kept & Kept.Values) != 0)
						{
							state.StaleValue.Add(row);
						}
					}
				}
				catch
				{
					Drop(state);
					throw;
				}
				state.Dirty.Clear();
				state.Results.Clear();
			}

			// Reads, converts or compares again the rows whose kept parts the total needs, and only those parts. Returns false when
			// the kept state cannot serve the total.
			private static bool Refresh(State state, Kept needed)
			{
				if (needed == Kept.None || (state.Kept & needed) != needed)
				{
					return needed == Kept.None;
				}
				if (state.StaleValue.Count > 0)
				{
					RefreshValues(state);
				}
				if ((needed & Kept.Sum) != 0 && state.StaleNumber.Count > 0)
				{
					RefreshNumbers(state);
				}
				if ((needed & Kept.Extremes) != 0 && state.StaleExtreme.Count > 0)
				{
					RefreshExtremes(state);
				}
				return true;
			}

			private static void RefreshValues(State state)
			{
				MordexRow[] rows = new MordexRow[state.StaleValue.Count];
				state.StaleValue.CopyTo(rows);
				object[] values = new object[rows.Length];
				for (int i = 0; i < rows.Length; i++)
				{
					values[i] = state.Field.GetRowValue(rows[i]);
				}
				for (int i = 0; i < rows.Length; i++)
				{
					Part part = state.Members[rows[i]];
					if (part.NonNull)
					{
						state.NonNull--;
					}
					part.Value = values[i];
					part.NonNull = !DataValueHelper.IsNullOrDBNull(values[i]);
					if (part.NonNull)
					{
						state.NonNull++;
					}
					state.Members[rows[i]] = part;
					if ((state.Kept & Kept.Sum) != 0)
					{
						state.StaleNumber.Add(rows[i]);
					}
					if ((state.Kept & Kept.Extremes) != 0)
					{
						state.StaleExtreme.Add(rows[i]);
					}
				}
				state.StaleValue.Clear();
			}

			private static void RefreshNumbers(State state)
			{
				MordexRow[] rows = new MordexRow[state.StaleNumber.Count];
				state.StaleNumber.CopyTo(rows);
				Part[] parts = new Part[rows.Length];
				// The numbers are found before the kept state changes, so a failure leaves it as it was.
				for (int i = 0; i < rows.Length; i++)
				{
					Part part = state.Members[rows[i]];
					part.Numeric = false;
					part.Number = 0m;
					if (DataValueHelper.CanConvertToDecimal(part.Value))
					{
						part.Numeric = true;
						part.Number = Convert.ToDecimal(part.Value);
					}
					parts[i] = part;
				}
				decimal sum = state.Sum;
				decimal magnitude = state.Magnitude;
				bool exact = true;
				try
				{
					// The changes are summed first: taking an old number out before the new one is in can leave the decimal
					// range although the new total fits. Every step must be exact, and so must the sum of the absolute values:
					// it bounds every partial sum of the full calculation, which then rounds nowhere and gives the same total in
					// any order.
					decimal change = 0m;
					decimal magnitudeChange = 0m;
					for (int i = 0; i < rows.Length && exact; i++)
					{
						Part old = state.Members[rows[i]];
						if (old.Numeric)
						{
							exact = AddExact(ref change, -old.Number) && AddExact(ref magnitudeChange, -Math.Abs(old.Number));
						}
						if (exact && parts[i].Numeric)
						{
							exact = AddExact(ref change, parts[i].Number) && AddExact(ref magnitudeChange, Math.Abs(parts[i].Number));
						}
					}
					exact = exact && AddExact(ref sum, change) && AddExact(ref magnitude, magnitudeChange);
				}
				catch (OverflowException)
				{
					exact = false;
				}
				if (!exact)
				{
					// The sum is not kept; a sum or average is then computed over all rows each time.
					state.Kept &= ~Kept.Sum;
					state.StaleNumber.Clear();
					ResetNumbers(state);
					return;
				}
				for (int i = 0; i < rows.Length; i++)
				{
					Part old = state.Members[rows[i]];
					if (old.Numeric)
					{
						state.Numeric--;
					}
					if (parts[i].Numeric)
					{
						state.Numeric++;
					}
					state.Members[rows[i]] = parts[i];
				}
				state.Sum = sum;
				state.Magnitude = magnitude;
				state.StaleNumber.Clear();
			}

			// Adds when the decimal sum is exact. A sum that had to drop digits has a smaller scale than its operands; it is not the
			// one of the full calculation.
			private static bool AddExact(ref decimal total, decimal value)
			{
				decimal result = total + value;
				if (ScaleOf(result) < Math.Max(ScaleOf(total), ScaleOf(value)))
				{
					return false;
				}
				total = result;
				return true;
			}

			private static int ScaleOf(decimal value)
			{
				return value.Scale;
			}

			private static void RefreshExtremes(State state)
			{
				MordexRow[] rows = new MordexRow[state.StaleExtreme.Count];
				state.StaleExtreme.CopyTo(rows);
				try
				{
					foreach (MordexRow row in rows)
					{
						Part part = state.Members[row];
						if (part.ExtNonNull)
						{
							ExtRemove(state, part.ExtValue);
							part.ExtNonNull = false;
							part.ExtValue = null;
							state.Members[row] = part;
						}
						if (part.NonNull)
						{
							part.ExtValue = part.Value;
							part.ExtNonNull = true;
							state.Members[row] = part;
							ExtAdd(state, part.Value);
						}
					}
				}
				catch
				{
					Drop(state);
					throw;
				}
				state.StaleExtreme.Clear();
			}

			private static bool Derive(State state, AggregateFunction function, out object result)
			{
				result = null;
				switch (function)
				{
				case AggregateFunction.Count:
					result = state.Members.Count;
					return true;
				case AggregateFunction.Sum:
					result = state.Sum;
					return (state.Kept & Kept.Sum) != 0;
				case AggregateFunction.Average:
					result = state.Numeric > 0 ? (object)((double)state.Sum / (double)state.Numeric) : null;
					return (state.Kept & Kept.Sum) != 0;
				case AggregateFunction.ValueCount:
					result = state.NonNull;
					return (state.Kept & Kept.Values) != 0;
				case AggregateFunction.Max:
					result = state.Max;
					return (state.Kept & Kept.Extremes) != 0 && !state.MaxStale;
				case AggregateFunction.Min:
					result = state.Min;
					return (state.Kept & Kept.Extremes) != 0 && !state.MinStale;
				default:
					return false;
				}
			}
		}
	}
}
