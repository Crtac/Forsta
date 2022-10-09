using Dapper;
using Microsoft.AspNetCore.Mvc;
using QuizService.Model;
using QuizService.Model.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace QuizService.Service
{
	public class QuizService
	{
		private readonly IDbConnection _connection;

		public QuizService(IDbConnection connection) //Dependency injection
		{
			_connection = connection;
		}

		#region Quizzes
		public List<QuizResponseModel> GetList()
		{
			const string sql = "SELECT * FROM Quiz;";
			var quizzes = _connection.Query<Quiz>(sql);

			return quizzes.Select(quiz =>
										new QuizResponseModel
										{
											Id = quiz.Id,
											Title = quiz.Title
										}).ToList();
		}

		public Quiz Get(int id)
		{
			const string sql = "SELECT * FROM Quiz WHERE Id = @QuizId;";
			var quiz = _connection.Query<Quiz>(sql, new { QuizId = id });

			return quiz.FirstOrDefault();
		}

		public object MakeQuizObject(Quiz quiz, List<Question> questions, Dictionary<int, IList<Answer>> answers)
		{
			var model = new QuizResponseModel
			{
				Id = quiz.Id,
				Title = quiz.Title,
				Questions = questions.Select(question => new QuizResponseModel.QuestionItem //TODO ne bih koristio dugi naziv, ostao bih kod q recimo za lamda expresion
				{
					Id = question.Id,
					Text = question.Text,
					Answers = answers.ContainsKey(question.Id)
								? answers[question.Id].Select(answer => new QuizResponseModel.AnswerItem
								{
									Id = answer.Id,
									Text = answer.Text
								})
								: new QuizResponseModel.AnswerItem[0],
					CorrectAnswerId = question.CorrectAnswerId
				}),
				Links = new Dictionary<string, string>
						{
								{"self", $"/api/quizzes/{quiz.Id}"},
								{"questions", $"/api/quizzes/{quiz.Id}/questions"}
						}
			};

			return model;
		}

		public int Add(QuizCreateModel model)
		{
			var sql = $"INSERT INTO Quiz (Title) VALUES('{model.Title}'); SELECT LAST_INSERT_ROWID();";
			var id = _connection.ExecuteScalar(sql);

			return Convert.ToInt32(id);
		}

		public int Update(int id, QuizUpdateModel model)
		{
			const string sql = "UPDATE Quiz SET Title = @Title WHERE Id = @Id";
			int rowsUpdated = _connection.Execute(sql, new { Id = id, Title = model.Title });

			return rowsUpdated;
		}

		public string Evaluate(int id, Dictionary<int, int> questionAnswerList)
		{
			//Necemo provjeravati da li postoje duplikati buduci da u dictionary ne moze da dodje key duplikat
			var questions = GetQuestionList(id);

			int maxPoints = questions.Count();

			var points = questions.Join(questionAnswerList,
								x => new { x.Id, Answer = x.CorrectAnswerId },
								y => new { Id = y.Key, Answer = y.Value },
								(x, y) => x.Id)
								.Count();

			return $"{points} / {maxPoints}";
		}

		public int Delete(int id)
		{
			const string sql = "DELETE FROM Quiz WHERE Id = @Id";
			int rowsDeleted = _connection.Execute(sql, new { Id = id });

			return rowsDeleted;
		}
		#endregion

		#region Questions
		public List<Question> GetQuestionList(int? id)
		{
			const string sql = "SELECT * FROM Question WHERE @QuizId IS NULL OR QuizId = @QuizId;";
			var questions = _connection.Query<Question>(sql, new { QuizId = id });

			return questions.ToList();
		}

		public Question GetQuestion(int qid)
		{
			const string sql = "SELECT * FROM Question WHERE Id = @QuestionId;";
			var questions = _connection.Query<Question>(sql, new { QuestionId = qid });

			return questions.FirstOrDefault();
		}

		public int AddQuestion(int id, QuestionCreateModel model)
		{
			const string sql = "INSERT INTO Question (Text, QuizId) VALUES(@Text, @QuizId); SELECT LAST_INSERT_ROWID();";
			var questionId = _connection.ExecuteScalar(sql, new { Text = model.Text, QuizId = id });

			return Convert.ToInt32(questionId);
		}

		public int UpdateQuestion(int qid, QuestionUpdateModel model)
		{
			const string sql = "UPDATE Question SET Text = @Text, CorrectAnswerId = @CorrectAnswerId WHERE Id = @QuestionId";
			int rowsUpdated = _connection.Execute(sql, new { QuestionId = qid, Text = model.Text, CorrectAnswerId = model.CorrectAnswerId });

			return rowsUpdated;
		}

		public int DeleteQuestion(int qid)
		{
			const string sql = "DELETE FROM Question WHERE Id = @QuestionId";
			int rowsDeleted = _connection.Execute(sql, new { QuestionId = qid });

			return rowsDeleted;
		}
		#endregion

		#region Answer
		public List<Answer> GetAnswerList(int qid)
		{
			const string sql = "SELECT * FROM Answer WHERE Id = @QuestionId;";
			var answers = _connection.Query<Answer>(sql, new { QuestionId = qid });

			return answers.ToList();
		}

		public Dictionary<int, IList<Answer>> GetAnswerListForQuiz(int id)
		{
			const string answersSql = "SELECT a.Id, a.Text, a.QuestionId FROM Answer a INNER JOIN Question q ON a.QuestionId = q.Id WHERE q.QuizId = @QuizId;";
			var answers = _connection.Query<Answer>(answersSql, new { QuizId = id })
					.Aggregate(new Dictionary<int, IList<Answer>>(), (dict, answer) =>
					{
						if (!dict.ContainsKey(answer.QuestionId))
							dict.Add(answer.QuestionId, new List<Answer>());
						dict[answer.QuestionId].Add(answer);
						return dict;
					});

			return answers;
		}

		public Answer GetAnswer(int aid)
		{
			const string sql = "SELECT * FROM Question WHERE Id = @AnswerId;";
			var answer = _connection.Query<Answer>(sql, new { AnswerId = aid });

			return answer.FirstOrDefault();
		}

		public int AddAnswer(int qid, AnswerCreateModel model)
		{
			const string sql = "INSERT INTO Answer (Text, QuestionId) VALUES(@Text, @QuestionId); SELECT LAST_INSERT_ROWID();";
			var answerId = _connection.ExecuteScalar(sql, new { Text = model.Text, QuestionId = qid });

			return Convert.ToInt32(answerId);
		}

		public int UpdateAnswer(int qid, AnswerUpdateModel model)
		{
			const string sql = "UPDATE Answer SET Text = @Text WHERE Id = @AnswerId";
			int rowsUpdated = _connection.Execute(sql, new { AnswerId = qid, Text = model.Text });

			return rowsUpdated;
		}

		public int DeleteAnswer(int aid)
		{
			const string sql = "DELETE FROM Answer WHERE Id = @AnswerId";
			int rowsDeleted = _connection.Execute(sql, new { AnswerId = aid });

			return rowsDeleted;
		}
		#endregion
	}
}
