﻿using System;
using Google.Apis.Sheets.v4;
using NUnit.Framework;
using Ulearn.Core.Configuration;
using Ulearn.Core.GoogleSheet;

namespace Ulearn.Core.Tests
{
	[TestFixture]
	public class GoogleSheetTests
	{
		private const string spreadsheetId = "1dlMITgyLknv-cRd0SleTTGetiNsSfdXAprjKGHU-FNI";
		private const bool writeTokenToConsole = false;
		private readonly string accessToken = ApplicationConfiguration.Read<UlearnConfiguration>().GoogleAccessToken;

		[Test, Explicit]
		public void FillingTest()
		{
			var client = new GoogleApiClient(writeTokenToConsole, accessToken);
			var sheet = new GoogleSheet.GoogleSheet(2, 2,  0);
			var date = DateTime.UtcNow;
			sheet.AddCell(0, 0, date);
			sheet.AddCell(0,1, 1);
			sheet.AddCell(1,0,"2");
			sheet.AddCell(1,1, date);
			client.FillSpreadSheet(spreadsheetId, sheet);
		}
	}
}