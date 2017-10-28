using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using FluentAssertions.Equivalency;
using NUnit.Framework;
using NUnit.Framework.Internal.Filters;

namespace HomeExercises
{
    public class NumberValidatorTests
    {
        [TestCase(-1, 2, true, TestName = "throw ArgumentExceprion when precision less than zero")]
        [TestCase(0, 2, true, TestName = "throw ArgumentExceprion when precision equal zero")]
        [TestCase(1, -1, true, TestName = "throw ArgumentExceprion when scale less than zero")]
        [TestCase(1, 2, true, TestName = "throw ArgumentExceprion when scale greater than precision")]
        [TestCase(2, 2, true, TestName = "throw ArgumentExceprion when scale equal precision")]
        public void NumberVilidatorConstructor_Should(int precison, int scale, bool onlyPositive)
        {
            Action acttion = () => new NumberValidator(precison, scale, onlyPositive);
            acttion.ShouldThrow<ArgumentException>($"precison={precison} scale={scale}");
        }

        [TestCase(null, 17, 2, true, TestName = "when value is null")]
        [TestCase("", 17, 2, true, TestName = "when value is empty")]
        [TestCase("12345", 2, 0, true, TestName = "when int part greater than precision")]
        [TestCase("00.00", 3, 2, true, TestName = "when int part plus frac part without sign greater than precision")]
        [TestCase("+0.00", 3, 2, true, TestName = "when int part plus frac part with sign greater than precision")]
        [TestCase("-1.23", 4, 2, true, TestName = "when value is valid negative number and flag onlyPositive is true")]
        [TestCase("0.000", 17, 2, true, TestName = "when frac part greater than scale")]
        [TestCase("a.bc", 3, 2, true, TestName = "when value contains forbidden symbol")]
        public void IsValidNumber_Should_BeFalse(string value, int precison, int scale, bool onlyPositive)
        {
            new NumberValidator(precison, scale, onlyPositive).IsValidNumber(value).Should().BeFalse($"value={value}");
        }

        [TestCase("0,0", 17, 2, true, TestName = "when value is valid float number with comma")]
        [TestCase("0.0", 17, 2, true, TestName = "when value is valid float number with dot")]
        [TestCase("0", 17, 2, true, TestName = "when value is valid integer")]
        [TestCase("+1.23", 4, 2, true, TestName = "when int part plus frac part with sign not greater than precision")]
        public void isValidNumber_Should_BeTrue(string value, int precison, int scale, bool onlyPositive)
        {
            new NumberValidator(precison, scale, onlyPositive).IsValidNumber(value).Should().BeTrue($"value={value}");
        }

        public class NumberValidator
        {
            private readonly Regex numberRegex;
            private readonly bool onlyPositive;
            private readonly int precision;
            private readonly int scale;

            public NumberValidator(int precision, int scale = 0, bool onlyPositive = false)
            {
                this.precision = precision;
                this.scale = scale;
                this.onlyPositive = onlyPositive;
                if (precision <= 0)
                    throw new ArgumentException("precision must be a positive number");
                if (scale < 0 || scale >= precision)
                    throw new ArgumentException("precision must be a non-negative number less or equal than precision");
                numberRegex = new Regex(@"^([+-]?)(\d+)([.,](\d+))?$", RegexOptions.IgnoreCase);
            }

            public bool IsValidNumber(string value)
            {
                // Проверяем соответствие входного значения формату N(m,k), в соответствии с правилом, 
                // описанным в Формате описи документов, направляемых в налоговый орган в электронном виде по телекоммуникационным каналам связи:
                // Формат числового значения указывается в виде N(m.к), где m – максимальное количество знаков в числе, включая знак (для отрицательного числа), 
                // целую и дробную часть числа без разделяющей десятичной точки, k – максимальное число знаков дробной части числа. 
                // Если число знаков дробной части числа равно 0 (т.е. число целое), то формат числового значения имеет вид N(m).

                if (string.IsNullOrEmpty(value))
                    return false;

                var match = numberRegex.Match(value);
                if (!match.Success)
                    return false;

                // Знак и целая часть
                var intPart = match.Groups[1].Value.Length + match.Groups[2].Value.Length;
                // Дробная часть
                var fracPart = match.Groups[4].Value.Length;

                if (intPart + fracPart > precision || fracPart > scale)
                    return false;

                if (onlyPositive && match.Groups[1].Value == "-")
                    return false;
                return true;
            }
        }
    }
}