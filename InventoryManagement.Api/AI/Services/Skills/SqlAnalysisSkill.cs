using InventoryManagement.Api.AI.Models;
using Newtonsoft.Json;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using InventoryManagement.Domain.Model;
using InventoryManagement.Api.UseCase;
using Microsoft.Extensions.Options;
using InventoryManagement.Api.Config;

namespace InventoryManagement.Api.AI.Services.Skills
{
    /// <summary>
    /// Skill that converts natural language to SQL queries, executes them, and provides AI-powered analysis
    /// </summary>
    public class SqlAnalysisSkill : ISkill
    {
        public SkillType SkillType => SkillType.SqlAnalysis;

        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly string _model;
        private readonly ILogger<SqlAnalysisSkill> _logger;
        private readonly InventoryConfig _configuration;
        private readonly BillService _billService;
        // Database schema information for AI context
        private readonly string _schemaInfo = @"
DATABASE SCHEMA:
- Bills(Id, Name, Mobile, Discount, Advance, PaymentMode, BillDate)
- BillItems(Id, BillId, ItemId, Quantity, Amount)
- Items(Id, Name, Car, Quantity, Description, Price, Barcode)
- Expenses(Id, Description, Amount, Date, ExpenseType, PaymentMode, SupplierId)
- Suppliers(Id, Name, Mobile, Date)

Enum Mappings (stored as INT in DB):
- PaymentMode → 0 = CARD, 1 = UPI, 2 = CASH
- ExpenseType → 0 = DEBIT, 1 = CREDIT

Common patterns:
- Always return valid SQLite SQL.
- Use JOINs when the query involves relationships (e.g., Bills ↔ BillItems ↔ Items).
- Join Bills and BillItems to analyze sales by product
- Join Expenses and Suppliers for supplier analysis  
- Join BillItems and Items to analyze product performance
- Union Bills and Expenses for financial analysis
- Use `strftime` for date filtering.
- Limit results only if asked by the user.
- Never generate `DROP`, `DELETE`, or schema modification queries.
- Only use SELECT, GROUP BY, ORDER BY, WHERE, SUM, COUNT, AVG when needed.
- Use CreatedAt for time-based analysis
- Filter by Name for user-specific data";

        public SqlAnalysisSkill(
            HttpClient httpClient,
            IOptionsSnapshot<InventoryConfig> configuration,
            ILogger<SqlAnalysisSkill> logger, BillService billService)
        {
            _httpClient = httpClient;
            _configuration = configuration.Value;
            _logger = logger;

            _apiKey = _configuration.PerplexityAI.ApiKey ?? throw new ArgumentNullException("PerplexityAI:ApiKey");
            _baseUrl = _configuration.PerplexityAI.BaseUrl ?? "https://api.perplexity.ai/chat/completions";
            _model = _configuration.PerplexityAI.Model ?? "llama-3.1-sonar-small-128k-online";

            // Get database connection string
            _billService = billService;
        }

