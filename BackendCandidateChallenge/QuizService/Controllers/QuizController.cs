using Microsoft.AspNetCore.Mvc;
using QuizService.Model;
using QuizService.Model.Domain;
using System.Collections.Generic;
using System.Data;

namespace QuizService.Controllers;

[Route("api/quizzes")]
public class QuizController : Controller
{
	private readonly Service.QuizService _service;

	public QuizController(IDbConnection connection)
	{
		_service = new Service.QuizService(connection);
	}

	#region Quizzes
	// GET api/quizzes
	[HttpGet]
	public IEnumerable<QuizResponseModel> Get()
	{
		return _service.GetList();
	}

	// GET api/quizzes/5
	[HttpGet("{id}")]
	public object Get(int id)
	{
		Quiz quiz = _service.Get(id);
		if (quiz == null)
			return NotFound();

		var listOfQuestions = _service.GetQuestionList(id);

		var listOfAnswers = _service.GetAnswerListForQuiz(id);

		return _service.MakeQuizObject(quiz, listOfQuestions, listOfAnswers);
	}

	// POST api/quizzes
	[HttpPost]
	public IActionResult Post([FromBody] QuizCreateModel value)
	{
		var id = _service.Add(value);

		return Created($"/api/quizzes/{id}", null);
	}

	// PUT api/quizzes/5
	[HttpPut("{id}")]
	public IActionResult Put(int id, [FromBody] QuizUpdateModel value)
	{
		int rowsUpdated = _service.Update(id, value);
		if (rowsUpdated == 0)
			return NotFound();

		return NoContent();
	}

	// PUT api/quizzes/5/evaluate
	[HttpPut("{id}/evaluate")]
	public string Evaluate(int id, [FromBody] Dictionary<int, int> questionAnswerList)
	{
		var quiz = _service.Get(id);
		if (quiz == null)
			return null; //Znak frontendu da nije pronadjen test te nije ocjenjen

		string grade = _service.Evaluate(id, questionAnswerList);

		return grade;
	}

	// DELETE api/quizzes/5
	[HttpDelete("{id}")]
	public IActionResult Delete(int id)
	{
		int rowsDeleted = _service.Delete(id);
		if (rowsDeleted == 0)
			return NotFound();

		return NoContent();
	}
	#endregion

	#region Questions
	// POST api/quizzes/5/questions
	[HttpPost]
	[Route("{id}/questions")]
	public IActionResult PostQuestion(int id, [FromBody] QuestionCreateModel value)
	{
		Quiz quiz = _service.Get(id);
		if (quiz == null)
			return NotFound();

		var questionId = _service.AddQuestion(id, value);

		return Created($"/api/quizzes/{id}/questions/{questionId}", null);
	}

	// PUT api/quizzes/5/questions/6
	[HttpPut("{id}/questions/{qid}")]
	public IActionResult PutQuestion(int id, int qid, [FromBody] QuestionUpdateModel value)
	{
		//Question nećemo provjeravati da li postoji budući da se neće izvršiti upit ako ga ne nađe u bazi
		Answer answer = _service.GetAnswer(value.CorrectAnswerId);
		if (answer == null)
			NotFound();

		int rowsUpdated = _service.UpdateQuestion(qid, value);
		if (rowsUpdated == 0)
			return NotFound();

		return NoContent();
	}

	// DELETE api/quizzes/5/questions/6
	[HttpDelete]
	[Route("{id}/questions/{qid}")]
	public IActionResult DeleteQuestion(int id, int qid)
	{
		//Question nećemo provjeravati da li postoji budući da se neće izvršiti upit ako ga ne nađe u bazi
		int rowsDeleted = _service.DeleteQuestion(qid);
		if (rowsDeleted == 0)
			return NotFound();

		return NoContent();
	}
	#endregion

	#region Answers
	// POST api/quizzes/5/questions/6/answers
	[HttpPost]
	[Route("{id}/questions/{qid}/answers")]
	public IActionResult PostAnswer(int id, int qid, [FromBody] AnswerCreateModel value)
	{
		Question question = _service.GetQuestion(qid);
		if (question == null)
			return NotFound();

		var answerId = _service.AddAnswer(qid, value);

		return Created($"/api/quizzes/{id}/questions/{qid}/answers/{answerId}", null);
	}

	// PUT api/quizzes/5/questions/6/answers/7
	[HttpPut("{id}/questions/{qid}/answers/{aid}")]
	public IActionResult PutAnswer(int id, int qid, int aid, [FromBody] AnswerUpdateModel value)
	{
		//Answer nećemo provjeravati da li postoji budući da se neće izvršiti upit ako ga ne nađe u bazi
		int rowsUpdated = _service.UpdateAnswer(qid, value);
		if (rowsUpdated == 0)
			return NotFound();

		return NoContent();
	}

	// DELETE api/quizzes/5/questions/6/answers/7
	[HttpDelete]
	[Route("{id}/questions/{qid}/answers/{aid}")]
	public IActionResult DeleteAnswer(int id, int qid, int aid)
	{
		//Answer nećemo provjeravati da li postoji budući da se neće izvršiti upit ako ga ne nađe u bazi
		int rowsDeleted = _service.DeleteAnswer(aid);
		if (rowsDeleted == 0)
			return NotFound();

		return NoContent();
	}
	#endregion
}