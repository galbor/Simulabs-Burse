Price randomly changes according to the following formula:
new_price = Abs(N(price, price/10) -0.5) + 0.5
where N is the normal distribution
the 0.5 should make sure the price doesn't drop to 0
the 10 and the 0.5 are arbitrary magic numbers

changes price randomly every 20 seconds


assumptions:
	amount of stock is integer

	if trader A makes an offer on a stock and trader B makes an opposite (buy/sell) offer on that same stock, and the buy offer is higher, 
	then the sale price will be the price trader A specified


Since I'm not doing UI, it's possible for me (the only client) to buy and sell from all traders
I'm using decimals even though they take up more space, because they are more precise (important when dealing with money)

The order of sales might be suboptimal if a lot of offers are made/deleted at the same time
however no offer is ignored
for instance: Imagine I make a sale offer for 2$ and a sale offer for 1$, and someone else makes a buy offer for 2$ and a buy offer for 1$ at the same time
if I make the offer for 1$ and they make the offer for 2$ and they are processed together (we make them at the same time before the other offers)
then the offers will cancel each other and a sale will be made, but then my sale offer for 2$ and they're buy offer for 1$ will not cancel each other
this problem occurs even without concurrency (and without concurrency it's also unsolvable unless we ask that 2 offers match exactly to cancel each other)