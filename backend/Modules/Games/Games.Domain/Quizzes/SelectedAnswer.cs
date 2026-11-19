namespace Games.Domain.Quizzes;

// The answer option a tourist picked for one question, both by their position in the quiz.
public sealed record SelectedAnswer
{
    public int QuestionIndex { get; private init; }
    public int OptionIndex { get; private init; }

    private SelectedAnswer() { }

    public SelectedAnswer(int questionIndex, int optionIndex)
    {
        QuestionIndex = questionIndex;
        OptionIndex = optionIndex;
    }
}
