using System;
using System.Globalization;
using System.Text;
using LLama;

namespace DepoimentoLocal.Windows
{
    // Compiled for the bundled .NET 8 runtime and embedded in the application.
    public static class ContextBudget
    {
        public static string Validate(LLamaWeights weights, string prompt, int maxTokens, int contextSize)
        {
            if (weights == null) throw new InvalidOperationException("Carregue um modelo antes de gerar.");
            if (maxTokens <= 0) throw new InvalidOperationException("A geração precisa de um limite positivo de tokens.");
            // Match StatelessExecutor: BOS enabled, special ChatML tokens parsed, UTF-8.
            int promptTokens = weights.Tokenize(prompt, true, true, Encoding.UTF8).Length;
            const int safetyTokens = 32;
            long required = (long)promptTokens + maxTokens + safetyTokens;
            string budget = string.Format(CultureInfo.InvariantCulture,
                "context={0}; promptTokens={1}; maxTokens={2}; safetyTokens={3}; requiredTokens={4}; remainingTokens={5}",
                contextSize, promptTokens, maxTokens, safetyTokens, required, contextSize - required);
            if (required > contextSize)
                throw new InvalidOperationException("O texto completo, as instruções e a reserva de resposta excedem o contexto disponível. Nenhum conteúdo foi cortado. Divida a entrada em trechos menores. " + budget);
            return budget;
        }
    }
}
