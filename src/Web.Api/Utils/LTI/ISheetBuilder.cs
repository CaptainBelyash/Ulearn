﻿using System;
using OfficeOpenXml.Style;

namespace Ulearn.Web.Api.Utils.LTI
{
	public interface ISheetBuilder
	{
		void AddCell(string value, int colspan = 1);
		void AddCell(int value, int colspan = 1);
		void GoToNewLine();
		void AddStyleRule(Action<ExcelStyle> styleFunction);
		void PopStyleRule();
		void AddStyleRuleForOneCell(Action<ExcelStyle> styleFunction);
	}
}