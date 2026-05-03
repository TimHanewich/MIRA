<p align="center">
  <img src="logo/mira.svg" alt="MIRA logo" width="180" />
</p>

# MIRA: Market Intelligence & Research Agent
Mira is an autonomous investing research agent that researches the market, develops investing and trading strategies and manages a persistent paper portfolio.

Instead of being a market chatbot, MIRA behaves more like a small autonomous analyst:

1. it wakes up with a portfolio to manage,
2. reviews its prior journal entries to recover context,
3. researches companies and market developments,
4. reads earnings call transcripts and SEC financial facts,
5. makes simulated buy/sell decisions,
6. and logs its strategy before going back to sleep.

This makes the project part **agent framework experiment**, part **autonomous research loop**, and part **paper-trading portfolio manager**.

## MIRA's Tools
MIRA is given a toolset it can use to inspect its portfolio, gather information, make simulated trades, and record its reasoning.

### Portfolio and trading tools
- `view_portfolio`: opens the current simulated portfolio, including holdings, values, and cash balance
- `quote`: gets the latest quote and daily move for a symbol
- `buy`: buys shares inside the simulated portfolio
- `sell`: sells shares inside the simulated portfolio

### Memory and continuity tools
- `open_journal`: lists the dates for which prior journal entries exist
- `read_journal`: reads journal entries from a selected day
- `log_journal`: writes a new investment journal entry so the agent can explain its thinking to its future self

### Research tools
- `web_search` (*built in*): searches the web for current company, sector, and market information
- `read_earnings_call_transcript`: reads earnings call transcripts from Motley Fool transcript URLs
- `get_cik`: resolves a stock ticker to the company's SEC CIK
- `search_financial_data`: searches available SEC XBRL facts for a company
- `get_financial_data`: retrieves historical datapoints for a specific reported SEC fact

### Utility tools
- `calculate`: evaluates math expressions for sizing, comparison, and quick analysis

## MIRA Modes

When you start the app, MIRA offers four entry points:

```text
1. Schedule MIRA for Automated Investments
2. Run MIRA Now: Autonomous Mode
3. Run MIRA Now: Assistant Mode
4. Review Portfolio
```

### 1. Schedule MIRA for Automated Investments
Runs MIRA on a recurring schedule on **weekdays at 3:00 PM Eastern** to make trades before market closes.

### 2. Run MIRA Now: Autonomous Mode
This is the core experience. MIRA wakes up, reviews context, researches, decides whether to trade, logs its reasoning, saves state, and shuts down.

### 3. Run MIRA Now: Assistant Mode
A guided chat mode where you can interact with the agent directly. Type `/save` to persist state and close the session.

### 4. Review Portfolio
Builds a dashboard of the current simulated portfolio, including:
- cash balance
- holdings value
- allocation by symbol
- daily move
- cost basis
- unrealized gain/loss
- cumulative input/output token consumption


## Getting started

### 1. Prerequisites
You will need:

- .NET 10 SDK
- Python 3
- a model deployment accessible through Azure AI Foundry / compatible endpoint
- an API key and model name for that deployment

### 2. Start the local quote server
Install the Python dependencies and run the included server:

```bash
pip install flask yfinance
python src\yfinance-server.py
```

By default, MIRA expects that service to be available at `http://localhost:8080`.

### 3. Create your settings file
MIRA reads settings from:

```text
%LOCALAPPDATA%\MIRA\settings.json
```

Example:

```json
{
  "FoundryEndpoint": "https://your-resource.openai.azure.com/",
  "FoundryApiKey": "YOUR_API_KEY",
  "FoundryModel": "your-model-name"
}
```

### 4. Run the app

```bash
dotnet run --project src\MIRA.csproj
```
  
## Disclaimer
This project is for experimentation and software development purposes. It is **not financial advice**, and it does **not** execute real-money trades.
