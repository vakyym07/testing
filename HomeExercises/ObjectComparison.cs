using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
	public class ObjectComparison
	{
        [Test]
	    [Description("Проверка текущего царя")]
	    [Category("ToRefactor")]
	    public void CheckCurrentTsar()
	    {
	        var actualTsar = TsarRegistry.GetCurrentTsar();

	        var expectedTsar = new Person("Ivan IV The Terrible", 54, 170, 70,
	            new Person("Vasili III of Russia", 28, 170, 60, null));

	        // Перепишите код на использование Fluent Assertions.
	        var chekingField = new List<string>() {"Name", "Age", "Height", "Weight"};
	        foreach (var field in chekingField)
	        {
	            var fld = typeof(Person).GetField(field);
	            fld.GetValue(actualTsar).Should().Be(fld.GetValue(expectedTsar));
	            fld.GetValue(actualTsar.Parent).Should().Be(fld.GetValue(expectedTsar.Parent));
	        }
	    }

	    [Test]
		[Description("Альтернативное решение. Какие у него недостатки?")]
		public void CheckCurrentTsar_WithCustomEquality()
		{
			var actualTsar = TsarRegistry.GetCurrentTsar();
			var expectedTsar = new Person("Ivan IV The Terrible", 54, 170, 70,
			new Person("Vasili III of Russia", 28, 170, 60, null));

            // Какие недостатки у такого подхода? 
            // Недостатки этого подхода заключаются в том, что при добавлении новых свойств в 
            // класс Person возможна ситуация, когда мы захотим сравнивать объкты по новым свойствам.
            // Для этого придётся добавлять в метод AreEqual новые строки. 
            // Если сравниваемых полей достаточно много, метод раздуется, станет нечитаемым. 
            // 
            // Возможно, если родителей довольно много, из-за рекурсии можем получить StackOverflowException
            Assert.True(AreEqual(actualTsar, expectedTsar));

		}

		private bool AreEqual(Person actual, Person expected)
		{
			if (actual == expected) return true;
			if (actual == null || expected == null) return false;
			return
			actual.Name == expected.Name
			&& actual.Age == expected.Age
			&& actual.Height == expected.Height
			&& actual.Weight == expected.Weight
			&& AreEqual(actual.Parent, expected.Parent);
		}
	}

	public class TsarRegistry
	{
		public static Person GetCurrentTsar()
		{
			return new Person(
				"Ivan IV The Terrible", 54, 170, 70,
				new Person("Vasili III of Russia", 28, 170, 60, null));
		}
	}

	public class Person
	{
		public static int IdCounter = 0;
		public int Age, Height, Weight;
		public string Name;
		public Person Parent;
		public int Id;

		public Person(string name, int age, int height, int weight, Person parent)
		{
			Id = IdCounter++;
			Name = name;
			Age = age;
			Height = height;
			Weight = weight;
			Parent = parent;
		}
	}
}
