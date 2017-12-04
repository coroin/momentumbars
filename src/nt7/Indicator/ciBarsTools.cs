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
using System.Drawing;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;

namespace NinjaTrader.Indicator
{
    /// <summary>
    /// ciBarsTools provides candle colors and range counter for MomentumBars
    /// </summary>
    [Description("ciBarsTools provides candle colors and range counter for MomentumBars")]
    public class ciBarsTools : IndicatorBase
    {
        /// <summary>
        /// configure indicator; called before any data is loaded
        /// </summary>
        protected override void Initialize()
        {
            CalculateOnBarClose = false;
            ChartOnly = true;
            DrawOnPricePanel = true;
            Overlay = true;
            PriceTypeSupported = false;
        }

        /// <summary>
        ///  called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
            // Candle Colors
            if (candleColors)
            {
                // set bar color
                if (Volume[0] == 0)
                    BarColor = zeroVolumeShading;
                else if (Close[0] == High[0] && Open[0] == Low[0])
                    BarColor = bullTrend;
                else if (Close[0] == Low[0] && Open[0] == High[0])
                    BarColor = bearTrend;
                else
                    BarColor = Color.Transparent;

                // set candle outline
                if (Volume[0] == 0)
                    CandleOutlineColor = zeroVolumeOutline;
                else if (Close[0] > Open[0])
                    CandleOutlineColor = bullOutline;
                else if (Close[0] < Open[0])
                    CandleOutlineColor = bearOutline;
                else
                    CandleOutlineColor = dojiColor;
            }

            // Range Counter
            if (rangeCounter)
            {
                // local variables
                string tag = "ebtRangeCounter_" + CurrentBar.ToString();
                double range = 1;
                double offset = rangeOffset * TickSize;
                double y = Low[0] - offset;
                Color color = rangeCounterColor;


                // calc range with user-specified formula
                switch (rangeCountCalcType)
                {
                    case ciRangeCalcType.HighLow:
                        range = Math.Abs(High[0] - Low[0]);
                        break;

                    case ciRangeCalcType.OpenClose:
                    default:
                        range = Math.Abs(Close[0] - Open[0]);
                        break;
                }
                range = Instrument.MasterInstrument.Round2TickSize(range) / TickSize;


                // draw text on chart
                DrawText(tag, range.ToString(), 0, y, color);
            }
        }

