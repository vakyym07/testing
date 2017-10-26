using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using FluentAssertions.Equivalency;
using NUnit.Framework;

namespace HomeExercises
{
    public class NumberValidatorTests
    {
        [Test]
        public void NumberValidatorConstructor_Should_ThrowArgumenException_When_PrecisionLessThanZero()
        {
            Action act = () => new NumberValidator(-1, 2, true);
            act.ShouldThrow<ArgumentException>("precision = -1, scale = 2")
                .WithMessage("precision must be a positive number");
        }

        [Test]
        public void NumberValidatorConstructor_Should_ThrowArgumenException_When_PrecisionEqualZero()
        {
            Action act = () => new NumberValidator(0, 2, true);
            act.ShouldThrow<ArgumentException>("precision = 0, scale = 2")
                .WithMessage("precision must be a positive number");
        }

        [Test]
        public void NumberValidatorConstructor_Should_ThrowArgumenException_When_ScaleLessThanZero()
        {
            Action act = () => new NumberValidator(1, -1, true);
            act.ShouldThrow<ArgumentException>("precision = 1, scale = -1")
                .WithMessage("precision must be a non-negative number less or equal than precision");
        }

        [Test]
        public void NumberValidatorConstructor_Should_ThrowArgumenException_When_ScaleGreaterThanPrecision()
        {
            Action act = () => new NumberValidator(1, 2, true);
            act.ShouldThrow<ArgumentException>("precision = 1, scale = 2")
                .WithMessage("precision must be a non-negative number less or equal than precision");
        }

        [Test]
        public void NumberValidatorConstructor_Should_ThrowArgumenException_When_ScaleEqualPrecision()
        {
            Action act = () => new NumberValidator(2, 2, true);
            act.ShouldThrow<ArgumentException>("precision = 2, scale = 2")
                .WithMessage("precision must be a non-negative number less or equal than precision");
        }

        [Test]
        public void IsValidNumber_Should_ReturnFalse_When_ValueIsNull()
        {
            new NumberValidator(17, 2, true).IsValidNumber(null).Should().BeFalse("value is null");
        }

        [Test]
        public void IsValidNumber_Should_RetrunFalse_When_ValueIsEmptyString()
        {
            new NumberValidator(17, 2, true).IsValidNumber(string.Empty).Should().BeFalse("value is empty");
        }

        [Test]
        public void IsValidNumber_Should_ReturnTrue_When_ValueIsValidFloatNumberWithComma()
        {
            new NumberValidator(17, 2, true).IsValidNumber("0,0").Should().BeTrue("value = 0,0");
        }

        [Test]
        public void IsValidNumber_Should_ReturnTrue_When_ValueIsValidFloatNumberWithDot()
        {
            new NumberValidator(17, 2, true).IsValidNumber("0.0").Should().BeTrue("value = 0.0");
        }

        [Test]
        public void IsValidNumber_Should_ReturnTrue_When_ValueIsValidIntegerNumber()
        {
            new NumberValidator(17, 2, true).IsValidNumber("0").Should().BeTrue("value = 0");
        }

        [Test]
        public void IsValidNumber_Should_RetrunFalse_When_IntPartGreaterThanPrecision()
        {
            new NumberValidator(2, 0, true).IsValidNumber("12345").Should().BeFalse("value = 12345");
        }

        [Test]
        public void IsValidNumber_Should_ReturnFalse_When_IntPartPlusFracPartWithOutSignGreterThanPrecision()
        {
            new NumberValidator(3, 2, true).IsValidNumber("00.00").Should().BeFalse("value = 00.00");
        }

        [Test]
        public void IsValidNumber_Should_ReturnFalse_When_IntPartPlusFracPartWithSignGreterThanPrecision()
        {
            new NumberValidator(3, 2, true).IsValidNumber("+0.00").Should().BeFalse("value = +0.00");
        }

        [Test]
        public void IsValidNumber_Should_RetrunTrue_When_IntPartPlusFracPartWithSignNotGreterThanPrecision()
        {
            new NumberValidator(4, 2, true).IsValidNumber("+1.23").Should().BeTrue("value = +1.23");
        }

        [Test]
        public void IsValidNumber_Should_ReturnFalse_WhenValueIsValidNegativeNumberAndFlagOnlyPositiveIsTrue()
        {
            new NumberValidator(4, 2, true).IsValidNumber("-1.23").Should().BeFalse("value = -1.23");
        }

        [Test]
        public void IsValidNumber_Should_ReturnFalse_WhenFracPartGreaterThanScale()
        {
            new NumberValidator(17, 2, true).IsValidNumber("0.000").Should().BeFalse("value = 0.000");
        }

        [Test]
        public void IsValidNumber_Should_RetrunFalse_When_ValueContainsForbiddenSymbol()
        {
            new NumberValidator(3, 2, true).IsValidNumber("a.sd").Should().BeFalse("value = a.sd");
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