//
// Copyright (C) 2011-2017, Coroin LLC <http://coroin.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
using System;
using System.ComponentModel;

namespace NinjaTrader.Data
{
    /// <summary>
    /// Momentum Bars Type is a variant of the Range Bar with a fixed or dynamic height candle body (open-close).
    /// User-defined values include RangeMax, RangeMin, and OpenOption. Fixed-range candles result when RangeMax
    /// and RangeMin are equal. Dynamic-range candles show each new trend beginning with RangeMin and incrementing
    /// one tick up to RangeMax for each successive with-trend candle. There are two OpenOptions: NoGap and TrueOpen.
    /// NoGap artifically sets the open of each candle to the close of the previous. TrueOpen uses the actual open.
    /// Regardless of dynamic or open settings, all candles express counter-trend sentiment as a wick (just like
    /// traditional range bars), meaning no upper wick on bull bar; no lower wick on bear bar; session-break bars
    /// may be dojis with upper/lower wicks, due to an internal NT7 issue).
    /// </summary>
    public class MomentumBarsType : BarsType
    {
        #region Fields

        static bool registered = Register(new MomentumBarsType());
        static double tickSize;
        string displayName = "MomentumBars";
        int prevBias = 0;
        double rangeMax;
        double rangeMin;
        int thisBias = 0;
        double thisMax;
        double thisMin;
        double thisOpen;
        double thisRange;

        #endregion Fields

        #region Methods

        /// <summary>
        /// constructor (note: change Final4 if conflicting with another custom bar type)
        /// </summary>
        public MomentumBarsType()
            : base(PeriodType.Final4)
        {
        }

        /// <summary>
        /// built from period type
        /// </summary>
        public override PeriodType BuiltFrom
        {
            get { return PeriodType.Tick; }
        }

        /// <summary>
        /// default value
        /// </summary>
        public override int DefaultValue
        {
            get { return 4; }
        }

        /// <summary>
        /// display name used for chart display
        /// </summary>
        public override string DisplayName
        {
            get { return displayName; }
        }

        /// <summary>
        /// intraday data
        /// </summary>
        public override bool IsIntraday
        {
            get { return true; }
        }

        /// <summary>
        /// add new tick data to bars
        /// </summary>
        public override void Add(Bars bars, double open, double high, double low, double close, DateTime time, long volume, bool isRealtime)
        {
            // create initial bar on first tick and handle NT7 session-break issue (note: removing IsNewSession() creates invalid bars; remove if preferred)
            if ((bars.Count == 0) || bars.IsNewSession(time, isRealtime))
            {
                // update fields
                tickSize = bars.Instrument.MasterInstrument.TickSize;
                rangeMax = AddTwoDoubles(bars, (double)Period.Value * tickSize, 0);
                rangeMin = AddTwoDoubles(bars, (double)Period.BasePeriodValue * tickSize, 0);

                /// swap min/max if entered incorrectly
                if (rangeMin > rangeMax)
                {
                    double tmp = rangeMax;
                    rangeMax = rangeMin;
                    rangeMin = tmp;
                }

                // set initial range, factoring dynamic
                thisRange = isDynamic ? rangeMin : rangeMax;
                AdjustMaxMin(bars, close, close);

                // add first bar
                AddBar(bars, thisOpen, thisOpen, thisOpen, thisOpen, time, volume, isRealtime);
            }

            // continue all subsequent ticks/bars
            else
            {
                // local variables
                Data.Bar thisBar = (Bar)bars.Get(bars.Count - 1);
                int maxCompare = bars.Instrument.MasterInstrument.Compare(close, thisMax);
                int minCompare = bars.Instrument.MasterInstrument.Compare(close, thisMin);
                double thisClose = maxCompare > 0 ? Math.Min(close, thisMax) : minCompare < 0 ? Math.Max(close, thisMin) : close;

                // range exceeded; create new bar(s)
                if (maxCompare > 0 || minCompare < 0)
                {
                    // local variables
                    bool newBar = true;

                    // update bias
                    prevBias = thisBias;
                    thisBias = close > thisBar.Open ? 1 : close < thisBar.Open ? -1 : 0;

                    // close current bar; volume included for on-touch only
                    // see this post for more info on volume calculation: http://www.ninjatrader.com/support/forum/showthread.php?p=302208#post302208
                    UpdateBar(bars, thisBar.Open, (maxCompare > 0 ? thisClose : thisBar.High), (minCompare < 0 ? thisClose : thisBar.Low), thisClose, time, 0, isRealtime);

                    // add next bar and loop phantom bars, if needed
                    do
                    {
                        // update thisRange for dynamic
                        if (isDynamic)
                        {
                            // increment range for same bias, if range has not exceeded max
                            if ((thisBias == prevBias || prevBias == 0) && thisRange < rangeMax)
                            {
                                thisRange = AddTwoDoubles(bars, thisRange, tickSize);
                            }

                            // increment range after trend change (will only fire once)
                            else if (thisBias != prevBias && prevBias != 0)
                            {
                                thisRange = AddTwoDoubles(bars, rangeMin, tickSize);
                            }

                            // ensure valid range
                            thisRange = Math.Min(thisRange, rangeMax);
                        }

                        // update fields
                        AdjustMaxMin(bars, thisClose, close);
                        thisClose = (maxCompare > 0) ? Math.Min(close, thisMax) : (minCompare < 0) ? Math.Max(close, thisMin) : close;

                        // add new bar; include volume once (except for on-touch), then create phantom bars
                        // see this post for more info on volume calculation: http://www.ninjatrader.com/support/forum/showthread.php?p=302208#post302208
                        AddBar(bars, thisOpen, (maxCompare > 0 ? thisClose : thisOpen), (minCompare < 0 ? thisClose : thisOpen), thisClose, time, (newBar ? volume : 0), isRealtime);
                        newBar = false;

                        // update fields
                        maxCompare = bars.Instrument.MasterInstrument.Compare(close, thisMax);
                        minCompare = bars.Instrument.MasterInstrument.Compare(close, thisMin);
                    }
                    while (maxCompare > 0 || minCompare < 0);
                }

                // range not exceeded; continue current bar
                else
                {
                    // update current bar
                    UpdateBar(bars, thisBar.Open, (close > thisBar.High ? close : thisBar.High), (close < thisBar.Low ? close : thisBar.Low), close, time, volume, isRealtime);
                }
            }

            // update last price
            bars.LastPrice = close;
        }

