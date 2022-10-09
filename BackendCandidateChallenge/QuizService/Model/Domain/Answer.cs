namespace QuizService.Model.Domain;

public class Answer
{
	public int Id { get; set; }
	public int QuestionId { get; set; } //TODO Ja bih od ova dva propertija kreirao primarni kljuc, tako da svaka numeracija odgovora ide unutar jednog pitanja
	public string Text { get; set; }
}