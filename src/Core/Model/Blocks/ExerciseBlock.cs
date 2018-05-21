﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using RunCsJob.Api;
using uLearn.Model.Edx.EdxComponents;
using Ulearn.Common.Extensions;
using Component = uLearn.Model.Edx.EdxComponents.Component;

namespace uLearn.Model.Blocks
{
	public abstract class ExerciseBlock : IncludeCode
	{
		protected ExerciseBlock()
		{
			MaxScore = 5;
			CorrectnessScore = MaxScore;
			Validator = new ValidatorDescription();
		}

		[XmlAttribute("type")]
		public ExerciseType ExerciseType { get; set; } = ExerciseType.CheckOutput;

		[XmlElement("max-score")]
		public int MaxScore { get; set; }

		[XmlElement("initial-code")]
		public string ExerciseInitialCode { get; set; }

		[XmlElement("inital-code")]
		public string LegacyExerciseInitialCode
		{
			get => ExerciseInitialCode;
			set => ExerciseInitialCode = value;
		}
		
		[XmlElement("hint")]
		public List<string> Hints { get; set; }

		[XmlElement("comment")]
		public string CommentAfterExerciseIsSolved { get; set; }

		[XmlElement("expected")]
		// Ожидаемый корректный вывод программы
		public string ExpectedOutput { get; set; }

		[XmlElement("hide-expected-output")]
		public bool HideExpectedOutputOnError { get; set; }

		[XmlElement("validator")]
		public ValidatorDescription Validator { get; set; }

		[XmlElement("texts")]
		public ExerciseTexts Texts { get; set; } = new ExerciseTexts();

		[XmlElement("check-for-plagiarism")]
		public bool CheckForPlagiarism { get; set; } = true;

		[XmlElement("hide-solutions")]
		public bool HideShowSolutionsButton { get; set; }

		[XmlElement("require-review")]
		public bool RequireReview { get; set; }

		[XmlElement("correctness-score")]
		public int CorrectnessScore { get; set; }

		[XmlElement("scoring-group")]
		public string ScoringGroup { get; set; }

		[XmlIgnore]
		public int MaxReviewScore => MaxScore - CorrectnessScore;

		[XmlIgnore]
		public List<string> HintsMd
		{
			get
			{
				return Hints = Hints?.Select(h => h.RemoveCommonNesting()).ToList() ?? new List<string>();
			}
		}

		public abstract string GetSourceCode(string code);

		public abstract SolutionBuildResult BuildSolution(string userWrittenCode);

		public abstract RunnerSubmission CreateSubmission(string submissionId, string code);

		#region equals

		private bool Equals(ExerciseBlock other)
		{
			return Equals(ExerciseInitialCode, other.ExerciseInitialCode) && Equals(ExpectedOutput, other.ExpectedOutput) && Equals(HintsMd, other.HintsMd);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals((ExerciseBlock)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = ExerciseInitialCode?.GetHashCode() ?? 0;
				hashCode = (hashCode * 397) ^ (ExpectedOutput?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ (HintsMd?.GetHashCode() ?? 0);
				return hashCode;
			}
		}

		#endregion

		public override string ToString()
		{
			return $"Exercise: {ExerciseInitialCode}, Hints: {string.Join("; ", HintsMd)}";
		}

		public Component GetSolutionsComponent(string displayName, Slide slide, int componentIndex, string launchUrl,
			string ltiId)
		{
			return new LtiComponent(displayName, slide.NormalizedGuid + componentIndex + "-solutions", launchUrl, ltiId, false,
				0, false);
		}

		public Component GetExerciseComponent(string displayName, Slide slide, int componentIndex, string launchUrl,
			string ltiId)
		{
			return new LtiComponent(displayName, slide.NormalizedGuid + componentIndex, launchUrl, ltiId, true, CorrectnessScore,
				false);
		}

		public override Component ToEdxComponent(string displayName, Slide slide, int componentIndex)
		{
			throw new NotSupportedException();
		}

		public override string TryGetText()
		{
			return (ExerciseInitialCode ?? "") + '\n'
					+ string.Join("\n", HintsMd) + '\n'
					+ (CommentAfterExerciseIsSolved ?? "");
		}

		public void CheckScoringGroup(string slideDescriptionForErrorMessage, ScoringSettings scoring)
		{
			var scoringGroupsIds = scoring.Groups.Keys;
			if (!string.IsNullOrEmpty(ScoringGroup) && !scoringGroupsIds.Contains(ScoringGroup))
				throw new CourseLoadingException(
					$"Неизвестная группа оценки у задания {slideDescriptionForErrorMessage}: {ScoringGroup}\n" +
					"Возможные значения: " + string.Join(", ", scoringGroupsIds));

			if (string.IsNullOrEmpty(ScoringGroup))
				ScoringGroup = scoring.DefaultScoringGroupForExercise;
		}
	}

	public class ExerciseTexts
	{
		// «Все тесты пройдены». Показывается тем, у кого потенциально бывает код-ревью, но это решение не отправлено на него.
		// Например, потому что есть более новое решение.
		[XmlElement("all-tests-passed")]
		public string AllTestsPassed { get; set; }
		
		// «Все тесты пройдены, задача сдана». Показывается тем, у кого не бывает код-ревью. Например, вольнослушателям.
		[XmlElement("all-tests-passed-without-review")]
		public string AllTestsPassedWithoutReview { get; set; }
		
		// «Все тесты пройдены, за&nbsp;код-ревью {0} +{1}». Вместо {0} и {1} подставляются слова «получено»/«получен» и «X балл/балла/баллов». 
		[XmlElement("code-review-passed")]
		public string CodeReviewPassed { get; set; }
		
		// «Все тесты пройдены, за&nbsp;<a href=\"{0}\" title=\"Отредактировать код-ревью\">код-ревью</a> {1} +{2}».
		// Аналогично предыдущему, только со ссылкой на редактирование код-ревью для преподавателя.
		[XmlElement("code-review-passed-instructor-view")]
		public string CodeReviewPassedInstructorView { get; set; }
		
		// «Все тесты пройдены, решение ожидает код-ревью». Показывается студенту.
		[XmlElement("waiting-for-code-review")]
		public string WaitingForCodeReview { get; set; }
		
		// «Все тесты пройдены, решение ожидает <a href=\"{0}\" title=\"Перейти к код-ревью\">код-ревью</a>». Показывается преподавателю.
		// Вместо {0} подставляется ссылка на код-ревью. 
		[XmlElement("waiting-for-code-review-instructor-view")]
		public string WaitingForCodeReviewInstructorView { get; set; }
	}

	public enum ExerciseType
	{
		[XmlEnum("check-exit-code")]
		CheckExitCode,
		
		[XmlEnum("check-output")]
		CheckOutput
	}

	public class ValidatorDescription
	{
		public ValidatorDescription()
		{
			ValidatorName = "";
		}
		
		[XmlAttribute("remove-defaults")]
		[DefaultValue(false)]
		public bool RemoveDefaults { get; set; }

		[XmlText]
		public string ValidatorName { get; set; }
	}
}