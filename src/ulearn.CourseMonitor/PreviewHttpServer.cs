﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RunCsJob;
using RunCsJob.Api;
using uLearn.Model.Blocks;
using uLearn.Web.Models;

namespace uLearn.CourseTool
{
	internal class PreviewHttpServer
	{
		private readonly HttpListener listener;
		private readonly string courseDir;
		private readonly string htmlDir;
		private readonly int port;
		private DateTime lastChangeTime = DateTime.MinValue;
		private volatile Course course;
		private readonly object locker = new object();

		public PreviewHttpServer(string courseDir, string htmlDir, int port)
		{
			listener = new HttpListener();
			listener.Prefixes.Add($"http://+:{port}/");
			this.courseDir = courseDir;
			this.htmlDir = htmlDir;
			this.port = port;
			CopyStaticToHtmlDir();
		}

		private void CopyStaticToHtmlDir()
		{
			if (!Directory.Exists(htmlDir))
				Directory.CreateDirectory(htmlDir);
			var staticDir = Path.Combine(htmlDir, "static");
			if (!Directory.Exists(staticDir))
				Directory.CreateDirectory(staticDir);
			Utils.DirectoryCopy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "renderer"), htmlDir, true);
		}

		public string FindLastChangedSlideHtmlPath()
		{
			var files = Directory.GetFiles(htmlDir, "*.html");
			if (files.Length == 0)
				return null;
			return files
				.Select(fn => Tuple.Create(File.GetLastWriteTime(fn), Path.GetFileName(fn)))
				.Max().Item2;
		}

		public void Start()
		{
			try
			{
				listener.Start();
			}
			catch (HttpListenerException e)
			{
				Console.WriteLine($"HttpListener Start Error: {e.Message}");
				Console.WriteLine();
				Console.WriteLine(@"On 'access is denied' error do one of the following:");
				Console.WriteLine(@"1. Run this application with admin rights.");
				Console.WriteLine(@"2. OR run this command in command line ('Everyone' may be some specific user):");
				Console.WriteLine($"   netsh http add urlacl url=http://+:{port}/ user=Everyone");
			}
			StartListen();
		}

		public void MarkCourseAsChanged()
		{
			lock (locker)
				lastChangeTime = DateTime.Now;
		}

		private async void StartListen()
		{
			while (true)
			{
				try
				{
					var context = await listener.GetContextAsync();
					StartHandle(context);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}
			// ReSharper disable once FunctionNeverReturns
		}

		private void StartHandle(HttpListenerContext context)
		{
			Task.Run(async () =>
			{
				var ctx = context;
				try
				{
					await OnContextAsync(ctx);
				}
				catch (HttpListenerException e)
				{
					if (e.ErrorCode != 1229)
						throw;
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
				finally
				{
					ctx.Response.Close();
				}
			});
		}

		private async Task OnContextAsync(HttpListenerContext context)
		{
			var query = context.Request.QueryString["query"];
			var path = context.Request.Url.LocalPath;
			byte[] response;
			var requestTime = DateTime.Now;
			var reloaded = ReloadCourseIfChanged(requestTime);
			if (!new[] { ".js", ".css", ".png", ".jpg", ".woff" }.Any(ext => path.EndsWith(ext)))
				Console.WriteLine($"{requestTime.ToString("T")} {context.Request.HttpMethod} {context.Request.Url}");
			switch (query)
			{
				case "needRefresh":
					response = await ServeNeedRefresh(reloaded, requestTime);
					break;
				case "submit":
					response = ServeRunExercise(context, path);
					break;
				default:
					response = ServeStatic(context, path);
					break;
			}
			await context.Response.OutputStream.WriteAsync(response, 0, response.Length);
			context.Response.OutputStream.Close();
		}

		private async Task<byte[]> ServeNeedRefresh(bool reloaded, DateTime requestTime)
		{
			var sw = Stopwatch.StartNew();
			while (true)
			{
				if (reloaded || sw.Elapsed > TimeSpan.FromSeconds(20))
				{
					Console.WriteLine($@"needRefresh:{reloaded}, LastChanged:{lastChangeTime}");
					return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(reloaded));
				}
				await Task.Delay(1000);
				reloaded = ReloadCourseIfChanged(requestTime);
			}
		}

		private byte[] ServeRunExercise(HttpListenerContext context, string path)
		{
			var code = context.Request.InputStream.GetString();
			var exercise = ((ExerciseSlide)course.Slides[int.Parse(path.Substring(1, 3))]).Exercise;
			var runResult = GetRunResult(exercise, code);
			context.Response.Headers["Content-Type"] = "application/json; charset=utf-8";
			return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(runResult));
		}

		private static RunSolutionResult GetRunResult(ExerciseBlock exercise, string code)
		{
			var buildResult = exercise.BuildSolution(code);
			if (buildResult.HasErrors)
				return new RunSolutionResult { IsCompileError = true, CompilationError = buildResult.ErrorMessage, ExecutionServiceName = "uLearn" };
			if (buildResult.HasStyleIssues)
				return new RunSolutionResult { IsStyleViolation = true, CompilationError = buildResult.StyleMessage, ExecutionServiceName = "uLearn" };

			var pathToCompiler = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Microsoft.Net.Compilers.1.3.2");
			var result = SandboxRunner.Run(pathToCompiler, exercise.CreateSubmition(Utils.NewNormalizedGuid(), code));
			return new RunSolutionResult
			{
				IsRightAnswer = result.Verdict == Verdict.Ok && result.GetOutput().NormalizeEoln() == exercise.ExpectedOutput.NormalizeEoln(),
				ActualOutput = result.GetOutput().NormalizeEoln(),
				CompilationError = result.CompilationOutput,
				ExecutionServiceName = "this",
				IsCompileError = result.Verdict == Verdict.CompilationError,
				ExpectedOutput = exercise.ExpectedOutput.NormalizeEoln(),
				SubmissionId = 0,
			};
		}

		private byte[] ServeStatic(HttpListenerContext context, string path)
		{
			byte[] response;
			try
			{
				response = File.ReadAllBytes(htmlDir + "/" + path);
			}
			catch (IOException e)
			{
				context.Response.StatusCode = 404;
				context.Response.Headers["Content-Type"] = "text/plain; charset=utf-8";
				response = Encoding.UTF8.GetBytes(e.ToString());
			}
			return response;
		}

		Course ReloadCourse()
		{
			var loadedCourse = new CourseLoader().LoadCourse(new DirectoryInfo(courseDir));
			var renderer = new SlideRenderer(new DirectoryInfo(htmlDir), loadedCourse);
			foreach (var slide in loadedCourse.Slides)
				renderer.RenderSlideToFile(slide, htmlDir);
			foreach (var unit in loadedCourse.GetUnits().Where(u => loadedCourse.FindInstructorNote(u) != null))
				renderer.RenderInstructorNotesToFile(unit, htmlDir);
			return loadedCourse;
		}

		private bool ReloadCourseIfChanged(DateTime requestTime)
		{
			lock (locker)
			{
				// Именно так. Предотвращает частую перезагрузку. Все должно обновляться за счет needrefresh.
				var needReload = lastChangeTime > requestTime.Add(TimeSpan.FromMilliseconds(500));
				if (needReload || course == null)
				{
					course = ReloadCourse();
					Console.WriteLine($"Course reloaded. LastChangeTime: {lastChangeTime}");
				}
				return needReload;
			}
		}
	}
}