        /// <summary>
        /// Executes the SQL analysis workflow: Query -> SQL -> Data -> AI Summary
        /// </summary>
        public async Task<SkillResponse> ExecuteAsync(SkillRequest request)
        {
            if (request.UserQuery.ToLower().Contains("suggestions"))
            {
                return GetSuggestedQuestions();
            }

            try
            {
                _logger.LogInformation("Processing SQL analysis query: {Query}", request.UserQuery);

                // Step 1: Convert natural language to SQL
                var sqlQuery = await GenerateSqlQueryAsync(request.UserQuery);
                
                if (string.IsNullOrEmpty(sqlQuery))
                {
                    return new SkillResponse
                    {
                        Response = "I couldn't generate a valid SQL query for your request. Please try rephrasing your question.",
                        UsedLocalData = false,
                        SkillType = SkillType.SqlAnalysis
                    };
                }

                // Step 2: Execute SQL query safely
                var queryResults = await ExecuteSqlQueryAsync(sqlQuery);
                
                if (queryResults == null || queryResults.Count == 0)
                {
                    queryResults = new List<Dictionary<string, object>>();
                }

                // Step 3: Format data for AI analysis
                var dataContext = FormatDataForAI(queryResults, sqlQuery);
                
                // Step 4: Generate AI-powered summary and insights
                var aiResponse = await GenerateAISummaryAsync(request.UserQuery, dataContext, sqlQuery);

                return new SkillResponse
                {
                    Response = aiResponse,
                    DataSummary = dataContext,
                    UsedLocalData = true,
                    SkillType = SkillType.SqlAnalysis,
                    SuggestedQuestions = GetRelatedQuestions()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing SQL analysis for query: {Query}", request.UserQuery);
                return new SkillResponse
                {
                    Response = "I encountered an error while analyzing your data. This could be due to a complex query or database issue. Please try a simpler question or contact support.",
                    UsedLocalData = false,
                    SkillType = SkillType.SqlAnalysis
                };
            }
        }

        /// <summary>
        /// Converts natural language query to SQL using AI
        /// </summary>
        private async Task<string> GenerateSqlQueryAsync(string userQuery)
        {
            var prompt = $@"Convert this business question to a SQL query for a SQLite inventory management database.

{_schemaInfo}

User Question: {userQuery}

Requirements:
- Generate ONLY a valid SQLite SELECT query
- Always include LIMIT 100 to prevent large result sets
- Use proper JOINs when needed
- Include relevant WHERE clauses
- No INSERT, UPDATE, DELETE, or DROP statements
- Return only the SQL query, no explanations

Examples:
""Show me top selling products"" → 
SELECT i.Name, SUM(bi.Quantity) as TotalSold, SUM(bi.Amount) as Revenue 
FROM Items i 
JOIN BillItems bi ON i.Id = bi.ItemId 
GROUP BY i.Id, i.Name 
ORDER BY TotalSold DESC 

""Sales in last 30 days"" →
SELECT DATE(b.CreatedAt) as Date, SUM(b.TotalAmount) as DailySales 
FROM Bills b 
WHERE b.CreatedAt >= datetime('now', '-30 days') 
GROUP BY DATE(b.CreatedAt) 
ORDER BY Date DESC 

SQL Query:";

            try
            {
                var request = new PerplexityRequest
                {
                    model = _model,
                    temperature = 0, // Deterministic for SQL generation
                    max_tokens = 300,
                    messages = new List<PerplexityMessage>
                    {
                        new() { role = "user", content = prompt }
                    }
                };

                var jsonContent = JsonConvert.SerializeObject(request);
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_baseUrl, httpContent);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var perplexityResponse = JsonConvert.DeserializeObject<PerplexityResponse>(responseContent);
                var sqlQuery = perplexityResponse?.choices?.FirstOrDefault()?.message?.content?.Trim();

                // Clean up the SQL query
                sqlQuery = CleanSqlQuery(sqlQuery);
                
                _logger.LogInformation("Generated SQL: {SQL}", sqlQuery);
                return sqlQuery;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating SQL query");
                return string.Empty;
            }
        }

        /// <summary>
        /// Executes SQL query safely with user context
        /// </summary>
        private async Task<List<Dictionary<string, object>>> ExecuteSqlQueryAsync(string sqlQuery)
        {
            try
            {
                // Validate SQL query for safety
                if (!IsSafeSqlQuery(sqlQuery))
                {
                    _logger.LogWarning("Unsafe SQL query blocked: {SQL}", sqlQuery);
                    return null;
                }

                // call inventory mangement api with sql query
                var result = _billService.ExecuteSqlQuery(sqlQuery);
                _logger.LogInformation("SQL query executed successfully, returned {RowCount} rows", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing SQL query: {SQL}", sqlQuery);
                return null;
            }
        }

        /// <summary>
        /// Formats query results for AI consumption
        /// </summary>
        private string FormatDataForAI(List<Dictionary<string, object>> data, string sqlQuery)
        {
            var summary = new StringBuilder();
            summary.AppendLine($"=== SQL QUERY RESULTS ===");
            summary.AppendLine($"Query: {sqlQuery}");
            summary.AppendLine($"Rows returned: {data.Count}");
            summary.AppendLine();

            // Include column headers
            if (data.Count > 0)
            {
                var headers = string.Join(" | ", data[0].Keys);
                summary.AppendLine($"Columns: {headers}");
                summary.AppendLine();
            }

            // Include sample data (first 20 rows)
                var rowsToShow = Math.Min(20, data.Count);
            summary.AppendLine($"Data (showing first {rowsToShow} rows):");
            
            for (int i = 0; i < rowsToShow; i++)
            {
                var row = data[i];
                var values = row.Select(field => SafeToString(field.Value) ?? "NULL");
                summary.AppendLine(string.Join(" | ", values));
            }

            if (data.Count > 20)
            {
                summary.AppendLine($"... and {data.Count - 20} more rows");
            }

            return summary.ToString();
        }
        private static string SafeToString(object value)
        {
            if (value == null || value is DBNull) 
                return "NULL";

            return value switch
            {
                DateTimeOffset dto => dto.ToString("yyyy-MM-dd HH:mm:ss"), // consistent datetime format
                DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"), // consistent datetime format
                bool b => b ? "TRUE" : "FALSE",
                decimal d => d.ToString("0.##"), // avoids long trailing decimals
                double db => db.ToString("0.##"),
                float f => f.ToString("0.##"),
                _ => value.ToString()
            };
        }

        /// <summary>
        /// Generates AI-powered analysis of the SQL results
        /// </summary>
        private async Task<string> GenerateAISummaryAsync(string userQuery, string dataContext, string sqlQuery)
        {
            var prompt = $@"You are a business intelligence analyst. Analyze this SQL query result and provide insights.

Original Question: {userQuery}
SQL Query Used: {sqlQuery}

Data Results:
{dataContext}

Instructions:
- Provide a clear, business-focused analysis
- Highlight key findings and patterns
- Include specific numbers from the data
- Do not suggest actionable recommendations if not asked
- Keep it concise but insightful
- If the data shows trends, explain their business significance
- The query itself is AI generated
- Do not include query or analyze query or provide insights or change on query
- use ₹ for currency formatting
- Focus on answering the user's original question with the actual data provided in Data-result section.

Focus on answering the user's original question with the actual data provided in Data-result section.
Keep the summary short and not more than 4 sentences
";

            try
            {
                var request = new PerplexityRequest
                {
                    model = _model,
                    temperature = 0.3,
                    max_tokens = 500,
                    messages = new List<PerplexityMessage>
                    {
                        new() { role = "user", content = prompt }
                    }
                };

                var jsonContent = JsonConvert.SerializeObject(request);
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_baseUrl, httpContent);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var perplexityResponse = JsonConvert.DeserializeObject<PerplexityResponse>(responseContent);

                return perplexityResponse?.choices?.FirstOrDefault()?.message?.content?.Trim()
                       ?? "I was able to retrieve your data, but I'm having trouble generating insights right now. Please review the raw data above.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI summary");
                return "I retrieved the data successfully, but couldn't generate a summary. Please review the data results above for insights.";
            }
        }

