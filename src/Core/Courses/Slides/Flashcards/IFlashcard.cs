using System;

namespace Ulearn.Core.Courses.Slides.Flashcards
{
	public interface IFlashcard
	{
		string Id { get; set; }
		Guid[] TheorySlidesIds { get; set; }
		FlashcardContent Question { get; set; }
		FlashcardContent Answer { get; set; }

		void BuildUp(SlideLoadingContext context, Slide flashcardSlide);

		void Validate(SlideLoadingContext context, Slide flashcardSlide);
	}
}