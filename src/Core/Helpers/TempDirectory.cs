﻿using System;
using System.IO;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core.Configuration;

namespace Ulearn.Core.Helpers
{
	public class TempDirectory: IDisposable
	{
		public readonly DirectoryInfo DirectoryInfo;
		public static readonly string TempDirectoryPath;

		static TempDirectory()
		{
			TempDirectoryPath = ApplicationConfiguration.Read<UlearnConfiguration>().TempDirectory ?? Path.GetTempPath();
			new DirectoryInfo(TempDirectoryPath).EnsureExists();
		}

		public TempDirectory(string directoryName)
		{
			var path = Path.Combine(TempDirectoryPath, directoryName);
			DirectoryInfo = new DirectoryInfo(path);
			DirectoryInfo.Create();
		}

		public void Dispose()
		{
			try
			{
				FuncUtils.TrySeveralTimes(() => DirectoryInfo.Delete(true), 3);
			}
			catch (Exception ex)
			{
				// ignore
			}
		}
	}
}