        /// <summary>
        /// Validates SQL query for safety (prevents malicious queries)
        /// </summary>
        private bool IsSafeSqlQuery(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql)) return false;
            
            var upperSql = sql.ToUpper().Trim();
            
            // Must be a SELECT query
            if (!upperSql.StartsWith("SELECT") && !upperSql.StartsWith("WITH")) return false;

            // Block dangerous operations - use word boundaries to avoid false positives
            var dangerousPatterns = new[] 
            { 
                @"\bDROP\b", @"\bDELETE\b", @"\bINSERT\b", @"\bUPDATE\b", 
                @"\bALTER\b", @"\bCREATE\s+TABLE\b", @"\bCREATE\s+INDEX\b", @"\bCREATE\s+VIEW\b",
                @"\bEXEC\b", @"\bEXECUTE\b", @"\bTRUNCATE\b", @"\bGRANT\b", @"\bREVOKE\b"
            };
            
            foreach (var pattern in dangerousPatterns)
            {
                if (Regex.IsMatch(upperSql, pattern, RegexOptions.IgnoreCase))
                {
                    Console.WriteLine($"Blocked dangerous SQL pattern: {pattern}");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Cleans up AI-generated SQL query
        /// </summary>
        private string CleanSqlQuery(string? sql)
        {
            if (string.IsNullOrEmpty(sql)) return string.Empty;
            
            // Remove markdown code blocks if present
            sql = Regex.Replace(sql, @"```sql\n?", "", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"```\n?", "", RegexOptions.IgnoreCase);
            
            // Remove extra whitespace and newlines
            sql = Regex.Replace(sql, @"\s+", " ").Trim();
            
            // Ensure it ends with semicolon
            if (!sql.EndsWith(";")) sql += ";";
            
            return sql;
        }

        /// <summary>
        /// Gets SQL analysis related suggested questions
        /// </summary>
        private List<string> GetRelatedQuestions()
        {
            return new List<string>
            {
                "Show me my top 10 selling products",
                "What are my daily sales for the last 30 days?",
                "Which customers have the highest purchase amounts?",
                "What products have the lowest inventory turnover?",
                "Show me revenue by month for this year",
                "Which items generate the most profit margin?",
                "What are my peak sales hours/days?",
                "Show me products that haven't sold in 90 days"
            };
        }

        /// <summary>
        /// Returns suggested questions for SQL analysis
        /// </summary>
        private SkillResponse GetSuggestedQuestions()
        {
            return new SkillResponse
            {
                Response = "I can analyze your data using custom SQL queries. Here are some examples of what you can ask:",
                SuggestedQuestions = GetRelatedQuestions(),
                UsedLocalData = false,
                SkillType = SkillType.SqlAnalysis
            };
        }
    }
}