        /// <summary>
        /// returns display name for chart
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "ciBarsTools";
        }

        #region Candle Colors - properties, and fields
        [Description("Enable or Disable candle color options.")]
        [GridCategory("\t\tCandle Colors")]
        [NinjaTrader.Gui.Design.DisplayNameAttribute("\tCandle Colors")]
        public ciEnableDisable CandleColors
        {
            get { return candleColors == true ? ciEnableDisable.Enable : ciEnableDisable.Disable; }
            set { candleColors = value == ciEnableDisable.Enable ? true : false; }
        }
        bool candleColors = true;

        /// <summary>
        /// bull candle outline color
        /// </summary>
        [XmlIgnore]
        [Description("Outline color for bull candles (close is above open)")]
        [GridCategory("\t\tCandle Colors")]
        [NinjaTrader.Gui.Design.DisplayNameAttribute("Bull Outline")]
        public Color BullOutline
        {
            get { return bullOutline; }
            set { bullOutline = value; }
        }
        Color bullOutline = Color.SteelBlue;

        /// <summary>
        /// serializable bull candle outline color
        /// </summary>
        [Browsable(false)]
        public string BullOutlineSerialize
        {
            get { return NinjaTrader.Gui.Design.SerializableColor.ToString(bullOutline); }
            set { bullOutline = NinjaTrader.Gui.Design.SerializableColor.FromString(value); }
        }

        /// <summary>
        /// bull trend candle color
        /// </summary>
        [XmlIgnore]
        [Description("Shading color for bull trend candles (no bear wick)")]
        [GridCategory("\t\tCandle Colors")]
        [NinjaTrader.Gui.Design.DisplayNameAttribute("Bull Trend")]
        public Color BullTrend
        {
            get { return bullTrend; }
            set { bullTrend = value; }
        }
        Color bullTrend = Color.LightSteelBlue;

        /// <summary>
        /// serializable bull trend candle color
        /// </summary>
        [Browsable(false)]
        public string BullTrendSerialize
        {
            get { return NinjaTrader.Gui.Design.SerializableColor.ToString(bullTrend); }
            set { bullTrend = NinjaTrader.Gui.Design.SerializableColor.FromString(value); }
        }

        /// <summary>
        /// bear candle outline color
        /// </summary>
        [XmlIgnore]
        [Description("Outline color for bear candles (close is below open)")]
        [GridCategory("\t\tCandle Colors")]
        [NinjaTrader.Gui.Design.DisplayNameAttribute("Bear Outline")]
        public Color BearOutline
        {
            get { return bearOutline; }
            set { bearOutline = value; }
        }
        Color bearOutline = Color.Crimson;

        /// <summary>
        /// serializable bear candle outline color
        /// </summary>
        [Browsable(false)]
        public string BearOutlineSerialize
        {
            get { return NinjaTrader.Gui.Design.SerializableColor.ToString(bearOutline); }
            set { bearOutline = NinjaTrader.Gui.Design.SerializableColor.FromString(value); }
        }

        /// <summary>
        /// bear trend candle color
        /// </summary>
        [XmlIgnore]
        [Description("Shading color for bear trend candles (no bull wick)")]
        [GridCategory("\t\tCandle Colors")]
        [NinjaTrader.Gui.Design.DisplayNameAttribute("Bear Trend")]
        public Color BearTrend
        {
            get { return bearTrend; }
            set { bearTrend = value; }
        }
        Color bearTrend = Color.LightCoral;

        /// <summary>
        /// serializable bear trend candle color
        /// </summary>
        [Browsable(false)]
        public string BearTrendSerialize
        {
            get { return NinjaTrader.Gui.Design.SerializableColor.ToString(bearTrend); }
            set { bearTrend = NinjaTrader.Gui.Design.SerializableColor.FromString(value); }
        }

        /// <summary>
        /// doji candle color
        /// </summary>
        [XmlIgnore]
        [Description("Color for doji candles (close same as open)")]
        [GridCategory("\t\tCandle Colors")]
        [NinjaTrader.Gui.Design.DisplayNameAttribute("Doji Color")]
        public Color DojiColor
        {
            get { return dojiColor; }
            set { dojiColor = value; }
        }
        Color dojiColor = Color.Olive;

        /// <summary>
        /// serializable doji candle color
        /// </summary>
        [Browsable(false)]
        public string DojiColorSerialize
        {
            get { return NinjaTrader.Gui.Design.SerializableColor.ToString(dojiColor); }
            set { dojiColor = NinjaTrader.Gui.Design.SerializableColor.FromString(value); }
        }

        /// <summary>
        /// zero-volume candle outline color
        /// </summary>
        [XmlIgnore]
        [Description("Outline color for zero-volume candles (eg. range bars)")]
        [GridCategory("\t\tCandle Colors")]
        [NinjaTrader.Gui.Design.DisplayNameAttribute("Zero-Volume Outline")]
        public Color ZeroVolumeOutline
        {
            get { return zeroVolumeOutline; }
            set { zeroVolumeOutline = value; }
        }
        Color zeroVolumeOutline = Color.DarkGray;

        /// <summary>
        /// serializable zero-volume candle outline color
        /// </summary>
        [Browsable(false)]
        public string ZeroVolumeOutlineSerialize
        {
            get { return NinjaTrader.Gui.Design.SerializableColor.ToString(zeroVolumeOutline); }
            set { zeroVolumeOutline = NinjaTrader.Gui.Design.SerializableColor.FromString(value); }
        }

        /// <summary>
        /// zero-volume candle shading color
        /// </summary>
        [XmlIgnore]
        [Description("Shading color for zero-volume candles (eg. range bars)")]
        [GridCategory("\t\tCandle Colors")]
        [NinjaTrader.Gui.Design.DisplayNameAttribute("Zero-Volume Shading")]
        public Color ZeroVolumeShading
        {
            get { return zeroVolumeShading; }
            set { zeroVolumeShading = value; }
        }
        Color zeroVolumeShading = Color.LightGray;

        /// <summary>
        /// serializable doji candle color
        /// </summary>
        [Browsable(false)]
        public string ZeroVolumeShadingSerialize
        {
            get { return NinjaTrader.Gui.Design.SerializableColor.ToString(zeroVolumeShading); }
            set { zeroVolumeShading = NinjaTrader.Gui.Design.SerializableColor.FromString(value); }
        }
        #endregion

        #region Range Counter - properties, and fields
        /// <summary>
        /// print range counter on chart
        /// </summary>
        [Description("Enable or Disable Range Counter which prints the Range value (Open-Close) associated with each candle on the chart.")]
        [GridCategory("\t\tRange Counter")]
        [NinjaTrader.Gui.Design.DisplayNameAttribute("\tRange Counter")]
        public ciEnableDisable RangeCounter
        {
            get { return rangeCounter == true ? ciEnableDisable.Enable : ciEnableDisable.Disable; }
            set { rangeCounter = value == ciEnableDisable.Enable ? true : false; }
        }
        bool rangeCounter = false;

        /// <summary>
        /// range counter color
        /// </summary>
        [XmlIgnore]
        [Description("Range Counter Color")]
        [GridCategory("\t\tRange Counter")]
        [NinjaTrader.Gui.Design.DisplayNameAttribute("Counter Color")]
        public Color RangeCounterColor
        {
            get { return rangeCounterColor; }
            set { rangeCounterColor = value; }
        }
        Color rangeCounterColor = Color.Black;

        /// <summary>
        /// serializable range counter color
        /// </summary>
        [Browsable(false)]
        public string RangeCounterColorSerialize
        {
            get { return NinjaTrader.Gui.Design.SerializableColor.ToString(rangeCounterColor); }
            set { rangeCounterColor = NinjaTrader.Gui.Design.SerializableColor.FromString(value); }
        }

        /// <summary>
        /// the relative position of range counter
        /// </summary>
        [Description("Calculation determins how the Range is calculated. Choices are OpenClose (default) and HighLow.")]
        [GridCategory("\t\tRange Counter")]
        [NinjaTrader.Gui.Design.DisplayNameAttribute("Calculation")]
        public ciRangeCalcType RangeCountCalcType
        {
            get { return rangeCountCalcType; }
            set { rangeCountCalcType = value; }
        }
        ciRangeCalcType rangeCountCalcType = ciRangeCalcType.OpenClose;

        /// <summary>
        /// number of ticks below the low to print range counter
        /// </summary>
        [Description("The Range Offset is the number of ticks below the low to print Range Count.")]
        [GridCategory("\t\tRange Counter")]
        [NinjaTrader.Gui.Design.DisplayNameAttribute("Range Offset")]
        public int RangeOffset
        {
            get { return rangeOffset; }
            set { rangeOffset = Math.Max(1, value); }
        }
        int rangeOffset = 1;
        #endregion
    }
}
#region Enums
/// <summary>
/// enumerator for Enable/Disable
/// </summary>
public enum ciEnableDisable
{
    Enable = 1,
    Disable = 0,
}

/// <summary>
/// enumerator for calc type
/// </summary>
public enum ciRangeCalcType
{
    OpenClose = 0,
    HighLow = 1,
}
#endregion
