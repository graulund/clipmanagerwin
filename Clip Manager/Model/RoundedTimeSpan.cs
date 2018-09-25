using System;

/*
 * ==============================================================
 * @ID       $Id: RoundedTimeSpan.cs 971 2010-09-30 16:09:30Z ww $
 * @created  2010-01-04
 * @project  http://cleancode.sourceforge.net/
 * ==============================================================
 *
 * The official license for this file is shown next.
 * Unofficially, consider this e-postcardware as well:
 * if you find this module useful, let us know via e-mail, along with
 * where in the world you are and (if applicable) your website address.
 */

/* ***** BEGIN LICENSE BLOCK *****
 * Version: MPL 1.1
 *
 * The contents of this file are subject to the Mozilla Public License Version
 * 1.1 (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 * http://www.mozilla.org/MPL/
 *
 * Software distributed under the License is distributed on an "AS IS" basis,
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
 * for the specific language governing rights and limitations under the
 * License.
 *
 * The Original Code is part of the CleanCode toolbox.
 *
 * The Initial Developer of the Original Code is Michael Sorens.
 * Portions created by the Initial Developer are Copyright (C) 2010-2010
 * the Initial Developer. All Rights Reserved.
 *
 * Contributor(s):
 *
 * ***** END LICENSE BLOCK *****
 */
namespace Clip_Manager.Model
{
	/// <summary>
	/// Use this struct to generate rounded <see cref="TimeSpan"/> values.
	/// </summary>
	/// <remarks>
	/// The standard TimeSpan struct renders like this:
	/// <code>
	/// Console.WriteLine(new TimeSpan(19365678)) => 00:00:01.9365678
	/// </code>
	/// A <c>RoundedTimeSpan</c> struct, on the other hand, lets you adjust the precision:
	/// <code>
	/// Console.WriteLine(new RoundedTimeSpan(19365678, 5)) => 00:00:01.93657
	/// Console.WriteLine(new RoundedTimeSpan(19365678, 3)) => 00:00:01.937
	/// </code>
	/// Those values implicitly come from the default <see cref="ToString()"/> method.
	/// There is an additional convenience method to generate a string with displayed digits different than the precision:
	/// <code>
	/// Console.WriteLine(new RoundedTimeSpan(19365678, 3).ToString(5)) => 00:00:01.93700
	/// </code>
	/// Be aware, however, that if the precision in your constructor is greater than
	/// the length specified to display,
	/// then <see cref="ToString(int)"/> effectively is truncating rather than rounding.
	/// <para>
	/// Since CleanCode 0.9.31.
	/// </para>
	/// </remarks>
	public struct RoundedTimeSpan
	{

		private const int TIMESPAN_SIZE = 7; // it always has seven digits

		private TimeSpan roundedTimeSpan;
		private int precision;

		/// <summary>
		/// Initializes a new instance of the <see cref="RoundedTimeSpan"/> class.
		/// </summary>
		/// <param name="ticks">A time period expressed in 100-nanosecond units.</param>
		/// <param name="precision">The non-negative precision to the right of the decimal.</param>
		/// <remarks>
		/// The algorithm used for rounding is valid only for the milliseconds of the
		/// underlying <see cref="TimeSpan"/>. Therefore the precision must be zero or greater.
		/// Attempts to use negative values (as are permitted with calls to <see cref="ToString(int)"/>
		/// will throw an <c>ArgumentException</c>.
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Throws this exception if precision is negative.
		/// </exception>
		public RoundedTimeSpan(long ticks, int precision)
		{
			if (precision < 0) { throw new ArgumentException("precision must be non-negative"); }
			if (precision > TIMESPAN_SIZE) { precision = TIMESPAN_SIZE; }

			this.precision = precision;
			int factor = (int)System.Math.Pow(10, (TIMESPAN_SIZE - precision));

			// This is only valid for rounding the milliseconds since the milliseconds
			// in the ticks are the actual digits in the ticks value.
			// That is if ticks is 123456789 then milliseconds are literally 3456789,
			// which is not true for seconds, minutes, or hours.
			roundedTimeSpan = new TimeSpan(((long)System.Math.Round((1.0 * ticks / factor)) * factor));
		}

		/// <summary>
		/// Gets the underlying rounded <see cref="TimeSpan"/>.
		/// </summary>
		/// <value>The time span.</value>
		public TimeSpan TimeSpan { get { return roundedTimeSpan; } }

		/// <summary>
		/// Returns the <see cref="TimeSpan"/> value rounded and trimmed
		/// to the precision specified in the constructor.
		/// </summary>
		/// <returns>
		/// A rounded string representation of a <see cref="TimeSpan"/>.
		/// </returns>
		/// <remarks>
		/// This method first rounds the TimeSpan to the specified precision,
		/// then trims its string representation to the same number of digits.
		/// </remarks>
		public override string ToString()
		{
			return ToString(precision);
		}

		/// <summary>
		/// Returns the <see cref="TimeSpan"/> value rounded 
		/// to the precision specified in the constructor then trimmed to the length specified here.
		/// </summary>
		/// <param name="length">The display length, a value between -9 and +7 inclusive.</param>
		/// <returns>
		/// A string representation of a <see cref="TimeSpan"/> rounded and padded with zeroes.
		/// </returns>
		/// <remarks>
		/// This method first rounds the TimeSpan to the specified precision,
		/// then trims to the length indicated.
		/// <list type="bullet">
		/// <item><description>
		/// If the length is greater than the precision the result is filled to the right
		/// with zeroes to that length.
		/// </description></item>
		/// <item><description>
		/// If the length is less than the precision the result is truncated
		/// to that length (so you have lost the benefit of rounding).
		/// </description></item>
		/// <item><description>
		/// The milliseconds of a TimeSpan object occupies seven characters so you may at
		/// most specify a length of 7.
		/// </description></item>
		/// <item><description>
		/// If the precision is specified as zero, the milliseconds are not shown at all,
		/// so calling this method would be identical to calling <see cref="ToString()"/>.
		/// </description></item>
		/// <item><description>
		/// If the length is 0, all milliseconds are suppressed but the decimal point is displayed.
		/// </description></item>
		/// <item><description>
		/// If the length is negative, you start trimming the decimal point, then the seconds then minutes then hours.
		/// These take a total of nine characters so you may specify up to -9.
		/// </description></item>
		/// </list>
		/// 
		/// </remarks>
		public string ToString(int length)
		{
			int digitsToStrip = TIMESPAN_SIZE - length;

			string s = roundedTimeSpan.ToString();

			if (!s.Contains(".") && length == 0) { return s; }

			// enforce canonical representation
			if (!s.Contains(".")) { s += "." + new string('0', TIMESPAN_SIZE); }

			int subLength = s.Length - digitsToStrip;
			return subLength < 0 ? "" : subLength > s.Length ? s : s.Substring(0, subLength);
		}

	}
}
