# 🚀 Aspire MCP Server: Your AI-Powered Web Scraping Companion!

Ever dreamed of effortlessly scraping and analyzing web content with the power of AI? Welcome to **Aspire MCP Server**, your new best friend for intelligent, efficient, and fun web scraping adventures!

---

## 🌟 Why Aspire MCP Server?

- **AI-Enhanced Scraping**: Leverage the power of AI tools through the Model Context Protocol (MCP) to intelligently scrape and analyze web content.
- **Easy Integration**: Built with .NET 9, Aspire MCP Server integrates seamlessly into your existing .NET ecosystem.
- **Fast & Efficient**: With built-in caching and optimized HTTP handling, your scraping tasks run lightning-fast.
- **Extensible & Customizable**: Easily add your own scraping logic, tools, and AI integrations.

---

## 🛠️ What Does It Do?

Aspire MCP Server provides two powerful built-in tools:

### 🕸️ **WebContentTool**

Scrape and clean HTML content from any URL effortlessly!

- Removes scripts, styles, and unnecessary HTML tags.
- Extracts meaningful content from `<main>`, `<article>`, or content-specific `<div>` elements.
- Caches results for blazing-fast repeated access.

### 🔍 **WebCrawlerService**

Automate your web crawling tasks with ease!

- Parses `robots.txt` to discover sitemap URLs.
- Scrapes and parses XML sitemaps to find all listed URLs.
- Visits each URL, extracting and displaying useful information like links.

---

## 🚦 Getting Started

### 📦 Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Docker (optional, for containerized deployments)

### 🚀 Quick Start

1. **Clone the repo**

```bash
git clone https://github.com/your-username/aspire-mcp.git
cd aspire-mcp