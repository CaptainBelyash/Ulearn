﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApprovalUtilities.Utilities;
using FluentAssertions;
using NUnit.Framework;
using uLearn.CSharp.Validators.SpellingValidator;
using Ulearn.Common.Extensions;

namespace uLearn.CSharp.SpellingValidation
{
	[TestFixture]
	public class SpellingValidator_should
	{
		private static DirectoryInfo TestDataDir => new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..",
			"..", "CSharp", "SpellingValidation", "TestData"));
		private static DirectoryInfo IncorrectTestDataDir => TestDataDir.GetDirectories("Incorrect").Single();
		private static DirectoryInfo CorrectTestDataDir => TestDataDir.GetDirectories("Correct").Single();

		private static IEnumerable<FileInfo> CorrectFiles => CorrectTestDataDir.EnumerateFiles();
		private static IEnumerable<FileInfo> IncorrectFiles => IncorrectTestDataDir.EnumerateFiles();

		private static DirectoryInfo BasicProgrammingDirectory =>
			new DirectoryInfo(TestPaths.BasicProgrammingDirectoryPath);

		private static IEnumerable<FileInfo> BasicProgrammingFiles =>
			BasicProgrammingDirectory
				.EnumerateFiles("*.cs", SearchOption.AllDirectories)
				.Where(f => !f.Name.Equals("Settings.Designer.cs") &&
							!f.Name.Equals("Resources.Designer.cs") &&
							!f.Name.Equals("AssemblyInfo.cs"));

		private static DirectoryInfo ULearnSubmissionsDirectory =>
			new DirectoryInfo(TestPaths.ULearnSubmissionsDirectoryPath);

		private static IEnumerable<FileInfo> SubmissionsFiles =>ULearnSubmissionsDirectory
			.EnumerateFiles("*.cs", SearchOption.AllDirectories)
			.Where(f => f.Name.Contains("Accepted"));
		
		private readonly SpellingValidator validator = new SpellingValidator();

		[TestCaseSource(nameof(IncorrectFiles))]
		public void FindErrors(FileInfo file)
		{
			var code = file.ContentAsUtf8();
			var errors = validator.FindErrors(code);

			errors.Should().NotBeEmpty();
		}

		[TestCaseSource(nameof(CorrectFiles))]
		public void NotFindErrors(FileInfo file)
		{
			var code = file.ContentAsUtf8();
			var errors = validator.FindErrors(code);
			if (errors != null)
			{
				Console.WriteLine(errors);
			}

			errors.Should().BeEmpty();
		}

		[Explicit]
		[TestCaseSource(nameof(BasicProgrammingFiles))]
		public void NotFindErrors_InBasicProgramming(FileInfo file)
		{
			var fileContent = file.ContentAsUtf8();

			var errors = validator.FindErrors(fileContent);

			if (errors.Any())
			{
				File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..",
						"..", "CSharp", "ExampleFiles", "errors", "spelling_validation", $"{file.Name}_errors.txt"),
					$@"{fileContent}

{errors.JoinStringsWith(err => $"{err.GetMessageWithPositions()}", Environment.NewLine)}");

				Assert.Fail();
			}
		}

		[Explicit]
		[TestCaseSource(nameof(SubmissionsFiles))]
		public void NotFindErrors_InCheckAcceptedFiles(FileInfo file)
		{
			var fileContent = file.ContentAsUtf8();

			var errors = validator.FindErrors(fileContent);
			if (!errors.Any())
			{
				File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..",
						"..", "CSharp", "ExampleFiles", "submissions_errors", "spelling_validation", $"{file.Name}_errors.txt"),
					$@"{fileContent}

{errors.JoinStringsWith(err => $"{err.GetMessageWithPositions()}", Environment.NewLine)}");

				Assert.Fail();
			}
		}
	}
}