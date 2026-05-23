using System;
using System.Linq;

namespace WordTrainer
{
    public static class TranslationChecker
    {
        private static readonly char[] VariantSeparators = { ',', ';', '/' };

        public static bool IsCorrect(string userAnswer, string correctAnswer)
        {
            var normalized = userAnswer.Trim();
            if (string.IsNullOrEmpty(normalized))
                return false;

            return correctAnswer
                .Split(VariantSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(variant => normalized.Equals(variant, StringComparison.OrdinalIgnoreCase));
        }
    }
}
