using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using QuizService.Model;
using Xunit;

namespace QuizService.Tests;

public class QuizzesControllerTest
{
	const string QuizApiEndPoint = "/api/quizzes/";

	[Fact]
	public async Task PostNewQuizAddsQuiz()
	{
		var quiz = new QuizCreateModel("Test title");
		using (var testHost = new TestServer(new WebHostBuilder()
							 .UseStartup<Startup>()))
		{
			var client = testHost.CreateClient();
			var content = new StringContent(JsonConvert.SerializeObject(quiz));
			content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
			var response = await client.PostAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}"),
					content);
			Assert.Equal(HttpStatusCode.Created, response.StatusCode);
			Assert.NotNull(response.Headers.Location);
		}
	}

	[Fact]
	public async Task AQuizExistGetReturnsQuiz()
	{
		using (var testHost = new TestServer(new WebHostBuilder()
							 .UseStartup<Startup>()))
		{
			var client = testHost.CreateClient();
			const long quizId = 1;
			var response = await client.GetAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}{quizId}"));
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.NotNull(response.Content);
			var quiz = JsonConvert.DeserializeObject<QuizResponseModel>(await response.Content.ReadAsStringAsync());
			Assert.Equal(quizId, quiz.Id);
			Assert.Equal("My first quiz", quiz.Title);
		}
	}

	[Fact]
	public async Task AQuizDoesNotExistGetFails()
	{
		using (var testHost = new TestServer(new WebHostBuilder()
							 .UseStartup<Startup>()))
		{
			var client = testHost.CreateClient();
			const long quizId = 999;
			var response = await client.GetAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}{quizId}"));
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
		}
	}

	[Fact]
	public async Task AQuizDoesNotExists_WhenPostingAQuestion_ReturnsNotFound()
	{
		const string QuizApiEndPoint = "/api/quizzes/999/questions";

		using (var testHost = new TestServer(new WebHostBuilder()
							 .UseStartup<Startup>()))
		{
			var client = testHost.CreateClient();
			const long quizId = 999;
			var question = new QuestionCreateModel("The answer to everything is what?");
			var content = new StringContent(JsonConvert.SerializeObject(question));
			content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
			var response = await client.PostAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}"), content);
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
		}
	}

	[Fact]
	public async Task MakeNewQuizDoTestAndEvaluateCorrectAnswers()
	{
		StringContent content; //TODO ne kreirati objekte u for petlji vec samo dodjeljivati vrijednosti
		HttpResponseMessage response;

		Dictionary<int, int> questionAnswerList = new Dictionary<int, int>();
		var newQuiz = new QuizCreateModel("New quiz");
		QuestionCreateModel newQuestion;
		QuestionUpdateModel updateQuestion;

		AnswerCreateModel newAnswer;

		int? questionId = -1;
		int? answerId = -1;
		using (var testHost = new TestServer(new WebHostBuilder()
							 .UseStartup<Startup>()))
		{
			var client = testHost.CreateClient();

			content = new StringContent(JsonConvert.SerializeObject(newQuiz));
			content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			response = await client.PostAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}"), content);
			var quizId = Convert.ToInt32(response.Headers.Location.OriginalString.Replace("/api/quizzes/", ""));

			for (int i = 0; i < 3; i++)
			{
				newQuestion = new QuestionCreateModel($"Question number {i + 1}");

				content = new StringContent(JsonConvert.SerializeObject(newQuestion));
				content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

				response = await client.PostAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}{quizId}/questions"), content);
				questionId = Convert.ToInt32(response.Headers.Location.OriginalString.Replace($"/api/quizzes/{quizId}/questions/", ""));

				for (int j = 0; j < 2; j++)
				{
					newAnswer = new AnswerCreateModel($"Answer {i + 1}");

					content = new StringContent(JsonConvert.SerializeObject(newAnswer));
					content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

					response = await client.PostAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}{quizId}/questions/{questionId}/answers"), content);
					answerId = Convert.ToInt32(response.Headers.Location.OriginalString.Replace($"/api/quizzes/{quizId}/questions/{questionId}/answers/", ""));
				}

				updateQuestion = new QuestionUpdateModel { Text = newQuestion.Text, CorrectAnswerId = answerId.Value };
				content = new StringContent(JsonConvert.SerializeObject(updateQuestion));
				content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

				response = await client.PutAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}{quizId}/questions/{questionId}"), content);
				if (i == 2)
					answerId -= 1;

				questionAnswerList.Add(questionId.Value, answerId.Value);
			}

			content = new StringContent(JsonConvert.SerializeObject(questionAnswerList));
			content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			response = await client.PutAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}{quizId}/evaluate"), content);
			
			var encoding = ASCIIEncoding.ASCII;

			string result;
			using (var responseStream = response.Content.ReadAsStream())
			using (var reader = new StreamReader(responseStream, encoding))
				result = reader.ReadToEnd();

			Assert.Equal("2 / 3", result);
		}
	}
}