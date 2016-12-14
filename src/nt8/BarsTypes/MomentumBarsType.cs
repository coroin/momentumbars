//
// Copyright (C) 2011-2016 Coroin LLC <http://coroin.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.ComponentModel;
using NinjaTrader.Cbi;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;

//This namespace holds Bars types in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.BarsTypes
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
        // fields
        string displayName = "MomentumBars";
        int    prevBias    = 0;
        double rangeMax;
        double rangeMin;
        int    thisBias    = 0;
        double thisMax;
        double thisMin;
        double thisOpen;
        double thisRange;

        public override void ApplyDefaultBasePeriodValue(BarsPeriod period)
        {
            // not used
        }

        public override void ApplyDefaultValue(BarsPeriod period)
        {
            period.BarsPeriodTypeName  = displayName;
            period.BaseBarsPeriodValue = 4; // rangeMax
            period.Value               = 4; // rangeMin
            period.Value2              = 2; // openOption
        }

        public override string ChartLabel(DateTime time)
        {
            return time.ToString("T", Core.Globals.GeneralOptions.CurrentCulture);
        }

        public override int GetInitialLookBackDays(BarsPeriod barsPeriod, TradingHours tradingHours, int barsBack)
        {
            return 1;
        }

        public override double GetPercentComplete(Bars bars, DateTime now)
        {
            return 0;
        }

        protected override void OnDataPoint(Bars bars, double open, double high, double low, double close, DateTime time, long volume, bool isBar, double bid, double ask)
        {
            // method variables
            bool isNewSession = SessionIterator.IsNewSession(time, isBar);
            double tickSize = bars.Instrument.MasterInstrument.TickSize;

            if (SessionIterator == null){
                SessionIterator = new SessionIterator(bars);
            }

            if (isNewSession){
                SessionIterator.GetNextSession(time, isBar);
            }

            if (bars.Count == 0 || (bars.IsResetOnNewTradingDay && isNewSession)) {
                // update fields
                rangeMax = AddTwoDoubles(bars, bars.BarsPeriod.BaseBarsPeriodValue * tickSize, 0);
                rangeMin = AddTwoDoubles(bars, bars.BarsPeriod.Value * tickSize, 0);

                // set initial range, factoring dynamic
                thisRange = isDynamic ? rangeMin : rangeMax;
                AdjustMaxMin(bars, close, close);

                // add first bar
                AddBar(bars, thisOpen, thisOpen, thisOpen, thisOpen, time, volume);
            }
            else
            {
                // local variables
                double barOpen    = bars.GetOpen(bars.Count - 1);
                double barHigh    = bars.GetHigh(bars.Count - 1);
                double barLow     = bars.GetLow(bars.Count  - 1);
                int    maxCompare = bars.Instrument.MasterInstrument.Compare(close, thisMax);
                int    minCompare = bars.Instrument.MasterInstrument.Compare(close, thisMin);
                double thisClose  = maxCompare > 0 ? Math.Min(close, thisMax) : minCompare < 0 ? Math.Max(close, thisMin) : close;

                // range exceeded; create new bar(s)
                if (maxCompare > 0 || minCompare < 0)
                {
                    // local variables
                    bool newBar = true;

                    // update bias
                    prevBias = thisBias;
                    thisBias = close > barOpen ? 1 : close < barOpen ? -1 : 0;

                    // close current bar; volume included for on-touch only
                    // see this post for more info on volume calculation: http://www.ninjatrader.com/support/forum/showthread.php?p=302208#post302208
                    UpdateBar(bars, (maxCompare > 0 ? thisClose : barHigh), (minCompare < 0 ? thisClose : barLow), thisClose, time, 0);

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
                        AddBar(bars, thisOpen, (maxCompare > 0 ? thisClose : thisOpen), (minCompare < 0 ? thisClose : thisOpen), thisClose, time, (newBar ? volume : 0));
                        newBar = false;

                        // update fields
                        maxCompare = bars.Instrument.MasterInstrument.Compare(close, thisMax);
                        minCompare = bars.Instrument.MasterInstrument.Compare(close, thisMin);
                    }
                    while (maxCompare > 0 || minCompare < 0);
                }
                else
                    UpdateBar(bars, (close > barHigh ? close : barHigh), (close < barLow ? close : barLow), close, time, volume);
            }
            bars.LastPrice = close;
        }

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name        = displayName;
                Description = @"Momentum Bars Type is a variant of the Range Bar with a fixed or dynamic height candle body (open-close) configured by RangeMax, RangeMin, and OpenOption.";
                BarsPeriod  = new BarsPeriod { BarsPeriodType = (BarsPeriodType) 42, BarsPeriodTypeName = "MomentumBars", Value = 4, BaseBarsPeriodValue = 4, Value2 = 2 };
                BuiltFrom   = BarsPeriodType.Tick;
                DaysToLoad  = 5;
                IsIntraday  = true;
                IsTimeBased = false;
            }
            else if (State == State.Configure)
            {
                // remove properties not needed for this bar type
                Properties.Remove(Properties.Find("BaseBarsPeriodType", true));
                Properties.Remove(Properties.Find("PointAndFigurePriceType", true));
                Properties.Remove(Properties.Find("ReversalType", true));

                // set display labels
                SetPropertyName("Value", "Range Min");
                SetPropertyName("BaseBarsPeriodValue", "Range Max");
                SetPropertyName("Value2", "1=NoGap; 2=Real");

                // swap min/max if entered incorrectly
                int tmpMin = this.BarsPeriod.Value;
                int tmpMax = this.BarsPeriod.BaseBarsPeriodValue;
                if (tmpMin > tmpMax)
                {
                    this.BarsPeriod.Value = tmpMax;
                    this.BarsPeriod.BaseBarsPeriodValue = tmpMin;
                }
            }
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
            return bars.Instrument.MasterInstrument.RoundToTickSize((Math.Floor(10000000.0 * d1) + Math.Floor(10000000.0 * d2)) / 10000000.0);
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
                thisMax = AddTwoDoubles(bars, thisOpen, ((thisBias == 1)  ?  thisRange :  rangeMin));
                thisMin = AddTwoDoubles(bars, thisOpen, ((thisBias == -1) ? -thisRange : -rangeMin));
            }

            // static range
            else
            {
                thisMax = AddTwoDoubles(bars, thisOpen,  thisRange);
                thisMin = AddTwoDoubles(bars, thisOpen, -thisRange);
            }
        }

        // properties
        string nameLong   { get { return string.Format("{0} {1} {2}", displayName, rangeName, gapName); } }
        string gapName    { get { return (this.BarsPeriod.Value2 == 1) ? "NoGap" : "TrueOpen"; } }
        bool   isDynamic  { get { return Math.Min(this.BarsPeriod.BaseBarsPeriodValue, this.BarsPeriod.Value) < Math.Max(this.BarsPeriod.BaseBarsPeriodValue, this.BarsPeriod.Value) ? true : false; } }
        int    openOption { get { return (this.BarsPeriod.Value2 == 1) ? this.BarsPeriod.Value2 : 2; } }
        string rangeName  { get { return (isDynamic) ? string.Concat(Math.Min(this.BarsPeriod.BaseBarsPeriodValue, this.BarsPeriod.Value).ToString(), "-", Math.Max(this.BarsPeriod.BaseBarsPeriodValue, this.BarsPeriod.Value).ToString()) : this.BarsPeriod.BaseBarsPeriodValue.ToString(); } }
    }
}