        /// <summary>
        /// apply default values to the base period type
        /// </summary>
        public override void ApplyDefaults(Gui.Chart.BarsData barsData)
        {
            // rangeMax
            barsData.Period.Value = 4;

            // rangeMin
            barsData.Period.BasePeriodValue = 4;

            // openOption
            barsData.Period.Value2 = 2;

            // days back default
            barsData.DaysBack = 10;
        }

        /// <summary>
        /// chart data box date
        /// </summary>
        public override string ChartDataBoxDate(DateTime time)
        {
            return time.ToString(Cbi.Globals.CurrentCulture.DateTimeFormat.ShortDatePattern);
        }

        /// <summary>
        /// chart label
        /// </summary>
        public override string ChartLabel(Gui.Chart.ChartControl chartControl, DateTime time)
        {
            return time.ToString(chartControl.LabelFormatTick, Cbi.Globals.CurrentCulture);
        }

        /// <summary>
        /// clone object
        /// </summary>
        public override object Clone()
        {
            return new MomentumBarsType();
        }

        /// <summary>
        /// get initial look back days to load into chart
        /// </summary>
        public override int GetInitialLookBackDays(Period period, int barsBack)
        {
            return new MomentumBarsType().GetInitialLookBackDays(period, barsBack);
        }

        /// <summary>
        /// get percent complete
        /// </summary>
        public override double GetPercentComplete(Bars bars, DateTime now)
        {
            throw new ApplicationException("GetPercentComplete not supported in " + DisplayName);
        }

        /// <summary>
        /// property descriptor collection - add / remove additional properties
        /// </summary>
        public override PropertyDescriptorCollection GetProperties(PropertyDescriptor propertyDescriptor, Period period, Attribute[] attributes)
        {
            // local variables
            PropertyDescriptorCollection properties = base.GetProperties(propertyDescriptor, period, attributes);

            // remove properties not needed for this bar type
            properties.Remove(properties.Find("BasePeriodType", true));
            properties.Remove(properties.Find("PointAndFigurePriceType", true));
            properties.Remove(properties.Find("ReversalType", true));

            // set display labels
            Gui.Design.DisplayNameAttribute.SetDisplayName(properties, "Value", "\r\rRange Max");
            Gui.Design.DisplayNameAttribute.SetDisplayName(properties, "BasePeriodValue", "\rRange Min");
            Gui.Design.DisplayNameAttribute.SetDisplayName(properties, "Value2", "Open Option");

            // return collection
            return properties;
        }

        /// <summary>
        /// display friendly label
        /// </summary>
        public override string ToString(Period period)
        {
            return displayNameLong;
        }

        /// <summary>
        /// add two doubles and return double rounded to ticksize
        /// </summary>
        /// <param name="bars">bars array</param>
        /// <param name="d1">first double input</param>
        /// <param name="d2">second double input</param>
        /// <returns>double that has been rounded to ticksize</returns>
        double AddTwoDoubles(Bars bars, double d1, double d2)
        {
            return bars.Instrument.MasterInstrument.Round2TickSize((Math.Floor(10000000.0 * d1) + Math.Floor(10000000.0 * d2)) / 10000000.0);
        }

        /// <summary>
        /// adjust fields used to determine range completion
        /// </summary>
        /// <param name="nogapClose">current value of thisClose</param>
        /// <param name="realClose">real price used for next open for gap bars</param>
        void AdjustMaxMin(Bars bars, double nogapClose, double realClose)
        {
            // update fields
            thisOpen = openOption == 1 ? nogapClose : realClose;

            // dynamic range
            if (isDynamic)
            {
                thisMax = AddTwoDoubles(bars, thisOpen, ((thisBias == 1) ? thisRange : rangeMin));
                thisMin = AddTwoDoubles(bars, thisOpen, ((thisBias == -1) ? -thisRange : -rangeMin));
            }

            // static range
            else
            {
                thisMax = AddTwoDoubles(bars, thisOpen, thisRange);
                thisMin = AddTwoDoubles(bars, thisOpen, -thisRange);
            }
        }

        #endregion Methods

        #region Properties

        string displayNameLong { get { return string.Format("{0} {1} {2}", displayName, rangeName, gapName); } }

        string gapName { get { return (Period.Value2 == 1) ? "NoGap" : "TrueOpen"; } }

        bool isDynamic { get { return Math.Min(Period.Value, Period.BasePeriodValue) < Math.Max(Period.Value, Period.BasePeriodValue) ? true : false; } }

        int openOption { get { return (Period.Value2 == 1) ? Period.Value2 : 2; } }

        string rangeName { get { return (isDynamic) ? string.Concat(Math.Min(Period.Value, Period.BasePeriodValue).ToString(), "-", Math.Max(Period.Value, Period.BasePeriodValue).ToString()) : Period.Value.ToString(); } }

        #endregion Properties
    }
}