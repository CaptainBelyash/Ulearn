namespace Ulearn.Web.Api.Models.Parameters
{
	public interface IPaginationParameters
	{
		int Offset { get; set; }
		
		int Count { get; set; }
	}
}