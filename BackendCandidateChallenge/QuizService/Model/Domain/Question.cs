namespace QuizService.Model.Domain;

public class Question
{
	public int Id { get; set; }
	public int QuizId { get; set; }//TODO takodje kao i za odgovor, ovdje bih kreirao ovo dvoje da bude primarni kljuc, ali onda bi se i model za odgovor morao proširiti sa id-jem kviza
	public string Text { get; set; }
	public int CorrectAnswerId { get; set; }
}