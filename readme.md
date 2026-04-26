# AIA: Auto-Invest-Agent

## Plan:
1. Agent wakes up at 3 PM EST each day. If weekend, skip
2. Agent gets :
    - system prompt of who it is and what it is to do
    - Current portfolio w/ gains/losses, transactions, etc.
    - Instructions for what to do (e.g. strategize and ways to do so)
    - New earnings call transcripts today
    - Investment journal entries it has made previously
3. Agent given tools:
    - Search web (built in)
    - Check stock price
    - Check earnings call transcript
    - Sell stock
    - Buy stock
    - End day early (no more trades needed - end day as is!)
    - Read investment journal entries
    - Add investment journal entry
4. Agent can make up to 10 trades (buy/sell) or terminate by ending day early
5. Goes to sleep