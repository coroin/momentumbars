## MomentumBars for NinjaTrader

MomentumBars is a variant of the Range Bar with a fixed or dynamic height candle body (open-close).

User-defined values include RangeMax, RangeMin, and OpenOption.

Fixed-range candles result when RangeMax and RangeMin are equal.

Dynamic-range candles show each new trend beginning with RangeMin and incrementing one tick up to RangeMax for each successive with-trend candle.

There are two OpenOptions:
* NoGap artifically sets the open of each candle to the close of the previous.
* TrueOpen uses the actual open.

Regardless of dynamic or open settings, all candles express counter-trend sentiment as a wick (just like traditional range bars), meaning no upper wick on bull bar; no lower wick on bear bar; session-break bars may be dojis with upper/lower wicks.

#### How can I get MomentumBars?
* NinjaTrader 8 : [Download](https://github.com/coroin/momentumbars/raw/master/bin/MomentumBarsType8.zip) the latest zip (exported 2016-12-15 from NT 8.0.1.0).
* NinjaTrader 7: [Download](https://github.com/coroin/momentumbars/raw/master/bin/MomentumBarsType7.zip) the latest zip (exported 2016-11-03 from NT7v31).
* Clone repo: https://github.com/coroin/momentumbars.git

#### How do I get my charts to look like the images?

Also included in the package is an indicator called ciBarsTools, which includes the CandleColor options previously available from ciCandleOutline and adds RangeCounter to plot the Range of the bar. This indicator may work on other bars types but has not been tested on anything other than MomentumBars.
