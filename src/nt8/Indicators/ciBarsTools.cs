//
// Copyright (C) 2011-2016, Coroin LLC <http://coroin.com>
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
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.DrawingTools;

//This namespace holds Indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
    public class ciBarsTools : Indicator
    {
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description               = @"ciBarsTools provides candle colors and range counter for MomentumBars";
                Name                      = "ciBarsTools";
                Calculate                 = Calculate.OnPriceChange;
                IsOverlay                 = true;
                DisplayInDataBox          = true;
                DrawOnPricePanel          = true;
                DrawHorizontalGridLines   = true;
                DrawVerticalGridLines     = true;
                PaintPriceMarkers         = true;
                ScaleJustification        = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive  = true;
                CandleColors              = true;
                BullOutline               = Brushes.SteelBlue;
                BullTrend                 = Brushes.LightSteelBlue;
                BearOutline               = Brushes.Crimson;
                BearTrend                 = Brushes.LightCoral;
                DojiColor                 = Brushes.Olive;
                ZeroVolumeOutline         = Brushes.DarkGray;
                ZeroVolumeShading         = Brushes.LightGray;
                RangeCounter              = false;
                RangeCounterColor         = Brushes.Black;
                RangeCountCalcType        = ciRangeCalcType.OpenClose;
                RangeOffset               = 1;
            }
            else if (State == State.Configure)
            {
            }
        }

        protected override void OnBarUpdate()
        {
            // Candle Colors
            if (CandleColors)
            {
                // set bar color
                if (Volume[0] == 0)
                    BarBrush = ZeroVolumeShading;
                else if (Close[0] == High[0] && Open[0] == Low[0])
                    BarBrush = BullTrend;
                else if (Close[0] == Low[0] && Open[0] == High[0])
                    BarBrush = BearTrend;
                else
                    BarBrush = Brushes.Transparent;

                // set candle outline
                if (Volume[0] == 0)
                    CandleOutlineBrush = ZeroVolumeOutline;
                else if (Close[0] > Open[0])
                    CandleOutlineBrush = BullOutline;
                else if (Close[0] < Open[0])
                    CandleOutlineBrush = BearOutline;
                else
                    CandleOutlineBrush = DojiColor;
            }

            // Range Counter
            if (RangeCounter)
            {
                // local variables
                string tag = "RangeCounter_" + CurrentBar.ToString();
                double range = 1;
                double offset = RangeOffset * TickSize;
                double y = Low[0] - offset;
                Brush brush = RangeCounterColor;


                // calc range with user-specified formula
                switch (RangeCountCalcType)
                {
                    case ciRangeCalcType.HighLow:
                        range = Math.Abs(High[0] - Low[0]);
                        break;

                    case ciRangeCalcType.OpenClose:
                    default:
                        range = Math.Abs(Close[0] - Open[0]);
                        break;
                }
                range = Instrument.MasterInstrument.RoundToTickSize(range) / TickSize;


                // draw text on chart
                Draw.Text(this, tag, range.ToString(), 0, y, brush);
            }
        }

        [NinjaScriptProperty]
        [Display(Name="CandleColors", Description="Enable or Disable candle color options", Order=1, GroupName="Candle Colors")]
        public bool CandleColors
        { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name="BullOutline", Description="Outline color for bull candles (close is above open)", Order=2, GroupName="Candle Colors")]
        public Brush BullOutline
        { get; set; }

        [Browsable(false)]
        public string BullOutlineSerializable
        {
            get { return Serialize.BrushToString(BullOutline); }
            set { BullOutline = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name="BullTrend", Description="Shading color for bull trend candles (no bear wick)", Order=3, GroupName="Candle Colors")]
        public Brush BullTrend
        { get; set; }

        [Browsable(false)]
        public string BullTrendSerializable
        {
            get { return Serialize.BrushToString(BullTrend); }
            set { BullTrend = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name="BearOutline", Description="Outline color for bear candles (close is below open)", Order=4, GroupName="Candle Colors")]
        public Brush BearOutline
        { get; set; }

        [Browsable(false)]
        public string BearOutlineSerializable
        {
            get { return Serialize.BrushToString(BearOutline); }
            set { BearOutline = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name="BearTrend", Description="Shading color for bear trend candles (no bull wick)", Order=5, GroupName="Candle Colors")]
        public Brush BearTrend
        { get; set; }

        [Browsable(false)]
        public string BearTrendSerializable
        {
            get { return Serialize.BrushToString(BearTrend); }
            set { BearTrend = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name="DojiColor", Description="Color for doji candles (close same as open)", Order=6, GroupName="Candle Colors")]
        public Brush DojiColor
        { get; set; }

        [Browsable(false)]
        public string DojiColorSerializable
        {
            get { return Serialize.BrushToString(DojiColor); }
            set { DojiColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name="ZeroVolumeOutline", Description="Outline color for zero-volume candles (eg. phantom bars)", Order=7, GroupName="Candle Colors")]
        public Brush ZeroVolumeOutline
        { get; set; }

        [Browsable(false)]
        public string ZeroVolumeOutlineSerializable
        {
            get { return Serialize.BrushToString(ZeroVolumeOutline); }
            set { ZeroVolumeOutline = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name="ZeroVolumeShading", Description="Shading color for zero-volume candles (eg. phantom bars)", Order=8, GroupName="Candle Colors")]
        public Brush ZeroVolumeShading
        { get; set; }

        [Browsable(false)]
        public string ZeroVolumeShadingSerializable
        {
            get { return Serialize.BrushToString(ZeroVolumeShading); }
            set { ZeroVolumeShading = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(Name="RangeCounter", Description="Enable or Disable Range Counter which prints the Range value (Open-Close) associated with each candle on the chart", Order=9, GroupName="Range Counter")]
        public bool RangeCounter
        { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name="RangeCounterColor", Description="Range Counter Color", Order=10, GroupName="Range Counter")]
        public Brush RangeCounterColor
        { get; set; }

        [Browsable(false)]
        public string RangeCounterColorSerializable
        {
            get { return Serialize.BrushToString(RangeCounterColor); }
            set { RangeCounterColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(Name="RangeCountCalcType", Description="Calcluate range as OpenClose (default) or HighLow", Order=11, GroupName="Range Counter")]
        public ciRangeCalcType RangeCountCalcType
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="RangeOffset", Description="The number of ticks below the low to print Range Count", Order=12, GroupName="Range Counter")]
        public int RangeOffset
        { get; set; }
    }
}

/// <summary>
/// enumerator for calc type
/// </summary>
public enum ciRangeCalcType
{
    OpenClose = 0,
    HighLow = 1,
}
