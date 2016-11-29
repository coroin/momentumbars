## MomentumBars for NinjaTrader

MomentumBars is a variant of the Range Bar with a fixed or dynamic height candle body (open-close).

User-defined values include RangeMax, RangeMin, and OpenOption.

Fixed-range candles result when RangeMax and RangeMin are equal.

Dynamic-range candles show each new trend beginning with RangeMin and incrementing one tick up to RangeMax for each successive with-trend candle.

There are two OpenOptions:
* NoGap artifically sets the open of each candle to the close of the previous.
* TrueOpen uses the actual open.

Regardless of dynamic or open settings, all candles express counter-trend sentiment as a wick (just like traditional range bars), meaning no upper wick on bull bar; no lower wick on bear bar; session-break bars may be dojis with upper/lower wicks.

#### How can I get MomentumBars for NinjaTrader 8?

* The NT8 version currently in development and is expected to release before end of year.

#### How can I get MomentumBars for NinjaTrader 7?

* [Download](https://gitlab.com/coroin-ninjatrader/momentumbars/raw/master/bin/MomentumBarsType7.zip) the latest NinjaTrader7 zip import (exported 2016-11-03 for NT7v31).
* Clone repo: https://gitlab.com/coroin-ninjatrader/momentumbars.git

#### How do I get my charts to look like the images?

Also included in the package is an indicator called ciBarsTools, which includes the CandleColor options previously available from ciCandleOutline and adds RangeCounter to plot the Range of the bar. This indicator may work on other bars types but has not been tested on anything other than MomentumBars.
