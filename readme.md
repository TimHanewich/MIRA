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
- built-in `web_search`: searches the web for current company, sector, and market information
- `read_earnings_call_transcript`: reads earnings call transcripts from Motley Fool transcript URLs
- `get_cik`: resolves a stock ticker to the company's SEC CIK
- `search_financial_data`: searches available SEC XBRL facts for a company
- `get_financial_data`: retrieves historical datapoints for a specific reported SEC fact

### Utility tools
- `calculate`: evaluates math expressions for sizing, comparison, and quick analysis

## Why this project is interesting

A lot of AI finance demos stop at "tell me about this stock."

MIRA goes a step further:

- **Stateful**: it keeps portfolio state, journal entries, and cumulative token usage.
- **Tool-using**: it can act on a portfolio instead of only describing one.
- **Continuity-driven**: it is explicitly instructed to read its own prior journal before making new decisions.
- **Research-oriented**: it blends web search, transcripts, and structured SEC data.
- **Autonomous by design**: it can be run on demand or scheduled to wake up automatically.

That journal-first design is one of the most fun parts of the repo: MIRA is treated like an agent with memory loss, so it has to reconstruct its strategy from notes it previously wrote to itself.

## Modes

When you start the app, MIRA offers four entry points:

```text
Schedule MIRA for Automated Investments
Run MIRA Now: Autonomous Mode
Run MIRA Now: Assistant Mode
Review Portfolio
```

### 1. Schedule MIRA for Automated Investments
Runs MIRA on a recurring schedule. The current implementation waits until the next **weekday at 3:00 PM Eastern** and then wakes the agent to perform its investment review.

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

## How it works

At a high level, an autonomous run looks like this:

1. Load settings from `%LOCALAPPDATA%\MIRA\settings.json`
2. Confirm the local quote server is online
3. Configure SEC EDGAR access
4. Load persistent state from `%LOCALAPPDATA%\MIRA\state.json`
5. Seed the portfolio with $100,000 if this is the first run
6. Create the agent with web search enabled and tool access
7. Instruct it to read its journal before doing fresh research
8. Let it analyze, trade, and log its reasoning
9. Save everything back to disk

The project uses **Azure AI Foundry/OpenAI-style model access** via `TimHanewich.Foundry`, with a tool-enabled agent built on `TimHanewich.AgentFramework`.

## Agent tools

MIRA is equipped with a focused set of tools:

| Tool | Purpose |
|---|---|
| `quote` | Get the latest price and daily change for a symbol |
| `buy` | Buy shares in the simulated portfolio |
| `sell` | Sell shares from the simulated portfolio |
| `view_portfolio` | Open the current portfolio with valuations |
| `open_journal` | List days that have journal entries |
| `read_journal` | Read journal entries from a given day |
| `log_journal` | Write a new investment journal entry |
| `read_earnings_call_transcript` | Read a Motley Fool earnings call transcript from a provided URL |
| `get_cik` | Convert a stock symbol into an SEC CIK |
| `search_financial_data` | Search available SEC XBRL facts for a company |
| `get_financial_data` | Retrieve historical data points for a specific SEC fact |
| `calculate` | Evaluate math expressions |

Together, those tools give the model a full loop of **research -> analysis -> decision -> portfolio action -> journaling**.

## Architecture highlights

### C#/.NET console app
The main application lives in `src/` and is built as a .NET console app using **Spectre.Console** for the terminal UX.

### Persistent local state
MIRA stores:

- portfolio state
- investment journal entries
- cumulative input tokens consumed
- cumulative output tokens consumed

This state is serialized to disk so the agent can continue from prior sessions.

### Local quote microservice
Quotes are not pulled directly inside the .NET app. Instead, MIRA talks to a tiny local HTTP service at `http://localhost:8080`.

That service is included in this repository as `src/yfinance-server.py` and exposes endpoints like:

- `GET /alive`
- `GET /quote/MSFT`
- `GET /quote/MSFT,AAPL,NVDA`

This keeps quote retrieval simple and decoupled from the agent runtime.

### SEC EDGAR integration
MIRA can move beyond news headlines and inspect structured company-reported data through SEC XBRL facts. That means it can search concepts like revenue, assets, liabilities, or other reported facts and then pull the historical datapoints behind them.

### Earnings transcript ingestion
The agent can also read earnings call transcripts using `TheMotleyFool.Transcripts`, which gives it another source of qualitative context when forming an investment view.

## Tech stack

- **.NET 10**
- **C#**
- **Spectre.Console**
- **TimHanewich.AgentFramework**
- **TimHanewich.Foundry**
- **TimHanewich.Investing**
- **SecuritiesExchangeCommission.Edgar**
- **TheMotleyFool.Transcripts**
- **Python + Flask + yfinance** for the local quote server

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

## Repository layout

```text
AutoInvestAI/
├── readme.md
├── logo/
└── src/
    ├── Program.cs
    ├── Prompt.cs
    ├── State.cs
    ├── PortfolioDashboard.cs
    ├── YFinanceServerBridge.cs
    ├── yfinance-server.py
    └── Tools/
        ├── Buy.cs
        ├── Sell.cs
        ├── Quote.cs
        ├── ViewPortfolio.cs
        ├── OpenJournal.cs
        ├── ReadJournal.cs
        ├── LogJournalEntry.cs
        ├── ReadEarningsCallTranscript.cs
        └── SEC/
```

## Important notes

- **This is a simulated trading system.** It does not connect to a real brokerage or place live trades.
- The portfolio is managed using `TimHanewich.Investing.Simulation.Portfolio`.
- The quote service must be running locally or MIRA will refuse to continue.
- The transcript reader expects Motley Fool transcript URLs.
- Autonomous scheduling currently targets **weekdays at 3 PM Eastern**.

## What this repo demonstrates

If you are browsing this project as a GitHub landing page, the short version is:

**MIRA is an autonomous, tool-using, stateful investment research agent that paper-trades a persistent portfolio using live quotes, web research, SEC filings, and earnings transcripts.**

It is less about "predicting stocks with AI" and more about building the scaffolding for a believable autonomous decision-maker: memory, tools, state, continuity, and a repeatable operating loop.

## Disclaimer

This project is for experimentation and software development purposes. It is **not financial advice**, and it does **not** execute real-money trades.
