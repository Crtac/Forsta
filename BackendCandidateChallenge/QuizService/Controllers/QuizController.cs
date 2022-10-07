using System.Collections.Generic;
using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using QuizService.Model;
using QuizService.Model.Domain;
using System.Linq;
using QuizService.Service;

namespace QuizService.Controllers;

[Route("api/quizzes")]
public class QuizController : Controller
{
	private readonly IDbConnection _connection;
	private readonly Service.QuizService _service;

	public QuizController(IDbConnection connection)
	{
		_connection = connection;
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
		return _service.Get(id);
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
		var questionId = _service.AddQuestion(id, value);
		return Created($"/api/quizzes/{id}/questions/{questionId}", null);
	}

	// PUT api/quizzes/5/questions/6
	[HttpPut("{id}/questions/{qid}")]
	public IActionResult PutQuestion(int id, int qid, [FromBody] QuestionUpdateModel value)
	{
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
		var answerId = _service.AddAnswer(qid, value);
		return Created($"/api/quizzes/{id}/questions/{qid}/answers/{answerId}", null);
	}

	// PUT api/quizzes/5/questions/6/answers/7
	[HttpPut("{id}/questions/{qid}/answers/{aid}")]
	public IActionResult PutAnswer(int id, int qid, int aid, [FromBody] AnswerUpdateModel value)
	{
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
		int rowsDeleted = _service.DeleteAnswer(aid);
		if (rowsDeleted == 0)
			return NotFound();
		return NoContent();
	}
	#endregion
}