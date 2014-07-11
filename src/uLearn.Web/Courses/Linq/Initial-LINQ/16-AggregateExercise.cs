﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace uLearn.Courses.Linq.Slides
{
	[TestFixture]
	public class AggregateExercise
	{
		/*

		##Задача: Самое длинное слово

		Дан список слов, нужно найти самое длинное слово из этого списка, 
		а из всех самых длинных — лексикографически первое слово.

		Решите эту задачу без использования методов сортировки. 
		Дело в том, что сложность сортировки O(N * log(N)), однако эту задачу можно решить за O(N).

		*/

		[Exercise(SingleStatement = true)]
		[Hint("Вспомните про кортежи")]
		[Hint("Вспомните про особенности сравнения кортежей")]
		public string GetLongest(IEnumerable<string> words)
		{
			return words.Min(line => Tuple.Create(-line.Length, line)).Item2;
		}

		[Test]
		public void Test()
		{
			Assert.That(GetLongest(new[] {"asas", "as", "sdsd"}), Is.EqualTo("asas"));
			Assert.That(GetLongest(new[] {"zzzz", "as", "sdsd"}), Is.EqualTo("sdsd"));
			Assert.That(GetLongest(new[] {"as", "12345", "as", "sds"}), Is.EqualTo("12345"));
			Assert.That(GetLongest(new[] {""}).Length, Is.EqualTo(0));
		}
	}
}