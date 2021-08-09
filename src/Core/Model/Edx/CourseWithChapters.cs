﻿using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Ulearn.Common.Extensions;

namespace Ulearn.Core.Model.Edx
{
	[XmlRoot("course")]
	public class CourseWithChapters : EdxItem
	{
		[XmlIgnore]
		public override string SubfolderName => "course";

		[XmlAttribute("advanced_modules")]
		public string AdvancedModules;

		[XmlAttribute("advertised_start")]
		public string AdvertisedStart;

		[XmlAttribute("allow_anonymous")]
		public bool AllowAnonymous;

		[XmlAttribute("allow_proctoring_opt_out")]
		public bool AllowProctoringOptOut;

		[XmlAttribute("available_proctoring_services")]
		public string AvailableProctoringServices;

		[XmlAttribute("cert_html_view_enabled")]
		public string CertHtmlViewEnabled;

		[XmlAttribute("days_early_for_beta")]
		public string DaysEarlyForBeta;

		[XmlAttribute("due_date_display_format")]
		public string DueDateDisplayFormat;

		[XmlAttribute("enable_proctored_exams")]
		public bool EnableProctoredExams;

		[XmlAttribute("enable_subsection_gating")]
		public bool EnableSubsectionGating;

		[XmlAttribute("enable_timed_exams")]
		public bool EnableTimedExams;

		[XmlAttribute("end")]
		public string End;

		[XmlAttribute("enrollment_end")]
		public string EnrollmentEnd;

		[XmlAttribute("enrollment_start")]
		public string EnrollmentStart;

		[XmlAttribute("graceperiod")]
		public string Graceperiod;

		[XmlAttribute("instructor_info")]
		public string InstructorInfo;

		[XmlAttribute("language")]
		public string Language;

		[XmlAttribute("learning_info")]
		public string LearningInfo;

		[XmlAttribute("lti_passports")]
		public string LtiPassports;

		[XmlAttribute("minimum_grade_credit")]
		public string MinimumGradeCredit;

		[XmlAttribute("start")]
		public string Start;

		[XmlAttribute("use_latex_compiler")]
		public bool UseLatexCompiler;

		[XmlAttribute("user_partitions")]
		public string UserPartitions;

		[XmlAttribute("video_upload_pipeline")]
		public string VideoUploadPipeline;

		[XmlElement("chapter", Order = 1)]
		public ChapterReference[] ChapterReferences
		{
			get { return chapterReferences = chapterReferences ?? new ChapterReference[0]; }
			set { chapterReferences = value; }
		}

		[XmlElement("wiki", Order = 2)]
		public Wiki[] Wiki;

		[XmlIgnore]
		public Chapter[] Chapters;

		private ChapterReference[] chapterReferences;

		public CourseWithChapters()
		{
			ChapterReferences = new ChapterReference[0];
		}

		public CourseWithChapters(string urlName, string displayName, string[] advancedModules, string[] ltiPassports, bool useLatexCompiler, Chapter[] chapters)
		{
			UrlName = urlName;
			DisplayName = displayName;
			AdvancedModules = advancedModules == null ? null : JsonConvert.SerializeObject(advancedModules);
			LtiPassports = ltiPassports == null ? null : JsonConvert.SerializeObject(ltiPassports);
			UseLatexCompiler = useLatexCompiler;
			Chapters = chapters;
			ChapterReferences = chapters.Select(x => x.GetReference()).ToArray();
		}

		public override void Save(string folderName)
		{
			var courseFile = $"{folderName}/{SubfolderName}/{UrlName}.xml";
			if (File.Exists(courseFile))
			{
				var doc = new XmlDocument();
				doc.LoadXml(File.ReadAllText(courseFile));

				XmlNode root = doc.DocumentElement;

				var count = root.ChildNodes.Count;
				for (var i = 0; i < count; i++)
					foreach (XmlElement childNode in root.ChildNodes)
						if (childNode.Name == "chapter")
							root.RemoveChild(childNode);

				foreach (var chapter in Chapters)
				{
					var elem = doc.CreateElement("chapter");
					elem.SetAttribute("url_name", chapter.UrlName);
					root.AppendChild(elem);
				}
				//Console.WriteLine(doc.XmlSerialize());

				File.WriteAllText(courseFile, doc.XmlSerialize());
				SaveAdditional(folderName);
			}
			else
				base.Save(folderName);
		}

		public override void SaveAdditional(string folderName)
		{
			foreach (var chapter in Chapters)
				chapter.Save(folderName);
		}

		public static CourseWithChapters Load(string folderName, string urlName, EdxLoadOptions options)
		{
			return Load<CourseWithChapters>(folderName, "course", urlName, options, c =>
			{
				c.Chapters = c.ChapterReferences.Select(x => Chapter.Load(folderName, x.UrlName, options)).ExceptNulls().ToArray();
				c.ChapterReferences = c.Chapters.Select(v => v.GetReference()).ToArray();
			});
		}
	}

	public class Wiki
	{
		[XmlAttribute("slug")]
		public string Slug;
	}
}